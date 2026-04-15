using MediatR;

namespace StaySync.Application.Features.Invites.Commands;

public record RevokeInviteCommand(Guid Id) : IRequest;
