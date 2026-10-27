# Architecture and technical design

**Status:** proposal for S0 review; not an implemented stack or a settled ADR.  
**Priority:** reliable campaign persistence and spoiler-safe interactive intelligence, then tabletop handoff.

## 1. Context and scope

GREY RELAY is a multi-campaign, solo-first, GM-assisted campaign system. A tabletop game resolves tactical combat externally. This application manages campaign continuity, player staff work, narrative and game-defined investigation. One optional module offers a terminal-like fictional cyber experience.

**Do not** build a real attack framework, socket command runner, or arbitrary system execution layer. Fictional nodes and verbs are application objects interpreted by pure game code.

### Context diagram

~~~mermaid
flowchart TD
  Player[Player Browser] --> API[Campaign API]
  GM[Authorized GM / Author] --> API
  API --> Auth[Visibility & Role Checks]
  Auth --> Domain[Domain Services]
  Domain --> Store[(Campaign Database)]
  Domain --> Intel[Evidence & Assessment]
  Domain --> Sim[Fictional Network Simulator]
  Domain --> Orders[Orders / AAR / Decision]
  Packs[Versioned Content Packs] --> Validator[Validation / Import]
  Validator --> Store
  Domain --> Print[Printer / Export]
  Print --> Player
~~~

## 2. Suggested stack and decision gates

**Suggested starting point:** React + TypeScript/Vite, C# ASP.NET Core API, PostgreSQL, JSON Schema Draft 2020-12 for portable documents, and tests in both JS and .NET. The owner has .NET experience; this is not a binding decision. Other acceptable approaches include Next.js/full TypeScript with an equivalent server-side authorization boundary.

**Avoid Docker as a default requirement.** Developer workflows should run locally on Mac with standard package managers and native PostgreSQL, if PostgreSQL is approved. Record architecture choices in an ADR before creating S0 scaffolding.

Open decisions:
- Backend language/runtime and minimum versions.
- Authentication for single-user local start vs internet-hosted/multi-user; whether GM needs a separate identity.
- SQLite vs PostgreSQL for development; if using SQLite for S0, do not silently assume production compatibility.
- Hosting/deployment and offline expectations; true offline GM-secret isolation cannot be guaranteed if the entire truth is shipped to the browser.
- AI integrations: optional future feature, not in S0. Never store secrets or external API keys inside imported content packs.
- Import compatibility and licensing.

## 3. Bounded contexts

**Campaign Registry:** Campaign, CampaignTemplate, Membership, RulesAdapter, ContentPack and version pinning. Independently isolate campaign A from B.

**Operational Picture:** Theater, Sector, Unit, Operator, Assignment, Readiness, Equipment, ResourceLedger, SituationReport.

**Intelligence:** InformationRequirement, Source, Report, EvidenceArtifact, Correlation, Hypothesis, Assessment, CollectionTask, confidence/provenance. Distinguish independent observations from repeated copies of one source report.

**Operations:** Mission, Operation, Objective, COA, WARNO, OPORD, FRAGO, AAR and Adjudication. A draft order should not act as execution.

**Fictional Cyber Game:** ScenarioDefinition, Session, FictionalNode, Artifact, AccessGate, FictionalExposure, ObjectiveEvaluation and GameAction. The interpreter is not an OS shell.

**Narrative:** Scene, NPC, Message, DecisionPoint, Trigger, Branch, GMFact and Reveal. GM-owned truth remains separate.

**Publishing:** Mission print view, reusable AI guide, exports and backups.

## 4. State, events and audit

**Recommended:** relational current-state projections backed by an append-only campaign event journal. This may be implemented as event-sourcing or transactional audit + projections; full event sourcing is not a mandated platform decision.

Each committed event should include:
- Stable event id, campaign id, aggregate id and monotonically increasing campaign sequence.
- Semantic event type and explicit schema version.
- In-universe occurrence time and independent real-world recording time.
- Actor and authority, source/mission ref, causation and optional correlation ids.
- Structured payload and content-pack/rules version references.
- Optional id of corrected/superseded event; never quietly edit history.

**Command flow:** validate current campaign revision → validate actor rights → validate payload → apply deterministic state change atomically → append audit event → update projections → return new revision. On revision conflict return a recoverable conflict; never silently overwrite another agent's changes.

**Read flow:** project to player or GM-specific DTOs → filter classified/hidden fields at the data-access boundary → return sources and confidence → render. Do not send GM truth to the browser and conceal it with CSS.

**Corrections:** Retcons, casualty reconciliation and rule clarifications are explicit correction events with rationale. Distinguish raw player AAR claims from authoritative confirmed entries. Example: “Varga incapacitated” is a fact; “Varga captured” is not unless established.

**Randomness:** For randomized events, store game-specific PRNG algorithm/version, seed, action order and result. Replaying the event stream with the same content versions must recover the same projections. GM scripted choices are not random rolls.

## 5. Database sketches (not migrations)

Candidates: campaigns; campaign_memberships; content_packs; campaign_content_versions; campaign_events; operators; unit_instances; missions; orders; mission_aar; intelligence_sources; evidence_artifacts; assessments; collection_tasks; cyber_scenarios; cyber_sessions; cyber_actions; narrative_messages; gm_secrets; exports.

Use UUID/ULID IDs, foreign keys per campaign, unique sequences, ownership/version columns and soft archival. Cross-campaign associations should be rejected unless explicitly imported/linked.

Indexes likely needed for (campaign_id, sequence), (campaign_id, status), evidence timestamps, messages and action logs. First profile on real sample data; avoid overengineering.

## 6. Content vs state

