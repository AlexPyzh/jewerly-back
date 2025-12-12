using System.Text;
using JewerlyBack.Application.Ai.Models;
using JewerlyBack.Models;

namespace JewerlyBack.Application.Ai;

/// <summary>
/// Implementation of AI prompt builder that generates concise, natural language prompts
/// optimized for AI image generation. Outputs a single flowing paragraph without JSON or sections.
/// </summary>
public sealed class AiPromptBuilder : IAiPromptBuilder
{
    private readonly ILogger<AiPromptBuilder> _logger;

    public AiPromptBuilder(ILogger<AiPromptBuilder> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Builds a concise, AI-optimized preview prompt from semantic configuration.
    /// Returns a single paragraph of 3-5 sentences describing the jewelry piece.
    /// </summary>
    public Task<string> BuildPreviewPromptAsync(AiConfigDto aiConfig, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(aiConfig);

        var hasStones = aiConfig.Stones?.Any() == true;
        var hasEngraving = !string.IsNullOrWhiteSpace(aiConfig.EngravingText);

        var prompt = new StringBuilder();

        // Sentence 1: Main instruction with key details
        prompt.Append(BuildMainSentence(aiConfig, hasStones, hasEngraving));
        prompt.Append(' ');

        // Sentence 2: Design details
        prompt.Append(BuildDesignSentence(aiConfig));
        prompt.Append(' ');

        // Sentence 3: Material and craftsmanship
        prompt.Append(BuildMaterialSentence(aiConfig));
        prompt.Append(' ');

        // Sentence 4: Embellishments (conditional - only if stones or engraving exist)
        if (hasStones || hasEngraving)
        {
            prompt.Append(BuildEmbellishmentsSentence(aiConfig, hasStones, hasEngraving));
            prompt.Append(' ');
        }

        // Sentence 5: Photography and quality
        prompt.Append(BuildPhotographySentence());

        var result = prompt.ToString().Trim();

        // Note: Prompt logging is intentionally NOT done here.
        // The full prompt is logged ONLY in AiPreviewBackgroundService when generation is triggered.
        // This ensures prompt is logged once per generation request, not on build.

        return Task.FromResult(result);
    }

    /// <summary>
    /// Sentence 1: Main instruction with jewelry type, material, design, and optional embellishments.
    /// </summary>
    private string BuildMainSentence(AiConfigDto aiConfig, bool hasStones, bool hasEngraving)
    {
        var sb = new StringBuilder();

        // Quality prefix
        sb.Append("Ultra high-quality studio render of ");

        // Jewelry type with article
        var jewelryType = ToSingularForm(aiConfig.CategoryName);
        var article = GetArticle(jewelryType);
        sb.Append($"{article} {jewelryType} ");

        // Material
        sb.Append($"in {aiConfig.MaterialName} ");

        // Design style
        var designName = aiConfig.BaseModelName.ToLowerInvariant();
        sb.Append($"featuring {designName} design");

        // Stones (brief mention)
        if (hasStones && aiConfig.Stones != null)
        {
            var stonePhrase = BuildBriefStonesPhrase(aiConfig.Stones);
            if (!string.IsNullOrEmpty(stonePhrase))
            {
                sb.Append($", {stonePhrase}");
            }
        }

        // Engraving (brief mention)
        if (hasEngraving)
        {
            var sanitizedText = SanitizeEngravingText(aiConfig.EngravingText!);
            if (!string.IsNullOrEmpty(sanitizedText))
            {
                sb.Append($", personalized with engraved text '{sanitizedText}'");
            }
        }

        // Photography style keywords
        sb.Append(", minimalistic luxury jewelry photography with soft shadows and white background.");

        return sb.ToString();
    }

    /// <summary>
    /// Sentence 2: Design details - shape, profile, surface characteristics.
    /// </summary>
    private string BuildDesignSentence(AiConfigDto aiConfig)
    {
        var visualElements = ExtractVisualElements(aiConfig.BaseModelDescription);
        return $"The piece showcases {visualElements}.";
    }

    /// <summary>
    /// Sentence 3: Material and craftsmanship details.
    /// </summary>
    private string BuildMaterialSentence(AiConfigDto aiConfig)
    {
        var colorDesc = ColorNameResolver.GetMaterialColorDescription(aiConfig.MaterialColorHex, aiConfig.MetalType);

        // Build purity phrase
        var purityPhrase = aiConfig.Karat.HasValue ? $"{aiConfig.Karat}K " : "";

        return $"Crafted from {purityPhrase}{aiConfig.MaterialName}, creating a {colorDesc} lustrous finish with refined elegance.";
    }

    /// <summary>
    /// Sentence 4: Embellishments - stones and/or engraving.
    /// </summary>
    private string BuildEmbellishmentsSentence(AiConfigDto aiConfig, bool hasStones, bool hasEngraving)
    {
        var parts = new List<string>();

        // Stones description
        if (hasStones && aiConfig.Stones != null)
        {
            var stonesDesc = BuildDetailedStonesPhrase(aiConfig.Stones);
            if (!string.IsNullOrEmpty(stonesDesc))
            {
                parts.Add($"enhanced with {stonesDesc}");
            }
        }

        // Engraving description
        if (hasEngraving)
        {
            var sanitizedText = SanitizeEngravingText(aiConfig.EngravingText!);
            if (!string.IsNullOrEmpty(sanitizedText))
            {
                parts.Add($"personalized with engraved inscription reading '{sanitizedText}'");
            }
        }

        if (parts.Count == 0)
        {
            return string.Empty;
        }

        // Combine with proper grammar
        var combined = parts.Count == 1
            ? parts[0]
            : string.Join(" and ", parts);

        // Capitalize first letter
        return char.ToUpper(combined[0]) + combined[1..] + ".";
    }

    /// <summary>
    /// Sentence 5: Photography style and technical quality.
    /// </summary>
    private static string BuildPhotographySentence()
    {
        return "Captured in professional jewelry photography with soft studio lighting, clean white background, 8k resolution and highly detailed rendering.";
    }

    /// <summary>
    /// Builds a brief stones phrase for the main sentence.
    /// </summary>
    private string BuildBriefStonesPhrase(IReadOnlyList<AiStoneConfigDto> stones)
    {
        var stoneGroups = GroupStones(stones);
        if (!stoneGroups.Any()) return string.Empty;

        var phrases = new List<string>();

        foreach (var group in stoneGroups.Take(2)) // Limit to 2 stone types for brevity
        {
            var colorDesc = ColorNameResolver.GetStoneColorDescription(group.Color, group.StoneName);

            if (group.TotalCount == 1)
            {
                phrases.Add($"set with a {colorDesc} {group.StoneName.ToLowerInvariant()}");
            }
            else
            {
                phrases.Add($"adorned with {group.TotalCount} {colorDesc} {group.StoneName.ToLowerInvariant()}s");
            }
        }

        return string.Join(" and ", phrases);
    }

    /// <summary>
    /// Builds a detailed stones phrase for the embellishments sentence.
    /// </summary>
    private string BuildDetailedStonesPhrase(IReadOnlyList<AiStoneConfigDto> stones)
    {
        var stoneGroups = GroupStones(stones);
        if (!stoneGroups.Any()) return string.Empty;

        var descriptions = new List<string>();

        foreach (var group in stoneGroups)
        {
            var colorDesc = ColorNameResolver.GetStoneColorDescription(group.Color, group.StoneName);
            var stoneName = group.StoneName.ToLowerInvariant();

            if (group.TotalCount == 1)
            {
                descriptions.Add($"a brilliant {colorDesc} {stoneName} as the centerpiece");
            }
            else
            {
                descriptions.Add($"{group.TotalCount} {colorDesc} {stoneName}s");
            }
        }

        return string.Join(" and ", descriptions);
    }

    private List<(string StoneName, string? Color, int TotalCount, decimal TotalCarats)> GroupStones(
        IReadOnlyList<AiStoneConfigDto> stones)
    {
        return stones
            .GroupBy(s => new { s.StoneTypeName, s.Color })
            .Select(g => (
                StoneName: g.Key.StoneTypeName,
                Color: g.Key.Color,
                TotalCount: g.Sum(s => s.Count),
                TotalCarats: g.Sum(s => s.CaratWeight ?? 0)
            ))
            .OrderByDescending(g => g.TotalCarats)
            .ThenByDescending(g => g.TotalCount)
            .ToList();
    }

    private string ExtractVisualElements(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "clean lines and refined craftsmanship";
        }

        // Remove any negative phrases
        var cleaned = RemoveNegativePhrases(description);

        // Clean up and return
        var result = cleaned.Trim().TrimEnd('.');

        // If the description is too long, truncate intelligently
        if (result.Length > 150)
        {
            var lastComma = result.LastIndexOf(',', 150);
            if (lastComma > 80)
            {
                result = result[..lastComma];
            }
            else
            {
                result = result[..150].TrimEnd();
            }
        }

        return result.ToLowerInvariant();
    }

