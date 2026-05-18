namespace HealthCare.Models.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    /// <summary>Appointment start (UTC).</summary>
    public DateTime ScheduledStart { get; set; }

    /// <summary>Appointment end (UTC).</summary>
    public DateTime ScheduledEnd { get; set; }

    /// <summary>Workflow status: Scheduled → CheckedIn → Completed | Canceled | NoShow.</summary>
    public string Status { get; set; } = "Scheduled";

    /// <summary>Optional notes added at booking by the receptionist.</summary>
    public string? Notes { get; set; }

    public Guid HealthcareCenterId { get; set; }
    public HealthcareCenter HealthcareCenter { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public MedicalRecord? MedicalRecord { get; set; }
    public Prescription? Prescription { get; set; }
}
