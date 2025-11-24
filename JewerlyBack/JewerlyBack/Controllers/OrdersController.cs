using JewerlyBack.Application.Interfaces;
using JewerlyBack.Dto;
using JewerlyBack.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewerlyBack.Controllers;

/// <summary>
/// Контроллер для работы с заказами
/// </summary>
/// <remarks>
/// Все endpoints требуют аутентификации - работают только с заказами текущего пользователя.
/// </remarks>
[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Получить список заказов текущего пользователя
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Список заказов</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderListItemDto>>> GetUserOrders(CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();
        var orders = await _orderService.GetUserOrdersAsync(userId, ct);
        return Ok(orders);
    }

    /// <summary>
    /// Получить детальную информацию о заказе
    /// </summary>
    /// <param name="id">ID заказа</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Детали заказа</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailDto>> GetOrderById([FromRoute] Guid id, CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();
        var order = await _orderService.GetOrderByIdAsync(userId, id, ct);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found or access denied for user {UserId}", id, userId);
            return NotFound(new { message = $"Order with ID {id} not found or access denied" });
        }

        return Ok(order);
    }

    /// <summary>
    /// Создать новый заказ
    /// </summary>
    /// <param name="request">Данные для создания заказа (список конфигураций с количеством)</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Информация о созданном заказе</returns>
    /// <remarks>
    /// Для создания заказа необходимо передать список конфигураций.
    /// Каждая конфигурация должна:
    /// - Принадлежать текущему пользователю
    /// - Иметь статус "Draft" или "ReadyToOrder"
    ///
    /// При создании заказа:
    /// - Цены пересчитываются на момент создания и фиксируются
    /// - Генерируется человекочитаемый номер заказа (ORD-YYYYMMDD-XXXXX)
    /// - Статус нового заказа = "New"
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();

        try
        {
            var orderId = await _orderService.CreateOrderAsync(userId, request, ct);

            // Получаем созданный заказ для возврата номера
            var order = await _orderService.GetOrderByIdAsync(userId, orderId, ct);

            var response = new CreateOrderResponse
            {
                Id = orderId,
                OrderNumber = order?.OrderNumber ?? string.Empty,
                Message = "Order created successfully"
            };

            return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for creating order");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Отменить заказ
    /// </summary>
    /// <param name="id">ID заказа</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Результат операции</returns>
    /// <remarks>
    /// Отменить можно только заказы со статусом "New" или "Pending".
    /// Заказы в других статусах (Processing, Shipped, Completed) отменить нельзя.
    /// </remarks>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CancelOrder([FromRoute] Guid id, CancellationToken ct)
    {
        var userId = User.GetCurrentUserId();
        var success = await _orderService.CancelOrderAsync(userId, id, ct);

        if (!success)
        {
            _logger.LogWarning("Failed to cancel order {OrderId} for user {UserId}", id, userId);
            return NotFound(new
            {
                message = $"Order with ID {id} not found, access denied, or cannot be cancelled (only New/Pending orders can be cancelled)"
            });
        }

        return Ok(new { message = "Order cancelled successfully" });
    }
}

/// <summary>
/// Ответ при успешном создании заказа
/// </summary>
public class CreateOrderResponse
{
    /// <summary>
    /// ID созданного заказа
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Человекочитаемый номер заказа
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Сообщение о результате операции
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
