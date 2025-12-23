namespace JewerlyBack.Application.Ai;

/// <summary>
/// Service for analyzing jewelry images using OpenAI Vision API.
/// Provides structured analysis and improvement suggestions.
/// </summary>
public interface IJewelryVisionAnalyzer
{
    /// <summary>
    /// Analyzes a jewelry image and returns structured analysis with improvement suggestions.
    /// </summary>
    /// <param name="imageUrl">Public URL of the jewelry image to analyze.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Structured analysis result with suggestions.</returns>
    /// <exception cref="JewelryAnalysisException">When analysis fails or image cannot be processed.</exception>
    Task<JewelryAnalysisResponse> AnalyzeJewelryImageAsync(
        string imageUrl,
        CancellationToken ct = default);

    /// <summary>
    /// Analyzes a jewelry image from base64 data.
    /// </summary>
    /// <param name="base64Image">Base64-encoded image data.</param>
    /// <param name="mimeType">MIME type of the image (e.g., "image/jpeg").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Structured analysis result with suggestions.</returns>
    Task<JewelryAnalysisResponse> AnalyzeJewelryImageFromBase64Async(
        string base64Image,
        string mimeType,
        CancellationToken ct = default);
}

/// <summary>
/// Complete response from jewelry vision analysis.
/// </summary>
public class JewelryAnalysisResponse
{
    /// <summary>
    /// Whether the analysis was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// One-line neutral description of the jewelry piece.
    /// </summary>
    public string PieceDescription { get; set; } = string.Empty;

    /// <summary>
    /// Brief statement about image quality or assumptions made.
    /// </summary>
    public string ConfidenceNote { get; set; } = string.Empty;

    /// <summary>
    /// Detected attributes of the jewelry piece.
    /// </summary>
    public DetectedJewelryAttributes DetectedAttributes { get; set; } = new();

    /// <summary>
    /// Improvement suggestions grouped by category.
    /// </summary>
    public List<ImprovementCategory> ImprovementCategories { get; set; } = new();

    /// <summary>
    /// The mandatory "Keep original design" option.
    /// </summary>
    public KeepOriginalOption KeepOriginal { get; set; } = new();

    /// <summary>
    /// Preview guidance describing visual changes if suggestions applied.
    /// </summary>
    public PreviewGuidance PreviewGuidance { get; set; } = new();

    /// <summary>
    /// Any factors that limited the analysis (null if no limitations).
    /// </summary>
    public string? AnalysisLimitations { get; set; }

    /// <summary>
    /// Clarification request if image quality is poor (null if not needed).
    /// </summary>
    public ClarificationRequest? ClarificationRequest { get; set; }

    /// <summary>
    /// Error message if analysis failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Detected attributes of the jewelry piece.
/// </summary>
public class DetectedJewelryAttributes
{
    /// <summary>
    /// Type of jewelry (ring, pendant, earrings, bracelet, brooch, other).
    /// </summary>
    public string JewelryType { get; set; } = "unknown";

    /// <summary>
    /// Whether stones are present in the piece.
    /// </summary>
    public bool HasStones { get; set; }

    /// <summary>
    /// Description of stones if present.
    /// </summary>
    public string? StoneDescription { get; set; }

    /// <summary>
    /// Apparent metal type with cautious phrasing.
    /// </summary>
    public string ApparentMetal { get; set; } = "uncertain";

    /// <summary>
    /// Apparent finish of the metal.
    /// </summary>
    public string ApparentFinish { get; set; } = "polished";

    /// <summary>
    /// Style character of the piece.
    /// </summary>
    public string StyleCharacter { get; set; } = "classic";
}

/// <summary>
/// Category of improvement suggestions.
/// </summary>
public class ImprovementCategory
{
    /// <summary>
    /// Category identifier (material_finish, stone_setting, proportion_balance, craftsmanship_detail).
    /// </summary>
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable category name.
    /// </summary>
    public string CategoryLabel { get; set; } = string.Empty;

    /// <summary>
    /// List of suggestions in this category.
    /// </summary>
    public List<ImprovementSuggestion> Suggestions { get; set; } = new();
}

/// <summary>
/// A single improvement suggestion.
/// </summary>
public class ImprovementSuggestion
{
    /// <summary>
    /// Unique identifier for this suggestion.
    /// </summary>
    public string SuggestionId { get; set; } = string.Empty;

    /// <summary>
    /// Concise, elegant title (3-6 words).
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Brief description of what would change.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Explanation of why this improves the piece.
    /// </summary>
    public string Benefit { get; set; } = string.Empty;

    /// <summary>
    /// Impact level: subtle, moderate, or bold.
    /// </summary>
    public string ImpactLevel { get; set; } = "moderate";

    /// <summary>
    /// Note if the suggestion shifts the piece's aesthetic character (null if not).
    /// </summary>
    public string? CharacterNote { get; set; }
}

/// <summary>
/// The keep original design option.
/// </summary>
public class KeepOriginalOption
{
    /// <summary>
    /// Title of the option.
    /// </summary>
    public string Title { get; set; } = "Keep Original Design";

    /// <summary>
    /// Description of the option.
    /// </summary>
    public string Description { get; set; } = "Preserve the piece exactly as designed, honoring the original vision.";

    /// <summary>
    /// Whether this is the default option.
    /// </summary>
    public bool IsDefault { get; set; } = true;
}

/// <summary>
/// Preview guidance describing visual changes.
/// </summary>
public class PreviewGuidance
{
    /// <summary>
    /// One sentence describing overall visual direction if suggestions applied.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// List of 2-4 primary visual differences.
    /// </summary>
    public List<string> KeyVisualChanges { get; set; } = new();
}

/// <summary>
/// Request for clarification when image quality is poor.
/// </summary>
public class ClarificationRequest
{
    /// <summary>
    /// Type of clarification needed (image_quality, object_recognition).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly message explaining what's needed.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Exception thrown when jewelry analysis fails.
/// </summary>
public class JewelryAnalysisException : Exception
{
    public JewelryAnalysisException(string message) : base(message) { }
    public JewelryAnalysisException(string message, Exception innerException) : base(message, innerException) { }
}
