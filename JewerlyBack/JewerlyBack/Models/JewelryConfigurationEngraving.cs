namespace JewerlyBack.Models;

public class JewelryConfigurationEngraving
{
    public Guid Id { get; set; }
    public Guid ConfigurationId { get; set; }
    public required string Text { get; set; }
    public string? FontName { get; set; }
    public required string Location { get; set; }
    public decimal? SizeMm { get; set; }
    public bool IsInternal { get; set; }

    // Навигационные свойства
    public JewelryConfiguration Configuration { get; set; } = null!;
}
