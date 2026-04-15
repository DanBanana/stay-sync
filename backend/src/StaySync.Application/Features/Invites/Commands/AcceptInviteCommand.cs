using MediatR;

namespace StaySync.Application.Features.Invites.Commands;

public record AcceptInviteCommand(string Token, string Name, string Password) : IRequest;
