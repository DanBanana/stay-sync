using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;

namespace StaySync.Application.Features.Bookings.Commands;

public class UpdateManualBookingCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<UpdateManualBookingCommand>
{
    public async Task Handle(UpdateManualBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await context.Bookings
            .Include(b => b.ExternalCalendar)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Booking", request.Id);

        if (currentUser.Role != "SuperAdmin" && booking.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        if (booking.ExternalCalendar.Platform != "Manual")
            throw new BadRequestException("Only manual bookings can be edited.");

        booking.CheckIn = request.CheckIn;
        booking.CheckOut = request.CheckOut;
        booking.GuestName = request.GuestName;
        booking.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}
