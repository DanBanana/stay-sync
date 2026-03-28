# Claude Instructions — StaySync

Read `docs/project-context.md` at the start of every session before writing any code or making suggestions. It contains the full architecture, data model, tech stack, roles, and milestone roadmap.

For Milestone 1 implementation detail, read `docs/milestone-1.md`.

---

## Current Status

### Milestone 1 — Project Foundation: COMPLETE

#### Backend (`backend/`)
- .NET 9 Clean Architecture: Domain → Application → Infrastructure → API
- Solution: `backend/StaySync.sln`
- PostgreSQL schema applied via EF Core migration (6 tables: users, property_managers, properties, rooms, external_calendars, bookings)
- JWT auth working: `POST /api/auth/login` → signed token with role + property_manager_id claims
- RBAC: SuperAdmin (all data) + PropertyManager (own data only)
- Tenant isolation enforced in all Application handlers via `ICurrentUserService`
- Endpoints: `/api/auth/login`, `/api/properties`, `/api/rooms`
- `ExceptionHandlingMiddleware`: 404 / 403 / 422 / 500
- API runs on: `http://localhost:5081`
- Swagger UI: `http://localhost:5081/swagger`

#### Frontend (`frontend/`)
- Angular 17, NgRx store, lazy-loaded feature modules
- Auth slice: actions, reducer, effects, selectors
- JWT persisted to `localStorage` via `ngrx-store-localstorage`
- `AuthInterceptor`: attaches Bearer token, dispatches logout on 401
- `AuthGuard` + `RoleGuard`
- Login page with reactive form, error display, loading state
- Dashboard placeholder (shows user + sign out)
- Environment files: dev → `:5081`, prod → set `apiBaseUrl` in `environment.prod.ts` to deployed .NET API URL
- CORS verified: Angular `:4200` ↔ API `:5081`
- `ng build --configuration=production` passes

---

## Running Locally

**Backend:**
```bash
cd backend
dotnet run --project src/StaySync.API
# API available at http://localhost:5081
```

**Frontend:**
```bash
cd frontend
ng serve
# App available at http://localhost:4200
```

**Dev accounts:**
| Email | Password | Role |
|---|---|---|
| admin@staysync.dev | Admin1234! | SuperAdmin |
| manager@staysync.dev | Manager1234! | PropertyManager |
| manager2@staysync.dev | Manager1234! | PropertyManager |

---

## Current Status

### Milestone 4 — Calendar Dashboard: COMPLETE

#### Backend additions
- `GET /api/bookings/calendar?propertyId=&from=&to=` — returns `CalendarBookingDto[]` with room name + platform
- `CalendarBookingDto`: Id, RoomId, RoomName, Platform, CheckIn, CheckOut, Status, GuestName
- `GetBookingsForCalendarQueryHandler`: date range overlap filter, tenant scope, joins Room + ExternalCalendar
- xUnit tests: 27 passing (3 new for calendar handler: in-range, boundary exclusion, forbidden)

#### Frontend additions
- NgRx slice: `calendarDashboard` (actions, reducer, selectors, effects)
- Smart 6-week rolling window cache: only fetches new date ranges on navigation
- `CalendarGanttComponent`: CSS Grid timeline (rooms × days), booking bars colored by platform
- `BookingDetailDialogComponent`: MatDialog popup on booking bar click
- Dashboard page fully replaces placeholder: property selector, week navigation, Gantt
- Calendar nav link added to sidenav (top of list)
- Jasmine tests: `calendar-dashboard.effects.spec.ts`, `calendar-gantt.component.spec.ts`
- `ng build --configuration=production` passes

---

### Milestone 5 — Manual Bookings: COMPLETE

#### Backend additions
- `POST /api/bookings` → 201 with booking ID
- `PUT /api/bookings/{id}` → 204 (manual-only enforced via BadRequestException)
- `DELETE /api/bookings/{id}` → 204 (manual-only enforced)
- Commands: `CreateManualBookingCommand`, `UpdateManualBookingCommand`, `DeleteManualBookingCommand`
- FluentValidation: `CreateManualBookingCommandValidator`, `UpdateManualBookingCommandValidator` (CheckOut > CheckIn)
- Auto-creates "Manual" platform `ExternalCalendar` per room on first manual booking
- `BadRequestException` wired into `ExceptionHandlingMiddleware` → 400
- xUnit tests: handler tests (create/update/delete) + validator tests (date range + RoomId)

