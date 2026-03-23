using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Rooms.DTOs;

namespace StaySync.Application.Features.Rooms.Commands;

public class UpdateRoomCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<UpdateRoomCommand, RoomDto>
{
    public async Task<RoomDto> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await context.Rooms
            .Include(r => r.Property)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Room", request.Id);

        if (currentUser.Role != "SuperAdmin" && room.Property.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        room.Name = request.Name;
        await context.SaveChangesAsync(cancellationToken);

        return new RoomDto(room.Id, room.Name, room.PropertyId, room.CreatedAt);
    }
}
