using StaySync.Domain.Common;

namespace StaySync.Domain.Entities;

public class ExternalCalendar : Entity
{
    public Guid RoomId { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string IcsUrl { get; set; } = string.Empty;
    public DateTimeOffset? LastSyncedAt { get; set; }

    public Room Room { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = [];
}
