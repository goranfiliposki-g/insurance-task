# Insurance Claims API

Backend API for insurance claims and covers (multi-tier, .NET 9).

## Run

```bash
dotnet run --project Claims.API
```

API runs at http://localhost:5107 (or https://localhost:7030). Swagger UI: https://localhost:7030/swagger (or same path on the HTTP port).

## Test

```bash
dotnet test Claims.Tests
```

## Structure

| Project | Role |
|--------|------|
| **Claims.API** | ASP.NET Core web API; controllers, filters, DI wiring. |
| **Claims.Application** | Use cases, DTOs, interfaces (services, repositories, premium policy). |
| **Claims.Domain** | Entities (Claim, Cover) and enums (ClaimType, CoverType). |
| **Claims.Infrastructure** | Persistence (EF Core, in-memory DBs), repositories, audit pipeline. |
| **Claims.Tests** | Unit and integration tests (xUnit, WebApplicationFactory). |

Dependencies: API → Application, Infrastructure; Application → Domain; Tests → API, Application, Domain, Infrastructure.

## Dates (cover and premium)

Cover periods and premium computation use **calendar-day** semantics only (no time-of-day). The API and application use `DateOnly` for start/end so that the period is unambiguous: one day means one calendar day, and the period length is the number of calendar days between start and end. Time-of-day is ignored; use `DateTime` only where an instant is required (e.g. claim "Created").
