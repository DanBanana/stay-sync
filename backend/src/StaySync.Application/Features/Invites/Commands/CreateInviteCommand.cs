using MediatR;
using StaySync.Application.Features.Invites.DTOs;
using StaySync.Domain.Enums;

namespace StaySync.Application.Features.Invites.Commands;

public record CreateInviteCommand(string Email, UserRole Role) : IRequest<CreateInviteResult>;
