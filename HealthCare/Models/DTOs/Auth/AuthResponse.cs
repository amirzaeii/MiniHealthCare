namespace HealthCare.Models.DTOs.Auth;

/// <summary>Successful authentication response containing the bearer token and basic profile.</summary>
public class AuthResponse
{
    /// <summary>JWT bearer token to include in the Authorization header of subsequent requests.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Token expiry in UTC.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Authenticated user's display name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Authenticated user's email.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Assigned role.</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Healthcare center the user belongs to. Null for SuperAdmin.</summary>
    public Guid? HealthcareCenterId { get; set; }
}
