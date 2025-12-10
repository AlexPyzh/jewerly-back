using JewerlyBack.Application.Ai.Models;

namespace JewerlyBack.Application.Ai;

/// <summary>
/// Service for building semantic context from AI configuration.
/// Transforms structured configuration data into narrative descriptions suitable for AI prompts.
/// </summary>
public interface ISemanticContextBuilder
{
    /// <summary>
    /// Builds a semantic context from the provided AI configuration.
    /// Extracts and combines all available AI descriptions into a structured semantic representation.
    /// </summary>
    /// <param name="aiConfig">The AI configuration containing jewelry details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A semantic context with all available descriptions populated</returns>
    /// <remarks>
    /// The builder gracefully handles missing descriptions - if a field is not available,
    /// it will be null in the resulting context. This ensures forward and backward compatibility.
    /// </remarks>
    Task<SemanticContext> BuildSemanticContextAsync(
        AiConfigDto aiConfig,
        CancellationToken ct = default);
}
