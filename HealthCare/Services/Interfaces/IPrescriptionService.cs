using HealthCare.Models.DTOs.Prescription;

namespace HealthCare.Services.Interfaces;

public interface IPrescriptionService
{
    Task<IReadOnlyList<PrescriptionResponse>> GetByPatientAsync(Guid patientId, Guid? healthcareCenterId, Guid? doctorProfileId, CancellationToken ct = default);
    Task<PrescriptionResponse> GetByIdAsync(Guid id, Guid? healthcareCenterId, Guid? doctorProfileId, CancellationToken ct = default);
    Task<PrescriptionResponse> CreateAsync(CreatePrescriptionRequest request, Guid doctorProfileId, Guid healthcareCenterId, CancellationToken ct = default);
}
