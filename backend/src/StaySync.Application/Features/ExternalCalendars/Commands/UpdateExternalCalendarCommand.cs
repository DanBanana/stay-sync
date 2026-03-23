using MediatR;
using StaySync.Application.Features.ExternalCalendars.DTOs;

namespace StaySync.Application.Features.ExternalCalendars.Commands;

public record UpdateExternalCalendarCommand(Guid Id, string Platform, string IcsUrl) : IRequest<ExternalCalendarDto>;
