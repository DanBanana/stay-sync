using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.Bookings.Queries;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;

namespace StaySync.Application.Tests.Features.Bookings;

public class GetBookingsForCalendarQueryHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsBookingsInRange_WhenOwner()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        var calendar = new ExternalCalendar { RoomId = room.Id, Platform = "Airbnb", IcsUrl = "https://airbnb.com/ical/test.ics" };
        var bookingInRange = new Booking
        {
            RoomId = room.Id,
            ExternalCalendarId = calendar.Id,
            PropertyManagerId = pmId,
            ExternalUid = "uid-1",
            CheckIn = new DateOnly(2026, 4, 5),
            CheckOut = new DateOnly(2026, 4, 10)
        };
        var bookingOutOfRange = new Booking
        {
            RoomId = room.Id,
            ExternalCalendarId = calendar.Id,
            PropertyManagerId = pmId,
            ExternalUid = "uid-2",
            CheckIn = new DateOnly(2026, 5, 10),
            CheckOut = new DateOnly(2026, 5, 17)
        };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        ctx.ExternalCalendars.Add(calendar);
        ctx.Bookings.Add(bookingInRange);
        ctx.Bookings.Add(bookingOutOfRange);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new GetBookingsForCalendarQueryHandler(ctx, currentUser);

        var result = await handler.Handle(
            new GetBookingsForCalendarQuery(property.Id, new DateOnly(2026, 3, 24), new DateOnly(2026, 5, 4)),
            CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Id.Should().NotBeEmpty();
        result[0].RoomName.Should().Be("Room A");
        result[0].Platform.Should().Be("Airbnb");
        result[0].CheckIn.Should().Be(new DateOnly(2026, 4, 5));
    }

    [Fact]
    public async Task Handle_ExcludesBookingsOutsideRange()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var property = new Property { Name = "Mountain Lodge", PropertyManagerId = pmId };
        var room = new Room { Name = "Suite B", PropertyId = property.Id, Property = property };
        var calendar = new ExternalCalendar { RoomId = room.Id, Platform = "Booking.com", IcsUrl = "https://booking.com/ical/test.ics" };
        // Booking ends exactly on the From date — should be excluded (CheckOut == From means no overlap)
        var bookingBefore = new Booking
        {
            RoomId = room.Id,
            ExternalCalendarId = calendar.Id,
            PropertyManagerId = pmId,
            ExternalUid = "uid-before",
            CheckIn = new DateOnly(2026, 3, 1),
            CheckOut = new DateOnly(2026, 3, 24)
        };
        // Booking starts exactly on the To date — should be excluded (CheckIn == To means no overlap)
        var bookingAfter = new Booking
        {
            RoomId = room.Id,
            ExternalCalendarId = calendar.Id,
            PropertyManagerId = pmId,
            ExternalUid = "uid-after",
            CheckIn = new DateOnly(2026, 5, 4),
            CheckOut = new DateOnly(2026, 5, 10)
        };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        ctx.ExternalCalendars.Add(calendar);
        ctx.Bookings.Add(bookingBefore);
        ctx.Bookings.Add(bookingAfter);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new GetBookingsForCalendarQueryHandler(ctx, currentUser);

        var result = await handler.Handle(
            new GetBookingsForCalendarQuery(property.Id, new DateOnly(2026, 3, 24), new DateOnly(2026, 5, 4)),
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ThrowsForbiddenException_WhenNotOwner()
    {
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = Guid.NewGuid() };
        ctx.Properties.Add(property);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = Guid.NewGuid() };
        var handler = new GetBookingsForCalendarQueryHandler(ctx, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(
                new GetBookingsForCalendarQuery(property.Id, new DateOnly(2026, 3, 24), new DateOnly(2026, 5, 4)),
                CancellationToken.None));
    }
}
