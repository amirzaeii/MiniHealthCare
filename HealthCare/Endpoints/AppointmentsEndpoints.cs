using HealthCare.Common.Constants;
using HealthCare.Common.Exceptions;
using HealthCare.Models.DTOs.Appointment;
using HealthCare.Services.Interfaces;

namespace HealthCare.Endpoints;

public static class AppointmentsEndpoints
{
    public static RouteGroupBuilder MapAppointmentsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAll)
            .WithName("GetAllAppointments")
            .WithSummary("List appointments with optional filters")
            .WithDescription(
                "Returns appointments scoped to the caller's healthcare center. " +
                "Optional query parameters: doctorId (filter by doctor), patientId (filter by patient). " +
                "Doctors automatically see only their own appointments. " +
                "Response includes scheduling information. Medical records and prescriptions are available via separate endpoints.")
            .WithTags("Appointments")
            .Produces<IReadOnlyList<AppointmentResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetAppointmentById")
            .WithSummary("Get a specific appointment")
            .WithDescription("Returns full appointment details including patient and doctor names.")
            .WithTags("Appointments")
            .Produces<AppointmentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateAppointment")
            .WithSummary("Schedule a new appointment")
            .WithDescription(
                "Books an appointment for a patient with a doctor. " +
                "Validates that: (1) the requested slot falls within the doctor's availability, " +
                "(2) the doctor has no overlapping active appointments, " +
                "(3) the patient has no overlapping active appointments. " +
                "Canceled and NoShow appointments are excluded from conflict checks. " +
                "Receptionist, Staff, and Admin roles can schedule appointments.")
            .WithTags("Appointments")
            .Produces<AppointmentResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.Receptionist, Roles.Staff));

        group.MapPatch("/{id:guid}/status", UpdateStatus)
            .WithName("UpdateAppointmentStatus")
            .WithSummary("Advance an appointment through its workflow")
            .WithDescription(
                "Transitions an appointment to the next status. Allowed transitions: " +
                "Scheduled → CheckedIn (Receptionist, Staff, Admin), " +
                "Scheduled → Canceled (all roles), " +
                "CheckedIn → Completed (Doctor, Admin), " +
                "CheckedIn → NoShow (Doctor, Admin). " +
                "Completed, Canceled, and NoShow are terminal — no further transitions are allowed.")
            .WithTags("Appointments")
            .Produces<AppointmentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> GetAll(
        IAppointmentService svc, ITenantService tenant,
        Guid? doctorId, Guid? patientId,
        CancellationToken ct)
    {
        // Doctors automatically see only their own appointments
        var effectiveDoctorId = tenant.Role == Roles.Doctor ? tenant.DoctorProfileId : doctorId;
        return Results.Ok(await svc.GetAllAsync(tenant.HealthcareCenterId, effectiveDoctorId, patientId, ct));
    }

    private static async Task<IResult> GetById(Guid id, IAppointmentService svc, ITenantService tenant, CancellationToken ct)
        => Results.Ok(await svc.GetByIdAsync(id, tenant.HealthcareCenterId, ct));

    private static async Task<IResult> Create(
        CreateAppointmentRequest request, IAppointmentService svc, ITenantService tenant, CancellationToken ct)
    {
        var centerId = tenant.HealthcareCenterId
            ?? throw new ForbiddenException("SuperAdmin cannot directly schedule appointments — use a center-scoped account.");
        var result = await svc.CreateAsync(request, centerId, ct);
        return Results.Created($"/api/appointments/{result.Id}", result);
    }

    private static async Task<IResult> UpdateStatus(
        Guid id, UpdateAppointmentStatusRequest request, IAppointmentService svc, ITenantService tenant, CancellationToken ct)
        => Results.Ok(await svc.UpdateStatusAsync(id, request.NewStatus, tenant.HealthcareCenterId, tenant.Role, ct));
}
