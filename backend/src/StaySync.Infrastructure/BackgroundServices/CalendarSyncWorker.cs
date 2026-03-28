using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StaySync.Application.Common.Interfaces;
using StaySync.Domain.Enums;

namespace StaySync.Infrastructure.BackgroundServices;

public class CalendarSyncWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<BackgroundSyncOptions> options,
    ILogger<CalendarSyncWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CalendarSyncWorker started. Interval: {Minutes} minutes.", options.Value.IntervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(options.Value.IntervalMinutes), stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            logger.LogInformation("CalendarSyncWorker: starting sync run.");

            int succeeded = 0;
            int failed = 0;

            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var syncService = scope.ServiceProvider.GetRequiredService<ICalendarSyncService>();

            var calendars = await context.ExternalCalendars
                .Where(c => c.Platform != "Manual")
                .ToListAsync(CancellationToken.None);

            foreach (var calendar in calendars)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    var result = await syncService.SyncAsync(calendar, CancellationToken.None);
                    succeeded++;
                    logger.LogInformation(
                        "CalendarSyncWorker: synced calendar {Id} (+{Inserted} inserted, +{Updated} updated).",
                        calendar.Id, result.Inserted, result.Updated);
                }
                catch (Exception ex)
                {
                    failed++;
                    calendar.LastSyncStatus = SyncStatus.Failed;
                    calendar.LastSyncErrorMessage = ex.Message.Length > 500
                        ? ex.Message[..500]
                        : ex.Message;
                    await context.SaveChangesAsync(CancellationToken.None);
                    logger.LogWarning(ex, "CalendarSyncWorker: sync failed for calendar {Id}.", calendar.Id);
                }
            }

            logger.LogInformation(
                "CalendarSyncWorker: run complete. Succeeded: {Succeeded}, Failed: {Failed}.",
                succeeded, failed);
        }

        logger.LogInformation("CalendarSyncWorker stopped.");
    }
}
