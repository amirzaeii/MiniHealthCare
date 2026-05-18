namespace HealthCare.Models.Entities;

public class Prescription
{
    public Guid Id { get; set; }

    /// <summary>One-to-one link to the appointment this prescription was issued in.</summary>
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    /// <summary>Medication names, dosages, and quantities (free-text or structured).</summary>
    public string MedicationDetails { get; set; } = string.Empty;

    /// <summary>Administration instructions for the patient.</summary>
    public string Instructions { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
