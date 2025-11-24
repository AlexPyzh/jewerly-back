using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Dto;

/// <summary>
/// Запрос на регистрацию нового пользователя через email/password
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Email пользователя
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Пароль (минимум 8 символов)
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [MaxLength(128, ErrorMessage = "Password must not exceed 128 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя (опционально)
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; set; }
}
