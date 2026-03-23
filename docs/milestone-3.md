# Milestone 3 — Core Domain CRUD + UI

## Goal
A logged-in PropertyManager can manage their properties, rooms, and ICS calendar URLs through the Angular UI. All domain entities have full backend CRUD. Bookings are read-only (manual booking creation is Milestone 5; ICS parsing is Milestone 7).

---

## Scope

### In scope
- Backend: PUT + DELETE for Properties and Rooms (GET/POST already done in M1)
- Backend: Full CRUD for ExternalCalendars
- Backend: Read-only Booking endpoints (GET by property, GET by room, GET by id)
- Backend: xUnit + Moq + FluentAssertions test project for all new handlers
- Frontend: Angular Material UI with sidebar + content area layout
- Frontend: Properties list + create/edit (mat-dialog forms)
- Frontend: Rooms list per property + create/edit
- Frontend: ExternalCalendars list per room + add/delete
- Frontend: Bookings read-only list per room (empty state until ICS sync in M7)
- Frontend: Responsive UI — web, tablet, mobile
- Frontend: Jasmine tests for new components and NgRx effects
- NgRx slices for properties, rooms, external-calendars, bookings

### Out of scope
- Manual booking creation/editing (Milestone 5)
- ICS fetching or parsing (Milestone 7)
- Calendar dashboard visualization — FullCalendar, colored rooms, conflict view (Milestone 4)
- Conflict detection UI (Milestone 6)

---

## API Endpoints

### Already done (M1)
| Method | Route | Description |
|---|---|---|
| POST | /api/auth/login | Login |
| GET | /api/properties | List properties (tenant-scoped) |
| GET | /api/properties/{id} | Get property by ID |
| POST | /api/properties | Create property |
| GET | /api/properties/{propertyId}/rooms | List rooms by property |
| POST | /api/properties/{propertyId}/rooms | Create room |

### New in Milestone 3
| Method | Route | Description |
|---|---|---|
| PUT | /api/properties/{id} | Update property |
| DELETE | /api/properties/{id} | Delete property |
| GET | /api/rooms/{id} | Get room by ID |
| PUT | /api/rooms/{id} | Update room |
| DELETE | /api/rooms/{id} | Delete room |
| GET | /api/rooms/{roomId}/calendars | List calendars for a room |
| POST | /api/rooms/{roomId}/calendars | Add external calendar |
| GET | /api/calendars/{id} | Get calendar by ID |
| PUT | /api/calendars/{id} | Update calendar |
| DELETE | /api/calendars/{id} | Delete calendar |
| GET | /api/properties/{propertyId}/bookings | List bookings for a property |
| GET | /api/rooms/{roomId}/bookings | List bookings for a room |
| GET | /api/bookings/{id} | Get booking by ID |

---

## Request / Response Shapes

### Properties

**PUT /api/properties/{id}**
```json
// Request
{ "name": "string", "address": "string" }

// Response 200
{ "id": "guid", "name": "string", "address": "string", "createdAt": "datetime", "updatedAt": "datetime" }
```

**DELETE /api/properties/{id}** → 204 No Content

### Rooms

**GET /api/rooms/{id}**
```json
// Response 200
{ "id": "guid", "propertyId": "guid", "name": "string", "createdAt": "datetime", "updatedAt": "datetime" }
```

**PUT /api/rooms/{id}**
```json
// Request
{ "name": "string" }

// Response 200 — RoomDto
{ "id": "guid", "propertyId": "guid", "name": "string", "createdAt": "datetime", "updatedAt": "datetime" }
```

**DELETE /api/rooms/{id}** → 204 No Content

### ExternalCalendars

**GET /api/rooms/{roomId}/calendars**
```json
// Response 200
[{ "id": "guid", "roomId": "guid", "platform": "string", "icsUrl": "string", "lastSyncedAt": "datetime|null", "createdAt": "datetime" }]
```

**POST /api/rooms/{roomId}/calendars**
```json
// Request
{ "platform": "string", "icsUrl": "string" }

// Response 201 — ExternalCalendarDto
{ "id": "guid", "roomId": "guid", "platform": "string", "icsUrl": "string", "lastSyncedAt": null, "createdAt": "datetime" }
```

**PUT /api/calendars/{id}**
```json
// Request
{ "platform": "string", "icsUrl": "string" }

// Response 200 — ExternalCalendarDto
```

