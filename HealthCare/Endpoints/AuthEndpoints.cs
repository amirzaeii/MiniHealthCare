using HealthCare.Models.DTOs.Auth;
using HealthCare.Services.Interfaces;

namespace HealthCare.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register a new user account")
            .WithDescription(
                "Creates a user account with the specified role. " +
                "SuperAdmin accounts can only be created directly via this endpoint with role='SuperAdmin'. " +
                "All other roles require a valid HealthcareCenterId. " +
                "Returns a JWT token on success.")
            .WithTags("Auth")
            .Produces<AuthResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .AllowAnonymous();

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Authenticate and receive a JWT token")
            .WithDescription(
                "Validates email and password credentials. " +
                "Returns a signed JWT bearer token valid for the configured expiration period. " +
                "Use the token in the Authorization: Bearer <token> header for all subsequent requests.")
            .WithTags("Auth")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        return group;
    }

    private static async Task<IResult> Register(RegisterRequest request, IAuthService authService, CancellationToken ct)
    {
        var response = await authService.RegisterAsync(request, ct);
        return Results.Created("/api/auth/login", response);
    }

    private static async Task<IResult> Login(LoginRequest request, IAuthService authService, CancellationToken ct)
    {
        var response = await authService.LoginAsync(request, ct);
        return Results.Ok(response);
    }
}
