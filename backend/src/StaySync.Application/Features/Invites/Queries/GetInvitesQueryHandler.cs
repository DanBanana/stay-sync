using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Invites.DTOs;
using StaySync.Domain.Enums;

namespace StaySync.Application.Features.Invites.Queries;

public class GetInvitesQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<GetInvitesQuery, List<InviteDto>>
{
    public async Task<List<InviteDto>> Handle(GetInvitesQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.Role != nameof(UserRole.SuperAdmin))
            throw new ForbiddenException();

        var now = DateTime.UtcNow;

        return await context.InviteTokens
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new InviteDto(
                t.Id,
                t.Email,
                t.Role.ToString(),
                t.ExpiresAt,
                t.UsedAt,
                t.UsedAt == null && t.ExpiresAt > now))
            .ToListAsync(cancellationToken);
    }
}
