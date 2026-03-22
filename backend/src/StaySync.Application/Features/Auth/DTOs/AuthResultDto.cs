namespace StaySync.Application.Features.Auth.DTOs;

public record AuthResultDto(string Token, DateTimeOffset ExpiresAt, string Role);
