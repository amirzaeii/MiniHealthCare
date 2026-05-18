using System.Security.Claims;

namespace HealthCare.Filters;

/// <summary>
/// Endpoint filter applied to all authenticated routes.
/// Ensures the JWT contains a valid hcid claim before allowing execution.
/// SuperAdmin tokens carry an empty hcid — the filter allows them through.
/// Actual data scoping is enforced inside each service via TenantService.
/// </summary>
public class TenantIsolationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var user = ctx.HttpContext.User;

        // Skip unauthenticated requests (anonymous endpoints handle their own auth)
        if (user.Identity?.IsAuthenticated != true)
            return await next(ctx);

        var hcid = user.FindFirstValue("hcid");

        // hcid must be present (SuperAdmin has an empty string, not null)
        if (hcid is null)
            return Results.Forbid();

        return await next(ctx);
    }
}
