using JewerlyBack.Application.Interfaces;
using JewerlyBack.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JewerlyBack.Services;

/// <summary>
/// Реализация сервиса для расчета стоимости ювелирных изделий
/// </summary>
public class PricingService : IPricingService
{
    private readonly AppDbContext _context;
    private readonly ICatalogCacheService _cacheService;
    private readonly ILogger<PricingService> _logger;

    public PricingService(
        AppDbContext context,
        ICatalogCacheService cacheService,
        ILogger<PricingService> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Calculates price from components without database access.
    /// Formula: (basePrice × materialPriceFactor) + Σ(pricePerCarat × caratWeight × count)
    /// </summary>
    public decimal CalculatePrice(
        decimal basePrice,
        decimal materialPriceFactor,
        IEnumerable<(decimal pricePerCarat, decimal caratWeight, int count)> stones)
    {
        var materialAdjustedPrice = basePrice * materialPriceFactor;

        decimal stonesPrice = 0;
        foreach (var (pricePerCarat, caratWeight, count) in stones)
        {
            stonesPrice += pricePerCarat * caratWeight * count;
        }

        return materialAdjustedPrice + stonesPrice;
    }

    /// <summary>
    /// Рассчитывает примерную стоимость конфигурации ювелирного изделия.
    /// MVP-логика: (BasePrice * MaterialPriceFactor) + SUM(StonePrice * CaratWeight * Count)
    /// </summary>
    public async Task<decimal> CalculateConfigurationPriceAsync(Guid configurationId, CancellationToken ct = default)
    {
        _logger.LogDebug("Calculating price for configuration {ConfigurationId}", configurationId);

        // Load configuration with BaseModel and Stones
        var configuration = await _context.JewelryConfigurations
            .AsNoTracking()
            .Include(c => c.BaseModel)
            .Include(c => c.Stones)
            .FirstOrDefaultAsync(c => c.Id == configurationId, ct);

        if (configuration == null)
        {
            _logger.LogWarning("Configuration {ConfigurationId} not found", configurationId);
            throw new ArgumentException($"Configuration {configurationId} not found");
        }

        // Get material from cache
        var material = await _cacheService.GetMaterialByIdAsync(configuration.MaterialId, ct);
        if (material == null)
        {
            _logger.LogWarning("Material {MaterialId} not found", configuration.MaterialId);
            throw new ArgumentException($"Material {configuration.MaterialId} not found");
        }

        // Get stone types from cache for pricing
        var stoneTypes = await _cacheService.GetStoneTypesAsync(ct);
        var stoneTypesDict = stoneTypes.ToDictionary(st => st.Id, st => st.DefaultPricePerCarat);

        // Map stones to tuples
        var stoneTuples = configuration.Stones?
            .Select(s => (
                pricePerCarat: stoneTypesDict.GetValueOrDefault(s.StoneTypeId, 0),
                caratWeight: s.CaratWeight ?? 0,
                count: s.Count
            ))
            .ToList() ?? new List<(decimal, decimal, int)>();

        var totalPrice = CalculatePrice(
            configuration.BaseModel.BasePrice,
            material.PriceFactor,
            stoneTuples);

        _logger.LogDebug(
            "Configuration {ConfigurationId} price calculated: {TotalPrice} (base: {BasePrice}, material factor: {MaterialFactor}, stones count: {StonesCount})",
            configurationId, totalPrice, configuration.BaseModel.BasePrice, material.PriceFactor, stoneTuples.Count);

        return totalPrice;
    }

    /// <summary>
    /// Calculates and saves the price to the configuration entity.
    /// </summary>
    public async Task<decimal> CalculateAndSavePriceAsync(Guid configurationId, CancellationToken ct = default)
    {
        _logger.LogDebug("Calculating and saving price for configuration {ConfigurationId}", configurationId);

        var price = await CalculateConfigurationPriceAsync(configurationId, ct);

        // Update the configuration with calculated price
        var configuration = await _context.JewelryConfigurations
            .FirstOrDefaultAsync(c => c.Id == configurationId, ct);

        if (configuration != null)
        {
            configuration.EstimatedPrice = price;
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Configuration {ConfigurationId} price saved: {Price}",
                configurationId, price);
        }

        return price;
    }
}
