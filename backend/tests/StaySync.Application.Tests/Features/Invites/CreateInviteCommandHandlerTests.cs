using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.Invites.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;
using StaySync.Domain.Enums;

namespace StaySync.Application.Tests.Features.Invites;

public class CreateInviteCommandHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsToken_WhenValidRequest()
    {
        var ctx = CreateContext();
        var currentUser = new TestCurrentUserService { Role = "SuperAdmin" };
        var handler = new CreateInviteCommandHandler(ctx, currentUser);

        var result = await handler.Handle(
            new CreateInviteCommand("newuser@example.com", UserRole.PropertyManager),
            CancellationToken.None);

        result.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        var saved = await ctx.InviteTokens.FirstOrDefaultAsync();
        saved.Should().NotBeNull();
        saved!.Email.Should().Be("newuser@example.com");
        saved.TokenHash.Should().NotBe(result.Token); // raw token is never stored
    }

    [Fact]
    public async Task Handle_ThrowsForbiddenException_WhenNotSuperAdmin()
    {
        var ctx = CreateContext();
        var currentUser = new TestCurrentUserService { Role = "PropertyManager" };
        var handler = new CreateInviteCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(
                new CreateInviteCommand("newuser@example.com", UserRole.PropertyManager),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsValidationException_WhenEmailAlreadyHasAccount()
    {
        var ctx = CreateContext();
        ctx.Users.Add(new User { Email = "existing@example.com", PasswordHash = "hash", Role = UserRole.PropertyManager });
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "SuperAdmin" };
        var handler = new CreateInviteCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(
                new CreateInviteCommand("existing@example.com", UserRole.PropertyManager),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsValidationException_WhenPendingInviteAlreadyExists()
    {
        var ctx = CreateContext();
        ctx.InviteTokens.Add(new InviteToken
        {
            Email = "pending@example.com",
            Role = UserRole.PropertyManager,
            TokenHash = "somehash",
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedByUserId = Guid.NewGuid()
        });
        await ctx.SaveChangesAsync();

        var currentUser = new TestCurrentUserService { Role = "SuperAdmin" };
        var handler = new CreateInviteCommandHandler(ctx, currentUser);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(
                new CreateInviteCommand("pending@example.com", UserRole.PropertyManager),
                CancellationToken.None));
    }
}
