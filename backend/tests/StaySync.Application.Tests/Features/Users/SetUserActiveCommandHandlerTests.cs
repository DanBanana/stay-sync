using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.Users.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;
using StaySync.Domain.Enums;

namespace StaySync.Application.Tests.Features.Users;

public class SetUserActiveCommandHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_DeactivatesUser_WhenSuperAdmin()
    {
        var adminId = Guid.NewGuid();
        var targetUser = new User { Email = "pm@example.com", PasswordHash = "hash", Role = UserRole.PropertyManager };
        var ctx = CreateContext();
        ctx.Users.Add(targetUser);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "SuperAdmin", UserId = adminId };
        var handler = new SetUserActiveCommandHandler(ctx, currentUser);

        await handler.Handle(new SetUserActiveCommand(targetUser.Id, false), CancellationToken.None);

        var updated = await ctx.Users.FindAsync(targetUser.Id);
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ThrowsBadRequestException_WhenDeactivatingSelf()
    {
        var adminId = Guid.NewGuid();
        var admin = new User { Id = adminId, Email = "admin@example.com", PasswordHash = "hash", Role = UserRole.SuperAdmin };
        var ctx = CreateContext();
        ctx.Users.Add(admin);
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "SuperAdmin", UserId = adminId };
        var handler = new SetUserActiveCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(new SetUserActiveCommand(adminId, false), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsForbiddenException_WhenNotSuperAdmin()
    {
        var ctx = CreateContext();
        var currentUser = new TestCurrentUserService { Role = "PropertyManager" };
        var handler = new SetUserActiveCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new SetUserActiveCommand(Guid.NewGuid(), false), CancellationToken.None));
    }
}
