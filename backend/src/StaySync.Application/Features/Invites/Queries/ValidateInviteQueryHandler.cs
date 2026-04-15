using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Invites.DTOs;

namespace StaySync.Application.Features.Invites.Queries;

public class ValidateInviteQueryHandler(
    IApplicationDbContext context) : IRequestHandler<ValidateInviteQuery, InviteInfoDto>
{
    public async Task<InviteInfoDto> Handle(ValidateInviteQuery request, CancellationToken cancellationToken)
    {
        var tokenHash = InviteTokenHelper.Hash(request.Token);

        var invite = await context.InviteTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken)
            ?? throw new NotFoundException("InviteToken", "provided token");

        if (invite.UsedAt is not null)
            throw new BadRequestException("This invite link has already been used.");

        if (invite.ExpiresAt <= DateTime.UtcNow)
            throw new BadRequestException("This invite link has expired.");

        return new InviteInfoDto(invite.Email, invite.Role.ToString());
    }
}
