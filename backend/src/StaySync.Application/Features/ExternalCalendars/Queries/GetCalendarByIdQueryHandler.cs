using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.ExternalCalendars.DTOs;

namespace StaySync.Application.Features.ExternalCalendars.Queries;

public class GetCalendarByIdQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<GetCalendarByIdQuery, ExternalCalendarDto>
{
    public async Task<ExternalCalendarDto> Handle(GetCalendarByIdQuery request, CancellationToken cancellationToken)
    {
        var calendar = await context.ExternalCalendars
            .Include(c => c.Room)
            .ThenInclude(r => r.Property)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("ExternalCalendar", request.Id);

        if (currentUser.Role != "SuperAdmin" && calendar.Room.Property.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        return new ExternalCalendarDto(calendar.Id, calendar.RoomId, calendar.Platform, calendar.IcsUrl, calendar.LastSyncedAt, calendar.LastSyncStatus, calendar.LastSyncErrorMessage, calendar.CreatedAt);
    }
}
