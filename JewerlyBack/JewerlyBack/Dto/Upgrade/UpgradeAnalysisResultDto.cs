namespace JewerlyBack.Dto.Upgrade;

/// <summary>
/// Result of AI analysis of an uploaded jewelry image
/// </summary>
public class UpgradeAnalysisResultDto
{
    /// <summary>
    /// Unique identifier for this analysis session
    /// </summary>
    public required Guid AnalysisId { get; set; }

    /// <summary>
    /// URL of the original uploaded image
    /// </summary>
    public required string OriginalImageUrl { get; set; }

    /// <summary>
    /// Detected jewelry type (ring, necklace, bracelet, earrings, pendant, brooch)
    /// </summary>
    public required string JewelryType { get; set; }

    /// <summary>
    /// Detected or inferred category ID from the catalog
    /// </summary>
    public int? DetectedCategoryId { get; set; }

    /// <summary>
    /// Detected metal type/color (yellow_gold, white_gold, rose_gold, platinum, silver)
    /// </summary>
    public string? DetectedMetal { get; set; }

    /// <summary>
    /// Human-readable metal description
    /// </summary>
    public string? DetectedMetalDescription { get; set; }

    /// <summary>
    /// Whether stones were detected in the image
    /// </summary>
    public bool HasStones { get; set; }

    /// <summary>
    /// Detected stone types if present
    /// </summary>
    public IReadOnlyList<DetectedStoneDto>? DetectedStones { get; set; }

    /// <summary>
    /// Overall style classification
    /// </summary>
    public required string StyleClassification { get; set; }

    /// <summary>
    /// Confidence score of the analysis (0.0 - 1.0)
    /// </summary>
    public double ConfidenceScore { get; set; }

    /// <summary>
    /// Timestamp when analysis was completed
    /// </summary>
    public DateTimeOffset AnalyzedAtUtc { get; set; }

    // ========================================
    // New fields from OpenAI Vision analysis
    // ========================================

    /// <summary>
    /// One-line neutral description of the jewelry piece from AI analysis
    /// </summary>
    public string? PieceDescription { get; set; }

    /// <summary>
    /// Brief statement about image quality or assumptions made during analysis
    /// </summary>
    public string? ConfidenceNote { get; set; }

    /// <summary>
    /// Apparent finish of the metal (e.g., "polished with subtle brushed accents")
    /// </summary>
    public string? ApparentFinish { get; set; }

    /// <summary>
    /// Any factors that limited the analysis (null if no limitations)
    /// </summary>
    public string? AnalysisLimitations { get; set; }

    /// <summary>
    /// Clarification request if image quality is poor
    /// </summary>
    public ClarificationRequestDto? ClarificationRequest { get; set; }

    /// <summary>
    /// Preview guidance describing visual changes if suggestions applied
    /// </summary>
    public PreviewGuidanceDto? PreviewGuidance { get; set; }
}

/// <summary>
/// Request for clarification when image quality is poor
/// </summary>
public class ClarificationRequestDto
{
    /// <summary>
    /// Type of clarification needed (image_quality, object_recognition)
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// User-friendly message explaining what's needed
    /// </summary>
    public required string Message { get; set; }
}

/// <summary>
/// Preview guidance describing visual changes if suggestions applied
/// </summary>
public class PreviewGuidanceDto
{
    /// <summary>
    /// One sentence describing overall visual direction if suggestions applied
    /// </summary>
    public required string Summary { get; set; }

    /// <summary>
    /// List of 2-4 primary visual differences
    /// </summary>
    public IReadOnlyList<string> KeyVisualChanges { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Detected stone information
/// </summary>
public class DetectedStoneDto
{
    /// <summary>
    /// Type of stone detected (diamond, ruby, emerald, sapphire, etc.)
    /// </summary>
    public required string StoneType { get; set; }

    /// <summary>
    /// Human-readable stone description
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Position in the piece (center, side, accent, etc.)
    /// </summary>
    public string? Position { get; set; }

    /// <summary>
    /// Estimated count of stones in this position
    /// </summary>
    public int EstimatedCount { get; set; } = 1;

    /// <summary>
    /// Matched stone type ID from catalog (if found)
    /// </summary>
    public int? MatchedStoneTypeId { get; set; }
}
