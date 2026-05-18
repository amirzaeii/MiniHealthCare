using HealthCare.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.FullName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Gender).HasMaxLength(10);
        builder.Property(p => p.ContactPhone).HasMaxLength(20);
        builder.Property(p => p.ContactEmail).HasMaxLength(150);
        builder.Property(p => p.Address).HasMaxLength(500);

        builder.HasOne(p => p.HealthcareCenter)
            .WithMany(h => h.Patients)
            .HasForeignKey(p => p.HealthcareCenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
