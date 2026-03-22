using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaySync.Domain.Entities;

namespace StaySync.Infrastructure.Persistence.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(255);
        builder.HasIndex(p => p.PropertyManagerId);

        builder.HasOne(p => p.PropertyManager)
            .WithMany(pm => pm.Properties)
            .HasForeignKey(p => p.PropertyManagerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
