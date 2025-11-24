using JewerlyBack.Application.Interfaces;
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
            .OrderBy(c => c.Name)
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

    public async Task<IReadOnlyList<JewelryBaseModelDto>> GetBaseModelsByCategoryAsync(int categoryId, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching active base models for category {CategoryId}", categoryId);

        var baseModels = await _context.JewelryBaseModels
            .Where(bm => bm.CategoryId == categoryId && bm.IsActive)
            .OrderBy(bm => bm.Name)
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

        _logger.LogInformation("Found {Count} active base models for category {CategoryId}", baseModels.Count, categoryId);
        return baseModels;
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
