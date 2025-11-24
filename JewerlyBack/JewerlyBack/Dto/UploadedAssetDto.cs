namespace JewerlyBack.Dto;

public class UploadedAssetDto
{
    public Guid Id { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
