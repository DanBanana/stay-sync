using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Auth.Commands;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;
using StaySync.Domain.Enums;

namespace StaySync.Application.Tests.Features.Auth;

public class LoginCommandHandlerHardeningTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    private static (TestDbContext, LoginCommandHandler) BuildHandler(TestDbContext ctx, bool passwordMatches = false)
    {
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(t => t.GenerateToken(It.IsAny<User>(), It.IsAny<PropertyManager?>()))
            .Returns("jwt-token");

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(passwordMatches);
        hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");

        var handler = new LoginCommandHandler(ctx, tokenService.Object, hasher.Object);
        return (ctx, handler);
    }

    [Fact]
    public async Task Handle_ThrowsForbiddenException_WhenUserIsInactive()
    {
        var ctx = CreateContext();
        ctx.Users.Add(new User
        {
            Email = "inactive@example.com",
            PasswordHash = "hash",
            Role = UserRole.PropertyManager,
            IsActive = false
        });
        await ctx.SaveChangesAsync();

        var (_, handler) = BuildHandler(ctx, passwordMatches: true);

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new LoginCommand("inactive@example.com", "pass"), CancellationToken.None));

        ex.Message.Should().Contain("disabled");
    }

    [Fact]
    public async Task Handle_ThrowsForbiddenException_WhenAccountIsLocked()
    {
        var ctx = CreateContext();
        ctx.Users.Add(new User
        {
            Email = "locked@example.com",
            PasswordHash = "hash",
            Role = UserRole.PropertyManager,
            IsActive = true,
            LockoutUntil = DateTime.UtcNow.AddMinutes(10)
        });
        await ctx.SaveChangesAsync();

        var (_, handler) = BuildHandler(ctx, passwordMatches: true);

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new LoginCommand("locked@example.com", "pass"), CancellationToken.None));

        ex.Message.Should().Contain("locked");
    }

    [Fact]
    public async Task Handle_LocksAccount_AfterFiveFailedAttempts()
    {
        var ctx = CreateContext();
        var user = new User
        {
            Email = "user@example.com",
            PasswordHash = "hash",
            Role = UserRole.PropertyManager,
            IsActive = true,
            FailedLoginAttempts = 4
        };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var (db, handler) = BuildHandler(ctx, passwordMatches: false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new LoginCommand("user@example.com", "wrongpass"), CancellationToken.None));

        var updated = await db.Users.FirstOrDefaultAsync(u => u.Email == "user@example.com");
        updated!.LockoutUntil.Should().NotBeNull();
        updated.LockoutUntil.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_ResetsFailedAttempts_OnSuccessfulLogin()
    {
        var ctx = CreateContext();
        var user = new User
        {
            Email = "user@example.com",
            PasswordHash = "hash",
            Role = UserRole.PropertyManager,
            IsActive = true,
            FailedLoginAttempts = 3
        };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(t => t.GenerateToken(It.IsAny<User>(), It.IsAny<PropertyManager?>()))
            .Returns("jwt-token");

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var handler = new LoginCommandHandler(ctx, tokenService.Object, hasher.Object);

        await handler.Handle(new LoginCommand("user@example.com", "correctpass"), CancellationToken.None);

        var updated = await ctx.Users.FirstOrDefaultAsync(u => u.Email == "user@example.com");
        updated!.FailedLoginAttempts.Should().Be(0);
        updated.LockoutUntil.Should().BeNull();
    }
}
