using JewerlyBack.Application.Interfaces;
using JewerlyBack.Data;
using JewerlyBack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace JewerlyBack.Services;

/// <summary>
/// Caching service for catalog data using IMemoryCache
/// </summary>
public class CatalogCacheService : ICatalogCacheService
{
    private readonly IMemoryCache _cache;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CatalogCacheService> _logger;

    private const string CategoriesCacheKey = "catalog:categories";
    private const string MaterialsCacheKey = "catalog:materials";
    private const string StoneTypesCacheKey = "catalog:stone-types";

    private static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(30);

    public CatalogCacheService(
        IMemoryCache cache,
        IServiceScopeFactory scopeFactory,
        ILogger<CatalogCacheService> logger)
    {
        _cache = cache;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<JewelryCategory>> GetCategoriesAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CategoriesCacheKey, out IReadOnlyList<JewelryCategory>? cached) && cached != null)
        {
            _logger.LogDebug("Cache hit for categories");
            return cached;
        }

        _logger.LogDebug("Cache miss for categories, fetching from database");

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var categories = await context.JewelryCategories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Id)
            .ToListAsync(ct);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(SlidingExpiration);

        _cache.Set(CategoriesCacheKey, (IReadOnlyList<JewelryCategory>)categories, cacheOptions);

        _logger.LogInformation("Cached {Count} categories", categories.Count);
        return categories;
    }

    public async Task<IReadOnlyList<Material>> GetMaterialsAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(MaterialsCacheKey, out IReadOnlyList<Material>? cached) && cached != null)
        {
            _logger.LogDebug("Cache hit for materials");
            return cached;
        }

        _logger.LogDebug("Cache miss for materials, fetching from database");

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var materials = await context.Materials
            .AsNoTracking()
            .Where(m => m.IsActive)
            .OrderBy(m => m.Name)
            .ToListAsync(ct);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(SlidingExpiration);

        _cache.Set(MaterialsCacheKey, (IReadOnlyList<Material>)materials, cacheOptions);

        _logger.LogInformation("Cached {Count} materials", materials.Count);
        return materials;
    }

    public async Task<IReadOnlyList<StoneType>> GetStoneTypesAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(StoneTypesCacheKey, out IReadOnlyList<StoneType>? cached) && cached != null)
        {
            _logger.LogDebug("Cache hit for stone types");
            return cached;
        }

        _logger.LogDebug("Cache miss for stone types, fetching from database");

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var stoneTypes = await context.StoneTypes
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(SlidingExpiration);

        _cache.Set(StoneTypesCacheKey, (IReadOnlyList<StoneType>)stoneTypes, cacheOptions);

        _logger.LogInformation("Cached {Count} stone types", stoneTypes.Count);
        return stoneTypes;
    }

    public async Task<JewelryCategory?> GetCategoryByIdAsync(int id, CancellationToken ct = default)
    {
        var categories = await GetCategoriesAsync(ct);
        return categories.FirstOrDefault(c => c.Id == id);
    }

    public async Task<Material?> GetMaterialByIdAsync(int id, CancellationToken ct = default)
    {
        var materials = await GetMaterialsAsync(ct);
        return materials.FirstOrDefault(m => m.Id == id);
    }

    public async Task<StoneType?> GetStoneTypeByIdAsync(int id, CancellationToken ct = default)
    {
        var stoneTypes = await GetStoneTypesAsync(ct);
        return stoneTypes.FirstOrDefault(s => s.Id == id);
    }

    public void InvalidateAll()
    {
        _logger.LogInformation("Invalidating all catalog cache");
        InvalidateCategories();
        InvalidateMaterials();
        InvalidateStoneTypes();
    }

    public void InvalidateCategories()
    {
        _logger.LogInformation("Invalidating categories cache");
        _cache.Remove(CategoriesCacheKey);
    }

    public void InvalidateMaterials()
    {
        _logger.LogInformation("Invalidating materials cache");
        _cache.Remove(MaterialsCacheKey);
    }

    public void InvalidateStoneTypes()
    {
        _logger.LogInformation("Invalidating stone types cache");
        _cache.Remove(StoneTypesCacheKey);
    }
}
