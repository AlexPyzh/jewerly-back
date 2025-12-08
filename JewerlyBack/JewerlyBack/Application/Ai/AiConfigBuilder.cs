using System.Text.Json;
using JewerlyBack.Application.Ai.Models;
using JewerlyBack.Data;
using Microsoft.EntityFrameworkCore;

namespace JewerlyBack.Application.Ai;

/// <summary>
/// Реализация сервиса для построения семантической AI-конфигурации.
/// Обогащает сырые ID/коды данными из БД для создания человеко-читаемого формата.
/// </summary>
public sealed class AiConfigBuilder : IAiConfigBuilder
{
    private readonly AppDbContext _context;
    private readonly ILogger<AiConfigBuilder> _logger;

    public AiConfigBuilder(
        AppDbContext context,
        ILogger<AiConfigBuilder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Строит семантическую AI-конфигурацию для указанной конфигурации ювелирного изделия.
    /// </summary>
    public async Task<AiConfigDto> BuildForConfigurationAsync(
        Guid configurationId,
        Guid? userId,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Building AI config for configuration {ConfigurationId}, userId {UserId}",
            configurationId, userId?.ToString() ?? "guest");

        // Загружаем конфигурацию со всеми связанными сущностями
        var configuration = await _context.JewelryConfigurations
            .AsNoTracking()
            .Include(c => c.BaseModel)
                .ThenInclude(bm => bm.Category)
            .Include(c => c.Material)
            .Include(c => c.Stones)
                .ThenInclude(s => s.StoneType)
            .Where(c => c.Id == configurationId)
            .FirstOrDefaultAsync(ct);

        if (configuration == null)
        {
            _logger.LogWarning(
                "Configuration {ConfigurationId} not found",
                configurationId);
            throw new ArgumentException($"Configuration {configurationId} not found");
        }

        // Проверка прав доступа
        if (configuration.UserId.HasValue)
        {
            // Конфигурация принадлежит авторизованному пользователю
            if (!userId.HasValue)
            {
                _logger.LogWarning(
                    "Guest attempted to access configuration {ConfigurationId} owned by user {UserId}",
                    configurationId, configuration.UserId.Value);
                throw new UnauthorizedAccessException("Configuration belongs to a registered user");
            }

            if (configuration.UserId.Value != userId.Value)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to access configuration {ConfigurationId} owned by {OwnerId}",
                    userId.Value, configurationId, configuration.UserId.Value);
                throw new UnauthorizedAccessException("Configuration does not belong to current user");
            }
        }
        else
        {
            // Гостевая конфигурация (UserId == null) - разрешаем доступ
            _logger.LogDebug(
                "Building AI config for guest configuration {ConfigurationId}",
                configurationId);
        }

        // Парсим MetadataJson базовой модели (если есть)
        Dictionary<string, object>? baseModelMetadata = null;
        if (!string.IsNullOrWhiteSpace(configuration.BaseModel.MetadataJson))
        {
            try
            {
                baseModelMetadata = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    configuration.BaseModel.MetadataJson);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex,
                    "Failed to parse MetadataJson for BaseModel {BaseModelId}",
                    configuration.BaseModel.Id);
            }
        }

        // Строим список камней (если есть)
        IReadOnlyList<AiStoneConfigDto>? stones = null;
        if (configuration.Stones?.Any() == true)
        {
            stones = configuration.Stones
                .OrderBy(s => s.PositionIndex)
                .Select(s => new AiStoneConfigDto
                {
                    StoneTypeCode = s.StoneType.Code,
                    StoneTypeName = s.StoneType.Name,
                    Color = s.StoneType.Color,
                    CaratWeight = s.CaratWeight,
                    SizeMm = s.SizeMm,
                    Count = s.Count,
                    PositionIndex = s.PositionIndex
                })
                .ToList()
                .AsReadOnly();
        }

        // Собираем семантическую конфигурацию
        var aiConfig = new AiConfigDto
        {
            ConfigurationId = configuration.Id,
            ConfigurationName = configuration.Name,

            // Category
            CategoryCode = configuration.BaseModel.Category.Code,
            CategoryName = configuration.BaseModel.Category.Name,
            CategoryDescription = configuration.BaseModel.Category.Description,
            CategoryAiDescription = configuration.BaseModel.Category.AiCategoryDescription,

            // BaseModel
            BaseModelId = configuration.BaseModel.Id,
            BaseModelCode = configuration.BaseModel.Code,
            BaseModelName = configuration.BaseModel.Name,
            BaseModelDescription = configuration.BaseModel.Description,
            BaseModelAiDescription = configuration.BaseModel.AiDescription,
            BaseModelMetadata = baseModelMetadata,

            // Material
            MaterialCode = configuration.Material.Code,
            MaterialName = configuration.Material.Name,
            MetalType = configuration.Material.MetalType,
            Karat = configuration.Material.Karat,
            MaterialColorHex = configuration.Material.ColorHex,

            // Stones
            Stones = stones
        };

        _logger.LogInformation(
            "AI config built successfully for configuration {ConfigurationId}. " +
            "Category: {Category}, BaseModel: {BaseModel}, Material: {Material}, Stones: {StoneCount}",
            configurationId,
            aiConfig.CategoryName,
            aiConfig.BaseModelName,
            aiConfig.MaterialName,
            stones?.Count ?? 0);

        return aiConfig;
    }
}
