using System.ComponentModel.DataAnnotations;

namespace HealthCare.Models.DTOs.HealthcareCenter;

/// <summary>Request body for creating a new healthcare center (SuperAdmin only).</summary>
public class CreateHealthcareCenterRequest
{
    /// <summary>Full name of the hospital or clinic.</summary>
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;

    /// <summary>Physical address.</summary>
    [Required, MaxLength(500)] public string Address { get; set; } = string.Empty;

    /// <summary>Primary contact phone number.</summary>
    [MaxLength(20)] public string ContactPhone { get; set; } = string.Empty;

    /// <summary>Primary contact email address.</summary>
    [EmailAddress, MaxLength(150)] public string ContactEmail { get; set; } = string.Empty;
}
