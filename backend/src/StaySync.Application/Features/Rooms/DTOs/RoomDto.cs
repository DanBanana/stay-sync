namespace StaySync.Application.Features.Rooms.DTOs;

public record RoomDto(Guid Id, string Name, Guid PropertyId, DateTimeOffset CreatedAt);
