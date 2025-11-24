using System.Security.Claims;

namespace JewerlyBack.Infrastructure.Extensions;

/// <summary>
/// Расширения для ClaimsPrincipal для удобной работы с JWT claims.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Получить ID текущего пользователя из JWT токена.
    /// </summary>
    /// <param name="principal">ClaimsPrincipal из JWT токена</param>
    /// <returns>GUID пользователя</returns>
    /// <exception cref="InvalidOperationException">Если claim "sub" отсутствует или невалиден</exception>
    /// <remarks>
    /// Извлекает userId из стандартного claim "sub" (ClaimTypes.NameIdentifier).
    /// Этот claim устанавливается в TokenService при генерации JWT.
    ///
    /// Использование:
    /// ```csharp
    /// [Authorize]
    /// public async Task<IActionResult> GetProfile()
    /// {
    ///     var userId = User.GetCurrentUserId();
    ///     // ...
    /// }
    /// ```
    /// </remarks>
    public static Guid GetCurrentUserId(this ClaimsPrincipal principal)
    {
        if (principal is null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        // Пытаемся получить claim "sub" (стандартный claim для user ID)
        var subClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
                       ?? principal.FindFirst("sub")
                       ?? principal.FindFirst("userId"); // Fallback на custom claim

        if (subClaim is null || string.IsNullOrWhiteSpace(subClaim.Value))
        {
            throw new InvalidOperationException("User ID claim (sub) not found in token. Ensure user is authenticated.");
        }

        if (!Guid.TryParse(subClaim.Value, out var userId))
        {
            throw new InvalidOperationException($"User ID claim value '{subClaim.Value}' is not a valid GUID.");
        }

        return userId;
    }

    /// <summary>
    /// Получить email текущего пользователя из JWT токена.
    /// </summary>
    /// <param name="principal">ClaimsPrincipal из JWT токена</param>
    /// <returns>Email пользователя или null, если не найден</returns>
    public static string? GetUserEmail(this ClaimsPrincipal principal)
    {
        if (principal is null)
        {
            return null;
        }

        var emailClaim = principal.FindFirst(ClaimTypes.Email)
                         ?? principal.FindFirst("email");

        return emailClaim?.Value;
    }

    /// <summary>
    /// Проверить, подтверждён ли email пользователя.
    /// </summary>
    /// <param name="principal">ClaimsPrincipal из JWT токена</param>
    /// <returns>True если email подтверждён, иначе false</returns>
    public static bool IsEmailVerified(this ClaimsPrincipal principal)
    {
        if (principal is null)
        {
            return false;
        }

        var verifiedClaim = principal.FindFirst("emailVerified");

        return verifiedClaim?.Value?.ToLowerInvariant() == "true";
    }

    /// <summary>
    /// Получить provider аутентификации (local, google, apple).
    /// </summary>
    /// <param name="principal">ClaimsPrincipal из JWT токена</param>
    /// <returns>Provider или null</returns>
    public static string? GetAuthProvider(this ClaimsPrincipal principal)
    {
        if (principal is null)
        {
            return null;
        }

        var providerClaim = principal.FindFirst("provider");

        return providerClaim?.Value;
    }
}
