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
    private readonly ILogger<PricingService> _logger;

    public PricingService(AppDbContext context, ILogger<PricingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Рассчитывает примерную стоимость конфигурации ювелирного изделия.
    /// MVP-логика: (BasePrice * MaterialPriceFactor) + SUM(StonePrice * CaratWeight * Count)
    /// </summary>
    public async Task<decimal> CalculateConfigurationPriceAsync(Guid configurationId, CancellationToken ct = default)
    {
        _logger.LogInformation("Calculating price for configuration {ConfigurationId}", configurationId);

        // Загружаем конфигурацию со всеми необходимыми связями
        var configuration = await _context.JewelryConfigurations
            .AsNoTracking()
            .Include(c => c.BaseModel)
            .Include(c => c.Material)
            .Include(c => c.Stones)
                .ThenInclude(s => s.StoneType)
            .FirstOrDefaultAsync(c => c.Id == configurationId, ct);

        if (configuration == null)
        {
            _logger.LogWarning("Configuration {ConfigurationId} not found", configurationId);
            throw new ArgumentException($"Configuration {configurationId} not found");
        }

        // 1. Базовая цена изделия
        var basePrice = configuration.BaseModel.BasePrice;
        _logger.LogDebug("Base price: {BasePrice}", basePrice);

        // 2. Множитель материала
        var materialPriceFactor = configuration.Material.PriceFactor;
        _logger.LogDebug("Material price factor: {MaterialPriceFactor}", materialPriceFactor);

        // 3. Цена с учётом материала
        // TODO: В будущем можно усложнить — учитывать реальный вес изделия, курсы драгметаллов
        var materialAdjustedPrice = basePrice * materialPriceFactor;
        _logger.LogDebug("Material adjusted price: {MaterialAdjustedPrice}", materialAdjustedPrice);

        // 4. Расчёт стоимости камней
        decimal stonesPrice = 0;
        if (configuration.Stones != null && configuration.Stones.Any())
        {
            foreach (var stone in configuration.Stones)
            {
                // Цена камня = цена за карат * вес в каратах * количество
                var caratWeight = stone.CaratWeight ?? 0;
                var pricePerCarat = stone.StoneType.DefaultPricePerCarat;
                var count = stone.Count;

                var stonePrice = pricePerCarat * caratWeight * count;
                stonesPrice += stonePrice;

                _logger.LogDebug(
                    "Stone {StoneType}: {PricePerCarat} * {CaratWeight} * {Count} = {StonePrice}",
                    stone.StoneType.Name, pricePerCarat, caratWeight, count, stonePrice);
            }

            // TODO: В будущем можно учитывать:
            // - Качество камня (огранка, чистота, цвет)
            // - Редкость камня
            // - Сложность закрепки
        }

        _logger.LogDebug("Total stones price: {StonesPrice}", stonesPrice);

        // 5. Итоговая цена
        var totalPrice = materialAdjustedPrice + stonesPrice;

        // TODO: В будущем добавить:
        // - Стоимость гравировки
        // - Стоимость работы мастера (зависит от сложности)
        // - Накладные расходы
        // - Маржа
        // - Скидки/акции

        _logger.LogInformation(
            "Configuration {ConfigurationId} price calculated: {TotalPrice} (base: {BasePrice}, material factor: {MaterialFactor}, stones: {StonesPrice})",
            configurationId, totalPrice, basePrice, materialPriceFactor, stonesPrice);

        return totalPrice;
    }
}
