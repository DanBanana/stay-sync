# Milestone 1 — Project Foundation

## Goal
Running backend with auth scaffolding, property + room CRUD, and Angular login flow wired to the API. Deployed skeletons on Railway and Vercel.

---

## Repository Structure

```
stay-sync/
├── .gitignore
├── README.md
├── docs/
│   ├── project-context.md
│   └── milestone-1.md
├── backend/
│   ├── StaySync.sln
│   ├── src/
│   │   ├── StaySync.Domain/
│   │   ├── StaySync.Application/
│   │   ├── StaySync.Infrastructure/
│   │   └── StaySync.API/
│   └── tests/
│       ├── StaySync.Domain.Tests/
│       ├── StaySync.Application.Tests/
│       └── StaySync.Infrastructure.Tests/
└── frontend/
    └── (Angular workspace — ng new stay-sync-frontend)
```

---

## Backend — Clean Architecture Setup

### Project References (strict inward only)

```
StaySync.API            → StaySync.Infrastructure
StaySync.API            → StaySync.Application
StaySync.Infrastructure → StaySync.Application
StaySync.Infrastructure → StaySync.Domain
StaySync.Application    → StaySync.Domain
```

### NuGet Packages by Project

**StaySync.Domain** — none

**StaySync.Application**:
- `MediatR`
- `FluentValidation`

**StaySync.Infrastructure**:
- `Microsoft.EntityFrameworkCore`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `System.IdentityModel.Tokens.Jwt`
- `BCrypt.Net-Next`
- `Ical.Net` (stub now, full use in Milestone 7)

**StaySync.API**:
- `MediatR.Extensions.Microsoft.DependencyInjection`
- `FluentValidation.AspNetCore`
- `Swashbuckle.AspNetCore`

---

## Backend Project Structure

### StaySync.Domain

```
StaySync.Domain/
├── Common/
│   └── Entity.cs                  // base: Guid Id, DateTimeOffset CreatedAt/UpdatedAt
├── Entities/
│   ├── User.cs
│   ├── PropertyManager.cs
│   ├── Property.cs
│   ├── Room.cs
│   ├── ExternalCalendar.cs
│   └── Booking.cs
├── Enums/
│   ├── UserRole.cs                // SuperAdmin, PropertyManager
│   └── BookingStatus.cs           // Confirmed, Cancelled, Blocked
├── ValueObjects/
│   └── DateRange.cs               // CheckIn, CheckOut + Overlaps(DateRange) method
├── Interfaces/
│   └── IBookingProvider.cs        // Task<IEnumerable<Booking>> FetchBookingsAsync(...)
└── Exceptions/
    ├── DomainException.cs
    └── ConflictDetectedException.cs
```

Key: `DateRange.Overlaps()` is the sole home of conflict detection logic — no DB, no service, fully unit-testable.

### StaySync.Application

```
StaySync.Application/
├── Common/
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs    // DbSet<T> properties + SaveChangesAsync
│   │   ├── ITokenService.cs            // string GenerateToken(User, PropertyManager?)
│   │   └── ICurrentUserService.cs      // UserId, Role, PropertyManagerId, IsAuthenticated
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs       // MediatR pipeline: runs FluentValidation
│   │   └── LoggingBehavior.cs
│   └── Exceptions/
│       ├── NotFoundException.cs
│       ├── ForbiddenException.cs
│       └── ValidationException.cs
├── Features/
│   ├── Auth/
│   │   ├── Commands/
│   │   │   ├── LoginCommand.cs
│   │   │   └── LoginCommandHandler.cs
│   │   └── DTOs/
│   │       └── AuthResultDto.cs        // { Token, ExpiresAt, Role }
│   ├── Properties/
│   │   ├── Commands/
│   │   │   ├── CreatePropertyCommand.cs + Handler
│   │   │   ├── UpdatePropertyCommand.cs + Handler
│   │   │   └── DeletePropertyCommand.cs + Handler
│   │   ├── Queries/
│   │   │   ├── GetPropertiesQuery.cs + Handler
│   │   │   └── GetPropertyByIdQuery.cs + Handler
│   │   └── DTOs/PropertyDto.cs
│   └── Rooms/
│       ├── Commands/ (Create, Update, Delete)
│       ├── Queries/ (GetByProperty)
│       └── DTOs/RoomDto.cs
└── DependencyInjection.cs              // AddApplication(): MediatR + FluentValidation
```

### StaySync.Infrastructure

