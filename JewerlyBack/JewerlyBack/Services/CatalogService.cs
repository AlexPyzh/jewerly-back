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
    private readonly ILogger<CatalogService> _logger;

    public CatalogService(AppDbContext context, ILogger<CatalogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<JewelryCategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all active jewelry categories");

        var categories = await _context.JewelryCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Id)
            .Select(c => new JewelryCategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description
            })
            .ToListAsync(ct);

        _logger.LogInformation("Found {Count} active categories", categories.Count);
        return categories;
    }

    public async Task<IReadOnlyList<MaterialDto>> GetMaterialsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all active materials");

        var materials = await _context.Materials
            .Where(m => m.IsActive)
            .OrderBy(m => m.Name)
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
            .ToListAsync(ct);

        _logger.LogInformation("Found {Count} active materials", materials.Count);
        return materials;
    }

    public async Task<IReadOnlyList<StoneTypeDto>> GetStoneTypesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all active stone types");

        var stoneTypes = await _context.StoneTypes
            .Where(st => st.IsActive)
            .OrderBy(st => st.Name)
            .Select(st => new StoneTypeDto
            {
                Id = st.Id,
                Code = st.Code,
                Name = st.Name,
                Color = st.Color,
                DefaultPricePerCarat = st.DefaultPricePerCarat
            })
            .ToListAsync(ct);

        _logger.LogInformation("Found {Count} active stone types", stoneTypes.Count);
        return stoneTypes;
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
