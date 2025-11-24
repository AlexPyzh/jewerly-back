using AutoMapper;
using JewerlyBack.Application.Interfaces;
using JewerlyBack.Data;
using JewerlyBack.Dto;
using JewerlyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace JewerlyBack.Services;

/// <summary>
/// Реализация сервиса для работы с заказами
/// </summary>
public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IPricingService _pricingService;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    /// <summary>
    /// Допустимые статусы конфигурации для создания заказа
    /// </summary>
    private static readonly string[] AllowedConfigurationStatuses = ["Draft", "ReadyToOrder"];

    /// <summary>
    /// Статусы заказа, которые можно отменить
    /// </summary>
    private static readonly string[] CancellableOrderStatuses = ["New", "Pending"];

    public OrderService(
        AppDbContext context,
        IPricingService pricingService,
        IMapper mapper,
        ILogger<OrderService> logger)
    {
        _context = context;
        _pricingService = pricingService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OrderListItemDto>> GetUserOrdersAsync(Guid userId, CancellationToken ct = default)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        return _mapper.Map<IReadOnlyList<OrderListItemDto>>(orders);
    }

    /// <inheritdoc />
    public async Task<OrderDetailDto?> GetOrderByIdAsync(Guid userId, Guid orderId, CancellationToken ct = default)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(i => i.Configuration)
                    .ThenInclude(c => c.BaseModel)
            .Where(o => o.Id == orderId && o.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (order == null)
        {
            return null;
        }

        return _mapper.Map<OrderDetailDto>(order);
    }

    /// <inheritdoc />
    public async Task<Guid> CreateOrderAsync(Guid userId, CreateOrderRequest request, CancellationToken ct = default)
    {
        // Валидация: проверяем что есть хотя бы один item
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new ArgumentException("Order must contain at least one item");
        }

        // Валидация: проверяем что quantity положительный
        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                throw new ArgumentException($"Quantity must be positive for configuration {item.ConfigurationId}");
            }
        }

        // Получаем все запрошенные конфигурации
        var configurationIds = request.Items.Select(i => i.ConfigurationId).Distinct().ToList();

        var configurations = await _context.JewelryConfigurations
            .AsNoTracking()
            .Include(c => c.BaseModel)
            .Include(c => c.Material)
            .Include(c => c.Stones)
                .ThenInclude(s => s.StoneType)
            .Where(c => configurationIds.Contains(c.Id))
            .ToListAsync(ct);

        // Валидация: все конфигурации должны существовать
        if (configurations.Count != configurationIds.Count)
        {
            var foundIds = configurations.Select(c => c.Id).ToHashSet();
            var missingIds = configurationIds.Where(id => !foundIds.Contains(id)).ToList();
            throw new ArgumentException($"Configurations not found: {string.Join(", ", missingIds)}");
        }

        // Валидация: все конфигурации должны принадлежать пользователю
        var notOwnedConfigs = configurations.Where(c => c.UserId != userId).ToList();
        if (notOwnedConfigs.Count > 0)
        {
            throw new ArgumentException(
                $"Access denied to configurations: {string.Join(", ", notOwnedConfigs.Select(c => c.Id))}");
        }

        // Валидация: все конфигурации должны иметь допустимый статус
        var invalidStatusConfigs = configurations
            .Where(c => !AllowedConfigurationStatuses.Contains(c.Status))
            .ToList();

        if (invalidStatusConfigs.Count > 0)
        {
            throw new ArgumentException(
                $"Configurations have invalid status for ordering (must be Draft or ReadyToOrder): " +
                $"{string.Join(", ", invalidStatusConfigs.Select(c => $"{c.Id} (status: {c.Status})"))}");
        }

        // Пересчитываем цены через PricingService
        var configPrices = new Dictionary<Guid, decimal>();
        foreach (var config in configurations)
        {
            var price = await _pricingService.CalculateConfigurationPriceAsync(config.Id, ct);
            configPrices[config.Id] = price;

            _logger.LogDebug("Calculated price for configuration {ConfigurationId}: {Price}",
                config.Id, price);
        }

        // Генерируем номер заказа
        var orderNumber = await GenerateOrderNumberAsync(ct);

        // Создаём заказ
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderNumber = orderNumber,
            Status = "New",
            Currency = "RUB",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Создаём OrderItems с зафиксированными ценами
        var orderItems = new List<OrderItem>();
        decimal totalPrice = 0;

        foreach (var requestItem in request.Items)
        {
            var unitPrice = configPrices[requestItem.ConfigurationId];
            var itemPrice = unitPrice * requestItem.Quantity;
            totalPrice += itemPrice;

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ConfigurationId = requestItem.ConfigurationId,
                Quantity = requestItem.Quantity,
                UnitPrice = unitPrice,
                ItemPrice = itemPrice
            });
        }

        order.TotalPrice = totalPrice;
        order.Items = orderItems;

        // Сохраняем
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Order {OrderNumber} created for user {UserId}. Items: {ItemCount}, Total: {TotalPrice} {Currency}",
            orderNumber, userId, orderItems.Count, totalPrice, order.Currency);

        return order.Id;
    }

    /// <inheritdoc />
    public async Task<bool> CancelOrderAsync(Guid userId, Guid orderId, CancellationToken ct = default)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId && o.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found for user {UserId}", orderId, userId);
            return false;
        }

        // Проверяем, можно ли отменить заказ
        if (!CancellableOrderStatuses.Contains(order.Status))
        {
            _logger.LogWarning(
                "Cannot cancel order {OrderId} with status {Status}. Cancellable statuses: {CancellableStatuses}",
                orderId, order.Status, string.Join(", ", CancellableOrderStatuses));
            return false;
        }

        order.Status = "Cancelled";
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Order {OrderNumber} cancelled by user {UserId}", order.OrderNumber, userId);

        return true;
    }

    /// <summary>
    /// Генерирует уникальный человекочитаемый номер заказа
    /// Формат: ORD-YYYYMMDD-XXXXX (где XXXXX - порядковый номер за день)
    /// </summary>
    private async Task<string> GenerateOrderNumberAsync(CancellationToken ct)
    {
        var today = DateTimeOffset.UtcNow;
        var datePrefix = today.ToString("yyyyMMdd");

        // Считаем количество заказов за сегодня
        var startOfDay = new DateTimeOffset(today.Year, today.Month, today.Day, 0, 0, 0, TimeSpan.Zero);
        var endOfDay = startOfDay.AddDays(1);

        var ordersToday = await _context.Orders
            .CountAsync(o => o.CreatedAt >= startOfDay && o.CreatedAt < endOfDay, ct);

        var sequenceNumber = ordersToday + 1;

        return $"ORD-{datePrefix}-{sequenceNumber:D5}";
    }
}
