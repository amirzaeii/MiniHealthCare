namespace HealthCare.Models.DTOs.MedicalRecord;

/// <summary>Medical record details. Accessible only by the treating doctor and Admin.</summary>
public class MedicalRecordResponse
{
    /// <summary>Medical record ID.</summary>
    public Guid Id { get; set; }

    /// <summary>ID of the associated appointment.</summary>
    public Guid AppointmentId { get; set; }

    /// <summary>ID of the patient.</summary>
    public Guid PatientId { get; set; }

    /// <summary>Full name of the patient.</summary>
    public string PatientName { get; set; } = string.Empty;

    /// <summary>ID of the treating doctor.</summary>
    public Guid DoctorId { get; set; }

    /// <summary>Full name of the treating doctor.</summary>
    public string DoctorName { get; set; } = string.Empty;

    /// <summary>Clinical diagnosis.</summary>
    public string Diagnosis { get; set; } = string.Empty;

    /// <summary>Clinical notes and observations.</summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>Timestamp when the record was created (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Timestamp of the last update (UTC).</summary>
    public DateTime UpdatedAt { get; set; }
}
