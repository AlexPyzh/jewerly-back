using JewerlyBack.Application.Ai;
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

    public AiPreviewBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<AiPreviewBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AiPreviewBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingJobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AiPreviewBackgroundService main loop");
            }

            // Ожидание перед следующим циклом
            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("AiPreviewBackgroundService stopped");
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

        _logger.LogInformation("Found {Count} pending AI preview jobs", pendingJobs.Count);

        // Обрабатываем каждое задание
        foreach (var job in pendingJobs)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await ProcessSingleJobAsync(job, db, aiProvider, promptBuilder, stoppingToken);

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
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Processing AI preview job {JobId} (Type={Type}, ConfigurationId={ConfigurationId})",
            job.Id, job.Type, job.ConfigurationId);

        try
        {
            // 1. Обновляем статус на Processing
            job.Status = AiPreviewStatus.Processing;
            job.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(stoppingToken);

            // 2. Загружаем конфигурацию со всеми связанными данными
            var configuration = await db.JewelryConfigurations
                .Include(c => c.BaseModel)
                    .ThenInclude(bm => bm.Category)
                .Include(c => c.Material)
                .Include(c => c.Stones)
                    .ThenInclude(s => s.StoneType)
                .FirstOrDefaultAsync(c => c.Id == job.ConfigurationId, stoppingToken);

            if (configuration == null)
            {
                throw new InvalidOperationException(
                    $"Configuration {job.ConfigurationId} not found");
            }

            // 3. Строим промпт
            var prompt = await promptBuilder.BuildPreviewPromptAsync(configuration, stoppingToken);

            _logger.LogInformation("Generated prompt for job {JobId}: {Prompt}", job.Id, prompt);

            // 4. Генерируем изображение в зависимости от типа
            if (job.Type == AiPreviewType.SingleImage)
            {
                var imageUrl = await aiProvider.GenerateSinglePreviewAsync(
                    prompt,
                    job.ConfigurationId,
                    job.Id,
                    stoppingToken);

                // 5. Обновляем задание как успешно выполненное
                job.Status = AiPreviewStatus.Completed;
                job.SingleImageUrl = imageUrl;
                job.FramesJson = null; // Для SingleImage не нужны кадры
                job.Prompt = prompt;
                job.UpdatedAtUtc = DateTimeOffset.UtcNow;

                _logger.LogInformation(
                    "Completed AI preview job {JobId} (Type={Type}). Image URL: {Url}",
                    job.Id, job.Type, imageUrl);
            }
            else if (job.Type == AiPreviewType.Preview360)
            {
                const int frameCount = 12; // Можно сделать конфигурируемым в будущем

                var frameUrls = await aiProvider.Generate360PreviewAsync(
                    prompt,
                    job.ConfigurationId,
                    job.Id,
                    frameCount,
                    stoppingToken);

                // 5. Обновляем задание как успешно выполненное
                job.Status = AiPreviewStatus.Completed;
                job.FramesJson = System.Text.Json.JsonSerializer.Serialize(frameUrls);
                // Первый кадр используется как превью
                job.SingleImageUrl = frameUrls.FirstOrDefault();
                job.Prompt = prompt;
                job.UpdatedAtUtc = DateTimeOffset.UtcNow;

                _logger.LogInformation(
                    "Completed AI preview job {JobId} (Type={Type}). Generated {FrameCount} frames",
                    job.Id, job.Type, frameUrls.Count);
            }
            else
            {
                throw new NotImplementedException(
                    $"AI preview type {job.Type} is not supported");
            }

            await db.SaveChangesAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process AI preview job {JobId} (Type={Type}, ConfigurationId={ConfigurationId})",
                job.Id, job.Type, job.ConfigurationId);

            // Обновляем задание как проваленное
            try
            {
                job.Status = AiPreviewStatus.Failed;
                job.ErrorMessage = ex.Message;
                job.UpdatedAtUtc = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception saveEx)
            {
                _logger.LogError(saveEx,
                    "Failed to save error state for job {JobId}",
                    job.Id);
            }
        }
    }
}
