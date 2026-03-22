using StaySync.Domain.Common;
using StaySync.Domain.Enums;

namespace StaySync.Domain.Entities;

public class User : Entity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public PropertyManager? PropertyManager { get; set; }
}
