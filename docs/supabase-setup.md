# Supabase Setup Guide

StaySync uses Supabase as a managed PostgreSQL host. The .NET API handles all auth (JWT), RBAC, and tenant isolation — Supabase Auth is not used.

---

## Architecture

```
Angular (Vercel)
  → POST /api/auth/login  → .NET API issues custom JWT
  → All requests          → .NET API validates JWT, enforces RBAC
  → .NET API              → Supabase PostgreSQL (Npgsql / EF Core)
```

Supabase is purely a Postgres host. No Supabase Auth, no RLS policies needed — tenant isolation is enforced in the .NET Application layer via `ICurrentUserService`.

---

## 1. Create Supabase Project

1. Go to [supabase.com](https://supabase.com) → New Project
2. Choose a region close to your .NET API host
3. Save the **database password** securely

---

## 2. Get Connection String

Supabase dashboard → **Settings → Database → Connection string**

Select **Session mode** (port **5432**, not 6543 — EF Core migrations require session mode).

```
Host=db.<project-ref>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<db-password>;SSL Mode=Require;Trust Server Certificate=true
```

---

## 3. Configure Backend

### Local development (`appsettings.Development.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.<ref>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<pw>;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

### Production (environment variables on your host)

Set these env vars — .NET maps them to `appsettings.json` automatically:

| Env Var | Value |
|---|---|
| `ConnectionStrings__DefaultConnection` | Supabase connection string (session mode) |
| `JWT__Secret` | Long random string (32+ chars) |
| `JWT__Issuer` | `staysync-api` |
| `JWT__Audience` | `staysync-client` |
| `Cors__AllowedOrigins__0` | Your Vercel frontend URL |

---

## 4. Apply Migrations

EF Core migrations run automatically on startup in Development. For production, run manually:

```bash
cd backend
dotnet ef database update --project src/StaySync.Infrastructure --startup-project src/StaySync.API
```

This creates all 6 tables: `users`, `property_managers`, `properties`, `rooms`, `external_calendars`, `bookings`.

---

## 5. Seed Dev Data (local only)

The `SeedDevDataAsync` function in `Program.cs` runs only in Development mode and creates:

| Email | Password | Role |
|---|---|---|
| admin@staysync.dev | Admin1234! | SuperAdmin |
| manager@staysync.dev | Manager1234! | PropertyManager |
| manager2@staysync.dev | Manager1234! | PropertyManager |

---

## 6. Frontend Config

Update [frontend/src/environments/environment.prod.ts](../frontend/src/environments/environment.prod.ts):

```ts
export const environment = {
  production: true,
  apiBaseUrl: 'https://your-api-domain.com'
};
```

No Supabase JS SDK needed — the Angular app communicates only with the .NET API.

---

## Connection Modes

| Mode | Port | Use |
|---|---|---|
| Session | 5432 | .NET API (EF Core, migrations) ✓ |
| Transaction (PgBouncer) | 6543 | Serverless functions (not needed here) |

Always use **Session mode (5432)** for the .NET API.

---

## Why Not Supabase Auth?

The .NET API already issues custom JWTs with:
- `role` claim (SuperAdmin / PropertyManager)
- `property_manager_id` claim (tenant isolation)

Migrating to Supabase Auth would require:
- Replacing `TokenService` and user/password management
- Custom JWT claims via Supabase Edge Functions or user metadata
- Updating Angular to use `@supabase/supabase-js`

This adds significant complexity with no user-facing benefit. The current RBAC system is robust and test-covered. Use Supabase as a Postgres host only.
