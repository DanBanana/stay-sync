using StaySync.Domain.Entities;

namespace StaySync.Domain.Interfaces;

public interface IBookingProvider
{
    Task<IEnumerable<Booking>> FetchBookingsAsync(ExternalCalendar calendar, CancellationToken cancellationToken = default);
}
