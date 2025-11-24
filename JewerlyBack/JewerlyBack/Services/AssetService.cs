using AutoMapper;
using JewerlyBack.Application.Interfaces;
using JewerlyBack.Data;
using JewerlyBack.Dto;
using JewerlyBack.Infrastructure.Storage;
using JewerlyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace JewerlyBack.Services;

/// <summary>
/// Реализация сервиса для работы с загруженными файлами и медиа-ресурсами.
/// </summary>
/// <remarks>
/// Архитектура:
/// - Бизнес-логика (валидация, метаданные) — здесь
/// - IO/S3 операции — делегируются в IS3StorageService
/// - EF операции — изолированы в методах этого сервиса
///
/// Безопасность:
/// - Все операции проверяют принадлежность ассета пользователю
/// - Валидация типов и размеров файлов
/// - Файлы хранятся с уникальными именами (предотвращает перезапись)
/// </remarks>
public class AssetService : IAssetService
{
    private readonly AppDbContext _context;
    private readonly IS3StorageService _storageService;
    private readonly IMapper _mapper;
    private readonly ILogger<AssetService> _logger;

    // Конфигурация валидации файлов
    // TODO: Вынести в appsettings.json для production
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/svg+xml"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"
    };

    // TODO: Настроить для production (рекомендуется 5-10 MB для изображений)
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private static readonly HashSet<string> AllowedFileTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image",
        "pattern",
        "texture",
        "reference"
    };

    public AssetService(
        AppDbContext context,
        IS3StorageService storageService,
        IMapper mapper,
        ILogger<AssetService> logger)
    {
        _context = context;
        _storageService = storageService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UploadedAssetDto>> GetUserAssetsAsync(Guid userId, CancellationToken ct = default)
    {
        var assets = await _context.UploadedAssets
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        return _mapper.Map<List<UploadedAssetDto>>(assets);
    }

    /// <inheritdoc />
    public async Task<UploadedAssetDto?> GetAssetAsync(Guid userId, Guid assetId, CancellationToken ct = default)
    {
        var asset = await _context.UploadedAssets
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == assetId && a.UserId == userId, ct);

        return asset is null ? null : _mapper.Map<UploadedAssetDto>(asset);
    }

    /// <inheritdoc />
    public async Task<Guid> UploadAssetAsync(
        Guid userId,
        IFormFile file,
        string fileType,
        Guid? configurationId,
        CancellationToken ct = default)
    {
        // 1. Валидация входных данных
        ValidateFile(file);
        ValidateFileType(fileType);

        // 2. Валидация конфигурации (если указана)
        if (configurationId.HasValue)
        {
            var configExists = await _context.JewelryConfigurations
                .AnyAsync(c => c.Id == configurationId.Value && c.UserId == userId, ct);

            if (!configExists)
            {
                throw new ArgumentException($"Configuration {configurationId} not found or access denied");
            }
        }

        // 3. Генерация уникального пути в S3
        var fileKey = GenerateFileKey(userId, file.FileName);
        var contentType = file.ContentType;

        // 4. Загрузка в S3
        string fileUrl;
        await using (var stream = file.OpenReadStream())
        {
            fileUrl = await _storageService.UploadAsync(stream, fileKey, contentType, ct);
        }

        // 5. Сохранение метаданных в БД
        var asset = new UploadedAsset
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConfigurationId = configurationId,
            FileType = fileType.ToLowerInvariant(),
            Url = fileUrl,
            OriginalFileName = SanitizeFileName(file.FileName),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.UploadedAssets.Add(asset);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Asset uploaded: {AssetId} by user {UserId}, Type: {FileType}, Key: {FileKey}",
            asset.Id, userId, fileType, fileKey);

        return asset.Id;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAssetAsync(Guid userId, Guid assetId, CancellationToken ct = default)
    {
        var asset = await _context.UploadedAssets
            .FirstOrDefaultAsync(a => a.Id == assetId && a.UserId == userId, ct);

        if (asset is null)
        {
            _logger.LogWarning("Asset {AssetId} not found or access denied for user {UserId}", assetId, userId);
            return false;
        }

        // Извлекаем fileKey из URL для удаления из S3
        var fileKey = ExtractFileKeyFromUrl(asset.Url);

        // Удаляем из S3 (не критично если не удалится — файл может быть удалён вручную)
        try
        {
            await _storageService.DeleteAsync(fileKey, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file from S3: {FileKey}. Proceeding with DB deletion.", fileKey);
        }

        // Удаляем запись из БД
        _context.UploadedAssets.Remove(asset);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Asset deleted: {AssetId} by user {UserId}", assetId, userId);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> AttachToConfigurationAsync(
        Guid userId,
        Guid assetId,
        Guid configurationId,
        CancellationToken ct = default)
    {
        var asset = await _context.UploadedAssets
            .FirstOrDefaultAsync(a => a.Id == assetId && a.UserId == userId, ct);

        if (asset is null)
        {
            return false;
        }

        var configExists = await _context.JewelryConfigurations
            .AnyAsync(c => c.Id == configurationId && c.UserId == userId, ct);

        if (!configExists)
        {
            return false;
        }

        asset.ConfigurationId = configurationId;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Asset {AssetId} attached to configuration {ConfigurationId}",
            assetId, configurationId);

        return true;
    }

    /// <summary>
    /// Валидация загружаемого файла
    /// </summary>
    private static void ValidateFile(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("File is required and cannot be empty");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException($"File size exceeds maximum allowed ({MaxFileSizeBytes / 1024 / 1024} MB)");
        }

        // Проверка Content-Type
        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            throw new ArgumentException($"File type '{file.ContentType}' is not allowed. Allowed types: {string.Join(", ", AllowedContentTypes)}");
        }

        // Проверка расширения файла
        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", AllowedExtensions)}");
        }
    }

    /// <summary>
    /// Валидация типа ассета (бизнес-категория)
    /// </summary>
    private static void ValidateFileType(string fileType)
    {
        if (string.IsNullOrWhiteSpace(fileType))
        {
            throw new ArgumentException("File type is required");
        }

        if (!AllowedFileTypes.Contains(fileType))
        {
            throw new ArgumentException($"File type '{fileType}' is not allowed. Allowed types: {string.Join(", ", AllowedFileTypes)}");
        }
    }

    /// <summary>
    /// Генерация уникального ключа файла в S3.
    /// Формат: assets/{userId}/{yyyy}/{MM}/{guid}.{ext}
    /// </summary>
    private static string GenerateFileKey(Guid userId, string originalFileName)
    {
        var now = DateTimeOffset.UtcNow;
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var uniqueId = Guid.NewGuid();

        return $"assets/{userId}/{now:yyyy}/{now:MM}/{uniqueId}{extension}";
    }

    /// <summary>
    /// Санитизация имени файла (удаление опасных символов)
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "unnamed";
        }

        // Убираем путь, оставляем только имя файла
        fileName = Path.GetFileName(fileName);

        // Заменяем опасные символы
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            fileName = fileName.Replace(c, '_');
        }

        // Ограничиваем длину
        if (fileName.Length > 200)
        {
            var ext = Path.GetExtension(fileName);
            fileName = fileName[..(200 - ext.Length)] + ext;
        }

        return fileName;
    }

    /// <summary>
    /// Извлекает fileKey из полного URL для операций удаления
    /// </summary>
    private static string ExtractFileKeyFromUrl(string url)
    {
        // URL формат: https://domain/bucket/assets/userId/yyyy/MM/file.ext
        // Нужно извлечь: assets/userId/yyyy/MM/file.ext

        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        const string assetsPrefix = "assets/";
        var index = url.IndexOf(assetsPrefix, StringComparison.OrdinalIgnoreCase);

        return index >= 0 ? url[index..] : url;
    }
}
