using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Dto;

/// <summary>
/// Запрос на создание заказа
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// Список элементов заказа
    /// </summary>
    [Required(ErrorMessage = "Items are required")]
    [MinLength(1, ErrorMessage = "Order must contain at least one item")]
    public List<CreateOrderItemRequest> Items { get; set; } = new();

    /// <summary>
    /// Контактное имя (опционально)
    /// </summary>
    [MaxLength(200, ErrorMessage = "ContactName must not exceed 200 characters")]
    public string? ContactName { get; set; }

    /// <summary>
    /// Контактный email (опционально)
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(256, ErrorMessage = "ContactEmail must not exceed 256 characters")]
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Контактный телефон (опционально)
    /// </summary>
    [MaxLength(50, ErrorMessage = "ContactPhone must not exceed 50 characters")]
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Адрес доставки (опционально)
    /// </summary>
    [MaxLength(1000, ErrorMessage = "DeliveryAddress must not exceed 1000 characters")]
    public string? DeliveryAddress { get; set; }

    /// <summary>
    /// Примечания к заказу (опционально)
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Notes must not exceed 2000 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// Элемент заказа
/// </summary>
public class CreateOrderItemRequest
{
    /// <summary>
    /// ID конфигурации украшения
    /// </summary>
    [Required(ErrorMessage = "ConfigurationId is required")]
    public Guid ConfigurationId { get; set; }

    /// <summary>
    /// Количество единиц
    /// </summary>
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
    public int Quantity { get; set; }
}
