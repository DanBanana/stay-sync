using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.Bookings.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;

namespace StaySync.Application.Tests.Features.Bookings;

public class DeleteManualBookingCommandHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    private static async Task<(TestDbContext ctx, Booking booking)> SetupManualBooking(Guid pmId)
    {
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        var calendar = new ExternalCalendar { RoomId = room.Id, Platform = "Manual", IcsUrl = string.Empty };
        var booking = new Booking
        {
            RoomId = room.Id,
            ExternalCalendarId = calendar.Id,
            PropertyManagerId = pmId,
            ExternalUid = Guid.NewGuid().ToString(),
            CheckIn = new DateOnly(2026, 4, 5),
            CheckOut = new DateOnly(2026, 4, 10),
        };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        ctx.ExternalCalendars.Add(calendar);
        ctx.Bookings.Add(booking);
        await ctx.SaveChangesAsync();
        return (ctx, booking);
    }

    [Fact]
    public async Task Handle_DeletesBooking_WhenOwnerAndManual()
    {
        var pmId = Guid.NewGuid();
        var (ctx, booking) = await SetupManualBooking(pmId);
        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new DeleteManualBookingCommandHandler(ctx, currentUser);

        await handler.Handle(new DeleteManualBookingCommand(booking.Id), CancellationToken.None);

        var deleted = await ctx.Bookings.FindAsync(booking.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ThrowsForbiddenException_WhenNotOwner()
    {
        var pmId = Guid.NewGuid();
        var (ctx, booking) = await SetupManualBooking(pmId);
        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = Guid.NewGuid() };
        var handler = new DeleteManualBookingCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new DeleteManualBookingCommand(booking.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsBadRequestException_WhenNotManual()
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
            CheckIn = new DateOnly(2026, 4, 5),
            CheckOut = new DateOnly(2026, 4, 10),
        };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        ctx.ExternalCalendars.Add(calendar);
        ctx.Bookings.Add(booking);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new DeleteManualBookingCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(new DeleteManualBookingCommand(booking.Id), CancellationToken.None));
    }
}
