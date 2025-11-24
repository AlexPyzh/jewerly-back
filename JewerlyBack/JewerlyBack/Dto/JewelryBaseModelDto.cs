namespace JewerlyBack.Dto;

public class JewelryBaseModelDto
{
    public Guid Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PreviewImageUrl { get; set; }
    public decimal BasePrice { get; set; }
}
