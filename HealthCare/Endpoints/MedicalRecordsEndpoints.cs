using HealthCare.Common.Constants;
using HealthCare.Models.DTOs.MedicalRecord;
using HealthCare.Services.Interfaces;

namespace HealthCare.Endpoints;

public static class MedicalRecordsEndpoints
{
    public static RouteGroupBuilder MapMedicalRecordsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/patient/{patientId:guid}", GetByPatient)
            .WithName("GetMedicalRecordsByPatient")
            .WithSummary("Get all medical records for a patient")
            .WithDescription(
                "Returns medical records for the specified patient. " +
                "Doctors see only records they created. " +
                "Admin sees all records for patients in their center. " +
                "Receptionist and Staff cannot access medical records (403).")
            .WithTags("Medical Records")
            .Produces<IReadOnlyList<MedicalRecordResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Doctor, Roles.Admin, Roles.SuperAdmin));

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetMedicalRecordById")
            .WithSummary("Get a specific medical record")
            .WithDescription(
                "Returns the full medical record including diagnosis and clinical notes. " +
                "Accessible by the creating doctor and Admin only.")
            .WithTags("Medical Records")
            .Produces<MedicalRecordResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Doctor, Roles.Admin, Roles.SuperAdmin));

        group.MapPost("/", Create)
            .WithName("CreateMedicalRecord")
            .WithSummary("Create a medical record for an appointment")
            .WithDescription(
                "Records the clinical outcome of an appointment. " +
                "The appointment must be in CheckedIn or Completed status. " +
                "Only one medical record per appointment is allowed. " +
                "The appointment must be assigned to the calling doctor.")
            .WithTags("Medical Records")
            .Produces<MedicalRecordResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Doctor));

        return group;
    }

    private static async Task<IResult> GetByPatient(
        Guid patientId, IMedicalRecordService svc, ITenantService tenant, CancellationToken ct)
    {
        // Admin gets full access; Doctor is scoped to their own records
        var doctorId = tenant.Role == Roles.Doctor ? tenant.DoctorProfileId : null;
        return Results.Ok(await svc.GetByPatientAsync(patientId, tenant.HealthcareCenterId, doctorId, ct));
    }

    private static async Task<IResult> GetById(
        Guid id, IMedicalRecordService svc, ITenantService tenant, CancellationToken ct)
    {
        var doctorId = tenant.Role == Roles.Doctor ? tenant.DoctorProfileId : null;
        return Results.Ok(await svc.GetByIdAsync(id, tenant.HealthcareCenterId, doctorId, ct));
    }

    private static async Task<IResult> Create(
        CreateMedicalRecordRequest request, IMedicalRecordService svc, ITenantService tenant, CancellationToken ct)
    {
        var doctorId = tenant.DoctorProfileId
            ?? throw new InvalidOperationException("Doctor profile ID not found in token.");
        var centerId = tenant.HealthcareCenterId
            ?? throw new InvalidOperationException("HealthcareCenterId not found in token.");
        var result = await svc.CreateAsync(request, doctorId, centerId, ct);
        return Results.Created($"/api/medical-records/{result.Id}", result);
    }
}
