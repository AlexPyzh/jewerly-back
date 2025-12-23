namespace JewerlyBack.Dto.Admin;

/// <summary>
/// Admin-specific DTO for jewelry base model with all fields including IsActive
/// </summary>
public class AdminBaseModelDto
{
    public Guid Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AiDescription { get; set; }
    public string? PreviewImageUrl { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
    public string? MetadataJson { get; set; }
}
