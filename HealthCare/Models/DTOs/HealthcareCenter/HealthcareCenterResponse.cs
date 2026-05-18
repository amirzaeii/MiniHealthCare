namespace HealthCare.Models.DTOs.HealthcareCenter;

/// <summary>Healthcare center details.</summary>
public class HealthcareCenterResponse
{
    /// <summary>Unique identifier of the center.</summary>
    public Guid Id { get; set; }

    /// <summary>Name of the hospital or clinic.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Physical address.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Contact phone number.</summary>
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>Contact email address.</summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>Date and time the center was registered (UTC).</summary>
    public DateTime CreatedAt { get; set; }
}