    private string RemoveNegativePhrases(string text)
    {
        var negativePatterns = new[]
        {
            ", without any stones or engravings",
            ", without stones or engravings",
            ", without any stones",
            ", without stones",
            ", without any engravings",
            ", without engravings",
            ", without engraving",
            " without any stones or engravings",
            " without stones or engravings",
            " without any stones",
            " without stones",
            " without any engravings",
            " without engravings",
            " without engraving",
            ", no stones",
            ", no engraving",
            " no stones",
            " no engraving",
            " and no additional stones",
            " and no embellishments"
        };

        var result = text;
        foreach (var pattern in negativePatterns)
        {
            result = result.Replace(pattern, "", StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    private static string GetArticle(string word)
    {
        if (string.IsNullOrEmpty(word)) return "a";

        var firstChar = char.ToLowerInvariant(word[0]);
        return firstChar is 'a' or 'e' or 'i' or 'o' or 'u' ? "an" : "a";
    }

    /// <summary>
    /// Converts a plural category name to singular form.
    /// </summary>
    private static string ToSingularForm(string categoryName)
    {
        if (string.IsNullOrEmpty(categoryName))
            return categoryName;

        var lower = categoryName.ToLowerInvariant();

        return lower switch
        {
            "rings" => "ring",
            "earrings" => "earring",
            "bracelets" => "bracelet",
            "necklaces" => "necklace",
            "pendants" => "pendant",
            "chains" => "chain",
            "brooches" => "brooch",
            "cufflinks" => "cufflink",
            "anklets" => "anklet",
            _ when lower.EndsWith('s') && !lower.EndsWith("ss") => lower[..^1],
            _ => lower
        };
    }

    private static string SanitizeEngravingText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Remove potential prompt injection characters and control characters
        var sanitized = text.Trim()
            .Replace("\"", "'")
            .Replace("\\", "")
            .Replace("\n", " ")
            .Replace("\r", "")
            .Replace("\t", " ");

        // Limit length
        const int maxLength = 50;
        if (sanitized.Length > maxLength)
        {
            sanitized = sanitized[..maxLength];
        }

        return sanitized.Trim();
    }

    // Legacy method - keeping for backwards compatibility
    [Obsolete("Use BuildPreviewPromptAsync(AiConfigDto aiConfig) instead")]
    public Task<string> BuildPreviewPromptAsync(JewelryConfiguration configuration, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var prompt = new StringBuilder();
        prompt.Append("Ultra high-quality studio render of ");

        if (configuration.Material != null)
        {
            prompt.Append($"{configuration.Material.Name} ");
        }

        if (configuration.BaseModel?.Category != null)
        {
            prompt.Append($"{configuration.BaseModel.Category.Name.ToLowerInvariant()}");
        }
        else
        {
            prompt.Append("jewelry piece");
        }

        if (configuration.Stones?.Any() == true)
        {
            var stoneGroups = configuration.Stones
                .Where(s => s.StoneType != null)
                .GroupBy(s => s.StoneType.Name)
                .Select(g => new { StoneName = g.Key, Count = g.Sum(s => s.Count) })
                .ToList();

            if (stoneGroups.Any())
            {
                var stoneDesc = string.Join(" and ", stoneGroups.Select(g =>
                    g.Count > 1 ? $"{g.Count} {g.StoneName} gemstones" : $"{g.StoneName} gemstone"));
                prompt.Append($" with {stoneDesc}");
            }
        }

        prompt.Append(", minimalistic luxury jewelry, soft shadows, white background, ");
        prompt.Append("professional jewelry product photography, 8k, extremely detailed");

        return Task.FromResult(prompt.ToString());
    }
}