```
StaySync.Infrastructure/
├── Persistence/
│   ├── AppDbContext.cs                      // implements IApplicationDbContext
│   ├── DesignTimeDbContextFactory.cs        // enables EF CLI migrations
│   ├── Configurations/
│   │   ├── UserConfiguration.cs
│   │   ├── PropertyManagerConfiguration.cs
│   │   ├── PropertyConfiguration.cs
│   │   ├── RoomConfiguration.cs
│   │   ├── ExternalCalendarConfiguration.cs
│   │   └── BookingConfiguration.cs
│   └── Migrations/                          // EF-generated, committed to source control
├── Identity/
│   ├── TokenService.cs                      // implements ITokenService
│   ├── PasswordHasher.cs                    // BCrypt wrapper
│   └── CurrentUserService.cs               // implements ICurrentUserService via IHttpContextAccessor
├── Providers/
│   └── Ics/
│       └── IcsBookingProvider.cs            // stub — implements IBookingProvider (Milestone 7)
└── DependencyInjection.cs                   // AddInfrastructure(): DbContext, Identity, JWT
```

### StaySync.API

```
StaySync.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── PropertiesController.cs
│   └── RoomsController.cs
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

`Program.cs` call order: `AddApplication()` → `AddInfrastructure(config)` → `AddControllers()` → `AddSwaggerGen()` → `UseExceptionHandling()` → `UseAuthentication()` → `UseAuthorization()` → `MapControllers()`

---

## PostgreSQL Schema

### users
| Column | Type | Constraints |
|---|---|---|
| id | UUID | PK, DEFAULT gen_random_uuid() |
| email | VARCHAR(255) | NOT NULL, UNIQUE |
| password_hash | TEXT | NOT NULL |
| role | VARCHAR(50) | NOT NULL, CHECK IN ('SuperAdmin','PropertyManager') |
| created_at | TIMESTAMPTZ | NOT NULL, DEFAULT now() |
| updated_at | TIMESTAMPTZ | NOT NULL, DEFAULT now() |

### property_managers
| Column | Type | Constraints |
|---|---|---|
| id | UUID | PK |
| user_id | UUID | NOT NULL, FK → users(id), UNIQUE |
| display_name | VARCHAR(255) | NOT NULL |
| created_at | TIMESTAMPTZ | NOT NULL |
| updated_at | TIMESTAMPTZ | NOT NULL |

### properties
| Column | Type | Constraints |
|---|---|---|
| id | UUID | PK |
| property_manager_id | UUID | NOT NULL, FK → property_managers(id) |
| name | VARCHAR(255) | NOT NULL |
| address | TEXT | |
| created_at | TIMESTAMPTZ | NOT NULL |
| updated_at | TIMESTAMPTZ | NOT NULL |

Index: `(property_manager_id)`

### rooms
| Column | Type | Constraints |
|---|---|---|
| id | UUID | PK |
| property_id | UUID | NOT NULL, FK → properties(id) |
| name | VARCHAR(255) | NOT NULL |
| created_at | TIMESTAMPTZ | NOT NULL |
| updated_at | TIMESTAMPTZ | NOT NULL |

Index: `(property_id)`

### external_calendars
| Column | Type | Constraints |
|---|---|---|
| id | UUID | PK |
| room_id | UUID | NOT NULL, FK → rooms(id) |
| platform | VARCHAR(100) | NOT NULL |
| ics_url | TEXT | NOT NULL |
| last_synced_at | TIMESTAMPTZ | nullable |
| created_at | TIMESTAMPTZ | NOT NULL |
| updated_at | TIMESTAMPTZ | NOT NULL |

Index: `(room_id)`

### bookings
| Column | Type | Constraints |
|---|---|---|
| id | UUID | PK |
| external_calendar_id | UUID | NOT NULL, FK → external_calendars(id) |
| property_manager_id | UUID | NOT NULL, FK → property_managers(id) — denormalized |
| room_id | UUID | NOT NULL, FK → rooms(id) — denormalized |
| external_uid | VARCHAR(500) | NOT NULL |
| guest_name | VARCHAR(255) | |
| check_in | DATE | NOT NULL |
| check_out | DATE | NOT NULL |
| status | VARCHAR(50) | NOT NULL DEFAULT 'confirmed' |
| raw_summary | TEXT | |
| created_at | TIMESTAMPTZ | NOT NULL |
| updated_at | TIMESTAMPTZ | NOT NULL |

Constraints:
- `UNIQUE(external_calendar_id, external_uid)` — prevents duplicate re-sync imports
- `CHECK(check_out > check_in)`

Indexes:
- `(room_id, check_in, check_out)` — overlap/conflict queries
- `(property_manager_id)` — tenant-scoped queries

**Why denormalize `property_manager_id` and `room_id` on bookings?** Avoids joining back through external_calendars → rooms → properties on every tenant filter or conflict query.

---

## Authentication Scaffolding

### Backend JWT Flow

1. `POST /api/auth/login` receives `{ email, password }`
2. `LoginCommandHandler` queries user by email, verifies BCrypt hash
3. Calls `ITokenService.GenerateToken(user, propertyManager?)` → signed JWT
4. JWT claims: `sub` (userId), `email`, `role`, `property_manager_id`
5. Returns `AuthResultDto { Token, ExpiresAt, Role }`

### ICurrentUserService Contract
```csharp
Guid UserId { get; }
string Role { get; }
Guid? PropertyManagerId { get; }   // null for SuperAdmin
bool IsAuthenticated { get; }
```
Implemented in Infrastructure via `IHttpContextAccessor` claims extraction.

### Tenant Scope Pattern (every handler)
```csharp
// Applied in all Application handlers returning PM-scoped data:
if (_currentUser.Role != "SuperAdmin")
    query = query.Where(x => x.PropertyManagerId == _currentUser.PropertyManagerId);
