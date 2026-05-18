using HealthCare.Models.DTOs.MedicalRecord;

namespace HealthCare.Services.Interfaces;

public interface IMedicalRecordService
{
    Task<IReadOnlyList<MedicalRecordResponse>> GetByPatientAsync(Guid patientId, Guid? healthcareCenterId, Guid? doctorProfileId, CancellationToken ct = default);
    Task<MedicalRecordResponse> GetByIdAsync(Guid id, Guid? healthcareCenterId, Guid? doctorProfileId, CancellationToken ct = default);
    Task<MedicalRecordResponse> CreateAsync(CreateMedicalRecordRequest request, Guid doctorProfileId, Guid healthcareCenterId, CancellationToken ct = default);
}
