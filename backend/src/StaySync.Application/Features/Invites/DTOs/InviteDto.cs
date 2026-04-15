namespace StaySync.Application.Features.Invites.DTOs;

public record InviteDto(
    Guid Id,
    string Email,
    string Role,
    DateTime ExpiresAt,
    DateTime? UsedAt,
    bool IsPending);
