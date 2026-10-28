# ADR 0002 — Campaign event journal, local storage and import fidelity

**Status:** Accepted — merged in PR #6 (2026-10-28 UTC). Amended 2026-10-28: `recorded_by` added to the envelope; Q2.1 closed.
**Date:** 2026-10-27
**Owner / decider:** project owner (RAVEN ACTUAL)
**Implementer:** technical implementation agent
**Story-continuity review requested:** §2.6 (event catalogue) and §2.8 (import fidelity), from the story-canon agent
**Roadmap:** S1, Issue 4
**Resolves:** ADR 0001 open questions Q2 (journal design) and Q3 (database location)

Labels follow ADR 0001:

- **[OWNER]**: decided by the owner.
- **[PROPOSED]**: the implementer's recommendation, pending approval of this PR.
- **[OPEN]**: undecided.

Everything below is **[PROPOSED]** unless marked otherwise.

## 1. Context

S1 must deliver durable multi-campaign persistence, an event journal, import/export and revision conflict checks ([ROADMAP](../ROADMAP.md) S1). The requirements already fixed by the handoff docs:

- **The journal is authoritative** for committed outcomes (README invariant 8).
- **Changes are explicit:** no invisible casualty, XP, resource or outcome updates. Corrections link to the original and never edit history (README invariant 4; CONTENT_CONTRACTS §8).
- **Replay is deterministic:** the same starting data, action log and seed yield identical results (README invariant 9; CONTENT_CONTRACTS §10 case 5).
- **Saves are portable:** schema-versioned, validated on import, with backup and restore (README invariant 10).
- **Import never implicitly overwrites a live campaign.** Roundtrip preserves outcomes, incomplete clues and unresolved statuses (CONTENT_CONTRACTS §9).
- **Storage** is SQLite behind ports, with PostgreSQL later [OWNER, ADR 0001].

The story-canon agent's review of S0 also asked that campaigns be importable **without unintentionally changing established narrative facts**. §2.8 addresses that directly.

## 2. Decision

### 2.1 Journal-authoritative state

- **One table is the source of truth.** Each campaign has an append-only event journal (`campaign_events`).
- **Current state is derived.** The current-state tables ("projections") the UI reads are computed from the journal. They are updated **in the same database transaction** as the append, so readers never see a committed event without its projection.
- **Projections are disposable.** An admin "rebuild" command replays the journal and must reproduce them exactly. A test asserts that the rebuilt and live projections are equal.

This is deliberately *not* full asynchronous CQRS or a dedicated event-store product. Synchronous projection keeps a local single-user app simple, and the journal still gives replay, audit and correction semantics.

### 2.2 Domain pattern: decide / evolve

Each aggregate exposes two pure functions, sometimes called the "decider" pattern:

- `Decide(state, command) → events | rejection`, which validates the command against the current state and returns the events it would produce.
- `Evolve(state, event) → state`, which applies one event.

Persistence calls `Decide`, appends the returned events, then folds them with `Evolve`, so domain code never touches storage. Replay is `Evolve` folded over the journal. Determinism comes from the purity rules already enforced by the S0 banned-API analyzer: no clock, GUID or random calls in domain code.

The S0 `Campaign` aggregate's mutate-in-place `Archive` method will be refactored to this shape. Its revision-conflict behaviour is unchanged.

### 2.3 Event envelope

Fields follow CONTENT_CONTRACTS §8:

| Field | Notes |
| --- | --- |
| `event_id` | UUID, from an injected ID generator |
| `campaign_id` | Every query is scoped by it; cross-campaign references are rejected |
| `sequence` | Per-campaign, gap-free, starts at 1. Unique `(campaign_id, sequence)`. **Campaign revision = last sequence** |
| `event_type` | Dotted name, e.g. `operator.accountability_changed` |
| `event_schema_version` | Integer, per event type |
| `actor` | `{kind: player\|gm\|system\|import, id}`: who made the decision |
| `recorded_by` | `{kind, id}`: who entered it into the app. Differs from `actor` when the owner records a chat GM ruling (ADR 0003 Q3.3) |
| `authority` | `{kind, reference}`: what authorised it, e.g. `player_order`, `gm_ruling` (citing the chat ruling), `import` or `correction` |
| `causation_id`, `correlation_id` | Optional links to the triggering command, AAR or session |
| `occurred_fictional` | Campaign clock `{day, time_z, precision}`, or explicit `unknown`. Never fabricated precision |
| `recorded_at_utc` | From the injected clock |
| `content_refs` | Pinned content-pack and rules-adapter versions |
| `corrects_event_id` | Set only on `event.corrected` |
| `payload` | JSON, validated against that event type's schema version |

