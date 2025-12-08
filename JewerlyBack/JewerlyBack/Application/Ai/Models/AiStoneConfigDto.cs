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
    public required string StoneTypeCode { get; init; }

    /// <summary>
    /// Название камня (Diamond, Ruby, Sapphire и т.д.)
    /// </summary>
    public required string StoneTypeName { get; init; }

    /// <summary>
    /// Цвет камня (если указан)
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Вес в каратах (если указан)
    /// </summary>
    public decimal? CaratWeight { get; init; }

    /// <summary>
    /// Размер в миллиметрах (если указан)
    /// </summary>
    public decimal? SizeMm { get; init; }

    /// <summary>
    /// Количество камней данного типа
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// Позиция/индекс размещения
    /// </summary>
    public int PositionIndex { get; init; }
}
