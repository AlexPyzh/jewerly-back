using JewerlyBack.Dto;
using JewerlyBack.Models;

namespace JewerlyBack.Application.Interfaces;

/// <summary>
/// Сервис для работы с учетными записями пользователей
/// (регистрация, аутентификация, управление профилем)
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Регистрация нового пользователя через email/password
    /// </summary>
    /// <param name="request">Данные для регистрации</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>ID созданного пользователя</returns>
    /// <exception cref="InvalidOperationException">Если email уже занят</exception>
    Task<Guid> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <summary>
    /// Аутентификация через email/password
    /// </summary>
    /// <param name="request">Данные для входа</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>AuthResponse с токеном или null если credentials неверны</returns>
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);

    /// <summary>
    /// Аутентификация через Google Sign-In
    /// </summary>
    /// <param name="request">ID Token от Google</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>AuthResponse с токеном или null если токен невалиден</returns>
    Task<AuthResponse?> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken ct = default);

    /// <summary>
    /// Аутентификация через Apple Sign-In
    /// </summary>
    /// <param name="request">ID Token от Apple</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>AuthResponse с токеном или null если токен невалиден</returns>
    Task<AuthResponse?> LoginWithAppleAsync(AppleLoginRequest request, CancellationToken ct = default);

    /// <summary>
    /// Получить пользователя по ID
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Пользователь или null</returns>
    Task<AppUser?> GetByIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Получить пользователя по email
    /// </summary>
    /// <param name="email">Email пользователя</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Пользователь или null</returns>
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Получить профиль текущего пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>UserProfileDto с данными пользователя</returns>
    /// <exception cref="InvalidOperationException">Если пользователь не найден</exception>
    Task<UserProfileDto> GetCurrentUserProfileAsync(Guid userId, CancellationToken ct = default);
}
