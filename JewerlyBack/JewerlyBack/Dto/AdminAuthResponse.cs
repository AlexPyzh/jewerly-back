namespace JewerlyBack.Dto;

/// <summary>
/// Response on successful admin authentication
/// </summary>
public class AdminAuthResponse
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration timestamp (Unix)
    /// </summary>
    public long ExpiresAt { get; set; }

    /// <summary>
    /// Token type (always "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Admin username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Role (always "admin")
    /// </summary>
    public string Role { get; set; } = "admin";
}
