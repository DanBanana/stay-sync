# StaySync Data Model

## The Core Idea

A property manager lists their rooms on multiple booking sites (Airbnb, Booking.com, etc.). Each site has its own calendar. StaySync pulls those calendars together into one place so the manager can see all bookings across all rooms and sites.

---

## Entity Hierarchy

```
User (credentials + role)
  └── PropertyManager (business identity)
        └── Property (many)
              └── Room (many)
                    └── ExternalCalendar (many)
                          └── Booking (many)
```

### User
Login credentials (email + password hash) and role (`SuperAdmin` or `PropertyManager`).
Purely for authentication — holds no business data.

### PropertyManager
The business profile linked to a User.
Holds a display name and owns all Properties.
`SuperAdmin` users have no `PropertyManager` record — they don't own properties, they manage the platform.

### Property
A physical property (hotel, apartment building, etc.) owned by a PropertyManager.

### Room
A bookable unit within a property (e.g. "Room 101", "Garden Suite").
A room is just a named container — it doesn't hold booking data directly.

### ExternalCalendar
A connection to a single OTA listing for a room.
Holds the **ICS feed URL** and the platform name (e.g. Airbnb, Booking.com).
One room can have multiple ExternalCalendars — one per booking site it's listed on.

### Booking
A single reservation, parsed from an ExternalCalendar's ICS feed.
Holds guest name, check-in/check-out dates, and status.
When a calendar is synced, its ICS file is parsed and Booking records are created or updated.

---

## How Bookings Get Created

1. Manager adds an ExternalCalendar for a room (paste ICS URL from Airbnb/Booking.com)
2. Background sync fetches the ICS feed
3. ICS events are parsed into Booking records linked to that ExternalCalendar
4. The calendar dashboard displays all Bookings per Room, across all sources

---

## What Gets Shown on the Calendar (M4+)

The calendar UI renders **Bookings** on a timeline — not ExternalCalendars.
Bookings can be color-coded by ExternalCalendar/platform to show which site each reservation came from.

---

## Key Design Decisions

- `Booking.property_manager_id` is **denormalized** (copied from the room's property) for efficient tenant-scoped queries — avoids joining up the full chain on every query.
- Conflict detection works at the **Room** level: two bookings on the same room with overlapping dates = conflict.
- The `DateRange.Overlaps()` domain method handles conflict logic — no DB dependency.
