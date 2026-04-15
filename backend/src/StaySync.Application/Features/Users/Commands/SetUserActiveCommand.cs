using MediatR;

namespace StaySync.Application.Features.Users.Commands;

public record SetUserActiveCommand(Guid UserId, bool IsActive) : IRequest;
