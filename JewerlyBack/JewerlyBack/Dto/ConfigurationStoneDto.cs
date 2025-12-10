using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Dto;

/// <summary>
/// DTO для камня в конфигурации украшения
/// </summary>
public class ConfigurationStoneDto
{
    /// <summary>
    /// ID записи о камне (опционально, автоматически генерируется при создании)
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// ID типа камня
    /// </summary>
    [Required(ErrorMessage = "StoneTypeId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "StoneTypeId must be greater than 0")]
    public int StoneTypeId { get; set; }

    /// <summary>
    /// Название типа камня
    /// </summary>
    [Required(ErrorMessage = "StoneTypeName is required")]
    [MaxLength(200, ErrorMessage = "StoneTypeName must not exceed 200 characters")]
    public string StoneTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Индекс позиции камня
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "PositionIndex must be greater than or equal to 0")]
    public int PositionIndex { get; set; }

    /// <summary>
    /// Вес в каратах (опционально)
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "CaratWeight must be greater than 0")]
    public decimal? CaratWeight { get; set; }

    /// <summary>
    /// Размер в миллиметрах (опционально)
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "SizeMm must be greater than 0")]
    public decimal? SizeMm { get; set; }

    /// <summary>
    /// Количество камней
    /// </summary>
    [Required(ErrorMessage = "Count is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Count must be greater than 0")]
    public int Count { get; set; }

    /// <summary>
    /// JSON с данными о размещении камня (опционально)
    /// </summary>
    public string? PlacementDataJson { get; set; }
}
