using MediatR;
using StaySync.Application.Features.Bookings.DTOs;

namespace StaySync.Application.Features.Bookings.Queries;

public record GetBookingsByRoomQuery(Guid RoomId) : IRequest<List<BookingDto>>;
