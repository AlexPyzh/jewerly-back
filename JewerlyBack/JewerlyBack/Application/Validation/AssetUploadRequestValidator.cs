using FluentValidation;
using JewerlyBack.Dto;

namespace JewerlyBack.Application.Validation;

/// <summary>
/// Валидатор для запроса загрузки ассета (файла)
/// </summary>
public class AssetUploadRequestValidator : AbstractValidator<AssetUploadRequest>
{
    private static readonly string[] AllowedFileTypes = new[]
    {
        "image",
        "pattern",
        "texture",
        "reference",
        "pattern_png",
        "reference_image"
    };

    private static readonly string[] AllowedExtensions = new[]
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".glb", ".gltf", ".obj", ".fbx"
    };

    // Максимальный размер файла: 50 МБ
    private const long MaxFileSizeBytes = 50 * 1024 * 1024;

    public AssetUploadRequestValidator()
    {
        // File валидация
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        // Размер файла
        RuleFor(x => x.File.Length)
            .GreaterThan(0)
            .WithMessage("File must not be empty")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"File size must not exceed {MaxFileSizeBytes / 1024 / 1024} MB")
            .When(x => x.File != null);

        // Расширение файла
        RuleFor(x => x.File.FileName)
            .Must(HasAllowedExtension)
            .WithMessage($"File extension must be one of: {string.Join(", ", AllowedExtensions)}")
            .When(x => x.File != null && !string.IsNullOrEmpty(x.File.FileName));

        // FileType валидация
        RuleFor(x => x.FileType)
            .NotEmpty()
            .WithMessage("FileType is required")
            .Must(BeValidFileType)
            .WithMessage($"FileType must be one of: {string.Join(", ", AllowedFileTypes)}");

        // ConfigurationId (опционально)
        RuleFor(x => x.ConfigurationId)
            .NotEmpty()
            .WithMessage("ConfigurationId must not be empty if provided")
            .When(x => x.ConfigurationId.HasValue);
    }

    /// <summary>
    /// Проверяет, что тип файла входит в список разрешённых
    /// </summary>
    private bool BeValidFileType(string fileType)
    {
        return AllowedFileTypes.Contains(fileType, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Проверяет, что расширение файла входит в список разрешённых
    /// </summary>
    private bool HasAllowedExtension(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }
}
