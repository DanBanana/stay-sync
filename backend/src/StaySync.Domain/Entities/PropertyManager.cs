using StaySync.Domain.Common;

namespace StaySync.Domain.Entities;

public class PropertyManager : Entity
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    public User User { get; set; } = null!;
    public ICollection<Property> Properties { get; set; } = [];
}
