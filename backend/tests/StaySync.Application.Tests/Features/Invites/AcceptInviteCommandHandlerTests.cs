using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.Invites;
using StaySync.Application.Features.Invites.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;
using StaySync.Domain.Enums;
using StaySync.Infrastructure.Identity;

namespace StaySync.Application.Tests.Features.Invites;

public class AcceptInviteCommandHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    private static InviteToken CreateValidInvite(string rawToken, string email = "new@example.com")
        => new()
        {
            Email = email,
            Role = UserRole.PropertyManager,
            TokenHash = InviteTokenHelper.Hash(rawToken),
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedByUserId = Guid.NewGuid()
        };

    [Fact]
    public async Task Handle_CreatesUserAndPropertyManager_WhenValidInvite()
    {
        var rawToken = InviteTokenHelper.GenerateRawToken();
        var ctx = CreateContext();
        ctx.InviteTokens.Add(CreateValidInvite(rawToken));
        await ctx.SaveChangesAsync();

        var handler = new AcceptInviteCommandHandler(ctx, new BcryptPasswordHasher());

        await handler.Handle(
            new AcceptInviteCommand(rawToken, "Jane Doe", "Secure1!"),
            CancellationToken.None);

        var user = await ctx.Users.FirstOrDefaultAsync();
        user.Should().NotBeNull();
        user!.Email.Should().Be("new@example.com");
        user.IsActive.Should().BeTrue();

        var pm = await ctx.PropertyManagers.FirstOrDefaultAsync();
        pm.Should().NotBeNull();
        pm!.DisplayName.Should().Be("Jane Doe");

        var invite = await ctx.InviteTokens.FirstOrDefaultAsync();
        invite!.UsedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ThrowsBadRequestException_WhenInviteAlreadyUsed()
    {
        var rawToken = InviteTokenHelper.GenerateRawToken();
        var ctx = CreateContext();
        var invite = CreateValidInvite(rawToken);
        invite.UsedAt = DateTime.UtcNow;
        ctx.InviteTokens.Add(invite);
        await ctx.SaveChangesAsync();

        var handler = new AcceptInviteCommandHandler(ctx, new BcryptPasswordHasher());

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(new AcceptInviteCommand(rawToken, "Jane", "Secure1!"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsBadRequestException_WhenInviteExpired()
    {
        var rawToken = InviteTokenHelper.GenerateRawToken();
        var ctx = CreateContext();
        var invite = CreateValidInvite(rawToken);
        invite.ExpiresAt = DateTime.UtcNow.AddHours(-1);
        ctx.InviteTokens.Add(invite);
        await ctx.SaveChangesAsync();

        var handler = new AcceptInviteCommandHandler(ctx, new BcryptPasswordHasher());

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(new AcceptInviteCommand(rawToken, "Jane", "Secure1!"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsValidationException_WhenPasswordTooWeak()
    {
        var rawToken = InviteTokenHelper.GenerateRawToken();
        var ctx = CreateContext();
        ctx.InviteTokens.Add(CreateValidInvite(rawToken));
        await ctx.SaveChangesAsync();

        var handler = new AcceptInviteCommandHandler(ctx, new BcryptPasswordHasher());

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new AcceptInviteCommand(rawToken, "Jane", "weak"), CancellationToken.None));
    }
}
