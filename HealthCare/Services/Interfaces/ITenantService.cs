namespace HealthCare.Services.Interfaces;

public interface ITenantService
{
    /// <summary>Healthcare center scoping the current request. Null for SuperAdmin.</summary>
    Guid? HealthcareCenterId { get; }

    /// <summary>ID of the authenticated user.</summary>
    string UserId { get; }

    /// <summary>Role of the authenticated user.</summary>
    string Role { get; }

    /// <summary>Doctor profile ID when the current user is a Doctor. Null otherwise.</summary>
    Guid? DoctorProfileId { get; }

    /// <summary>True when the caller is a SuperAdmin (no center scoping applied).</summary>
    bool IsSuperAdmin { get; }
}
