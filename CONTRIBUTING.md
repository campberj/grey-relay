# Contributing to GREY RELAY

Read [README.md](README.md), [AGENTS.md](AGENTS.md) and [docs/AGENT_HANDOFF.md](docs/AGENT_HANDOFF.md) first. Architecture decisions live in [docs/adr/](docs/adr/). The stack is defined by [ADR 0001](docs/adr/0001-s0-stack-and-local-persistence.md).

**Current state:** S0 skeleton plus the S1 event journal. The API has a health route, an empty campaign list and an append-only SQLite event journal (not yet exposed through the API). The web shell has placeholder Campaign Library and Operations Center screens. There are no campaign features or gameplay yet.

## Prerequisites (macOS, no Docker)

| Tool | Version | Install |
| --- | --- | --- |
| .NET SDK | 10.0.400 or later 10.0.x (pinned in [global.json](global.json)) | <https://dotnet.microsoft.com/download> |
| Node.js | 26 LTS (pinned in [.nvmrc](.nvmrc)) | `brew install node@26`, `nvm install`, or `fnm use` |

In Homebrew, `node@26` is currently an alias for the main `node` formula. Once Node 27 ships it becomes a separate keg-only formula; then put it first on your `PATH` for this project:

```sh
export PATH="$(brew --prefix node@26)/bin:$PATH"
node -v   # v26.x
```

## First run

```sh
dotnet restore
npm ci --prefix src/web
```

Start the API and the web client in two terminals:

```sh
dotnet run --project src/api/GreyRelay.Api   # http://localhost:5080
npm run dev --prefix src/web                 # http://localhost:5173 (proxies /api to 5080)
```

Health check:

```sh
curl http://localhost:5080/api/health        # Healthy
```

### Database

The API creates and migrates its SQLite database on startup and logs the path it uses ([ADR 0002](docs/adr/0002-campaign-event-journal-and-import.md) §2.9):

| Context | Location |
| --- | --- |
| Development | `.data/greyrelay.dev.db` in the repository (git-ignored) |
| Normal use | `~/Library/Application Support/GreyRelay/greyrelay.db` |
| Override | `Storage:DatabasePath` setting or the `Storage__DatabasePath` environment variable |

To reset development data, stop the API and delete `.data/`.

The API listens on `localhost` only and rejects requests with a non-`localhost` `Host` header. In Development it serves its OpenAPI document at <http://localhost:5080/openapi/v1.json>.

## Everyday commands

| Task | Command |
| --- | --- |
| All .NET tests | `dotnet test` |
| .NET format check / fix | `dotnet format --verify-no-changes` / `dotnet format` |
| Web tests (once / watch) | `npm test --prefix src/web` / `npm run test:watch --prefix src/web` |
| Web lint, types, format check | `npm run lint --prefix src/web`, `npm run typecheck --prefix src/web`, `npm run format:check --prefix src/web` |
| Web format fix | `npm run format --prefix src/web` |
| Regenerate API types | `dotnet build`, then `npm run gen:api --prefix src/web` |
| Add an EF Core migration | `dotnet tool restore`, then `dotnet ef migrations add <Name> --project src/api/GreyRelay.Infrastructure --startup-project src/api/GreyRelay.Infrastructure --output-dir Persistence/Migrations` |
| Check migrations match the model | `dotnet ef migrations has-pending-model-changes --project src/api/GreyRelay.Infrastructure --startup-project src/api/GreyRelay.Infrastructure` |

CI ([.github/workflows/ci.yml](.github/workflows/ci.yml)) runs all of the above on macOS and Linux for every pull request.

## Layout and rules

```text
src/api/GreyRelay.Domain          pure domain model; no framework dependencies
src/api/GreyRelay.Application     use cases, ports (interfaces) and read models
src/api/GreyRelay.Infrastructure  adapters implementing the ports
src/api/GreyRelay.Api             ASP.NET Core host and composition root
src/web                           React + TypeScript client
tests/                            one test project per layer, plus architecture tests
```

- **Dependencies point inward:** Api → Infrastructure → Application → Domain. `GreyRelay.Architecture.Tests` enforces this.
- **Domain and Application are pure.** [src/api/BannedSymbols.txt](src/api/BannedSymbols.txt) makes the build fail if they use processes, networking, the file system, dynamic assembly loading, the system clock, `Guid.NewGuid` or `Random`. Inject a port instead. This keeps behaviour replayable (README invariant 9) and the fictional console safe (README invariant 11).
- **Never return domain entities from the API.** Map them to purpose-built read models; GM-only data will get separate routes and models (ADR 0001 §2.4).
- **The API contract is generated.** The API build writes `src/web/openapi/GreyRelay.Api.json`, and `npm run gen:api` turns it into `src/web/src/api/schema.d.ts`. Commit both. CI fails if they are out of date.
- **The web client reaches data only through ports.** Components call `CampaignApi` via context, never `fetch` directly; tests inject fakes.
- **Warnings are errors** in .NET builds.
- **The event journal is append-only.** Database triggers reject `UPDATE`, `DELETE` and out-of-sequence inserts on `campaign_events`. Fix mistakes with correction events (ADR 0002 §2.5), never by editing rows. A migration that rebuilds that table must recreate its triggers.

## Workflow

- Branch from `main`, keep PRs focused, and never merge your own PR. The owner reviews and merges.
- Each PR description covers:
  - scope
  - implemented vs proposed
  - assumptions
  - tests run
  - reproduction steps
  - screenshots for UI changes
  - data migration
  - story impact

  See the operating agreement in [docs/ROADMAP.md](docs/ROADMAP.md).
- Do not invent rules, story outcomes or casualty fate, and do not commit third-party assets (see AGENTS.md).

## Troubleshooting

| Symptom | Fix |
| --- | --- |
| `Error: Port 5173 is already in use` | Another Vite server is running. Stop it, or run `npm run dev --prefix src/web -- --port 5174`. |
| `node -v` shows a version other than 26 | Install or select Node 26 (see Prerequisites). `npm ci` warns when `engines` doesn't match. |
| `dyld: Library not loaded … libsimdjson` when running `node` | Your Homebrew `node` formula is out of date with a shared library. Run `brew upgrade node`. |
| `Testing with VSTest target is no longer supported` | Run `dotnet test` from the repository root, so `global.json` opts into Microsoft.Testing.Platform. |
| CI fails at "Check EF Core migrations match the model" | Add a migration for your model change (see Everyday commands) and commit it. |
| `SQLite Error 19: 'campaign_events is append-only…'` | Working as intended: history cannot be edited. Append a correction event instead. |
| CI fails at "Check API contract is up to date" | Run `dotnet build`, then `npm run gen:api --prefix src/web`, and commit the two regenerated files. |
