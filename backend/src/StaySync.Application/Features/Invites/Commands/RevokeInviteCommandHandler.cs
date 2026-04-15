using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Domain.Enums;

namespace StaySync.Application.Features.Invites.Commands;

public class RevokeInviteCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<RevokeInviteCommand>
{
    public async Task Handle(RevokeInviteCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.Role != nameof(UserRole.SuperAdmin))
            throw new ForbiddenException();

        var invite = await context.InviteTokens
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("InviteToken", request.Id);

        if (invite.UsedAt is not null)
            throw new BadRequestException("This invite has already been used or revoked.");

        invite.UsedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }
}
