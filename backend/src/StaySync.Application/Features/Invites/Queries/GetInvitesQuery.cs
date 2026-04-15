using MediatR;
using StaySync.Application.Features.Invites.DTOs;

namespace StaySync.Application.Features.Invites.Queries;

public record GetInvitesQuery : IRequest<List<InviteDto>>;
