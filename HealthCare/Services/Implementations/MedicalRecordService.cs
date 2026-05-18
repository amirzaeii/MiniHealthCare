using HealthCare.Common.Constants;
using HealthCare.Common.Exceptions;
using HealthCare.Data;
using HealthCare.Models.DTOs.MedicalRecord;
using HealthCare.Models.Entities;
using HealthCare.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Services.Implementations;

public class MedicalRecordService(AppDbContext db) : IMedicalRecordService
{
    public async Task<IReadOnlyList<MedicalRecordResponse>> GetByPatientAsync(
        Guid patientId, Guid? healthcareCenterId, Guid? doctorProfileId, CancellationToken ct = default)
    {
        // Verify patient is accessible in the center
        await AssertPatientAccessAsync(patientId, healthcareCenterId, doctorProfileId, ct);

        var query = db.MedicalRecords
            .Include(m => m.Patient)
            .Include(m => m.Doctor).ThenInclude(d => d.User)
            .Where(m => m.PatientId == patientId);

        // Doctors only see records they created
        if (doctorProfileId.HasValue)
            query = query.Where(m => m.DoctorId == doctorProfileId.Value);

        return await query.Select(m => ToResponse(m)).ToListAsync(ct);
    }

    public async Task<MedicalRecordResponse> GetByIdAsync(
        Guid id, Guid? healthcareCenterId, Guid? doctorProfileId, CancellationToken ct = default)
    {
        var record = await FindAsync(id, healthcareCenterId, doctorProfileId, ct);
        return ToResponse(record);
    }

    public async Task<MedicalRecordResponse> CreateAsync(
        CreateMedicalRecordRequest request, Guid doctorProfileId, Guid healthcareCenterId, CancellationToken ct = default)
    {
        // Appointment must belong to this doctor and center
        var appointment = await db.Appointments
            .FirstOrDefaultAsync(a =>
                a.Id == request.AppointmentId
                && a.DoctorId == doctorProfileId
                && a.HealthcareCenterId == healthcareCenterId, ct)
            ?? throw new NotFoundException($"Appointment {request.AppointmentId} not found or not assigned to you.");

        // Appointment must have started (CheckedIn)
        if (appointment.Status != AppointmentStatus.CheckedIn && appointment.Status != AppointmentStatus.Completed)
            throw new ConflictException("A medical record can only be created for a CheckedIn or Completed appointment.");

        // Prevent duplicate records
        var exists = await db.MedicalRecords.AnyAsync(m => m.AppointmentId == request.AppointmentId, ct);
        if (exists)
            throw new ConflictException("A medical record already exists for this appointment.");

        var now = DateTime.UtcNow;
        var record = new MedicalRecord
        {
            Id            = Guid.NewGuid(),
            AppointmentId = request.AppointmentId,
            PatientId     = appointment.PatientId,
            DoctorId      = doctorProfileId,
            Diagnosis     = request.Diagnosis,
            Notes         = request.Notes,
            CreatedAt     = now,
            UpdatedAt     = now,
        };
        db.MedicalRecords.Add(record);
        await db.SaveChangesAsync(ct);

        return await GetByIdAsync(record.Id, healthcareCenterId, doctorProfileId, ct);
    }

    private async Task<MedicalRecord> FindAsync(Guid id, Guid? healthcareCenterId, Guid? doctorProfileId, CancellationToken ct)
    {
        var query = db.MedicalRecords
            .Include(m => m.Patient)
            .Include(m => m.Doctor).ThenInclude(d => d.User)
            .Where(m => m.Id == id);

        if (doctorProfileId.HasValue)
            query = query.Where(m => m.DoctorId == doctorProfileId.Value);

        // Scope to center via patient
        if (healthcareCenterId.HasValue)
            query = query.Where(m => m.Patient.HealthcareCenterId == healthcareCenterId.Value);

        return await query.FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"Medical record {id} not found.");
    }

    private async Task AssertPatientAccessAsync(Guid patientId, Guid? healthcareCenterId, Guid? doctorProfileId, CancellationToken ct)
    {
        var query = db.Patients.Where(p => p.Id == patientId);
        if (healthcareCenterId.HasValue)
            query = query.Where(p => p.HealthcareCenterId == healthcareCenterId.Value);

        var patientExists = await query.AnyAsync(ct);
        if (!patientExists)
            throw new NotFoundException($"Patient {patientId} not found.");

        // Doctor must have at least one appointment with the patient
        if (doctorProfileId.HasValue)
        {
            var hasAppointment = await db.Appointments
                .AnyAsync(a => a.PatientId == patientId && a.DoctorId == doctorProfileId.Value, ct);
            if (!hasAppointment)
                throw new ForbiddenException("You do not have access to this patient's records.");
        }
    }

    private static MedicalRecordResponse ToResponse(MedicalRecord m) => new()
    {
        Id            = m.Id,
        AppointmentId = m.AppointmentId,
        PatientId     = m.PatientId,
        PatientName   = m.Patient.FullName,
        DoctorId      = m.DoctorId,
        DoctorName    = $"{m.Doctor.User.FirstName} {m.Doctor.User.LastName}",
        Diagnosis     = m.Diagnosis,
        Notes         = m.Notes,
        CreatedAt     = m.CreatedAt,
        UpdatedAt     = m.UpdatedAt,
    };
}
