using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.Properties.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;

namespace StaySync.Application.Tests.Features.Properties;

public class DeletePropertyCommandHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_DeletesProperty_WhenOwner()
    {
        var pmId = Guid.NewGuid();
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = pmId };
        ctx.Properties.Add(property);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = pmId };
        var handler = new DeletePropertyCommandHandler(ctx, currentUser);

        await handler.Handle(new DeletePropertyCommand(property.Id), CancellationToken.None);

        ctx.Properties.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ThrowsForbiddenException_WhenNotOwner()
    {
        var ctx = CreateContext();
        var property = new Property { Name = "Beach House", PropertyManagerId = Guid.NewGuid() };
        ctx.Properties.Add(property);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "PropertyManager", PropertyManagerId = Guid.NewGuid() };
        var handler = new DeletePropertyCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new DeletePropertyCommand(property.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenPropertyDoesNotExist()
    {
        var ctx = CreateContext();
        var currentUser = new TestCurrentUserService { Role = "PropertyManager" };
        var handler = new DeletePropertyCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new DeletePropertyCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
