using MediatR;

namespace StaySync.Application.Features.ExternalCalendars.Commands;

public record SyncCalendarCommand(Guid ExternalCalendarId) : IRequest<SyncCalendarResult>;

public record SyncCalendarResult(int Inserted, int Updated);
