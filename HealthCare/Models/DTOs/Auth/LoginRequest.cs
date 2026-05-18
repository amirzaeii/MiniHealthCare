using System.ComponentModel.DataAnnotations;

namespace HealthCare.Models.DTOs.Auth;

/// <summary>Credentials for obtaining a JWT token.</summary>
public class LoginRequest
{
    /// <summary>Registered email address.</summary>
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;

    /// <summary>Account password.</summary>
    [Required] public string Password { get; set; } = string.Empty;
}
