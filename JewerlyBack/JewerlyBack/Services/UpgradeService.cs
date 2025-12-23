using System.Text.Json;
using JewerlyBack.Application.Ai;
using JewerlyBack.Application.Interfaces;
using JewerlyBack.Data;
using JewerlyBack.Dto.Upgrade;
using JewerlyBack.Infrastructure.Storage;
using JewerlyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace JewerlyBack.Services;

/// <summary>
/// Service for handling jewelry upgrade flow with OpenAI Vision integration
/// </summary>
public class UpgradeService : IUpgradeService
{
    private readonly AppDbContext _context;
    private readonly IS3StorageService _storageService;
    private readonly IJewelryVisionAnalyzer _visionAnalyzer;
    private readonly ILogger<UpgradeService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public UpgradeService(
        AppDbContext context,
        IS3StorageService storageService,
        IJewelryVisionAnalyzer visionAnalyzer,
        ILogger<UpgradeService> logger)
    {
        _context = context;
        _storageService = storageService;
        _visionAnalyzer = visionAnalyzer;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<UpgradeImageUploadResponseDto> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        Guid? userId,
        string? guestClientId,
        CancellationToken ct = default)
    {
        // Validate guest client ID for anonymous users
        if (!userId.HasValue && string.IsNullOrWhiteSpace(guestClientId))
        {
            throw new ArgumentException("GuestClientId is required for anonymous users");
        }

        _logger.LogInformation(
            "Starting image upload for upgrade analysis. User: {UserId}, Guest: {GuestClientId}",
            userId?.ToString() ?? "anonymous", guestClientId ?? "N/A");

        var now = DateTimeOffset.UtcNow;

        // Upload image to S3
        var imageUrl = await _storageService.UploadAsync(
            imageStream,
            $"upgrade-images/{now:yyyy/MM/dd}/{Guid.NewGuid()}{Path.GetExtension(fileName)}",
            contentType,
            ct);

        // Create analysis record
        var analysis = new UpgradeAnalysis
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GuestClientId = guestClientId,
            OriginalImageUrl = imageUrl,
            Status = UpgradeAnalysisStatus.Pending,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _context.UpgradeAnalyses.Add(analysis);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created upgrade analysis {AnalysisId} for image upload",
            analysis.Id);

        // Perform real AI analysis
        await PerformVisionAnalysisAsync(analysis.Id, imageUrl, ct);

