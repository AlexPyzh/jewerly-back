using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Dto.Admin;

/// <summary>
/// Request to create a new jewelry category
/// </summary>
public class AdminCategoryCreateRequest
{
    /// <summary>
    /// Unique category code (e.g., "rings", "earrings")
    /// </summary>
    [Required(ErrorMessage = "Code is required")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the category
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// AI-specific description for generating images
    /// </summary>
    [StringLength(1000, ErrorMessage = "AI description cannot exceed 1000 characters")]
    public string? AiCategoryDescription { get; set; }

    /// <summary>
    /// Whether the category is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}