**DELETE /api/calendars/{id}** → 204 No Content

### Bookings (read-only)

**GET /api/properties/{propertyId}/bookings** and **GET /api/rooms/{roomId}/bookings**
```json
// Response 200
[{
  "id": "guid",
  "roomId": "guid",
  "externalCalendarId": "guid",
  "externalUid": "string",
  "guestName": "string|null",
  "checkIn": "date",
  "checkOut": "date",
  "status": "Confirmed|Cancelled|Blocked",
  "rawSummary": "string|null"
}]
```

---

## Backend — New Files

### Application Layer

```
Application/Features/
├── Properties/
│   ├── Commands/
│   │   ├── UpdatePropertyCommand.cs + Handler   ← NEW
│   │   └── DeletePropertyCommand.cs + Handler   ← NEW
│   └── Validators/
│       ├── CreatePropertyCommandValidator.cs     ← NEW (add if missing)
│       └── UpdatePropertyCommandValidator.cs     ← NEW
├── Rooms/
│   ├── Commands/
│   │   ├── UpdateRoomCommand.cs + Handler        ← NEW
│   │   └── DeleteRoomCommand.cs + Handler        ← NEW
│   ├── Queries/
│   │   └── GetRoomByIdQuery.cs + Handler         ← NEW
│   └── Validators/
│       └── UpdateRoomCommandValidator.cs         ← NEW
├── ExternalCalendars/
│   ├── Commands/
│   │   ├── CreateExternalCalendarCommand.cs + Handler   ← NEW
│   │   ├── UpdateExternalCalendarCommand.cs + Handler   ← NEW
│   │   └── DeleteExternalCalendarCommand.cs + Handler   ← NEW
│   ├── Queries/
│   │   ├── GetCalendarsByRoomQuery.cs + Handler         ← NEW
│   │   └── GetCalendarByIdQuery.cs + Handler            ← NEW
│   ├── DTOs/
│   │   └── ExternalCalendarDto.cs                       ← NEW
│   └── Validators/
│       ├── CreateExternalCalendarCommandValidator.cs    ← NEW
│       └── UpdateExternalCalendarCommandValidator.cs    ← NEW
└── Bookings/
    ├── Queries/
    │   ├── GetBookingsByPropertyQuery.cs + Handler      ← NEW
    │   ├── GetBookingsByRoomQuery.cs + Handler          ← NEW
    │   └── GetBookingByIdQuery.cs + Handler             ← NEW
    └── DTOs/
        └── BookingDto.cs                                ← NEW
```

### API Layer

```
API/Controllers/
├── PropertiesController.cs    ← ADD PUT + DELETE actions
├── RoomsController.cs         ← ADD GET by ID, PUT, DELETE actions
├── CalendarsController.cs     ← NEW (GET by room, POST, GET by id, PUT, DELETE)
└── BookingsController.cs      ← NEW (GET by property, GET by room, GET by id)
```

### Tenant isolation rules
- PropertyManager: Properties/Rooms/Calendars/Bookings scoped to own `PropertyManagerId`
- All handlers: verify ownership before update/delete — throw `ForbiddenException` if mismatch
- SuperAdmin: no scope filter

### Ownership check pattern (update/delete handlers)
```csharp
var entity = await _context.Properties
    .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
    ?? throw new NotFoundException(nameof(Property), request.Id);

if (_currentUser.Role != "SuperAdmin" && entity.PropertyManagerId != _currentUser.PropertyManagerId)
    throw new ForbiddenException();
```

---

## Frontend — Angular Material Setup

```bash
cd frontend
ng add @angular/material@17
# Choose: Indigo/Pink theme, Yes to typography, Yes to animations
```

Create `frontend/src/app/shared/material.module.ts` — re-exports all used Material modules:
- `MatToolbarModule`, `MatSidenavModule`, `MatListModule`
- `MatCardModule`, `MatTableModule`, `MatFormFieldModule`, `MatInputModule`
- `MatButtonModule`, `MatIconModule`, `MatDialogModule`
- `MatSnackBarModule`, `MatSelectModule`, `MatProgressSpinnerModule`
- `MatChipsModule`, `MatMenuModule`, `MatTabsModule`, `MatTooltipModule`

Import `MaterialModule` in `SharedModule`. Import `SharedModule` in every feature module.

