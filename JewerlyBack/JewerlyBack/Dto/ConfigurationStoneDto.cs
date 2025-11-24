namespace JewerlyBack.Dto;

public class ConfigurationStoneDto
{
    public Guid Id { get; set; }
    public int StoneTypeId { get; set; }
    public string StoneTypeName { get; set; } = string.Empty;
    public int PositionIndex { get; set; }
    public decimal? CaratWeight { get; set; }
    public decimal? SizeMm { get; set; }
    public int Count { get; set; }
    public string? PlacementDataJson { get; set; }
}
