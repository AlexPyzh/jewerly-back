namespace JewerlyBack.Models;

public class AppUser
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    // Навигационные свойства
    public ICollection<JewelryConfiguration> Configurations { get; set; } = new List<JewelryConfiguration>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<UploadedAsset> Assets { get; set; } = new List<UploadedAsset>();
}
