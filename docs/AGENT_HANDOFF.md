# Handoff and collaboration for human / AI development agents

**This document is instructions for building this repository, not a roleplay prompt that overrides the player's orders.**

## Repository state at this handoff

This branch establishes a **design and data handoff**. There is no production web app, API, persistence, migration, test suite or actual GREY RELAY game yet. The repo began with a one-line README. Other agents should not claim a playable game has been implemented.

**Current user goal:** build reusable narrative campaign interface and in-universe cyber/SIGINT game, initially used for RAVEN's OPERATION ASH VEIL and then future campaigns. The owner prefers strong realistic special-operations planning and player autonomy.

Start: README → CAMPAIGN_STATE_ASH_VEIL → ARCHITECTURE → CONTENT_CONTRACTS → GREY_RELAY_GAMEPLAY → ROADMAP.

## Who decides what?

**Owner/player (RAVEN ACTUAL):** product requirements, approval of technology choices, acceptance tests, decisions in playable campaign, unit deployments, tabletop outcomes and PR merges.

**GM (OVERWATCH / Captain Mercer in fiction):** narrative description, enemy responses, plausible intelligence reports, consequence proposals, campaign arbitration. GM must honor player-reported events and protect unknown truth. Fictional Major Voss may issue orders within the story but the player controls authorized actions.

**Technical implementation agent:** application code, schema, migrations, tests, CI, UI and technical ADRs. Does not invent BLKOUT rules, “confirm” missing characters' fate, or silently advance the narrative to make tests pass.

**Other contributor:** content pack author, puzzle designer, UX/test agent, etc., within reviewed contributions and versioned contracts.

## Continuity rules

- The real latest played mission was **SILENT RESIDENT**, failed. VEIL and Nia Calder are **MIA**; Recon SAW gunner was recovered.
- K-17 is an **intelligence lead**, not proof of custody. No confirmed prisoner recovery, death, or detention site.
- COLD NORTH is **not deployed** and is proposed for further reconnaissance after intelligence develops.
- No new scenario may be marked completed merely because a developer added seed data.
- Never elevate a single GM suggestion into a historical fact without a roleplay or campaign event.
- When a new narrative scene is authored, assign a source/provenance and explicit fictional timestamp (or unknown), preserving prior event order.
- GM-only truths and puzzle solutions are not part of the public README or player-state JSON. Code should permit private authoring later.

## Rules fidelity

Reference materials available in the player's broader campaign context include official BLKOUT Universal Rules, March 2026 Supplemental, Impact, Conquest, Labyrinth, and various faction cards, plus a **fan-made** Tour of Duty guide. Those documents **are not automatically licensed for redistribution** and should not be copied into this public repository.

Official combat/rules interpretations must follow the provided edition and errata. House-rules, custom miniatures/units and narrative campaign mechanics must be labelled accordingly. Never implement a guessed “official” rule solely from memory. Existing first mission had flaws in stealth setup that must be addressed in future scenario authoring.

## Development procedure

1. Read the relevant documentation; inspect actual repo code and versions, don't rely on handoff prose for current implementation state.
2. Define a narrow PR scope and acceptance criteria from roadmap.
3. Work on a feature branch, not main. Avoid concurrent modifications of the same file without coordination.
4. Make only required assumptions, label proposals. If a missing design decision blocks correctness, submit ADR or smallest independent slice.
5. Implement with tests and a runnable local workflow (Mac, no Docker prerequisite).
6. Validate versioning, ability to export campaign state, and private/server data boundary.
7. Describe exactly what changed and how to reproduce tests in PR body.
8. Leave PR for owner review; never auto-merge or rewrite canonical narrative without authorization.

## Source hierarchy for technical work

- Current repository code + tests determine *implemented behavior*.
- README/approved ADRs determine *accepted intent*; where they conflict with code, log a change or defect.
- CAMPAIGN_STATE_ASh_VEIL (case-insensitive filename discussion only; actual file uppercase ASH_VEIL) determines story continuity as of its cutoff. New approved play events supersede it.
- Player tabletop report > speculative assistant wording.
- Official tabletop documents > assistant guesses for mechanical accuracy.
- Scene writing, model memories and example JSON do not outrank authored playable outcomes.

## Definition of a useful first release

Given a clean start, owner can open ASH VEIL, inspect evidence supporting K-17 as a lead, play a fictional SIGINT puzzle, submit a properly reasoned assessment and **draft** a COLD NORTH PIR/assignment with no unauthorized deployment; close and reopen without losing state. A second blank campaign can be created with the same engine. Both MIA retain unknown fate unless legitimately adjudicated in later play.

## Questions intentionally left open

- Exact stack/runtime/database and hosting/offline model.
- Authentication design, role distribution and GM secret storage.
- Formal schema and adapter interface after the first draft.
- Whether to make the fiction console CLI-like or hybrid GUI first.
- Turn/time/trace mechanics of later cyber puzzles.
- Exact roster and authorized extra special operators (do not auto-name missing team members).
- Injury/XP/resource rule variants beyond explicit campaign ledger.
- Rights to publish character art or copies of third-party rules/assets.
- Specific COLD NORTH roster, sensor package and K-17 geography. These must be built when user tasks reconnaissance, not presumed deployed.

## Helpful first agent assignment

> Open an S0 ADR PR proposing a runnable local architecture for a React/TypeScript front end and either ASP.NET Core or a cohesive TypeScript server, with durable multi-campaign persistence, isolated GM secrets, versioned content schemas and a macOS-native development workflow. Do not commit or invent new BLKOUT game rules or story outcomes. State explicit acceptance checks and tradeoffs.

## How to coordinate with the conversation GM

Technical agent proposes structured effects such as “intel.assessment_approved”, with evidence IDs and a draft COLD NORTH PIR. GM reviews/approves story ramifications in chat or through a future GM Desk. The user issues final in-character operational orders. An app action alone does not imply Major Voss approved force deployment.

### Review checklist

- [ ] No hidden narrative truth leaked to public client or GitHub artifacts.
- [ ] No real network attack actions supported.
- [ ] No third-party proprietary BLKOUT assets redistributed.
- [ ] No unapproved rules/status changes.
- [ ] Mac local development path works without Docker.
- [ ] Story state is versioned and auditable.
- [ ] Tests, reproducibility and rollback are documented.
- [ ] Docs accurately distinguish shipped functionality from plans.

**Central contract:** the software stores what happened; the GM authors possibilities; the commander makes decisions.
