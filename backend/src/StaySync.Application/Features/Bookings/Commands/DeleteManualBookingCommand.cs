using MediatR;

namespace StaySync.Application.Features.Bookings.Commands;

public record DeleteManualBookingCommand(Guid Id) : IRequest;
