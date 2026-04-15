using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Users.DTOs;
using StaySync.Domain.Enums;

namespace StaySync.Application.Features.Users.Queries;

public class GetUsersQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.Role != nameof(UserRole.SuperAdmin))
            throw new ForbiddenException();

        return await context.Users
            .OrderBy(u => u.Email)
            .Select(u => new UserDto(u.Id, u.Email, u.Role.ToString(), u.IsActive, u.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