---

## Frontend — App Shell Layout

**File:** `frontend/src/app/core/layout/layout.component`

All authenticated routes render inside `LayoutComponent`:

```
mat-toolbar         ← top bar: "StaySync" logo | user email | sign out button
  mat-sidenav-container
    mat-sidenav     ← left: property list (mat-nav-list) + "New Property" button
    mat-sidenav-content
      <router-outlet>  ← feature content renders here
```

**Responsive behavior:**
- Desktop (≥ 768px): `mode="side"`, sidenav always open
- Mobile (< 768px): `mode="over"`, hamburger toggle button in toolbar, sidenav closes after navigation

Use Angular CDK `BreakpointObserver` to detect breakpoint and toggle sidenav mode.

Dashboard route redirects to `/properties`.

---

## Frontend — Responsive Rules

| Breakpoint | Behavior |
|---|---|
| < 768px (mobile) | Sidenav collapses to hamburger, dialogs full-width, tables scroll horizontally, 1-col card grid |
| 768–1024px (tablet) | Sidenav optional, 2-col card grid |
| > 1024px (desktop) | Sidenav always open, 3-col card grid |

Full-width dialogs on mobile: `MatDialog.open(…, { panelClass: 'full-width-dialog' })`.

---

## Frontend — New Files

### Routing

```
/properties                     → content area: rooms for selected property (sidenav drives selection)
/properties/:id/calendars       → CalendarsListComponent for a room
/properties/:id/bookings        → BookingsListComponent (read-only)
```

Sidenav lists all properties. Selecting one loads its rooms in the content area. Rooms link to calendars. All routes lazy-loaded, behind `AuthGuard`.

### Feature Modules

```
src/app/features/
├── properties/
│   ├── properties-list/
│   │   ├── properties-list.component.ts
│   │   └── properties-list.component.html
│   ├── property-detail/
│   │   ├── property-detail.component.ts
│   │   └── property-detail.component.html
│   ├── property-form/
│   │   ├── property-form.component.ts         // create + edit (mode driven by route)
│   │   └── property-form.component.html
│   ├── properties-routing.module.ts
│   └── properties.module.ts
├── rooms/
│   ├── rooms-list/
│   ├── room-detail/
│   ├── room-form/
│   ├── rooms-routing.module.ts
│   └── rooms.module.ts
├── calendars/
│   ├── calendars-list/
│   ├── calendar-form/
│   ├── calendars-routing.module.ts
│   └── calendars.module.ts
└── bookings/
    ├── bookings-list/
    ├── bookings-routing.module.ts
    └── bookings.module.ts
```

### NgRx — New Slices

**Properties slice** (`store/properties/`):
```typescript
interface PropertiesState {
  properties: Property[];
  selectedProperty: Property | null;
  loading: boolean;
  error: string | null;
}
```
Actions: `loadProperties`, `loadPropertiesSuccess`, `loadPropertiesFailure`,
`loadProperty`, `loadPropertySuccess`, `loadPropertyFailure`,
`createProperty`, `createPropertySuccess`, `createPropertyFailure`,
`updateProperty`, `updatePropertySuccess`, `updatePropertyFailure`,
`deleteProperty`, `deletePropertySuccess`, `deletePropertyFailure`

**Rooms slice** (`store/rooms/`):
```typescript
interface RoomsState {
  rooms: Room[];
  selectedRoom: Room | null;
  loading: boolean;
  error: string | null;
}
```
Same action pattern as properties, scoped by `propertyId`.

**ExternalCalendars slice** (`store/calendars/`):
```typescript
interface CalendarsState {
  calendars: ExternalCalendar[];
  loading: boolean;
  error: string | null;
}
```
Actions: `loadCalendars`, `createCalendar`, `updateCalendar`, `deleteCalendar` + Success/Failure variants.

**Bookings slice** (`store/bookings/`):
```typescript
interface BookingsState {
  bookings: Booking[];
  loading: boolean;
  error: string | null;
}
```
Actions: `loadBookingsByRoom`, `loadBookingsByProperty` + Success/Failure variants (read-only).

### Core Models (update `core/models/`)

