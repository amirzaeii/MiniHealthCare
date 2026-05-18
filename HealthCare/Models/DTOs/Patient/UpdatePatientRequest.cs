using System.ComponentModel.DataAnnotations;

namespace HealthCare.Models.DTOs.Patient;

/// <summary>Request body for updating an existing patient's details.</summary>
public class UpdatePatientRequest
{
    /// <summary>Updated full name.</summary>
    [Required, MaxLength(200)] public string FullName { get; set; } = string.Empty;

    /// <summary>Updated date of birth.</summary>
    [Required] public DateOnly DateOfBirth { get; set; }

    /// <summary>Updated gender.</summary>
    [MaxLength(10)] public string Gender { get; set; } = string.Empty;

    /// <summary>Updated contact phone number.</summary>
    [MaxLength(20)] public string ContactPhone { get; set; } = string.Empty;

    /// <summary>Updated contact email address.</summary>
    [EmailAddress, MaxLength(150)] public string ContactEmail { get; set; } = string.Empty;

    /// <summary>Updated residential address.</summary>
    [MaxLength(500)] public string Address { get; set; } = string.Empty;
}
