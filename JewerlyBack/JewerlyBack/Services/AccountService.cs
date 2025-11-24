using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;
using JewerlyBack.Application.Interfaces;
using JewerlyBack.Data;
using JewerlyBack.Dto;
using JewerlyBack.Infrastructure.Auth;
using JewerlyBack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JewerlyBack.Services;

/// <summary>
/// Реализация сервиса для работы с учетными записями пользователей.
/// Поддерживает email/password, Google Sign-In, Apple Sign-In.
/// </summary>
/// <remarks>
/// Безопасность:
/// - Пароли хэшируются через PasswordHasher (PBKDF2)
/// - Google токены валидируются через официальную библиотеку
/// - Apple токены валидируются через публичные ключи JWKS
/// - Не раскрывается информация о существовании пользователя при неверном логине
/// </remarks>
public class AccountService : IAccountService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AccountService> _logger;
    private readonly PasswordHasher<AppUser> _passwordHasher;
    private readonly GoogleAuthOptions _googleOptions;
    private readonly AppleAuthOptions _appleOptions;

    public AccountService(
        AppDbContext context,
        ITokenService tokenService,
        IOptions<GoogleAuthOptions> googleOptions,
        IOptions<AppleAuthOptions> appleOptions,
        ILogger<AccountService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _googleOptions = googleOptions.Value;
        _appleOptions = appleOptions.Value;
        _logger = logger;
        _passwordHasher = new PasswordHasher<AppUser>();
    }

    /// <inheritdoc />
    public async Task<Guid> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        // Проверка уникальности email (case-insensitive)
        var emailLower = request.Email.ToLowerInvariant();
        var existingUser = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == emailLower, ct);

        if (existingUser)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", emailLower);
            throw new InvalidOperationException("User with this email already exists");
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = emailLower,
            Name = request.Name?.Trim(),
            IsEmailConfirmed = false, // TODO: Реализовать email verification
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Хэшируем пароль
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("User registered: {UserId}, Email: {Email}", user.Id, user.Email);

        return user.Id;
    }

    /// <inheritdoc />
    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var emailLower = request.Email.ToLowerInvariant();
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower, ct);

        // Не раскрываем, существует ли пользователь
        if (user is null)
        {
            _logger.LogWarning("Login attempt for non-existent user: {Email}", emailLower);
            return null;
        }

        // Проверяем, что пользователь зарегистрирован через email/password
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            _logger.LogWarning(
                "Login attempt with password for external provider user: {UserId}, Provider: {Provider}",
                user.Id, user.Provider);
            return null;
        }

        // Верифицируем пароль
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Invalid password for user: {UserId}", user.Id);
            return null;
        }

        // Если пароль нужно перехэшировать (устаревший алгоритм)
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            _logger.LogInformation("Password rehashed for user: {UserId}", user.Id);
        }

        // Обновляем время последнего входа
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("User logged in: {UserId}", user.Id);

        return CreateAuthResponse(user);
    }

    /// <inheritdoc />
    public async Task<AuthResponse?> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken ct = default)
    {
        GoogleJsonWebSignature.Payload payload;

        try
        {
            // Валидация токена через официальную библиотеку Google
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleOptions.ClientId }
            };

            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid Google ID token");
            return null;
        }

        // Извлекаем данные из токена
        var googleId = payload.Subject;
        var email = payload.Email;
        var name = payload.Name;
        var emailVerified = payload.EmailVerified;

        if (string.IsNullOrEmpty(googleId))
        {
            _logger.LogWarning("Google token missing subject claim");
            return null;
        }

        // Ищем существующего пользователя
        var user = await FindOrCreateExternalUserAsync(
            provider: "google",
            externalId: googleId,
            email: email,
            name: name,
            isEmailVerified: emailVerified,
            ct: ct);

        if (user is null)
        {
            return null;
        }

        _logger.LogInformation("User logged in via Google: {UserId}", user.Id);

        return CreateAuthResponse(user);
    }

    /// <inheritdoc />
    public async Task<AuthResponse?> LoginWithAppleAsync(AppleLoginRequest request, CancellationToken ct = default)
    {
        // Валидация Apple ID Token
        var applePayload = await ValidateAppleTokenAsync(request.IdToken);

        if (applePayload is null)
        {
            return null;
        }

        var appleId = applePayload.Subject;
        var email = applePayload.Email;
        var emailVerified = applePayload.EmailVerified;

        // Apple передаёт имя только при первом входе, поэтому берём из request
        var name = request.FullName;

        if (string.IsNullOrEmpty(appleId))
        {
            _logger.LogWarning("Apple token missing subject claim");
            return null;
        }

        // Ищем существующего пользователя
        var user = await FindOrCreateExternalUserAsync(
            provider: "apple",
            externalId: appleId,
            email: email,
            name: name,
            isEmailVerified: emailVerified,
            ct: ct);

        if (user is null)
        {
            return null;
        }

        _logger.LogInformation("User logged in via Apple: {UserId}", user.Id);

        return CreateAuthResponse(user);
    }

    /// <inheritdoc />
    public async Task<AppUser?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    /// <inheritdoc />
    public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var emailLower = email.ToLowerInvariant();
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower, ct);
    }

    /// <summary>
    /// Ищет или создаёт пользователя для внешнего провайдера
    /// </summary>
    private async Task<AppUser?> FindOrCreateExternalUserAsync(
        string provider,
        string externalId,
        string? email,
        string? name,
        bool isEmailVerified,
        CancellationToken ct)
    {
        // Сначала ищем по provider + externalId
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Provider == provider && u.ExternalId == externalId, ct);

        if (user is not null)
        {
            // Обновляем данные при входе
            user.LastLoginAt = DateTimeOffset.UtcNow;

            // Обновляем email, если он изменился и подтверждён
            if (!string.IsNullOrEmpty(email) && isEmailVerified && user.Email != email.ToLowerInvariant())
            {
                // Проверяем, что новый email не занят другим пользователем
                var emailLower = email.ToLowerInvariant();
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email.ToLower() == emailLower && u.Id != user.Id, ct);

                if (!emailExists)
                {
                    user.Email = emailLower;
                    user.IsEmailConfirmed = true;
                }
            }

            await _context.SaveChangesAsync(ct);
            return user;
        }

        // Если пользователь не найден по externalId, ищем по email
        if (!string.IsNullOrEmpty(email))
        {
            var emailLower = email.ToLowerInvariant();
            user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower, ct);

            if (user is not null)
            {
                // Связываем существующего пользователя с провайдером
                // Только если у пользователя ещё нет другого провайдера
                if (string.IsNullOrEmpty(user.Provider))
                {
                    user.Provider = provider;
                    user.ExternalId = externalId;
                    user.LastLoginAt = DateTimeOffset.UtcNow;

                    if (isEmailVerified)
                    {
                        user.IsEmailConfirmed = true;
                    }

                    await _context.SaveChangesAsync(ct);
                    return user;
                }

                // Пользователь уже связан с другим провайдером
                // В будущем можно реализовать связывание нескольких провайдеров
                _logger.LogWarning(
                    "Email {Email} already linked to provider {ExistingProvider}, attempted {NewProvider}",
                    email, user.Provider, provider);
                return null;
            }
        }

        // Создаём нового пользователя
        if (string.IsNullOrEmpty(email))
        {
            // Apple может не передать email (пользователь скрыл его)
            // Генерируем placeholder email
            email = $"{provider}_{externalId}@placeholder.local";
        }

        var newUser = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            Name = name?.Trim(),
            Provider = provider,
            ExternalId = externalId,
            IsEmailConfirmed = isEmailVerified,
            CreatedAt = DateTimeOffset.UtcNow,
            LastLoginAt = DateTimeOffset.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Created new user via {Provider}: {UserId}, Email: {Email}",
            provider, newUser.Id, newUser.Email);

        return newUser;
    }

    /// <summary>
    /// Валидация Apple ID Token
    /// </summary>
    private async Task<AppleTokenPayload?> ValidateAppleTokenAsync(string idToken)
    {
        try
        {
            // Декодируем JWT без валидации для извлечения header
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(idToken);

            // Проверяем issuer
            if (jsonToken.Issuer != _appleOptions.Issuer)
            {
                _logger.LogWarning("Apple token invalid issuer: {Issuer}", jsonToken.Issuer);
                return null;
            }

            // Проверяем audience
            if (!jsonToken.Audiences.Contains(_appleOptions.ClientId))
            {
                _logger.LogWarning("Apple token invalid audience");
                return null;
            }

            // Проверяем срок действия
            if (jsonToken.ValidTo < DateTime.UtcNow)
            {
                _logger.LogWarning("Apple token expired");
                return null;
            }

            // Получаем публичные ключи Apple для валидации подписи
            var isSignatureValid = await ValidateAppleSignatureAsync(idToken, jsonToken);

            if (!isSignatureValid)
            {
                _logger.LogWarning("Apple token signature validation failed");
                return null;
            }

            // Извлекаем payload
            var subject = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var email = jsonToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var emailVerifiedClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value;
            var emailVerified = emailVerifiedClaim == "true";

            return new AppleTokenPayload
            {
                Subject = subject ?? string.Empty,
                Email = email,
                EmailVerified = emailVerified
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate Apple token");
            return null;
        }
    }

    /// <summary>
    /// Валидация подписи Apple токена через JWKS
    /// </summary>
    private async Task<bool> ValidateAppleSignatureAsync(string idToken, JwtSecurityToken jsonToken)
    {
        try
        {
            // Получаем ключ из header токена
            var kid = jsonToken.Header.Kid;

            if (string.IsNullOrEmpty(kid))
            {
                _logger.LogWarning("Apple token missing kid in header");
                return false;
            }

            // Загружаем публичные ключи Apple
            using var httpClient = new HttpClient();
            var jwksJson = await httpClient.GetStringAsync(_appleOptions.JwksUrl);
            var jwks = new JsonWebKeySet(jwksJson);

            // Находим нужный ключ
            var key = jwks.Keys.FirstOrDefault(k => k.Kid == kid);

            if (key is null)
            {
                _logger.LogWarning("Apple JWKS key not found: {Kid}", kid);
                return false;
            }

            // Валидируем подпись
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _appleOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = _appleOptions.ClientId,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key
            };

            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(idToken, validationParameters, out _);

            return true;
        }
        catch (SecurityTokenValidationException ex)
        {
            _logger.LogWarning(ex, "Apple token signature validation error");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Apple token validation");
            return false;
        }
    }

    /// <summary>
    /// Создаёт AuthResponse для пользователя
    /// </summary>
    private AuthResponse CreateAuthResponse(AppUser user)
    {
        var (token, expiresAt) = _tokenService.GenerateAccessToken(user);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            Token = token,
            ExpiresAt = expiresAt,
            Provider = user.Provider
        };
    }

    /// <summary>
    /// Payload из Apple ID Token
    /// </summary>
    private class AppleTokenPayload
    {
        public string Subject { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool EmailVerified { get; set; }
    }
}
