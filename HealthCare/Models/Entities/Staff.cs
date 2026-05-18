namespace HealthCare.Models.Entities;

public class Staff
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string Department { get; set; } = string.Empty;
    public Guid HealthcareCenterId { get; set; }
    public HealthcareCenter HealthcareCenter { get; set; } = null!;
}
