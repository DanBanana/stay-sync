using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaySync.Domain.Entities;

namespace StaySync.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.ExternalUid).IsRequired().HasMaxLength(500);
        builder.Property(b => b.Status).IsRequired().HasConversion<string>().HasMaxLength(50);

        builder.HasIndex(b => new { b.ExternalCalendarId, b.ExternalUid }).IsUnique();
        builder.HasIndex(b => new { b.RoomId, b.CheckIn, b.CheckOut });
        builder.HasIndex(b => b.PropertyManagerId);

        builder.HasOne(b => b.ExternalCalendar)
            .WithMany(ec => ec.Bookings)
            .HasForeignKey(b => b.ExternalCalendarId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
