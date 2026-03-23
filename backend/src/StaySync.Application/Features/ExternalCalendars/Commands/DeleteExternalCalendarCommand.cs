using MediatR;

namespace StaySync.Application.Features.ExternalCalendars.Commands;

public record DeleteExternalCalendarCommand(Guid Id) : IRequest;
