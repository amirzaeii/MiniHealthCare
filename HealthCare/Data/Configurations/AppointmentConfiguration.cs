using HealthCare.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Status).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Notes).HasMaxLength(1000);

        // Indexes to speed up conflict detection queries
        builder.HasIndex(a => new { a.DoctorId, a.ScheduledStart, a.ScheduledEnd });
        builder.HasIndex(a => new { a.PatientId, a.ScheduledStart, a.ScheduledEnd });

        builder.HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.HealthcareCenter)
            .WithMany(h => h.Appointments)
            .HasForeignKey(a => a.HealthcareCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.MedicalRecord)
            .WithOne(m => m.Appointment)
            .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Prescription)
            .WithOne(p => p.Appointment)
            .HasForeignKey<Prescription>(p => p.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
