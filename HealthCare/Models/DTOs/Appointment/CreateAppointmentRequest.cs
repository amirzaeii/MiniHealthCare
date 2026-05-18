using System.ComponentModel.DataAnnotations;

namespace HealthCare.Models.DTOs.Appointment;

/// <summary>Request body for scheduling a new appointment.</summary>
public class CreateAppointmentRequest
{
    /// <summary>ID of the patient booking the appointment.</summary>
    [Required] public Guid PatientId { get; set; }

    /// <summary>ID of the doctor. Must belong to the same healthcare center as the caller.</summary>
    [Required] public Guid DoctorId { get; set; }

    /// <summary>Appointment start time in UTC. Must fall within the doctor's availability window for that day.</summary>
    [Required] public DateTime ScheduledStart { get; set; }

    /// <summary>Appointment end time in UTC. Must be after ScheduledStart.</summary>
    [Required] public DateTime ScheduledEnd { get; set; }

    /// <summary>Optional booking notes visible to both receptionist and doctor.</summary>
    [MaxLength(1000)] public string? Notes { get; set; }
}
