using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Dto;

/// <summary>
/// DTO для гравировки в конфигурации украшения
/// </summary>
public class ConfigurationEngravingDto
{
    /// <summary>
    /// ID записи о гравировке
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Текст гравировки
    /// </summary>
    [Required(ErrorMessage = "Text is required")]
    [MaxLength(500, ErrorMessage = "Text must not exceed 500 characters")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Название шрифта (опционально)
    /// </summary>
    [MaxLength(100, ErrorMessage = "FontName must not exceed 100 characters")]
    public string? FontName { get; set; }

    /// <summary>
    /// Расположение гравировки
    /// </summary>
    [Required(ErrorMessage = "Location is required")]
    [MaxLength(100, ErrorMessage = "Location must not exceed 100 characters")]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Размер в миллиметрах (опционально)
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "SizeMm must be greater than 0")]
    public decimal? SizeMm { get; set; }

    /// <summary>
    /// Является ли гравировка внутренней
    /// </summary>
    public bool IsInternal { get; set; }
}
