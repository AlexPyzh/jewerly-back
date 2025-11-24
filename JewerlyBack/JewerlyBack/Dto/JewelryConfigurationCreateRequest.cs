namespace JewerlyBack.Dto;

public class JewelryConfigurationCreateRequest
{
    public Guid BaseModelId { get; set; }
    public int MaterialId { get; set; }
    public string? Name { get; set; }
    public string? ConfigJson { get; set; }
}
