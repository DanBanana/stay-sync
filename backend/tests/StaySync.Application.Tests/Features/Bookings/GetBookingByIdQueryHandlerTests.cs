using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.Bookings.Queries;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;

namespace StaySync.Application.Tests.Features.Bookings;

public class GetBookingByIdQueryHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsBooking_WhenOwner()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var booking = new Booking
        {
            RoomId = Guid.NewGuid(),
            ExternalCalendarId = Guid.NewGuid(),
            PropertyManagerId = pmId,
            ExternalUid = "uid-1",
            CheckIn = new DateOnly(2025, 6, 1),
            CheckOut = new DateOnly(2025, 6, 7)
        };
        ctx.Bookings.Add(booking);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new GetBookingByIdQueryHandler(ctx, currentUser);

        var result = await handler.Handle(new GetBookingByIdQuery(booking.Id), CancellationToken.None);

        result.Id.Should().Be(booking.Id);
        result.ExternalUid.Should().Be("uid-1");
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenBookingDoesNotExist()
    {
        var ctx = CreateContext();
        var currentUser = new TestCurrentUserService { Role = "PropertyManager" };
        var handler = new GetBookingByIdQueryHandler(ctx, currentUser);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetBookingByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
