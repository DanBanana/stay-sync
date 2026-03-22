namespace StaySync.Application.Features.Properties.DTOs;

public record PropertyDto(Guid Id, string Name, string? Address, Guid PropertyManagerId, DateTimeOffset CreatedAt);
