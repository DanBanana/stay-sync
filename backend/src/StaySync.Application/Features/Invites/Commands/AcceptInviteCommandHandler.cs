using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Domain.Entities;
using StaySync.Domain.Enums;

namespace StaySync.Application.Features.Invites.Commands;

public class AcceptInviteCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher) : IRequestHandler<AcceptInviteCommand>
{
    public async Task Handle(AcceptInviteCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = InviteTokenHelper.Hash(request.Token);

        var invite = await context.InviteTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken)
            ?? throw new NotFoundException("InviteToken", request.Token[..6] + "…");

        if (invite.UsedAt is not null)
            throw new BadRequestException("This invite link has already been used.");

        if (invite.ExpiresAt <= DateTime.UtcNow)
            throw new BadRequestException("This invite link has expired.");

        ValidatePassword(request.Password);

        var user = new User
        {
            Email = invite.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = invite.Role,
            IsActive = true
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        if (invite.Role == UserRole.PropertyManager)
        {
            context.PropertyManagers.Add(new PropertyManager
            {
                UserId = user.Id,
                DisplayName = request.Name
            });
        }

        invite.UsedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }

    private static void ValidatePassword(string password)
    {
        var failures = new List<ValidationFailure>();

        if (password.Length < 8)
            failures.Add(new ValidationFailure("Password", "Password must be at least 8 characters."));
        if (!password.Any(char.IsUpper))
            failures.Add(new ValidationFailure("Password", "Password must contain at least one uppercase letter."));
        if (!password.Any(char.IsDigit))
            failures.Add(new ValidationFailure("Password", "Password must contain at least one digit."));
        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            failures.Add(new ValidationFailure("Password", "Password must contain at least one special character."));

        if (failures.Count > 0)
            throw new ValidationException(failures);
    }
}
