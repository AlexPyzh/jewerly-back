namespace JewerlyBack.Dto;

public class JewelryConfigurationDetailDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid BaseModelId { get; set; }
    public int MaterialId { get; set; }
    public string? Name { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ConfigJson { get; set; }

    /// <summary>
    /// Simple engraving text for MVP (optional personalization message)
    /// </summary>
    public string? EngravingText { get; set; }

    public decimal? EstimatedPrice { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    // Связанные данные
    public JewelryBaseModelDto? BaseModel { get; set; }
    public MaterialDto? Material { get; set; }
    public List<ConfigurationStoneDto> Stones { get; set; } = new();
    public List<ConfigurationEngravingDto> Engravings { get; set; } = new();
    public List<UploadedAssetDto> Assets { get; set; } = new();
}
