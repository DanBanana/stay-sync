using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.Rooms.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;

namespace StaySync.Application.Tests.Features.Rooms;

public class UpdateRoomCommandHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_UpdatesRoom_WhenOwner()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new UpdateRoomCommandHandler(ctx, currentUser);

        var result = await handler.Handle(new UpdateRoomCommand(room.Id, "Room B"), CancellationToken.None);

        result.Name.Should().Be("Room B");
        result.Id.Should().Be(room.Id);
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
        var handler = new UpdateRoomCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new UpdateRoomCommand(room.Id, "Room B"), CancellationToken.None));
    }
}
