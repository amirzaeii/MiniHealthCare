using HealthCare.Common.Constants;
using HealthCare.Models.DTOs.Patient;
using HealthCare.Services.Interfaces;

namespace HealthCare.Endpoints;

public static class PatientsEndpoints
{
    public static RouteGroupBuilder MapPatientsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAll)
            .WithName("GetAllPatients")
            .WithSummary("List all patients in the current center")
            .WithDescription(
                "Returns all patients registered in the caller's healthcare center. " +
                "Results are automatically scoped to the caller's center via their JWT. " +
                "SuperAdmin receives all patients across all centers.")
            .WithTags("Patients")
            .Produces<IReadOnlyList<PatientResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetPatientById")
            .WithSummary("Get a patient by ID")
            .WithDescription(
                "Returns full patient details. " +
                "Access is scoped to the caller's healthcare center. " +
                "Doctors must have at least one appointment with the patient to access their medical records (separate endpoint).")
            .WithTags("Patients")
            .Produces<PatientResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/", Create)
            .WithName("CreatePatient")
            .WithSummary("Register a new patient")
            .WithDescription(
                "Creates a patient record in the caller's healthcare center. " +
                "Requires Admin role. Patient is automatically assigned to the Admin's center.")
            .WithTags("Patients")
            .Produces<PatientResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.SuperAdmin));

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdatePatient")
            .WithSummary("Update a patient's details")
            .WithDescription("Updates contact information and demographics for an existing patient. Admin role required.")
            .WithTags("Patients")
            .Produces<PatientResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.SuperAdmin));

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeletePatient")
            .WithSummary("Delete a patient record")
            .WithDescription("Permanently removes a patient. Admin role required. Will fail if active appointments exist.")
            .WithTags("Patients")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.SuperAdmin));

        return group;
    }

    private static async Task<IResult> GetAll(IPatientService svc, ITenantService tenant, CancellationToken ct)
        => Results.Ok(await svc.GetAllAsync(tenant.HealthcareCenterId, ct));

    private static async Task<IResult> GetById(Guid id, IPatientService svc, ITenantService tenant, CancellationToken ct)
        => Results.Ok(await svc.GetByIdAsync(id, tenant.HealthcareCenterId, ct));

    private static async Task<IResult> Create(CreatePatientRequest request, IPatientService svc, ITenantService tenant, CancellationToken ct)
    {
        var centerId = tenant.HealthcareCenterId
            ?? throw new InvalidOperationException("SuperAdmin must provide a HealthcareCenterId via the request.");
        var result = await svc.CreateAsync(request, centerId, ct);
        return Results.Created($"/api/patients/{result.Id}", result);
    }

    private static async Task<IResult> Update(Guid id, UpdatePatientRequest request, IPatientService svc, ITenantService tenant, CancellationToken ct)
        => Results.Ok(await svc.UpdateAsync(id, request, tenant.HealthcareCenterId, ct));

    private static async Task<IResult> Delete(Guid id, IPatientService svc, ITenantService tenant, CancellationToken ct)
    {
        await svc.DeleteAsync(id, tenant.HealthcareCenterId, ct);
        return Results.NoContent();
    }
}
