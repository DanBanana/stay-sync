using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Rooms.DTOs;

namespace StaySync.Application.Features.Rooms.Queries;

public class GetRoomsByPropertyQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<GetRoomsByPropertyQuery, List<RoomDto>>
{
    public async Task<List<RoomDto>> Handle(GetRoomsByPropertyQuery request, CancellationToken cancellationToken)
    {
        var property = await context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken)
            ?? throw new NotFoundException("Property", request.PropertyId);

        if (currentUser.Role != "SuperAdmin" && property.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        return await context.Rooms
            .Where(r => r.PropertyId == request.PropertyId)
            .OrderBy(r => r.Name)
            .Select(r => new RoomDto(r.Id, r.Name, r.PropertyId, r.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
