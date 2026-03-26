using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.Bookings.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;
using StaySync.Domain.Enums;
using StaySync.Domain.Exceptions;

namespace StaySync.Application.Tests.Features.Bookings;

public class CreateManualBookingCommandHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_CreatesBookingAndManualCalendar_WhenOwner()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new CreateManualBookingCommandHandler(ctx, currentUser);

        var bookingId = await handler.Handle(
            new CreateManualBookingCommand(room.Id, new DateOnly(2026, 4, 5), new DateOnly(2026, 4, 10), "Alice"),
            CancellationToken.None);

        var booking = await ctx.Bookings.FindAsync(bookingId);
        booking.Should().NotBeNull();
        booking!.CheckIn.Should().Be(new DateOnly(2026, 4, 5));
        booking.CheckOut.Should().Be(new DateOnly(2026, 4, 10));
        booking.GuestName.Should().Be("Alice");
        booking.Status.Should().Be(BookingStatus.Confirmed);

        var manualCalendar = await ctx.ExternalCalendars
            .FirstOrDefaultAsync(ec => ec.RoomId == room.Id && ec.Platform == "Manual");
        manualCalendar.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ReusesExistingManualCalendar_OnSecondBooking()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new CreateManualBookingCommandHandler(ctx, currentUser);

        await handler.Handle(
            new CreateManualBookingCommand(room.Id, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 5), null),
            CancellationToken.None);

        await handler.Handle(
            new CreateManualBookingCommand(room.Id, new DateOnly(2026, 4, 10), new DateOnly(2026, 4, 15), null),
            CancellationToken.None);

        var calendarCount = await ctx.ExternalCalendars.CountAsync(ec => ec.RoomId == room.Id && ec.Platform == "Manual");
        calendarCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ThrowsForbiddenException_WhenNotOwner()
    {
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = Guid.NewGuid() };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = Guid.NewGuid() };
        var handler = new CreateManualBookingCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(
                new CreateManualBookingCommand(room.Id, new DateOnly(2026, 4, 5), new DateOnly(2026, 4, 10), null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsConflictDetectedException_WhenDatesOverlapExistingBooking()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new CreateManualBookingCommandHandler(ctx, currentUser);

        await handler.Handle(
            new CreateManualBookingCommand(room.Id, new DateOnly(2026, 4, 5), new DateOnly(2026, 4, 10), null),
            CancellationToken.None);

        await Assert.ThrowsAsync<ConflictDetectedException>(() =>
            handler.Handle(
                new CreateManualBookingCommand(room.Id, new DateOnly(2026, 4, 7), new DateOnly(2026, 4, 12), null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Succeeds_WhenBookingsAreAdjacent()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new CreateManualBookingCommandHandler(ctx, currentUser);

        await handler.Handle(
            new CreateManualBookingCommand(room.Id, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 5), null),
            CancellationToken.None);

        // checkIn of second == checkOut of first — half-open interval, no overlap
        var act = () => handler.Handle(
            new CreateManualBookingCommand(room.Id, new DateOnly(2026, 4, 5), new DateOnly(2026, 4, 10), null),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
