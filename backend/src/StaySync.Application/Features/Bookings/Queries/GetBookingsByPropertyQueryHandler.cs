using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Bookings.DTOs;

namespace StaySync.Application.Features.Bookings.Queries;

public class GetBookingsByPropertyQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<GetBookingsByPropertyQuery, List<BookingDto>>
{
    public async Task<List<BookingDto>> Handle(GetBookingsByPropertyQuery request, CancellationToken cancellationToken)
    {
        var property = await context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken)
            ?? throw new NotFoundException("Property", request.PropertyId);

        if (currentUser.Role != "SuperAdmin" && property.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        return await context.Bookings
            .Where(b => b.PropertyManagerId == property.PropertyManagerId &&
                        context.Rooms.Any(r => r.PropertyId == request.PropertyId && r.Id == b.RoomId))
            .OrderBy(b => b.CheckIn)
            .Select(b => new BookingDto(b.Id, b.RoomId, b.ExternalCalendarId, b.ExternalUid, b.GuestName, b.CheckIn, b.CheckOut, b.Status.ToString(), b.RawSummary))
            .ToListAsync(cancellationToken);
    }
}
