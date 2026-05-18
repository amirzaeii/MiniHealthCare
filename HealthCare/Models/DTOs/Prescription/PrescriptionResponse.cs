namespace HealthCare.Models.DTOs.Prescription;

/// <summary>Prescription details. Accessible only by the prescribing doctor and Admin.</summary>
public class PrescriptionResponse
{
    /// <summary>Prescription ID.</summary>
    public Guid Id { get; set; }

    /// <summary>ID of the associated appointment.</summary>
    public Guid AppointmentId { get; set; }

    /// <summary>ID of the patient.</summary>
    public Guid PatientId { get; set; }

    /// <summary>Full name of the patient.</summary>
    public string PatientName { get; set; } = string.Empty;

    /// <summary>ID of the prescribing doctor.</summary>
    public Guid DoctorId { get; set; }

    /// <summary>Full name of the prescribing doctor.</summary>
    public string DoctorName { get; set; } = string.Empty;

    /// <summary>Medication details.</summary>
    public string MedicationDetails { get; set; } = string.Empty;

    /// <summary>Administration instructions.</summary>
    public string Instructions { get; set; } = string.Empty;

    /// <summary>Date and time the prescription was issued (UTC).</summary>
    public DateTime CreatedAt { get; set; }
}
