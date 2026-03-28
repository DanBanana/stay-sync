using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.ExternalCalendars.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;
using StaySync.Domain.Enums;
using StaySync.Domain.Interfaces;

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

    private static Booking MakeBooking(ExternalCalendar cal, string uid, DateOnly checkIn, DateOnly checkOut,
        BookingStatus status = BookingStatus.Confirmed) => new()
    {
        ExternalCalendarId = cal.Id,
        RoomId = cal.RoomId,
        PropertyManagerId = cal.Room.Property.PropertyManagerId,
        ExternalUid = uid,
        CheckIn = checkIn,
        CheckOut = checkOut,
        Status = status,
        RawSummary = "Airbnb (Not available)",
        GuestName = null
    };

    [Fact]
    public async Task Handle_InsertsBookings_WhenFeedReturnsNewEvents()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var (_, _, calendar) = SeedCalendar(ctx, pmId);

        var fetched = new[]
        {
            MakeBooking(calendar, "uid-1", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 5)),
            MakeBooking(calendar, "uid-2", new DateOnly(2025, 6, 10), new DateOnly(2025, 6, 15)),
            MakeBooking(calendar, "uid-3", new DateOnly(2025, 6, 20), new DateOnly(2025, 6, 25)),
        };

        var providerMock = new Mock<IBookingProvider>();
        providerMock.Setup(p => p.FetchBookingsAsync(It.IsAny<ExternalCalendar>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fetched);

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new SyncCalendarCommandHandler(ctx, currentUser, providerMock.Object);

        var result = await handler.Handle(new SyncCalendarCommand(calendar.Id), CancellationToken.None);

        result.Inserted.Should().Be(3);
        result.Updated.Should().Be(0);

        var saved = await ctx.Bookings.Where(b => b.ExternalCalendarId == calendar.Id).ToListAsync();
        saved.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_UpdatesExistingBookings_WhenUidsMatch()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var (_, _, calendar) = SeedCalendar(ctx, pmId);

        var existing = MakeBooking(calendar, "uid-1", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 5));
        ctx.Bookings.Add(existing);
        await ctx.SaveChangesAsync();

        var updated = MakeBooking(calendar, "uid-1", new DateOnly(2025, 6, 2), new DateOnly(2025, 6, 6));

        var providerMock = new Mock<IBookingProvider>();
        providerMock.Setup(p => p.FetchBookingsAsync(It.IsAny<ExternalCalendar>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { updated });

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new SyncCalendarCommandHandler(ctx, currentUser, providerMock.Object);

        var result = await handler.Handle(new SyncCalendarCommand(calendar.Id), CancellationToken.None);

        result.Inserted.Should().Be(0);
        result.Updated.Should().Be(1);

        var saved = await ctx.Bookings.Where(b => b.ExternalCalendarId == calendar.Id).ToListAsync();
        saved.Should().HaveCount(1);
        saved[0].CheckIn.Should().Be(new DateOnly(2025, 6, 2));
        saved[0].CheckOut.Should().Be(new DateOnly(2025, 6, 6));
    }

    [Fact]
    public async Task Handle_SetsCancelledStatus_WhenEventIsCancelled()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var (_, _, calendar) = SeedCalendar(ctx, pmId);

        var existing = MakeBooking(calendar, "uid-1", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 5));
        ctx.Bookings.Add(existing);
        await ctx.SaveChangesAsync();

        var cancelled = MakeBooking(calendar, "uid-1", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 5),
            BookingStatus.Cancelled);

        var providerMock = new Mock<IBookingProvider>();
        providerMock.Setup(p => p.FetchBookingsAsync(It.IsAny<ExternalCalendar>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { cancelled });

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new SyncCalendarCommandHandler(ctx, currentUser, providerMock.Object);

        await handler.Handle(new SyncCalendarCommand(calendar.Id), CancellationToken.None);

        var saved = await ctx.Bookings.FirstAsync(b => b.ExternalUid == "uid-1");
        saved.Status.Should().Be(BookingStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_UpdatesLastSyncedAt()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var (_, _, calendar) = SeedCalendar(ctx, pmId);

        var providerMock = new Mock<IBookingProvider>();
        providerMock.Setup(p => p.FetchBookingsAsync(It.IsAny<ExternalCalendar>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Booking>());

        var before = DateTimeOffset.UtcNow;
        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new SyncCalendarCommandHandler(ctx, currentUser, providerMock.Object);

        await handler.Handle(new SyncCalendarCommand(calendar.Id), CancellationToken.None);

        var saved = await ctx.ExternalCalendars.FindAsync(calendar.Id);
        saved!.LastSyncedAt.Should().NotBeNull();
        saved.LastSyncedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenCalendarDoesNotExist()
    {
        var ctx = CreateContext();
        var providerMock = new Mock<IBookingProvider>();
        var currentUser = new TestCurrentUserService { Role = "PropertyManager" };
        var handler = new SyncCalendarCommandHandler(ctx, currentUser, providerMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new SyncCalendarCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsForbiddenException_WhenNotOwner()
    {
        var ctx = CreateContext();
        var (_, _, calendar) = SeedCalendar(ctx, Guid.NewGuid());

        var providerMock = new Mock<IBookingProvider>();
        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = Guid.NewGuid() };
        var handler = new SyncCalendarCommandHandler(ctx, currentUser, providerMock.Object);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new SyncCalendarCommand(calendar.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsBadRequestException_WhenCalendarIsManual()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var (_, _, calendar) = SeedCalendar(ctx, pmId, platform: "Manual");

        var providerMock = new Mock<IBookingProvider>();
        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new SyncCalendarCommandHandler(ctx, currentUser, providerMock.Object);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(new SyncCalendarCommand(calendar.Id), CancellationToken.None));
    }
}
