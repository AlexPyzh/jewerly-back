using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Dto;

/// <summary>
/// Запрос на загрузку файла (ассета)
/// </summary>
public class AssetUploadRequest
{
    /// <summary>
    /// Файл для загрузки
    /// </summary>
    [Required(ErrorMessage = "File is required")]
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Тип файла: image, pattern, texture, reference
    /// </summary>
    [Required(ErrorMessage = "FileType is required")]
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Опциональная привязка к конфигурации украшения
    /// </summary>
    public Guid? ConfigurationId { get; set; }
}
