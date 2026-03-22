using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using StaySync.Application.Common.Interfaces;

namespace StaySync.Infrastructure.Identity;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid UserId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue("sub"), out var id)
            ? id
            : Guid.Empty;

    public string Role => User?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    public Guid? PropertyManagerId =>
        Guid.TryParse(User?.FindFirstValue("property_manager_id"), out var id)
            ? id
            : null;
}
