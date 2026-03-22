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
    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.PropertyManager)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken)
            ?? throw new NotFoundException("User", request.Email);

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new NotFoundException("User", request.Email);

        var token = tokenService.GenerateToken(user, user.PropertyManager);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);

        return new AuthResultDto(token, expiresAt, user.Role.ToString());
    }
}
