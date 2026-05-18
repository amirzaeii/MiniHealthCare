namespace HealthCare.Models.DTOs.Appointment;

/// <summary>Appointment details. Medical record and prescription fields are omitted for Receptionist and Staff roles.</summary>
public class AppointmentResponse
{
    /// <summary>Unique appointment ID.</summary>
    public Guid Id { get; set; }

    /// <summary>ID of the patient.</summary>
    public Guid PatientId { get; set; }

    /// <summary>Full name of the patient.</summary>
    public string PatientName { get; set; } = string.Empty;

    /// <summary>ID of the doctor.</summary>
    public Guid DoctorId { get; set; }

    /// <summary>Full name of the doctor.</summary>
    public string DoctorName { get; set; } = string.Empty;

    /// <summary>Appointment start time (UTC).</summary>
    public DateTime ScheduledStart { get; set; }

    /// <summary>Appointment end time (UTC).</summary>
    public DateTime ScheduledEnd { get; set; }

    /// <summary>Current workflow status: Scheduled, CheckedIn, Completed, Canceled, or NoShow.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Booking notes added at scheduling time.</summary>
    public string? Notes { get; set; }

    /// <summary>Healthcare center this appointment belongs to.</summary>
    public Guid HealthcareCenterId { get; set; }

    /// <summary>Timestamp when the appointment was created (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Timestamp of the last status update (UTC).</summary>
    public DateTime UpdatedAt { get; set; }
}
