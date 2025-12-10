namespace JewerlyBack.Models;

public class JewelryConfiguration
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid BaseModelId { get; set; }
    public int MaterialId { get; set; }
    public string? Name { get; set; }
    public required ConfigurationStatus Status { get; set; }
    public string? ConfigJson { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    // Навигационные свойства
    public AppUser? User { get; set; }
    public JewelryBaseModel BaseModel { get; set; } = null!;
    public Material Material { get; set; } = null!;
    public ICollection<JewelryConfigurationStone> Stones { get; set; } = new List<JewelryConfigurationStone>();
    public ICollection<JewelryConfigurationEngraving> Engravings { get; set; } = new List<JewelryConfigurationEngraving>();
    public ICollection<UploadedAsset> Assets { get; set; } = new List<UploadedAsset>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
