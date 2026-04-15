using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Domain.Enums;

namespace StaySync.Application.Features.Users.Commands;

public class SetUserActiveCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<SetUserActiveCommand>
{
    public async Task Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.Role != nameof(UserRole.SuperAdmin))
            throw new ForbiddenException();

        if (request.UserId == currentUser.UserId)
            throw new BadRequestException("You cannot change the active status of your own account.");

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        user.IsActive = request.IsActive;
        await context.SaveChangesAsync(cancellationToken);
    }
}
