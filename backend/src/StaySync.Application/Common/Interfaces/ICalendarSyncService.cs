using StaySync.Application.Features.ExternalCalendars.Commands;
using StaySync.Domain.Entities;

namespace StaySync.Application.Common.Interfaces;

public interface ICalendarSyncService
{
    Task<SyncCalendarResult> SyncAsync(ExternalCalendar calendar, CancellationToken cancellationToken = default);
}
