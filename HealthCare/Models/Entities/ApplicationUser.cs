using Microsoft.AspNetCore.Identity;

namespace HealthCare.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>Denormalized role for fast JWT claim emission.</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Null only for SuperAdmin who is not scoped to a center.</summary>
    public Guid? HealthcareCenterId { get; set; }
    public HealthcareCenter? HealthcareCenter { get; set; }
}
