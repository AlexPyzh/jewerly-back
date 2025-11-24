namespace JewerlyBack.Dto;

public class StoneTypeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public decimal DefaultPricePerCarat { get; set; }
}
