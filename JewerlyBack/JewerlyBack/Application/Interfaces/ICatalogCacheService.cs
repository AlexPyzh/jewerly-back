using JewerlyBack.Models;

namespace JewerlyBack.Application.Interfaces;

/// <summary>
/// Caching service for catalog data (categories, materials, stone types)
/// </summary>
public interface ICatalogCacheService
{
    /// <summary>
    /// Get all active categories from cache
    /// </summary>
    Task<IReadOnlyList<JewelryCategory>> GetCategoriesAsync(CancellationToken ct = default);

    /// <summary>
    /// Get all active materials from cache
    /// </summary>
    Task<IReadOnlyList<Material>> GetMaterialsAsync(CancellationToken ct = default);

    /// <summary>
    /// Get all active stone types from cache
    /// </summary>
    Task<IReadOnlyList<StoneType>> GetStoneTypesAsync(CancellationToken ct = default);

    /// <summary>
    /// Get a category by ID from cache
    /// </summary>
    Task<JewelryCategory?> GetCategoryByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Get a material by ID from cache
    /// </summary>
    Task<Material?> GetMaterialByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Get a stone type by ID from cache
    /// </summary>
    Task<StoneType?> GetStoneTypeByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Invalidate all cached catalog data
    /// </summary>
    void InvalidateAll();

    /// <summary>
    /// Invalidate cached categories
    /// </summary>
    void InvalidateCategories();

    /// <summary>
    /// Invalidate cached materials
    /// </summary>
    void InvalidateMaterials();

    /// <summary>
    /// Invalidate cached stone types
    /// </summary>
    void InvalidateStoneTypes();
}
