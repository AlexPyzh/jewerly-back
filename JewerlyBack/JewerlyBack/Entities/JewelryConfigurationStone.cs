namespace JewerlyBack.Models;

public class JewelryConfigurationStone
{
    public Guid Id { get; set; }
    public Guid ConfigurationId { get; set; }
    public int StoneTypeId { get; set; }
    public int PositionIndex { get; set; }
    public decimal? CaratWeight { get; set; }
    public decimal? SizeMm { get; set; }
    public int Count { get; set; }
    public string? PlacementDataJson { get; set; }

    // Навигационные свойства
    public JewelryConfiguration Configuration { get; set; } = null!;
    public StoneType StoneType { get; set; } = null!;
}