```typescript
// property.model.ts
export interface Property {
  id: string;
  name: string;
  address: string;
  createdAt: string;
  updatedAt: string;
}

// room.model.ts
export interface Room {
  id: string;
  propertyId: string;
  name: string;
  createdAt: string;
  updatedAt: string;
}

// external-calendar.model.ts
export interface ExternalCalendar {
  id: string;
  roomId: string;
  platform: string;
  icsUrl: string;
  lastSyncedAt: string | null;
  createdAt: string;
}

// booking.model.ts (already exists, verify fields match)
export interface Booking {
  id: string;
  roomId: string;
  externalCalendarId: string;
  externalUid: string;
  guestName: string | null;
  checkIn: string;   // 'YYYY-MM-DD'
  checkOut: string;  // 'YYYY-MM-DD'
  status: 'Confirmed' | 'Cancelled' | 'Blocked';
  rawSummary: string | null;
}
```

### Angular Services (add to `core/`)

```
core/
├── services/
│   ├── property.service.ts        // HTTP calls for /api/properties
│   ├── room.service.ts            // HTTP calls for /api/rooms + /api/properties/{id}/rooms
│   ├── calendar.service.ts        // HTTP calls for /api/calendars + /api/rooms/{id}/calendars
│   └── booking.service.ts         // HTTP calls for /api/bookings (read-only)
```

Register in `CoreModule` providers.

---

## App Routing Update

```typescript
// app-routing.module.ts additions
{
  path: 'properties',
  loadChildren: () => import('./features/properties/properties.module').then(m => m.PropertiesModule),
  canActivate: [AuthGuard]
},
{
  path: 'rooms',
  loadChildren: () => import('./features/rooms/rooms.module').then(m => m.RoomsModule),
  canActivate: [AuthGuard]
},
{
  path: 'calendars',
  loadChildren: () => import('./features/calendars/calendars.module').then(m => m.CalendarsModule),
  canActivate: [AuthGuard]
},
{
  path: 'bookings',
  loadChildren: () => import('./features/bookings/bookings.module').then(m => m.BookingsModule),
  canActivate: [AuthGuard]
}
```

Add NgRx feature slices to `AppModule`:
```typescript
StoreModule.forRoot({
  auth: authReducer,
  properties: propertiesReducer,
  rooms: roomsReducer,
  calendars: calendarsReducer,
  bookings: bookingsReducer
}, { metaReducers })
```

---

## Backend — Test Project

**Location:** `backend/tests/StaySync.Application.Tests/`

**Setup:**
```bash
cd backend
dotnet new xunit -n StaySync.Application.Tests -o tests/StaySync.Application.Tests
dotnet sln add tests/StaySync.Application.Tests/StaySync.Application.Tests.csproj
cd tests/StaySync.Application.Tests
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add reference ../../src/StaySync.Application/StaySync.Application.csproj
```

**Stack:** xUnit + Moq + FluentAssertions

**Coverage:** One test class per handler — happy path + at least one failure case (NotFoundException or ForbiddenException).

**Example pattern:**
```csharp
public class DeletePropertyCommandHandlerTests
{
    [Fact]
    public async Task Handle_ThrowsForbiddenException_WhenNotOwner()
    {
        // Arrange: mock context returns property with different PropertyManagerId
        // Act + Assert: await Assert.ThrowsAsync<ForbiddenException>(...)
    }
}
```

---

## Frontend — Tests

**Stack:** Jasmine + Karma (already installed with Angular)

One `.spec.ts` per new component:
- Component creates (`TestBed.createComponent`) without error
- Key interaction (button click dispatches NgRx action or opens dialog)

One `.spec.ts` per new NgRx effect:
- Success path: API returns data → success action dispatched
- Failure path: API throws → failure action dispatched

---

## Implementation Phases

### Phase 3.1 — Backend: Complete Properties + Rooms

- [ ] Add `UpdatePropertyCommand` + handler + validator
- [ ] Add `DeletePropertyCommand` + handler (ownership check)
- [ ] Add PUT + DELETE actions to `PropertiesController`
- [ ] Add `GetRoomByIdQuery` + handler
- [ ] Add `UpdateRoomCommand` + handler + validator
- [ ] Add `DeleteRoomCommand` + handler (ownership check)
- [ ] Add GET by ID, PUT, DELETE actions to `RoomsController`
- [ ] Test via Swagger: update property name, delete room, verify 403 on cross-tenant

### Phase 3.2 — Backend: ExternalCalendars CRUD

