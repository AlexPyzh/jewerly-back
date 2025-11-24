using JewerlyBack.Application.Interfaces;
using JewerlyBack.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewerlyBack.Controllers;

/// <summary>
/// Контроллер аутентификации и управления учётными записями.
/// Поддерживает email/password, Google Sign-In, Apple Sign-In.
/// </summary>
/// <remarks>
/// Все endpoints возвращают JWT токен при успешной аутентификации.
/// Для Flutter-клиента токен должен сохраняться в secure storage.
///
/// Безопасность:
/// - Пароли не логируются
/// - Общий ответ "Invalid credentials" при неверных данных
/// - Rate limiting рекомендуется настроить на уровне reverse proxy
/// </remarks>
[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAccountService accountService, ILogger<AccountController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    /// <summary>
    /// Регистрация нового пользователя через email/password
    /// </summary>
    /// <param name="request">Email, пароль и опционально имя</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>AuthResponse с JWT токеном (автоматический вход после регистрации)</returns>
    /// <remarks>
    /// После успешной регистрации пользователь автоматически авторизован.
    ///
    /// Требования к паролю:
    /// - Минимум 8 символов
    /// - Максимум 128 символов
    ///
    /// Пример запроса:
    /// ```json
    /// {
    ///   "email": "user@example.com",
    ///   "password": "SecurePassword123",
    ///   "name": "John Doe"
    /// }
    /// ```
    /// </remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = await _accountService.RegisterAsync(request, ct);

            _logger.LogInformation("User registered: {Email}", request.Email);

            // Автоматический вход после регистрации
            var loginResult = await _accountService.LoginAsync(
                new LoginRequest { Email = request.Email, Password = request.Password },
                ct);

            if (loginResult is null)
            {
                // Не должно происходить, но на всякий случай
                _logger.LogError("Failed to auto-login after registration: {UserId}", userId);
                return StatusCode(500, new { message = "Registration successful but auto-login failed" });
            }

            return CreatedAtAction(nameof(GetProfile), new { id = userId }, loginResult);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning("Registration failed - email exists: {Email}", request.Email);
            return Conflict(new { message = "User with this email already exists" });
        }
    }

    /// <summary>
    /// Аутентификация через email/password
    /// </summary>
    /// <param name="request">Email и пароль</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>AuthResponse с JWT токеном</returns>
    /// <remarks>
    /// При неверных credentials возвращается общий ответ "Invalid credentials"
    /// без указания, что именно неверно (email или пароль).
    ///
    /// Пример запроса:
    /// ```json
    /// {
    ///   "email": "user@example.com",
    ///   "password": "SecurePassword123"
    /// }
    /// ```
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _accountService.LoginAsync(request, ct);

        if (result is null)
        {
            // Общий ответ — не раскрываем, существует ли пользователь
            return Unauthorized(new { message = "Invalid credentials" });
        }

        _logger.LogInformation("User logged in: {UserId}", result.UserId);

        return Ok(result);
    }

    /// <summary>
    /// Аутентификация через Google Sign-In
    /// </summary>
    /// <param name="request">ID Token от Google</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>AuthResponse с JWT токеном</returns>
    /// <remarks>
    /// Flutter-клиент получает id_token от Google Sign-In SDK и отправляет его сюда.
    /// Backend валидирует токен, создаёт/находит пользователя и выдаёт свой JWT.
    ///
    /// Пример запроса:
    /// ```json
    /// {
    ///   "idToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..."
    /// }
    /// ```
    /// </remarks>
    [HttpPost("google")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginWithGoogle(
        [FromBody] GoogleLoginRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _accountService.LoginWithGoogleAsync(request, ct);

        if (result is null)
        {
            return Unauthorized(new { message = "Invalid Google token" });
        }

        _logger.LogInformation("User logged in via Google: {UserId}", result.UserId);

        return Ok(result);
    }

    /// <summary>
    /// Аутентификация через Apple Sign-In
    /// </summary>
    /// <param name="request">ID Token от Apple и опционально имя</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>AuthResponse с JWT токеном</returns>
    /// <remarks>
    /// Flutter-клиент получает id_token от Sign in with Apple и отправляет его сюда.
    ///
    /// Важно: Apple передаёт имя пользователя только при ПЕРВОМ входе.
    /// Клиент должен сохранить имя локально и передавать его в fullName при каждом запросе.
    ///
    /// Пример запроса:
    /// ```json
    /// {
    ///   "idToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
    ///   "fullName": "John Doe"
    /// }
    /// ```
    /// </remarks>
    [HttpPost("apple")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginWithApple(
        [FromBody] AppleLoginRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _accountService.LoginWithAppleAsync(request, ct);

        if (result is null)
        {
            return Unauthorized(new { message = "Invalid Apple token" });
        }

        _logger.LogInformation("User logged in via Apple: {UserId}", result.UserId);

        return Ok(result);
    }

    /// <summary>
    /// Получить профиль текущего пользователя
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Информация о пользователе</returns>
    /// <remarks>
    /// Требует авторизации (JWT токен в заголовке Authorization: Bearer {token})
    /// </remarks>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileResponse>> GetProfile(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("userId")?.Value
                         ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var user = await _accountService.GetByIdAsync(userId, ct);

        if (user is null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        return Ok(new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Provider = user.Provider,
            IsEmailConfirmed = user.IsEmailConfirmed,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        });
    }

    // TODO: Добавить endpoints:
    // - POST /api/account/refresh — обновление access токена через refresh token
    // - POST /api/account/logout — инвалидация refresh токена
    // - POST /api/account/revoke — отзыв всех токенов пользователя
    // - POST /api/account/change-password — смена пароля
    // - POST /api/account/forgot-password — запрос сброса пароля
    // - POST /api/account/reset-password — сброс пароля по токену
    // - POST /api/account/verify-email — подтверждение email
    // - PUT /api/account/profile — обновление профиля
}

/// <summary>
/// Ответ с профилем пользователя
/// </summary>
public class UserProfileResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Provider { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
}
