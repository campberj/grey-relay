# ADR 0001 — S0 runtime, local persistence and privacy boundary

**Status:** Proposed — awaiting owner review in PR.
**Date:** 2026-10-27
**Owner / decider:** project owner (RAVEN ACTUAL)
**Implementer:** technical implementation agent
**Roadmap:** S0, Issue 1

Each item below is labelled:

- **[OWNER]**: the owner chose it explicitly on 2026-10-27, before this ADR was written.
- **[PROPOSED]**: the implementer's recommendation. It holds only if the owner approves this PR.
- **[OPEN]**: not decided; listed with the milestone it blocks.

## 1. Context

GREY RELAY is a documentation-only repository (see [README](../../README.md), [ARCHITECTURE](../ARCHITECTURE.md)). S0 needs a runnable local web app and API, with lint, format, test and CI commands. It must run natively on macOS without Docker. It must also have a clear server-side boundary, so GM-only truth and puzzle keys never reach the player client.

Constraints from the handoff docs:

- The owner knows .NET.
- The workflow is macOS-native, with no Docker.
- Persistence covers multiple campaigns and carries a versioned event journal (S1).
- JSON Schema Draft 2020-12 is the portable content format. A provisional schema already exists in [`schemas/`](../../schemas/campaign-player-state.v1.schema.json).
- There is no real network, shell or process execution from the fictional console.
- The repository is **public**, so no secrets, GM truth or third-party assets go into it.

Local toolchain at the time of writing: .NET SDK 10.0.402 and Node v25.6.1 (not LTS) are installed. PostgreSQL is not installed.

## 2. Decision

### 2.1 Runtime and languages

| Concern | Decision | Label |
| --- | --- | --- |
| Frontend | React + TypeScript (strict) + Vite | [OWNER] |
| Backend | ASP.NET Core on **.NET 10 (LTS)** | [OWNER] |
| .NET SDK pin | `global.json` → `10.0.400`, `rollForward: latestFeature` | [PROPOSED] |
| Node.js | **Node 24 LTS**, pinned via `.nvmrc` and `package.json` `engines` | [OWNER] |
| JS package manager | npm (ships with Node; avoids an extra prerequisite) | [PROPOSED] |

### 2.2 Persistence

| Concern | Decision | Label |
| --- | --- | --- |
| S0/S1 database | **SQLite**, behind a storage abstraction | [OWNER] |
| Later database | **PostgreSQL**, via a separate ADR and tracked migration. SQLite is not assumed to be production-compatible | [OWNER] |
| Data access | EF Core 10 with the SQLite provider. The Npgsql provider is added later behind the same repository interfaces | [PROPOSED] |
| Schema changes | EF Core migrations, committed per provider | [PROPOSED] |
| SQLite settings | WAL journal mode; foreign keys enforced; file path from configuration, defaulting to a git-ignored `.data/` directory in development | [PROPOSED] |

The application layer defines repository and unit-of-work interfaces (ports), and the infrastructure layer implements them (Dependency Inversion). Domain and application code never reference EF Core or SQLite types. This keeps the PostgreSQL move to a new adapter and its migrations, not a domain rewrite.

Known SQLite/PostgreSQL divergences to watch: `DateTimeOffset` ordering and storage, concurrency tokens (no native `xmin`), case-sensitive collation, and sequence generation. The S1 integration test suite should be provider-agnostic, so it can later run against both.

### 2.3 Deployment and identity

| Concern | Decision | Label |
| --- | --- | --- |
| Deployment target S0–S3 | **Local single-user** on the owner's Mac | [OWNER] |
| Authentication | None in S0–S3. Real auth gets its own ADR before any hosting | [OWNER] |
| Roles | `player`, `gm`, `admin` as **logical roles**, enforced server-side | [OWNER] |
| Network binding | API listens on `localhost` only. Vite dev server proxies `/api` to it, so no CORS policy is needed | [PROPOSED] |

### 2.4 Privacy boundary (GM truth)

[PROPOSED]

- GM-only data (hidden truths, puzzle rubrics, unrevealed artifacts) is stored in **separate tables**. It is reached only through GM-scoped repository interfaces, which player-facing handlers cannot resolve.
- Every API response is a purpose-built DTO produced by a role-specific projection. Entities are never serialized directly.
- Player endpoints and GM endpoints live in separate route groups (`/api/...` and `/api/gm/...`) with separate authorization policies.
- Tests assert that player routes never return GM-only fields or unrevealed IDs (README invariant 6; CONTENT_CONTRACTS §10 case 3).

S0 contains no GM data. This section fixes the shape so that S1/S2 follow it.

### 2.5 Fictional-console safety

[PROPOSED]

- `Microsoft.CodeAnalysis.BannedApiAnalyzers` is applied to Domain and Application projects. It bans `System.Diagnostics.Process`, `System.Net.*` sockets/HTTP, and reflection-based dynamic loading. Violations fail the build.
- Architecture tests verify layer dependencies: Domain depends on nothing, and Application depends only on Domain.

### 2.6 Solution layout

[PROPOSED]

~~~text
global.json
GreyRelay.sln
.editorconfig
.nvmrc
src/
  api/
    GreyRelay.Domain/          pure domain model and rules; no framework dependencies
    GreyRelay.Application/     use cases, ports (repository/clock interfaces), DTO projections
    GreyRelay.Infrastructure/  EF Core SQLite, repository adapters, system clock
    GreyRelay.Api/             ASP.NET Core host, endpoint groups, composition root
  web/                         Vite + React + TypeScript client
