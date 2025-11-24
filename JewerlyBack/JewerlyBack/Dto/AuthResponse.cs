namespace JewerlyBack.Dto;

/// <summary>
/// Ответ при успешной аутентификации
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// ID пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Email пользователя
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя (если есть)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// JWT access token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Время истечения токена (Unix timestamp)
    /// </summary>
    public long ExpiresAt { get; set; }

    /// <summary>
    /// Тип токена (всегда "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Провайдер аутентификации (null для email/password, "google", "apple")
    /// </summary>
    public string? Provider { get; set; }
}
