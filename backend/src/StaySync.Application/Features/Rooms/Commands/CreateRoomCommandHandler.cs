using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Rooms.DTOs;
using StaySync.Domain.Entities;

namespace StaySync.Application.Features.Rooms.Commands;

public class CreateRoomCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<CreateRoomCommand, RoomDto>
{
    public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var property = await context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken)
            ?? throw new NotFoundException("Property", request.PropertyId);

        if (currentUser.Role != "SuperAdmin" && property.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        var room = new Room
        {
            PropertyId = request.PropertyId,
            Name = request.Name
        };

        context.Rooms.Add(room);
        await context.SaveChangesAsync(cancellationToken);

        return new RoomDto(room.Id, room.Name, room.PropertyId, room.CreatedAt);
    }
}
