namespace JewerlyBack.Application.Ai.Models;

/// <summary>
/// Represents the semantic context for AI image generation.
/// Contains human-readable descriptions of all aspects of the jewelry piece.
/// This is an internal structure designed to be easily extensible as new semantic fields are added.
/// </summary>
/// <remarks>
/// Design principles:
/// - All fields are optional (nullable) for backward compatibility
/// - New description fields can be added without breaking existing code
/// - Descriptions are in English and suitable for AI prompt generation
/// - This structure is internal to the backend and not exposed to frontend
/// </remarks>
public sealed class SemanticContext
{
    /// <summary>
    /// AI-optimized description of the jewelry category.
    /// Explains what this type of jewelry is and how it's worn.
    /// Example: "A ring is a circular band worn on the finger..."
    /// </summary>
    public string? CategoryDescription { get; init; }

    /// <summary>
    /// AI-optimized description of the base model's design and geometry.
    /// Provides specific details about the structure, shape, and style.
    /// Example: "A classic solid band ring with a smooth, even surface..."
    /// </summary>
    public string? BaseModelDescription { get; init; }

    /// <summary>
    /// AI-optimized description of the material properties and appearance.
    /// Future field - currently not populated but ready for implementation.
    /// Example: "14K yellow gold with a warm lustrous finish..."
    /// </summary>
    public string? MaterialDescription { get; init; }

    /// <summary>
    /// AI-optimized description of the stones configuration.
    /// Future field - currently not populated but ready for implementation.
    /// Example: "A single 1-carat round brilliant diamond..."
    /// </summary>
    public string? StonesDescription { get; init; }

    /// <summary>
    /// AI-optimized description of engraving details.
    /// Future field - currently not populated but ready for implementation.
    /// Example: "Personalized engraving in elegant script font..."
    /// </summary>
    public string? EngravingDescription { get; init; }

    /// <summary>
    /// Any additional semantic context for future extensions.
    /// Allows adding new description types without modifying the class structure.
    /// </summary>
    public Dictionary<string, string>? AdditionalContext { get; init; }

    /// <summary>
    /// Combines all non-null semantic descriptions into a single coherent narrative.
    /// Descriptions are ordered from general (category) to specific (details).
    /// </summary>
    /// <param name="separator">Separator between description segments (default: space)</param>
    /// <returns>Combined semantic description suitable for AI prompts</returns>
    public string ToCombinedDescription(string separator = " ")
    {
        var segments = new List<string>();

        // Order: Category -> BaseModel -> Material -> Stones -> Engraving -> Additional
        // This creates a narrative flow from general to specific

        if (!string.IsNullOrWhiteSpace(CategoryDescription))
        {
            segments.Add(CategoryDescription.Trim());
        }

        if (!string.IsNullOrWhiteSpace(BaseModelDescription))
        {
            segments.Add(BaseModelDescription.Trim());
        }

        if (!string.IsNullOrWhiteSpace(MaterialDescription))
        {
            segments.Add(MaterialDescription.Trim());
        }

        if (!string.IsNullOrWhiteSpace(StonesDescription))
        {
            segments.Add(StonesDescription.Trim());
        }

        if (!string.IsNullOrWhiteSpace(EngravingDescription))
        {
            segments.Add(EngravingDescription.Trim());
        }

        // Include additional context if provided
        if (AdditionalContext?.Any() == true)
        {
            foreach (var kvp in AdditionalContext.OrderBy(x => x.Key))
            {
                if (!string.IsNullOrWhiteSpace(kvp.Value))
                {
                    segments.Add(kvp.Value.Trim());
                }
            }
        }

        return string.Join(separator, segments);
    }

    /// <summary>
    /// Checks if the semantic context has any populated descriptions.
    /// </summary>
    public bool HasContent =>
        !string.IsNullOrWhiteSpace(CategoryDescription) ||
        !string.IsNullOrWhiteSpace(BaseModelDescription) ||
        !string.IsNullOrWhiteSpace(MaterialDescription) ||
        !string.IsNullOrWhiteSpace(StonesDescription) ||
        !string.IsNullOrWhiteSpace(EngravingDescription) ||
        (AdditionalContext?.Any(kvp => !string.IsNullOrWhiteSpace(kvp.Value)) == true);

    /// <summary>
    /// Creates an empty semantic context with no descriptions.
    /// </summary>
    public static SemanticContext Empty => new();
}