tests/
  GreyRelay.Domain.Tests/
  GreyRelay.Application.Tests/
  GreyRelay.Api.Tests/         in-process integration tests (WebApplicationFactory)
  GreyRelay.Architecture.Tests/
.github/workflows/ci.yml
~~~

Rationale: the layering follows Clean Architecture. Each project has one reason to change (Single Responsibility). Only the API project knows about concrete implementations, which are wired in its composition root.

### 2.7 Quality tooling

| Concern | Decision | Label |
| --- | --- | --- |
| .NET tests | xUnit v3; in-process API tests via `WebApplicationFactory` | [PROPOSED] |
| Web tests | Vitest + React Testing Library. Playwright end-to-end tests added in S3, not S0 | [PROPOSED] |
| .NET lint/format | `.editorconfig`, `dotnet format --verify-no-changes`, nullable enabled, warnings as errors | [PROPOSED] |
| Web lint/format | ESLint (typescript-eslint) + Prettier; `tsc --noEmit` | [PROPOSED] |
| API contract | Built-in ASP.NET Core OpenAPI document → TypeScript types generated with `openapi-typescript`. The schemas in `schemas/` remain the source of truth for portable save/pack documents | [PROPOSED] |
| CI | GitHub Actions on pull requests: `macos-latest` and `ubuntu-latest`, building, linting and testing both API and web | [OWNER] |

### 2.8 Developer commands (target for S0)

[PROPOSED]

~~~sh
brew install node@24          # prerequisite; .NET 10 SDK also required
dotnet restore && npm ci --prefix src/web
dotnet test                   # all .NET tests
npm test --prefix src/web     # web tests
dotnet run --project src/api/GreyRelay.Api
npm run dev --prefix src/web  # http://localhost:5173, proxies /api
~~~

## 3. Alternatives considered

**Backend:**

- **TypeScript server (Fastify):** one language end to end and shared types. Rejected by the owner in favour of .NET familiarity and stronger domain-modelling ergonomics.
- **Next.js full-stack:** fastest to scaffold. However, it blurs the client/server boundary, which raises the risk of leaking spoilers through hydration or prefetch (ARCHITECTURE §8).

**Database:**

- **PostgreSQL now (Homebrew):** no dev/prod divergence, but it needs a service install and management for a local single-user app. Deferred, not rejected.
- **SQLite only:** simplest, but it would block a hosted, multi-user future. Rejected in favour of an abstraction with a planned PostgreSQL adapter.

**Data access:**

- **Dapper + hand-written SQL:** more control and less magic. It would mean maintaining two SQL dialects by hand when PostgreSQL arrives. Kept as an option for read-heavy projections if EF Core proves limiting.

**Identity:**

- **Real auth now:** adds S0 scope with no current user benefit. Deferred to the hosting ADR.

## 4. Consequences

- There are two toolchains (.NET and Node). Contributors need both SDKs. CI covers both on macOS and Linux.
- GM/player separation is an architectural property from day one (separate tables, repositories, route groups and DTOs), not a later retrofit.
- PostgreSQL migration cost is limited to a new infrastructure adapter, its migrations and a provider-parity test run.
- Without auth, anyone with access to the owner's machine and localhost can call GM routes. That is acceptable only for local single-user mode (see open question Q1).

## 5. Rollout / migration

1. Merge this ADR.
2. **S0 scaffold PR** (stacked on this branch): solution layout, `/api/health`, placeholder Campaign Library and TOC screens with no mutation, one deterministic domain test for an independent campaign aggregate, lint/format/test scripts, CI and setup docs.
3. S1 opens its own ADR for event journal design before writing persistence code.

No data migration: no persisted data exists yet.

## 6. Reversal conditions

- **EF Core** blocks event-journal requirements such as append-only sequence guarantees or projection performance → replace those adapters with Dapper.
- **SQLite** concurrency or locking breaks local multi-tab use → bring the PostgreSQL ADR forward.
- **Hosting** is required → a new auth/identity ADR supersedes §2.3.
- **Node 24** support or Vite compatibility issues → update the Node pin in a focused PR.

## 7. Open questions

| ID | Question | Blocks |
| --- | --- | --- |
| Q1 | How does the local single user act as GM? Options include a server-side config flag enabling `/api/gm`, a separate local GM token, or a separate GM app. Without real auth, nothing technically stops the player from reading GM data on their own machine. | S2 (GM view) |
| Q2 | Event journal design: full event sourcing, or transactional audit log + projections | S1 |
| Q3 | SQLite file location for non-development use (e.g. `~/Library/Application Support/GreyRelay`) | S1 |
| Q4 | Repository software license (none selected; README §9) | Before external contributions |
| Q5 | Fictional console: CLI-like first or hybrid GUI first | S3 |

## 8. S0 acceptance checks for this decision

- [ ] Fresh macOS clone with only the .NET 10 SDK and Node 24 installed starts API and web using documented commands, without Docker.
- [ ] `GET /api/health` returns 200 on `localhost`.
- [ ] `dotnet test` and `npm test` pass locally and in CI on macOS and Linux.
- [ ] Format/lint checks fail CI on violations.
- [ ] Banned-API analyzer fails the build if Domain/Application references `Process` or networking APIs.
- [ ] Architecture tests enforce Domain → nothing; Application → Domain.
- [ ] No GM data, secrets or third-party assets committed.
