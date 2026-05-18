using HealthCare.Common.Constants;
using HealthCare.Common.Exceptions;
using HealthCare.Data;
using HealthCare.Models.DTOs.Prescription;
using HealthCare.Models.Entities;
using HealthCare.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Services.Implementations;

public class PrescriptionService(AppDbContext db) : IPrescriptionService
{
    public async Task<IReadOnlyList<PrescriptionResponse>> GetByPatientAsync(
        Guid patientId, Guid? healthcareCenterId, Guid? doctorProfileId, CancellationToken ct = default)
    {
        await AssertPatientAccessAsync(patientId, healthcareCenterId, doctorProfileId, ct);

        var query = db.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor).ThenInclude(d => d.User)
            .Where(p => p.PatientId == patientId);

        if (doctorProfileId.HasValue)
            query = query.Where(p => p.DoctorId == doctorProfileId.Value);

        return await query.Select(p => ToResponse(p)).ToListAsync(ct);
    }

    public async Task<PrescriptionResponse> GetByIdAsync(
        Guid id, Guid? healthcareCenterId, Guid? doctorProfileId, CancellationToken ct = default)
    {
        var prescription = await FindAsync(id, healthcareCenterId, doctorProfileId, ct);
        return ToResponse(prescription);
    }

    public async Task<PrescriptionResponse> CreateAsync(
        CreatePrescriptionRequest request, Guid doctorProfileId, Guid healthcareCenterId, CancellationToken ct = default)
    {
        var appointment = await db.Appointments
            .FirstOrDefaultAsync(a =>
                a.Id == request.AppointmentId
                && a.DoctorId == doctorProfileId
                && a.HealthcareCenterId == healthcareCenterId, ct)
            ?? throw new NotFoundException($"Appointment {request.AppointmentId} not found or not assigned to you.");

        if (appointment.Status != AppointmentStatus.CheckedIn && appointment.Status != AppointmentStatus.Completed)
            throw new ConflictException("Prescription can only be created for a CheckedIn or Completed appointment.");

        var exists = await db.Prescriptions.AnyAsync(p => p.AppointmentId == request.AppointmentId, ct);
        if (exists)
            throw new ConflictException("A prescription already exists for this appointment.");

        var prescription = new Prescription
        {
            Id                = Guid.NewGuid(),
            AppointmentId     = request.AppointmentId,
            PatientId         = appointment.PatientId,
            DoctorId          = doctorProfileId,
            MedicationDetails = request.MedicationDetails,
            Instructions      = request.Instructions,
            CreatedAt         = DateTime.UtcNow,
        };
        db.Prescriptions.Add(prescription);
        await db.SaveChangesAsync(ct);

        return await GetByIdAsync(prescription.Id, healthcareCenterId, doctorProfileId, ct);
    }

    private async Task<Prescription> FindAsync(Guid id, Guid? healthcareCenterId, Guid? doctorProfileId, CancellationToken ct)
    {
        var query = db.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor).ThenInclude(d => d.User)
            .Where(p => p.Id == id);

        if (doctorProfileId.HasValue)
            query = query.Where(p => p.DoctorId == doctorProfileId.Value);

        if (healthcareCenterId.HasValue)
            query = query.Where(p => p.Patient.HealthcareCenterId == healthcareCenterId.Value);

        return await query.FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"Prescription {id} not found.");
    }

    private async Task AssertPatientAccessAsync(Guid patientId, Guid? healthcareCenterId, Guid? doctorProfileId, CancellationToken ct)
    {
        var query = db.Patients.Where(p => p.Id == patientId);
        if (healthcareCenterId.HasValue)
            query = query.Where(p => p.HealthcareCenterId == healthcareCenterId.Value);

        if (!await query.AnyAsync(ct))
            throw new NotFoundException($"Patient {patientId} not found.");

        if (doctorProfileId.HasValue)
        {
            var hasAppointment = await db.Appointments
                .AnyAsync(a => a.PatientId == patientId && a.DoctorId == doctorProfileId.Value, ct);
            if (!hasAppointment)
                throw new ForbiddenException("You do not have access to this patient's prescriptions.");
        }
    }

    private static PrescriptionResponse ToResponse(Prescription p) => new()
    {
        Id                = p.Id,
        AppointmentId     = p.AppointmentId,
        PatientId         = p.PatientId,
        PatientName       = p.Patient.FullName,
        DoctorId          = p.DoctorId,
        DoctorName        = $"{p.Doctor.User.FirstName} {p.Doctor.User.LastName}",
        MedicationDetails = p.MedicationDetails,
        Instructions      = p.Instructions,
        CreatedAt         = p.CreatedAt,
    };
}
