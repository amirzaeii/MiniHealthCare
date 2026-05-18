namespace HealthCare.Models.DTOs.Doctor;

/// <summary>Doctor's availability slot for a specific day of the week.</summary>
public class DoctorAvailabilityResponse
{
    /// <summary>Availability record ID.</summary>
    public Guid Id { get; set; }

    /// <summary>Doctor this availability belongs to.</summary>
    public Guid DoctorId { get; set; }

    /// <summary>Day of the week.</summary>
    public string DayOfWeek { get; set; } = string.Empty;

    /// <summary>Start of the available window.</summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>End of the available window.</summary>
    public TimeOnly EndTime { get; set; }
}
