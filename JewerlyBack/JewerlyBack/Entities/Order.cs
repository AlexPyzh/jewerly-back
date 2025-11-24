namespace JewerlyBack.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string OrderNumber { get; set; }
    public required string Status { get; set; }
    public decimal TotalPrice { get; set; }
    public required string Currency { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    // Навигационные свойства
    public AppUser User { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
