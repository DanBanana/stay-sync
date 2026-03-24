using MediatR;
using StaySync.Application.Features.Bookings.DTOs;

namespace StaySync.Application.Features.Bookings.Queries;

public record GetBookingsForCalendarQuery(Guid PropertyId, DateOnly From, DateOnly To)
    : IRequest<List<CalendarBookingDto>>;
