using MediatR;

namespace StaySync.Application.Features.Users.Commands;

public record DeleteUserCommand(Guid UserId) : IRequest;
