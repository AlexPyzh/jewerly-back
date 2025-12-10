using System.Text;
using System.Text.Json;
using JewerlyBack.Application.Ai.Models;
using JewerlyBack.Models;

namespace JewerlyBack.Application.Ai;

/// <summary>
/// Реализация сервиса для построения AI-промптов на основе конфигурации ювелирного изделия.
/// </summary>
public sealed class AiPromptBuilder : IAiPromptBuilder
{
    private readonly ILogger<AiPromptBuilder> _logger;
    private readonly ISemanticContextBuilder _semanticContextBuilder;
    private readonly JsonSerializerOptions _jsonOptions;

    public AiPromptBuilder(
        ILogger<AiPromptBuilder> logger,
        ISemanticContextBuilder semanticContextBuilder)
    {
        _logger = logger;
        _semanticContextBuilder = semanticContextBuilder;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Строит промпт для генерации AI-превью на основе семантической конфигурации.
    /// Использует семантический контекст для создания богатого описания изделия.
    /// </summary>
    public async Task<string> BuildPreviewPromptAsync(AiConfigDto aiConfig, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(aiConfig);

        // Строим семантический контекст из конфигурации
        var semanticContext = await _semanticContextBuilder.BuildSemanticContextAsync(aiConfig, ct);

        var prompt = new StringBuilder();

        // === ЧАСТЬ 1: Основной визуальный промпт ===
        prompt.Append("Ultra high-quality studio render of ");
        prompt.Append($"{aiConfig.MaterialName} {aiConfig.CategoryName.ToLowerInvariant()}");

        // Используем краткое описание базовой модели из старого поля для совместимости
        if (!string.IsNullOrWhiteSpace(aiConfig.BaseModelDescription))
        {
            prompt.Append($" ({aiConfig.BaseModelDescription.ToLowerInvariant()})");
        }

        // Добавляем описание камней, если есть
        if (aiConfig.Stones?.Any() == true)
        {
            var stonesDescription = BuildStonesDescriptionFromDto(aiConfig.Stones);
            if (!string.IsNullOrEmpty(stonesDescription))
            {
                prompt.Append($" with {stonesDescription}");
            }
        }

        // Финальные параметры качества
        prompt.Append(", minimalistic luxury jewelry, soft shadows, white background, ");
        prompt.Append("professional jewelry product photography, 8k, extremely detailed");

        // === ЧАСТЬ 2: Семантический контекст (для улучшенного понимания AI) ===
        if (semanticContext.HasContent)
        {
            prompt.AppendLine();
            prompt.AppendLine();
            prompt.AppendLine("--- Detailed Semantic Context ---");
            prompt.AppendLine();

            // Категория
            if (!string.IsNullOrWhiteSpace(semanticContext.CategoryDescription))
            {
                prompt.AppendLine($"Category: {semanticContext.CategoryDescription}");
                prompt.AppendLine();
            }

            // Базовая модель
            if (!string.IsNullOrWhiteSpace(semanticContext.BaseModelDescription))
            {
                prompt.AppendLine($"Design: {semanticContext.BaseModelDescription}");
                prompt.AppendLine();
            }

            // Материал
            if (!string.IsNullOrWhiteSpace(semanticContext.MaterialDescription))
            {
                prompt.AppendLine($"Material: {semanticContext.MaterialDescription}");
                prompt.AppendLine();
            }

            // Камни
            if (!string.IsNullOrWhiteSpace(semanticContext.StonesDescription))
            {
                prompt.AppendLine($"Stones: {semanticContext.StonesDescription}");
                prompt.AppendLine();
            }

            // Гравировка
            if (!string.IsNullOrWhiteSpace(semanticContext.EngravingDescription))
            {
                prompt.AppendLine($"Engraving: {semanticContext.EngravingDescription}");
                prompt.AppendLine();
            }

            // Дополнительный контекст
            if (semanticContext.AdditionalContext?.Any() == true)
            {
                foreach (var kvp in semanticContext.AdditionalContext.OrderBy(x => x.Key))
                {
                    if (!string.IsNullOrWhiteSpace(kvp.Value))
                    {
                        prompt.AppendLine($"{kvp.Key}: {kvp.Value}");
                        prompt.AppendLine();
                    }
                }
            }
        }

        // === ЧАСТЬ 3: Структурированный JSON конфигурации ===
        prompt.AppendLine("--- Structured Configuration (JSON) ---");
        var configJson = JsonSerializer.Serialize(aiConfig, _jsonOptions);
        prompt.AppendLine(configJson);

        var result = prompt.ToString();

        _logger.LogInformation(
            "Built AI prompt with semantic context for configuration {ConfigurationId}. " +
            "Semantic fields: Category={HasCategory}, BaseModel={HasBaseModel}, Material={HasMaterial}, Stones={HasStones}",
            aiConfig.ConfigurationId,
            !string.IsNullOrWhiteSpace(semanticContext.CategoryDescription),
            !string.IsNullOrWhiteSpace(semanticContext.BaseModelDescription),
            !string.IsNullOrWhiteSpace(semanticContext.MaterialDescription),
            !string.IsNullOrWhiteSpace(semanticContext.StonesDescription));

        return result;
    }

    /// <summary>
    /// [Устаревший] Строит промпт для генерации AI-превью на основе конфигурации изделия.
    /// </summary>
    [Obsolete("Use BuildPreviewPromptAsync(AiConfigDto aiConfig) instead")]
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
    /// Строит описание камней для промпта из семантического DTO.
    /// </summary>
    private string BuildStonesDescriptionFromDto(IReadOnlyList<AiStoneConfigDto> stones)
    {
        if (stones == null || !stones.Any())
        {
            return string.Empty;
        }

        // Группируем камни по типу
        var stoneGroups = stones
            .GroupBy(s => s.StoneTypeName)
            .Select(g => new
            {
                StoneName = g.Key,
                TotalCount = g.Sum(s => s.Count),
                Color = g.First().Color
            })
            .ToList();

        if (!stoneGroups.Any())
        {
            return string.Empty;
        }

        var descriptions = stoneGroups.Select(g =>
        {
            var stoneName = g.StoneName.ToLowerInvariant();
            var colorPart = !string.IsNullOrEmpty(g.Color) ? $"{g.Color.ToLowerInvariant()} " : "";

            if (g.TotalCount > 1)
            {
                return $"{g.TotalCount} {colorPart}{stoneName} gemstones";
            }
            else
            {
                return $"{colorPart}{stoneName} gemstone";
            }
        });

        return string.Join(" and ", descriptions);
    }

    /// <summary>
    /// [Устаревший] Строит описание камней для промпта.
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
