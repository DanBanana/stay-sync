using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaySync.Domain.Entities;

namespace StaySync.Infrastructure.Persistence.Configurations;

public class InviteTokenConfiguration : IEntityTypeConfiguration<InviteToken>
{
    public void Configure(EntityTypeBuilder<InviteToken> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Email).IsRequired().HasMaxLength(255);
        builder.Property(t => t.Role).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(64);
        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Ignore(t => t.IsPending);

        builder.HasIndex(t => t.TokenHash).IsUnique();
    }
}
