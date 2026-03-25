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
- Environment files: dev → `:5081`, prod → Railway (update URL when deployed)
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

## Milestone Roadmap

| # | Milestone | Status |
|---|---|---|
| 1 | Project Foundation | **Done** |
| 2 | Auth + RBAC | **Done** (included in M1) |
| 3 | Core Domain — Property/Room/Calendar/Booking CRUD + UI | **Done** |
| 4 | Calendar Dashboard | **Done** |
| 5 | Manual Bookings | **Done** |
| 6 | Conflict Detection — includes overlap detection, conflict highlighting, and visual lane-assignment rendering (stacking overlapping booking bars into sub-rows) | Pending |
| 7 | ICS Integration | Pending |
| 8 | Background Sync | Pending |
| 9 | Hardening | Pending |

---

## Frontend Development Standards

These apply to **every milestone** from M3 onward.

- All Angular forms must use Reactive Forms (FormBuilder / FormGroup / formControlName). Template-driven forms (ngModel) are not allowed.
- UI form validation must go through reactive form validators.
- Date form fields must bind to `Date` objects. Convert to/from `YYYY-MM-DD` strings only at the API boundary (on submit / on load via `toDate()` / `toDateString()` helpers).

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
