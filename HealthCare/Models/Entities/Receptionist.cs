namespace HealthCare.Models.Entities;

public class Receptionist
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public Guid HealthcareCenterId { get; set; }
    public HealthcareCenter HealthcareCenter { get; set; } = null!;
}
