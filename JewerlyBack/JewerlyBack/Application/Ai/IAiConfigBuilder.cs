using JewerlyBack.Application.Ai.Models;

namespace JewerlyBack.Application.Ai;

/// <summary>
/// Сервис для построения семантической AI-конфигурации из сырых данных конфигурации.
/// Преобразует ID/коды в человеко-читаемый формат для передачи в AI-генератор.
/// </summary>
public interface IAiConfigBuilder
{
    /// <summary>
    /// Строит семантическую AI-конфигурацию для указанной конфигурации ювелирного изделия.
    /// </summary>
    /// <param name="configurationId">ID конфигурации ювелирного изделия.</param>
    /// <param name="userId">ID пользователя (null для гостей). Используется для проверки прав доступа.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Семантическая модель конфигурации для AI.</returns>
    /// <exception cref="ArgumentException">Если конфигурация не найдена.</exception>
    /// <exception cref="UnauthorizedAccessException">Если конфигурация не принадлежит пользователю.</exception>
    /// <remarks>
    /// Метод загружает конфигурацию со всеми связанными сущностями (Category, BaseModel, Material, Stones)
    /// и преобразует их в семантический формат, понятный для AI-модели.
    /// Для гостевых конфигураций (userId == null и configuration.UserId == null) доступ разрешается.
    /// </remarks>
    Task<AiConfigDto> BuildForConfigurationAsync(
        Guid configurationId,
        Guid? userId,
        CancellationToken ct = default);
}
