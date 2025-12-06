using JewerlyBack.Application.Interfaces;
using JewerlyBack.Application.Models;
using JewerlyBack.Data;
using JewerlyBack.Dto;
using JewerlyBack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JewerlyBack.Services;

/// <summary>
/// Реализация сервиса для работы с конфигурациями ювелирных изделий
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly IPricingService _pricingService;

    public ConfigurationService(
        AppDbContext context,
        ILogger<ConfigurationService> logger,
        IPricingService pricingService)
    {
        _context = context;
        _logger = logger;
        _pricingService = pricingService;
    }

    public async Task<PagedResult<JewelryConfigurationListItemDto>> GetUserConfigurationsAsync(
        Guid userId,
        PaginationQuery pagination,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Getting configurations for user {UserId} (Page: {Page}, PageSize: {PageSize})",
            userId, pagination.Page, pagination.PageSize);

        var query = _context.JewelryConfigurations
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Include(c => c.BaseModel)
            .Include(c => c.Material);

        // Получаем общее количество элементов
        var totalCount = await query.CountAsync(ct);

        // Получаем элементы текущей страницы
        var items = await query
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(c => new JewelryConfigurationListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Status = c.Status,
                BaseModelName = c.BaseModel.Name,
                MaterialName = c.Material.Name,
                EstimatedPrice = c.EstimatedPrice,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<JewelryConfigurationListItemDto>
        {
            Items = items,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<JewelryConfigurationDetailDto?> GetConfigurationByIdAsync(
        Guid userId,
        Guid configurationId,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Getting configuration {ConfigurationId} for user {UserId}",
            configurationId, userId);

        var configuration = await _context.JewelryConfigurations
            .AsNoTracking()
            .Where(c => c.Id == configurationId && c.UserId == userId)
            .Include(c => c.BaseModel)
            .Include(c => c.Material)
            .Include(c => c.Stones)
                .ThenInclude(s => s.StoneType)
            .Include(c => c.Engravings)
            .Include(c => c.Assets)
            .FirstOrDefaultAsync(ct);

        if (configuration == null)
        {
            _logger.LogWarning("Configuration {ConfigurationId} not found or access denied for user {UserId}",
                configurationId, userId);
            return null;
        }

        return new JewelryConfigurationDetailDto
        {
            Id = configuration.Id,
            UserId = configuration.UserId,
            BaseModelId = configuration.BaseModelId,
            MaterialId = configuration.MaterialId,
            Name = configuration.Name,
            Status = configuration.Status,
            ConfigJson = configuration.ConfigJson,
            EstimatedPrice = configuration.EstimatedPrice,
            CreatedAt = configuration.CreatedAt,
            UpdatedAt = configuration.UpdatedAt,
            BaseModel = new JewelryBaseModelDto
            {
                Id = configuration.BaseModel.Id,
                CategoryId = configuration.BaseModel.CategoryId,
                Name = configuration.BaseModel.Name,
                Code = configuration.BaseModel.Code,
                Description = configuration.BaseModel.Description,
                PreviewImageUrl = configuration.BaseModel.PreviewImageUrl,
                BasePrice = configuration.BaseModel.BasePrice
            },
            Material = new MaterialDto
            {
                Id = configuration.Material.Id,
                Code = configuration.Material.Code,
                Name = configuration.Material.Name,
                MetalType = configuration.Material.MetalType,
                Karat = configuration.Material.Karat,
                ColorHex = configuration.Material.ColorHex,
                PriceFactor = configuration.Material.PriceFactor
            },
            Stones = configuration.Stones.Select(s => new ConfigurationStoneDto
            {
                Id = s.Id,
                StoneTypeId = s.StoneTypeId,
                StoneTypeName = s.StoneType.Name,
                PositionIndex = s.PositionIndex,
                CaratWeight = s.CaratWeight,
                SizeMm = s.SizeMm,
                Count = s.Count,
                PlacementDataJson = s.PlacementDataJson
            }).ToList(),
            Engravings = configuration.Engravings.Select(e => new ConfigurationEngravingDto
            {
                Id = e.Id,
                Text = e.Text,
                FontName = e.FontName,
                Location = e.Location,
                SizeMm = e.SizeMm,
                IsInternal = e.IsInternal
            }).ToList(),
            Assets = configuration.Assets.Select(a => new UploadedAssetDto
            {
                Id = a.Id,
                FileType = a.FileType,
                Url = a.Url,
                OriginalFileName = a.OriginalFileName,
                CreatedAt = a.CreatedAt
            }).ToList()
        };
    }

    public async Task<Guid> CreateConfigurationAsync(
        Guid? userId,
        JewelryConfigurationCreateRequest request,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Creating configuration for user {UserId}, baseModelId={BaseModelId}, materialId={MaterialId}",
            userId, request.BaseModelId, request.MaterialId);

        try
        {
            // Проверка существования базовой модели
            var baseModelExists = await _context.JewelryBaseModels
                .AnyAsync(m => m.Id == request.BaseModelId, ct);

            if (!baseModelExists)
            {
                _logger.LogWarning("Base model {BaseModelId} not found in database", request.BaseModelId);
                throw new ArgumentException($"Base model {request.BaseModelId} not found");
            }

            // Проверка существования материала
            var materialExists = await _context.Materials
                .AnyAsync(m => m.Id == request.MaterialId, ct);

            if (!materialExists)
            {
                _logger.LogWarning("Material {MaterialId} not found in database", request.MaterialId);
                throw new ArgumentException($"Material {request.MaterialId} not found");
            }
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Error during validation in CreateConfigurationAsync");
            throw;
        }

        var now = DateTimeOffset.UtcNow;
        var configuration = new JewelryConfiguration
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BaseModelId = request.BaseModelId,
            MaterialId = request.MaterialId,
            Name = request.Name,
            Status = "Draft",
            ConfigJson = request.ConfigJson,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.JewelryConfigurations.Add(configuration);
        await _context.SaveChangesAsync(ct);

        // Рассчитываем цену после сохранения конфигурации
        // (на этом этапе камни ещё не добавлены, но базовая цена уже рассчитается)
        try
        {
            configuration.EstimatedPrice = await _pricingService.CalculateConfigurationPriceAsync(configuration.Id, ct);
            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to calculate price for configuration {ConfigurationId}", configuration.Id);
            // Не критично, если расчёт цены не удался - конфигурация уже создана
        }

        _logger.LogInformation("Configuration {ConfigurationId} created for user {UserId}",
            configuration.Id, userId);

        return configuration.Id;
    }

    public async Task<bool> UpdateConfigurationAsync(
        Guid? userId,
        Guid configurationId,
        JewelryConfigurationUpdateRequest request,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Updating configuration {ConfigurationId} for user {UserId}",
            configurationId, userId);

        var configuration = await _context.JewelryConfigurations
            .Where(c => c.Id == configurationId && c.UserId == userId)
            .Include(c => c.Stones)
            .Include(c => c.Engravings)
            .FirstOrDefaultAsync(ct);

        if (configuration == null)
        {
            _logger.LogWarning("Configuration {ConfigurationId} not found or access denied for user {UserId}",
                configurationId, userId);
            return false;
        }

        // Обновление базовых полей
        if (request.MaterialId.HasValue)
        {
            var materialExists = await _context.Materials
                .AnyAsync(m => m.Id == request.MaterialId.Value, ct);

            if (!materialExists)
            {
                throw new ArgumentException($"Material {request.MaterialId.Value} not found");
            }

            configuration.MaterialId = request.MaterialId.Value;
        }

        if (request.Name != null)
        {
            configuration.Name = request.Name;
        }

        if (request.ConfigJson != null)
        {
            configuration.ConfigJson = request.ConfigJson;
        }

        if (request.Status != null)
        {
            configuration.Status = request.Status;
        }

        // Обновление камней
        if (request.Stones != null)
        {
            // Удаляем старые камни
            _context.JewelryConfigurationStones.RemoveRange(configuration.Stones);

            // Добавляем новые камни
            foreach (var stoneDto in request.Stones)
            {
                var stone = new JewelryConfigurationStone
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = configurationId,
                    StoneTypeId = stoneDto.StoneTypeId,
                    PositionIndex = stoneDto.PositionIndex,
                    CaratWeight = stoneDto.CaratWeight,
                    SizeMm = stoneDto.SizeMm,
                    Count = stoneDto.Count,
                    PlacementDataJson = stoneDto.PlacementDataJson
                };
                configuration.Stones.Add(stone);
            }
        }

        // Обновление гравировок
        if (request.Engravings != null)
        {
            // Удаляем старые гравировки
            _context.JewelryConfigurationEngravings.RemoveRange(configuration.Engravings);

            // Добавляем новые гравировки
            foreach (var engravingDto in request.Engravings)
            {
                var engraving = new JewelryConfigurationEngraving
                {
                    Id = Guid.NewGuid(),
                    ConfigurationId = configurationId,
                    Text = engravingDto.Text,
                    FontName = engravingDto.FontName,
                    Location = engravingDto.Location,
                    SizeMm = engravingDto.SizeMm,
                    IsInternal = engravingDto.IsInternal
                };
                configuration.Engravings.Add(engraving);
            }
        }

        configuration.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(ct);

        // Пересчитываем цену после всех изменений (материал, камни, гравировки)
        // Проверяем, были ли изменены параметры, влияющие на цену
        bool shouldRecalculatePrice = request.MaterialId.HasValue || request.Stones != null;

        if (shouldRecalculatePrice)
        {
            try
            {
                configuration.EstimatedPrice = await _pricingService.CalculateConfigurationPriceAsync(configurationId, ct);
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to recalculate price for configuration {ConfigurationId}", configurationId);
                // Не критично, если пересчёт цены не удался - конфигурация уже обновлена
            }
        }

        _logger.LogInformation("Configuration {ConfigurationId} updated for user {UserId}",
            configurationId, userId);

        return true;
    }

    public async Task<bool> DeleteConfigurationAsync(
        Guid userId,
        Guid configurationId,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting configuration {ConfigurationId} for user {UserId}",
            configurationId, userId);

        var configuration = await _context.JewelryConfigurations
            .Where(c => c.Id == configurationId && c.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (configuration == null)
        {
            _logger.LogWarning("Configuration {ConfigurationId} not found or access denied for user {UserId}",
                configurationId, userId);
            return false;
        }

        _context.JewelryConfigurations.Remove(configuration);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Configuration {ConfigurationId} deleted for user {UserId}",
            configurationId, userId);

        return true;
    }

    public async Task<IReadOnlyList<JewelryConfigurationSummaryDto>> GetRecentForUserAsync(
        Guid userId,
        int take,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Getting recent {Count} configurations for user {UserId}", take, userId);

        // Нормализуем количество элементов (от 1 до 20)
        var normalizedTake = Math.Clamp(take, 1, 20);

        var items = await _context.JewelryConfigurations
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Include(c => c.BaseModel)
                .ThenInclude(bm => bm.Category)
            .Include(c => c.Material)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .Take(normalizedTake)
            .Select(c => new JewelryConfigurationSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                CategoryName = c.BaseModel.Category.Name,
                MaterialName = c.Material.Name,
                EstimatedPrice = c.EstimatedPrice,
                UpdatedAt = c.UpdatedAt,
                ThumbnailUrl = c.BaseModel.PreviewImageUrl
            })
            .ToListAsync(ct);

        _logger.LogInformation("Found {Count} recent configurations for user {UserId}", items.Count, userId);

        return items;
    }
}
