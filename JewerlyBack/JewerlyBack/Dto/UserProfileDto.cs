namespace JewerlyBack.Dto;

/// <summary>
/// DTO профиля текущего пользователя для GET /api/account/me
/// </summary>
public class UserProfileDto
{
    /// <summary>
    /// Уникальный идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Email пользователя
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// Имя пользователя (опционально)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Дата создания учетной записи
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// URL аватара пользователя (опционально)
    /// </summary>
    public string? AvatarUrl { get; set; }
}
