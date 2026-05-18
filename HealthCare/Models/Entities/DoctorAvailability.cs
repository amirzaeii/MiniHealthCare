namespace HealthCare.Models.Entities;

public class DoctorAvailability
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    /// <summary>Day of week this availability applies to.</summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>Start of the available window (local time).</summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>End of the available window (local time).</summary>
    public TimeOnly EndTime { get; set; }
}
