namespace HealthCare.Models.DTOs.Patient;

/// <summary>Patient record details.</summary>
public class PatientResponse
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Full name of the patient.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Date of birth.</summary>
    public DateOnly DateOfBirth { get; set; }

    /// <summary>Gender.</summary>
    public string Gender { get; set; } = string.Empty;

    /// <summary>Contact phone number.</summary>
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>Contact email address.</summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>Residential address.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Healthcare center this patient is registered with.</summary>
    public Guid HealthcareCenterId { get; set; }
}
