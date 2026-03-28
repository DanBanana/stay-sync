using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.ExternalCalendars.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;

namespace StaySync.Application.Tests.Features.ExternalCalendars;

public class SyncCalendarCommandHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    private static (Property property, Room room, ExternalCalendar calendar) SeedCalendar(
        TestDbContext ctx, Guid pmId, string platform = "Airbnb")
    {
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        var calendar = new ExternalCalendar
        {
            RoomId = room.Id,
            Room = room,
            Platform = platform,
            IcsUrl = "https://airbnb.com/ical/test.ics"
        };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        ctx.ExternalCalendars.Add(calendar);
        ctx.SaveChanges();
        return (property, room, calendar);
    }

    [Fact]
    public async Task Handle_DelegatesToSyncService_WhenAuthorized()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var (_, _, calendar) = SeedCalendar(ctx, pmId);

        var syncServiceMock = new Mock<ICalendarSyncService>();
        syncServiceMock
            .Setup(s => s.SyncAsync(It.IsAny<ExternalCalendar>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncCalendarResult(3, 1));

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new SyncCalendarCommandHandler(ctx, currentUser, syncServiceMock.Object);

        var result = await handler.Handle(new SyncCalendarCommand(calendar.Id), CancellationToken.None);

        result.Inserted.Should().Be(3);
        result.Updated.Should().Be(1);
        syncServiceMock.Verify(s => s.SyncAsync(It.IsAny<ExternalCalendar>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenCalendarDoesNotExist()
    {
        var ctx = CreateContext();
        var syncServiceMock = new Mock<ICalendarSyncService>();
        var currentUser = new TestCurrentUserService { Role = "PropertyManager" };
        var handler = new SyncCalendarCommandHandler(ctx, currentUser, syncServiceMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new SyncCalendarCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsForbiddenException_WhenNotOwner()
    {
        var ctx = CreateContext();
        var (_, _, calendar) = SeedCalendar(ctx, Guid.NewGuid());

        var syncServiceMock = new Mock<ICalendarSyncService>();
        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = Guid.NewGuid() };
        var handler = new SyncCalendarCommandHandler(ctx, currentUser, syncServiceMock.Object);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new SyncCalendarCommand(calendar.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsBadRequestException_WhenCalendarIsManual()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var (_, _, calendar) = SeedCalendar(ctx, pmId, platform: "Manual");

        var syncServiceMock = new Mock<ICalendarSyncService>();
        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new SyncCalendarCommandHandler(ctx, currentUser, syncServiceMock.Object);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(new SyncCalendarCommand(calendar.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SuperAdminCanSyncAnyCalendar()
    {
        var ctx = CreateContext();
        var (_, _, calendar) = SeedCalendar(ctx, Guid.NewGuid());

        var syncServiceMock = new Mock<ICalendarSyncService>();
        syncServiceMock
            .Setup(s => s.SyncAsync(It.IsAny<ExternalCalendar>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncCalendarResult(0, 0));

        var currentUser = new TestCurrentUserService { Role = "SuperAdmin" };
        var handler = new SyncCalendarCommandHandler(ctx, currentUser, syncServiceMock.Object);

        var result = await handler.Handle(new SyncCalendarCommand(calendar.Id), CancellationToken.None);

        result.Should().NotBeNull();
        syncServiceMock.Verify(s => s.SyncAsync(It.IsAny<ExternalCalendar>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
