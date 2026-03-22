using StaySync.Domain.Common;
using StaySync.Domain.Enums;

namespace StaySync.Domain.Entities;

public class Booking : Entity
{
    public Guid ExternalCalendarId { get; set; }
    public Guid PropertyManagerId { get; set; }
    public Guid RoomId { get; set; }
    public string ExternalUid { get; set; } = string.Empty;
    public string? GuestName { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public string? RawSummary { get; set; }

    public ExternalCalendar ExternalCalendar { get; set; } = null!;
}
