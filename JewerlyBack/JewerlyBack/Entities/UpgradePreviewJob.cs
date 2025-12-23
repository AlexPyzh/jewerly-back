namespace JewerlyBack.Models;

/// <summary>
/// Represents a preview generation job for an upgrade analysis
/// </summary>
public class UpgradePreviewJob
{
    /// <summary>
    /// Unique identifier for this preview job
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the analysis session
    /// </summary>
    public Guid AnalysisId { get; set; }

    /// <summary>
    /// ID of the user (null for guests)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Guest client ID for anonymous users
    /// </summary>
    public string? GuestClientId { get; set; }

    /// <summary>
    /// Current status of the preview job
    /// </summary>
    public AiPreviewStatus Status { get; set; }

    /// <summary>
    /// Whether the original design was kept (no enhancements)
    /// </summary>
    public bool KeptOriginal { get; set; }

    /// <summary>
    /// JSON array of applied suggestion IDs
    /// </summary>
    public string? AppliedSuggestionsJson { get; set; }

    /// <summary>
    /// The prompt sent to AI for generation
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// URL of the generated enhanced preview
    /// </summary>
    public string? EnhancedImageUrl { get; set; }

    /// <summary>
    /// Error message if generation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp when job was created
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Timestamp of last update
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    // Navigation properties
    public UpgradeAnalysis Analysis { get; set; } = null!;
    public AppUser? User { get; set; }
}
