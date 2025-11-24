namespace JewerlyBack.Infrastructure.Auth;

/// <summary>
/// Конфигурация JWT-аутентификации.
/// Секция "Auth" в appsettings.json
/// </summary>
/// <remarks>
/// БЕЗОПАСНОСТЬ: JwtKey должен храниться в Secret Manager / ENV переменных в production.
/// Минимальная длина ключа — 32 символа (256 бит) для HMAC-SHA256.
/// </remarks>
public class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>
    /// Издатель токена (iss claim)
    /// </summary>
    public required string JwtIssuer { get; init; }

    /// <summary>
    /// Аудитория токена (aud claim)
    /// </summary>
    public required string JwtAudience { get; init; }

    /// <summary>
    /// Секретный ключ для подписи токенов (минимум 32 символа)
    /// </summary>
    /// <remarks>
    /// В production использовать: Environment.GetEnvironmentVariable("JWT_KEY")
    /// или Azure Key Vault / AWS Secrets Manager
    /// </remarks>
    public required string JwtKey { get; init; }

    /// <summary>
    /// Время жизни access токена в минутах
    /// </summary>
    public int TokenLifetimeMinutes { get; init; } = 60;

    /// <summary>
    /// Время жизни refresh токена в днях (для будущей реализации)
    /// </summary>
    public int RefreshTokenLifetimeDays { get; init; } = 30;
}

/// <summary>
/// Конфигурация Google Sign-In
/// </summary>
public class GoogleAuthOptions
{
    public const string SectionName = "GoogleAuth";

    /// <summary>
    /// Client ID приложения в Google Cloud Console
    /// </summary>
    /// <remarks>
    /// Для мобильных приложений используется Web Client ID или iOS/Android Client ID
    /// в зависимости от настроек Sign-In
    /// </remarks>
    public required string ClientId { get; init; }

    /// <summary>
    /// Issuer для валидации токена (обычно https://accounts.google.com)
    /// </summary>
    public string Issuer { get; init; } = "https://accounts.google.com";
}

/// <summary>
/// Конфигурация Apple Sign-In
/// </summary>
/// <remarks>
/// Для полноценной валидации токенов Apple требуется:
/// - Service ID (ClientId)
/// - Team ID
/// - Key ID и приватный ключ для генерации client_secret
/// На данном этапе реализована валидация публичных ключей Apple.
/// </remarks>
public class AppleAuthOptions
{
    public const string SectionName = "AppleAuth";

    /// <summary>
    /// Service ID (Bundle ID для iOS приложения)
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Team ID разработчика Apple
    /// </summary>
    /// <remarks>
    /// Используется для генерации client_secret при серверной валидации.
    /// TODO: Реализовать генерацию client_secret для полного flow.
    /// </remarks>
    public string? TeamId { get; init; }

    /// <summary>
    /// Key ID для Sign in with Apple
    /// </summary>
    public string? KeyId { get; init; }

    /// <summary>
    /// Issuer для валидации токена
    /// </summary>
    public string Issuer { get; init; } = "https://appleid.apple.com";

    /// <summary>
    /// URL для получения публичных ключей Apple
    /// </summary>
    public string JwksUrl { get; init; } = "https://appleid.apple.com/auth/keys";
}
