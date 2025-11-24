using JewerlyBack.Application.Models;
using JewerlyBack.Dto;
using Microsoft.AspNetCore.Http;

namespace JewerlyBack.Application.Interfaces;

/// <summary>
/// Сервис для работы с загруженными файлами и медиа-ресурсами.
/// Отвечает за бизнес-логику управления ассетами (валидация, хранение метаданных, связь с конфигурациями).
/// </summary>
public interface IAssetService
{
    /// <summary>
    /// Получить пагинированный список ассетов пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="pagination">Параметры пагинации</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Пагинированный список ассетов пользователя</returns>
    Task<PagedResult<UploadedAssetDto>> GetUserAssetsAsync(
        Guid userId,
        PaginationQuery pagination,
        CancellationToken ct = default);

    /// <summary>
    /// Получить информацию о конкретном ассете
    /// </summary>
    /// <param name="userId">ID пользователя (для проверки доступа)</param>
    /// <param name="assetId">ID ассета</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Информация об ассете или null если не найден/нет доступа</returns>
    Task<UploadedAssetDto?> GetAssetAsync(Guid userId, Guid assetId, CancellationToken ct = default);

    /// <summary>
    /// Загрузить новый ассет
    /// </summary>
    /// <param name="userId">ID пользователя-владельца</param>
    /// <param name="file">Загружаемый файл</param>
    /// <param name="fileType">Тип файла (image, pattern, model3d и т.д.)</param>
    /// <param name="configurationId">Опциональная привязка к конфигурации</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>ID созданного ассета</returns>
    /// <exception cref="ArgumentException">Если файл не прошёл валидацию</exception>
    Task<Guid> UploadAssetAsync(
        Guid userId,
        IFormFile file,
        string fileType,
        Guid? configurationId,
        CancellationToken ct = default);

    /// <summary>
    /// Удалить ассет
    /// </summary>
    /// <param name="userId">ID пользователя (для проверки доступа)</param>
    /// <param name="assetId">ID ассета</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>true если ассет успешно удалён, false если не найден/нет доступа</returns>
    Task<bool> DeleteAssetAsync(Guid userId, Guid assetId, CancellationToken ct = default);

    /// <summary>
    /// Привязать ассет к конфигурации
    /// </summary>
    /// <param name="userId">ID пользователя (для проверки доступа)</param>
    /// <param name="assetId">ID ассета</param>
    /// <param name="configurationId">ID конфигурации</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>true если операция успешна</returns>
    Task<bool> AttachToConfigurationAsync(
        Guid userId,
        Guid assetId,
        Guid configurationId,
        CancellationToken ct = default);
}
