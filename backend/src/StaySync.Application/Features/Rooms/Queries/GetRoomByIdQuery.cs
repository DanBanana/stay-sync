using MediatR;
using StaySync.Application.Features.Rooms.DTOs;

namespace StaySync.Application.Features.Rooms.Queries;

public record GetRoomByIdQuery(Guid Id) : IRequest<RoomDto>;
