using System.Diagnostics;
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

        // Log configuration on startup
        Console.WriteLine();
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ 💾 S3 Storage Service Configuration                         │");
        Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
        Console.WriteLine($"│ Service URL:   {_options.ServiceUrl,-44}│");
        Console.WriteLine($"│ Bucket Name:   {_options.BucketName,-44}│");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
        Console.WriteLine();
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(Stream stream, string fileKey, string contentType, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        // Get stream length for logging
        var streamLength = stream.CanSeek ? stream.Length : -1;

        Console.WriteLine();
        Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
        Console.WriteLine("   │ 📤 S3 Upload Operation                                  │");
        Console.WriteLine("   ├─────────────────────────────────────────────────────────┤");
        Console.WriteLine($"   │ Bucket:       {_options.BucketName,-40}│");
        Console.WriteLine($"   │ Key:          {(fileKey.Length > 40 ? fileKey[..37] + "..." : fileKey),-40}│");
        Console.WriteLine($"   │ Content Type: {contentType,-40}│");
        if (streamLength >= 0)
        {
            Console.WriteLine($"   │ File Size:    {streamLength:N0} bytes ({streamLength / 1024.0:F1} KB){new string(' ', 20)}│");
        }
        Console.WriteLine($"   │ ACL:          PublicRead{new string(' ', 30)}│");
        Console.WriteLine("   └─────────────────────────────────────────────────────────┘");

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = fileKey,
            InputStream = stream,
            ContentType = contentType,
            // Для публичного бакета. После перехода на приватный — убрать.
            // TODO: Убрать CannedACL после настройки приватного бакета
            CannedACL = S3CannedACL.PublicRead
        };

        var uploadStopwatch = Stopwatch.StartNew();

        try
        {
            Console.WriteLine("   📤 Uploading to S3...");

            var response = await _s3Client.PutObjectAsync(request, ct);

            uploadStopwatch.Stop();

            Console.WriteLine($"   ✓ Upload completed in {uploadStopwatch.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"   ✓ HTTP Status: {(int)response.HttpStatusCode} {response.HttpStatusCode}");
            Console.WriteLine($"   ✓ ETag: {response.ETag}");

            var publicUrl = GetPublicUrl(fileKey);
            Console.WriteLine($"   ✓ Public URL: {publicUrl}");

            _logger.LogInformation(
                "✅ File uploaded to S3: {FileKey}, ETag: {ETag}, Status: {StatusCode}, Duration: {Duration}s",
                fileKey, response.ETag, response.HttpStatusCode, uploadStopwatch.Elapsed.TotalSeconds);

            return publicUrl;
        }
        catch (AmazonS3Exception ex)
        {
            uploadStopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("   │ ❌ S3 Upload Failed                                     │");
            Console.WriteLine("   ├─────────────────────────────────────────────────────────┤");
            Console.WriteLine($"   │ Error Code:   {ex.ErrorCode,-40}│");
            Console.WriteLine($"   │ Status Code:  {(int)ex.StatusCode} {ex.StatusCode,-32}│");
            Console.WriteLine($"   │ Request ID:   {ex.RequestId,-40}│");
            var errorMsg = ex.Message.Length > 40 ? ex.Message[..37] + "..." : ex.Message;
            Console.WriteLine($"   │ Message:      {errorMsg,-40}│");
            Console.WriteLine($"   │ Duration:     {uploadStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 36)}│");
            Console.WriteLine("   └─────────────────────────────────────────────────────────┘");

            _logger.LogError(ex,
                "❌ S3 upload failed: {FileKey}, Error: {ErrorCode}, StatusCode: {StatusCode}, Message: {Message}, RequestId: {RequestId}, Duration: {Duration}s",
                fileKey, ex.ErrorCode, ex.StatusCode, ex.Message, ex.RequestId, uploadStopwatch.Elapsed.TotalSeconds);
            throw;
        }
        catch (Exception ex)
        {
            uploadStopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("   │ ❌ S3 Upload Failed (Non-S3 Error)                      │");
            Console.WriteLine("   ├─────────────────────────────────────────────────────────┤");
            Console.WriteLine($"   │ Error Type:   {ex.GetType().Name,-40}│");
            var errorMsg = ex.Message.Length > 40 ? ex.Message[..37] + "..." : ex.Message;
            Console.WriteLine($"   │ Message:      {errorMsg,-40}│");
            Console.WriteLine($"   │ Duration:     {uploadStopwatch.Elapsed.TotalSeconds:F2}s{new string(' ', 36)}│");
            Console.WriteLine("   └─────────────────────────────────────────────────────────┘");

            _logger.LogError(ex,
                "❌ S3 upload failed with non-S3 error: {FileKey}, Error: {Error}, Duration: {Duration}s",
                fileKey, ex.Message, uploadStopwatch.Elapsed.TotalSeconds);
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
        // Для публичных URL нужно использовать полный bucket ID
        var fullBucketName = "b6dff85a0bf0428f9df1725ed460985b:jewbucket";
        var url = $"{_options.ServiceUrl.TrimEnd('/')}/{fullBucketName}/{fileKey}";

        return url;
    }

    public void Dispose()
    {
        _s3Client.Dispose();
    }
}
