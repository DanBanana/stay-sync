using StaySync.Domain.Enums;

namespace StaySync.Application.Features.ExternalCalendars.DTOs;

public record ExternalCalendarDto(
    Guid Id,
    Guid RoomId,
    string Platform,
    string IcsUrl,
    DateTimeOffset? LastSyncedAt,
    SyncStatus? LastSyncStatus,
    string? LastSyncErrorMessage,
    DateTimeOffset CreatedAt);
