# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Development
dotnet run --project SeedPlan/SeedPlan.csproj          # HTTP on localhost:5258
dotnet run --project SeedPlan/SeedPlan.csproj -- https # HTTPS on localhost:7134

# Build
dotnet build

# Tests
dotnet test SeedPlan.Client.UnitTests/SeedPlan.Client.UnitTests.csproj

# Production
docker build -t seedplan .   # multi-stage: SDK 8.0 build → AspNet 8.0 runtime, port 8080
```

## Architecture

**SeedPlan** is a mobile-first PWA for seed inventory and sowing management. Built with Blazor WebAssembly (.NET 8) hosted by an ASP.NET Core server, with Supabase as the sole backend (PostgreSQL + GoTrue auth).

```
SeedPlan/              # ASP.NET Core host — serves the Blazor WASM app and static files
SeedPlan.Client/       # Blazor WebAssembly app (Pages, Components, Layout, Services)
Shared/                # Models, Interfaces, Helpers shared between client and tests
SeedPlan.Client.UnitTests/  # MSTest + Moq unit tests
supabase/              # Supabase config, migrations, and Edge Functions
```

### Data flow

There is **no intermediate API layer**. The Blazor WASM client calls Supabase Postgrest directly via service classes in `SeedPlan.Client/Services/`. Supabase Row Level Security (RLS) enforces per-user data isolation at the database level — models carry Supabase column-mapping attributes, not Entity Framework.

The `Shared/Interfaces/` layer (e.g., `IAuthClient`, `IPlantLibraryService`, `IUserInventoryService`) exists primarily to enable unit testing without hitting Supabase. In production, the implementations in `Services/` are registered in `SeedPlan.Client/Program.cs`.

### Authentication

`SupabaseAuthStateProvider` implements Blazor's `AuthenticationStateProvider`. Sessions are persisted to `localStorage` (remember me) or `sessionStorage`. `SupabaseAuthClient` wraps the GoTrue SDK and is abstracted behind `IAuthClient`.

### App modes

The app has two modes toggled via `localStorage` key `appMode`:
- **SeedPlan** (default): seed inventory (`/seeds`), sowings (`/sowings`), plant guide (`/guide`)
- **DahliaBox**: dahlia library (`/dahlias`), tuber inventory (`/tubers`)

### Sowing status flow

Sowings progress through a defined state machine in `Shared/Helpers/SowingStatusFlow.cs`:
`Sown → Germinated → TrueLeaves → PottedOn → HardeningOff → PlantedOut → Harvested / Finished / Failed`

### Tags system

Tags are user-owned (`tags` table with `user_id`). Many-to-many relationships via `seed_tags` and `plant_tags` junction tables. RLS restricts access to the owning user only. See migration `20260410120000_fix_tags_rls.sql`.

### Settings architecture

- `/profile` — user profile, email, password
- `/settings` — frost date, growing zone, notification toggles

### Error handling

Services return `Result<T>` from **FluentResults** rather than throwing. Check `result.IsFailed` / `result.Value` at call sites.

## Code guidelines (from `.github/copilot-instructions.md`)

- Use **English** for all code comments.
- When debugging mismatched braces in Blazor markup, look for **stray closing `}` in rendered HTML text nodes** rather than mismatched `</div>` tags.
- In `PlantDetailModal`: action buttons (add-seed and close) must stay **on the same row as the title**, not below it. Include a small chevron icon in the summary section (as in the Seeds groups).
