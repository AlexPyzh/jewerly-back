using JewerlyBack.Models;

namespace JewerlyBack.Dto;

/// <summary>
/// Запрос на создание задания AI превью
/// </summary>
public class CreateAiPreviewRequest
{
    /// <summary>
    /// ID конфигурации ювелирного изделия
    /// </summary>
    public required Guid ConfigurationId { get; set; }

    /// <summary>
    /// Тип превью (SingleImage = 0 или Preview360 = 1)
    /// </summary>
    public required AiPreviewType Type { get; set; }
}
