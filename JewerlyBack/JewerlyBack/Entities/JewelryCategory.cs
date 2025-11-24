namespace JewerlyBack.Models;

public class JewelryCategory
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    // Навигационные свойства
    public ICollection<JewelryBaseModel> BaseModels { get; set; } = new List<JewelryBaseModel>();
}
