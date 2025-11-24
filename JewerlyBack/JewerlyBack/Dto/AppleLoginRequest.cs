using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Dto;

/// <summary>
/// Запрос на вход через Apple Sign-In.
/// Flutter-клиент получает id_token от Sign in with Apple и отправляет его на backend.
/// </summary>
public class AppleLoginRequest
{
    /// <summary>
    /// ID Token, полученный от Sign in with Apple
    /// </summary>
    /// <remarks>
    /// Это JWT токен, подписанный Apple, содержащий информацию о пользователе:
    /// - sub: уникальный Apple user ID (стабильный для данного приложения)
    /// - email: email пользователя (может быть приватным relay email)
    /// - email_verified: подтверждён ли email
    ///
    /// Важно: Apple передаёт email только при ПЕРВОМ входе.
    /// При последующих входах email может отсутствовать.
    /// </remarks>
    [Required(ErrorMessage = "IdToken is required")]
    public string IdToken { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя (опционально, передаётся только при первом входе)
    /// </summary>
    /// <remarks>
    /// Apple передаёт имя пользователя только при первом входе.
    /// Клиент должен сохранить его и передавать на backend.
    /// </remarks>
    public string? FullName { get; set; }
}
