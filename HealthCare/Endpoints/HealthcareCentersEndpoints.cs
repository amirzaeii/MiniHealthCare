using HealthCare.Common.Constants;
using HealthCare.Models.DTOs.HealthcareCenter;
using HealthCare.Services.Interfaces;

namespace HealthCare.Endpoints;

public static class HealthcareCentersEndpoints
{
    public static RouteGroupBuilder MapHealthcareCentersEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAll)
            .WithName("GetAllHealthcareCenters")
            .WithSummary("List all healthcare centers")
            .WithDescription("Returns all registered hospitals and clinics. Accessible by SuperAdmin only.")
            .WithTags("Healthcare Centers")
            .Produces<IReadOnlyList<HealthcareCenterResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.SuperAdmin));

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetHealthcareCenterById")
            .WithSummary("Get a healthcare center by ID")
            .WithDescription("Returns details of a specific healthcare center. Accessible by SuperAdmin and Admin.")
            .WithTags("Healthcare Centers")
            .Produces<HealthcareCenterResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.SuperAdmin, Roles.Admin));

        group.MapPost("/", Create)
            .WithName("CreateHealthcareCenter")
            .WithSummary("Register a new healthcare center")
            .WithDescription(
                "Creates a new hospital or clinic entry. SuperAdmin only. " +
                "The center ID returned here is needed when registering users for that center.")
            .WithTags("Healthcare Centers")
            .Produces<HealthcareCenterResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.SuperAdmin));

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteHealthcareCenter")
            .WithSummary("Delete a healthcare center")
            .WithDescription(
                "Permanently removes a healthcare center. SuperAdmin only. " +
                "Will fail if the center still has associated users, patients, or appointments (Restrict delete behavior).")
            .WithTags("Healthcare Centers")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole(Roles.SuperAdmin));

        return group;
    }

    private static async Task<IResult> GetAll(IHealthcareCenterService svc, CancellationToken ct)
        => Results.Ok(await svc.GetAllAsync(ct));

    private static async Task<IResult> GetById(Guid id, IHealthcareCenterService svc, CancellationToken ct)
        => Results.Ok(await svc.GetByIdAsync(id, ct));

    private static async Task<IResult> Create(CreateHealthcareCenterRequest request, IHealthcareCenterService svc, CancellationToken ct)
    {
        var result = await svc.CreateAsync(request, ct);
        return Results.Created($"/api/healthcare-centers/{result.Id}", result);
    }

    private static async Task<IResult> Delete(Guid id, IHealthcareCenterService svc, CancellationToken ct)
    {
        await svc.DeleteAsync(id, ct);
        return Results.NoContent();
    }
}
