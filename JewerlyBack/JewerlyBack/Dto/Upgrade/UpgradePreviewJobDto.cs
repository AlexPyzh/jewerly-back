using JewerlyBack.Models;

namespace JewerlyBack.Dto.Upgrade;

/// <summary>
/// Status of an upgrade preview generation job
/// </summary>
public class UpgradePreviewJobDto
{
    /// <summary>
    /// Unique identifier for this preview job
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Reference to the analysis session
    /// </summary>
    public required Guid AnalysisId { get; set; }

    /// <summary>
    /// Current status of the job
    /// </summary>
    public required AiPreviewStatus Status { get; set; }

    /// <summary>
    /// URL of the original image
    /// </summary>
    public required string OriginalImageUrl { get; set; }

    /// <summary>
    /// URL of the generated enhanced preview (when completed)
    /// </summary>
    public string? EnhancedImageUrl { get; set; }

    /// <summary>
    /// List of applied suggestion IDs
    /// </summary>
    public IReadOnlyList<Guid>? AppliedSuggestionIds { get; set; }

    /// <summary>
    /// Whether the original design was kept (no enhancements)
    /// </summary>
    public bool KeptOriginal { get; set; }

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
}
