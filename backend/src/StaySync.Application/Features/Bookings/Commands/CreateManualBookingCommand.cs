using MediatR;

namespace StaySync.Application.Features.Bookings.Commands;

public record CreateManualBookingCommand(
    Guid RoomId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    string? GuestName) : IRequest<Guid>;
