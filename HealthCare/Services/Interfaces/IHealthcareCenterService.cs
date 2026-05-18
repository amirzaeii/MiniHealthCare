using HealthCare.Models.DTOs.HealthcareCenter;

namespace HealthCare.Services.Interfaces;

public interface IHealthcareCenterService
{
    Task<IReadOnlyList<HealthcareCenterResponse>> GetAllAsync(CancellationToken ct = default);
    Task<HealthcareCenterResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<HealthcareCenterResponse> CreateAsync(CreateHealthcareCenterRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
