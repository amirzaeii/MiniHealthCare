using HealthCare.Common.Constants;
using HealthCare.Common.Exceptions;
using HealthCare.Data;
using HealthCare.Models.DTOs.Doctor;
using HealthCare.Models.Entities;
using HealthCare.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Services.Implementations;

public class DoctorService(AppDbContext db, UserManager<ApplicationUser> userManager) : IDoctorService
{
    public async Task<IReadOnlyList<DoctorResponse>> GetAllAsync(Guid? healthcareCenterId, CancellationToken ct = default)
    {
        var query = db.Doctors.Include(d => d.User).AsQueryable();
        if (healthcareCenterId.HasValue)
            query = query.Where(d => d.HealthcareCenterId == healthcareCenterId.Value);

        return await query.Select(d => ToResponse(d)).ToListAsync(ct);
    }

    public async Task<DoctorResponse> GetByIdAsync(Guid id, Guid? healthcareCenterId, CancellationToken ct = default)
    {
        var doctor = await FindWithUserAsync(id, healthcareCenterId, ct);
        return ToResponse(doctor);
    }

    public async Task<DoctorResponse> CreateAsync(CreateDoctorRequest request, Guid healthcareCenterId, CancellationToken ct = default)
    {
        var user = new ApplicationUser
        {
            FirstName          = request.FirstName,
            LastName           = request.LastName,
            Email              = request.Email,
            UserName           = request.Email,
            Role               = Roles.Doctor,
            HealthcareCenterId = healthcareCenterId,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new ConflictException(string.Join("; ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, Roles.Doctor);

        var doctor = new Doctor
        {
            Id                 = Guid.NewGuid(),
            UserId             = user.Id,
            Specialization     = request.Specialization,
            HealthcareCenterId = healthcareCenterId,
        };
        db.Doctors.Add(doctor);
        await db.SaveChangesAsync(ct);
        doctor.User = user;
        return ToResponse(doctor);
    }

    public async Task DeleteAsync(Guid id, Guid? healthcareCenterId, CancellationToken ct = default)
    {
        var doctor = await FindWithUserAsync(id, healthcareCenterId, ct);
        db.Doctors.Remove(doctor);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<DoctorAvailabilityResponse>> GetAvailabilitiesAsync(Guid doctorId, Guid? healthcareCenterId, CancellationToken ct = default)
    {
        await AssertDoctorExistsAsync(doctorId, healthcareCenterId, ct);
        return await db.DoctorAvailabilities
            .Where(a => a.DoctorId == doctorId)
            .Select(a => ToAvailabilityResponse(a))
            .ToListAsync(ct);
    }

    public async Task<DoctorAvailabilityResponse> UpsertAvailabilityAsync(Guid doctorId, DoctorAvailabilityRequest request, Guid? healthcareCenterId, CancellationToken ct = default)
    {
        if (request.EndTime <= request.StartTime)
            throw new ConflictException("EndTime must be after StartTime.");

        await AssertDoctorExistsAsync(doctorId, healthcareCenterId, ct);

        var existing = await db.DoctorAvailabilities
            .FirstOrDefaultAsync(a => a.DoctorId == doctorId && a.DayOfWeek == request.DayOfWeek, ct);

        if (existing is null)
        {
            existing = new DoctorAvailability { Id = Guid.NewGuid(), DoctorId = doctorId };
            db.DoctorAvailabilities.Add(existing);
        }

        existing.DayOfWeek = request.DayOfWeek;
        existing.StartTime = request.StartTime;
        existing.EndTime   = request.EndTime;
        await db.SaveChangesAsync(ct);
        return ToAvailabilityResponse(existing);
    }

    public async Task DeleteAvailabilityAsync(Guid doctorId, DayOfWeek dayOfWeek, Guid? healthcareCenterId, CancellationToken ct = default)
    {
        await AssertDoctorExistsAsync(doctorId, healthcareCenterId, ct);
        var slot = await db.DoctorAvailabilities
            .FirstOrDefaultAsync(a => a.DoctorId == doctorId && a.DayOfWeek == dayOfWeek, ct)
            ?? throw new NotFoundException($"No availability for {dayOfWeek}.");

        db.DoctorAvailabilities.Remove(slot);
        await db.SaveChangesAsync(ct);
    }

    private async Task<Doctor> FindWithUserAsync(Guid id, Guid? healthcareCenterId, CancellationToken ct)
    {
        var query = db.Doctors.Include(d => d.User).Where(d => d.Id == id);
        if (healthcareCenterId.HasValue)
            query = query.Where(d => d.HealthcareCenterId == healthcareCenterId.Value);

        return await query.FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"Doctor {id} not found.");
    }

    private async Task AssertDoctorExistsAsync(Guid doctorId, Guid? healthcareCenterId, CancellationToken ct)
    {
        var query = db.Doctors.Where(d => d.Id == doctorId);
        if (healthcareCenterId.HasValue)
            query = query.Where(d => d.HealthcareCenterId == healthcareCenterId.Value);

        if (!await query.AnyAsync(ct))
            throw new NotFoundException($"Doctor {doctorId} not found.");
    }

    private static DoctorResponse ToResponse(Doctor d) => new()
    {
        Id                 = d.Id,
        UserId             = d.UserId,
        FullName           = $"{d.User.FirstName} {d.User.LastName}",
        Email              = d.User.Email ?? string.Empty,
        Specialization     = d.Specialization,
        HealthcareCenterId = d.HealthcareCenterId,
    };

    private static DoctorAvailabilityResponse ToAvailabilityResponse(DoctorAvailability a) => new()
    {
        Id        = a.Id,
        DoctorId  = a.DoctorId,
        DayOfWeek = a.DayOfWeek.ToString(),
        StartTime = a.StartTime,
        EndTime   = a.EndTime,
    };
}
