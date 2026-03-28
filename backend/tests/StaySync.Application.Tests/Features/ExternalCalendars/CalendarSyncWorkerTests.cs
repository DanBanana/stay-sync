using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.ExternalCalendars.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;
using StaySync.Domain.Enums;
using StaySync.Infrastructure.BackgroundServices;

namespace StaySync.Application.Tests.Features.ExternalCalendars;

public class CalendarSyncWorkerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    private static IServiceScope BuildScope(TestDbContext ctx, ICalendarSyncService syncService)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IApplicationDbContext>(ctx);
        services.AddSingleton(syncService);

        var provider = services.BuildServiceProvider();
        return provider.CreateScope();
    }

    private static IServiceScopeFactory BuildScopeFactory(TestDbContext ctx, ICalendarSyncService syncService)
    {
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        serviceProviderMock.Setup(p => p.GetService(typeof(IApplicationDbContext))).Returns(ctx);
        serviceProviderMock.Setup(p => p.GetService(typeof(ICalendarSyncService))).Returns(syncService);
        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        return scopeFactoryMock.Object;
    }

    private static ExternalCalendar SeedCalendar(TestDbContext ctx, string platform = "Airbnb")
    {
        var pmId = Guid.NewGuid();
        var property = new Property { Name = "Test Property", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        var calendar = new ExternalCalendar
        {
            RoomId = room.Id,
            Room = room,
            Platform = platform,
            IcsUrl = "https://example.com/cal.ics"
        };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        ctx.ExternalCalendars.Add(calendar);
        ctx.SaveChanges();
        return calendar;
    }

    [Fact]
    public async Task Worker_SyncsAllNonManualCalendars()
    {
        var ctx = CreateContext();
        var cal1 = SeedCalendar(ctx, "Airbnb");
        var cal2 = SeedCalendar(ctx, "Booking.com");
        SeedCalendar(ctx, "Manual");

        var syncServiceMock = new Mock<ICalendarSyncService>();
        syncServiceMock
            .Setup(s => s.SyncAsync(It.IsAny<ExternalCalendar>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncCalendarResult(1, 0));

        var scopeFactory = BuildScopeFactory(ctx, syncServiceMock.Object);
        var options = Options.Create(new BackgroundSyncOptions { IntervalMinutes = 0 });
        var worker = new CalendarSyncWorker(scopeFactory, options, NullLogger<CalendarSyncWorker>.Instance);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(200));

        await worker.StartAsync(cts.Token);
        await Task.Delay(300);
        await worker.StopAsync(CancellationToken.None);

        syncServiceMock.Verify(
            s => s.SyncAsync(It.Is<ExternalCalendar>(c => c.Platform != "Manual"), It.IsAny<CancellationToken>()),
            Times.AtLeast(2));
        syncServiceMock.Verify(
            s => s.SyncAsync(It.Is<ExternalCalendar>(c => c.Platform == "Manual"), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Worker_MarksSyncFailed_WhenServiceThrows()
    {
        var ctx = CreateContext();
        var calendar = SeedCalendar(ctx, "Airbnb");

        var syncServiceMock = new Mock<ICalendarSyncService>();
        syncServiceMock
            .Setup(s => s.SyncAsync(It.IsAny<ExternalCalendar>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("ICS fetch failed"));

        var scopeFactory = BuildScopeFactory(ctx, syncServiceMock.Object);
        var options = Options.Create(new BackgroundSyncOptions { IntervalMinutes = 0 });
        var worker = new CalendarSyncWorker(scopeFactory, options, NullLogger<CalendarSyncWorker>.Instance);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(200));

        await worker.StartAsync(cts.Token);
        await Task.Delay(300);
        await worker.StopAsync(CancellationToken.None);

        var saved = await ctx.ExternalCalendars.FindAsync(calendar.Id);
        saved!.LastSyncStatus.Should().Be(SyncStatus.Failed);
        saved.LastSyncErrorMessage.Should().Be("ICS fetch failed");
    }

    [Fact]
    public async Task Worker_ContinuesAfterOneCalendarFails()
    {
        var ctx = CreateContext();
        var failingCal = SeedCalendar(ctx, "Airbnb");
        var successCal = SeedCalendar(ctx, "Booking.com");

        var syncServiceMock = new Mock<ICalendarSyncService>();
        syncServiceMock
            .Setup(s => s.SyncAsync(It.Is<ExternalCalendar>(c => c.Id == failingCal.Id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Fetch error"));
        syncServiceMock
            .Setup(s => s.SyncAsync(It.Is<ExternalCalendar>(c => c.Id == successCal.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncCalendarResult(2, 0));

        var scopeFactory = BuildScopeFactory(ctx, syncServiceMock.Object);
        var options = Options.Create(new BackgroundSyncOptions { IntervalMinutes = 0 });
        var worker = new CalendarSyncWorker(scopeFactory, options, NullLogger<CalendarSyncWorker>.Instance);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(200));

        await worker.StartAsync(cts.Token);
        await Task.Delay(300);
        await worker.StopAsync(CancellationToken.None);

        syncServiceMock.Verify(
            s => s.SyncAsync(It.Is<ExternalCalendar>(c => c.Id == successCal.Id), It.IsAny<CancellationToken>()),
            Times.AtLeast(1));

        var savedFailing = await ctx.ExternalCalendars.FindAsync(failingCal.Id);
        savedFailing!.LastSyncStatus.Should().Be(SyncStatus.Failed);
    }
}
