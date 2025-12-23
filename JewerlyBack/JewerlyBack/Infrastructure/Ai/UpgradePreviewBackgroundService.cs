using System.Diagnostics;
using System.Text.Json;
using JewerlyBack.Application.Ai;
using JewerlyBack.Data;
using JewerlyBack.Dto.Upgrade;
using JewerlyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace JewerlyBack.Infrastructure.Ai;

/// <summary>
/// Background service for processing upgrade preview generation jobs.
/// Polls the database for pending UpgradePreviewJob entities and processes them
/// using the Ideogram AI service.
/// </summary>
public sealed class UpgradePreviewBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UpgradePreviewBackgroundService> _logger;

    // Worker settings
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _processingDelay = TimeSpan.FromSeconds(1);
    private readonly int _batchSize = 2;
    private readonly TimeSpan _jobTimeout = TimeSpan.FromMinutes(2);
    private readonly TimeSpan _stuckJobThreshold = TimeSpan.FromMinutes(3);

    // Tracking
    private int _totalJobsProcessed = 0;

    public UpgradePreviewBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<UpgradePreviewBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     🔧 UPGRADE PREVIEW BACKGROUND SERVICE STARTED            ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Started at: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC                   ║");
        Console.WriteLine($"║  Polling interval: {_pollingInterval.TotalSeconds}s                                     ║");
        Console.WriteLine($"║  Batch size: {_batchSize}                                               ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        _logger.LogInformation(
            "🔧 UpgradePreviewBackgroundService STARTED. PollingInterval={PollingInterval}s, BatchSize={BatchSize}",
            _pollingInterval.TotalSeconds, _batchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Recover stuck jobs first
                await RecoverStuckJobsAsync(stoppingToken);

                // Process pending jobs
                await ProcessPendingJobsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [Upgrade Preview Worker ERROR] {ex.Message}");
                _logger.LogError(ex, "Error in UpgradePreviewBackgroundService main loop");
            }

            try
            {
                await Task.Delay(_pollingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     🛑 UPGRADE PREVIEW BACKGROUND SERVICE STOPPED            ║");
        Console.WriteLine($"║  Total jobs processed: {_totalJobsProcessed,-35} ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        _logger.LogInformation(
            "🛑 UpgradePreviewBackgroundService STOPPED. TotalJobsProcessed={TotalJobsProcessed}",
            _totalJobsProcessed);
    }

    private async Task RecoverStuckJobsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var stuckThreshold = DateTimeOffset.UtcNow - _stuckJobThreshold;

        var stuckJobs = await db.UpgradePreviewJobs
            .Where(j => j.Status == AiPreviewStatus.Processing && j.UpdatedAtUtc < stuckThreshold)
            .ToListAsync(stoppingToken);

        if (!stuckJobs.Any())
        {
            return;
        }

        Console.WriteLine($"⚠️ [Upgrade Preview] Found {stuckJobs.Count} stuck job(s)");

        foreach (var job in stuckJobs)
        {
            job.Status = AiPreviewStatus.Failed;
            job.ErrorMessage = "Job timed out - processing took too long. Please try again.";
            job.UpdatedAtUtc = DateTimeOffset.UtcNow;

            _logger.LogWarning(
                "Recovering stuck upgrade preview job {JobId}",
                job.Id);
        }

        await db.SaveChangesAsync(stoppingToken);
    }

    private async Task ProcessPendingJobsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var aiProvider = scope.ServiceProvider.GetRequiredService<IAiImageProvider>();

        var pendingJobs = await db.UpgradePreviewJobs
            .Include(j => j.Analysis)
            .Where(j => j.Status == AiPreviewStatus.Pending)
            .OrderBy(j => j.CreatedAtUtc)
            .Take(_batchSize)
            .ToListAsync(stoppingToken);

        if (!pendingJobs.Any())
        {
            return;
        }

        Console.WriteLine($"📋 [Upgrade Preview] Found {pendingJobs.Count} pending job(s)");
        _logger.LogInformation("Found {Count} pending upgrade preview jobs", pendingJobs.Count);

        foreach (var job in pendingJobs)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            await ProcessSingleJobAsync(job, db, aiProvider, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_processingDelay, stoppingToken);
            }
        }
    }

    private async Task ProcessSingleJobAsync(
        UpgradePreviewJob job,
        AppDbContext db,
        IAiImageProvider aiProvider,
        CancellationToken stoppingToken)
    {
        var jobStopwatch = Stopwatch.StartNew();

        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine($"🔧 [UPGRADE PREVIEW JOB] Processing Job {job.Id}");
        Console.WriteLine($"   Analysis ID: {job.AnalysisId}");
        Console.WriteLine($"   Keep Original: {job.KeptOriginal}");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");

        _logger.LogInformation(
            "Processing upgrade preview job {JobId} for analysis {AnalysisId}",
            job.Id, job.AnalysisId);

        try
        {
            // Update status to Processing
            job.Status = AiPreviewStatus.Processing;
            job.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(stoppingToken);

            // Get analysis data
            var analysis = job.Analysis;
            if (analysis == null)
            {
                throw new InvalidOperationException("Analysis not found for preview job");
            }

            // Build the prompt for Ideogram
            var prompt = BuildUpgradePreviewPrompt(job, analysis);
            Console.WriteLine($"   Prompt length: {prompt.Length} characters");

            _logger.LogInformation("UPGRADE PREVIEW PROMPT\n{Prompt}", prompt);

            // Generate image with timeout
            using var timeoutCts = new CancellationTokenSource(_jobTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutCts.Token);

            Console.WriteLine("   🌐 Calling AI service...");

            try
            {
                var imageUrl = await aiProvider.GenerateSinglePreviewAsync(
                    prompt,
                    job.AnalysisId, // Use analysis ID as configuration reference
                    job.Id,
                    linkedCts.Token);

                jobStopwatch.Stop();
                Console.WriteLine($"   ✓ Image generated in {jobStopwatch.Elapsed.TotalSeconds:F2}s");
                Console.WriteLine($"   Image URL: {imageUrl}");

                // Update job as completed
                job.Status = AiPreviewStatus.Completed;
                job.EnhancedImageUrl = imageUrl;
                job.Prompt = prompt;
                job.UpdatedAtUtc = DateTimeOffset.UtcNow;

                _logger.LogInformation(
                    "✅ Completed upgrade preview job {JobId}. Duration: {Duration}s",
                    job.Id, jobStopwatch.Elapsed.TotalSeconds);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"AI generation timed out after {_jobTimeout.TotalMinutes} minutes");
            }

            await db.SaveChangesAsync(stoppingToken);
            _totalJobsProcessed++;

            Console.WriteLine($"✅ [UPGRADE PREVIEW JOB COMPLETED] Job {job.Id}");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            jobStopwatch.Stop();

            Console.WriteLine($"❌ [UPGRADE PREVIEW JOB FAILED] Job {job.Id}");
            Console.WriteLine($"   Error: {ex.Message}");

            _logger.LogError(ex,
                "Failed to process upgrade preview job {JobId}",
                job.Id);

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
                    "Failed to save error state for upgrade preview job {JobId}",
                    job.Id);
            }
        }
    }

    /// <summary>
    /// Builds the Ideogram prompt for upgrade preview generation.
    /// </summary>
    private string BuildUpgradePreviewPrompt(
        UpgradePreviewJob job,
        UpgradeAnalysis analysis)
    {
        var promptParts = new List<string>();

        // Base description from analysis
        promptParts.Add("Professional product photography of a jewelry piece on a pure white background.");

        // Jewelry type
        var jewelryType = analysis.JewelryType ?? "jewelry piece";
        promptParts.Add($"Subject: {GetJewelryTypeDescription(jewelryType)}.");

        // Metal description
        if (!string.IsNullOrEmpty(analysis.DetectedMetalDescription))
        {
            promptParts.Add($"Metal: {analysis.DetectedMetalDescription}.");
        }
        else if (!string.IsNullOrEmpty(analysis.DetectedMetal))
        {
            promptParts.Add($"Metal: {GetMetalDescription(analysis.DetectedMetal)}.");
        }

        // Stones if present
        if (analysis.HasStones && !string.IsNullOrEmpty(analysis.DetectedStonesJson))
        {
            try
            {
                var stones = JsonSerializer.Deserialize<List<DetectedStoneDto>>(
                    analysis.DetectedStonesJson,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                if (stones?.Any() == true)
                {
                    var stoneDesc = string.Join(", ", stones.Select(s => s.Description ?? s.StoneType));
                    promptParts.Add($"Stones: {stoneDesc}.");
                }
            }
            catch (JsonException)
            {
                // Ignore parsing errors
            }
        }

        // Style
        if (!string.IsNullOrEmpty(analysis.StyleClassification))
        {
            promptParts.Add($"Style: {GetStyleDescription(analysis.StyleClassification)}.");
        }

        // Add selected suggestions if not keeping original
        if (!job.KeptOriginal && !string.IsNullOrEmpty(job.AppliedSuggestionsJson))
        {
            try
            {
                var suggestionIds = JsonSerializer.Deserialize<List<Guid>>(
                    job.AppliedSuggestionsJson,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                if (suggestionIds?.Any() == true && !string.IsNullOrEmpty(analysis.SuggestionsJson))
                {
                    var allSuggestions = JsonSerializer.Deserialize<List<UpgradeSuggestionDto>>(
                        analysis.SuggestionsJson,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                    if (allSuggestions != null)
                    {
                        var selectedSuggestions = allSuggestions
                            .Where(s => suggestionIds.Contains(s.Id))
                            .ToList();

                        if (selectedSuggestions.Any())
                        {
                            promptParts.Add("Enhancements applied:");
                            foreach (var suggestion in selectedSuggestions)
                            {
                                var enhancement = !string.IsNullOrEmpty(suggestion.Description)
                                    ? suggestion.Description
                                    : suggestion.Rationale;
                                promptParts.Add($"- {suggestion.Title}: {enhancement}");
                            }
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse suggestions for preview prompt");
            }
        }

        // Photography requirements
        promptParts.Add("Clean, centered composition. Professional studio lighting. Sharp focus. No shadows on background.");

        return string.Join(" ", promptParts);
    }

    private static string GetJewelryTypeDescription(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "ring" => "an elegant ring",
            "earrings" => "a pair of elegant earrings",
            "pendant" => "an elegant pendant",
            "necklace" => "an elegant necklace",
            "bracelet" => "an elegant bracelet",
            "brooch" => "an elegant brooch",
            _ => "an elegant jewelry piece"
        };
    }

    private static string GetMetalDescription(string metal)
    {
        return metal.ToLowerInvariant() switch
        {
            "yellow_gold" => "warm yellow gold",
            "white_gold" => "bright white gold",
            "rose_gold" => "romantic rose gold",
            "platinum" => "lustrous platinum",
            "silver" => "polished silver",
            _ => metal.Replace("_", " ")
        };
    }

    private static string GetStyleDescription(string style)
    {
        return style.ToLowerInvariant() switch
        {
            "classic" => "classic and timeless design",
            "modern" => "modern contemporary design",
            "vintage" => "vintage-inspired design",
            "minimalist" => "minimalist clean design",
            "art_deco" => "art deco geometric design",
            "bold" => "bold statement design",
            _ => style.Replace("_", " ") + " design"
        };
    }
}