### 2.4 Commands, concurrency and idempotency

The command pipeline runs in one transaction:

1. Load the projection.
2. Compare `expected_revision` with the current revision. A mismatch returns `409 Conflict` with the current revision.
3. Authorise the actor's role.
4. Validate the command.
5. `Decide`.
6. Append the events and update projections.
7. Commit and return the new revision.

Additional rules:

- **Idempotency:** every mutating request carries an `idempotency_key`. `(campaign_id, idempotency_key)` is unique and stores the original result, so a retried request returns that result instead of applying twice.
- **Database guards:** the unique `(campaign_id, sequence)` constraint backstops the revision check. SQLite triggers `RAISE(ABORT)` on any `UPDATE` or `DELETE` of `campaign_events`, so history cannot be edited even by a buggy adapter.
- **SQLite settings:** WAL mode, `foreign_keys=ON`, a busy timeout, and a single writer connection.

### 2.5 Corrections and event versioning

- **Corrections are new events.** An `event.corrected` event names the target event, the replacement or reference, and a reason. The original remains in the journal, and projections replay with the correction applied.
- **Old payloads are upgraded on read.** When an event payload's schema changes, "upcasters" convert the old version as it's read. **Stored events are never rewritten.**

### 2.6 Event catalogue governance

- **Every event type is registered** in a catalogue with a payload schema and a one-line meaning. An unregistered type is rejected on append.
- **Canon-touching types need story-canon review.** These are types that touch personnel accountability, medical status, mission outcome, intelligence confidence or campaign metrics. They are added in PRs reviewed by the story-canon agent and approved by the owner.
- **The catalogue has no shortcut statuses.** There is no "captured", "rescued" or "KIA" event unless a later approved ruling introduces one. Personnel status uses the orthogonal fields in CONTENT_CONTRACTS §3 (tactical_state, accountability, medical, duty).

### 2.7 Randomness

When a later feature needs randomness, the event records the PRNG algorithm and version, the seed, the draw index and the result. Replay uses the **recorded result**, never a re-roll. S1 itself has no random mechanics.

### 2.8 Import and export fidelity

Import exists to carry established fiction **exactly as it is**. These rules address the story-canon agent's concern:

1. **Import always creates a new campaign instance.** It never merges into or overwrites an existing campaign; an explicit merge strategy is out of scope for S1.
2. **Dry run first.** Validation produces a report before anything is written. It covers:
   - schema version and JSON Schema validity
   - unknown fields, which are rejected rather than ignored
   - ID uniqueness and referential integrity
   - size limits
   - `contains_gm_secrets: false` for player-state imports

   Nothing is written unless the report is clean and the user confirms.
3. **The import is the genesis event.** A single `campaign.imported` event stores the validated document **verbatim**, with its SHA-256 checksum, schema version and source name. All projections derive from that event.
4. **No normalisation of narrative values.** The importer never defaults, infers, "cleans up" or upgrades a status, confidence, qualifier or `unknown` value. An absent value stays absent, `accountability: "missing"` and `fate: "unknown"` stay exactly that, and every `does_not_establish` list is kept intact.
5. **Changes after import are explicit events**, with actor and authority, like any other change.
6. **Schema migrations are structural only.** A migration may rename or reshape fields but must not change a narrative value. A migration test fails if any canon-bearing value differs before and after.
7. **Export is canonical JSON** of the current player-visible state, plus journal metadata. Player exports exclude GM data by default (ADR 0003). An export is not a raw database file copy.
8. **Golden tests** use [examples/ash-veil.player-state.v1.json](../../examples/ash-veil.player-state.v1.json):
   - Import → export → re-import is lossless.
   - VEIL and Calder keep `accountability: "missing"` with `injury`, `custody` and `fate` all `"unknown"`.
   - The COLD NORTH tasking keeps `status: "proposed"` and `deployment_authorized: false`.
   - Every intelligence artifact keeps its `confidence` and its `does_not_establish` list unchanged.
   - Importing the same file twice creates two independent campaigns with distinct IDs.

