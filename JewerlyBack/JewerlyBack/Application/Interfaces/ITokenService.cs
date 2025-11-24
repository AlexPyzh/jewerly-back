using JewerlyBack.Models;

namespace JewerlyBack.Application.Interfaces;

/// <summary>
/// Сервис для генерации и валидации JWT токенов
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Генерирует JWT access token для пользователя
    /// </summary>
    /// <param name="user">Пользователь</param>
    /// <returns>Кортеж (token, expiresAt в Unix timestamp)</returns>
    (string Token, long ExpiresAt) GenerateAccessToken(AppUser user);

    /// <summary>
    /// Генерирует refresh token (для будущей реализации)
    /// </summary>
    /// <returns>Refresh token</returns>
    /// <remarks>
    /// TODO: Реализовать хранение refresh токенов в БД
    /// с поддержкой revoke и ротации.
    /// </remarks>
    string GenerateRefreshToken();
}
