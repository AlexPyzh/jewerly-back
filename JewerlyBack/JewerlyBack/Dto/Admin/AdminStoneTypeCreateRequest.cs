using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Dto.Admin;

/// <summary>
/// Request to create a new stone type
/// </summary>
public class AdminStoneTypeCreateRequest
{
    /// <summary>
    /// Unique stone type code (e.g., "diamond", "ruby", "sapphire")
    /// </summary>
    [Required(ErrorMessage = "Code is required")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the stone type
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Stone color description (e.g., "Clear", "Red", "Blue")
    /// </summary>
    [StringLength(50, ErrorMessage = "Color cannot exceed 50 characters")]
    public string? Color { get; set; }

    /// <summary>
    /// Default price per carat for this stone type
    /// </summary>
    [Required(ErrorMessage = "Default price per carat is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Default price per carat must be non-negative")]
    public decimal DefaultPricePerCarat { get; set; }

    /// <summary>
    /// Whether the stone type is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}
