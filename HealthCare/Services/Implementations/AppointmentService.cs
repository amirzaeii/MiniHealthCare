using HealthCare.Common.Constants;
using HealthCare.Common.Exceptions;
using HealthCare.Data;
using HealthCare.Models.DTOs.Appointment;
using HealthCare.Models.Entities;
using HealthCare.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Services.Implementations;

public class AppointmentService(AppDbContext db) : IAppointmentService
{
    // Valid status transitions per role
    private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new()
    {
        [AppointmentStatus.Scheduled] = [AppointmentStatus.CheckedIn, AppointmentStatus.Canceled],
        [AppointmentStatus.CheckedIn] = [AppointmentStatus.Completed, AppointmentStatus.NoShow],
        [AppointmentStatus.Completed] = [],
        [AppointmentStatus.Canceled]  = [],
        [AppointmentStatus.NoShow]    = [],
    };

    // Which roles may trigger which target status
    private static readonly Dictionary<string, HashSet<string>> RoleAllowedTargets = new()
    {
        [Roles.Admin]        = [AppointmentStatus.CheckedIn, AppointmentStatus.Canceled, AppointmentStatus.Completed, AppointmentStatus.NoShow],
        [Roles.Doctor]       = [AppointmentStatus.Canceled, AppointmentStatus.Completed, AppointmentStatus.NoShow],
        [Roles.Receptionist] = [AppointmentStatus.CheckedIn, AppointmentStatus.Canceled],
        [Roles.Staff]        = [AppointmentStatus.CheckedIn, AppointmentStatus.Canceled],
    };

    public async Task<IReadOnlyList<AppointmentResponse>> GetAllAsync(
        Guid? healthcareCenterId, Guid? doctorId, Guid? patientId, CancellationToken ct = default)
    {
        var query = db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .AsQueryable();

        if (healthcareCenterId.HasValue)
            query = query.Where(a => a.HealthcareCenterId == healthcareCenterId.Value);
        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId.Value);
        if (patientId.HasValue)
            query = query.Where(a => a.PatientId == patientId.Value);

        return await query.Select(a => ToResponse(a)).ToListAsync(ct);
    }

    public async Task<AppointmentResponse> GetByIdAsync(Guid id, Guid? healthcareCenterId, CancellationToken ct = default)
    {
        var appointment = await FindAsync(id, healthcareCenterId, ct);
        return ToResponse(appointment);
    }

    public async Task<AppointmentResponse> CreateAsync(
        CreateAppointmentRequest request, Guid healthcareCenterId, CancellationToken ct = default)
    {
        if (request.ScheduledEnd <= request.ScheduledStart)
            throw new ConflictException("ScheduledEnd must be after ScheduledStart.");

        // Validate doctor exists in center
        var doctor = await db.Doctors
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId && d.HealthcareCenterId == healthcareCenterId, ct)
            ?? throw new NotFoundException($"Doctor {request.DoctorId} not found in this center.");

        // Validate patient exists in center
        var patientExists = await db.Patients
            .AnyAsync(p => p.Id == request.PatientId && p.HealthcareCenterId == healthcareCenterId, ct);
        if (!patientExists)
            throw new NotFoundException($"Patient {request.PatientId} not found in this center.");

        // Validate slot is within doctor availability for that day
        var dayOfWeek = request.ScheduledStart.DayOfWeek;
        var availability = await db.DoctorAvailabilities
            .FirstOrDefaultAsync(a => a.DoctorId == request.DoctorId && a.DayOfWeek == dayOfWeek, ct)
            ?? throw new ConflictException($"Doctor has no availability on {dayOfWeek}.");

        var slotStart = TimeOnly.FromDateTime(request.ScheduledStart);
        var slotEnd   = TimeOnly.FromDateTime(request.ScheduledEnd);
        if (slotStart < availability.StartTime || slotEnd > availability.EndTime)
            throw new ConflictException("Requested slot falls outside the doctor's available hours.");

        // Check doctor scheduling conflict (excluding terminal statuses)
        var terminalStatuses = new[] { AppointmentStatus.Canceled, AppointmentStatus.NoShow };
        var doctorBusy = await db.Appointments.AnyAsync(a =>
            a.DoctorId == request.DoctorId
            && !terminalStatuses.Contains(a.Status)
            && a.ScheduledStart < request.ScheduledEnd
            && a.ScheduledEnd   > request.ScheduledStart, ct);
        if (doctorBusy)
            throw new ConflictException("Doctor has an overlapping appointment at the requested time.");

        // Check patient scheduling conflict
        var patientBusy = await db.Appointments.AnyAsync(a =>
            a.PatientId == request.PatientId
            && !terminalStatuses.Contains(a.Status)
            && a.ScheduledStart < request.ScheduledEnd
            && a.ScheduledEnd   > request.ScheduledStart, ct);
        if (patientBusy)
            throw new ConflictException("Patient has an overlapping appointment at the requested time.");

        var now = DateTime.UtcNow;
        var appointment = new Appointment
        {
            Id                 = Guid.NewGuid(),
            PatientId          = request.PatientId,
            DoctorId           = request.DoctorId,
            ScheduledStart     = request.ScheduledStart,
            ScheduledEnd       = request.ScheduledEnd,
            Status             = AppointmentStatus.Scheduled,
            Notes              = request.Notes,
            HealthcareCenterId = healthcareCenterId,
            CreatedAt          = now,
            UpdatedAt          = now,
        };
        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(ct);

        // Reload with navigation properties for response
        return await GetByIdAsync(appointment.Id, healthcareCenterId, ct);
    }

    public async Task<AppointmentResponse> UpdateStatusAsync(
        Guid id, string newStatus, Guid? healthcareCenterId, string callerRole, CancellationToken ct = default)
    {
        var appointment = await FindAsync(id, healthcareCenterId, ct);

        // Validate state machine transition
        if (!AllowedTransitions.TryGetValue(appointment.Status, out var allowed) || !allowed.Contains(newStatus))
            throw new ConflictException(
                $"Transition from '{appointment.Status}' to '{newStatus}' is not permitted.");

        // Validate caller role is permitted to set this target status
        if (!RoleAllowedTargets.TryGetValue(callerRole, out var roleTargets) || !roleTargets.Contains(newStatus))
            throw new ForbiddenException(
                $"Role '{callerRole}' is not permitted to set status '{newStatus}'.");

        appointment.Status    = newStatus;
        appointment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return ToResponse(appointment);
    }

    private async Task<Appointment> FindAsync(Guid id, Guid? healthcareCenterId, CancellationToken ct)
    {
        var query = db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Where(a => a.Id == id);

        if (healthcareCenterId.HasValue)
            query = query.Where(a => a.HealthcareCenterId == healthcareCenterId.Value);

        return await query.FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"Appointment {id} not found.");
    }

    private static AppointmentResponse ToResponse(Appointment a) => new()
    {
        Id                 = a.Id,
        PatientId          = a.PatientId,
        PatientName        = a.Patient.FullName,
        DoctorId           = a.DoctorId,
        DoctorName         = $"{a.Doctor.User.FirstName} {a.Doctor.User.LastName}",
        ScheduledStart     = a.ScheduledStart,
        ScheduledEnd       = a.ScheduledEnd,
        Status             = a.Status,
        Notes              = a.Notes,
        HealthcareCenterId = a.HealthcareCenterId,
        CreatedAt          = a.CreatedAt,
        UpdatedAt          = a.UpdatedAt,
    };
}
