using System.ComponentModel.DataAnnotations;

namespace HealthCare.Models.DTOs.Doctor;

/// <summary>Request body for setting or updating a doctor's availability on a specific day.</summary>
public class DoctorAvailabilityRequest
{
    /// <summary>Day of the week (0 = Sunday, 1 = Monday, ..., 6 = Saturday).</summary>
    [Required] public DayOfWeek DayOfWeek { get; set; }

    /// <summary>Start of the available window, e.g. "09:00".</summary>
    [Required] public TimeOnly StartTime { get; set; }

    /// <summary>End of the available window, e.g. "17:00". Must be after StartTime.</summary>
    [Required] public TimeOnly EndTime { get; set; }
}
