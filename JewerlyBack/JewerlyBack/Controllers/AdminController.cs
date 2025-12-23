using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JewerlyBack.Dto;
using JewerlyBack.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JewerlyBack.Controllers;

/// <summary>
/// Controller for admin authentication
/// </summary>
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AuthOptions _authOptions;
    private readonly ILogger<AdminController> _logger;

    // Hardcoded admin credentials
    private const string AdminUsername = "admin";
    private const string AdminPassword = "admin";
    private const string AdminRole = "admin";

    public AdminController(IOptions<AuthOptions> authOptions, ILogger<AdminController> logger)
    {
        _authOptions = authOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Admin login endpoint
    /// </summary>
    /// <param name="request">Admin credentials</param>
    /// <returns>JWT token with admin role</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AdminAuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<AdminAuthResponse> Login([FromBody] AdminLoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate hardcoded credentials
        if (request.Username != AdminUsername || request.Password != AdminPassword)
        {
            _logger.LogWarning("Failed admin login attempt for username: {Username}", request.Username);
            return Unauthorized(new { message = "Invalid credentials" });
        }

        // Generate JWT token with admin role
        var token = GenerateAdminToken();

        _logger.LogInformation("Admin logged in successfully");

        return Ok(new AdminAuthResponse
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            Username = AdminUsername,
            Role = AdminRole
        });
    }

    /// <summary>
    /// Verify admin token and get current admin info
    /// </summary>
    /// <returns>Admin info</returns>
    [HttpGet("me")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult GetCurrentAdmin()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            Username = AdminUsername,
            Role = role ?? AdminRole
        });
    }

    private (string Token, long ExpiresAt) GenerateAdminToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.JwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(_authOptions.TokenLifetimeMinutes);
        var expiresAt = new DateTimeOffset(expires).ToUnixTimeSeconds();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "admin"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.Role, AdminRole),
            new("role", AdminRole),
            new("username", AdminUsername)
        };

        var token = new JwtSecurityToken(
            issuer: _authOptions.JwtIssuer,
            audience: _authOptions.JwtAudience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogDebug("Generated admin JWT, expires at {ExpiresAt}", expires);

        return (tokenString, expiresAt);
    }
}
