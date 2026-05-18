using System.ComponentModel.DataAnnotations;

namespace HealthCare.Models.DTOs.Doctor;

/// <summary>Request body for creating a doctor profile linked to an existing user account.</summary>
public class CreateDoctorRequest
{
    /// <summary>First name of the doctor.</summary>
    [Required, MaxLength(100)] public string FirstName { get; set; } = string.Empty;

    /// <summary>Last name of the doctor.</summary>
    [Required, MaxLength(100)] public string LastName { get; set; } = string.Empty;

    /// <summary>Email address (used as login).</summary>
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;

    /// <summary>Password for the doctor account.</summary>
    [Required, MinLength(8)] public string Password { get; set; } = string.Empty;

    /// <summary>Medical specialization (e.g., Cardiology, Pediatrics).</summary>
    [Required, MaxLength(100)] public string Specialization { get; set; } = string.Empty;
}
