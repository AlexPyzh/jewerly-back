namespace JewerlyBack.Infrastructure.Storage;

/// <summary>
/// Конфигурация для S3-совместимого хранилища.
/// Маппится из секции "S3" в appsettings.json
/// </summary>
public class S3Options
{
    /// <summary>
    /// Название секции в конфигурации
    /// </summary>
    public const string SectionName = "S3";

    /// <summary>
    /// URL S3-совместимого endpoint (например, https://usc1.contabostorage.com)
    /// </summary>
    public required string ServiceUrl { get; init; }

    /// <summary>
    /// Имя бакета (для Contabo включает идентификатор: "bucketId:bucketName")
    /// </summary>
    public required string BucketName { get; init; }

    /// <summary>
    /// Ключ доступа (Access Key ID)
    /// </summary>
    public required string AccessKey { get; init; }

    /// <summary>
    /// Секретный ключ (Secret Access Key)
    /// </summary>
    public required string SecretKey { get; init; }

    /// <summary>
    /// Использовать path-style URLs вместо virtual-hosted style.
    /// Требуется для большинства S3-совместимых хостингов (Contabo, MinIO и т.д.)
    /// </summary>
    public bool ForcePathStyle { get; init; } = true;

    /// <summary>
    /// Регион S3. Для Contabo можно использовать любой (например, "us-east-1").
    /// </summary>
    public string Region { get; init; } = "us-east-1";
}
