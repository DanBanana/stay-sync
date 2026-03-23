using MediatR;
using StaySync.Application.Features.Rooms.DTOs;

namespace StaySync.Application.Features.Rooms.Commands;

public record UpdateRoomCommand(Guid Id, string Name) : IRequest<RoomDto>;
