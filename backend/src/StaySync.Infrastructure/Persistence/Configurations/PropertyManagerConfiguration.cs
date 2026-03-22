using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaySync.Domain.Entities;

namespace StaySync.Infrastructure.Persistence.Configurations;

public class PropertyManagerConfiguration : IEntityTypeConfiguration<PropertyManager>
{
    public void Configure(EntityTypeBuilder<PropertyManager> builder)
    {
        builder.HasKey(pm => pm.Id);
        builder.Property(pm => pm.DisplayName).IsRequired().HasMaxLength(255);
        builder.HasIndex(pm => pm.UserId).IsUnique();
    }
}
