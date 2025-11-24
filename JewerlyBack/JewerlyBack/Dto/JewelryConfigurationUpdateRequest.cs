namespace JewerlyBack.Dto;

public class JewelryConfigurationUpdateRequest
{
    public int? MaterialId { get; set; }
    public string? Name { get; set; }
    public string? ConfigJson { get; set; }
    public string? Status { get; set; }
    public List<ConfigurationStoneDto>? Stones { get; set; }
    public List<ConfigurationEngravingDto>? Engravings { get; set; }
}
