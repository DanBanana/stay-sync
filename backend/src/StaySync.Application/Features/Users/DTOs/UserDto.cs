namespace StaySync.Application.Features.Users.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string Role,
    bool IsActive,
    DateTimeOffset CreatedAt);
