using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;

namespace StaySync.Application.Features.Bookings.Commands;

public class DeleteManualBookingCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<DeleteManualBookingCommand>
{
    public async Task Handle(DeleteManualBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await context.Bookings
            .Include(b => b.ExternalCalendar)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Booking", request.Id);

        if (currentUser.Role != "SuperAdmin" && booking.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        if (booking.ExternalCalendar.Platform != "Manual")
            throw new BadRequestException("Only manual bookings can be deleted.");

        context.Bookings.Remove(booking);
        await context.SaveChangesAsync(cancellationToken);
    }
}
