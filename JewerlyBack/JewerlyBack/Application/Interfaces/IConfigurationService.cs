using JewerlyBack.Dto;

namespace JewerlyBack.Application.Interfaces;

/// <summary>
/// Сервис для работы с конфигурациями ювелирных изделий
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Получить список конфигураций пользователя
    /// </summary>
    Task<IReadOnlyList<JewelryConfigurationListItemDto>> GetUserConfigurationsAsync(
        Guid userId,
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
}
