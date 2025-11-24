namespace JewerlyBack.Models;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ConfigurationId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ItemPrice { get; set; }

    // Навигационные свойства
    public Order Order { get; set; } = null!;
    public JewelryConfiguration Configuration { get; set; } = null!;
}
