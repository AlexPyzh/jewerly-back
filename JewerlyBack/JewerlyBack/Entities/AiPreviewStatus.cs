namespace JewerlyBack.Models;

/// <summary>
/// Статус обработки AI превью
/// </summary>
public enum AiPreviewStatus
{
    /// <summary>
    /// Ожидает обработки
    /// </summary>
    Pending = 0,

    /// <summary>
    /// В процессе генерации
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Успешно завершено
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Ошибка при генерации
    /// </summary>
    Failed = 3
}
