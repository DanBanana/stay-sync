using System.Reflection;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Interfaces;
using StaySync.Domain.Entities;

namespace StaySync.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<PropertyManager> PropertyManagers => Set<PropertyManager>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<ExternalCalendar> ExternalCalendars => Set<ExternalCalendar>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
