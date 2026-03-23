using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.ExternalCalendars.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;

namespace StaySync.Application.Tests.Features.ExternalCalendars;

public class CreateExternalCalendarCommandHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_CreatesCalendar_WhenOwnerOfRoom()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new CreateExternalCalendarCommandHandler(ctx, currentUser);

        var result = await handler.Handle(
            new CreateExternalCalendarCommand(room.Id, "Airbnb", "https://airbnb.com/ical/test.ics"),
            CancellationToken.None);

        result.Platform.Should().Be("Airbnb");
        result.RoomId.Should().Be(room.Id);
        result.LastSyncedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ThrowsForbiddenException_WhenNotOwnerOfRoom()
    {
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = Guid.NewGuid() };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = Guid.NewGuid() };
        var handler = new CreateExternalCalendarCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(
                new CreateExternalCalendarCommand(room.Id, "Airbnb", "https://airbnb.com/ical/test.ics"),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenRoomDoesNotExist()
    {
        var ctx = CreateContext();
        var currentUser = new TestCurrentUserService { Role = "PropertyManager" };
        var handler = new CreateExternalCalendarCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CreateExternalCalendarCommand(Guid.NewGuid(), "Airbnb", "https://airbnb.com/ical/test.ics"),
                CancellationToken.None));
    }
}
