using JewerlyBack.Application.Ai.Models;
using JewerlyBack.Models;

namespace JewerlyBack.Application.Ai;

/// <summary>
/// Service for building AI prompts based on jewelry configuration.
/// </summary>
public interface IAiPromptBuilder
{
    /// <summary>
    /// Builds a structured JSON prompt for AI preview generation.
    /// This is the preferred method for generating prompts.
    /// </summary>
    /// <param name="aiConfig">Semantic configuration of the jewelry piece (AiConfigDto).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Serialized JSON string containing the structured prompt.</returns>
    /// <remarks>
    /// The structured prompt includes:
    /// - task: always "text_to_image"
    /// - subject: jewelry type, style, material, center stone
    /// - personalization: engraving details (if present)
    /// - rendering: hardcoded photorealistic studio settings
    /// - background: hardcoded pure white background
    /// - constraints: hardcoded forbidden elements
    /// - output: hardcoded 1:1 aspect ratio and resolution
    /// </remarks>
    Task<string> BuildStructuredPromptAsync(AiConfigDto aiConfig, CancellationToken ct = default);

    /// <summary>
    /// Builds a structured prompt DTO object for AI preview generation.
    /// Use this when you need the object instead of serialized JSON.
    /// </summary>
    /// <param name="aiConfig">Semantic configuration of the jewelry piece (AiConfigDto).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>StructuredPromptDto object ready for serialization.</returns>
    Task<StructuredPromptDto> BuildStructuredPromptDtoAsync(AiConfigDto aiConfig, CancellationToken ct = default);

    /// <summary>
    /// Builds a text-based prompt for AI preview generation based on semantic configuration.
    /// </summary>
    /// <param name="aiConfig">Semantic configuration of the jewelry piece (AiConfigDto).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Generated prompt for the AI model.</returns>
    /// <remarks>
    /// The prompt includes:
    /// - Text description of the piece (category, material, stones, style)
    /// - Render quality parameters
    /// - Structured JSON configuration (for better AI understanding)
    /// </remarks>
    [Obsolete("Use BuildStructuredPromptAsync instead for better AI understanding. This method will be removed in future versions.")]
    Task<string> BuildPreviewPromptAsync(AiConfigDto aiConfig, CancellationToken ct = default);

    /// <summary>
    /// [Obsolete] Builds a text-based prompt for AI preview generation based on jewelry configuration.
    /// </summary>
    /// <param name="configuration">Jewelry configuration with loaded related entities.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Generated prompt for the AI model.</returns>
    /// <remarks>
    /// This method is obsolete. Use BuildStructuredPromptAsync with AiConfigDto instead.
    /// </remarks>
    [Obsolete("Use BuildStructuredPromptAsync(AiConfigDto aiConfig) instead. This method will be removed in future versions.")]
    Task<string> BuildPreviewPromptAsync(JewelryConfiguration configuration, CancellationToken ct = default);
}
