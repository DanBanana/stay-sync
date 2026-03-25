using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Domain.Entities;
using StaySync.Domain.Enums;

namespace StaySync.Application.Features.Bookings.Commands;

public class CreateManualBookingCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<CreateManualBookingCommand, Guid>
{
    public async Task<Guid> Handle(CreateManualBookingCommand request, CancellationToken cancellationToken)
    {
        var room = await context.Rooms
            .Include(r => r.Property)
            .FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken)
            ?? throw new NotFoundException("Room", request.RoomId);

        if (currentUser.Role != "SuperAdmin" && room.Property.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        var manualCalendar = await context.ExternalCalendars
            .FirstOrDefaultAsync(ec => ec.RoomId == request.RoomId && ec.Platform == "Manual", cancellationToken);

        if (manualCalendar is null)
        {
            manualCalendar = new ExternalCalendar
            {
                RoomId = request.RoomId,
                Platform = "Manual",
                IcsUrl = string.Empty,
            };
            context.ExternalCalendars.Add(manualCalendar);
            await context.SaveChangesAsync(cancellationToken);
        }

        var booking = new Booking
        {
            ExternalCalendarId = manualCalendar.Id,
            PropertyManagerId = room.Property.PropertyManagerId,
            RoomId = request.RoomId,
            ExternalUid = Guid.NewGuid().ToString(),
            GuestName = request.GuestName,
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            Status = BookingStatus.Confirmed,
        };

        context.Bookings.Add(booking);
        await context.SaveChangesAsync(cancellationToken);

        return booking.Id;
    }
}
