namespace JewerlyBack.Models;

public class UploadedAsset
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ConfigurationId { get; set; }
    public required string FileType { get; set; }
    public required string Url { get; set; }
    public string? OriginalFileName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Навигационные свойства
    public AppUser User { get; set; } = null!;
    public JewelryConfiguration? Configuration { get; set; }
}
