using MediatR;
using StaySync.Application.Features.Bookings.DTOs;

namespace StaySync.Application.Features.Bookings.Queries;

public record GetBookingsByPropertyQuery(Guid PropertyId) : IRequest<List<BookingDto>>;
