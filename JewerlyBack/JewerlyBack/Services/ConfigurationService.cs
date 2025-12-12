using System.Diagnostics;
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
    private readonly IAuditService _auditService;

    public ConfigurationService(
        AppDbContext context,
        ILogger<ConfigurationService> logger,
        IPricingService pricingService,
        IAuditService auditService)
    {
        _context = context;
        _logger = logger;
        _pricingService = pricingService;
        _auditService = auditService;
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
                Status = c.Status.ToString(),
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
            Status = configuration.Status.ToString(),
            ConfigJson = configuration.ConfigJson,
            EngravingText = configuration.EngravingText,
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
            Status = ConfigurationStatus.Draft,
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

        // Audit log
        await _auditService.LogCreateAsync(
            userId,
            "JewelryConfiguration",
            configuration.Id.ToString(),
            new { request.BaseModelId, request.MaterialId, request.Name },
            ct);

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

        // Load configuration with proper null handling for anonymous users
        var configuration = await _context.JewelryConfigurations
            .Where(c => c.Id == configurationId && (c.UserId == userId || (c.UserId == null && userId == null)))
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
            if (Enum.TryParse<ConfigurationStatus>(request.Status, ignoreCase: true, out var parsedStatus))
            {
                configuration.Status = parsedStatus;
            }
            else
            {
                throw new ArgumentException($"Invalid status value: {request.Status}");
            }
        }

        // Update engraving text (allow clearing with empty string or null)
        if (request.EngravingText != null)
        {
            configuration.EngravingText = string.IsNullOrWhiteSpace(request.EngravingText)
                ? null
                : request.EngravingText.Trim();
        }

        // Обновление камней
        if (request.Stones != null)
        {
            // IMPORTANT: Detach existing tracked stones BEFORE using ExecuteDeleteAsync
            // ExecuteDeleteAsync bypasses change tracking, so tracked entities would cause
            // DbUpdateConcurrencyException when SaveChangesAsync tries to delete already-deleted rows
            foreach (var existingStone in configuration.Stones.ToList())
            {
                _context.Entry(existingStone).State = EntityState.Detached;
            }
            configuration.Stones.Clear();

            // Use ExecuteDeleteAsync for efficient bulk deletion (safe even if no rows exist)
            await _context.JewelryConfigurationStones
                .Where(s => s.ConfigurationId == configurationId)
                .ExecuteDeleteAsync(ct);

            // Add new stones - explicitly add to DbSet to ensure they are marked as Added
            var newStones = request.Stones.Select(stoneDto => new JewelryConfigurationStone
            {
                Id = Guid.NewGuid(),
                ConfigurationId = configurationId,
                StoneTypeId = stoneDto.StoneTypeId,
                PositionIndex = stoneDto.PositionIndex,
                CaratWeight = stoneDto.CaratWeight,
                SizeMm = stoneDto.SizeMm,
                Count = stoneDto.Count,
                PlacementDataJson = stoneDto.PlacementDataJson
            }).ToList();

            // Add to DbSet explicitly (ensures EntityState.Added)
            // NOTE: We don't add to navigation property to avoid duplicates
            await _context.JewelryConfigurationStones.AddRangeAsync(newStones, ct);
        }

        // Обновление гравировок
        if (request.Engravings != null)
        {
            // IMPORTANT: Detach existing tracked engravings BEFORE using ExecuteDeleteAsync
            foreach (var existingEngraving in configuration.Engravings.ToList())
            {
                _context.Entry(existingEngraving).State = EntityState.Detached;
            }
            configuration.Engravings.Clear();

            // Use ExecuteDeleteAsync for efficient bulk deletion (safe even if no rows exist)
            await _context.JewelryConfigurationEngravings
                .Where(e => e.ConfigurationId == configurationId)
                .ExecuteDeleteAsync(ct);

            // Add new engravings - explicitly add to DbSet to ensure they are marked as Added
            var newEngravings = request.Engravings.Select(engravingDto => new JewelryConfigurationEngraving
            {
                Id = Guid.NewGuid(),
                ConfigurationId = configurationId,
                Text = engravingDto.Text,
                FontName = engravingDto.FontName,
                Location = engravingDto.Location,
                SizeMm = engravingDto.SizeMm,
                IsInternal = engravingDto.IsInternal
            }).ToList();

            // Add to DbSet explicitly (ensures EntityState.Added)
            // NOTE: We don't add to navigation property to avoid duplicates
            await _context.JewelryConfigurationEngravings.AddRangeAsync(newEngravings, ct);
        }

        configuration.UpdatedAt = DateTimeOffset.UtcNow;

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex,
                "Configuration {ConfigurationId} was modified or deleted (concurrency conflict). Treating as not found for user {UserId}",
                configurationId, userId);
            return false;
        }

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
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex,
                    "Configuration {ConfigurationId} was deleted during price calculation. Ignoring price update.",
                    configurationId);
                // Configuration was deleted during price calculation - not critical
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

    public async Task<JewelryConfigurationDetailDto> SaveOrUpdateConfigurationAsync(
        Guid? userId,
        Guid? configurationId,
        Guid baseModelId,
        int materialId,
        JewelryConfigurationUpdateRequest request,
        CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "SaveOrUpdateConfiguration: userId={UserId}, configId={ConfigId}, baseModelId={BaseModelId}, materialId={MaterialId}",
            userId, configurationId, baseModelId, materialId);

        // Use execution strategy to support retrying execution strategy (NpgsqlRetryingExecutionStrategy)
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // Use explicit transaction for consistency (inside execution strategy)
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
            JewelryConfiguration? configuration = null;

            // Пытаемся найти существующую конфигурацию
            var findConfigStart = stopwatch.ElapsedMilliseconds;
            if (configurationId.HasValue)
            {
                configuration = await _context.JewelryConfigurations
                    .Where(c => c.Id == configurationId.Value && (c.UserId == userId || (c.UserId == null && userId == null)))
                    .Include(c => c.Stones)
                    .Include(c => c.Engravings)
                    .FirstOrDefaultAsync(ct);

                if (configuration != null)
                {
                    _logger.LogInformation(
                        "Found existing configuration {ConfigurationId} in {ElapsedMs}ms, updating",
                        configurationId.Value, stopwatch.ElapsedMilliseconds - findConfigStart);
                }
                else
                {
                    _logger.LogWarning(
                        "Configuration {ConfigurationId} not found or access denied for user {UserId}, will create new",
                        configurationId.Value, userId);
                }
            }

        // Если конфигурация не найдена или ID не был указан - создаём новую
        if (configuration == null)
        {
            _logger.LogInformation(
                "Creating new configuration for user {UserId}, baseModelId={BaseModelId}, materialId={MaterialId}",
                userId, baseModelId, materialId);

            // Валидация BaseModel и Material
            var baseModelExists = await _context.JewelryBaseModels
                .AnyAsync(m => m.Id == baseModelId, ct);

            if (!baseModelExists)
            {
                _logger.LogWarning("Base model {BaseModelId} not found in database", baseModelId);
                throw new ArgumentException($"Base model {baseModelId} not found");
            }

            var materialExists = await _context.Materials
                .AnyAsync(m => m.Id == materialId, ct);

            if (!materialExists)
            {
                _logger.LogWarning("Material {MaterialId} not found in database", materialId);
                throw new ArgumentException($"Material {materialId} not found");
            }

            var now = DateTimeOffset.UtcNow;
            configuration = new JewelryConfiguration
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BaseModelId = baseModelId,
                MaterialId = materialId,
                Name = request.Name,
                Status = string.IsNullOrEmpty(request.Status)
                    ? ConfigurationStatus.Draft
                    : Enum.TryParse<ConfigurationStatus>(request.Status, ignoreCase: true, out var parsedStatus)
                        ? parsedStatus
                        : throw new ArgumentException($"Invalid status value: {request.Status}"),
                ConfigJson = request.ConfigJson,
                EngravingText = request.EngravingText?.Trim(),
                CreatedAt = now,
                UpdatedAt = now,
                Stones = new List<JewelryConfigurationStone>(),
                Engravings = new List<JewelryConfigurationEngraving>()
            };

            _context.JewelryConfigurations.Add(configuration);

            _logger.LogInformation(
                "Created new configuration with ID {ConfigurationId}",
                configuration.Id);
        }
        else
        {
            // Обновляем существующую конфигурацию
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
                if (Enum.TryParse<ConfigurationStatus>(request.Status, ignoreCase: true, out var parsedStatus))
                {
                    configuration.Status = parsedStatus;
                }
                else
                {
                    throw new ArgumentException($"Invalid status value: {request.Status}");
                }
            }

            // Update engraving text (allow clearing with empty string or null)
            if (request.EngravingText != null)
            {
                configuration.EngravingText = string.IsNullOrWhiteSpace(request.EngravingText)
                    ? null
                    : request.EngravingText.Trim();
            }

            configuration.UpdatedAt = DateTimeOffset.UtcNow;
        }

        // Обновление камней (для обоих случаев: новая и существующая)
        if (request.Stones != null)
        {
            var stonesUpdateStart = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Updating stones for configuration {ConfigurationId}: {StoneCount} stones in request",
                configuration.Id, request.Stones.Count);

            // IMPORTANT: Detach existing tracked stones BEFORE using ExecuteDeleteAsync
            // ExecuteDeleteAsync bypasses change tracking, so tracked entities would cause
            // DbUpdateConcurrencyException when SaveChangesAsync tries to delete already-deleted rows
            foreach (var existingStone in configuration.Stones.ToList())
            {
                _context.Entry(existingStone).State = EntityState.Detached;
            }
            configuration.Stones.Clear();

            // Use ExecuteDeleteAsync for efficient bulk deletion (safe now that entities are detached)
            var deletedCount = await _context.JewelryConfigurationStones
                .Where(s => s.ConfigurationId == configuration.Id)
                .ExecuteDeleteAsync(ct);

            _logger.LogDebug(
                "Deleted {DeletedCount} existing stones for configuration {ConfigurationId} in {ElapsedMs}ms",
                deletedCount, configuration.Id, stopwatch.ElapsedMilliseconds - stonesUpdateStart);

            // Add new stones - explicitly add to DbSet to ensure they are marked as Added
            var newStones = request.Stones.Select(stoneDto => new JewelryConfigurationStone
            {
                Id = Guid.NewGuid(),
                ConfigurationId = configuration.Id,
                StoneTypeId = stoneDto.StoneTypeId,
                PositionIndex = stoneDto.PositionIndex,
                CaratWeight = stoneDto.CaratWeight,
                SizeMm = stoneDto.SizeMm,
                Count = stoneDto.Count,
                PlacementDataJson = stoneDto.PlacementDataJson
            }).ToList();

            // Add to DbSet explicitly (ensures EntityState.Added)
            // NOTE: We don't add to navigation property here to avoid duplicates
            // The navigation property will be reloaded after SaveChangesAsync
            await _context.JewelryConfigurationStones.AddRangeAsync(newStones, ct);
            foreach (var stone in newStones)
            {
                _logger.LogDebug(
                    "Added stone: StoneTypeId={StoneTypeId}, Position={Position}, Count={Count}",
                    stone.StoneTypeId, stone.PositionIndex, stone.Count);
            }

            _logger.LogInformation(
                "Stones updated for configuration {ConfigurationId}: {StoneCount} stones added in {ElapsedMs}ms",
                configuration.Id, request.Stones.Count, stopwatch.ElapsedMilliseconds - stonesUpdateStart);
        }

        // Обновление гравировок (для обоих случаев: новая и существующая)
        if (request.Engravings != null)
        {
            // IMPORTANT: Detach existing tracked engravings BEFORE using ExecuteDeleteAsync
            // ExecuteDeleteAsync bypasses change tracking, so tracked entities would cause
            // DbUpdateConcurrencyException when SaveChangesAsync tries to delete already-deleted rows
            foreach (var existingEngraving in configuration.Engravings.ToList())
            {
                _context.Entry(existingEngraving).State = EntityState.Detached;
            }
            configuration.Engravings.Clear();

            // Use ExecuteDeleteAsync for efficient bulk deletion (safe now that entities are detached)
            await _context.JewelryConfigurationEngravings
                .Where(e => e.ConfigurationId == configuration.Id)
                .ExecuteDeleteAsync(ct);

            // Add new engravings - explicitly add to DbSet to ensure they are marked as Added
            var newEngravings = request.Engravings.Select(engravingDto => new JewelryConfigurationEngraving
            {
                Id = Guid.NewGuid(),
                ConfigurationId = configuration.Id,
                Text = engravingDto.Text,
                FontName = engravingDto.FontName,
                Location = engravingDto.Location,
                SizeMm = engravingDto.SizeMm,
                IsInternal = engravingDto.IsInternal
            }).ToList();

            // Add to DbSet explicitly (ensures EntityState.Added)
            // NOTE: We don't add to navigation property here to avoid duplicates
            // The navigation property will be reloaded after SaveChangesAsync
            await _context.JewelryConfigurationEngravings.AddRangeAsync(newEngravings, ct);
        }

        // FIXED: Calculate price BEFORE saving to avoid multiple SaveChangesAsync calls
        bool shouldRecalculatePrice = request.MaterialId.HasValue || request.Stones != null;

        if (shouldRecalculatePrice)
        {
            try
            {
                _logger.LogInformation("Calculating price for configuration {ConfigurationId} before save", configuration.Id);

                // Calculate price based on the in-memory configuration (not yet saved)
                var basePrice = configuration.BaseModel?.BasePrice ??
                    (await _context.JewelryBaseModels
                        .Where(m => m.Id == configuration.BaseModelId)
                        .Select(m => m.BasePrice)
                        .FirstOrDefaultAsync(ct));

                var materialPriceFactor = configuration.Material?.PriceFactor ??
                    (await _context.Materials
                        .Where(m => m.Id == configuration.MaterialId)
                        .Select(m => m.PriceFactor)
                        .FirstOrDefaultAsync(ct));

                var materialAdjustedPrice = basePrice * materialPriceFactor;

                // Calculate stones price
                decimal stonesPrice = 0;
                if (configuration.Stones != null && configuration.Stones.Any())
                {
                    var stoneTypeIds = configuration.Stones.Select(s => s.StoneTypeId).ToList();
                    var stoneTypes = await _context.StoneTypes
                        .Where(st => stoneTypeIds.Contains(st.Id))
                        .ToDictionaryAsync(st => st.Id, st => st.DefaultPricePerCarat, ct);

                    foreach (var stone in configuration.Stones)
                    {
                        var caratWeight = stone.CaratWeight ?? 0;
                        var pricePerCarat = stoneTypes.GetValueOrDefault(stone.StoneTypeId, 0);
                        var count = stone.Count;
                        stonesPrice += pricePerCarat * caratWeight * count;
                    }
                }

                configuration.EstimatedPrice = materialAdjustedPrice + stonesPrice;

                _logger.LogInformation(
                    "Price calculated for configuration {ConfigurationId}: {Price}",
                    configuration.Id, configuration.EstimatedPrice);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to calculate price for configuration {ConfigurationId}, will use 0", configuration.Id);
                configuration.EstimatedPrice = 0;
            }
        }

        // Сохраняем изменения (SINGLE SaveChangesAsync call)
        var saveStart = stopwatch.ElapsedMilliseconds;
        _logger.LogInformation("Saving configuration {ConfigurationId} to database", configuration.Id);

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "✅ Configuration saved: id={ConfigurationId}, stones={StoneCount}, price={Price}, saveTime={SaveMs}ms",
            configuration.Id, configuration.Stones?.Count ?? 0, configuration.EstimatedPrice,
            stopwatch.ElapsedMilliseconds - saveStart);

        // IMPORTANT: Load navigation properties BEFORE CommitAsync
        // After CommitAsync, the transaction is completed and LoadAsync will fail

        // Load BaseModel with Category if not already loaded
        if (configuration.BaseModel == null)
        {
            await _context.Entry(configuration)
                .Reference(c => c.BaseModel)
                .Query()
                .Include(bm => bm.Category)
                .LoadAsync(ct);
        }

        // Load Material if not already loaded
        if (configuration.Material == null)
        {
            await _context.Entry(configuration)
                .Reference(c => c.Material)
                .LoadAsync(ct);
        }

        // Reload stones from database (needed because we use AddRangeAsync to DbSet, not navigation property)
        await _context.Entry(configuration)
            .Collection(c => c.Stones)
            .Query()
            .Include(s => s.StoneType)
            .LoadAsync(ct);

        // Reload engravings from database (needed because we use AddRangeAsync to DbSet, not navigation property)
        await _context.Entry(configuration)
            .Collection(c => c.Engravings)
            .LoadAsync(ct);

        // Assets are not modified in this operation, load if needed
        if (configuration.Assets == null || !configuration.Assets.Any())
        {
            await _context.Entry(configuration)
                .Collection(c => c.Assets)
                .LoadAsync(ct);
        }

        _logger.LogInformation(
            "✅ Configuration fully loaded: id={ConfigurationId}, baseModelId={BaseModelId}, materialId={MaterialId}, stones={StoneCount}",
            configuration.Id, configuration.BaseModelId, configuration.MaterialId, configuration.Stones?.Count ?? 0);

        // Commit the transaction AFTER all loading is done
        await transaction.CommitAsync(ct);

        _logger.LogInformation(
            "✅ Transaction committed: id={ConfigurationId}, totalTime={TotalMs}ms",
            configuration.Id, stopwatch.ElapsedMilliseconds);

        // Audit log - properly awaited to ensure completion
        // AuditService uses IServiceScopeFactory to create its own DbContext scope
        var isNewConfiguration = !configurationId.HasValue || configuration.CreatedAt == configuration.UpdatedAt;
        await _auditService.LogActionAsync(
            userId,
            "JewelryConfiguration",
            configuration.Id.ToString(),
            isNewConfiguration ? "Created" : "Updated",
            new { BaseModelId = baseModelId, MaterialId = materialId, StonesCount = request.Stones?.Count ?? 0 });

        // Ensure navigation properties are loaded before mapping
        if (configuration.BaseModel == null)
        {
            throw new InvalidOperationException($"BaseModel not loaded for configuration {configuration.Id}");
        }

        if (configuration.Material == null)
        {
            throw new InvalidOperationException($"Material not loaded for configuration {configuration.Id}");
        }

        return new JewelryConfigurationDetailDto
        {
            Id = configuration.Id,
            UserId = configuration.UserId,
            BaseModelId = configuration.BaseModelId,
            MaterialId = configuration.MaterialId,
            Name = configuration.Name,
            Status = configuration.Status.ToString(),
            ConfigJson = configuration.ConfigJson,
            EngravingText = configuration.EngravingText,
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
            Stones = configuration.Stones?.Select(s => new ConfigurationStoneDto
            {
                Id = s.Id,
                StoneTypeId = s.StoneTypeId,
                StoneTypeName = s.StoneType?.Name ?? "Unknown",
                PositionIndex = s.PositionIndex,
                CaratWeight = s.CaratWeight,
                SizeMm = s.SizeMm,
                Count = s.Count,
                PlacementDataJson = s.PlacementDataJson
            }).ToList() ?? new List<ConfigurationStoneDto>(),
            Engravings = configuration.Engravings?.Select(e => new ConfigurationEngravingDto
            {
                Id = e.Id,
                Text = e.Text,
                FontName = e.FontName,
                Location = e.Location,
                SizeMm = e.SizeMm,
                IsInternal = e.IsInternal
            }).ToList() ?? new List<ConfigurationEngravingDto>(),
            Assets = configuration.Assets?.Select(a => new UploadedAssetDto
            {
                Id = a.Id,
                FileType = a.FileType,
                Url = a.Url,
                OriginalFileName = a.OriginalFileName,
                CreatedAt = a.CreatedAt
            }).ToList() ?? new List<UploadedAssetDto>()
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex,
                    "Configuration {ConfigurationId} was modified or deleted (concurrency conflict). Total time: {TotalMs}ms",
                    configurationId, stopwatch.ElapsedMilliseconds);

                await transaction.RollbackAsync();

                // Rethrow to let the execution strategy handle retry, or caller can retry with null configurationId
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error saving configuration. Total time: {TotalMs}ms",
                    stopwatch.ElapsedMilliseconds);

                await transaction.RollbackAsync();
                throw;
            }
        });
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

        // Audit log
        await _auditService.LogDeleteAsync(
            userId,
            "JewelryConfiguration",
            configurationId.ToString(),
            ct);

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
