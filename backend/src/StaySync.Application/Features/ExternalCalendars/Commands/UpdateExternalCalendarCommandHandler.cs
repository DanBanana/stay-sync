using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.ExternalCalendars.DTOs;

namespace StaySync.Application.Features.ExternalCalendars.Commands;

public class UpdateExternalCalendarCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<UpdateExternalCalendarCommand, ExternalCalendarDto>
{
    public async Task<ExternalCalendarDto> Handle(UpdateExternalCalendarCommand request, CancellationToken cancellationToken)
    {
        var calendar = await context.ExternalCalendars
            .Include(c => c.Room)
            .ThenInclude(r => r.Property)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("ExternalCalendar", request.Id);

        if (currentUser.Role != "SuperAdmin" && calendar.Room.Property.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        calendar.Platform = request.Platform;
        calendar.IcsUrl = request.IcsUrl;
        await context.SaveChangesAsync(cancellationToken);

        return new ExternalCalendarDto(calendar.Id, calendar.RoomId, calendar.Platform, calendar.IcsUrl, calendar.LastSyncedAt, calendar.LastSyncStatus, calendar.LastSyncErrorMessage, calendar.CreatedAt);
    }
}
