using JewerlyBack.Application.Models;
using JewerlyBack.Dto;

namespace JewerlyBack.Application.Interfaces;

/// <summary>
/// Сервис для работы с заказами
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Получить пагинированный список заказов пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="pagination">Параметры пагинации</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Пагинированный список заказов</returns>
    Task<PagedResult<OrderListItemDto>> GetUserOrdersAsync(
        Guid userId,
        PaginationQuery pagination,
        CancellationToken ct = default);

    /// <summary>
    /// Получить детали заказа по ID
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="orderId">ID заказа</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Детали заказа или null, если не найден/нет доступа</returns>
    Task<OrderDetailDto?> GetOrderByIdAsync(Guid userId, Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Создать заказ из конфигураций
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="request">Данные для создания заказа</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>ID созданного заказа</returns>
    /// <exception cref="ArgumentException">Если валидация не прошла</exception>
    Task<Guid> CreateOrderAsync(Guid userId, CreateOrderRequest request, CancellationToken ct = default);

    /// <summary>
    /// Отменить заказ
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="orderId">ID заказа</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>true если заказ успешно отменён, false если не найден/нет доступа/нельзя отменить</returns>
    Task<bool> CancelOrderAsync(Guid userId, Guid orderId, CancellationToken ct = default);
}
