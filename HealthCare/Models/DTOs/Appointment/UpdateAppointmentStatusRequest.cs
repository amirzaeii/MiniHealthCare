using System.ComponentModel.DataAnnotations;

namespace HealthCare.Models.DTOs.Appointment;

/// <summary>
/// Request body for advancing an appointment through its workflow.
/// Allowed transitions: Scheduled → CheckedIn | Canceled; CheckedIn → Completed | NoShow.
/// </summary>
public class UpdateAppointmentStatusRequest
{
    /// <summary>
    /// Target status. Valid values: CheckedIn, Completed, Canceled, NoShow.
    /// The transition is validated against the current status and the caller's role.
    /// </summary>
    [Required] public string NewStatus { get; set; } = string.Empty;
}
