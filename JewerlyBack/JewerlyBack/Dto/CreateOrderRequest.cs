namespace JewerlyBack.Dto;

public class CreateOrderRequest
{
    public List<CreateOrderItemRequest> Items { get; set; } = new();
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? Notes { get; set; }
}

public class CreateOrderItemRequest
{
    public Guid ConfigurationId { get; set; }
    public int Quantity { get; set; }
}
