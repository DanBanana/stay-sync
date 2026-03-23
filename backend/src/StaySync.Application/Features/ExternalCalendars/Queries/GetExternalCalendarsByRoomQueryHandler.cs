using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.ExternalCalendars.DTOs;

namespace StaySync.Application.Features.ExternalCalendars.Queries;

public class GetExternalCalendarsByRoomQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<GetExternalCalendarsByRoomQuery, List<ExternalCalendarDto>>
{
    public async Task<List<ExternalCalendarDto>> Handle(GetExternalCalendarsByRoomQuery request, CancellationToken cancellationToken)
    {
        var room = await context.Rooms
            .Include(r => r.Property)
            .FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken)
            ?? throw new NotFoundException("Room", request.RoomId);

        if (currentUser.Role != "SuperAdmin" && room.Property.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        return await context.ExternalCalendars
            .Where(c => c.RoomId == request.RoomId)
            .OrderBy(c => c.Platform)
            .Select(c => new ExternalCalendarDto(c.Id, c.RoomId, c.Platform, c.IcsUrl, c.LastSyncedAt, c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
