using HealthCare.Models.DTOs.Appointment;

namespace HealthCare.Services.Interfaces;

public interface IAppointmentService
{
    Task<IReadOnlyList<AppointmentResponse>> GetAllAsync(Guid? healthcareCenterId, Guid? doctorId, Guid? patientId, CancellationToken ct = default);
    Task<AppointmentResponse> GetByIdAsync(Guid id, Guid? healthcareCenterId, CancellationToken ct = default);
    Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, Guid healthcareCenterId, CancellationToken ct = default);
    Task<AppointmentResponse> UpdateStatusAsync(Guid id, string newStatus, Guid? healthcareCenterId, string callerRole, CancellationToken ct = default);
}
