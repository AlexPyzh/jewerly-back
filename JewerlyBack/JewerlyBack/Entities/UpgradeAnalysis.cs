namespace JewerlyBack.Models;

/// <summary>
/// Represents an upgrade analysis session for an existing jewelry piece
/// </summary>
public class UpgradeAnalysis
{
    /// <summary>
    /// Unique identifier for this analysis session
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the user who initiated the analysis (null for guests)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Guest client ID for anonymous users
    /// </summary>
    public string? GuestClientId { get; set; }

    /// <summary>
    /// URL of the uploaded original image
    /// </summary>
    public required string OriginalImageUrl { get; set; }

    /// <summary>
    /// Current status of the analysis
    /// </summary>
    public UpgradeAnalysisStatus Status { get; set; }

    /// <summary>
    /// Detected jewelry type (ring, necklace, bracelet, etc.)
    /// </summary>
    public string? JewelryType { get; set; }

    /// <summary>
    /// Detected category ID from catalog
    /// </summary>
    public int? DetectedCategoryId { get; set; }

    /// <summary>
    /// Detected metal type/color
    /// </summary>
    public string? DetectedMetal { get; set; }

    /// <summary>
    /// Human-readable metal description
    /// </summary>
    public string? DetectedMetalDescription { get; set; }

    /// <summary>
    /// Whether stones were detected
    /// </summary>
    public bool HasStones { get; set; }

    /// <summary>
    /// JSON array of detected stones
    /// </summary>
    public string? DetectedStonesJson { get; set; }

    /// <summary>
    /// Overall style classification
    /// </summary>
    public string? StyleClassification { get; set; }

    /// <summary>
    /// Confidence score of analysis (0.0 - 1.0)
    /// </summary>
    public double ConfidenceScore { get; set; }

    /// <summary>
    /// JSON containing all AI analysis data
    /// </summary>
    public string? AnalysisDataJson { get; set; }

    /// <summary>
    /// JSON array of generated suggestions
    /// </summary>
    public string? SuggestionsJson { get; set; }

    /// <summary>
    /// Error message if analysis failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp when analysis was created
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Timestamp when analysis was completed
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; set; }

    /// <summary>
    /// Timestamp of last update
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    // Navigation properties
    public AppUser? User { get; set; }
    public JewelryCategory? DetectedCategory { get; set; }
    public ICollection<UpgradePreviewJob> PreviewJobs { get; set; } = new List<UpgradePreviewJob>();
}

/// <summary>
/// Status of an upgrade analysis
/// </summary>
public enum UpgradeAnalysisStatus
{
    /// <summary>
    /// Image uploaded, analysis pending
    /// </summary>
    Pending = 0,

    /// <summary>
    /// AI is analyzing the image
    /// </summary>
    Analyzing = 1,

    /// <summary>
    /// Analysis completed successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Analysis failed
    /// </summary>
    Failed = 3
}
