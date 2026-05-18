using HealthCare.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Data.Configurations;

public class ReceptionistConfiguration : IEntityTypeConfiguration<Receptionist>
{
    public void Configure(EntityTypeBuilder<Receptionist> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.User)
            .WithOne()
            .HasForeignKey<Receptionist>(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.HealthcareCenter)
            .WithMany(h => h.Receptionists)
            .HasForeignKey(r => r.HealthcareCenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