        return new UpgradeImageUploadResponseDto
        {
            AnalysisId = analysis.Id,
            ImageUrl = imageUrl,
            Message = "Your image has been uploaded. Analysis in progress."
        };
    }

    public async Task<UpgradeAnalysisResultDto?> GetAnalysisAsync(
        Guid analysisId,
        Guid? userId,
        CancellationToken ct = default)
    {
        var analysis = await _context.UpgradeAnalyses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == analysisId, ct);

        if (analysis == null)
        {
            _logger.LogWarning("Analysis {AnalysisId} not found", analysisId);
            return null;
        }

        // Access control
        if (analysis.UserId.HasValue && userId != analysis.UserId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to access analysis {AnalysisId} owned by {OwnerId}",
                userId, analysisId, analysis.UserId);
            return null;
        }

        // Parse detected stones
        List<DetectedStoneDto>? detectedStones = null;
        if (!string.IsNullOrEmpty(analysis.DetectedStonesJson))
        {
            try
            {
                detectedStones = JsonSerializer.Deserialize<List<DetectedStoneDto>>(
                    analysis.DetectedStonesJson, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse detected stones for analysis {AnalysisId}", analysisId);
            }
        }

        // Parse additional analysis data
        AnalysisDataDto? analysisData = null;
        if (!string.IsNullOrEmpty(analysis.AnalysisDataJson))
        {
            try
            {
                analysisData = JsonSerializer.Deserialize<AnalysisDataDto>(
                    analysis.AnalysisDataJson, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse analysis data for analysis {AnalysisId}", analysisId);
            }
        }

        return new UpgradeAnalysisResultDto
        {
            AnalysisId = analysis.Id,
            OriginalImageUrl = analysis.OriginalImageUrl,
            JewelryType = analysis.JewelryType ?? "unknown",
            DetectedCategoryId = analysis.DetectedCategoryId,
            DetectedMetal = analysis.DetectedMetal,
            DetectedMetalDescription = analysis.DetectedMetalDescription,
            HasStones = analysis.HasStones,
            DetectedStones = detectedStones,
            StyleClassification = analysis.StyleClassification ?? "classic",
            ConfidenceScore = analysis.ConfidenceScore,
            AnalyzedAtUtc = analysis.CompletedAtUtc ?? analysis.UpdatedAtUtc,
            // New fields from Vision analysis
            PieceDescription = analysisData?.PieceDescription,
            ConfidenceNote = analysisData?.ConfidenceNote,
            ApparentFinish = analysisData?.ApparentFinish,
            AnalysisLimitations = analysisData?.AnalysisLimitations,
            ClarificationRequest = analysisData?.ClarificationRequest,
            PreviewGuidance = analysisData?.PreviewGuidance
        };
    }

    public async Task<UpgradeSuggestionsResponseDto?> GetSuggestionsAsync(
        Guid analysisId,
        Guid? userId,
        CancellationToken ct = default)
    {
        var analysis = await _context.UpgradeAnalyses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == analysisId, ct);

        if (analysis == null)
        {
            _logger.LogWarning("Analysis {AnalysisId} not found", analysisId);
            return null;
        }

        // Access control
        if (analysis.UserId.HasValue && userId != analysis.UserId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to access suggestions for analysis {AnalysisId} owned by {OwnerId}",
                userId, analysisId, analysis.UserId);
            return null;
        }

        // Check if analysis is complete
        if (analysis.Status != UpgradeAnalysisStatus.Completed)
        {
            _logger.LogWarning("Analysis {AnalysisId} not yet completed", analysisId);
            return null;
        }

        // Parse suggestions from stored JSON
        List<UpgradeSuggestionDto> suggestions;
        if (!string.IsNullOrEmpty(analysis.SuggestionsJson))
        {
            try
            {
                suggestions = JsonSerializer.Deserialize<List<UpgradeSuggestionDto>>(
                    analysis.SuggestionsJson, _jsonOptions) ?? new List<UpgradeSuggestionDto>();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse suggestions for analysis {AnalysisId}", analysisId);
                suggestions = new List<UpgradeSuggestionDto>();
            }
        }
        else
        {
            suggestions = new List<UpgradeSuggestionDto>();
        }

        return new UpgradeSuggestionsResponseDto
        {
            AnalysisId = analysis.Id,
            OriginalImageUrl = analysis.OriginalImageUrl,
            Suggestions = suggestions,
            GeneratedAtUtc = analysis.UpdatedAtUtc
        };
    }

    public async Task<UpgradePreviewJobDto> CreatePreviewJobAsync(
        UpgradePreviewRequestDto request,
        Guid? userId,
        CancellationToken ct = default)
    {
        // Validate guest client ID for anonymous users
        if (!userId.HasValue && string.IsNullOrWhiteSpace(request.GuestClientId))
        {
            throw new ArgumentException("GuestClientId is required for anonymous users");
        }

        var analysis = await _context.UpgradeAnalyses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AnalysisId, ct);

        if (analysis == null)
        {
            throw new ArgumentException($"Analysis {request.AnalysisId} not found");
        }

        // Access control
        if (analysis.UserId.HasValue && userId != analysis.UserId)
        {
            throw new UnauthorizedAccessException("Analysis does not belong to current user");
        }

        var now = DateTimeOffset.UtcNow;

        var job = new UpgradePreviewJob
        {
            Id = Guid.NewGuid(),
            AnalysisId = request.AnalysisId,
            UserId = userId,
            GuestClientId = request.GuestClientId,
            Status = AiPreviewStatus.Pending,
            KeptOriginal = request.KeepOriginal,
            AppliedSuggestionsJson = request.SelectedSuggestionIds.Count > 0
                ? JsonSerializer.Serialize(request.SelectedSuggestionIds, _jsonOptions)
                : null,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _context.UpgradePreviewJobs.Add(job);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created upgrade preview job {JobId} for analysis {AnalysisId}",
            job.Id, request.AnalysisId);

        // Note: Preview generation would be handled by a background service
        // For now, we mark the job as pending and it will be processed later
        // In a full implementation, this would trigger background processing

        return MapToJobDto(job, analysis.OriginalImageUrl);
    }

    public async Task<UpgradePreviewJobDto?> GetPreviewJobAsync(
        Guid jobId,
        Guid? userId,
        CancellationToken ct = default)
    {
        var job = await _context.UpgradePreviewJobs
            .AsNoTracking()
            .Include(j => j.Analysis)
            .FirstOrDefaultAsync(j => j.Id == jobId, ct);

        if (job == null)
        {
            _logger.LogWarning("Preview job {JobId} not found", jobId);
            return null;
        }

        // Access control
        if (job.UserId.HasValue && userId != job.UserId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to access job {JobId} owned by {OwnerId}",
                userId, jobId, job.UserId);
            return null;
        }

        return MapToJobDto(job, job.Analysis.OriginalImageUrl);
    }

    public async Task<IReadOnlyList<UpgradeAnalysisResultDto>> GetRecentAnalysesAsync(
        Guid userId,
        int take = 5,
        CancellationToken ct = default)
    {
        var analyses = await _context.UpgradeAnalyses
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.Status == UpgradeAnalysisStatus.Completed)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(take)
            .ToListAsync(ct);

        return analyses.Select(a => new UpgradeAnalysisResultDto
        {
            AnalysisId = a.Id,
            OriginalImageUrl = a.OriginalImageUrl,
            JewelryType = a.JewelryType ?? "unknown",
            DetectedCategoryId = a.DetectedCategoryId,
            DetectedMetal = a.DetectedMetal,
            DetectedMetalDescription = a.DetectedMetalDescription,
            HasStones = a.HasStones,
            StyleClassification = a.StyleClassification ?? "classic",
            ConfidenceScore = a.ConfidenceScore,
            AnalyzedAtUtc = a.CompletedAtUtc ?? a.UpdatedAtUtc
        }).ToList();
    }

    // ============================================================
    // PRIVATE HELPERS - OpenAI Vision Integration
    // ============================================================

    /// <summary>
    /// Performs real AI analysis using OpenAI Vision API
    /// </summary>
    private async Task PerformVisionAnalysisAsync(
        Guid analysisId,
        string imageUrl,
        CancellationToken ct)
    {
        var analysis = await _context.UpgradeAnalyses
            .FirstOrDefaultAsync(a => a.Id == analysisId, ct);

        if (analysis == null) return;

        try
        {
            analysis.Status = UpgradeAnalysisStatus.Analyzing;
            analysis.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Starting OpenAI Vision analysis for {AnalysisId}", analysisId);

            // Call OpenAI Vision API
            var visionResult = await _visionAnalyzer.AnalyzeJewelryImageAsync(imageUrl, ct);

            var now = DateTimeOffset.UtcNow;

            if (!visionResult.Success)
            {
                _logger.LogWarning(
                    "Vision analysis failed for {AnalysisId}: {Error}",
                    analysisId, visionResult.ErrorMessage);

                analysis.Status = UpgradeAnalysisStatus.Failed;
                analysis.ErrorMessage = visionResult.ErrorMessage ?? "Analysis could not be completed";
                analysis.UpdatedAtUtc = now;
                await _context.SaveChangesAsync(ct);
                return;
            }

            // Map Vision response to analysis entity
            MapVisionResultToAnalysis(analysis, visionResult);
            analysis.Status = UpgradeAnalysisStatus.Completed;
            analysis.CompletedAtUtc = now;
            analysis.UpdatedAtUtc = now;

            // Generate and store suggestions from Vision analysis
            var suggestions = MapVisionSuggestionsToDto(visionResult);
            analysis.SuggestionsJson = JsonSerializer.Serialize(suggestions, _jsonOptions);

            // Store additional analysis data
            var analysisData = new AnalysisDataDto
            {
                PieceDescription = visionResult.PieceDescription,
                ConfidenceNote = visionResult.ConfidenceNote,
                ApparentFinish = visionResult.DetectedAttributes.ApparentFinish,
                AnalysisLimitations = visionResult.AnalysisLimitations,
                ClarificationRequest = visionResult.ClarificationRequest != null
                    ? new ClarificationRequestDto
                    {
                        Type = visionResult.ClarificationRequest.Type,
                        Message = visionResult.ClarificationRequest.Message
                    }
                    : null,
                PreviewGuidance = new PreviewGuidanceDto
                {
                    Summary = visionResult.PreviewGuidance.Summary,
                    KeyVisualChanges = visionResult.PreviewGuidance.KeyVisualChanges
                }
            };
            analysis.AnalysisDataJson = JsonSerializer.Serialize(analysisData, _jsonOptions);

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Completed Vision analysis {AnalysisId}: {JewelryType}, {Metal}, hasStones={HasStones}, {SuggestionCount} suggestions",
                analysisId, analysis.JewelryType, analysis.DetectedMetal, analysis.HasStones, suggestions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vision analysis failed for {AnalysisId}", analysisId);

            analysis.Status = UpgradeAnalysisStatus.Failed;
            analysis.ErrorMessage = "Analysis service encountered an error. Please try again.";
            analysis.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// Maps Vision API response to analysis entity
    /// </summary>
    private void MapVisionResultToAnalysis(UpgradeAnalysis analysis, JewelryAnalysisResponse visionResult)
    {
        var attrs = visionResult.DetectedAttributes;

        // Map jewelry type
        analysis.JewelryType = NormalizeJewelryType(attrs.JewelryType);

        // Map to catalog category
        analysis.DetectedCategoryId = MapJewelryTypeToCategory(analysis.JewelryType);

        // Map metal
        analysis.DetectedMetal = NormalizeMetalType(attrs.ApparentMetal);
        analysis.DetectedMetalDescription = attrs.ApparentMetal;

        // Map stones
        analysis.HasStones = attrs.HasStones;
        if (attrs.HasStones && !string.IsNullOrEmpty(attrs.StoneDescription))
        {
            var detectedStones = new List<DetectedStoneDto>
            {
                new DetectedStoneDto
                {
                    StoneType = ExtractStoneType(attrs.StoneDescription),
                    Description = attrs.StoneDescription,
                    Position = "detected",
                    EstimatedCount = 1
                }
            };
            analysis.DetectedStonesJson = JsonSerializer.Serialize(detectedStones, _jsonOptions);
        }

        // Map style
        analysis.StyleClassification = NormalizeStyleCharacter(attrs.StyleCharacter);

        // Set confidence score based on whether there are limitations
        analysis.ConfidenceScore = string.IsNullOrEmpty(visionResult.AnalysisLimitations)
            ? 0.9
            : 0.7;
    }

    /// <summary>
    /// Maps Vision suggestions to DTO format
    /// </summary>
    private List<UpgradeSuggestionDto> MapVisionSuggestionsToDto(JewelryAnalysisResponse visionResult)
    {
        var suggestions = new List<UpgradeSuggestionDto>();
        var order = 0;

        foreach (var category in visionResult.ImprovementCategories)
        {
            var categoryEnum = MapCategoryIdToEnum(category.CategoryId);

            foreach (var suggestion in category.Suggestions)
            {
                suggestions.Add(new UpgradeSuggestionDto
                {
                    Id = Guid.TryParse(suggestion.SuggestionId, out var id) ? id : Guid.NewGuid(),
                    Category = categoryEnum,
                    CategoryLabel = category.CategoryLabel,
                    Title = TruncateString(suggestion.Title, 100),
                    Rationale = suggestion.Benefit,
                    Description = suggestion.Description,
                    Benefit = suggestion.Benefit,
                    ImpactLevel = suggestion.ImpactLevel,
                    CharacterNote = suggestion.CharacterNote,
                    DisplayOrder = order++,
                    ConflictGroup = GetConflictGroup(categoryEnum, suggestion.Title)
                });
            }
        }

        return suggestions;
    }

    private static string NormalizeJewelryType(string type)
    {
        return type?.ToLowerInvariant() switch
        {
            "ring" => "ring",
            "rings" => "ring",
            "pendant" => "pendant",
            "pendants" => "pendant",
            "earrings" => "earrings",
            "earring" => "earrings",
            "bracelet" => "bracelet",
            "bracelets" => "bracelet",
            "necklace" => "necklace",
            "necklaces" => "necklace",
            "brooch" => "brooch",
            "brooches" => "brooch",
            _ => "unknown"
        };
    }

    private static int? MapJewelryTypeToCategory(string jewelryType)
    {
        return jewelryType switch
        {
            "ring" => 1,
            "earrings" => 2,
            "pendant" => 3,
            "necklace" => 4,
            "bracelet" => 5,
            "brooch" => 7,
            _ => null
        };
    }

    private static string NormalizeMetalType(string apparentMetal)
    {
        var lower = apparentMetal?.ToLowerInvariant() ?? "";

        if (lower.Contains("platinum"))
            return "platinum";
        if (lower.Contains("white gold") || lower.Contains("white-gold"))
            return "white_gold";
        if (lower.Contains("rose gold") || lower.Contains("rose-gold") || lower.Contains("pink gold"))
            return "rose_gold";
        if (lower.Contains("yellow gold") || lower.Contains("yellow-gold"))
            return "yellow_gold";
        if (lower.Contains("gold"))
            return "yellow_gold";
        if (lower.Contains("silver"))
            return "silver";

        return "unknown";
    }

    private static string ExtractStoneType(string stoneDescription)
    {
        var lower = stoneDescription?.ToLowerInvariant() ?? "";

        if (lower.Contains("diamond")) return "diamond";
        if (lower.Contains("sapphire")) return "sapphire";
        if (lower.Contains("ruby")) return "ruby";
        if (lower.Contains("emerald")) return "emerald";
        if (lower.Contains("moissanite")) return "moissanite";
        if (lower.Contains("topaz")) return "topaz";
        if (lower.Contains("amethyst")) return "amethyst";
        if (lower.Contains("aquamarine")) return "aquamarine";

        return "gemstone";
    }

    private static string NormalizeStyleCharacter(string style)
    {
        var lower = style?.ToLowerInvariant() ?? "";

        if (lower.Contains("classic")) return "classic";
        if (lower.Contains("modern") || lower.Contains("contemporary")) return "modern";
        if (lower.Contains("vintage") || lower.Contains("antique")) return "vintage";
        if (lower.Contains("minimal")) return "minimalist";
        if (lower.Contains("art deco") || lower.Contains("art_deco")) return "art_deco";
        if (lower.Contains("bold") || lower.Contains("statement")) return "bold";

        return "classic";
    }

    private static UpgradeSuggestionCategory MapCategoryIdToEnum(string categoryId)
    {
        return categoryId?.ToLowerInvariant() switch
        {
            "material_finish" => UpgradeSuggestionCategory.Material,
            "stone_setting" => UpgradeSuggestionCategory.Stones,
            "proportion_balance" => UpgradeSuggestionCategory.Proportions,
            "craftsmanship_detail" => UpgradeSuggestionCategory.Craftsmanship,
            _ => UpgradeSuggestionCategory.Craftsmanship
        };
    }

    private static string? GetConflictGroup(UpgradeSuggestionCategory category, string title)
    {
        var lower = title?.ToLowerInvariant() ?? "";

        // Metal purity conflicts
        if (category == UpgradeSuggestionCategory.Material)
        {
            if (lower.Contains("18k") || lower.Contains("14k") || lower.Contains("gold"))
                return "metal_purity";
            if (lower.Contains("platinum") || lower.Contains("palladium"))
                return "metal_type";
        }

        return null;
    }

    private static string TruncateString(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";
    }

    private UpgradePreviewJobDto MapToJobDto(UpgradePreviewJob job, string originalImageUrl)
    {
        List<Guid>? appliedSuggestionIds = null;
        if (!string.IsNullOrEmpty(job.AppliedSuggestionsJson))
        {
            try
            {
                appliedSuggestionIds = JsonSerializer.Deserialize<List<Guid>>(
                    job.AppliedSuggestionsJson, _jsonOptions);
            }
            catch (JsonException)
            {
                // Ignore parsing errors
            }
        }

        return new UpgradePreviewJobDto
        {
            Id = job.Id,
            AnalysisId = job.AnalysisId,
            Status = job.Status,
            OriginalImageUrl = originalImageUrl,
            EnhancedImageUrl = job.EnhancedImageUrl,
            AppliedSuggestionIds = appliedSuggestionIds,
            KeptOriginal = job.KeptOriginal,
            ErrorMessage = job.ErrorMessage,
            CreatedAtUtc = job.CreatedAtUtc,
            UpdatedAtUtc = job.UpdatedAtUtc
        };
    }
}

/// <summary>
/// Internal DTO for storing extended analysis data as JSON
/// </summary>
internal class AnalysisDataDto
{
    public string? PieceDescription { get; set; }
    public string? ConfidenceNote { get; set; }
    public string? ApparentFinish { get; set; }
    public string? AnalysisLimitations { get; set; }
    public ClarificationRequestDto? ClarificationRequest { get; set; }
    public PreviewGuidanceDto? PreviewGuidance { get; set; }
}
