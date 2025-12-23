namespace JewerlyBack.Dto.Upgrade;

/// <summary>
/// A suggested improvement for the jewelry piece
/// </summary>
public class UpgradeSuggestionDto
{
    /// <summary>
    /// Unique identifier for this suggestion
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Category of the suggestion (material, stones, proportions, craftsmanship)
    /// </summary>
    public required UpgradeSuggestionCategory Category { get; set; }

    /// <summary>
    /// Human-readable category label
    /// </summary>
    public required string CategoryLabel { get; set; }

    /// <summary>
    /// Short, clear title of the suggestion (3-6 words)
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Detailed explanation of why this improvement is suggested
    /// Focuses on benefits: balance, durability, elegance, craftsmanship
    /// </summary>
    public required string Rationale { get; set; }

    /// <summary>
    /// Reference to catalog item if applicable (material ID, stone type ID, etc.)
    /// </summary>
    public int? CatalogReferenceId { get; set; }

    /// <summary>
    /// Type of catalog reference (material, stoneType, etc.)
    /// </summary>
    public string? CatalogReferenceType { get; set; }

    /// <summary>
    /// Priority/order of this suggestion (lower = higher priority)
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether this suggestion conflicts with another (mutex group)
    /// Suggestions in the same conflict group are mutually exclusive
    /// </summary>
    public string? ConflictGroup { get; set; }

    // ========================================
    // New fields from OpenAI Vision analysis
    // ========================================

    /// <summary>
    /// Brief description of what would change with this suggestion
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Explanation of why this improves the piece (durability, comfort, elegance, etc.)
    /// </summary>
    public string? Benefit { get; set; }

    /// <summary>
    /// Impact level of the suggestion: subtle, moderate, or bold
    /// </summary>
    public string ImpactLevel { get; set; } = "moderate";

    /// <summary>
    /// Note if this suggestion slightly alters the character of the original piece
    /// </summary>
    public string? CharacterNote { get; set; }
}

/// <summary>
/// Categories for upgrade suggestions
/// </summary>
public enum UpgradeSuggestionCategory
{
    /// <summary>
    /// Material refinement (metal type, purity, finish)
    /// </summary>
    Material = 0,

    /// <summary>
    /// Stone enhancement (quality, replacement, accent stones)
    /// </summary>
    Stones = 1,

    /// <summary>
    /// Proportions and balance adjustments
    /// </summary>
    Proportions = 2,

    /// <summary>
    /// Craftsmanship and detailing improvements
    /// </summary>
    Craftsmanship = 3
}

/// <summary>
/// Response containing all upgrade suggestions for an analyzed piece
/// </summary>
public class UpgradeSuggestionsResponseDto
{
    /// <summary>
    /// Reference to the analysis this is based on
    /// </summary>
    public required Guid AnalysisId { get; set; }

    /// <summary>
    /// URL of the original image
    /// </summary>
    public required string OriginalImageUrl { get; set; }

    /// <summary>
    /// List of suggested improvements, grouped by category
    /// </summary>
    public required IReadOnlyList<UpgradeSuggestionDto> Suggestions { get; set; }

    /// <summary>
    /// Timestamp when suggestions were generated
    /// </summary>
    public DateTimeOffset GeneratedAtUtc { get; set; }
}
