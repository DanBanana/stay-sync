using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;

namespace StaySync.Application.Features.ExternalCalendars.Commands;

public class SyncCalendarCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ICalendarSyncService syncService) : IRequestHandler<SyncCalendarCommand, SyncCalendarResult>
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

        return await syncService.SyncAsync(calendar, cancellationToken);
    }
}
