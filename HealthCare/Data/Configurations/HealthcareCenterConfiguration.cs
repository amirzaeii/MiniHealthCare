using HealthCare.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Data.Configurations;

public class HealthcareCenterConfiguration : IEntityTypeConfiguration<HealthcareCenter>
{
    public void Configure(EntityTypeBuilder<HealthcareCenter> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).IsRequired().HasMaxLength(200);
        builder.Property(h => h.Address).IsRequired().HasMaxLength(500);
        builder.Property(h => h.ContactPhone).HasMaxLength(20);
        builder.Property(h => h.ContactEmail).HasMaxLength(150);
        builder.Property(h => h.CreatedAt).IsRequired();
    }
}
