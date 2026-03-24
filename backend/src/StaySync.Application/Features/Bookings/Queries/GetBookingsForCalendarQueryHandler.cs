using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Bookings.DTOs;

namespace StaySync.Application.Features.Bookings.Queries;

public class GetBookingsForCalendarQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<GetBookingsForCalendarQuery, List<CalendarBookingDto>>
{
    public async Task<List<CalendarBookingDto>> Handle(GetBookingsForCalendarQuery request, CancellationToken cancellationToken)
    {
        var property = await context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken)
            ?? throw new NotFoundException("Property", request.PropertyId);

        if (currentUser.Role != "SuperAdmin" && property.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        return await context.Bookings
            .Where(b =>
                context.Rooms.Any(r => r.PropertyId == request.PropertyId && r.Id == b.RoomId) &&
                b.CheckIn < request.To && b.CheckOut > request.From)
            .Join(context.Rooms,
                b => b.RoomId,
                r => r.Id,
                (b, r) => new { Booking = b, Room = r })
            .Join(context.ExternalCalendars,
                x => x.Booking.ExternalCalendarId,
                ec => ec.Id,
                (x, ec) => new CalendarBookingDto(
                    x.Booking.Id,
                    x.Booking.RoomId,
                    x.Room.Name,
                    ec.Platform,
                    x.Booking.CheckIn,
                    x.Booking.CheckOut,
                    x.Booking.Status.ToString()))
            .OrderBy(dto => dto.RoomName)
            .ThenBy(dto => dto.CheckIn)
            .ToListAsync(cancellationToken);
    }
}
