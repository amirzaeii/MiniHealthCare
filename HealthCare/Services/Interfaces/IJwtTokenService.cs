using HealthCare.Models.Entities;

namespace HealthCare.Services.Interfaces;

public interface IJwtTokenService
{
    /// <summary>Generates a signed JWT for the given user. Includes hcid and doctor_id claims.</summary>
    (string token, DateTime expiresAt) GenerateToken(ApplicationUser user, Guid? doctorProfileId);
}
