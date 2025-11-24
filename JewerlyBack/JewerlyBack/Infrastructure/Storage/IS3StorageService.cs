namespace JewerlyBack.Infrastructure.Storage;

/// <summary>
/// Интерфейс для работы с S3-совместимым хранилищем файлов.
/// Абстрагирует взаимодействие с конкретным S3-провайдером (AWS, Contabo, MinIO и т.д.)
/// </summary>
public interface IS3StorageService
{
    /// <summary>
    /// Загружает файл в S3 бакет
    /// </summary>
    /// <param name="stream">Поток данных файла</param>
    /// <param name="fileKey">Уникальный ключ файла в бакете (путь)</param>
    /// <param name="contentType">MIME-тип файла</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Публичный URL загруженного файла</returns>
    Task<string> UploadAsync(Stream stream, string fileKey, string contentType, CancellationToken ct = default);

    /// <summary>
    /// Удаляет файл из S3 бакета
    /// </summary>
    /// <param name="fileKey">Ключ файла в бакете</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>true если файл успешно удалён, false если файл не найден</returns>
    Task<bool> DeleteAsync(string fileKey, CancellationToken ct = default);

    /// <summary>
    /// Проверяет существование файла в бакете
    /// </summary>
    /// <param name="fileKey">Ключ файла в бакете</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>true если файл существует</returns>
    Task<bool> ExistsAsync(string fileKey, CancellationToken ct = default);

    /// <summary>
    /// Генерирует presigned URL для временного доступа к файлу.
    /// Используется для приватных бакетов.
    /// </summary>
    /// <param name="fileKey">Ключ файла в бакете</param>
    /// <param name="expiresIn">Время жизни URL</param>
    /// <returns>Временный URL с подписью</returns>
    /// <remarks>
    /// TODO: Реализовать после перехода на приватный бакет.
    /// Сейчас возвращает публичный URL.
    /// </remarks>
    string GetPresignedUrl(string fileKey, TimeSpan expiresIn);

    /// <summary>
    /// Возвращает публичный URL для файла.
    /// Актуально пока бакет публичный.
    /// </summary>
    /// <param name="fileKey">Ключ файла в бакете</param>
    /// <returns>Публичный URL файла</returns>
    string GetPublicUrl(string fileKey);
}
