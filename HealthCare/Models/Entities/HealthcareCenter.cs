namespace HealthCare.Models.Entities;

public class HealthcareCenter
{
    public Guid Id { get; set; }

    /// <summary>Full name of the hospital or clinic.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Physical address of the healthcare center.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Primary contact phone number.</summary>
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>Primary contact email address.</summary>
    public string ContactEmail { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public ICollection<Patient> Patients { get; set; } = [];
    public ICollection<Doctor> Doctors { get; set; } = [];
    public ICollection<Receptionist> Receptionists { get; set; } = [];
    public ICollection<Staff> Staff { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
}
