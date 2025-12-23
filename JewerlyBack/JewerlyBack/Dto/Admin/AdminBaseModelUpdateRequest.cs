using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Dto.Admin;

/// <summary>
/// Request to update an existing jewelry base model
/// </summary>
public class AdminBaseModelUpdateRequest
{
    /// <summary>
    /// Category ID for the base model
    /// </summary>
    [Required(ErrorMessage = "Category ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be a positive number")]
    public int CategoryId { get; set; }

    /// <summary>
    /// Display name for the base model
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique base model code (e.g., "ring-001", "earring-classic")
    /// </summary>
    [Required(ErrorMessage = "Code is required")]
    [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Base model description
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// AI-specific description for generating images
    /// </summary>
    [StringLength(2000, ErrorMessage = "AI description cannot exceed 2000 characters")]
    public string? AiDescription { get; set; }

    /// <summary>
    /// Preview image URL
    /// </summary>
    [StringLength(500, ErrorMessage = "Preview image URL cannot exceed 500 characters")]
    public string? PreviewImageUrl { get; set; }

    /// <summary>
    /// Base price for the model
    /// </summary>
    [Required(ErrorMessage = "Base price is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Base price must be non-negative")]
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Whether the base model is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Additional metadata in JSON format
    /// </summary>
    [StringLength(5000, ErrorMessage = "Metadata JSON cannot exceed 5000 characters")]
    public string? MetadataJson { get; set; }
}
