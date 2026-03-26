using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.Bookings.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;
using StaySync.Domain.Exceptions;

namespace StaySync.Application.Tests.Features.Bookings;

public class UpdateManualBookingCommandHandlerTests
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
    public async Task Handle_UpdatesBooking_WhenOwnerAndManual()
    {
        var pmId = Guid.NewGuid();
        var (ctx, booking) = await SetupManualBooking(pmId);
        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new UpdateManualBookingCommandHandler(ctx, currentUser);

        await handler.Handle(
            new UpdateManualBookingCommand(booking.Id, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 7), "Bob"),
            CancellationToken.None);

        var updated = await ctx.Bookings.FindAsync(booking.Id);
        updated!.CheckIn.Should().Be(new DateOnly(2026, 5, 1));
        updated.CheckOut.Should().Be(new DateOnly(2026, 5, 7));
        updated.GuestName.Should().Be("Bob");
    }

    [Fact]
    public async Task Handle_ThrowsForbiddenException_WhenNotOwner()
    {
        var pmId = Guid.NewGuid();
        var (ctx, booking) = await SetupManualBooking(pmId);
        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = Guid.NewGuid() };
        var handler = new UpdateManualBookingCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(
                new UpdateManualBookingCommand(booking.Id, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 7), null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsConflictDetectedException_WhenUpdateOverlapsOtherBooking()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        var calendar = new ExternalCalendar { RoomId = room.Id, Platform = "Manual", IcsUrl = string.Empty };
        var bookingA = new Booking
        {
            RoomId = room.Id, ExternalCalendarId = calendar.Id, PropertyManagerId = pmId,
            ExternalUid = Guid.NewGuid().ToString(), CheckIn = new DateOnly(2026, 4, 1), CheckOut = new DateOnly(2026, 4, 7),
        };
        var bookingB = new Booking
        {
            RoomId = room.Id, ExternalCalendarId = calendar.Id, PropertyManagerId = pmId,
            ExternalUid = Guid.NewGuid().ToString(), CheckIn = new DateOnly(2026, 4, 15), CheckOut = new DateOnly(2026, 4, 20),
        };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        ctx.ExternalCalendars.Add(calendar);
        ctx.Bookings.AddRange(bookingA, bookingB);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new UpdateManualBookingCommandHandler(ctx, currentUser);

        // Update bookingB to overlap bookingA
        await Assert.ThrowsAsync<ConflictDetectedException>(() =>
            handler.Handle(
                new UpdateManualBookingCommand(bookingB.Id, new DateOnly(2026, 4, 5), new DateOnly(2026, 4, 10), null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DoesNotThrow_WhenUpdatingSelfWithoutOverlappingOthers()
    {
        var pmId = Guid.NewGuid();
        var (ctx, booking) = await SetupManualBooking(pmId);
        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new UpdateManualBookingCommandHandler(ctx, currentUser);

        // Shifting the same booking's dates — self-exclusion must prevent false conflict
        var act = () => handler.Handle(
            new UpdateManualBookingCommand(booking.Id, new DateOnly(2026, 4, 6), new DateOnly(2026, 4, 11), null),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
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
        var handler = new UpdateManualBookingCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(
                new UpdateManualBookingCommand(booking.Id, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 7), null),
                CancellationToken.None));
    }
}
