using HealthCare.Common.Constants;
using HealthCare.Common.Exceptions;
using HealthCare.Data;
using HealthCare.Models.DTOs.Auth;
using HealthCare.Models.Entities;
using HealthCare.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Services.Implementations;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    AppDbContext db,
    IJwtTokenService jwtTokenService) : IAuthService
{
    private static readonly HashSet<string> ValidRoles =
    [
        Roles.SuperAdmin, Roles.Admin, Roles.Doctor, Roles.Receptionist, Roles.Staff
    ];

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (!ValidRoles.Contains(request.Role))
            throw new ConflictException($"Invalid role '{request.Role}'.");

        // SuperAdmin requires no center; all other roles must supply one
        if (request.Role != Roles.SuperAdmin && request.HealthcareCenterId is null)
            throw new ConflictException("HealthcareCenterId is required for non-SuperAdmin roles.");

        if (request.HealthcareCenterId.HasValue)
        {
            var centerExists = await db.HealthcareCenters
                .AnyAsync(h => h.Id == request.HealthcareCenterId.Value, ct);
            if (!centerExists)
                throw new NotFoundException($"Healthcare center {request.HealthcareCenterId} not found.");
        }

        var user = new ApplicationUser
        {
            FirstName          = request.FirstName,
            LastName           = request.LastName,
            Email              = request.Email,
            UserName           = request.Email,
            Role               = request.Role,
            HealthcareCenterId = request.HealthcareCenterId,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new ConflictException(string.Join("; ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, request.Role);

        // Create role-specific profile records
        Guid? doctorProfileId = null;

        if (request.Role == Roles.Doctor)
        {
            var doctor = new Doctor
            {
                Id                 = Guid.NewGuid(),
                UserId             = user.Id,
                Specialization     = request.Specialization ?? string.Empty,
                HealthcareCenterId = request.HealthcareCenterId!.Value,
            };
            db.Doctors.Add(doctor);
            doctorProfileId = doctor.Id;
        }
        else if (request.Role == Roles.Receptionist)
        {
            db.Receptionists.Add(new Receptionist
            {
                Id                 = Guid.NewGuid(),
                UserId             = user.Id,
                HealthcareCenterId = request.HealthcareCenterId!.Value,
            });
        }
        else if (request.Role == Roles.Staff)
        {
            db.Staff.Add(new Staff
            {
                Id                 = Guid.NewGuid(),
                UserId             = user.Id,
                Department         = request.Department ?? string.Empty,
                HealthcareCenterId = request.HealthcareCenterId!.Value,
            });
        }

        await db.SaveChangesAsync(ct);

        var (token, expiresAt) = jwtTokenService.GenerateToken(user, doctorProfileId);

        return new AuthResponse
        {
            Token              = token,
            ExpiresAt          = expiresAt,
            FullName           = $"{user.FirstName} {user.LastName}",
            Email              = user.Email!,
            Role               = user.Role,
            HealthcareCenterId = user.HealthcareCenterId,
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
            ?? throw new NotFoundException("Invalid email or password.");

        if (!await userManager.CheckPasswordAsync(user, request.Password))
            throw new NotFoundException("Invalid email or password.");

        Guid? doctorProfileId = null;
        if (user.Role == Roles.Doctor)
        {
            var doctor = await db.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id, ct);
            doctorProfileId = doctor?.Id;
        }

        var (token, expiresAt) = jwtTokenService.GenerateToken(user, doctorProfileId);

        return new AuthResponse
        {
            Token              = token,
            ExpiresAt          = expiresAt,
            FullName           = $"{user.FirstName} {user.LastName}",
            Email              = user.Email!,
            Role               = user.Role,
            HealthcareCenterId = user.HealthcareCenterId,
        };
    }
}