#### Frontend additions
- `CreateEditBookingDialogComponent`: create + edit modes, Angular Material datepicker, cross-field date validation, guest name field
- `BookingDetailDialogComponent`: displays guest name; Edit + Delete actions restricted to manual bookings
- `ConfirmDialogComponent`: reusable delete confirmation dialog
- NgRx `calendarDashboard` slice extended: create/update/delete booking actions, effects, reducer, reload-after-mutation
- `BookingService`: `createManual()`, `updateManual()`, `deleteManual()`
- FAB button on dashboard for creating new bookings
- Date fields use `Date` objects in forms; `toDate()`/`toDateString()` helpers convert at API boundary only
- Jasmine tests: `create-edit-booking-dialog.component.spec.ts`, `booking-detail-dialog.component.spec.ts`, `confirm-dialog.component.spec.ts`, `calendar-dashboard.effects.spec.ts` (extended)
- `ng build --configuration=production` passes

---

### Milestone 6 — Conflict Detection: COMPLETE

#### Backend additions
- `ConflictDetectedException` wired into `ExceptionHandlingMiddleware` → 409 Conflict
- Conflict check in `CreateManualBookingCommandHandler`: blocks overlapping manual bookings pre-save
- Conflict check in `UpdateManualBookingCommandHandler`: blocks overlapping updates, self-excluded via `b.Id != booking.Id`
- Uses `DateRange.Overlaps()` (half-open interval: `CheckIn < other.CheckOut && other.CheckIn < CheckOut`)
- Adjacent bookings (checkOut == checkIn) do NOT conflict
- ICS-imported bookings are never blocked — conflict prevention applies to manual ops only
- xUnit tests: 4 new conflict tests (create overlap throws, create adjacent succeeds, update overlap throws, update self no-throw)

#### Frontend additions
- `BookingBar` interface: extends `CalendarBooking` with `lane`, `hasConflict`, `conflictsWith: ConflictInfo[]`
- `RoomRow` updated: adds `laneCount` and `conflictCount`
- `selectGroupedByRoom`: `assignLanes()` greedy algorithm stacks overlapping bookings into sub-rows; computes conflicts across all bookings regardless of platform
- `CalendarGanttComponent`: dynamic row height via `rowHeight()`, bar `top` computed from `booking.lane`
- Conflict indicators: red `box-shadow` outline, `⚠` badge top-right of bar, upgraded tooltip with full overlap details, room-level `⚠ N` conflict count badge
- Effects: `createBooking$` and `updateBooking$` read `err.error?.message` to surface 409 API message in snackbar
- Jasmine tests: selector spec (5 tests for lane/conflict logic), gantt spec extended (conflict CSS, badges, `rowHeight()`)
- `ng build --configuration=production` passes

---

### Milestone 7 — ICS Integration: COMPLETE

#### Backend additions
- `IcsBookingProvider` implemented using `Ical.Net` 5.x: downloads ICS via `HttpClient`, parses `VEvent` objects, maps to `Booking` domain entities
- Events without UID, DtStart, or DtEnd are silently skipped; `CANCELLED` status mapped to `BookingStatus.Cancelled`
- `SyncCalendarCommand(ExternalCalendarId)` → `SyncCalendarResult(Inserted, Updated)`
- `SyncCalendarCommandHandler`: upsert by `ExternalUid` (load existing → match by UID → update or insert), stamps `LastSyncedAt` on the calendar
- Manual calendars cannot be synced (guard throws `BadRequestException`)
- `POST /api/external-calendars/{id}/sync` → 200 with `{ inserted, updated }`
- DI: `services.AddHttpClient<IBookingProvider, IcsBookingProvider>()` (typed client pattern — single registration)
- xUnit tests: 7 new (insert, upsert, cancelled status, LastSyncedAt update, NotFound, Forbidden, Manual guard) — 57 total passing

#### Frontend additions
- `SyncCalendarResult` interface added to `external-calendar.model.ts`
- `ExternalCalendarService.sync(id)`: `POST /api/external-calendars/{id}/sync`
- NgRx `externalCalendars` slice extended: `syncCalendar` / `syncCalendarSuccess` / `syncCalendarFailure` actions; `syncingId: string | null` state tracks in-progress sync per row
- `selectSyncingId` selector
- `syncCalendar$` effect: calls service, dispatches success/failure
- `syncSuccess$` effect: shows snackbar (`"Sync complete — X new, Y updated"`), dispatches `setProperty` to refresh Gantt dashboard if a property is selected
- `ExternalCalendarsPageComponent`: sync column added with per-row spinner (shown while `syncingId === cal.id`), mobile columns updated
- Jasmine tests: effects spec extended (5 tests), component spec extended (3 new tests)
- `ng build --configuration=production` passes

