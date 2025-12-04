using JewerlyBack.Application.Models;
using JewerlyBack.Dto;

namespace JewerlyBack.Application.Interfaces;

/// <summary>
/// Сервис для работы с конфигурациями ювелирных изделий
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Получить пагинированный список конфигураций пользователя
    /// </summary>
    Task<PagedResult<JewelryConfigurationListItemDto>> GetUserConfigurationsAsync(
        Guid userId,
        PaginationQuery pagination,
        CancellationToken ct = default);

    /// <summary>
    /// Получить детальную информацию о конфигурации
    /// </summary>
    Task<JewelryConfigurationDetailDto?> GetConfigurationByIdAsync(
        Guid userId,
        Guid configurationId,
        CancellationToken ct = default);

    /// <summary>
    /// Создать новую конфигурацию
    /// </summary>
    Task<Guid> CreateConfigurationAsync(
        Guid userId,
        JewelryConfigurationCreateRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Обновить существующую конфигурацию
    /// </summary>
    Task<bool> UpdateConfigurationAsync(
        Guid userId,
        Guid configurationId,
        JewelryConfigurationUpdateRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Удалить конфигурацию
    /// </summary>
    Task<bool> DeleteConfigurationAsync(
        Guid userId,
        Guid configurationId,
        CancellationToken ct = default);

    /// <summary>
    /// Получить последние конфигурации пользователя
    /// </summary>
    Task<IReadOnlyList<JewelryConfigurationSummaryDto>> GetRecentForUserAsync(
        Guid userId,
        int take,
        CancellationToken ct = default);
}
