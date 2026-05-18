using HealthCare.Common.Exceptions;
using HealthCare.Data;
using HealthCare.Models.DTOs.HealthcareCenter;
using HealthCare.Models.Entities;
using HealthCare.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Services.Implementations;

public class HealthcareCenterService(AppDbContext db) : IHealthcareCenterService
{
    public async Task<IReadOnlyList<HealthcareCenterResponse>> GetAllAsync(CancellationToken ct = default)
        => await db.HealthcareCenters
            .Select(h => ToResponse(h))
            .ToListAsync(ct);

    public async Task<HealthcareCenterResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var center = await db.HealthcareCenters.FindAsync([id], ct)
            ?? throw new NotFoundException($"Healthcare center {id} not found.");
        return ToResponse(center);
    }

    public async Task<HealthcareCenterResponse> CreateAsync(CreateHealthcareCenterRequest request, CancellationToken ct = default)
    {
        var center = new HealthcareCenter
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Address = request.Address,
            ContactPhone = request.ContactPhone,
            ContactEmail = request.ContactEmail,
            CreatedAt = DateTime.UtcNow,
        };
        db.HealthcareCenters.Add(center);
        await db.SaveChangesAsync(ct);
        return ToResponse(center);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var center = await db.HealthcareCenters.FindAsync([id], ct)
            ?? throw new NotFoundException($"Healthcare center {id} not found.");
        db.HealthcareCenters.Remove(center);
        await db.SaveChangesAsync(ct);
    }

    private static HealthcareCenterResponse ToResponse(HealthcareCenter h) => new()
    {
        Id = h.Id,
        Name = h.Name,
        Address = h.Address,
        ContactPhone = h.ContactPhone,
        ContactEmail = h.ContactEmail,
        CreatedAt = h.CreatedAt,
    };
}
