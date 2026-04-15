using StaySync.Domain.Common;
using StaySync.Domain.Enums;

namespace StaySync.Domain.Entities;

public class InviteToken : Entity
{
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    /// <summary>SHA-256 hash of the raw token. The raw token is never persisted.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    /// <summary>Null = pending. Set on accept OR revoke.</summary>
    public DateTime? UsedAt { get; set; }

    public Guid CreatedByUserId { get; set; }

    public bool IsPending => UsedAt is null && ExpiresAt > DateTime.UtcNow;
}
