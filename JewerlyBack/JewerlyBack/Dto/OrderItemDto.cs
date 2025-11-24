namespace JewerlyBack.Dto;

public class OrderItemDto
{
    public Guid ConfigurationId { get; set; }
    public string ConfigurationName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ItemPrice { get; set; }
    public string? PreviewImageUrl { get; set; }
}
