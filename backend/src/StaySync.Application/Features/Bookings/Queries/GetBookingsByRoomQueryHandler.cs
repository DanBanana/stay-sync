using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Bookings.DTOs;

namespace StaySync.Application.Features.Bookings.Queries;

public class GetBookingsByRoomQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<GetBookingsByRoomQuery, List<BookingDto>>
{
    public async Task<List<BookingDto>> Handle(GetBookingsByRoomQuery request, CancellationToken cancellationToken)
    {
        var room = await context.Rooms
            .Include(r => r.Property)
            .FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken)
            ?? throw new NotFoundException("Room", request.RoomId);

        if (currentUser.Role != "SuperAdmin" && room.Property.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        return await context.Bookings
            .Where(b => b.RoomId == request.RoomId)
            .OrderBy(b => b.CheckIn)
            .Select(b => new BookingDto(b.Id, b.RoomId, b.ExternalCalendarId, b.ExternalUid, b.GuestName, b.CheckIn, b.CheckOut, b.Status.ToString(), b.RawSummary))
            .ToListAsync(cancellationToken);
    }
}
