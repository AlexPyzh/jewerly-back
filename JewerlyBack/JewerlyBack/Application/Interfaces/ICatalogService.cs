using JewerlyBack.Dto;

namespace JewerlyBack.Application.Interfaces;

/// <summary>
/// Сервис для работы с каталогом изделий (категории, базовые модели, материалы, типы камней)
/// </summary>
public interface ICatalogService
{
    /// <summary>
    /// Получить список всех активных категорий
    /// </summary>
    Task<IReadOnlyList<JewelryCategoryDto>> GetCategoriesAsync(CancellationToken ct = default);

    /// <summary>
    /// Получить список всех активных материалов
    /// </summary>
    Task<IReadOnlyList<MaterialDto>> GetMaterialsAsync(CancellationToken ct = default);

    /// <summary>
    /// Получить список всех активных типов камней
    /// </summary>
    Task<IReadOnlyList<StoneTypeDto>> GetStoneTypesAsync(CancellationToken ct = default);

    /// <summary>
    /// Получить список активных базовых моделей по категории
    /// </summary>
    Task<IReadOnlyList<JewelryBaseModelDto>> GetBaseModelsByCategoryAsync(int categoryId, CancellationToken ct = default);

    /// <summary>
    /// Получить детальную информацию о базовой модели по ID
    /// </summary>
    Task<JewelryBaseModelDto?> GetBaseModelByIdAsync(Guid id, CancellationToken ct = default);
}
