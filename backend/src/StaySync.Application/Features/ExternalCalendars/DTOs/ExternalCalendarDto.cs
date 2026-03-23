namespace StaySync.Application.Features.ExternalCalendars.DTOs;

public record ExternalCalendarDto(
    Guid Id,
    Guid RoomId,
    string Platform,
    string IcsUrl,
    DateTimeOffset? LastSyncedAt,
    DateTimeOffset CreatedAt);
