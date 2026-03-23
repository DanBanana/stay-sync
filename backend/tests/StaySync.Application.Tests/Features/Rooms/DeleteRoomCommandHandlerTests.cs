using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.Rooms.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;

namespace StaySync.Application.Tests.Features.Rooms;

public class DeleteRoomCommandHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_DeletesRoom_WhenOwner()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        var room = new Room { Name = "Room A", PropertyId = property.Id, Property = property };
        ctx.Properties.Add(property);
        ctx.Rooms.Add(room);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new DeleteRoomCommandHandler(ctx, currentUser);

        await handler.Handle(new DeleteRoomCommand(room.Id), CancellationToken.None);

        ctx.Rooms.Should().BeEmpty();
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
        var handler = new DeleteRoomCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new DeleteRoomCommand(room.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenRoomDoesNotExist()
    {
        var ctx = CreateContext();
        var currentUser = new TestCurrentUserService { Role = "PropertyManager" };
        var handler = new DeleteRoomCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new DeleteRoomCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
