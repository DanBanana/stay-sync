namespace StaySync.Application.Features.Invites.DTOs;

public record CreateInviteResult(string Token, DateTime ExpiresAt);