---

## Milestone Roadmap

| # | Milestone | Status |
|---|---|---|
| 1 | Project Foundation | **Done** |
| 2 | Auth + RBAC | **Done** (included in M1) |
| 3 | Core Domain — Property/Room/Calendar/Booking CRUD + UI | **Done** |
| 4 | Calendar Dashboard | **Done** |
| 5 | Manual Bookings | **Done** |
| 6 | Conflict Detection | **Done** |
| 7 | ICS Integration | **Done** |
| 8 | Background Sync | Pending |
| 9 | Hardening | Pending |

---

## Frontend Development Standards (Angular + Material Aware)

These apply to **every milestone from M3 onward** and are **non-optional**.

### Core Principle

* **Component-first architecture**
* Angular Material = **UI components**
* You = **layout, spacing, structure, discipline**
* Follow `docs/frontend-guidelines.md` strictly

---

### Angular Component Rules

* One responsibility per component
* Keep components small, reusable, and focused
* Co-locate `.ts`, `.html`, `.scss`
* Avoid “god components”

---

### Forms

* Reactive Forms ONLY (no `ngModel`)
* Validation via Angular validators
* Date fields use `Date` objects internally
* Convert to string only at API boundary

---

### HTML & Semantics (Templates)

* Use semantic elements (`main`, `section`, etc.)
* Avoid unnecessary wrappers
* Maintain heading hierarchy (`h1 → h6`)
* Ensure accessibility (labels, alt text, ARIA when needed)
* No inline styles — SCSS only

---

### Angular Material Usage

* Use Material for UI components (buttons, dialogs, forms, cards)
* DO NOT:

  * Override internal DOM aggressively
  * Use `::ng-deep` unless absolutely necessary
* Prefer:

  * Wrapping components cleanly
  * Using Material theming
  * Extending, not fighting

---

### SCSS Architecture (Angular Scoped)

* Prefer **component-level SCSS**
* Global `styles.scss` only for:

  * variables (CSS custom properties)
  * base styles
* Use 4px/8px spacing scale (`--space-*`)
* Avoid deep nesting (max 2–3 levels)
* Keep specificity low
* Use semantic naming (BEM optional, not mandatory)

---

### Layout (Critical)

* Angular Material does NOT handle layout
* Always use:

  * Flexbox → 1D layouts
  * Grid → 2D layouts
* Use `gap` for spacing (no margin hacks)
* Avoid fixed widths

---

### Responsiveness

* Mobile-first approach
* Breakpoints:

  * `768px` (tablet)
  * `1024px` (desktop)
* Use fluid units (`rem`, `%`, `fr`)
* Ensure layouts adapt cleanly across all sizes

---

### Scroll Behavior (Strict Rule)

* **Avoid scrollbars whenever possible**
* Layouts must fit naturally within viewport
* Use flexible sizing and proper wrapping
* Only allow scroll when:

  * content is inherently long (tables, feeds)
* Avoid:

  * nested scroll containers
  * fixed-height overflow issues

---

### Typography & Spacing

* Use consistent type scale
* Limit fonts (1–2 max)
* Maintain readable line length
* Use spacing tokens only (no magic numbers)

---

### Performance & Maintainability

* Keep styles modular and scoped
* Avoid unused CSS
* Minimize overrides and hacks
* Keep code readable and predictable

---

### Enforcement Rule

Before generating or modifying frontend code, ALWAYS:

1. Reference `docs/frontend-guidelines.md`
2. Ensure compliance with:

   * Angular component architecture
   * Angular Material constraints
   * Layout + spacing standards
   * Scroll behavior rules

Non-compliant code must be refactored.

---

## Global Development Standards

These apply to **every milestone** from M3 onward. Do not skip them.

### Responsive UI
- All Angular components must be responsive: web, tablet, and mobile
- Use mobile-first CSS — style for small screens first, scale up with breakpoints
- Standard breakpoints: mobile < 768px, tablet 768–1024px, desktop > 1024px
- Use CSS Flexbox or Grid for layouts; avoid fixed pixel widths
- Test layouts at all three breakpoints before considering a component done

### Testing
- Every new backend Application handler gets a corresponding xUnit test
- Every new Angular component or service gets a corresponding Jasmine spec
- **Backend**: xUnit + Moq (mocking) + FluentAssertions (readable assertions)
- **Frontend**: Jasmine + Karma (Angular default, already installed)
- Tests must cover: happy path + at least one failure/edge case
- Keep tests simple and focused — not exhaustive coverage, but meaningful coverage
- Test files live alongside the code they test (same folder)
