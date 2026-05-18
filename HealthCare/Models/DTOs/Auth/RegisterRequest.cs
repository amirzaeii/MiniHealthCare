using System.ComponentModel.DataAnnotations;

namespace HealthCare.Models.DTOs.Auth;

/// <summary>Request body for registering a new user account.</summary>
public class RegisterRequest
{
    /// <summary>User's first name.</summary>
    [Required] public string FirstName { get; set; } = string.Empty;

    /// <summary>User's last name.</summary>
    [Required] public string LastName { get; set; } = string.Empty;

    /// <summary>Email address used as the login username.</summary>
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;

    /// <summary>Password (min 8 chars, at least one digit).</summary>
    [Required, MinLength(8)] public string Password { get; set; } = string.Empty;

    /// <summary>Role to assign: Admin, Doctor, Receptionist, or Staff. SuperAdmin is not self-registerable.</summary>
    [Required] public string Role { get; set; } = string.Empty;

    /// <summary>ID of the healthcare center this user belongs to. Not required for SuperAdmin.</summary>
    public Guid? HealthcareCenterId { get; set; }

    /// <summary>Required when role is Doctor. Medical specialization of the doctor.</summary>
    public string? Specialization { get; set; }

    /// <summary>Required when role is Staff. Department the staff member belongs to.</summary>
    public string? Department { get; set; }
}
