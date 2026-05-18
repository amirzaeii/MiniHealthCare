using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthCare.Models.Entities;
using HealthCare.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace HealthCare.Services.Implementations;

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public (string token, DateTime expiresAt) GenerateToken(ApplicationUser user, Guid? doctorProfileId)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpirationMinutes"]!));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,        user.Id),
            new(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email,      user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.GivenName,  user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(ClaimTypes.Role,                    user.Role),
            new("hcid",                             user.HealthcareCenterId?.ToString() ?? string.Empty),
        };

        if (doctorProfileId.HasValue)
            claims.Add(new("doctor_id", doctorProfileId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer:   jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims:   claims,
            expires:  expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
