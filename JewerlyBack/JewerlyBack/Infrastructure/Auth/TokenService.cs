using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JewerlyBack.Application.Interfaces;
using JewerlyBack.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JewerlyBack.Infrastructure.Auth;

/// <summary>
/// Реализация сервиса для генерации JWT токенов
/// </summary>
public class TokenService : ITokenService
{
    private readonly AuthOptions _options;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IOptions<AuthOptions> options, ILogger<TokenService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public (string Token, long ExpiresAt) GenerateAccessToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.JwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(_options.TokenLifetimeMinutes);
        var expiresAt = new DateTimeOffset(expires).ToUnixTimeSeconds();

        var claims = new List<Claim>
        {
            // Standard claims
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),

            // Custom claims
            new("userId", user.Id.ToString()),
            new("emailVerified", user.IsEmailConfirmed.ToString().ToLower())
        };

        // Добавляем имя, если есть
        if (!string.IsNullOrWhiteSpace(user.Name))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.Name));
        }

        // Добавляем провайдера, если пользователь авторизован через внешний сервис
        if (!string.IsNullOrWhiteSpace(user.Provider))
        {
            claims.Add(new Claim("provider", user.Provider));
        }

        var token = new JwtSecurityToken(
            issuer: _options.JwtIssuer,
            audience: _options.JwtAudience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogDebug(
            "Generated JWT for user {UserId}, expires at {ExpiresAt}",
            user.Id, expires);

        return (tokenString, expiresAt);
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        // Генерируем криптографически стойкий случайный токен
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);

        // TODO: Реализовать хранение refresh токенов в БД:
        // - Таблица RefreshTokens (Id, UserId, Token, ExpiresAt, CreatedAt, RevokedAt, ReplacedByToken)
        // - При использовании refresh токена — ротация (выдача нового, инвалидация старого)
        // - Endpoint POST /api/account/refresh для обновления токенов
        // - Endpoint POST /api/account/revoke для отзыва токена
    }
}
