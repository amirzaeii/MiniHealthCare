using HealthCare.Common.Constants;
using HealthCare.Models.DTOs.Prescription;
using HealthCare.Services.Interfaces;

namespace HealthCare.Endpoints;

public static class PrescriptionsEndpoints
{
    public static RouteGroupBuilder MapPrescriptionsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/patient/{patientId:guid}", GetByPatient)
            .WithName("GetPrescriptionsByPatient")
            .WithSummary("Get all prescriptions for a patient")
            .WithDescription(
                "Returns prescriptions for the specified patient. " +
                "Doctors see only prescriptions they issued. " +
                "Admin sees all prescriptions for patients in their center. " +
                "Receptionist and Staff cannot access prescriptions (403).")
            .WithTags("Prescriptions")
            .Produces<IReadOnlyList<PrescriptionResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Doctor, Roles.Admin, Roles.SuperAdmin));

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetPrescriptionById")
            .WithSummary("Get a specific prescription")
            .WithDescription(
                "Returns prescription details including medication and administration instructions. " +
                "Accessible by the issuing doctor and Admin only.")
            .WithTags("Prescriptions")
            .Produces<PrescriptionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Doctor, Roles.Admin, Roles.SuperAdmin));

        group.MapPost("/", Create)
            .WithName("CreatePrescription")
            .WithSummary("Issue a prescription for an appointment")
            .WithDescription(
                "Issues a prescription for a patient based on an appointment. " +
                "The appointment must be in CheckedIn or Completed status. " +
                "Only one prescription per appointment is allowed. " +
                "The calling doctor must be the one assigned to the appointment.")
            .WithTags("Prescriptions")
            .Produces<PrescriptionResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Doctor));

        return group;
    }

    private static async Task<IResult> GetByPatient(
        Guid patientId, IPrescriptionService svc, ITenantService tenant, CancellationToken ct)
    {
        var doctorId = tenant.Role == Roles.Doctor ? tenant.DoctorProfileId : null;
        return Results.Ok(await svc.GetByPatientAsync(patientId, tenant.HealthcareCenterId, doctorId, ct));
    }

    private static async Task<IResult> GetById(
        Guid id, IPrescriptionService svc, ITenantService tenant, CancellationToken ct)
    {
        var doctorId = tenant.Role == Roles.Doctor ? tenant.DoctorProfileId : null;
        return Results.Ok(await svc.GetByIdAsync(id, tenant.HealthcareCenterId, doctorId, ct));
    }

    private static async Task<IResult> Create(
        CreatePrescriptionRequest request, IPrescriptionService svc, ITenantService tenant, CancellationToken ct)
    {
        var doctorId = tenant.DoctorProfileId
            ?? throw new InvalidOperationException("Doctor profile ID not found in token.");
        var centerId = tenant.HealthcareCenterId
            ?? throw new InvalidOperationException("HealthcareCenterId not found in token.");
        var result = await svc.CreateAsync(request, doctorId, centerId, ct);
        return Results.Created($"/api/prescriptions/{result.Id}", result);
    }
}
