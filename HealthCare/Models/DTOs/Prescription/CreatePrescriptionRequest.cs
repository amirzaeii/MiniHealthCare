using System.ComponentModel.DataAnnotations;

namespace HealthCare.Models.DTOs.Prescription;

/// <summary>Request body for issuing a prescription (Doctor only).</summary>
public class CreatePrescriptionRequest
{
    /// <summary>ID of the appointment for which this prescription is being issued. Must belong to the calling doctor.</summary>
    [Required] public Guid AppointmentId { get; set; }

    /// <summary>Medication names, dosages, and quantities. Can be free-text or structured (e.g., "Amoxicillin 500mg x 3/day for 7 days").</summary>
    [Required, MaxLength(5000)] public string MedicationDetails { get; set; } = string.Empty;

    /// <summary>Patient administration instructions (e.g., "Take after meals", "Avoid alcohol").</summary>
    [MaxLength(2000)] public string Instructions { get; set; } = string.Empty;
}
