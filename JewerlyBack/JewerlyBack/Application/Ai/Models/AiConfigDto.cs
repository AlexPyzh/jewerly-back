using System.Text.Json.Serialization;

namespace JewerlyBack.Application.Ai.Models;

/// <summary>
/// Семантическая модель конфигурации ювелирного изделия для AI.
/// Содержит человеко-читаемое описание изделия вместо сырых ID/кодов.
/// Используется для генерации промптов и передачи структурированной информации в AI-модель.
/// </summary>
public sealed class AiConfigDto
{
    /// <summary>
    /// ID конфигурации (для reference)
    /// </summary>
    [JsonPropertyName("configurationId")]
    public Guid ConfigurationId { get; init; }

    /// <summary>
    /// Название конфигурации (если задано пользователем)
    /// </summary>
    [JsonPropertyName("configurationName")]
    public string? ConfigurationName { get; init; }

    /// <summary>
    /// Код категории (ring, earrings, pendant, bracelet и т.д.)
    /// </summary>
    [JsonPropertyName("categoryCode")]
    public required string CategoryCode { get; init; }

    /// <summary>
    /// Название категории (Ring, Earrings, Pendant и т.д.)
    /// </summary>
    [JsonPropertyName("categoryName")]
    public required string CategoryName { get; init; }

    /// <summary>
    /// Описание категории (если есть)
    /// </summary>
    [JsonPropertyName("categoryDescription")]
    public string? CategoryDescription { get; init; }

    /// <summary>
    /// AI-описание категории (семантическое описание для AI промптов)
    /// </summary>
    [JsonPropertyName("categoryAiDescription")]
    public string? CategoryAiDescription { get; init; }

    /// <summary>
    /// ID базовой модели (для reference)
    /// </summary>
    [JsonPropertyName("baseModelId")]
    public Guid BaseModelId { get; init; }

    /// <summary>
    /// Код базовой модели (slim_band, halo_ring, stud_earrings и т.д.)
    /// </summary>
    [JsonPropertyName("baseModelCode")]
    public required string BaseModelCode { get; init; }

    /// <summary>
    /// Название базовой модели (Slim Band, Halo Ring, Stud Earrings и т.д.)
    /// </summary>
    [JsonPropertyName("baseModelName")]
    public required string BaseModelName { get; init; }

    /// <summary>
    /// Описание базовой модели
    /// </summary>
    [JsonPropertyName("baseModelDescription")]
    public string? BaseModelDescription { get; init; }

    /// <summary>
    /// AI-описание базовой модели (семантическое описание геометрии и стиля для AI промптов)
    /// </summary>
    [JsonPropertyName("baseModelAiDescription")]
    public string? BaseModelAiDescription { get; init; }

    /// <summary>
    /// Метаданные базовой модели (профиль, ширина, толщина и т.п. из MetadataJson)
    /// </summary>
    [JsonPropertyName("baseModelMetadata")]
    public Dictionary<string, object>? BaseModelMetadata { get; init; }

    /// <summary>
    /// Код материала (gold_14k_yellow, platinum_950 и т.д.)
    /// </summary>
    [JsonPropertyName("materialCode")]
    public required string MaterialCode { get; init; }

    /// <summary>
    /// Полное название материала (14K Yellow Gold, Platinum 950 и т.д.)
    /// </summary>
    [JsonPropertyName("materialName")]
    public required string MaterialName { get; init; }

    /// <summary>
    /// Тип металла (gold, platinum, silver и т.д.)
    /// </summary>
    [JsonPropertyName("metalType")]
    public required string MetalType { get; init; }

    /// <summary>
    /// Проба (14, 18, 24 для золота; null для платины)
    /// </summary>
    [JsonPropertyName("karat")]
    public int? Karat { get; init; }

    /// <summary>
    /// Цвет материала в HEX формате (для визуализации)
    /// </summary>
    [JsonPropertyName("materialColorHex")]
    public string? MaterialColorHex { get; init; }

    /// <summary>
    /// Список камней в конфигурации (если есть)
    /// </summary>
    [JsonPropertyName("stones")]
    public IReadOnlyList<AiStoneConfigDto>? Stones { get; init; }

    /// <summary>
    /// Дополнительные параметры конфигурации (для будущих расширений)
    /// </summary>
    [JsonPropertyName("extra")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Extra { get; init; }
}
