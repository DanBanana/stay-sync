using StaySync.Domain.Common;

namespace StaySync.Domain.Entities;

public class Room : Entity
{
    public Guid PropertyId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Property Property { get; set; } = null!;
    public ICollection<ExternalCalendar> ExternalCalendars { get; set; } = [];
}
