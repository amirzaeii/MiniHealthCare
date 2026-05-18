using HealthCare.Common.Constants;
using HealthCare.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context     = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        // Seed identity roles
        string[] roles = [Roles.SuperAdmin, Roles.Admin, Roles.Doctor, Roles.Receptionist, Roles.Staff];
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // Idempotent guard: skip if already seeded
        if (await context.HealthcareCenters.AnyAsync()) return;

        // 1. Create Demo Hospital
        var center = new HealthcareCenter
        {
            Id           = Guid.NewGuid(),
            Name         = "Demo Hospital",
            Address      = "123 Health St, Medical City",
            ContactPhone = "+1-555-0100",
            ContactEmail = "info@demohospital.com",
            CreatedAt    = DateTime.UtcNow,
        };
        context.HealthcareCenters.Add(center);
        await context.SaveChangesAsync();

        // 2. SuperAdmin
        var superAdmin = new ApplicationUser
        {
            FirstName          = "Super",
            LastName           = "Admin",
            Email              = "superadmin@system.com",
            UserName           = "superadmin@system.com",
            Role               = Roles.SuperAdmin,
            HealthcareCenterId = null,
        };
        await userManager.CreateAsync(superAdmin, "SuperAdmin@1234!");
        await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin);

        // 3. Admin for Demo Hospital
        var admin = new ApplicationUser
        {
            FirstName          = "Hospital",
            LastName           = "Admin",
            Email              = "admin@demohospital.com",
            UserName           = "admin@demohospital.com",
            Role               = Roles.Admin,
            HealthcareCenterId = center.Id,
        };
        await userManager.CreateAsync(admin, "Admin@1234!");
        await userManager.AddToRoleAsync(admin, Roles.Admin);

        // 4. Doctor
        var doctorUser = new ApplicationUser
        {
            FirstName          = "Sarah",
            LastName           = "Johnson",
            Email              = "doctor@demohospital.com",
            UserName           = "doctor@demohospital.com",
            Role               = Roles.Doctor,
            HealthcareCenterId = center.Id,
        };
        await userManager.CreateAsync(doctorUser, "Doctor@1234!");
        await userManager.AddToRoleAsync(doctorUser, Roles.Doctor);

        var doctor = new Doctor
        {
            Id                 = Guid.NewGuid(),
            UserId             = doctorUser.Id,
            Specialization     = "General Practice",
            HealthcareCenterId = center.Id,
        };
        context.Doctors.Add(doctor);

        // Doctor availability: Mon-Fri 09:00–17:00
        var workdays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        foreach (var day in workdays)
        {
            context.DoctorAvailabilities.Add(new DoctorAvailability
            {
                Id        = Guid.NewGuid(),
                DoctorId  = doctor.Id,
                DayOfWeek = day,
                StartTime = new TimeOnly(9, 0),
                EndTime   = new TimeOnly(17, 0),
            });
        }

        // 5. Receptionist
        var receptionistUser = new ApplicationUser
        {
            FirstName          = "Front",
            LastName           = "Desk",
            Email              = "reception@demohospital.com",
            UserName           = "reception@demohospital.com",
            Role               = Roles.Receptionist,
            HealthcareCenterId = center.Id,
        };
        await userManager.CreateAsync(receptionistUser, "Recept@1234!");
        await userManager.AddToRoleAsync(receptionistUser, Roles.Receptionist);

        context.Receptionists.Add(new Receptionist
        {
            Id                 = Guid.NewGuid(),
            UserId             = receptionistUser.Id,
            HealthcareCenterId = center.Id,
        });

        // 6. Sample patients
        var patient1 = new Patient
        {
            Id                 = Guid.NewGuid(),
            FullName           = "John Doe",
            DateOfBirth        = new DateOnly(1985, 3, 15),
            Gender             = "Male",
            ContactPhone       = "+1-555-0201",
            ContactEmail       = "john.doe@email.com",
            Address            = "456 Patient Ave, Medical City",
            HealthcareCenterId = center.Id,
        };

        var patient2 = new Patient
        {
            Id                 = Guid.NewGuid(),
            FullName           = "Jane Smith",
            DateOfBirth        = new DateOnly(1990, 7, 22),
            Gender             = "Female",
            ContactPhone       = "+1-555-0202",
            ContactEmail       = "jane.smith@email.com",
            Address            = "789 Health Blvd, Medical City",
            HealthcareCenterId = center.Id,
        };

        context.Patients.AddRange(patient1, patient2);
        await context.SaveChangesAsync();

        // 7. Sample appointment (next Monday at 10:00)
        var nextMonday = GetNextWeekday(DateTime.UtcNow, DayOfWeek.Monday);
        var appointment = new Appointment
        {
            Id                 = Guid.NewGuid(),
            PatientId          = patient1.Id,
            DoctorId           = doctor.Id,
            ScheduledStart     = nextMonday.AddHours(10),
            ScheduledEnd       = nextMonday.AddHours(10).AddMinutes(30),
            Status             = AppointmentStatus.Scheduled,
            Notes              = "Routine checkup",
            HealthcareCenterId = center.Id,
            CreatedAt          = DateTime.UtcNow,
            UpdatedAt          = DateTime.UtcNow,
        };
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();
    }

    private static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
    {
        var daysUntil = ((int)day - (int)start.DayOfWeek + 7) % 7;
        if (daysUntil == 0) daysUntil = 7;
        return start.Date.AddDays(daysUntil);
    }
}
