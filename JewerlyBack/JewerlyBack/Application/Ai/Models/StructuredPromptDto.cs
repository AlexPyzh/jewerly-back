using System.Text.Json.Serialization;

namespace JewerlyBack.Application.Ai.Models;

/// <summary>
/// Structured JSON prompt for AI image generation.
/// Uses simple name/description pattern for all components.
/// All descriptions come directly from database entities.
/// </summary>
public sealed class StructuredPromptDto
{
    /// <summary>
    /// Task type - always "text_to_image" for image generation.
    /// </summary>
    [JsonPropertyName("task")]
    public string Task { get; init; } = "text_to_image";

    /// <summary>
    /// Subject description - the jewelry piece details using name/description pattern.
    /// </summary>
    [JsonPropertyName("subject")]
    public required StructuredPromptSubjectDto Subject { get; init; }

    /// <summary>
    /// Personalization options (engraving).
    /// Only included when engraving text is present.
    /// </summary>
    [JsonPropertyName("personalization")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StructuredPromptPersonalizationDto? Personalization { get; init; }

    /// <summary>
    /// Rendering settings - always photorealistic studio.
    /// </summary>
    [JsonPropertyName("rendering")]
    public StructuredPromptRenderingDto Rendering { get; init; } = new();

    /// <summary>
    /// Background settings - always pure white.
    /// </summary>
    [JsonPropertyName("background")]
    public StructuredPromptBackgroundDto Background { get; init; } = new();

    /// <summary>
    /// Generation constraints - forbidden elements.
    /// </summary>
    [JsonPropertyName("constraints")]
    public StructuredPromptConstraintsDto Constraints { get; init; } = new();

    /// <summary>
    /// Output format settings.
    /// </summary>
    [JsonPropertyName("output")]
    public StructuredPromptOutputDto Output { get; init; } = new();
}

/// <summary>
/// Subject description for the jewelry piece.
/// Uses simple name/description pattern for all components.
/// </summary>
public sealed class StructuredPromptSubjectDto
{
    /// <summary>
    /// Category information (e.g., Rings, Earrings, Necklaces).
    /// </summary>
    [JsonPropertyName("category")]
    public required NameDescriptionDto Category { get; init; }

    /// <summary>
    /// Base model/design information.
    /// </summary>
    [JsonPropertyName("base_model")]
    public required NameDescriptionDto BaseModel { get; init; }

    /// <summary>
    /// Material information (e.g., 14K White Gold).
    /// </summary>
    [JsonPropertyName("material")]
    public required NameDescriptionDto Material { get; init; }

    /// <summary>
    /// Center stone details (optional).
    /// Only included when the configuration has stones.
    /// </summary>
    [JsonPropertyName("center_stone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NameDescriptionDto? CenterStone { get; init; }
}

/// <summary>
/// Simple name/description pair for any component.
/// All values come directly from database entities.
/// </summary>
public sealed class NameDescriptionDto
{
    /// <summary>
    /// Display name of the component (from database).
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Description of the component (from database).
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }
}

/// <summary>
/// Personalization options.
/// </summary>
public sealed class StructuredPromptPersonalizationDto
{
    /// <summary>
    /// Engraving details.
    /// </summary>
    [JsonPropertyName("engraving")]
    public required StructuredPromptEngravingDto Engraving { get; init; }
}

/// <summary>
/// Engraving details.
/// </summary>
public sealed class StructuredPromptEngravingDto
{
    /// <summary>
    /// The engraving text.
    /// </summary>
    [JsonPropertyName("text")]
    public required string Text { get; init; }

    /// <summary>
    /// Engraving placement - outside_band for rings.
    /// </summary>
    [JsonPropertyName("placement")]
    public string Placement { get; init; } = "outside_band";

    /// <summary>
    /// Priority level for rendering the engraving.
    /// </summary>
    [JsonPropertyName("priority")]
    public string Priority { get; init; } = "high";

    /// <summary>
    /// Whether the engraving must be readable in the generated image.
    /// </summary>
    [JsonPropertyName("must_be_readable")]
    public bool MustBeReadable { get; init; } = true;
}

/// <summary>
/// Rendering settings - hardcoded for consistent quality.
/// </summary>
public sealed class StructuredPromptRenderingDto
{
    /// <summary>
    /// Rendering style - always photorealistic_studio.
    /// </summary>
    [JsonPropertyName("style")]
    public string Style { get; init; } = "photorealistic_studio";

    /// <summary>
    /// Detail level - always high.
    /// </summary>
    [JsonPropertyName("detail")]
    public string Detail { get; init; } = "high";
}

/// <summary>
/// Background settings - hardcoded for pure white background.
/// </summary>
public sealed class StructuredPromptBackgroundDto
{
    /// <summary>
    /// Background color - always pure white.
    /// </summary>
    [JsonPropertyName("color")]
    public string Color { get; init; } = "#FFFFFF";

    /// <summary>
    /// Enforce pure white only - no gradients or textures.
    /// </summary>
    [JsonPropertyName("pure_white_only")]
    public bool PureWhiteOnly { get; init; } = true;

    /// <summary>
    /// No shadows on background.
    /// </summary>
    [JsonPropertyName("no_shadows")]
    public bool NoShadows { get; init; } = true;

    /// <summary>
    /// No reflections on background.
    /// </summary>
    [JsonPropertyName("no_reflections")]
    public bool NoReflections { get; init; } = true;
}

/// <summary>
/// Generation constraints - hardcoded forbidden elements.
/// </summary>
public sealed class StructuredPromptConstraintsDto
{
    /// <summary>
    /// List of forbidden elements in the generated image.
    /// </summary>
    [JsonPropertyName("forbid")]
    public string[] Forbid { get; init; } =
    [
        "text_inside_band",
        "misspelled_or_mirrored_text",
        "non_white_background",
        "background_shadows",
        "background_reflections",
        "props",
        "logos",
        "watermarks"
    ];
}

/// <summary>
/// Output format settings - hardcoded for consistent output.
/// </summary>
public sealed class StructuredPromptOutputDto
{
    /// <summary>
    /// Aspect ratio - always 1:1 square.
    /// </summary>
    [JsonPropertyName("aspect_ratio")]
    public string AspectRatio { get; init; } = "1:1";

    /// <summary>
    /// Resolution in pixels.
    /// </summary>
    [JsonPropertyName("resolution")]
    public int Resolution { get; init; } = 600;
}
