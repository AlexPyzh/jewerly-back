using System.Diagnostics;
using System.Text.Json;
using JewerlyBack.Application.Ai;
using JewerlyBack.Application.Ai.Models;
using JewerlyBack.Data;
using JewerlyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace JewerlyBack.Infrastructure.Ai;

/// <summary>
/// Фоновый воркер для обработки заданий на генерацию AI-превью.
/// Периодически проверяет базу данных на наличие заданий со статусом Pending
/// и обрабатывает их, вызывая OpenAI API для генерации изображений.
/// </summary>
public sealed class AiPreviewBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AiPreviewBackgroundService> _logger;

    // Настройки воркера
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10); // Период опроса БД
    private readonly TimeSpan _processingDelay = TimeSpan.FromSeconds(2);  // Задержка между обработкой job'ов
    private readonly int _batchSize = 3; // Количество job'ов, обрабатываемых за один цикл
    private readonly TimeSpan _jobTimeout = TimeSpan.FromMinutes(2); // Timeout for AI generation (reduced from 3min for faster failure)
    private readonly TimeSpan _stuckJobThreshold = TimeSpan.FromMinutes(3); // Threshold for stuck job detection (reduced from 5min)
    private readonly TimeSpan _heartbeatInterval = TimeSpan.FromSeconds(30); // Heartbeat logging interval

    // Tracking
    private int _totalJobsProcessed = 0;
    private DateTimeOffset _lastHeartbeat = DateTimeOffset.MinValue;

    public AiPreviewBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<AiPreviewBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // ===== STARTUP LOG =====
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     🚀 AI PREVIEW BACKGROUND SERVICE STARTED                 ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Started at: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC                   ║");
        Console.WriteLine($"║  Polling interval: {_pollingInterval.TotalSeconds}s                                    ║");
        Console.WriteLine($"║  Batch size: {_batchSize}                                               ║");
        Console.WriteLine($"║  Job timeout: {_jobTimeout.TotalMinutes} minutes                                    ║");
        Console.WriteLine($"║  Stuck job threshold: {_stuckJobThreshold.TotalMinutes} minutes                         ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        _logger.LogInformation(
            "🚀 AiPreviewBackgroundService STARTED. PollingInterval={PollingInterval}s, BatchSize={BatchSize}, JobTimeout={JobTimeout}min, StuckThreshold={StuckThreshold}min",
            _pollingInterval.TotalSeconds, _batchSize, _jobTimeout.TotalMinutes, _stuckJobThreshold.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Heartbeat logging
                if (DateTimeOffset.UtcNow - _lastHeartbeat >= _heartbeatInterval)
                {
                    _lastHeartbeat = DateTimeOffset.UtcNow;
                    Console.WriteLine($"💓 [AI Worker Heartbeat] {DateTimeOffset.UtcNow:HH:mm:ss} UTC | Jobs processed: {_totalJobsProcessed} | Status: Running");
                    _logger.LogDebug("💓 AI Worker heartbeat. TotalJobsProcessed={TotalJobsProcessed}", _totalJobsProcessed);
                }

                // First, recover any stuck jobs
                await RecoverStuckJobsAsync(stoppingToken);

                // Then process pending jobs
                await ProcessPendingJobsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Expected during shutdown
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AI Worker ERROR] {DateTimeOffset.UtcNow:HH:mm:ss} UTC | Main loop error: {ex.Message}");
                _logger.LogError(ex, "❌ Error in AiPreviewBackgroundService main loop");
            }

            // Ожидание перед следующим циклом
            try
            {
                await Task.Delay(_pollingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        // ===== SHUTDOWN LOG =====
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     🛑 AI PREVIEW BACKGROUND SERVICE STOPPED                 ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Stopped at: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC                   ║");
        Console.WriteLine($"║  Total jobs processed: {_totalJobsProcessed,-35} ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        _logger.LogInformation("🛑 AiPreviewBackgroundService STOPPED. TotalJobsProcessed={TotalJobsProcessed}", _totalJobsProcessed);
    }

    /// <summary>
    /// Detects and recovers jobs that have been stuck in Processing status for too long.
    /// </summary>
    private async Task RecoverStuckJobsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var stuckThreshold = DateTimeOffset.UtcNow - _stuckJobThreshold;

        var stuckJobs = await db.AiPreviewJobs
            .Where(j => j.Status == AiPreviewStatus.Processing && j.UpdatedAtUtc < stuckThreshold)
            .ToListAsync(stoppingToken);

        if (!stuckJobs.Any())
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"⚠️ [STUCK JOB RECOVERY] Found {stuckJobs.Count} stuck job(s) in Processing status for more than {_stuckJobThreshold.TotalMinutes} minutes");

        foreach (var job in stuckJobs)
        {
            var stuckDuration = DateTimeOffset.UtcNow - job.UpdatedAtUtc;

            Console.WriteLine($"  📌 Job {job.Id}: stuck for {stuckDuration.TotalMinutes:F1} minutes, marking as Failed");
            _logger.LogWarning(
                "⚠️ Recovering stuck job {JobId}. Was in Processing status for {StuckMinutes:F1} minutes. Marking as Failed.",
                job.Id, stuckDuration.TotalMinutes);

            try
            {
                job.Status = AiPreviewStatus.Failed;
                job.ErrorMessage = $"Job timed out - processing took too long (stuck for {stuckDuration.TotalMinutes:F1} minutes). The AI service may have been unresponsive. Please try again.";
                job.UpdatedAtUtc = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ Failed to update stuck job {job.Id}: {ex.Message}");
                _logger.LogError(ex, "Failed to update stuck job {JobId}", job.Id);
            }
        }

        try
        {
            await db.SaveChangesAsync(stoppingToken);
            Console.WriteLine($"✅ [STUCK JOB RECOVERY] Successfully recovered {stuckJobs.Count} stuck job(s)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [STUCK JOB RECOVERY] Failed to save recovered jobs: {ex.Message}");
            _logger.LogError(ex, "Failed to save recovered stuck jobs");
        }
    }

    /// <summary>
    /// Обрабатывает pending задания из базы данных.
    /// </summary>
    private async Task ProcessPendingJobsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var aiProvider = scope.ServiceProvider.GetRequiredService<IAiImageProvider>();
        var promptBuilder = scope.ServiceProvider.GetRequiredService<IAiPromptBuilder>();
        var aiConfigBuilder = scope.ServiceProvider.GetRequiredService<IAiConfigBuilder>();

        // Получаем pending задания
        var pendingJobs = await db.AiPreviewJobs
            .Where(j => j.Status == AiPreviewStatus.Pending)
            .OrderBy(j => j.CreatedAtUtc)
            .Take(_batchSize)
            .ToListAsync(stoppingToken);

        if (!pendingJobs.Any())
        {
            _logger.LogDebug("No pending AI preview jobs found");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"📋 [AI Worker] Found {pendingJobs.Count} pending job(s) to process");
        _logger.LogInformation("Found {Count} pending AI preview jobs", pendingJobs.Count);

        // Обрабатываем каждое задание
        foreach (var job in pendingJobs)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("⚠️ [AI Worker] Cancellation requested, stopping job processing");
                break;
            }

            await ProcessSingleJobAsync(job, db, aiProvider, promptBuilder, aiConfigBuilder, stoppingToken);

            // Небольшая задержка между job'ами
            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_processingDelay, stoppingToken);
            }
        }
    }

    /// <summary>
    /// Обрабатывает одно задание на генерацию превью.
    /// </summary>
    private async Task ProcessSingleJobAsync(
        AiPreviewJob job,
        AppDbContext db,
        IAiImageProvider aiProvider,
        IAiPromptBuilder promptBuilder,
        IAiConfigBuilder aiConfigBuilder,
        CancellationToken stoppingToken)
    {
        var jobStopwatch = Stopwatch.StartNew();

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine($"🎯 [JOB START] Processing AI Preview Job");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine($"  Job ID:          {job.Id}");
        Console.WriteLine($"  Type:            {job.Type}");
        Console.WriteLine($"  Configuration:   {job.ConfigurationId}");
        Console.WriteLine($"  User ID:         {job.UserId?.ToString() ?? "Guest"}");
        Console.WriteLine($"  Guest Client ID: {job.GuestClientId ?? "N/A"}");
        Console.WriteLine($"  Started at:      {DateTimeOffset.UtcNow:HH:mm:ss.fff} UTC");
        Console.WriteLine($"  Timeout:         {_jobTimeout.TotalMinutes} minutes");
        Console.WriteLine("───────────────────────────────────────────────────────────────");

        _logger.LogInformation(
            "🎯 Processing AI preview job {JobId} (Type={Type}, ConfigurationId={ConfigurationId}, UserId={UserId})",
            job.Id, job.Type, job.ConfigurationId, job.UserId?.ToString() ?? "Guest");

        try
        {
            // 1. Обновляем статус на Processing
            Console.WriteLine("📝 Step 1: Updating job status to Processing...");
            job.Status = AiPreviewStatus.Processing;
            job.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(stoppingToken);
            Console.WriteLine("   ✓ Status updated to Processing");

            // 2. Получаем или строим семантический AI config
            Console.WriteLine("📝 Step 2: Loading/building AI configuration...");
            AiConfigDto aiConfig;

            if (!string.IsNullOrWhiteSpace(job.AiConfigJson))
            {
                // Используем уже готовый AiConfigJson из job (был сохранен при создании)
                Console.WriteLine("   Using existing AiConfigJson from job");
                var deserializeOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                aiConfig = JsonSerializer.Deserialize<AiConfigDto>(job.AiConfigJson, deserializeOptions)
                    ?? throw new InvalidOperationException("Failed to deserialize AiConfigJson");

                // Log the config details
                Console.WriteLine();
                Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
                Console.WriteLine("│ 📦 AI CONFIGURATION                                         │");
                Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
                Console.WriteLine($"│ Category:     {aiConfig.CategoryName,-45} │");
                Console.WriteLine($"│ Base Model:   {aiConfig.BaseModelName,-45} │");
                Console.WriteLine($"│ Material:     {aiConfig.MaterialName,-45} │");
                if (aiConfig.Stones?.Any() == true)
                {
                    var stonesStr = string.Join(", ", aiConfig.Stones.Select(s => $"{s.StoneTypeName} x{s.Count}"));
                    Console.WriteLine($"│ Stones:       {stonesStr,-45} │");
                }
                if (!string.IsNullOrEmpty(aiConfig.BaseModelDescription))
                {
                    var desc = aiConfig.BaseModelDescription.Length > 45
                        ? aiConfig.BaseModelDescription[..42] + "..."
                        : aiConfig.BaseModelDescription;
                    Console.WriteLine($"│ Description:  {desc,-45} │");
                }
                Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
                Console.WriteLine();
            }
            else
            {
                // Для старых job'ов без AiConfigJson - строим заново
                Console.WriteLine("   ⚠️ AiConfigJson not found, building from configuration...");
                _logger.LogWarning(
                    "AiConfigJson not found for job {JobId}, building from configuration",
                    job.Id);

                aiConfig = await aiConfigBuilder.BuildForConfigurationAsync(
                    job.ConfigurationId,
                    job.UserId,
                    stoppingToken);

                // Сохраняем построенный config в job для будущих reference
                job.AiConfigJson = JsonSerializer.Serialize(aiConfig, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                Console.WriteLine("   ✓ Built new AiConfigJson");
            }

            // 3. Build structured JSON prompt based on semantic config
            Console.WriteLine("📝 Step 3: Building structured JSON prompt...");
            var promptStopwatch = Stopwatch.StartNew();
            var prompt = await promptBuilder.BuildStructuredPromptAsync(aiConfig, stoppingToken);
            promptStopwatch.Stop();

            Console.WriteLine($"   ✓ Structured prompt built in {promptStopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"   Prompt length: {prompt.Length} characters");
            Console.WriteLine($"   Prompt type: Structured JSON");
            Console.WriteLine();

            // Log the FULL structured prompt - this is the ONLY place where the prompt should be logged
            // Uses exact format "AI PREVIEW STRUCTURED PROMPT\n{prompt}" for easy searching/parsing
            _logger.LogInformation("AI PREVIEW STRUCTURED PROMPT\n{Prompt}", prompt);

            // 4. Генерируем изображение в зависимости от типа WITH TIMEOUT
            Console.WriteLine("📝 Step 4: Generating AI image...");
            Console.WriteLine($"   Timeout configured: {_jobTimeout.TotalMinutes} minutes");

            using var timeoutCts = new CancellationTokenSource(_jobTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutCts.Token);

            var generationStopwatch = Stopwatch.StartNew();

            try
            {
                if (job.Type == AiPreviewType.SingleImage)
                {
                    Console.WriteLine("   Type: SingleImage");
                    Console.WriteLine("   🌐 Calling AI service...");

                    var imageUrl = await aiProvider.GenerateSinglePreviewAsync(
                        prompt,
                        job.ConfigurationId,
                        job.Id,
                        linkedCts.Token);

                    generationStopwatch.Stop();
                    Console.WriteLine($"   ✓ Image generated in {generationStopwatch.Elapsed.TotalSeconds:F2}s");
                    Console.WriteLine($"   Image URL: {imageUrl}");

                    // 5. Обновляем задание как успешно выполненное
                    job.Status = AiPreviewStatus.Completed;
                    job.SingleImageUrl = imageUrl;
                    job.FramesJson = null;
                    job.Prompt = prompt;
                    job.UpdatedAtUtc = DateTimeOffset.UtcNow;

                    _logger.LogInformation(
                        "✅ Completed AI preview job {JobId} (Type={Type}). Image URL: {Url}. Duration: {Duration}s",
                        job.Id, job.Type, imageUrl, generationStopwatch.Elapsed.TotalSeconds);
                }
                else if (job.Type == AiPreviewType.Preview360)
                {
                    const int frameCount = 12;
                    Console.WriteLine($"   Type: Preview360 ({frameCount} frames)");
                    Console.WriteLine("   🌐 Calling AI service...");

                    var frameUrls = await aiProvider.Generate360PreviewAsync(
                        prompt,
                        job.ConfigurationId,
                        job.Id,
                        frameCount,
                        linkedCts.Token);

                    generationStopwatch.Stop();
                    Console.WriteLine($"   ✓ {frameUrls.Count} frames generated in {generationStopwatch.Elapsed.TotalSeconds:F2}s");

                    // 5. Обновляем задание как успешно выполненное
                    job.Status = AiPreviewStatus.Completed;
                    job.FramesJson = JsonSerializer.Serialize(frameUrls);
                    job.SingleImageUrl = frameUrls.FirstOrDefault();
                    job.Prompt = prompt;
                    job.UpdatedAtUtc = DateTimeOffset.UtcNow;

                    _logger.LogInformation(
                        "✅ Completed AI preview job {JobId} (Type={Type}). Generated {FrameCount} frames. Duration: {Duration}s",
                        job.Id, job.Type, frameUrls.Count, generationStopwatch.Elapsed.TotalSeconds);
                }
                else
                {
                    throw new NotImplementedException(
                        $"AI preview type {job.Type} is not supported");
                }
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                generationStopwatch.Stop();
                Console.WriteLine($"   ⏰ TIMEOUT! AI generation exceeded {_jobTimeout.TotalMinutes} minutes");
                throw new TimeoutException(
                    $"AI image generation timed out after {_jobTimeout.TotalMinutes} minutes. The AI service may be slow or unresponsive.");
            }

            // 6. Save the completed job
            Console.WriteLine("📝 Step 5: Saving completed job...");
            await db.SaveChangesAsync(stoppingToken);
            Console.WriteLine("   ✓ Job saved successfully");

            _totalJobsProcessed++;
            jobStopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine($"✅ [JOB COMPLETED] Job {job.Id}");
            Console.WriteLine($"   Total duration: {jobStopwatch.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"   Status: {job.Status}");
            Console.WriteLine($"   Image URL: {job.SingleImageUrl}");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            jobStopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine($"❌ [JOB FAILED] Job {job.Id}");
            Console.WriteLine($"   Duration: {jobStopwatch.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"   Error Type: {ex.GetType().Name}");
            Console.WriteLine($"   Error Message: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
            }
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine();

            _logger.LogError(ex,
                "❌ Failed to process AI preview job {JobId} (Type={Type}, ConfigurationId={ConfigurationId}). Duration: {Duration}s",
                job.Id, job.Type, job.ConfigurationId, jobStopwatch.Elapsed.TotalSeconds);

            // Обновляем задание как проваленное
            try
            {
                Console.WriteLine("📝 Updating job status to Failed...");
                job.Status = AiPreviewStatus.Failed;
                job.ErrorMessage = $"{ex.GetType().Name}: {ex.Message}";
                job.UpdatedAtUtc = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(stoppingToken);
                Console.WriteLine("   ✓ Job marked as Failed");

                _logger.LogInformation(
                    "Job {JobId} marked as Failed with error: {Error}",
                    job.Id, job.ErrorMessage);
            }
            catch (Exception saveEx)
            {
                Console.WriteLine($"   ❌ CRITICAL: Failed to save error state: {saveEx.Message}");
                _logger.LogError(saveEx,
                    "❌ CRITICAL: Failed to save error state for job {JobId}. Original error: {OriginalError}",
                    job.Id, ex.Message);
            }
        }
    }
}
