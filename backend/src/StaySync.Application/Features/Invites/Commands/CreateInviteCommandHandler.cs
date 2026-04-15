using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Invites.DTOs;
using StaySync.Domain.Entities;
using StaySync.Domain.Enums;

namespace StaySync.Application.Features.Invites.Commands;

public class CreateInviteCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<CreateInviteCommand, CreateInviteResult>
{
    private const int ExpiryHours = 48;

    public async Task<CreateInviteResult> Handle(CreateInviteCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.Role != nameof(UserRole.SuperAdmin))
            throw new ForbiddenException();

        var email = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (emailTaken)
            throw new ValidationException([new ValidationFailure("Email", "An account with this email already exists.")]);

        var pendingExists = await context.InviteTokens
            .AnyAsync(t => t.Email == email && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow, cancellationToken);

        if (pendingExists)
            throw new ValidationException([new ValidationFailure("Email", "A pending invite for this email already exists.")]);

        var rawToken = InviteTokenHelper.GenerateRawToken();
        var expiresAt = DateTime.UtcNow.AddHours(ExpiryHours);

        context.InviteTokens.Add(new InviteToken
        {
            Email = email,
            Role = request.Role,
            TokenHash = InviteTokenHelper.Hash(rawToken),
            ExpiresAt = expiresAt,
            CreatedByUserId = currentUser.UserId
        });

        await context.SaveChangesAsync(cancellationToken);

        return new CreateInviteResult(rawToken, expiresAt);
    }
}
