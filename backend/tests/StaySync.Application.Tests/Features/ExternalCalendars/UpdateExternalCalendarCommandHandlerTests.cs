using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.ExternalCalendars.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;

namespace StaySync.Application.Tests.Features.ExternalCalendars;

public class UpdateExternalCalendarCommandHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_UpdatesCalendar_WhenOwner()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        var calendar = new ExternalCalendar { RoomId = room.Id, Room = room, Platform = "Airbnb", IcsUrl = "https://airbnb.com/ical/old.ics" };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        ctx.ExternalCalendars.Add(calendar);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new UpdateExternalCalendarCommandHandler(ctx, currentUser);

        var result = await handler.Handle(
            new UpdateExternalCalendarCommand(calendar.Id, "Booking.com", "https://booking.com/ical/new.ics"),
            CancellationToken.None);

        result.Platform.Should().Be("Booking.com");
        result.IcsUrl.Should().Be("https://booking.com/ical/new.ics");
    }

    [Fact]
    public async Task Handle_ThrowsForbiddenException_WhenNotOwner()
    {
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = Guid.NewGuid() };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        var calendar = new ExternalCalendar { RoomId = room.Id, Room = room, Platform = "Airbnb", IcsUrl = "https://airbnb.com/ical/test.ics" };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        ctx.ExternalCalendars.Add(calendar);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = Guid.NewGuid() };
        var handler = new UpdateExternalCalendarCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(
                new UpdateExternalCalendarCommand(calendar.Id, "Booking.com", "https://booking.com/ical/new.ics"),
                CancellationToken.None));
    }
}
