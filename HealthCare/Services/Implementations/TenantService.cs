using System.Security.Claims;
using HealthCare.Common.Constants;
using HealthCare.Services.Interfaces;

namespace HealthCare.Services.Implementations;

public class TenantService(IHttpContextAccessor accessor) : ITenantService
{
    private readonly ClaimsPrincipal _user = accessor.HttpContext?.User
        ?? throw new InvalidOperationException("No HTTP context available.");

    public Guid? HealthcareCenterId
    {
        get
        {
            var hcid = _user.FindFirstValue("hcid");
            return string.IsNullOrEmpty(hcid) ? null : Guid.Parse(hcid);
        }
    }

    public string UserId => _user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    public string Role => _user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    public Guid? DoctorProfileId
    {
        get
        {
            var did = _user.FindFirstValue("doctor_id");
            return did is null ? null : Guid.Parse(did);
        }
    }

    public bool IsSuperAdmin => Role == Roles.SuperAdmin;
}
