namespace HealthCare.Models.Entities;

public class Patient
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Guid HealthcareCenterId { get; set; }
    public HealthcareCenter HealthcareCenter { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = [];
}
