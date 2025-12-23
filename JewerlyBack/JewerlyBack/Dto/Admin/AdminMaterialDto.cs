namespace JewerlyBack.Dto.Admin;

/// <summary>
/// Admin-specific DTO for material with all fields including IsActive
/// </summary>
public class AdminMaterialDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MetalType { get; set; } = string.Empty;
    public int? Karat { get; set; }
    public string? ColorHex { get; set; }
    public decimal PriceFactor { get; set; }
    public bool IsActive { get; set; }
}
