using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaySync.Domain.Entities;

namespace StaySync.Infrastructure.Persistence.Configurations;

public class ExternalCalendarConfiguration : IEntityTypeConfiguration<ExternalCalendar>
{
    public void Configure(EntityTypeBuilder<ExternalCalendar> builder)
    {
        builder.HasKey(ec => ec.Id);
        builder.Property(ec => ec.Platform).IsRequired().HasMaxLength(100);
        builder.Property(ec => ec.IcsUrl).IsRequired();
        builder.HasIndex(ec => ec.RoomId);

        builder.HasOne(ec => ec.Room)
            .WithMany(r => r.ExternalCalendars)
            .HasForeignKey(ec => ec.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
