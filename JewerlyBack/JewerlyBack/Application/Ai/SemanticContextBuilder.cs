using System.Text;
using JewerlyBack.Application.Ai.Models;

namespace JewerlyBack.Application.Ai;

/// <summary>
/// Implementation of semantic context builder.
/// Responsible for extracting and formatting AI descriptions from configuration data.
/// </summary>
public sealed class SemanticContextBuilder : ISemanticContextBuilder
{
    private readonly ILogger<SemanticContextBuilder> _logger;

    public SemanticContextBuilder(ILogger<SemanticContextBuilder> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Builds semantic context from AI configuration.
    /// Populates all available description fields, gracefully handling missing data.
    /// </summary>
    public Task<SemanticContext> BuildSemanticContextAsync(
        AiConfigDto aiConfig,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(aiConfig);

        var context = new SemanticContext
        {
            // Category description from database
            CategoryDescription = aiConfig.CategoryAiDescription,

            // Base model description from database
            BaseModelDescription = aiConfig.BaseModelAiDescription,

            // Material description - future implementation
            // Will be populated when Material entity gets AiDescription field
            MaterialDescription = BuildMaterialDescription(aiConfig),

            // Stones description - future implementation
            // Will be populated when StoneType entity gets AiDescription field
            StonesDescription = BuildStonesDescription(aiConfig),

            // Engraving description - built from user's personalized text
            EngravingDescription = BuildEngravingDescription(aiConfig),

            // Additional context for extensibility
            AdditionalContext = BuildAdditionalContext(aiConfig)
        };

        _logger.LogDebug(
            "Built semantic context for configuration {ConfigurationId}. " +
            "Category: {HasCategory}, BaseModel: {HasBaseModel}, Material: {HasMaterial}, Stones: {HasStones}, Engraving: {HasEngraving}",
            aiConfig.ConfigurationId,
            !string.IsNullOrWhiteSpace(context.CategoryDescription),
            !string.IsNullOrWhiteSpace(context.BaseModelDescription),
            !string.IsNullOrWhiteSpace(context.MaterialDescription),
            !string.IsNullOrWhiteSpace(context.StonesDescription),
            !string.IsNullOrWhiteSpace(context.EngravingDescription));

        return Task.FromResult(context);
    }

    /// <summary>
    /// Builds a descriptive text for the material.
    /// Currently generates a simple description from available data.
    /// In the future, this can use a dedicated MaterialAiDescription field.
    /// </summary>
    private string? BuildMaterialDescription(AiConfigDto aiConfig)
    {
        if (string.IsNullOrWhiteSpace(aiConfig.MaterialName))
        {
            return null;
        }

        var description = new StringBuilder();

        // Basic material identification
        description.Append($"Made from {aiConfig.MaterialName}");

        // Add metal type context if available
        if (!string.IsNullOrWhiteSpace(aiConfig.MetalType))
        {
            var metalType = aiConfig.MetalType.ToLowerInvariant();

            // Add characteristic descriptions based on metal type
            description.Append(metalType switch
            {
                "gold" when aiConfig.Karat.HasValue =>
                    $", a precious metal with {aiConfig.Karat}-karat purity",
                "gold" =>
                    ", a precious yellow metal",
                "platinum" =>
                    ", a rare and durable precious metal with a silvery-white appearance",
                "silver" =>
                    ", a lustrous white precious metal",
                "titanium" =>
                    ", a strong and lightweight metal with a dark gray finish",
                _ => ""
            });
        }

        description.Append('.');

        return description.ToString();
    }

    /// <summary>
    /// Builds a descriptive text for the stones configuration.
    /// Currently generates from stone data in the config.
    /// In the future, this can incorporate StoneTypeAiDescription fields.
    /// </summary>
    private string? BuildStonesDescription(AiConfigDto aiConfig)
    {
        if (aiConfig.Stones == null || !aiConfig.Stones.Any())
        {
            return null;
        }

        var description = new StringBuilder();

        // Group stones by type for clearer description
        var stoneGroups = aiConfig.Stones
            .GroupBy(s => new { s.StoneTypeCode, s.StoneTypeName, s.Color })
            .Select(g => new
            {
                g.Key.StoneTypeName,
                g.Key.Color,
                TotalCount = g.Sum(s => s.Count),
                TotalCarats = g.Sum(s => s.CaratWeight ?? 0),
                Stones = g.ToList()
            })
            .OrderByDescending(g => g.TotalCarats)
            .ToList();

        description.Append("Set with ");

        var stoneDescriptions = new List<string>();

        foreach (var group in stoneGroups)
        {
            var stoneDesc = new StringBuilder();

            // Count
            if (group.TotalCount > 1)
            {
                stoneDesc.Append($"{group.TotalCount} ");
            }
            else
            {
                stoneDesc.Append("a ");
            }

            // Color
            if (!string.IsNullOrWhiteSpace(group.Color))
            {
                stoneDesc.Append($"{group.Color.ToLowerInvariant()} ");
            }

            // Stone type
            var stoneName = group.StoneTypeName.ToLowerInvariant();
            stoneDesc.Append(group.TotalCount > 1 ? $"{stoneName}s" : stoneName);

            // Carat weight if significant
            if (group.TotalCarats > 0)
            {
                stoneDesc.Append($" ({group.TotalCarats:F2} carats total)");
            }

            stoneDescriptions.Add(stoneDesc.ToString());
        }

        // Combine stone descriptions
        if (stoneDescriptions.Count == 1)
        {
            description.Append(stoneDescriptions[0]);
        }
        else if (stoneDescriptions.Count == 2)
        {
            description.Append($"{stoneDescriptions[0]} and {stoneDescriptions[1]}");
        }
        else
        {
            for (int i = 0; i < stoneDescriptions.Count; i++)
            {
                if (i == stoneDescriptions.Count - 1)
                {
                    description.Append($"and {stoneDescriptions[i]}");
                }
                else
                {
                    description.Append($"{stoneDescriptions[i]}, ");
                }
            }
        }

        description.Append('.');

        return description.ToString();
    }

    /// <summary>
    /// Builds a descriptive text for engraving.
    /// Formats the user's personalized message for AI prompt.
    /// </summary>
    private string? BuildEngravingDescription(AiConfigDto aiConfig)
    {
        if (string.IsNullOrWhiteSpace(aiConfig.EngravingText))
        {
            return null;
        }

        // Sanitize the engraving text for AI prompt
        var sanitizedText = SanitizeEngravingText(aiConfig.EngravingText.Trim());

        if (string.IsNullOrEmpty(sanitizedText))
        {
            return null;
        }

        return $"Personalized with engraved text that says '{sanitizedText}'.";
    }

    /// <summary>
    /// Sanitizes engraving text for inclusion in AI prompt.
    /// Removes or escapes characters that might confuse the AI model.
    /// </summary>
    private static string SanitizeEngravingText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        // Remove potential prompt injection characters and control characters
        var sanitized = text
            .Replace("\"", "'")  // Replace double quotes with single
            .Replace("\\", "")   // Remove backslashes
            .Replace("\n", " ")  // Replace newlines with spaces
            .Replace("\r", "")   // Remove carriage returns
            .Replace("\t", " "); // Replace tabs with spaces

        // Limit length to prevent overly long engravings from dominating the prompt
        const int maxLength = 50;
        if (sanitized.Length > maxLength)
        {
            sanitized = sanitized[..maxLength];
        }

        return sanitized.Trim();
    }

    /// <summary>
    /// Builds additional context information that doesn't fit into standard categories.
    /// This is for future extensibility.
    /// </summary>
    private Dictionary<string, string>? BuildAdditionalContext(AiConfigDto aiConfig)
    {
        // Currently no additional context needed
        // In the future, this could include:
        // - Size/dimension descriptions
        // - Style tags
        // - Finish details
        // - Special features
        return null;
    }
}
