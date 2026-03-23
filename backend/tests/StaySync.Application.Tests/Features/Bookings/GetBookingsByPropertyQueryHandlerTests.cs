using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.Bookings.Queries;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;

namespace StaySync.Application.Tests.Features.Bookings;

public class GetBookingsByPropertyQueryHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsBookings_WhenOwner()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        var calendar = new ExternalCalendar { RoomId = room.Id, Platform = "Airbnb", IcsUrl = "https://airbnb.com/ical/test.ics" };
        var booking = new Booking
        {
            RoomId = room.Id,
            ExternalCalendarId = calendar.Id,
            PropertyManagerId = pmId,
            ExternalUid = "uid-1",
            CheckIn = new DateOnly(2025, 6, 1),
            CheckOut = new DateOnly(2025, 6, 7)
        };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        ctx.ExternalCalendars.Add(calendar);
        ctx.Bookings.Add(booking);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new GetBookingsByPropertyQueryHandler(ctx, currentUser);

        var result = await handler.Handle(new GetBookingsByPropertyQuery(property.Id), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].RoomId.Should().Be(room.Id);
    }

    [Fact]
    public async Task Handle_ThrowsForbiddenException_WhenNotOwner()
    {
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = Guid.NewGuid() };
        ctx.Properties.Add(property);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = Guid.NewGuid() };
        var handler = new GetBookingsByPropertyQueryHandler(ctx, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new GetBookingsByPropertyQuery(property.Id), CancellationToken.None));
    }
}
