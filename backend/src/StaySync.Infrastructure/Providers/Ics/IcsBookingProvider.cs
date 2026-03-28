using Ical.Net;
using Ical.Net.CalendarComponents;
using StaySync.Application.Common.Exceptions;
using StaySync.Domain.Entities;
using StaySync.Domain.Enums;
using StaySync.Domain.Interfaces;

namespace StaySync.Infrastructure.Providers.Ics;

public class IcsBookingProvider(HttpClient httpClient) : IBookingProvider
{
    public async Task<IEnumerable<Booking>> FetchBookingsAsync(
        ExternalCalendar calendar,
        CancellationToken cancellationToken = default)
    {
        string icsContent;
        try
        {
            icsContent = await httpClient.GetStringAsync(calendar.IcsUrl, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new BadRequestException($"Unable to fetch ICS feed: {ex.Message}");
        }

        CalendarCollection calendars;
        try
        {
            calendars = CalendarCollection.Load(icsContent);
        }
        catch (Exception ex)
        {
            throw new BadRequestException($"ICS feed returned invalid data: {ex.Message}");
        }

        var bookings = new List<Booking>();
        var propertyManagerId = calendar.Room.Property.PropertyManagerId;

        foreach (var cal in calendars)
        {
            foreach (var evt in cal.Events.OfType<CalendarEvent>())
            {
                if (evt.DtStart is null || evt.DtEnd is null || evt.Uid is null)
                    continue;

                var checkIn = DateOnly.FromDateTime(evt.DtStart.Value);
                var checkOut = DateOnly.FromDateTime(evt.DtEnd.Value);

                if (checkOut <= checkIn)
                    continue;

                var status = string.Equals(evt.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase)
                    ? BookingStatus.Cancelled
                    : BookingStatus.Confirmed;

                bookings.Add(new Booking
                {
                    ExternalCalendarId = calendar.Id,
                    RoomId = calendar.RoomId,
                    PropertyManagerId = propertyManagerId,
                    ExternalUid = evt.Uid,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    Status = status,
                    RawSummary = evt.Summary,
                    GuestName = null,
                });
            }
        }

        return bookings;
    }
}
