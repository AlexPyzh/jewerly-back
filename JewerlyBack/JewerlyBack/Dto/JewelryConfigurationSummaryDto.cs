namespace JewerlyBack.Dto;

/// <summary>
/// DTO для отображения краткой информации о конфигурации на главном экране
/// </summary>
public class JewelryConfigurationSummaryDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public decimal? EstimatedPrice { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? ThumbnailUrl { get; set; }
}
