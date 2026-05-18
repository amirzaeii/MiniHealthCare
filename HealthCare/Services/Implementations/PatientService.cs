using HealthCare.Common.Exceptions;
using HealthCare.Data;
using HealthCare.Models.DTOs.Patient;
using HealthCare.Models.Entities;
using HealthCare.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Services.Implementations;

public class PatientService(AppDbContext db) : IPatientService
{
    public async Task<IReadOnlyList<PatientResponse>> GetAllAsync(Guid? healthcareCenterId, CancellationToken ct = default)
    {
        var query = db.Patients.AsQueryable();
        if (healthcareCenterId.HasValue)
            query = query.Where(p => p.HealthcareCenterId == healthcareCenterId.Value);

        return await query.Select(p => ToResponse(p)).ToListAsync(ct);
    }

    public async Task<PatientResponse> GetByIdAsync(Guid id, Guid? healthcareCenterId, CancellationToken ct = default)
    {
        var patient = await FindAsync(id, healthcareCenterId, ct);
        return ToResponse(patient);
    }

    public async Task<PatientResponse> CreateAsync(CreatePatientRequest request, Guid healthcareCenterId, CancellationToken ct = default)
    {
        var patient = new Patient
        {
            Id                 = Guid.NewGuid(),
            FullName           = request.FullName,
            DateOfBirth        = request.DateOfBirth,
            Gender             = request.Gender,
            ContactPhone       = request.ContactPhone,
            ContactEmail       = request.ContactEmail,
            Address            = request.Address,
            HealthcareCenterId = healthcareCenterId,
        };
        db.Patients.Add(patient);
        await db.SaveChangesAsync(ct);
        return ToResponse(patient);
    }

    public async Task<PatientResponse> UpdateAsync(Guid id, UpdatePatientRequest request, Guid? healthcareCenterId, CancellationToken ct = default)
    {
        var patient = await FindAsync(id, healthcareCenterId, ct);
        patient.FullName     = request.FullName;
        patient.DateOfBirth  = request.DateOfBirth;
        patient.Gender       = request.Gender;
        patient.ContactPhone = request.ContactPhone;
        patient.ContactEmail = request.ContactEmail;
        patient.Address      = request.Address;
        await db.SaveChangesAsync(ct);
        return ToResponse(patient);
    }

    public async Task DeleteAsync(Guid id, Guid? healthcareCenterId, CancellationToken ct = default)
    {
        var patient = await FindAsync(id, healthcareCenterId, ct);
        db.Patients.Remove(patient);
        await db.SaveChangesAsync(ct);
    }

    private async Task<Patient> FindAsync(Guid id, Guid? healthcareCenterId, CancellationToken ct)
    {
        var query = db.Patients.Where(p => p.Id == id);
        if (healthcareCenterId.HasValue)
            query = query.Where(p => p.HealthcareCenterId == healthcareCenterId.Value);

        return await query.FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"Patient {id} not found.");
    }

    private static PatientResponse ToResponse(Patient p) => new()
    {
        Id                 = p.Id,
        FullName           = p.FullName,
        DateOfBirth        = p.DateOfBirth,
        Gender             = p.Gender,
        ContactPhone       = p.ContactPhone,
        ContactEmail       = p.ContactEmail,
        Address            = p.Address,
        HealthcareCenterId = p.HealthcareCenterId,
    };
}
