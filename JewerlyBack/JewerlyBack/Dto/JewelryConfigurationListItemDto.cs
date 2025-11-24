namespace JewerlyBack.Dto;

public class JewelryConfigurationListItemDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string Status { get; set; } = string.Empty;
    public string BaseModelName { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public decimal? EstimatedPrice { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
