namespace JewerlyBack.Infrastructure.Configuration;

/// <summary>
/// Конфигурация CORS для приложения
/// </summary>
public class CorsOptions
{
    public const string SectionName = "Cors";

    /// <summary>
    /// Список разрешённых origin'ов для CORS
    /// </summary>
    /// <remarks>
    /// В Development: localhost-адреса для локальной разработки
    /// В Production: должны быть заменены на реальные домены (TODO)
    /// </remarks>
    public string[] AllowedOrigins { get; init; } = Array.Empty<string>();
}
