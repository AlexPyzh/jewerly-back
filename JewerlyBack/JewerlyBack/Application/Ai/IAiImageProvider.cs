namespace JewerlyBack.Application.Ai;

/// <summary>
/// Сервис для генерации AI-превью изображений ювелирных изделий.
/// Использует OpenAI API для создания изображений на основе промптов.
/// </summary>
public interface IAiImageProvider
{
    /// <summary>
    /// Генерирует одиночное превью изображение на основе промпта.
    /// </summary>
    /// <param name="prompt">Промпт для генерации изображения (описание ювелирного изделия).</param>
    /// <param name="configurationId">ID конфигурации ювелирного изделия (для формирования пути в S3).</param>
    /// <param name="jobId">ID задания на генерацию превью (для формирования пути в S3).</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>URL сгенерированного изображения в S3 хранилище.</returns>
    /// <remarks>
    /// Используется для создания 2D превью кастомного ювелирного изделия.
    /// Метод загружает сгенерированное изображение в S3 и возвращает публичный URL.
    /// Изображение сохраняется по пути: ai-previews/{configurationId}/{jobId}/preview.png
    /// </remarks>
    Task<string> GenerateSinglePreviewAsync(string prompt, Guid configurationId, Guid jobId, CancellationToken ct = default);

    /// <summary>
    /// Генерирует набор изображений для 360-градусного превью.
    /// </summary>
    /// <param name="prompt">Базовый промпт для генерации изображений (описание ювелирного изделия).</param>
    /// <param name="configurationId">ID конфигурации ювелирного изделия (для формирования пути в S3).</param>
    /// <param name="jobId">ID задания на генерацию превью (для формирования пути в S3).</param>
    /// <param name="frameCount">Количество кадров для 360-обзора (по умолчанию 12).</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Список URL сгенерированных изображений в S3 хранилище (кадры для 360-обзора).</returns>
    /// <remarks>
    /// Используется для создания 360-градусного превью кастомного ювелирного изделия.
    /// Метод генерирует frameCount изображений с разных ракурсов (углов обзора),
    /// загружает их в S3 и возвращает список публичных URL.
    /// Изображения сохраняются по пути: ai-previews/{configurationId}/{jobId}/frames/frame_{i:D2}.png
    /// </remarks>
    Task<IReadOnlyList<string>> Generate360PreviewAsync(
        string prompt,
        Guid configurationId,
        Guid jobId,
        int frameCount = 12,
        CancellationToken ct = default);
}
