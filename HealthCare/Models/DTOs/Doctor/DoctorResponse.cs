namespace HealthCare.Models.DTOs.Doctor;

/// <summary>Doctor profile details.</summary>
public class DoctorResponse
{
    /// <summary>Doctor profile ID.</summary>
    public Guid Id { get; set; }

    /// <summary>Linked user account ID.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Full name (first + last).</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Medical specialization.</summary>
    public string Specialization { get; set; } = string.Empty;

    /// <summary>Healthcare center the doctor is assigned to.</summary>
    public Guid HealthcareCenterId { get; set; }
}
