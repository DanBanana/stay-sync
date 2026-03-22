using MediatR;
using StaySync.Application.Features.Rooms.DTOs;

namespace StaySync.Application.Features.Rooms.Commands;

public record CreateRoomCommand(Guid PropertyId, string Name) : IRequest<RoomDto>;
