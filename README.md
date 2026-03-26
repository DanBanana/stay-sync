# StaySync

Booking aggregation and conflict detection for hospitality operators.

Collects ICS calendar feeds from OTA platforms (Airbnb, Booking.com, Expedia), normalizes bookings into a unified model, and displays them on a single calendar dashboard with conflict highlighting.

## Documentation

- [docs/project-context.md](docs/project-context.md) — Architecture overview, data model, tech stack, milestones

## Stack

- **Frontend**: Angular + RxJS + NgRx → Vercel
- **Backend**: .NET Web API (Clean Architecture) → VPS / serverless
- **Database**: PostgreSQL → Supabase
- **Auth**: JWT + RBAC (SuperAdmin / PropertyManager), issued by .NET API; Supabase used as DB host only
