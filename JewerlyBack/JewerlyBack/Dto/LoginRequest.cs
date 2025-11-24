using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Dto;

/// <summary>
/// Запрос на вход через email/password
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Email пользователя
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Пароль
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}
