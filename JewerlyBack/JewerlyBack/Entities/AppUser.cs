namespace JewerlyBack.Models;

/// <summary>
/// Пользователь приложения.
/// Поддерживает аутентификацию через email/password и внешних провайдеров (Google, Apple).
/// </summary>
public class AppUser
{
    public Guid Id { get; set; }

    /// <summary>
    /// Email пользователя (уникальный, case-insensitive)
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Хэш пароля (BCrypt/Argon2 через PasswordHasher).
    /// Может быть пустым для пользователей, зарегистрированных только через внешнего провайдера.
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Имя пользователя (опционально)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Провайдер аутентификации: null (email/password), "google", "apple"
    /// </summary>
    /// <remarks>
    /// Если пользователь зарегистрирован через email+password, это поле null.
    /// Если пользователь использует внешнего провайдера — здесь имя провайдера.
    /// В будущем можно расширить до отдельной таблицы UserExternalLogins для поддержки
    /// нескольких провайдеров на одного пользователя.
    /// </remarks>
    public string? Provider { get; set; }

    /// <summary>
    /// Уникальный идентификатор пользователя у внешнего провайдера (sub claim)
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Подтверждён ли email пользователя
    /// </summary>
    /// <remarks>
    /// Для внешних провайдеров (Google, Apple) устанавливается в true,
    /// если провайдер гарантирует верифицированный email.
    /// Для email+password регистрации — false до подтверждения через email.
    /// </remarks>
    public bool IsEmailConfirmed { get; set; }

    /// <summary>
    /// Дата/время последнего входа
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    // Навигационные свойства
    public ICollection<JewelryConfiguration> Configurations { get; set; } = new List<JewelryConfiguration>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<UploadedAsset> Assets { get; set; } = new List<UploadedAsset>();
}
