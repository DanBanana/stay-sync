using StaySync.Domain.Entities;
using StaySync.Domain.Interfaces;

namespace StaySync.Infrastructure.Providers.Ics;

// Stub — full ICS parsing implemented in Milestone 7
public class IcsBookingProvider : IBookingProvider
{
    public Task<IEnumerable<Booking>> FetchBookingsAsync(ExternalCalendar calendar, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("ICS provider will be implemented in Milestone 7.");
    }
}
