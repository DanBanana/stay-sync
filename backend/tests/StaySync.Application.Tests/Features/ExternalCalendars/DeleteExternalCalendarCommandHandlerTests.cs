using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.ExternalCalendars.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;

namespace StaySync.Application.Tests.Features.ExternalCalendars;

public class DeleteExternalCalendarCommandHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_DeletesCalendar_WhenOwner()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        var calendar = new ExternalCalendar { RoomId = room.Id, Room = room, Platform = "Airbnb", IcsUrl = "https://airbnb.com/ical/test.ics" };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        ctx.ExternalCalendars.Add(calendar);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new DeleteExternalCalendarCommandHandler(ctx, currentUser);

        await handler.Handle(new DeleteExternalCalendarCommand(calendar.Id), CancellationToken.None);

        var deleted = await ctx.ExternalCalendars.FindAsync(calendar.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenCalendarDoesNotExist()
    {
        var ctx = CreateContext();
        var currentUser = new TestCurrentUserService { Role = "PropertyManager" };
        var handler = new DeleteExternalCalendarCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new DeleteExternalCalendarCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
