namespace JewerlyBack.Infrastructure.Ai.Configuration;

/// <summary>
/// Настройки для интеграции с OpenAI API.
///
/// ВАЖНО: ApiKey НЕ должен храниться в appsettings.json!
/// ApiKey автоматически загружается из переменной окружения OPENAI_API_KEY.
///
/// Установка ключа:
/// - Переменная окружения: OPENAI_API_KEY=sk-...
/// - Docker/Heroku/Render/GitHub Actions: установите OPENAI_API_KEY в environment variables
/// - User-secrets (для разработки): dotnet user-secrets set "Ai:OpenAi:ApiKey" "sk-..." (устарело)
///
/// Валидация ApiKey происходит при старте приложения (ValidateOnStart).
/// Если OPENAI_API_KEY не установлен, приложение не запустится.
/// </summary>
public sealed class OpenAiOptions
{
    /// <summary>
    /// Имя секции в appsettings.json
    /// </summary>
    public const string SectionName = "Ai:OpenAi";

    /// <summary>
    /// API-ключ для доступа к OpenAI API.
    /// Загружается из переменной окружения OPENAI_API_KEY.
    /// Значение из appsettings.json игнорируется и перезаписывается при старте приложения.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Модель для генерации изображений (например, "dall-e-3").
    /// </summary>
    public string Model { get; init; } = "dall-e-3";

    /// <summary>
    /// Базовый URL для OpenAI API.
    /// </summary>
    public string BaseUrl { get; init; } = "https://api.openai.com/v1";

    /// <summary>
    /// Таймаут для HTTP-запросов к OpenAI API в секундах.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 120;

    /// <summary>
    /// Максимальное количество повторных попыток при ошибках.
    /// </summary>
    public int MaxRetryAttempts { get; init; } = 3;
}
