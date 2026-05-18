namespace HealthCare.Models.Entities;

public class Doctor
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string Specialization { get; set; } = string.Empty;
    public Guid HealthcareCenterId { get; set; }
    public HealthcareCenter HealthcareCenter { get; set; } = null!;
    public ICollection<DoctorAvailability> Availabilities { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
}
