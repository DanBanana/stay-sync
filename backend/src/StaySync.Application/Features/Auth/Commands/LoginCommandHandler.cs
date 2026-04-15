using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Auth.DTOs;

namespace StaySync.Application.Features.Auth.Commands;

public class LoginCommandHandler(
    IApplicationDbContext context,
    ITokenService tokenService,
    IPasswordHasher passwordHasher) : IRequestHandler<LoginCommand, AuthResultDto>
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.PropertyManager)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken)
            ?? throw new NotFoundException("User", request.Email);

        if (!user.IsActive)
            throw new ForbiddenException("Account is disabled. Please contact an administrator.");

        if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.UtcNow)
            throw new ForbiddenException("Account is temporarily locked. Please try again later.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= MaxFailedAttempts)
                user.LockoutUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);

            await context.SaveChangesAsync(cancellationToken);
            throw new NotFoundException("User", request.Email);
        }

        user.FailedLoginAttempts = 0;
        user.LockoutUntil = null;
        await context.SaveChangesAsync(cancellationToken);

        var token = tokenService.GenerateToken(user, user.PropertyManager);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);

        return new AuthResultDto(token, expiresAt, user.Role.ToString());
    }
}
