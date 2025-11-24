using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace JewerlyBack.Infrastructure.Storage;

/// <summary>
/// Реализация сервиса для работы с S3-совместимым хранилищем (Contabo Object Storage).
/// </summary>
/// <remarks>
/// Особенности Contabo S3:
/// - Требуется ForcePathStyle = true
/// - Bucket name включает идентификатор: "bucketId:bucketName"
/// - Endpoint: https://usc1.contabostorage.com (US Central)
///
/// Безопасность:
/// - Сейчас бакет публичный, возвращаем прямые URL
/// - TODO: После настройки приватного доступа использовать presigned URLs
/// </remarks>
public sealed class S3StorageService : IS3StorageService, IDisposable
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _options;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(IAmazonS3 s3Client, IOptions<S3Options> options, ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(Stream stream, string fileKey, string contentType, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = fileKey,
            InputStream = stream,
            ContentType = contentType,
            // Для публичного бакета. После перехода на приватный — убрать.
            // TODO: Убрать CannedACL после настройки приватного бакета
            CannedACL = S3CannedACL.PublicRead,
            DisablePayloadSigning = true // Требуется для некоторых S3-совместимых хостингов
        };

        try
        {
            var response = await _s3Client.PutObjectAsync(request, ct);

            _logger.LogInformation(
                "File uploaded to S3: {FileKey}, ETag: {ETag}, Status: {StatusCode}",
                fileKey, response.ETag, response.HttpStatusCode);

            return GetPublicUrl(fileKey);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex,
                "S3 upload failed: {FileKey}, Error: {ErrorCode}, Message: {Message}",
                fileKey, ex.ErrorCode, ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string fileKey, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileKey);

        var request = new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = fileKey
        };

        try
        {
            var response = await _s3Client.DeleteObjectAsync(request, ct);

            _logger.LogInformation(
                "File deleted from S3: {FileKey}, Status: {StatusCode}",
                fileKey, response.HttpStatusCode);

            // S3 возвращает 204 даже если файл не существовал
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
        {
            _logger.LogWarning("File not found in S3 for deletion: {FileKey}", fileKey);
            return false;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex,
                "S3 delete failed: {FileKey}, Error: {ErrorCode}",
                fileKey, ex.ErrorCode);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string fileKey, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileKey);

        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _options.BucketName,
                Key = fileKey
            };

            await _s3Client.GetObjectMetadataAsync(request, ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public string GetPresignedUrl(string fileKey, TimeSpan expiresIn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileKey);

        // TODO: Реализовать после перехода на приватный бакет
        // Сейчас возвращаем публичный URL, т.к. бакет открыт

        // Пример реализации presigned URL (раскомментировать после настройки приватного бакета):
        /*
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = fileKey,
            Expires = DateTime.UtcNow.Add(expiresIn),
            Verb = HttpVerb.GET
        };
        return _s3Client.GetPreSignedURL(request);
        */

        _logger.LogDebug(
            "GetPresignedUrl called but bucket is public. Returning public URL. FileKey: {FileKey}",
            fileKey);

        return GetPublicUrl(fileKey);
    }

    /// <inheritdoc />
    public string GetPublicUrl(string fileKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileKey);

        // Формат URL для Contabo Object Storage:
        // https://usc1.contabostorage.com/bucketId:bucketName/fileKey
        var url = $"{_options.ServiceUrl.TrimEnd('/')}/{_options.BucketName}/{fileKey}";

        return url;
    }

    public void Dispose()
    {
        _s3Client.Dispose();
    }
}
