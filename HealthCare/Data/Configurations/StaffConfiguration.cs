using HealthCare.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Data.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Department).HasMaxLength(100);

        builder.HasOne(s => s.User)
            .WithOne()
            .HasForeignKey<Staff>(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.HealthcareCenter)
            .WithMany(h => h.Staff)
            .HasForeignKey(s => s.HealthcareCenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
