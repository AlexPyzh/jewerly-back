using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Dto.Admin;

/// <summary>
/// Request to update an existing material
/// </summary>
public class AdminMaterialUpdateRequest
{
    /// <summary>
    /// Unique material code (e.g., "gold-18k", "silver-925")
    /// </summary>
    [Required(ErrorMessage = "Code is required")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the material
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of metal (e.g., "Gold", "Silver", "Platinum")
    /// </summary>
    [Required(ErrorMessage = "Metal type is required")]
    [StringLength(50, ErrorMessage = "Metal type cannot exceed 50 characters")]
    public string MetalType { get; set; } = string.Empty;

    /// <summary>
    /// Karat value for gold (e.g., 14, 18, 24)
    /// </summary>
    [Range(1, 24, ErrorMessage = "Karat must be between 1 and 24")]
    public int? Karat { get; set; }

    /// <summary>
    /// Color in hexadecimal format (e.g., "#FFD700" for gold)
    /// </summary>
    [StringLength(7, ErrorMessage = "Color hex cannot exceed 7 characters")]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color hex must be in format #RRGGBB")]
    public string? ColorHex { get; set; }

    /// <summary>
    /// Price multiplier factor for this material
    /// </summary>
    [Required(ErrorMessage = "Price factor is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Price factor must be non-negative")]
    public decimal PriceFactor { get; set; }

    /// <summary>
    /// Whether the material is active
    /// </summary>
    public bool IsActive { get; set; }
}
