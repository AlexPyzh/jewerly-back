namespace JewerlyBack.Models;

public class StoneType
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Color { get; set; }
    public decimal DefaultPricePerCarat { get; set; }
    public bool IsActive { get; set; }

    // Навигационные свойства
    public ICollection<JewelryConfigurationStone> ConfigurationStones { get; set; } = new List<JewelryConfigurationStone>();
}
