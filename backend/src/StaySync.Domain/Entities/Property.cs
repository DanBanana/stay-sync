using StaySync.Domain.Common;

namespace StaySync.Domain.Entities;

public class Property : Entity
{
    public Guid PropertyManagerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }

    public PropertyManager PropertyManager { get; set; } = null!;
    public ICollection<Room> Rooms { get; set; } = [];
}
