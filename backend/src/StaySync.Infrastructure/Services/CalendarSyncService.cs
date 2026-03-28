using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.ExternalCalendars.Commands;
using StaySync.Domain.Entities;
using StaySync.Domain.Enums;
using StaySync.Domain.Interfaces;

namespace StaySync.Infrastructure.Services;

public class CalendarSyncService(
    IApplicationDbContext context,
    IBookingProvider bookingProvider) : ICalendarSyncService
{
    public async Task<SyncCalendarResult> SyncAsync(ExternalCalendar calendar, CancellationToken cancellationToken = default)
    {
        var fetched = await bookingProvider.FetchBookingsAsync(calendar, cancellationToken);

        var existing = await context.Bookings
            .Where(b => b.ExternalCalendarId == calendar.Id)
            .ToListAsync(cancellationToken);

        var existingByUid = existing.ToDictionary(b => b.ExternalUid);

        int inserted = 0;
        int updated = 0;

        foreach (var booking in fetched)
        {
            if (existingByUid.TryGetValue(booking.ExternalUid, out var existingBooking))
            {
                existingBooking.CheckIn = booking.CheckIn;
                existingBooking.CheckOut = booking.CheckOut;
                existingBooking.Status = booking.Status;
                existingBooking.RawSummary = booking.RawSummary;
                existingBooking.UpdatedAt = DateTimeOffset.UtcNow;
                updated++;
            }
            else
            {
                context.Bookings.Add(booking);
                inserted++;
            }
        }

        calendar.LastSyncedAt = DateTimeOffset.UtcNow;
        calendar.LastSyncStatus = SyncStatus.Success;
        calendar.LastSyncErrorMessage = null;
        await context.SaveChangesAsync(cancellationToken);

        return new SyncCalendarResult(inserted, updated);
    }
}
