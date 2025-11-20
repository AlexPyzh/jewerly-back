namespace JewerlyBack.Models;

public class Material
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string MetalType { get; set; }
    public int? Karat { get; set; }
    public string? ColorHex { get; set; }
    public decimal PriceFactor { get; set; }
    public bool IsActive { get; set; }

    // Навигационные свойства
    public ICollection<JewelryConfiguration> Configurations { get; set; } = new List<JewelryConfiguration>();
}
