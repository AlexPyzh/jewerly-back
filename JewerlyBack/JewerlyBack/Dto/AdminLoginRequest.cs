using System.ComponentModel.DataAnnotations;

namespace JewerlyBack.Dto;

/// <summary>
/// Request for admin login
/// </summary>
public class AdminLoginRequest
{
    /// <summary>
    /// Admin username
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Admin password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}
