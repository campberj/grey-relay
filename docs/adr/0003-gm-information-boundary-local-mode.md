# ADR 0003 — Protecting GM-only information in local single-user mode

**Status:** Proposed — awaiting owner review in PR.
**Date:** 2026-10-27
**Owner / decider:** project owner (RAVEN ACTUAL)
**Implementer:** technical implementation agent
**Story-continuity review requested:** §2.1 and §2.2, from the story-canon agent
**Resolves:** ADR 0001 open question Q1 (how the local single user acts as GM)

Labels follow ADR 0001:

- **[OWNER]**: decided by the owner.
- **[PROPOSED]**: the implementer's recommendation, pending approval of this PR.
- **[OPEN]**: undecided.

Everything below is **[PROPOSED]** unless marked otherwise.

## 1. Context

Hidden truths and puzzle answers must stay on an authorised server/GM storage boundary, "not merely hidden with CSS in the client". They must never appear in HTML, hydration, prefetch payloads, search indexes, player backups or client error traces (README invariant 6; ARCHITECTURE §8).

ADR 0001 fixed **local single-user mode with logical roles and no login** [OWNER]. That leaves a real tension: on one machine, nothing technically stops the owner from opening their own files.

Today, GM truth for ASH VEIL lives **outside the app**, with the conversational GM (OVERWATCH). The committed example state is player-visible only (`contains_gm_secrets: false`). The story-canon agent asked that this boundary be settled before S1/S2.

**The conversational AI is the GM exclusively** [OWNER, 2026-10-27]. The owner is only ever the player, so the app has **no human GM user**. Nothing needs a "GM mode" switch that would let the owner see GM data.

## 2. Decision

### 2.1 Threat model: prevent accidental spoilers, not a determined owner

In local mode, the boundary exists to stop GM-only information reaching the player **by accident**:

- through the UI, API responses, the JavaScript bundle or exports
- through logs, error messages, or the public repository

It does not try to stop the owner deliberately reading their own disk; that is impossible without real authentication, which is deferred [OWNER]. The design must still be the same shape that later hosting with real auth needs, so nothing gets retrofitted.

### 2.2 Staged scope

| Stage | GM-only data stored in the app | GM routes |
| --- | --- | --- |
| **S1–S2** | **None.** The app holds only player-visible state; GM truth stays with the conversational GM. Player-state imports with `contains_gm_secrets: true` are rejected (ADR 0002 §2.8). | Do not exist |
| **S3** (GREY RELAY puzzle) | Puzzle rubrics, hidden artifacts and unlock conditions, needed for **server-side** scoring | Not needed. Scoring runs inside the player's API call and returns only the result. |
| **S5** (authoring) | Authored GM facts, scene secrets, reveal conditions | `/api/gm/*`, disabled by default (§2.4), **only** if the AI GM is later integrated with the app |

Nothing is built ahead of its stage. Because the AI is the only GM [OWNER], **there is no human GM Desk screen** in the player web app. ROADMAP/README mentions of a "GM Desk" are reinterpreted as machine access for the AI GM, if that is ever approved.

### 2.3 Storage: a separate GM database file (from S3)

- **Separate file.** GM-only content lives in `greyrelay.gm.db`, next to the main database (ADR 0002 §2.9). This is a separate file, not just separate tables in the same file.
- **Separate repository interfaces.** GM data is reached only through GM-scoped repository interfaces in a dedicated Application namespace. Player-facing handlers cannot resolve them, and an architecture test enforces this.
- **Exports are separate.** A player export or backup reads only the main database, so it cannot include GM data by construction. A GM backup is a separate, explicitly named command.
- **Reveals go through the main journal.** A reveal writes a player-visible event to the main journal (ADR 0002), referencing the GM fact's ID, and is idempotent. The main journal is authoritative for *what has been revealed*. The GM file only records *what exists*.

This extends ADR 0001 §2.4, which specified separate tables, by moving those tables into their own file.

### 2.4 GM access (from S5, only if AI GM integration is approved)

These routes would serve a **machine client** acting for the AI GM, never a human GM screen.

- **Off by default.** `/api/gm/*` routes are registered only when the API starts with `GameMaster:Enabled=true`.
- **A token is required when enabled.** On first enable, the app generates a random per-install token and stores it at `~/Library/Application Support/GreyRelay/gm.token` with file mode `0600`. GM requests must carry it in a header, and the player UI never reads that file.
- **The token stops accidents, not attackers.** It keeps a stray player-side request or link from reaching GM data. It is not authentication, and the hosting ADR replaces it with real identity.

### 2.5 Verification

- **Canary secret.** GM test fixtures contain a unique canary string. Tests assert that it never appears in:
  - any player API response
  - error bodies
  - player exports
  - captured logs
  - the built web bundle
- **Undiscovered artifacts stay hidden.** Requesting an undiscovered artifact by a guessed ID returns the same `404` as a nonexistent one, so the response doesn't reveal that it exists.
- **Logs stay clean.** Structured logging never writes GM payloads; redaction is applied at the logging boundary.
- **Static checks.** Architecture tests stop player endpoint types from referencing GM ports, and stop player DTOs from carrying GM fields.

## 3. Alternatives considered

- **Same database, separate tables (ADR 0001 baseline):** simpler transactions, but a raw database copy or a careless query/export path can include GM data. Kept as the fallback if two-file consistency proves troublesome.
- **A separate GM application or process:** the strongest separation, but roughly double the build and run surface for a single user. Revisit for hosting. With no human GM [OWNER], a separate GM app has no user to serve.
- **No GM data in the app ever (always external GM):** the simplest option, but S3 needs server-side rubric scoring for fair, precommitted puzzles (GREY_RELAY_GAMEPLAY §6). That requires storing the rubric somewhere the player client cannot read.
- **Real authentication now:** contradicts ADR 0001 [OWNER] and adds no protection against the machine's owner.

## 4. Consequences

- S1 and S2 carry no GM-secret risk, because there are no GM secrets to leak.
- From S3, every new feature touching hidden content needs a canary test.
- Two SQLite files mean two backups, and reveal consistency rests on idempotent reveal events.

## 5. Open questions

| ID | Question | Blocks |
| --- | --- | --- |
| ~~Q3.1~~ | **Resolved [OWNER, 2026-10-27]:** the conversational AI is the GM exclusively. There is no human GM user or GM Desk screen (§1, §2.2). | — |
| Q3.2 | How does an AI-authored S3 puzzle rubric reach the private GM file without the owner (the player) reading it, and without passing through the public repository? Recommendation: the AI GM supplies a private content pack file, which the owner imports with a command-line import that validates it **without printing its contents**. The file is never committed. | S3 |
| Q3.3 | GM rulings happen in chat. Should the owner enter them into the app as events, with `actor.kind = gm` and `authority` citing the chat ruling, or should the app only record player-reported outcomes until AI GM integration exists? | S4 (AAR) |

## 6. Acceptance checks

- [ ] S1/S2: rejecting `contains_gm_secrets: true` imports is tested.
- [ ] From S3: canary tests (§2.5) pass for responses, errors, exports, logs and the bundle.
- [ ] From S3: a guessed undiscovered artifact ID gets the same 404 as a nonexistent one.
- [ ] From S5, if AI GM integration is approved: with `GameMaster:Enabled` unset, `/api/gm/*` returns 404. With it set but no token, requests return 401.
- [ ] The player web app contains no GM screen, route or GM-data UI code.
