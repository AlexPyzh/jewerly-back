using JewerlyBack.Application.Interfaces;
using JewerlyBack.Application.Models;
using JewerlyBack.Data;
using JewerlyBack.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JewerlyBack.Services;

/// <summary>
/// Реализация сервиса для работы с каталогом изделий
/// </summary>
public class CatalogService : ICatalogService
{
    private readonly AppDbContext _context;
    private readonly ICatalogCacheService _cacheService;
    private readonly ILogger<CatalogService> _logger;

    public CatalogService(
        AppDbContext context,
        ICatalogCacheService cacheService,
        ILogger<CatalogService> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<JewelryCategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching all active jewelry categories from cache");

        var categories = await _cacheService.GetCategoriesAsync(ct);

        var result = categories
            .Select(c => new JewelryCategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description
            })
            .ToList();

        _logger.LogDebug("Returning {Count} active categories", result.Count);
        return result;
    }

    public async Task<IReadOnlyList<MaterialDto>> GetMaterialsAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching all active materials from cache");

        var materials = await _cacheService.GetMaterialsAsync(ct);

        var result = materials
            .Select(m => new MaterialDto
            {
                Id = m.Id,
                Code = m.Code,
                Name = m.Name,
                MetalType = m.MetalType,
                Karat = m.Karat,
                ColorHex = m.ColorHex,
                PriceFactor = m.PriceFactor
            })
            .ToList();

        _logger.LogDebug("Returning {Count} active materials", result.Count);
        return result;
    }

    public async Task<IReadOnlyList<StoneTypeDto>> GetStoneTypesAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching all active stone types from cache");

        var stoneTypes = await _cacheService.GetStoneTypesAsync(ct);

        var result = stoneTypes
            .Select(st => new StoneTypeDto
            {
                Id = st.Id,
                Code = st.Code,
                Name = st.Name,
                Color = st.Color,
                DefaultPricePerCarat = st.DefaultPricePerCarat
            })
            .ToList();

        _logger.LogDebug("Returning {Count} active stone types", result.Count);
        return result;
    }

    public async Task<PagedResult<JewelryBaseModelDto>> GetBaseModelsByCategoryAsync(
        int categoryId,
        PaginationQuery pagination,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching active base models for category {CategoryId} (Page: {Page}, PageSize: {PageSize})",
            categoryId, pagination.Page, pagination.PageSize);

        var query = _context.JewelryBaseModels
            .Where(bm => bm.CategoryId == categoryId && bm.IsActive);

        // Получаем общее количество элементов
        var totalCount = await query.CountAsync(ct);

        // Получаем элементы текущей страницы
        var items = await query
            .OrderBy(bm => bm.Name)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(bm => new JewelryBaseModelDto
            {
                Id = bm.Id,
                CategoryId = bm.CategoryId,
                Name = bm.Name,
                Code = bm.Code,
                Description = bm.Description,
                PreviewImageUrl = bm.PreviewImageUrl,
                BasePrice = bm.BasePrice
            })
            .ToListAsync(ct);

        _logger.LogInformation("Found {TotalCount} active base models for category {CategoryId}, returned {ItemCount} items",
            totalCount, categoryId, items.Count);

        return new PagedResult<JewelryBaseModelDto>
        {
            Items = items,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<JewelryBaseModelDto?> GetBaseModelByIdAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching base model with ID {BaseModelId}", id);

        var baseModel = await _context.JewelryBaseModels
            .Where(bm => bm.Id == id && bm.IsActive)
            .Select(bm => new JewelryBaseModelDto
            {
                Id = bm.Id,
                CategoryId = bm.CategoryId,
                Name = bm.Name,
                Code = bm.Code,
                Description = bm.Description,
                PreviewImageUrl = bm.PreviewImageUrl,
                BasePrice = bm.BasePrice
            })
            .FirstOrDefaultAsync(ct);

        if (baseModel == null)
        {
            _logger.LogWarning("Base model with ID {BaseModelId} not found or is not active", id);
        }
        else
        {
            _logger.LogInformation("Successfully fetched base model {BaseModelCode}", baseModel.Code);
        }

        return baseModel;
    }
}
