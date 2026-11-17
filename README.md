# Track Management API

A REST API for managing artists, their tracks, and the distribution of those tracks to Digital Service Providers (DSPs) such as Spotify, Apple Music and YouTube. Built for a media-tech company distributing content across the MENA region.

- **Runtime:** .NET 10 / ASP.NET Core Web API
- **Persistence:** EF Core 10 with SQL Server and code-first migrations
- **Architecture:** Clean Architecture (Domain → Application → Infrastructure → API)
- **Auth:** JWT bearer tokens, PBKDF2 password hashing, default-deny authorization
- **Docs:** OpenAPI document and Swagger UI (Development only)

---

## Table of contents

1. [Features](#features)
2. [Architecture](#architecture)
3. [Getting started](#getting-started)
4. [Configuration](#configuration)
5. [Authentication](#authentication)
6. [API reference](#api-reference)
7. [Domain rules](#domain-rules)
8. [Error handling](#error-handling)
9. [Seed data](#seed-data)
10. [Database and migrations](#database-and-migrations)
11. [Logging](#logging)
12. [Testing](#testing)
13. [Known limitations](#known-limitations)

---

## Features

- Create and list artists
- Create tracks (ISRC validated and unique) and list them with filters and paging
- Track details including every DSP distribution and its status
- Submit a track to one or more DSPs in a single all-or-nothing operation
- Manual track status workflow (`Draft` → `Submitted` → `Distributed`) with enforced transition rules
- JWT login, with write endpoints protected and read endpoints public
- Consistent RFC 7807 `ProblemDetails` error responses with stable machine-readable codes
- Optimistic concurrency (SQL Server `rowversion`) and database-level constraints as a second line of defence
- Realistic seed data, applied automatically with the migrations

## Architecture

The solution follows Clean Architecture. Dependencies point inward only.

```
Api ──► Application ──► Domain
 │          ▲
 └──► Infrastructure ─┘
```

| Project | Responsibility |
| --- | --- |
| `TrackManagement.Domain` | Entities, value objects (`Isrc`), enums, business rules, domain exceptions. No dependencies. |
| `TrackManagement.Application` | Use cases, DTOs, FluentValidation validators, repository and service abstractions. |
| `TrackManagement.Infrastructure` | EF Core `DbContext`, entity configurations, migrations, repositories, seeding, JWT and password hashing. |
| `TrackManagement.Api` | Controllers, authentication setup, global exception handler, OpenAPI, CORS, Serilog. |

Key design decisions:

- **No MediatR or AutoMapper.** Each operation is a plain `*UseCase` class, registered automatically by assembly scanning (Scrutor). The list of use cases is the list of things the system can do.
- **Rich domain model.** `Track` is an aggregate root that owns its `TrackDistribution` children and enforces status transitions and distribution rules. Entities have private setters and factory methods.
- **Simple per-aggregate repositories**, no generic repository. `IUnitOfWork` wraps `SaveChanges` and translates SQL errors (unique violations, concurrency) into application exceptions so no EF or SQL type leaks upward.
- **Client-generated identifiers.** Ids are GUID v7 (time-ordered), generated in the domain.
- **Enums stored as strings** with `CHECK` constraints generated from the enum names.
- **Default-deny security.** A fallback authorization policy requires authentication everywhere; public endpoints opt out explicitly with `[AllowAnonymous]`.

### Repository layout

```
.
├── src/
│   ├── TrackManagement.Api/
│   ├── TrackManagement.Application/
│   ├── TrackManagement.Domain/
│   └── TrackManagement.Infrastructure/
├── tests/
│   ├── TrackManagement.UnitTests/
│   └── TrackManagement.IntegrationTests/
├── SQLQuery.sql              # Verification queries for the seeded database
├── TrackManagement.slnx
├── global.json               # Pins the .NET SDK
└── dotnet-tools.json         # dotnet-ef local tool
```

## Getting started

### Prerequisites

- [.NET SDK 10.0.103](https://dotnet.microsoft.com/download) or later (see `global.json`)
- SQL Server (LocalDB, Express, Developer, or Docker)

### 1. Clone and restore

```bash
git clone https://github.com/girgism/Track-Management-System.git
cd Track-Management-System
dotnet tool restore
dotnet restore
```

### 2. Configure the database

The default connection string in `src/TrackManagement.Api/appsettings.json` uses Windows authentication against a local SQL Server instance:

```
Server=.;Database=TrackManagementDb;Trusted_Connection=True;TrustServerCertificate=True
```

To use a different server (for example SQL Server in Docker), override it without editing the file:

```bash
# Start SQL Server in Docker (optional)
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_strong_Password1" \
  -p 1433:1433 --name trackdb -d mcr.microsoft.com/mssql/server:2022-latest

# Point the API at it
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=TrackManagementDb;User Id=sa;Password=Your_strong_Password1;TrustServerCertificate=True"
```

### 3. Create the database and seed it

Seeding (sample data and the admin user) runs as part of the migration:

```bash
dotnet ef database update \
  --project src/TrackManagement.Infrastructure \
  --startup-project src/TrackManagement.Api
```

> The application does not migrate the database on startup. Run the command above first, and again whenever new migrations are added.

### 4. Run the API

```bash
dotnet run --project src/TrackManagement.Api --launch-profile https
```

| URL | Purpose |
| --- | --- |
| `https://localhost:7074/swagger` | Swagger UI (Development only) |
| `https://localhost:7074/openapi/v1.json` | OpenAPI document (Development only) |
| `http://localhost:5071` | HTTP endpoint (redirects to HTTPS when the HTTPS profile is running) |

### 5. Try it

```bash
# 1. Log in as the seeded admin
curl -sk https://localhost:7074/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@trackmanagement.local","password":"Admin@12345"}'

# 2. Public read, no token needed
curl -sk "https://localhost:7074/api/tracks?status=Submitted"

# 3. Protected write, token required
curl -sk -X POST https://localhost:7074/api/artists \
  -H "Authorization: Bearer <accessToken>" \
  -H "Content-Type: application/json" \
  -d '{"name":"Salma Adel","email":"salma@example.com","country":"EG"}'
```

In Swagger UI, call `POST /api/auth/login`, copy the `accessToken`, click **Authorize**, and paste the token (the `Bearer` prefix is added for you).

## Configuration

Settings are read from `appsettings.json`, `appsettings.{Environment}.json`, user secrets and environment variables (use `__` as the section separator, e.g. `Jwt__SigningKey`).

| Key | Description | Default |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | Local SQL Server, Windows auth |
| `Jwt:Issuer` | Token issuer | `TrackManagement.Api` |
| `Jwt:Audience` | Token audience | `TrackManagement.Clients` |
| `Jwt:SigningKey` | HMAC-SHA256 signing key, **at least 32 bytes**. Required. | Development value only, in `appsettings.Development.json` |
| `Jwt:ExpiryMinutes` | Token lifetime, 1 to 1440 | `60` |
| `Seed:AdminEmail` | Email of the admin created by seeding | Development value only |
| `Seed:AdminPassword` | Admin password, **at least 10 characters** | Development value only |
| `Cors:AllowedOrigins` | Allowed front-end origins | `http://localhost:4200`, `https://localhost:4200` |

The app validates JWT settings at startup and refuses to start with a missing or weak signing key. If `Seed:AdminEmail` or `Seed:AdminPassword` is absent, no admin user is created.

### Secrets outside Development

`appsettings.Development.json` contains throwaway credentials for local use only. For any other environment, supply your own:

```bash
dotnet user-secrets init --project src/TrackManagement.Api
dotnet user-secrets set "Jwt:SigningKey" "<random string of 32+ bytes>" --project src/TrackManagement.Api
dotnet user-secrets set "Seed:AdminEmail" "you@company.com" --project src/TrackManagement.Api
dotnet user-secrets set "Seed:AdminPassword" "<strong password>" --project src/TrackManagement.Api
```

or via environment variables: `Jwt__SigningKey`, `Seed__AdminEmail`, `Seed__AdminPassword`.

## Authentication

### Default user (Development)

| Field | Value |
| --- | --- |
| Email | `admin@trackmanagement.local` |
| Password | `Admin@12345` |
| Role | `Admin` |

> **Warning:** these credentials exist for local development only. Change or remove them before deploying anywhere shared.

The admin is created by the seeder only if no user with that email exists. The password is stored as a salted PBKDF2-HMAC-SHA256 hash (600,000 iterations), never in plain text.

### Login

`POST /api/auth/login` (anonymous)

Request:

```json
{
  "email": "admin@trackmanagement.local",
  "password": "Admin@12345"
}
```

Response `200 OK`:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresAtUtc": "2026-11-18T12:00:00+00:00"
}
```

Send the token on protected requests:

```
Authorization: Bearer <accessToken>
```

Failed logins return `401` with the same message for an unknown email and a wrong password, and both paths cost the same time, so accounts cannot be enumerated.

### Token details

- Algorithm HS256, pinned on validation (blocks `alg: none` and algorithm-confusion attacks)
- Claims: `sub` (user id), `email`, `role`, `jti`
- Issuer, audience, lifetime and signature are all validated; clock skew is 30 seconds
- Lifetime is `Jwt:ExpiryMinutes` (60 by default). There is no refresh token; log in again after expiry.

### Which endpoints need a token

Reads are public. Everything that changes data requires authentication.

| Endpoint | Access |
| --- | --- |
| `POST /api/auth/login` | Public |
| `GET /api/artists` | Public |
| `GET /api/tracks` | Public |
| `GET /api/tracks/{id}` | Public |
| `POST /api/artists` | **JWT required** |
| `POST /api/tracks` | **JWT required** |
| `POST /api/tracks/{id}/distribute` | **JWT required** |
| `PATCH /api/tracks/{id}/status` | **JWT required** |

A missing, expired or invalid token returns `401 Unauthorized`.

## API reference

Interactive documentation is available in Swagger UI. Property names are camelCase and enums are serialized as **strings** (integers are rejected).

### Artists

**`POST /api/artists`** — create an artist (JWT)

```json
{ "name": "Salma Adel", "email": "salma@example.com", "country": "EG" }
```

- `name`: required, max 200 characters
- `email`: required, valid address, max 254 characters, unique (case-insensitive)
- `country`: required, 2-letter ISO 3166-1 alpha-2 code (stored uppercase)

Response `200 OK`: `{ "id": "<guid>", "message": "Artist created successfully." }`

**`GET /api/artists`** — list all artists ordered by name

```json
[{ "id": "<guid>", "name": "Karim Fawzy", "email": "karim.fawzy@example.com", "country": "EG" }]
```

### Tracks

**`POST /api/tracks`** — create a track in `Draft` status (JWT)

```json
{
  "title": "Sahar El Leil",
  "artistId": "<guid>",
  "isrc": "EG-N01-24-00001",
  "releaseDate": "2024-03-15",
  "genre": "Pop"
}
```

- `title`: required, max 300 characters
- `artistId`: must reference an existing artist
- `isrc`: 12 characters: 2-letter country, 3 alphanumeric registrant, 2-digit year, 5-digit designation. Hyphens and spaces are accepted and stripped; stored uppercase. Unique.
- `releaseDate`: `yyyy-MM-dd`, not before 1900
- `genre`: `Pop`, `Rock`, `HipHop`, `Electronic`, `RnB`, `Jazz`, `Classical`, `Folk`, `Shaabi`, `Mahraganat`

**`GET /api/tracks`** — list tracks (public)

| Query parameter | Type | Notes |
| --- | --- | --- |
| `artistId` | GUID | Filter by artist |
| `genre` | enum | Filter by genre |
| `status` | enum | `Draft`, `Submitted`, `Distributed` |
| `page` | int | Default `1` |
| `pageSize` | int | Default `20`, maximum `100` |

Returns an array ordered by release date (newest first):

```json
[{
  "id": "<guid>", "title": "Zahma", "artistId": "<guid>", "artistName": "Karim Fawzy",
  "isrc": "EGK0125000001", "releaseDate": "2025-12-05", "genre": "Mahraganat", "status": "Submitted"
}]
```

**`GET /api/tracks/{id}`** — track details with distributions (public)

```json
{
  "id": "<guid>",
  "title": "Cedars in the Rain",
  "artist": { "id": "<guid>", "name": "Layla Haddad", "email": "layla.haddad@example.com", "country": "LB" },
  "isrc": "LBL0125000001",
  "releaseDate": "2025-05-10",
  "genre": "Folk",
  "status": "Submitted",
  "distributions": [
    { "id": "<guid>", "dspId": "<guid>", "dspName": "Apple Music", "submittedAt": "2026-06-05T09:00:00+00:00", "status": "Rejected" },
    { "id": "<guid>", "dspId": "<guid>", "dspName": "Spotify", "submittedAt": "2026-06-05T09:00:00+00:00", "status": "Live" }
  ]
}
```

**`POST /api/tracks/{id}/distribute`** — submit a track to DSPs (JWT)

```json
{ "dspIds": ["<spotify-guid>", "<youtube-guid>"] }
```

All-or-nothing: if any DSP is unknown or already associated, nothing is saved. Rejected distributions are resubmitted (status returns to `Pending`). See [Domain rules](#domain-rules).

**`PATCH /api/tracks/{id}/status`** — change the track status (JWT)

```json
{ "status": "Submitted" }
```

### Status codes

| Code | Meaning |
| --- | --- |
| `200` | Success |
| `400` | Validation failed, or a referenced id in the body does not exist |
| `401` | Missing/invalid token, or bad login credentials |
| `404` | Resource in the URL does not exist |
| `409` | Duplicate, concurrent modification, or business-rule violation |
| `500` | Unexpected server error |

## Domain rules

### Track status workflow

Status is changed manually through `PATCH /api/tracks/{id}/status`. It is never promoted automatically.

| From | To | Allowed when |
| --- | --- | --- |
| `Draft` | `Submitted` | Always |
| `Submitted` | `Draft` | The track has no distribution records |
| `Submitted` | `Distributed` | At least one distribution is `Live` |
| any | same status | No-op |
| anything else | | Rejected with `409` (`track.invalid_status_transition`) |

### Distribution rules

- A track must be `Submitted` or `Distributed` to be sent to DSPs; `Draft` tracks are rejected (`track.not_distributable`).
- A track can have at most one distribution per DSP (unique index on `TrackId` + `DspId`).
- Submitting a DSP that already has a `Pending` or `Live` distribution is a conflict (`track.duplicate_distribution`).
- A `Rejected` distribution can be resubmitted and becomes `Pending` again.
- One request may target up to 50 DSPs; duplicate ids in the request are rejected.
- Distribution status moves `Pending` → `Live` or `Pending` → `Rejected` only.

## Error handling

All errors use `application/problem+json` (RFC 7807) with two extras: a stable `code` and a `traceId` for log correlation.

Validation error (`400`):

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "isrc": ["ISRC must be 12 characters: 2-letter country, 3-character registrant, 2-digit year, 5-digit designation (e.g. EG-A01-23-00001)."]
  },
  "code": "validation.failed",
  "traceId": "00-..."
}
```

Business-rule error (`409`):

```json
{
  "title": "Business rule violation",
  "status": 409,
  "detail": "A track can only be marked Distributed once at least one DSP distribution is Live.",
  "code": "track.invalid_status_transition",
  "traceId": "00-..."
}
```

Common codes:

| Code | Status | Cause |
| --- | --- | --- |
| `validation.failed` / `domain.validation` | 400 | Invalid input |
| `reference.not_found` | 400 | `artistId` or `dspIds` reference something that does not exist |
| `track.not_found` | 404 | Unknown track id |
| `artist.duplicate_email` | 409 | Email already used |
| `track.duplicate_isrc` | 409 | ISRC already used |
| `track.duplicate_distribution` | 409 | Already associated with that DSP |
| `track.not_distributable` | 409 | Track is `Draft` |
| `track.invalid_status_transition` | 409 | Transition not allowed |
| `concurrency.conflict` | 409 | Row changed by another request |
| `auth.invalid_credentials` | 401 | Wrong email or password |
| `server.error` | 500 | Unhandled exception; quote the `traceId` |

Server errors never expose exception details to the client; they are logged with the same `traceId`.

## Seed data

Seeding runs once on an empty database via the EF Core `UseSeeding` hook and is idempotent.

| Entity | Count | Details |
| --- | --- | --- |
| Artists | 4 | Nour El-Sayed (EG), Layla Haddad (LB), Omar Al-Rashid (SA), Karim Fawzy (EG) |
| DSPs | 3 | Spotify, Apple Music, YouTube |
| Tracks | 11 | 3 `Distributed`, 5 `Submitted`, 3 `Draft`, across 8 genres |
| Distributions | 14 | A mix of `Live`, `Pending` and `Rejected` |
| Users | 1 | The admin described above (only if seed credentials are configured) |

`SQLQuery.sql` contains queries to verify the seed counts, plus statements that deliberately trigger the database constraints (duplicate ISRC, invalid status, duplicate distribution, deleting an artist that has tracks).

## Database and migrations

Migrations live in `src/TrackManagement.Infrastructure/Persistence/Migrations`.

```bash
# Add a migration
dotnet ef migrations add <Name> \
  --project src/TrackManagement.Infrastructure \
  --startup-project src/TrackManagement.Api \
  --output-dir Persistence/Migrations

# Apply migrations (also runs seeding)
dotnet ef database update \
  --project src/TrackManagement.Infrastructure \
  --startup-project src/TrackManagement.Api

# Start over locally
dotnet ef database drop --force \
  --project src/TrackManagement.Infrastructure \
  --startup-project src/TrackManagement.Api
```

Schema highlights:

- Unique indexes: `UX_Artists_Email`, `UX_Tracks_Isrc`, `UX_Dsps_Name`, `UX_Users_Email`, `UX_TrackDistributions_TrackId_DspId`
- `CHECK` constraints on every enum column (genre, track status, distribution status, user role)
- Foreign keys: artist → tracks and DSP → distributions use `Restrict`; deleting a track cascades to its distributions
- `rowversion` concurrency tokens on `Tracks` and `TrackDistributions`
- ISRC stored as `char(12)`, country as `char(2)`

## Logging

Serilog writes to the console and to rolling daily files in `logs/trackmanagement-.log` (14 days retained). Requests are logged with `UseSerilogRequestLogging`. Rejected client requests are logged with status and code only, never messages that could contain personal data such as emails. In Development, EF Core SQL commands are logged at `Information`.

## Testing

The solution contains `TrackManagement.UnitTests` (xUnit, referencing Domain and Application) and `TrackManagement.IntegrationTests` (xUnit, referencing the API) projects.

```bash
dotnet test
```

> Automated tests are still to be written. The projects are scaffolded and the domain was designed to be unit-testable without infrastructure (time via `TimeProvider`, rules inside the aggregate).

## Known limitations

- The API does not apply migrations on startup; run `dotnet ef database update` explicitly.
- Only one role (`Admin`) exists. Every authenticated user can call every write endpoint.
- There are no endpoints yet to create DSPs or to mark a distribution `Live` or `Rejected`; those transitions exist in the domain and are exercised by the seeder.
- No refresh tokens, login throttling or account lockout.
- `GET /api/tracks` returns a plain array; the total count is not exposed to clients.
- Swagger UI and the OpenAPI document are only enabled in the Development environment.