A **definition** is shareable game content: optional scenario tree, fixed clues, NPC templates, world regions, mission layouts, puzzle logic. A **save** is private/unique: chosen decisions, unlocked clues, injuries, alarm posture, messages, generated entities and outstanding orders. Definitions can be reused; a campaign instance must pin content versions so future pack updates do not silently change prior puzzle outcomes.

Content is **data**, never executable JS, SQL or C# scripts. All imported files must be size-limited, schema-validated, sanitized and versioned. Reject unknown executable instruction fields rather than evaluating them. For triggers, use an allowlisted declarative condition vocabulary; all conditions are evaluated server-side for spoiler-sensitive scenarios.

## 7. Fictional-console execution boundary

The input grammar is a small game DSL, e.g. help, sources, inspect [id], correlate [id] [id], hypotheses, assess [id] [rating], report and disconnect. Commands return modeled responses from the scenario service. Implement via a parser/dispatcher with explicit verb handling and strict argument types.

**Prohibited:** eval, Function, child_process, subprocess, spawning a shell, DNS/network probing, fetching user-supplied URLs, querying arbitrary IPs, loading executable plugins, arbitrary file reads or credential collection. UI HTML must escape untrusted strings. Tests should verify command injection strings are treated as invalid inputs/data.

An interactive cyber puzzle must have author-defined evidence dependencies, objective criteria, partial-success and loss-of-access branches. If scenario answers are spoilers, score and unlock on server, never bundle validation keys in a public JS artifact.

## 8. Permissions and secrecy

Initially identify **player**, **GM/author**, and **admin** capabilities as logical roles even if a local instance has one user. Restrict campaign scope by identity and protect GM secrets separately. An author can access unrevealed hidden facts; a player cannot. Each scenario session exposes only unlocked artifacts to that role.

Permission tests must attempt:
- Cross-campaign reads/writes.
- Guessing an undiscovered artifact ID.
- Accessing a public pack's hidden truth via player route.
- Replay of a completed scenario with modified event payload.
- Direct API mutation of mission outcome without authorization.

Private evidence must not appear in HTML, initial hydration, prefetch payloads, search indexes, downloadable backups intended for players, or client error traces.

## 9. Suggested API shape (tentative)

~~~text
GET    /api/health
GET    /api/campaigns
POST   /api/campaigns
GET    /api/campaigns/{campaignId}/toc
GET    /api/campaigns/{campaignId}/events?cursor=...
GET    /api/campaigns/{campaignId}/intel
POST   /api/campaigns/{campaignId}/assessments
GET    /api/campaigns/{campaignId}/operators
POST   /api/campaigns/{campaignId}/orders/drafts
POST   /api/campaigns/{campaignId}/orders/{id}/commit
POST   /api/campaigns/{campaignId}/aars
POST   /api/campaigns/{campaignId}/cyber-sessions
POST   /api/campaigns/{campaignId}/cyber-sessions/{id}/actions
POST   /api/campaigns/import/validate
POST   /api/campaigns/import
GET    /api/campaigns/{campaignId}/export
~~~

Each mutating operation should accept an idempotency key and expected campaign revision as appropriate. An action endpoint returns newly **revealed** evidence and updated session state, not all potential clues. Actual routes need an ADR.

## 10. User interface goals

MVP navigation: **Operations Center → Intel Board → GREY RELAY Investigation → Assessment → Draft COLD NORTH Tasking**. Roster access can be a panel rather than a separate entire app for S2. Represent unknowns unambiguously. Source links/IDs should be inspectable. Distinguish actor quotes, source reports and the AI GM's newly proposed fictional events.

Accessibility: keyboard command input, focus management, transcript, no time-gated reflex requirements, contrast-checked displays, no color-only indication, scalable text, responsive layouts. Print on US Letter landscape and verify true page counts.

## 11. Testing strategy

**Domain:** invariant tests for MIA/KIA separation, causal event order, no unearned resource consumption, explicit override of fan rules, same seed + content gives same scenario result.

**Intel:** confidence cannot promote a hypothesis to observed fact; evidence citation resolution, source duplication awareness, insufficient data as valid outcome.

**Cyber:** parser unit tests, forbidden-string tests, replay, objective scoring, session state transitions, hidden clue non-disclosure, stale/concurrent actions.

**Content:** import/export roundtrip and schema validation, invalid size/type, unknown version, malicious content and missing references.

**Integration:** save/restart/load, multi-campaign isolation, authorized player and GM queries, concurrency conflict, migrated state.

**End to end:** load Ash Veil → inspect K-17 lead → solve or partially solve first investigation → draft but **do not deploy** COLD NORTH → restart → state preserved. Second unrelated campaign should work using generic engine.

**Print:** 2-page scenario document with repeatable screenshot/PDF regression checks; mission-specific packets separate from reusable AI reference.

## 12. Initial repository organization (proposal)

~~~text
README.md
AGENTS.md
docs/
  ARCHITECTURE.md
  CAMPAIGN_STATE_ASH_VEIL.md
  CONTENT_CONTRACTS.md
  GREY_RELAY_GAMEPLAY.md
  ROADMAP.md
  AGENT_HANDOFF.md
examples/
  ash-veil.player-state.v1.json
schemas/
  campaign-player-state.v1.schema.json   (future)
src/
  web/                                   (S0 onward)
  api/                                   (S0 onward)
tests/                                    (S0 onward)
~~~

Do not create empty implementation directories merely for aesthetics. Add tests and scripts when the actual stack is selected.

## 13. ADR template

Every consequential architecture choice should state: **Context; Decision; Alternatives; Consequences; Rollout/Migration; Reversal Conditions; Date/Owner**. Current open ADRs: backend/database, offline/hosting, player/GM access, event journal design and cyber content secrecy.

The owner has explicitly requested a developer handoff that another agent can implement from. Where a decision is uncertain, annotate it; do not invent the decision or describe nonexistent features as live.
