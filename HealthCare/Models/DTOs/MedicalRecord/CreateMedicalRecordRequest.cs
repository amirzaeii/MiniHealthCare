using System.ComponentModel.DataAnnotations;

namespace HealthCare.Models.DTOs.MedicalRecord;

/// <summary>Request body for creating a medical record after an appointment (Doctor only).</summary>
public class CreateMedicalRecordRequest
{
    /// <summary>ID of the appointment this record belongs to. The appointment must belong to the calling doctor.</summary>
    [Required] public Guid AppointmentId { get; set; }

    /// <summary>Clinical diagnosis summary.</summary>
    [Required, MaxLength(2000)] public string Diagnosis { get; set; } = string.Empty;

    /// <summary>Detailed clinical notes, observations, and treatment plan.</summary>
    [MaxLength(5000)] public string Notes { get; set; } = string.Empty;
}
