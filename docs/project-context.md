# StaySync — Project Context

## Problem
Hospitality operators list rooms on multiple OTAs (Airbnb, Booking.com, Expedia). Each OTA manages bookings independently, causing double bookings, manual error, and poor visibility.

## Solution
Booking aggregation and conflict detection system. Collects ICS calendar feeds from OTAs, normalizes into a unified model, displays on a single calendar dashboard with conflict highlighting.

## Product Goal
Simple, reliable system for hospitality operators to manage availability across platforms. Priorities: simplicity, clarity, reliability, maintainability.

## Constraints
- Solo developer, minimal budget
- Must be easy to maintain and modify
- Avoid unnecessary complexity
- Designed for future expansion

---

## Technology Stack

| Layer | Technology |
|---|---|
| Frontend | Angular + RxJS + NgRx |
| Backend | .NET Web API (Clean Architecture) |
| Database | PostgreSQL |
| Auth | JWT + RBAC |
| Frontend Hosting | Vercel |
| Backend + DB Hosting | Railway |

---

## Roles and Permissions

| Role | Access |
|---|---|
| SuperAdmin | All properties, all tenants, system-wide data, manage accounts |
| PropertyManager | Own properties, rooms, calendars, bookings only |

Multi-tenant architecture: all domain data scoped by `PropertyManagerId`.

---

## System Overview

```
OTA Listing (Airbnb/Booking.com/Expedia)
  → ICS Feed URL (manually entered by PropertyManager)
  → Background Sync Service (periodic download + parse)
  → Normalized Booking records (PostgreSQL)
  → Calendar Dashboard (Angular)
  → Conflict Detection (overlap highlighting)
```

---

## Core Features

1. ICS feed registration per room/listing
2. Periodic sync: download + parse ICS, upsert bookings
3. Calendar dashboard: visualize bookings by room
4. Conflict detection: highlight overlapping bookings across sources
5. Manual booking management (CRUD)
6. Authentication and role-based access

---

## Data Model

```
PropertyManager (1) → (many) Property
Property (1) → (many) Room
Room (1) → (many) ExternalCalendar
ExternalCalendar (1) → (many) Booking
```

### Key Entities

**User** — email, password_hash, role (SuperAdmin | PropertyManager)
**PropertyManager** — user_id (1:1), display_name
**Property** — property_manager_id, name, address
**Room** — property_id, name
**ExternalCalendar** — room_id, platform, ics_url, last_synced_at
**Booking** — external_calendar_id, property_manager_id (denormalized), room_id (denormalized), external_uid, check_in, check_out, status

**Deduplication key**: `UNIQUE(external_calendar_id, external_uid)`
**Tenant filter column**: `property_manager_id` on Booking (denormalized for query efficiency)

---

## OTA Integration Strategy

- **Phase 1**: ICS calendar feeds only (no official OTA APIs)
- PropertyManager manually pastes ICS URL per listing
- Background service periodically fetches + parses ICS
- Provider pattern: `IBookingProvider` / `IcsBookingProvider`
- Future: swap/add providers for official OTA APIs without changing domain logic

---

## Backend Architecture (Clean Architecture)

```
StaySync.Domain        ← no dependencies
StaySync.Application   ← depends on Domain
StaySync.Infrastructure ← depends on Application + Domain
StaySync.API           ← depends on Infrastructure + Application
```

**Domain**: Entities, ValueObjects (DateRange with Overlaps()), Enums, IBookingProvider interface
**Application**: MediatR CQRS (Commands/Queries), FluentValidation, IApplicationDbContext, ICurrentUserService, ITokenService
**Infrastructure**: AppDbContext (EF Core + Npgsql), IcsBookingProvider (Ical.Net), TokenService, CurrentUserService
**API**: Controllers, ExceptionHandlingMiddleware, Program.cs

### Tenant Scope Enforcement
- Enforced in Application handlers (not just controllers)
- `ICurrentUserService` exposes `PropertyManagerId` extracted from JWT claims
- SuperAdmin handlers skip scope filtering

### Conflict Detection
- `DateRange.Overlaps(DateRange other)` — domain value object
- No DB or service dependency — fully unit testable

---

## Frontend Architecture (Angular)

```
CoreModule     — auth services, interceptors, guards (imported once in AppModule)
SharedModule   — reusable dumb components, pipes (imported in feature modules)
FeatureModules — lazy-loaded: auth, dashboard, properties, rooms, calendars, bookings
Store (NgRx)   — auth slice (global), feature slices per domain area
```

**Auth flow**: Login form → NgRx action → Effect calls API → token stored in state → persisted to localStorage via meta-reducer → interceptor attaches to all requests → 401 triggers logout action

---

## Authentication

- JWT tokens, 24h expiry (Phase 1)
- Claims: `sub` (userId), `email`, `role`, `property_manager_id`
- Login endpoint: `POST /api/auth/login` → `{ token, expiresAt, role }`
- Angular: `AuthInterceptor` reads token from NgRx store, attaches `Authorization: Bearer`

---

## Deployment

| Service | Platform | Notes |
|---|---|---|
| Angular SPA | Vercel | `ng build --configuration=production` |
| .NET API | Railway | Docker or buildpack |
| PostgreSQL | Railway | Migrations applied at API startup |

Environment config via Railway env vars → mapped to `appsettings.json` via .NET convention (`JWT__Secret` → `Jwt:Secret`). Angular uses `environment.prod.ts` with `apiBaseUrl` set to Railway API URL.

---

## Development Milestones

| # | Milestone | Scope |
|---|---|---|
| 1 | Project Foundation | Repo structure, clean arch setup, Angular scaffold, DB schema, JWT auth |
| 2 | Auth + RBAC | Login endpoint, JWT, role guards, user management |
| 3 | Core Domain | Property, Room, ExternalCalendar, Booking CRUD with tenant isolation |
| 4 | Calendar Dashboard | Angular calendar UI, booking visualization per room |
| 5 | Manual Bookings | Create/edit/delete bookings from UI |
| 6 | Conflict Detection | Overlap detection, dashboard highlighting |
| 7 | ICS Integration | ICS download, parse, normalize, upsert |
| 8 | Background Sync | Scheduled ICS sync service |
| 9 | Hardening | Logging, validation, error handling, production readiness |

See `docs/milestone-1.md` for the detailed Milestone 1 technical breakdown.

---

## Coding Principles

- Readability first — code is written once, read many times
- Clean Architecture boundaries — no layer skipping
- SOLID principles throughout
- Simple, maintainable solutions over clever ones
- Minimal complexity for current milestone scope
- Industry-standard practices (no reinventing the wheel)
