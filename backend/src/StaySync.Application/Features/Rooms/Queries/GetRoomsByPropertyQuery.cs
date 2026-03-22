using MediatR;
using StaySync.Application.Features.Rooms.DTOs;

namespace StaySync.Application.Features.Rooms.Queries;

public record GetRoomsByPropertyQuery(Guid PropertyId) : IRequest<List<RoomDto>>;
