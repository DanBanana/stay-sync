using MediatR;

namespace StaySync.Application.Features.Bookings.Commands;

public record UpdateManualBookingCommand(
    Guid Id,
    DateOnly CheckIn,
    DateOnly CheckOut,
    string? GuestName) : IRequest;
