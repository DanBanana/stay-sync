using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.ExternalCalendars.DTOs;
using StaySync.Domain.Entities;

namespace StaySync.Application.Features.ExternalCalendars.Commands;

public class CreateExternalCalendarCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<CreateExternalCalendarCommand, ExternalCalendarDto>
{
    public async Task<ExternalCalendarDto> Handle(CreateExternalCalendarCommand request, CancellationToken cancellationToken)
    {
        var room = await context.Rooms
            .Include(r => r.Property)
            .FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken)
            ?? throw new NotFoundException("Room", request.RoomId);

        if (currentUser.Role != "SuperAdmin" && room.Property.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        var calendar = new ExternalCalendar
        {
            RoomId = request.RoomId,
            Platform = request.Platform,
            IcsUrl = request.IcsUrl
        };

        context.ExternalCalendars.Add(calendar);
        await context.SaveChangesAsync(cancellationToken);

        return new ExternalCalendarDto(calendar.Id, calendar.RoomId, calendar.Platform, calendar.IcsUrl, calendar.LastSyncedAt, calendar.LastSyncStatus, calendar.LastSyncErrorMessage, calendar.CreatedAt);
    }
}
