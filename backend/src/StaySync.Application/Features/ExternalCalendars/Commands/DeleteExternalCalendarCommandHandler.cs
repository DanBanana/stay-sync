using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;

namespace StaySync.Application.Features.ExternalCalendars.Commands;

public class DeleteExternalCalendarCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<DeleteExternalCalendarCommand>
{
    public async Task Handle(DeleteExternalCalendarCommand request, CancellationToken cancellationToken)
    {
        var calendar = await context.ExternalCalendars
            .Include(c => c.Room)
            .ThenInclude(r => r.Property)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("ExternalCalendar", request.Id);

        if (currentUser.Role != "SuperAdmin" && calendar.Room.Property.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        context.ExternalCalendars.Remove(calendar);
        await context.SaveChangesAsync(cancellationToken);
    }
}
