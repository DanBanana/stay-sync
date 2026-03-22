namespace StaySync.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Role { get; }
    Guid? PropertyManagerId { get; }
    bool IsAuthenticated { get; }
}
