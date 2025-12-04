using JewerlyBack.Models;

namespace JewerlyBack.Dto;

/// <summary>
/// DTO для отображения статуса AI превью задания
/// </summary>
public class AiPreviewJobDto
{
    /// <summary>
    /// ID задания
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID конфигурации
    /// </summary>
    public Guid ConfigurationId { get; set; }

    /// <summary>
    /// Тип превью
    /// </summary>
    public AiPreviewType Type { get; set; }

    /// <summary>
    /// Текущий статус обработки
    /// </summary>
    public AiPreviewStatus Status { get; set; }

    /// <summary>
    /// URL готового изображения (только для SingleImage)
    /// </summary>
    public string? SingleImageUrl { get; set; }

    /// <summary>
    /// Список URL фреймов для 360-просмотра (только для Preview360)
    /// </summary>
    public IReadOnlyList<string>? FrameUrls { get; set; }

    /// <summary>
    /// Сообщение об ошибке (если статус Failed)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Дата создания задания (UTC)
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Дата последнего обновления (UTC)
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
