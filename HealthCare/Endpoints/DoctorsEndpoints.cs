using HealthCare.Common.Constants;
using HealthCare.Models.DTOs.Doctor;
using HealthCare.Services.Interfaces;

namespace HealthCare.Endpoints;

public static class DoctorsEndpoints
{
    public static RouteGroupBuilder MapDoctorsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAll)
            .WithName("GetAllDoctors")
            .WithSummary("List all doctors in the current center")
            .WithDescription("Returns all doctor profiles scoped to the caller's healthcare center.")
            .WithTags("Doctors")
            .Produces<IReadOnlyList<DoctorResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetDoctorById")
            .WithSummary("Get a doctor by ID")
            .WithDescription("Returns a doctor's profile including specialization and center details.")
            .WithTags("Doctors")
            .Produces<DoctorResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateDoctor")
            .WithSummary("Create a doctor account and profile")
            .WithDescription(
                "Creates a user account with the Doctor role and a corresponding doctor profile. " +
                "The doctor is automatically assigned to the Admin's healthcare center. Admin role required.")
            .WithTags("Doctors")
            .Produces<DoctorResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.SuperAdmin));

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteDoctor")
            .WithSummary("Delete a doctor and their account")
            .WithDescription("Removes the doctor profile and associated user account. Admin role required.")
            .WithTags("Doctors")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.SuperAdmin));

        // Availability sub-routes
        group.MapGet("/{doctorId:guid}/availability", GetAvailabilities)
            .WithName("GetDoctorAvailability")
            .WithSummary("Get a doctor's weekly availability")
            .WithDescription("Returns all weekly availability slots for the specified doctor.")
            .WithTags("Doctors")
            .Produces<IReadOnlyList<DoctorAvailabilityResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{doctorId:guid}/availability", UpsertAvailability)
            .WithName("UpsertDoctorAvailability")
            .WithSummary("Set or update a doctor's availability for a day")
            .WithDescription(
                "Creates or updates the doctor's available time window for the specified day of the week. " +
                "Only one availability block per day is allowed. " +
                "Admin role required.")
            .WithTags("Doctors")
            .Produces<DoctorAvailabilityResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.SuperAdmin));

        group.MapDelete("/{doctorId:guid}/availability/{dayOfWeek:int}", DeleteAvailability)
            .WithName("DeleteDoctorAvailability")
            .WithSummary("Remove a doctor's availability for a specific day")
            .WithDescription("Deletes the availability slot for the given day of week (0=Sunday ... 6=Saturday). Admin role required.")
            .WithTags("Doctors")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.SuperAdmin));

        return group;
    }

    private static async Task<IResult> GetAll(IDoctorService svc, ITenantService tenant, CancellationToken ct)
        => Results.Ok(await svc.GetAllAsync(tenant.HealthcareCenterId, ct));

    private static async Task<IResult> GetById(Guid id, IDoctorService svc, ITenantService tenant, CancellationToken ct)
        => Results.Ok(await svc.GetByIdAsync(id, tenant.HealthcareCenterId, ct));

    private static async Task<IResult> Create(CreateDoctorRequest request, IDoctorService svc, ITenantService tenant, CancellationToken ct)
    {
        var centerId = tenant.HealthcareCenterId
            ?? throw new InvalidOperationException("HealthcareCenterId required.");
        var result = await svc.CreateAsync(request, centerId, ct);
        return Results.Created($"/api/doctors/{result.Id}", result);
    }

    private static async Task<IResult> Delete(Guid id, IDoctorService svc, ITenantService tenant, CancellationToken ct)
    {
        await svc.DeleteAsync(id, tenant.HealthcareCenterId, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> GetAvailabilities(Guid doctorId, IDoctorService svc, ITenantService tenant, CancellationToken ct)
        => Results.Ok(await svc.GetAvailabilitiesAsync(doctorId, tenant.HealthcareCenterId, ct));

    private static async Task<IResult> UpsertAvailability(Guid doctorId, DoctorAvailabilityRequest request, IDoctorService svc, ITenantService tenant, CancellationToken ct)
        => Results.Ok(await svc.UpsertAvailabilityAsync(doctorId, request, tenant.HealthcareCenterId, ct));

    private static async Task<IResult> DeleteAvailability(Guid doctorId, int dayOfWeek, IDoctorService svc, ITenantService tenant, CancellationToken ct)
    {
        await svc.DeleteAvailabilityAsync(doctorId, (DayOfWeek)dayOfWeek, tenant.HealthcareCenterId, ct);
        return Results.NoContent();
    }
}
