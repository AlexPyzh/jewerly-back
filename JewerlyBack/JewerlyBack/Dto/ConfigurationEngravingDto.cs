namespace JewerlyBack.Dto;

public class ConfigurationEngravingDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? FontName { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal? SizeMm { get; set; }
    public bool IsInternal { get; set; }
}
