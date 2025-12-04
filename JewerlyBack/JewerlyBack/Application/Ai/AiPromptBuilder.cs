using System.Text;
using JewerlyBack.Models;

namespace JewerlyBack.Application.Ai;

/// <summary>
/// Реализация сервиса для построения AI-промптов на основе конфигурации ювелирного изделия.
/// </summary>
public sealed class AiPromptBuilder : IAiPromptBuilder
{
    private readonly ILogger<AiPromptBuilder> _logger;

    public AiPromptBuilder(ILogger<AiPromptBuilder> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Строит промпт для генерации AI-превью на основе конфигурации изделия.
    /// </summary>
    public Task<string> BuildPreviewPromptAsync(JewelryConfiguration configuration, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var prompt = new StringBuilder();

        // Базовое описание: стиль и качество
        prompt.Append("Ultra high-quality studio render of ");

        // Материал
        if (configuration.Material != null)
        {
            prompt.Append($"{configuration.Material.Name} ");
        }

        // Категория (тип изделия)
        if (configuration.BaseModel?.Category != null)
        {
            var categoryName = configuration.BaseModel.Category.Name.ToLowerInvariant();
            prompt.Append($"{categoryName}");
        }
        else
        {
            prompt.Append("jewelry piece");
        }

        // Камни (если есть)
        if (configuration.Stones?.Any() == true)
        {
            var stonesDescription = BuildStonesDescription(configuration.Stones);
            if (!string.IsNullOrEmpty(stonesDescription))
            {
                prompt.Append($" with {stonesDescription}");
            }
        }

        // Финальные параметры качества
        prompt.Append(", minimalistic luxury jewelry, soft shadows, white background, ");
        prompt.Append("professional jewelry product photography, 8k, extremely detailed");

        var result = prompt.ToString();

        _logger.LogInformation(
            "Built AI prompt for configuration {ConfigurationId}: {Prompt}",
            configuration.Id, result);

        return Task.FromResult(result);
    }

    /// <summary>
    /// Строит описание камней для промпта.
    /// </summary>
    private string BuildStonesDescription(ICollection<JewelryConfigurationStone> stones)
    {
        if (stones == null || !stones.Any())
        {
            return string.Empty;
        }

        // Группируем камни по типу
        var stoneGroups = stones
            .Where(s => s.StoneType != null)
            .GroupBy(s => s.StoneType.Name)
            .Select(g => new
            {
                StoneName = g.Key,
                TotalCount = g.Sum(s => s.Count)
            })
            .ToList();

        if (!stoneGroups.Any())
        {
            return string.Empty;
        }

        var descriptions = stoneGroups.Select(g =>
        {
            if (g.TotalCount > 1)
            {
                return $"{g.TotalCount} {g.StoneName} gemstones";
            }
            else
            {
                return $"{g.StoneName} gemstone";
            }
        });

        return string.Join(" and ", descriptions);
    }
}
