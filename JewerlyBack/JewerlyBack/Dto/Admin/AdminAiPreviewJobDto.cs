using JewerlyBack.Entities;
using JewerlyBack.Models;

namespace JewerlyBack.Dto.Admin;

/// <summary>
/// Admin DTO for AI preview job with full details
/// </summary>
public class AdminAiPreviewJobDto
{
    public Guid Id { get; set; }
    public Guid ConfigurationId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? GuestClientId { get; set; }
    public AiPreviewType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public AiPreviewStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? Prompt { get; set; }
    public string? AiConfigJson { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SingleImageUrl { get; set; }
    public string? FramesJson { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
