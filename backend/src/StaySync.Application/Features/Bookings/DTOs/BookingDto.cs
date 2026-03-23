namespace StaySync.Application.Features.Bookings.DTOs;

public record BookingDto(
    Guid Id,
    Guid RoomId,
    Guid ExternalCalendarId,
    string ExternalUid,
    string? GuestName,
    DateOnly CheckIn,
    DateOnly CheckOut,
    string Status,
    string? RawSummary);
