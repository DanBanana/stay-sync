using MediatR;

namespace StaySync.Application.Features.Rooms.Commands;

public record DeleteRoomCommand(Guid Id) : IRequest;
