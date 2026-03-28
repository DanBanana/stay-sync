using StaySync.Domain.Common;
using StaySync.Domain.Enums;

namespace StaySync.Domain.Entities;

public class ExternalCalendar : Entity
{
    public Guid RoomId { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string IcsUrl { get; set; } = string.Empty;
    public DateTimeOffset? LastSyncedAt { get; set; }
    public SyncStatus? LastSyncStatus { get; set; }
    public string? LastSyncErrorMessage { get; set; }

    public Room Room { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = [];
}
