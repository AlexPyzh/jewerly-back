namespace JewerlyBack.Infrastructure.Ai.Configuration;

/// <summary>
/// Настройки для AI Preview сервиса.
/// Управляет лимитами генерации AI превью для разных типов пользователей.
/// </summary>
public sealed class AiPreviewOptions
{
    /// <summary>
    /// Имя секции в appsettings.json
    /// </summary>
    public const string SectionName = "AiPreview";

    /// <summary>
    /// Лимит бесплатных AI превью для гостей (неавторизованных пользователей).
    ///
    /// Значения:
    /// - 0: лимит отключён (неограниченное количество превью для гостей) - используется в Development
    /// - >0: максимальное количество превью для гостя - используется в Production
    ///
    /// По умолчанию: 5 (Production лимит)
    /// </summary>
    public int GuestFreePreviewLimit { get; set; } = 5;
}
