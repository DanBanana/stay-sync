using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Bookings.DTOs;

namespace StaySync.Application.Features.Bookings.Queries;

public class GetBookingByIdQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<GetBookingByIdQuery, BookingDto>
{
    public async Task<BookingDto> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await context.Bookings
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Booking", request.Id);

        if (currentUser.Role != "SuperAdmin" && booking.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        return new BookingDto(booking.Id, booking.RoomId, booking.ExternalCalendarId, booking.ExternalUid,
            booking.GuestName, booking.CheckIn, booking.CheckOut, booking.Status.ToString(), booking.RawSummary);
    }
}
