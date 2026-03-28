using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Domain.Interfaces;

namespace StaySync.Application.Features.ExternalCalendars.Commands;

public class SyncCalendarCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    IBookingProvider bookingProvider) : IRequestHandler<SyncCalendarCommand, SyncCalendarResult>
{
    public async Task<SyncCalendarResult> Handle(SyncCalendarCommand request, CancellationToken cancellationToken)
    {
        var calendar = await context.ExternalCalendars
            .Include(ec => ec.Room)
            .ThenInclude(r => r.Property)
            .FirstOrDefaultAsync(ec => ec.Id == request.ExternalCalendarId, cancellationToken)
            ?? throw new NotFoundException("ExternalCalendar", request.ExternalCalendarId);

        if (currentUser.Role != "SuperAdmin" && calendar.Room.Property.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        if (calendar.Platform == "Manual")
            throw new BadRequestException("Manual calendars cannot be synced via ICS.");

        var fetched = await bookingProvider.FetchBookingsAsync(calendar, cancellationToken);

        var existing = await context.Bookings
            .Where(b => b.ExternalCalendarId == calendar.Id)
            .ToListAsync(cancellationToken);

        var existingByUid = existing.ToDictionary(b => b.ExternalUid);

        int inserted = 0;
        int updated = 0;

        foreach (var booking in fetched)
        {
            if (existingByUid.TryGetValue(booking.ExternalUid, out var existing_))
            {
                existing_.CheckIn = booking.CheckIn;
                existing_.CheckOut = booking.CheckOut;
                existing_.Status = booking.Status;
                existing_.RawSummary = booking.RawSummary;
                existing_.UpdatedAt = DateTimeOffset.UtcNow;
                updated++;
            }
            else
            {
                context.Bookings.Add(booking);
                inserted++;
            }
        }

        calendar.LastSyncedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new SyncCalendarResult(inserted, updated);
    }
}
