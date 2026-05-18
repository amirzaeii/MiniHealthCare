using HealthCare.Models.DTOs.Patient;

namespace HealthCare.Services.Interfaces;

public interface IPatientService
{
    Task<IReadOnlyList<PatientResponse>> GetAllAsync(Guid? healthcareCenterId, CancellationToken ct = default);
    Task<PatientResponse> GetByIdAsync(Guid id, Guid? healthcareCenterId, CancellationToken ct = default);
    Task<PatientResponse> CreateAsync(CreatePatientRequest request, Guid healthcareCenterId, CancellationToken ct = default);
    Task<PatientResponse> UpdateAsync(Guid id, UpdatePatientRequest request, Guid? healthcareCenterId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid? healthcareCenterId, CancellationToken ct = default);
}
