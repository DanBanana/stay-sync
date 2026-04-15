using FluentAssertions;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Features.Invites;
using StaySync.Application.Features.Invites.Queries;
using StaySync.Application.Tests.Common;
using StaySync.Domain.Entities;
using StaySync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace StaySync.Application.Tests.Features.Invites;

public class ValidateInviteQueryHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsEmailAndRole_WhenTokenIsValid()
    {
        var rawToken = InviteTokenHelper.GenerateRawToken();
        var ctx = CreateContext();
        ctx.InviteTokens.Add(new InviteToken
        {
            Email = "invited@example.com",
            Role = UserRole.PropertyManager,
            TokenHash = InviteTokenHelper.Hash(rawToken),
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedByUserId = Guid.NewGuid()
        });
        await ctx.SaveChangesAsync();

        var handler = new ValidateInviteQueryHandler(ctx);
        var result = await handler.Handle(new ValidateInviteQuery(rawToken), CancellationToken.None);

        result.Email.Should().Be("invited@example.com");
        result.Role.Should().Be("PropertyManager");
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenTokenUnknown()
    {
        var ctx = CreateContext();
        var handler = new ValidateInviteQueryHandler(ctx);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new ValidateInviteQuery("unknowntoken"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsBadRequestException_WhenTokenExpired()
    {
        var rawToken = InviteTokenHelper.GenerateRawToken();
        var ctx = CreateContext();
        ctx.InviteTokens.Add(new InviteToken
        {
            Email = "invited@example.com",
            Role = UserRole.PropertyManager,
            TokenHash = InviteTokenHelper.Hash(rawToken),
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            CreatedByUserId = Guid.NewGuid()
        });
        await ctx.SaveChangesAsync();

        var handler = new ValidateInviteQueryHandler(ctx);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(new ValidateInviteQuery(rawToken), CancellationToken.None));
    }
}
