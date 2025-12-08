namespace JewerlyBack.Models;

/// <summary>
/// Задание на генерацию AI превью ювелирного изделия
/// </summary>
public class AiPreviewJob
{
    /// <summary>
    /// Уникальный идентификатор задания
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID конфигурации ювелирного изделия
    /// </summary>
    public Guid ConfigurationId { get; set; }

    /// <summary>
    /// ID пользователя (null для гостей)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// ID анонимного клиента (заполняется фронтом для гостей)
    /// </summary>
    public string? GuestClientId { get; set; }

    /// <summary>
    /// Тип превью (SingleImage или Preview360)
    /// </summary>
    public AiPreviewType Type { get; set; }

    /// <summary>
    /// Текущий статус обработки
    /// </summary>
    public AiPreviewStatus Status { get; set; }

    /// <summary>
    /// Промпт для AI модели (для логов и аудита)
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// Семантический JSON конфигурации для AI (обогащенный человеко-читаемыми данными)
    /// </summary>
    /// <remarks>
    /// Содержит AiConfigDto в JSON формате - полное семантическое описание изделия
    /// с названиями категорий, материалов, камней вместо сырых ID/кодов.
    /// Используется для отладки и понимания, какой конфиг был передан в AI.
    /// </remarks>
    public string? AiConfigJson { get; set; }

    /// <summary>
    /// Сообщение об ошибке (если статус Failed)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// URL готового изображения (для типа SingleImage)
    /// </summary>
    public string? SingleImageUrl { get; set; }

    /// <summary>
    /// JSON-массив с URL фреймов для 360-просмотра (для типа Preview360)
    /// Пример: ["https://.../frame1.png", "https://.../frame2.png", ...]
    /// </summary>
    public string? FramesJson { get; set; }

    /// <summary>
    /// Дата создания задания (UTC)
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Дата последнего обновления (UTC)
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    // Навигационные свойства
    public JewelryConfiguration Configuration { get; set; } = null!;
}