```
Tenant isolation is enforced at the handler level, not just the controller, so it cannot be bypassed by a misconfigured route.

### Exception → HTTP Mapping
| Application Exception | HTTP Status |
|---|---|
| NotFoundException | 404 Not Found |
| ForbiddenException | 403 Forbidden |
| ValidationException | 422 Unprocessable Entity |
| Unhandled Exception | 500 Internal Server Error |

---

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "Secret": "",
    "Issuer": "staysync-api",
    "Audience": "staysync-client",
    "ExpiryHours": 24
  },
  "Cors": {
    "AllowedOrigins": [ "http://localhost:4200" ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### Railway Environment Variables
```
DATABASE_URL=postgres://user:pass@host:5432/staysync
JWT__Secret=<256-bit minimum secret>
JWT__Issuer=staysync-api
JWT__Audience=staysync-client
JWT__ExpiryHours=24
ALLOWED_ORIGINS=https://stay-sync.vercel.app
ASPNETCORE_ENVIRONMENT=Production
```

.NET maps `JWT__Secret` → `Jwt:Secret` automatically via environment variable convention.
`DATABASE_URL` (Railway's Postgres URL format) must be mapped manually in `Program.cs` to `ConnectionStrings:DefaultConnection` (ADO.NET format).

---

## Angular Project Setup

```bash
# From repo root
ng new stay-sync-frontend --routing --style=scss
cd stay-sync-frontend
ng add @ngrx/store @ngrx/effects @ngrx/store-devtools @ngrx/entity
npm install ngrx-store-localstorage
```

### Folder Structure

```
src/app/
├── core/
│   ├── auth/
│   │   ├── auth.service.ts
│   │   ├── auth.interceptor.ts
│   │   ├── auth.guard.ts
│   │   └── role.guard.ts
│   ├── models/
│   │   ├── user.model.ts
│   │   ├── property.model.ts
│   │   ├── room.model.ts
│   │   └── booking.model.ts
│   └── core.module.ts
├── shared/
│   ├── components/
│   └── shared.module.ts
├── features/
│   ├── auth/
│   │   ├── login/
│   │   │   ├── login.component.ts
│   │   │   └── login.component.html
│   │   └── auth-routing.module.ts
│   ├── dashboard/
│   ├── properties/
│   └── rooms/
├── store/
│   ├── auth/
│   │   ├── auth.actions.ts
│   │   ├── auth.reducer.ts
│   │   ├── auth.effects.ts
│   │   └── auth.selectors.ts
│   └── app.state.ts
├── app-routing.module.ts
├── app.component.ts
└── app.module.ts
```

### NgRx Auth State Shape
```typescript
interface AuthState {
  token: string | null;
  user: {
    id: string;
    email: string;
    role: 'SuperAdmin' | 'PropertyManager';
    propertyManagerId: string | null;
  } | null;
  loading: boolean;
  error: string | null;
}
```

### Auth Actions
- `login({ email, password })` — triggered by login form submit
- `loginSuccess({ token, user })` — triggered by successful API response
- `loginFailure({ error })` — triggered by API error
- `logout` — triggered by interceptor on 401, or user action

### Auth Effect Responsibilities
- `login$`: calls `AuthService.login()`, maps to `loginSuccess` or `loginFailure`
- `loginSuccess$`: navigates to `/dashboard`
- `logout$`: clears token from localStorage, navigates to `/login`

### Token Persistence
Use `ngrx-store-localstorage` meta-reducer to sync the `auth` slice to `localStorage`. On app init, `AppModule` rehydrates the store from `localStorage` before the first route activation — so authenticated users stay logged in on page refresh.

### Environment Files
```typescript
// environment.ts
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5000'
};