### 2.9 Database location (Q3)

| Context | Path |
| --- | --- |
| Development (`ASPNETCORE_ENVIRONMENT=Development`) | `<repo>/.data/greyrelay.dev.db` (git-ignored) |
| Normal local use | `~/Library/Application Support/GreyRelay/greyrelay.db` |
| Override | `Storage:DatabasePath` in configuration or the `Storage__DatabasePath` environment variable |
| Automated tests | Temporary per-test database files, deleted afterwards |

The app creates the directory if it is missing and logs the resolved path at startup. Development and real campaign data never share a file.

## 3. Alternatives considered

- **Mutable current-state tables plus a side audit log:** simpler queries, but the audit log can drift from the real state. That contradicts "the journal is authoritative" and makes replay or projection rebuild impossible. Rejected.
- **Full event sourcing with asynchronous projections:** adds eventual consistency and background processing a single-user local app doesn't need. Deferred until hosting or multiplayer.
- **A dedicated event store (EventStoreDB, Marten):** EventStoreDB is a separate server, which is awkward without Docker. Marten requires PostgreSQL. Both conflict with ADR 0001. Rejected for S1.
- **Import by mapping the document straight into tables:** faster, but loses the verbatim original and makes lossless roundtrip unprovable. Rejected.

## 4. Consequences

- Every state change becomes an event type plus a projector. That costs more upfront, but gives audit, correction and replay for free.
- Projection rebuild doubles as a consistency test and a recovery tool.
- The SQLite/PostgreSQL divergences listed in ADR 0001 now matter specifically for sequences, triggers and JSON columns. The S1 integration suite stays provider-agnostic.
- The event catalogue becomes a shared contract between the implementation agent and the story-canon agent.

## 5. Rollout

S1 PRs in order. Each is small and tested:

1. Journal tables, triggers, the append/read port and the SQLite adapter.
2. The decide/evolve refactor of `Campaign`, plus the command pipeline with revision and idempotency checks.
3. The projection rebuild command and its test.
4. Import dry run, import, and export, with the golden ASH VEIL tests.
5. A blank second campaign, proving no pilot-specific assumptions.

No existing data needs migrating; S0 persisted nothing.

## 6. Reversal conditions

- **Projection rebuild** becomes too slow on realistic journals → add snapshots (the journal stays authoritative).
- **Multi-user hosting** is approved → revisit async projections alongside the PostgreSQL ADR.
- **The story-canon agent or owner** finds the catalogue governance too heavy → relax review to canon-touching types only (already the scope), or move to a lighter checklist.

## 7. Open questions

| ID | Question | Blocks |
| --- | --- | --- |
| ~~Q2.1~~ | **Closed as moot (2026-10-28):** the AI is the sole GM (ADR 0003). Rulings entered by the owner are player-visible, and GM-only content lives in the separate GM database, not this journal. The journal therefore has no hidden events, and revision numbers have no hidden gaps. Reopen if GM-only journal events are ever introduced. | — |
| Q2.2 | Should an imported campaign's genesis event keep the source file's `revision` value, or always restart at sequence 1 and record the original revision as metadata? Recommendation: restart at 1 and keep the original as metadata. | S1 step 4 |

## 8. Acceptance checks

- [ ] Save → shut down → restart reproduces the same projections and identical journal.
- [ ] Rebuilt projections equal live projections.
- [ ] A stale `expected_revision` returns 409 and changes nothing; a repeated idempotency key returns the original result.
- [ ] `UPDATE`/`DELETE` on `campaign_events` fails at the database level.
- [ ] A correction leaves the original event intact and linked.
- [ ] ASH VEIL golden tests (§2.8 item 8) pass.
- [ ] Malformed, oversized, unknown-version or GM-secret-bearing imports fail with actionable errors and write nothing.
- [ ] Two campaigns never read or write each other's events.
