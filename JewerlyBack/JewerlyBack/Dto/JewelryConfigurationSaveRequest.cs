using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Dto;

/// <summary>
/// Запрос на сохранение (создание или обновление) конфигурации украшения
/// </summary>
public class JewelryConfigurationSaveRequest
{
    /// <summary>
    /// ID существующей конфигурации (опционально, если null - будет создана новая)
    /// </summary>
    public Guid? ConfigurationId { get; set; }

    /// <summary>
    /// ID базовой 3D-модели украшения
    /// </summary>
    [Required(ErrorMessage = "BaseModelId is required")]
    public Guid BaseModelId { get; set; }

    /// <summary>
    /// ID материала (золото, серебро и т.д.)
    /// </summary>
    [Required(ErrorMessage = "MaterialId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "MaterialId must be greater than 0")]
    public int MaterialId { get; set; }

    /// <summary>
    /// Название конфигурации (опционально)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Name must not exceed 500 characters")]
    public string? Name { get; set; }

    /// <summary>
    /// JSON с настройками конфигурации (опционально)
    /// </summary>
    [MaxLength(10000, ErrorMessage = "ConfigJson must not exceed 10000 characters")]
    public string? ConfigJson { get; set; }

    /// <summary>
    /// Статус конфигурации (опционально)
    /// </summary>
    [MaxLength(50, ErrorMessage = "Status must not exceed 50 characters")]
    public string? Status { get; set; }

    /// <summary>
    /// Список камней в конфигурации (опционально)
    /// </summary>
    public List<ConfigurationStoneDto>? Stones { get; set; }

    /// <summary>
    /// Список гравировок в конфигурации (опционально)
    /// </summary>
    public List<ConfigurationEngravingDto>? Engravings { get; set; }
}
