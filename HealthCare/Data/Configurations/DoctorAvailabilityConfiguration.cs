using HealthCare.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Data.Configurations;

public class DoctorAvailabilityConfiguration : IEntityTypeConfiguration<DoctorAvailability>
{
    public void Configure(EntityTypeBuilder<DoctorAvailability> builder)
    {
        builder.HasKey(a => a.Id);

        // One availability block per day per doctor
        builder.HasIndex(a => new { a.DoctorId, a.DayOfWeek }).IsUnique();

        builder.Property(a => a.DayOfWeek)
            .HasConversion<int>();
    }
}
