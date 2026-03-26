using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Domain.Enums;
using StaySync.Domain.Exceptions;
using StaySync.Domain.ValueObjects;

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

        var updatedRange = new DateRange(request.CheckIn, request.CheckOut);
        var existing = await context.Bookings
            .Where(b => b.RoomId == booking.RoomId && b.Id != booking.Id && b.Status != BookingStatus.Cancelled)
            .ToListAsync(cancellationToken);

        if (existing.Any(b => updatedRange.Overlaps(new DateRange(b.CheckIn, b.CheckOut))))
            throw new ConflictDetectedException(booking.RoomId, request.CheckIn, request.CheckOut);

        booking.CheckIn = request.CheckIn;
        booking.CheckOut = request.CheckOut;
        booking.GuestName = request.GuestName;
        booking.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}
