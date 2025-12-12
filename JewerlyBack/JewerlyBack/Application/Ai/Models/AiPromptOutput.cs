using System.Text.Json.Serialization;

namespace JewerlyBack.Application.Ai.Models;

/// <summary>
/// Clean, AI-optimized output model for jewelry preview generation.
/// Contains only information relevant for image generation - no database IDs or technical codes.
/// </summary>
public sealed class AiPromptOutput
{
    /// <summary>
    /// The main instruction prompt - a single flowing sentence describing what to render.
    /// Example: "Ultra high-quality studio render of a ring in 14K Rose Gold featuring a classic solid band design..."
    /// </summary>
    [JsonPropertyName("mainPrompt")]
    public required string MainPrompt { get; init; }

    /// <summary>
    /// Natural language description - a flowing paragraph telling the story of the jewelry piece.
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>
    /// Clean JSON configuration with only creative parameters needed for image generation.
    /// </summary>
    [JsonPropertyName("configuration")]
    public required AiPromptConfiguration Configuration { get; init; }
}

/// <summary>
/// Clean configuration object containing only creative parameters for AI image generation.
/// No database IDs, no technical codes, no hex colors.
/// </summary>
public sealed class AiPromptConfiguration
{
    /// <summary>
    /// The jewelry type in singular form: ring, earring, necklace, pendant, bracelet
    /// </summary>
    [JsonPropertyName("jewelryType")]
    public required string JewelryType { get; init; }

    /// <summary>
    /// Design details describing the model style
    /// </summary>
    [JsonPropertyName("design")]
    public required AiPromptDesign Design { get; init; }

    /// <summary>
    /// Material details with human-readable color names
    /// </summary>
    [JsonPropertyName("material")]
    public required AiPromptMaterial Material { get; init; }

    /// <summary>
    /// Embellishments (stones and engraving). Only included if stones or engraving exist.
    /// </summary>
    [JsonPropertyName("embellishments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AiPromptEmbellishments? Embellishments { get; init; }

    /// <summary>
    /// Photography/rendering style instructions
    /// </summary>
    [JsonPropertyName("renderingStyle")]
    public required string RenderingStyle { get; init; }
}

/// <summary>
/// Design information for the jewelry piece
/// </summary>
public sealed class AiPromptDesign
{
    /// <summary>
    /// The style name from the base model (e.g., "Classic Solid Band", "Solitaire Engagement Ring")
    /// </summary>
    [JsonPropertyName("styleName")]
    public required string StyleName { get; init; }

    /// <summary>
    /// Visual characteristics describing the shape and form
    /// </summary>
    [JsonPropertyName("visualCharacteristics")]
    public required string VisualCharacteristics { get; init; }

    /// <summary>
    /// Style descriptor keywords (e.g., ["timeless", "elegant", "minimalist"])
    /// </summary>
    [JsonPropertyName("aestheticKeywords")]
    public required IReadOnlyList<string> AestheticKeywords { get; init; }
}

/// <summary>
/// Material information with human-readable descriptions
/// </summary>
public sealed class AiPromptMaterial
{
    /// <summary>
    /// Full material name (e.g., "14K Rose Gold", "Sterling Silver")
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Metal type (gold, silver, platinum, titanium)
    /// </summary>
    [JsonPropertyName("metalType")]
    public required string MetalType { get; init; }

    /// <summary>
    /// Purity with suffix if applicable (e.g., "14K", "18K", "925")
    /// </summary>
    [JsonPropertyName("purity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Purity { get; init; }

    /// <summary>
    /// Human-readable color description (e.g., "warm rose pink tone", "bright yellow gold")
    /// </summary>
    [JsonPropertyName("colorDescription")]
    public required string ColorDescription { get; init; }
}

/// <summary>
/// Embellishments container for stones and engraving.
/// Only create this object if at least one embellishment exists.
/// </summary>
public sealed class AiPromptEmbellishments
{
    /// <summary>
    /// Array of stone descriptions. Only include if stones exist.
    /// </summary>
    [JsonPropertyName("stones")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<AiPromptStone>? Stones { get; init; }

    /// <summary>
    /// Engraved inscription text. Only include if engraving exists.
    /// </summary>
    [JsonPropertyName("personalInscription")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PersonalInscription { get; init; }
}

/// <summary>
/// Stone description with human-readable properties
/// </summary>
public sealed class AiPromptStone
{
    /// <summary>
    /// Gemstone type name (e.g., "Diamond", "Amethyst", "Ruby")
    /// </summary>
    [JsonPropertyName("gemType")]
    public required string GemType { get; init; }

    /// <summary>
    /// Human-readable color description (e.g., "deep purple", "brilliant clear", "vivid red")
    /// </summary>
    [JsonPropertyName("colorDescription")]
    public required string ColorDescription { get; init; }

    /// <summary>
    /// Number of stones of this type
    /// </summary>
    [JsonPropertyName("quantity")]
    public required int Quantity { get; init; }

    /// <summary>
    /// Size indicator if available (e.g., "0.5 carats", "3mm")
    /// </summary>
    [JsonPropertyName("sizeIndicator")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SizeIndicator { get; init; }

    /// <summary>
    /// Placement description (e.g., "center setting", "accent stones", "pavé band")
    /// </summary>
    [JsonPropertyName("placementDescription")]
    public required string PlacementDescription { get; init; }
}
