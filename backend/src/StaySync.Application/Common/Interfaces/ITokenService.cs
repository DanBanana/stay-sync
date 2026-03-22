using StaySync.Domain.Entities;

namespace StaySync.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user, PropertyManager? propertyManager);
}
