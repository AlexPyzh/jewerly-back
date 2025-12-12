using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Dto;

/// <summary>
/// Запрос на обновление конфигурации украшения
/// </summary>
public class JewelryConfigurationUpdateRequest
{
    /// <summary>
    /// ID материала (опционально)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "MaterialId must be greater than 0")]
    public int? MaterialId { get; set; }

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
    /// Simple engraving text for MVP (optional personalization message)
    /// </summary>
    [MaxLength(100, ErrorMessage = "EngravingText must not exceed 100 characters")]
    public string? EngravingText { get; set; }

    /// <summary>
    /// Список камней в конфигурации (опционально)
    /// </summary>
    public List<ConfigurationStoneDto>? Stones { get; set; }

    /// <summary>
    /// Список гравировок в конфигурации (опционально)
    /// </summary>
    public List<ConfigurationEngravingDto>? Engravings { get; set; }
}
