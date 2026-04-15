using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Domain.Enums;

namespace StaySync.Application.Features.Users.Commands;

public class DeleteUserCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.Role != nameof(UserRole.SuperAdmin))
            throw new ForbiddenException();

        if (request.UserId == currentUser.UserId)
            throw new BadRequestException("You cannot delete your own account.");

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);
    }
}
