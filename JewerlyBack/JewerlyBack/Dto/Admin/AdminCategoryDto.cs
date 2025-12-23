namespace JewerlyBack.Dto.Admin;

/// <summary>
/// Admin-specific DTO for jewelry category with all fields including IsActive
/// </summary>
public class AdminCategoryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AiCategoryDescription { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
}
