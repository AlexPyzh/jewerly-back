namespace JewerlyBack.Dto.Upgrade;

/// <summary>
/// Request to generate an AI preview for an upgraded jewelry piece
/// </summary>
public class UpgradePreviewRequestDto
{
    /// <summary>
    /// Reference to the analysis session
    /// </summary>
    public required Guid AnalysisId { get; set; }

    /// <summary>
    /// List of selected suggestion IDs to apply
    /// Empty list means keep original design
    /// </summary>
    public IReadOnlyList<Guid> SelectedSuggestionIds { get; set; } = Array.Empty<Guid>();

    /// <summary>
    /// Whether to keep the original design (no suggestions applied)
    /// </summary>
    public bool KeepOriginal { get; set; }

    /// <summary>
    /// Guest client ID for anonymous users
    /// </summary>
    public string? GuestClientId { get; set; }
}

/// <summary>
/// Request to upload and analyze a jewelry image
/// </summary>
public class UpgradeImageUploadRequestDto
{
    /// <summary>
    /// Guest client ID for anonymous users
    /// </summary>
    public string? GuestClientId { get; set; }
}

/// <summary>
/// Response after uploading an image for upgrade analysis
/// </summary>
public class UpgradeImageUploadResponseDto
{
    /// <summary>
    /// The analysis session ID to use for subsequent requests
    /// </summary>
    public required Guid AnalysisId { get; set; }

    /// <summary>
    /// URL where the uploaded image is stored
    /// </summary>
    public required string ImageUrl { get; set; }

    /// <summary>
    /// Message to display during analysis
    /// </summary>
    public string Message { get; set; } = "Your image has been uploaded. Analysis in progress.";
}
