using MediatR;
using StaySync.Application.Features.ExternalCalendars.DTOs;

namespace StaySync.Application.Features.ExternalCalendars.Queries;

public record GetExternalCalendarsByRoomQuery(Guid RoomId) : IRequest<List<ExternalCalendarDto>>;
