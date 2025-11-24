using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Dto;

/// <summary>
/// Запрос на вход через Google Sign-In.
/// Flutter-клиент получает id_token от Google и отправляет его на backend.
/// </summary>
public class GoogleLoginRequest
{
    /// <summary>
    /// ID Token, полученный от Google Sign-In SDK
    /// </summary>
    /// <remarks>
    /// Это JWT токен, подписанный Google, содержащий информацию о пользователе:
    /// - sub: уникальный Google user ID
    /// - email: email пользователя
    /// - email_verified: подтверждён ли email
    /// - name: имя пользователя
    /// - picture: URL аватара
    /// </remarks>
    [Required(ErrorMessage = "IdToken is required")]
    public string IdToken { get; set; } = string.Empty;
}
