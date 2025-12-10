namespace JewerlyBack.Application.Interfaces;

/// <summary>
/// Сервис для расчета стоимости ювелирных изделий
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Calculates price from components without database access
    /// </summary>
    /// <param name="basePrice">Base price of the jewelry model</param>
    /// <param name="materialPriceFactor">Material price factor multiplier</param>
    /// <param name="stones">Stone tuples: (pricePerCarat, caratWeight, count)</param>
    /// <returns>Calculated total price</returns>
    decimal CalculatePrice(
        decimal basePrice,
        decimal materialPriceFactor,
        IEnumerable<(decimal pricePerCarat, decimal caratWeight, int count)> stones);

    /// <summary>
    /// Рассчитывает примерную стоимость конфигурации ювелирного изделия
    /// </summary>
    /// <param name="configurationId">ID конфигурации</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Рассчитанная стоимость</returns>
    Task<decimal> CalculateConfigurationPriceAsync(Guid configurationId, CancellationToken ct = default);

    /// <summary>
    /// Calculates and saves the price to the configuration entity
    /// </summary>
    /// <param name="configurationId">ID конфигурации</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Calculated and saved price</returns>
    Task<decimal> CalculateAndSavePriceAsync(Guid configurationId, CancellationToken ct = default);
}
