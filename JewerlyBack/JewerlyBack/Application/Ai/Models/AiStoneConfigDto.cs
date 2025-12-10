using System.Text.Json.Serialization;

namespace JewerlyBack.Application.Ai.Models;

/// <summary>
/// Семантическая модель конфигурации камня для AI.
/// Используется для передачи понятной информации о камнях в AI-генератор.
/// </summary>
public sealed class AiStoneConfigDto
{
    /// <summary>
    /// Код типа камня (diamond, ruby, sapphire и т.д.)
    /// </summary>
    [JsonPropertyName("stoneTypeCode")]
    public required string StoneTypeCode { get; init; }

    /// <summary>
    /// Название камня (Diamond, Ruby, Sapphire и т.д.)
    /// </summary>
    [JsonPropertyName("stoneTypeName")]
    public required string StoneTypeName { get; init; }

    /// <summary>
    /// Цвет камня (если указан)
    /// </summary>
    [JsonPropertyName("color")]
    public string? Color { get; init; }

    /// <summary>
    /// Вес в каратах (если указан)
    /// </summary>
    [JsonPropertyName("caratWeight")]
    public decimal? CaratWeight { get; init; }

    /// <summary>
    /// Размер в миллиметрах (если указан)
    /// </summary>
    [JsonPropertyName("sizeMm")]
    public decimal? SizeMm { get; init; }

    /// <summary>
    /// Количество камней данного типа
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; init; }

    /// <summary>
    /// Позиция/индекс размещения
    /// </summary>
    [JsonPropertyName("positionIndex")]
    public int PositionIndex { get; init; }
}
