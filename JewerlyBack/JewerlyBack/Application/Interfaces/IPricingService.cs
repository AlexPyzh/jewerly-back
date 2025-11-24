namespace JewerlyBack.Application.Interfaces;

/// <summary>
/// Сервис для расчета стоимости ювелирных изделий
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Рассчитывает примерную стоимость конфигурации ювелирного изделия
    /// </summary>
    /// <param name="configurationId">ID конфигурации</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Рассчитанная стоимость</returns>
    Task<decimal> CalculateConfigurationPriceAsync(Guid configurationId, CancellationToken ct = default);

    // TODO: Методы для будущей расширенной логики:
    // - Расчет стоимости материала по весу (с учётом курсов драгметаллов)
    // - Расчет стоимости камней с учётом качества (огранка, чистота, цвет)
    // - Расчет стоимости гравировки
    // - Расчет стоимости работы мастера
    // - Получение итоговой цены с учётом всех надбавок и скидок
}
