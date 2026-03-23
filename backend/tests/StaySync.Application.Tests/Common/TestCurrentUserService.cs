using StaySync.Application.Common.Interfaces;

namespace StaySync.Application.Tests.Common;

public class TestCurrentUserService : ICurrentUserService
{
    public Guid UserId { get; init; } = Guid.NewGuid();
    public string Role { get; init; } = "PropertyManager";
    public Guid? PropertyManagerId { get; init; } = Guid.NewGuid();
    public bool IsAuthenticated => true;
}
