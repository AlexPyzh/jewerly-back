namespace JewerlyBack.Models;

public class JewelryBaseModel
{
    public Guid Id { get; set; }
    public int CategoryId { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public string? Description { get; set; }
    public string? AiDescription { get; set; }
    public string? PreviewImageUrl { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
    public string? MetadataJson { get; set; }

    // Навигационные свойства
    public JewelryCategory Category { get; set; } = null!;
    public ICollection<JewelryConfiguration> Configurations { get; set; } = new List<JewelryConfiguration>();
}
