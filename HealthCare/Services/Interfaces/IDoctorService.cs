using HealthCare.Models.DTOs.Doctor;

namespace HealthCare.Services.Interfaces;

public interface IDoctorService
{
    Task<IReadOnlyList<DoctorResponse>> GetAllAsync(Guid? healthcareCenterId, CancellationToken ct = default);
    Task<DoctorResponse> GetByIdAsync(Guid id, Guid? healthcareCenterId, CancellationToken ct = default);
    Task<DoctorResponse> CreateAsync(CreateDoctorRequest request, Guid healthcareCenterId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid? healthcareCenterId, CancellationToken ct = default);

    Task<IReadOnlyList<DoctorAvailabilityResponse>> GetAvailabilitiesAsync(Guid doctorId, Guid? healthcareCenterId, CancellationToken ct = default);
    Task<DoctorAvailabilityResponse> UpsertAvailabilityAsync(Guid doctorId, DoctorAvailabilityRequest request, Guid? healthcareCenterId, CancellationToken ct = default);
    Task DeleteAvailabilityAsync(Guid doctorId, DayOfWeek dayOfWeek, Guid? healthcareCenterId, CancellationToken ct = default);
}
