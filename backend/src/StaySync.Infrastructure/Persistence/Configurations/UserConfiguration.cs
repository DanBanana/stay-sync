using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaySync.Domain.Entities;

namespace StaySync.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Role).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasOne(u => u.PropertyManager)
            .WithOne(pm => pm.User)
            .HasForeignKey<PropertyManager>(pm => pm.UserId);
    }
}
