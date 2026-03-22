using Microsoft.EntityFrameworkCore;
using StaySync.Domain.Entities;

namespace StaySync.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<PropertyManager> PropertyManagers { get; }
    DbSet<Property> Properties { get; }
    DbSet<Room> Rooms { get; }
    DbSet<ExternalCalendar> ExternalCalendars { get; }
    DbSet<Booking> Bookings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
