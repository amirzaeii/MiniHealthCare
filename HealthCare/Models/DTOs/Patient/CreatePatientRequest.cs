using System.ComponentModel.DataAnnotations;

namespace HealthCare.Models.DTOs.Patient;

/// <summary>Request body for registering a new patient.</summary>
public class CreatePatientRequest
{
    /// <summary>Patient's full name.</summary>
    [Required, MaxLength(200)] public string FullName { get; set; } = string.Empty;

    /// <summary>Date of birth.</summary>
    [Required] public DateOnly DateOfBirth { get; set; }

    /// <summary>Gender: Male, Female, or Other.</summary>
    [MaxLength(10)] public string Gender { get; set; } = string.Empty;

    /// <summary>Contact phone number.</summary>
    [MaxLength(20)] public string ContactPhone { get; set; } = string.Empty;

    /// <summary>Contact email address.</summary>
    [EmailAddress, MaxLength(150)] public string ContactEmail { get; set; } = string.Empty;

    /// <summary>Residential address.</summary>
    [MaxLength(500)] public string Address { get; set; } = string.Empty;
}
