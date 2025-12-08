using JewerlyBack.Application.Ai.Models;
using JewerlyBack.Models;

namespace JewerlyBack.Application.Ai;

/// <summary>
/// Сервис для построения AI-промптов на основе конфигурации ювелирного изделия.
/// </summary>
public interface IAiPromptBuilder
{
    /// <summary>
    /// Строит текстовый промпт для генерации AI-превью на основе семантической конфигурации.
    /// </summary>
    /// <param name="aiConfig">Семантическая конфигурация изделия (AiConfigDto).</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Сформированный промпт для AI модели.</returns>
    /// <remarks>
    /// Промпт включает:
    /// - Текстовое описание изделия (категория, материал, камни, стиль)
    /// - Параметры качества рендера
    /// - Структурированный JSON конфигурации (для лучшего понимания AI моделью)
    /// </remarks>
    Task<string> BuildPreviewPromptAsync(AiConfigDto aiConfig, CancellationToken ct = default);

    /// <summary>
    /// [Устаревший] Строит текстовый промпт для генерации AI-превью на основе конфигурации изделия.
    /// </summary>
    /// <param name="configuration">Конфигурация ювелирного изделия с загруженными связанными сущностями.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Сформированный промпт для AI модели.</returns>
    /// <remarks>
    /// Этот метод устарел. Используйте вместо него перегрузку с AiConfigDto.
    /// </remarks>
    [Obsolete("Use BuildPreviewPromptAsync(AiConfigDto aiConfig) instead. This method will be removed in future versions.")]
    Task<string> BuildPreviewPromptAsync(JewelryConfiguration configuration, CancellationToken ct = default);
}