// environment.prod.ts
export const environment = {
  production: true,
  apiBaseUrl: 'https://<railway-api-url>'
};
```

---

## EF Core Migration Strategy

- Run migrations from `backend/src/StaySync.Infrastructure/`
- `DesignTimeDbContextFactory` reads connection string from `appsettings.Development.json`
- Migration naming: `YYYYMMDD_DescriptiveName` (e.g., `20260322_InitialSchema`)
- Migrations are committed to source control
- In production: `app.Services.GetRequiredService<AppDbContext>().Database.MigrateAsync()` in `Program.cs` (wrapped in try/catch with logging)
- Never auto-generate migrations in CI

```bash
# From backend/src/StaySync.Infrastructure/
dotnet ef migrations add 20260322_InitialSchema --startup-project ../StaySync.API
dotnet ef database update --startup-project ../StaySync.API
```

---

## Implementation Phases

### Phase 1.1 — Backend Scaffolding
- [ ] Create `StaySync.sln` + 4 projects with correct project references
- [ ] Install NuGet packages per project
- [ ] Create `Entity.cs` base class
- [ ] Create domain entity stubs (properties matching schema, no nav props yet)
- [ ] Create `AppDbContext` implementing `IApplicationDbContext`
- [ ] Create `DesignTimeDbContextFactory`
- [ ] Create entity configurations (one class per entity)
- [ ] Create `DependencyInjection.cs` in Infrastructure and Application
- [ ] Wire `Program.cs` (AddApplication, AddInfrastructure, Swagger, CORS)
- [ ] Run `dotnet ef migrations add InitialSchema && dotnet ef database update`
- [ ] Verify all tables exist in local Postgres

### Phase 1.2 — JWT Auth Backend
- [ ] Define `ITokenService`, `ICurrentUserService` in Application
- [ ] Implement `TokenService`, `CurrentUserService`, `PasswordHasher` in Infrastructure
- [ ] Implement `LoginCommand` + `LoginCommandHandler`
- [ ] Wire `ValidationBehavior` and `LoggingBehavior` into MediatR pipeline
- [ ] Implement `ExceptionHandlingMiddleware`
- [ ] Add `AuthController` with `POST /api/auth/login`
- [ ] Seed dev users (one SuperAdmin, one PropertyManager) in `Program.cs` dev block
- [ ] Test via Swagger: login → get token → use token on protected endpoint

### Phase 1.3 — Properties + Rooms CRUD
- [ ] Implement Create/GetAll/GetById commands and queries for Properties
- [ ] Add `PropertiesController`
- [ ] Repeat for Rooms (scoped by `propertyId`)
- [ ] Add `RoomsController`
- [ ] Add FluentValidation validators for all commands
- [ ] Test tenant isolation: PM sees own data only, SuperAdmin sees all

### Phase 1.4 — Angular Scaffold + Auth
- [ ] `ng new stay-sync-frontend --routing --style=scss`
- [ ] Set up CoreModule, SharedModule, lazy feature module routing
- [ ] Install and configure NgRx (store, effects, devtools)
- [ ] Create auth NgRx slice (actions, reducer, effects, selectors)
- [ ] Implement `AuthService.login()`
- [ ] Implement `AuthInterceptor`
- [ ] Implement `AuthGuard` and `RoleGuard`
- [ ] Build `LoginComponent` with reactive form
- [ ] Configure environment files
- [ ] Test: login form → token in NgRx DevTools → localStorage persisted → redirect to /dashboard

### Phase 1.5 — Integration Smoke Test
- [ ] CORS: Angular dev server (4200) → local API (5000) works
- [ ] 401 response: interceptor dispatches logout, redirects to /login
- [ ] Cross-tenant request: returns 403
- [ ] Swagger works on Railway deployment
- [ ] `ng build --configuration=production` succeeds on Vercel

---

## Verification Checklist

- [ ] `dotnet build` succeeds across all 4 projects
- [ ] `dotnet ef database update` creates all 6 tables correctly
- [ ] `POST /api/auth/login` returns JWT for valid credentials
- [ ] Request to protected endpoint without token returns 401
- [ ] PropertyManager token: queries return only their own data
- [ ] SuperAdmin token: queries return data across all tenants
- [ ] `ng build --configuration=production` succeeds
- [ ] Login form submits, token visible in NgRx DevTools
- [ ] AuthGuard redirects unauthenticated user to `/login`
- [ ] AuthInterceptor attaches `Authorization: Bearer <token>` to API calls
