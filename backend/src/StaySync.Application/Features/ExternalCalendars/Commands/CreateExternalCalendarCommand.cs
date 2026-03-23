using MediatR;
using StaySync.Application.Features.ExternalCalendars.DTOs;

namespace StaySync.Application.Features.ExternalCalendars.Commands;

public record CreateExternalCalendarCommand(Guid RoomId, string Platform, string IcsUrl) : IRequest<ExternalCalendarDto>;
