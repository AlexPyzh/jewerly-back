using JewerlyBack.Models;

namespace JewerlyBack.Application.Ai;

/// <summary>
/// Сервис для построения AI-промптов на основе конфигурации ювелирного изделия.
/// </summary>
public interface IAiPromptBuilder
{
    /// <summary>
    /// Строит текстовый промпт для генерации AI-превью на основе конфигурации изделия.
    /// </summary>
    /// <param name="configuration">Конфигурация ювелирного изделия с загруженными связанными сущностями.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Сформированный промпт для AI модели.</returns>
    /// <remarks>
    /// Промпт должен включать:
    /// - Название категории (Ring, Earrings, Pendant)
    /// - Материал (14K Yellow Gold, Platinum и т.д.)
    /// - Информацию о камнях (тип, количество)
    /// - Стиль и качественные характеристики для фотореалистичного рендера
    /// </remarks>
    Task<string> BuildPreviewPromptAsync(JewelryConfiguration configuration, CancellationToken ct = default);
}
