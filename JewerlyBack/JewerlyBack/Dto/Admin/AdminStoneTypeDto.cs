namespace JewerlyBack.Dto.Admin;

/// <summary>
/// Admin-specific DTO for stone type with all fields including IsActive
/// </summary>
public class AdminStoneTypeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public decimal DefaultPricePerCarat { get; set; }
    public bool IsActive { get; set; }
}