- [ ] Create `ExternalCalendarDto`
- [ ] Add `GetCalendarsByRoomQuery` + handler
- [ ] Add `GetCalendarByIdQuery` + handler
- [ ] Add `CreateExternalCalendarCommand` + handler + validator (validate ICS URL format)
- [ ] Add `UpdateExternalCalendarCommand` + handler + validator
- [ ] Add `DeleteExternalCalendarCommand` + handler
- [ ] Create `CalendarsController`
- [ ] Test via Swagger: add calendar to room, update URL, delete

### Phase 3.3 — Backend: Bookings Read Endpoints

- [ ] Create `BookingDto`
- [ ] Add `GetBookingsByPropertyQuery` + handler
- [ ] Add `GetBookingsByRoomQuery` + handler
- [ ] Add `GetBookingByIdQuery` + handler
- [ ] Create `BookingsController` (GET only for now)
- [ ] Test via Swagger: GET returns empty array (no bookings yet — that's correct)

### Phase 3.4 — Frontend: NgRx Slices + Services

- [ ] Create `property.service.ts`, `room.service.ts`, `calendar.service.ts`, `booking.service.ts`
- [ ] Create properties NgRx slice (actions, reducer, effects, selectors)
- [ ] Create rooms NgRx slice
- [ ] Create calendars NgRx slice
- [ ] Create bookings NgRx slice (read-only actions)
- [ ] Register all slices in `AppModule`
- [ ] Update `app.state.ts` with new slice types

### Phase 3.5 — Frontend: Properties UI

- [ ] Create `PropertiesModule` with routing
- [ ] Build `PropertiesListComponent` — loads from store, links to detail
- [ ] Build `PropertyDetailComponent` — shows property info + rooms list + links
- [ ] Build `PropertyFormComponent` — reactive form, create + edit mode
- [ ] Add properties routes to `app-routing.module.ts`
- [ ] Test: create property → appears in list → edit name → delete

### Phase 3.6 — Frontend: Rooms UI

- [ ] Create `RoomsModule` with routing
- [ ] Build `RoomsListComponent` — scoped to property, loaded from route param
- [ ] Build `RoomDetailComponent` — shows room info + calendars list
- [ ] Build `RoomFormComponent` — create + edit mode
- [ ] Test: add room to property → edit → delete

### Phase 3.7 — Frontend: Calendars UI

- [ ] Create `CalendarsModule` with routing
- [ ] Build `CalendarsListComponent` — scoped to room, shows platform + ICS URL
- [ ] Build `CalendarFormComponent` — platform name + ICS URL input, create + edit mode
- [ ] Test: add Airbnb calendar to room → edit URL → delete

### Phase 3.8 — Frontend: Bookings UI

- [ ] Create `BookingsModule` with routing
- [ ] Build `BookingsListComponent` — read-only table (guest name, check-in/out, status, platform)
- [ ] Test: list shows empty state correctly (no bookings until M7 sync)

### Phase 3.9 — Dashboard Navigation

- [ ] Update `DashboardComponent` — replace placeholder with nav links to Properties
- [ ] Verify full navigation flow: Login → Dashboard → Properties → Rooms → Calendars

---

## Verification Checklist

- [ ] PUT /api/properties/{id} updates name/address, returns 403 for wrong tenant
- [ ] DELETE /api/properties/{id} returns 204, returns 403 for wrong tenant
- [ ] GET /api/rooms/{id} returns room, 404 for unknown id
- [ ] PUT + DELETE /api/rooms/{id} work correctly with ownership checks
- [ ] GET /api/rooms/{roomId}/calendars returns empty array for new room
- [ ] POST /api/rooms/{roomId}/calendars creates calendar, returns 201
- [ ] PUT + DELETE /api/calendars/{id} work correctly
- [ ] GET /api/rooms/{roomId}/bookings returns empty array (no bookings yet — correct)
- [ ] All endpoints return 401 without token, 403 for cross-tenant access
- [ ] Angular: can create, edit, delete a property
- [ ] Angular: can add rooms to a property, edit, delete
- [ ] Angular: can add ICS calendar URL to a room, edit, delete
- [ ] Angular: bookings list shows empty state gracefully
- [ ] Navigation flows naturally from Dashboard → Properties → Rooms → Calendars
- [ ] `ng build --configuration=production` passes
