namespace StaySync.Application.Features.Bookings.DTOs;

public record CalendarBookingDto(
    Guid Id,
    Guid RoomId,
    string RoomName,
    string Platform,
    DateOnly CheckIn,
    DateOnly CheckOut,
    string Status);
