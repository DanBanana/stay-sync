using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;
using StaySync.Domain.Enums;
using StaySync.Domain.Interfaces;
using StaySync.Infrastructure.Services;

namespace StaySync.Application.Tests.Features.ExternalCalendars;

public class CalendarSyncServiceTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    private static (Property property, Room room, ExternalCalendar calendar) SeedCalendar(TestDbContext ctx, Guid pmId)
    {
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        var calendar = new ExternalCalendar
        {
            RoomId = room.Id,
            Room = room,
            Platform = "Airbnb",
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
    public async Task SyncAsync_InsertsBookings_WhenFeedReturnsNewEvents()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var (_, _, calendar) = SeedCalendar(ctx, pmId);

        var fetched = new[]
        {
            MakeBooking(calendar, "uid-1", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 5)),
            MakeBooking(calendar, "uid-2", new DateOnly(2025, 6, 10), new DateOnly(2025, 6, 15)),
        };

        var providerMock = new Mock<IBookingProvider>();
        providerMock.Setup(p => p.FetchBookingsAsync(It.IsAny<ExternalCalendar>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fetched);

        var service = new CalendarSyncService(ctx, providerMock.Object);
        var result = await service.SyncAsync(calendar);

        result.Inserted.Should().Be(2);
        result.Updated.Should().Be(0);

        var saved = await ctx.Bookings.Where(b => b.ExternalCalendarId == calendar.Id).ToListAsync();
        saved.Should().HaveCount(2);
    }

    [Fact]
    public async Task SyncAsync_UpdatesExistingBookings_WhenUidsMatch()
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

        var service = new CalendarSyncService(ctx, providerMock.Object);
        var result = await service.SyncAsync(calendar);

        result.Inserted.Should().Be(0);
        result.Updated.Should().Be(1);

        var saved = await ctx.Bookings.FirstAsync(b => b.ExternalUid == "uid-1");
        saved.CheckIn.Should().Be(new DateOnly(2025, 6, 2));
        saved.CheckOut.Should().Be(new DateOnly(2025, 6, 6));
    }

    [Fact]
    public async Task SyncAsync_SetsCancelledStatus_WhenEventIsCancelled()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var (_, _, calendar) = SeedCalendar(ctx, pmId);

        ctx.Bookings.Add(MakeBooking(calendar, "uid-1", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 5)));
        await ctx.SaveChangesAsync();

        var cancelled = MakeBooking(calendar, "uid-1", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 5),
            BookingStatus.Cancelled);

        var providerMock = new Mock<IBookingProvider>();
        providerMock.Setup(p => p.FetchBookingsAsync(It.IsAny<ExternalCalendar>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { cancelled });

        var service = new CalendarSyncService(ctx, providerMock.Object);
        await service.SyncAsync(calendar);

        var saved = await ctx.Bookings.FirstAsync(b => b.ExternalUid == "uid-1");
        saved.Status.Should().Be(BookingStatus.Cancelled);
    }

    [Fact]
    public async Task SyncAsync_SetsLastSyncStatusSuccess_AndClearsError()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var (_, _, calendar) = SeedCalendar(ctx, pmId);
        calendar.LastSyncStatus = SyncStatus.Failed;
        calendar.LastSyncErrorMessage = "Previous error";
        await ctx.SaveChangesAsync();

        var providerMock = new Mock<IBookingProvider>();
        providerMock.Setup(p => p.FetchBookingsAsync(It.IsAny<ExternalCalendar>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Booking>());

        var before = DateTimeOffset.UtcNow;
        var service = new CalendarSyncService(ctx, providerMock.Object);
        await service.SyncAsync(calendar);

        var saved = await ctx.ExternalCalendars.FindAsync(calendar.Id);
        saved!.LastSyncedAt.Should().BeOnOrAfter(before);
        saved.LastSyncStatus.Should().Be(SyncStatus.Success);
        saved.LastSyncErrorMessage.Should().BeNull();
    }
}
