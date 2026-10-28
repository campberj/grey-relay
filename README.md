# GREY RELAY

**A persistent, campaign-agnostic tactical operations center, intelligence investigation game, and fictional cyber-operations simulator for narrative tabletop wargaming.**

> **Project status: S1 in progress.** This repository contains documentation, example data and a runnable local skeleton (API health route, empty Campaign Library, placeholder Operations Center) with an append-only SQLite event journal underneath. There are **no campaign features or gameplay yet**. To run it, see [CONTRIBUTING.md](CONTRIBUTING.md). Features and technical choices described as proposed are not implemented or approved rules. The first playable pilot is **Operation ASH VEIL**, a persistent solo BLKOUT campaign. The engine must also support future campaigns in other settings and game systems.

**Repository:** https://github.com/campberj/grey-relay  
**Owner / product authority:** campaign player and project owner  
**Primary user:** solo tabletop commander; optional human or AI-assisted GM  
**First game module:** GREY RELAY — a fictional military signals-analysis and network-investigation console.

## Developer / agent orientation

Read in this order:

1. **README.md (this file)** — vision, feature requirements, established campaign situation, scope and constraints.
2. [docs/CAMPAIGN_STATE_ASH_VEIL.md](docs/CAMPAIGN_STATE_ASH_VEIL.md) — careful player-visible chronology, personnel, intelligence, uncertainties, next orders. **This is continuity-critical.**
3. [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) — suggested domain and technical architecture, save/event handling, trust boundaries, API design.
4. [docs/CONTENT_CONTRACTS.md](docs/CONTENT_CONTRACTS.md) — versioned campaign-pack, save, evidence and playable-scenario data contracts.
5. [docs/GREY_RELAY_GAMEPLAY.md](docs/GREY_RELAY_GAMEPLAY.md) — first interactive investigation, requirements, fair-puzzle and fictional console specification.
6. [docs/ROADMAP.md](docs/ROADMAP.md) — incremental milestones, acceptance criteria and first developer tasks.
7. [docs/AGENT_HANDOFF.md](docs/AGENT_HANDOFF.md) — multi-agent collaboration, source hierarchy, test expectations, unresolved decisions.
8. [examples/ash-veil.player-state.v1.json](examples/ash-veil.player-state.v1.json) — **illustrative, player-visible** portable state, not a hidden GM save.

**Important:** do not confuse this repository with Imperator Black or the separately discussed BLKOUT: Tactical Operations Center. Related ideas are useful, but there is no shared codebase, shared database, or authorized dependency.

---

## 1. Vision and player experience

GREY RELAY provides the infrastructure for a living, persistent military science-fiction campaign. Between tabletop engagements, the player acts as a special operations commander: reviews the operational picture, tasks units, screens intelligence, makes risk decisions, conducts *playable fictional* cyber and SIGINT exploitation, plans reconnaissance, and produces mission orders. Actual tabletop engagements are resolved using the applicable tabletop rules, then faithfully reported back as after-action events.

The app is **not** just a scripted terminal minigame. Its name comes from the first playable cyber investigation, but its scope is a **reusable campaign command platform**.

### Design pillars

- **Campaign independence.** Data packs define setting, factions, rosters, rules references, maps, missions, puzzles and NPCs. No universal code path should require ABOL, VEIL, BLKOUT, or Kappa K-17.
- **One continuing world.** Decisions, casualties, source credibility, intelligence gaps, equipment, enemy alert posture and relationships survive between missions and app sessions.
- **Solo-first.** A strong one-player + GM experience, where a conversational AI can deliver narrative while the application preserves structured evidence and state. Architect so human GM / multiplayer can be added later.
- **Grounded command.** Employ useful staff-work concepts (mission analysis, CCIR/PIR, COAs, WARNORD/OPORD/FRAGO, intelligence preparation, risk, branches/sequels, and AAR) without turning the UI into paperwork for its own sake.
- **Actual player reasoning.** Inspect evidence, compare timestamps and transport metadata, assess confidence, decide access/collection priorities and author conclusions. Do not substitute a single “roll to hack” button.
- **Fairness.** Puzzle clues and validation conditions should exist before play; GM may adjudicate dynamically but cannot retroactively change a success condition to suit the desired narrative.
- **Truth discipline.** An observation, an analyst inference, a rumor, and a hidden GM truth must remain distinct. Missing does **not** mean dead, captured or recoverable.
- **Legible tabletop bridge.** Printable mission packets, round/force tracking, enemy AI standing orders and player AAR, never silently overriding official tabletop rules.
- **Portable content.** Versioned imports/exports and explicit migrations; users own the continuity of their campaigns.
- **Safety and rights.** A fictional sandbox console, never real hacking; never redistribute third-party BLKOUT rulebooks, cards, proprietary art or STLs without rights.

### Core gameplay loop

1. **Situation:** Review latest intelligence, commander's guidance, campaign resources, personnel readiness and key unknowns.
2. **Decision:** Identify the mission's purpose and priority intelligence requirements. Choose assets, method and acceptable risk.
3. **Tasking:** Issue orders to signals analysts, human sources, scouts, ISR, logistical teams, or tactical elements.
4. **Gameplay:** Perform interactive evidence/cyber investigations, dialogue, source evaluation, observation, or a physical tabletop mission.
5. **Adjudication:** Resolve outcomes consistently, with unknowns and GM-only information protected.
6. **Update:** Commit a versioned event and refresh the operational picture, orders, roster and narrative consequences.
7. **Follow-on:** Generate the next decision point based on what really happened, not a predetermined victory script.

A 15–40 minute non-tabletop session should be possible; a tactical mission can take as long as required. The first end-to-end vertical slice should be playable without miniatures.

---

## 2. Functional areas

### 2.1 Campaign library and setup
Create, import, export, archive, duplicate and resume multiple campaigns. Each campaign has its own setting/rules adapter, saved state, player-visible evidence, optional GM-held truths, narrative content packs and versioned event log. A reusable template is distinct from a played instance.

### 2.2 Tactical Operations Center (TOC)
Campaign day/time (fictional), theater/area map, unit locations (known vs estimated), current operations, decision queue, staff messages, alerts, resources, readiness and casualty summary. All metrics should explain their provenance. Support a “what changed since last briefing?” view.

### 2.3 Force roster and logistics
Stable operator identity, affiliation, callsign, team, specialties, attachments, equipment, injuries, deployments, decorations and experience **where configured by the campaign**. Distinguish WIA, MIA, known KIA, medical care pending, and recovered-but-not-yet-cleared. Track inventory and expenditures only if the active campaign has authorized those mechanics.

### 2.4 Intelligence, HUMINT and ISR
Evidence board with source type, observation time, ingestion time, source reliability, corroboration links, contradictions, analyst confidence and visibility. Requirements: PIR/IR/CCIR. Source screening, contradictory accounts, transport trails, decrypted fragments, medical records and collection gaps should be playable investigations rather than automatically solved reports.

### 2.5 GREY RELAY fictional network console
A terminal-style and accessible GUI experience operating entirely on **invented records and nodes**. Inspect messages, directories, telemetry, access logs and captured fragments; correlate clues; submit hypotheses; manage fictional exposure/trace budget; choose to preserve access or disconnect. No real endpoints or arbitrary command execution. See gameplay spec.

### 2.6 Mission planning and tabletop handoff
Compose WARNORD, OPORD, FRAGO, COA analysis, route/terrain assumptions, enemy estimates, triggers and abort criteria. Prepare 2-page mission-specific OPORDs separate from a reusable 2-page board/solo-AI reference for printing/laminating. During tabletop play record objectives, turns, alert states, reinforcements, casualty status, and optional counters. Respect user-supplied dice/LOS/range outcomes; do not automate unsupported rules.

### 2.7 AAR, adjudication and continuity
Capture player-reported events and outcomes, reconcile contradictions visibly, record unresolved casualty fate, update campaign state with explicit events and an audit trail. Support reversible draft changes, explicit corrections, and approved/committed results.

### 2.8 Narrative and GM authoring
Authored messages, scenes, character dialogue, NPC intentions, branching consequences, trigger conditions, and secrets. Strictly separate player access from GM truth. Allow optional AI-assisted scene creation with review of canonical state changes.

---

## 3. Pilot: Operation ASH VEIL (established campaign continuity)

**Setting:** BLKOUT-inspired conflict on ABOL. The **Sable Corridor**, several named locations and all named campaign NPCs below are **original fan-campaign fiction**, *not* claims about official BLKOUT lore. The tabletop source includes official BLKOUT core/expansion materials and a separate fan-made *Tour of Duty* guide. Source/house rules must be identified by provenance and edition.

**Player element:** Task Force Boone composite detachment **RAVEN** with Recon, Marksman, strike/handler support; broader miniatures available include official BLKOUT forces and suitable **Near Present Anchorage / Hankou** models for clearly designated custom team profiles.

**Command chain / NPCs:**

| Identity | Role | Current continuity |
| --- | --- | --- |
| Major Elena Voss | Boone operational commander | Approves deployments; seeks personnel recovery without wasteful second failure |
| Captain Elias Mercer, **OVERWATCH** | Intelligence/operations coordinator and GM radio voice | Directed NEEDLE IN THE NOISE SIGINT collection |
| CWO Tessa Kade | Logistics and recovery staff | Resource/readiness support |
| Lt Col Adrian Kessler | Authority opposition commander | Security posture intensified after Habitat Nine |
| WO Ana Varga, **VEIL** | Covert Handler | **MIA** after being wounded at the Safe Flat; unconfirmed fate |
| Cpl Tomas Venn | Boone Marksman | Withdrew west, leaving wounded spotter; alive and troubled; volunteered for recovery |
| Spc Nia Calder | Marksman spotter | **MIA**, left on western rooftop; unconfirmed fate |
| **COLD NORTH** | Prospective Anchorage reconnaissance element | **Not deployed**; proposed OP/LP or electronic ISR emplacement |

**Areas:** Sable Gate (Boone FOB), Habitat Nine (first contact mission), Kappa Industrial (suspected medical/intelligence node), Dustwater Crossing, Blackglass Ridge, Relay Thirteen. These are scenario geography placeholders, not published official maps.

### Mission 001 — SILENT RESIDENT (completed, failed)
- Goal: VEIL contacts an informant at Habitat Nine and recovers intelligence; Marksman Team supports from rooftop, Recon Team QRF staged approximately 700 m away.
- Covert approach collapsed following a **Local Watchers** event and Authority alarm. VEIL killed two riflemen with grenade fire, then was incapacitated at the Safe Flat in Round 4. Her fate remains unresolved.
- One Marksman spotter, **Nia Calder**, was downed by Pointmen in Round 3. Surviving marksman Venn withdrew to the west and **did not spend actions to recover her**.
- Boone Recon QRF fought through; SAW gunner eliminated multiple riflemen and was hit, then **recovered and evacuated** by Recon Team Leader. Remaining Recon withdrew south.
- Enemy casualties tracked: **five UN riflemen and one Pointman eliminated**; a Pointman survived. Additional Reserve QRF entered from **east** during the withdrawal.
- No informant contact or recovered mission intelligence. **Mission failure**, but orderly withdrawal prevented more losses.
- Two MIA (Varga, Calder), one recovered wounded SAW gunner, others retreated. Neither MIA may be silently marked KIA or captured.

**Design correction for future stealth:** initial Pointmen were guarding within 4 inches of the Flat while informant contact required no enemy within 6 inches; this made nominal stealth success highly unlikely. Local Watchers went straight to forced alarm without player counterplay. Future covert missions must distinguish suspicion / investigation / identification / alarm and have a reachable noncombat path.

### Campaign Day 02 — NEEDLE IN THE NOISE (completed as narrative SIGINT)
- A medical transport apparently left Habitat Nine **2050Z** and a compatible-but-unconfirmed vehicle passed Kappa Industrial Gate Four **2116Z** toward **Annex K-17**.
- K-17 issued a restricted request for **two trauma-stabilization modules 2124Z**. That count **does not prove the patients were Varga/Calder**.
- Annex communications appear associated with UN 3rd Battalion / regional security medical coordination.
- Partial restricted message: **SPECIAL HANDLING / TRANSFER HOLD / MEDICAL CLEARANCE PENDING**; identity redacted and connection to MIA not proven.
- Original informant credentials flagged **1756Z**; service corridor restrictions enacted **1842Z**, before RAVEN's **1930Z** infiltration. Possible source compromise; **not confirmed**. Civilian informants require renewed validation; no proof local civilians intentionally betrayed Boone.
- Current best lead: **Annex K-17**. Not confirmed as a detention center or that either MIA is alive/present.
- **COLD NORTH** requested as later reconnaissance unit for OP/LP vs ISR installation depending on atmosphere, visibility and Authority posture. **No recon battle or mission authorized.**
- **Next activity proposed:** playable **GREY RELAY** SIGINT/evidence investigation to refine COLD NORTH tasking; no tactical deployment and no rescue automatically awarded.

**Current provisional campaign metrics:** Day 02 ~1845Z; intelligence quality **2/5**, Authority alert **4/5**, logistical support **3/5**, civilian confidence **1/5**. These are **campaign bookkeeping only**, never BLKOUT stats. Specific timing in the narrative has occasional inconsistencies; preserve source timestamps as presented and flag any proposed retiming for GM review.

### Current user direction
- Prioritize **realistic special-operations staff processes, decision points, assignments and analysis**.
- Make non-tabletop investigations genuinely **interactive games**.
- Use all available fictional EWAR personnel for a defensible actionable intelligence product; **screen and corroborate HUMINT** carefully after Habitat Nine.
- Do **not** auto-deploy COLD NORTH. User will decide reconnaissance mission, OP/LP vs sensor emplacement after examining environment/security.
- Keep Varga, Calder and original informant unresolved until an earned intelligence/action outcome.

Detailed record and confidence table: [docs/CAMPAIGN_STATE_ASH_VEIL.md](docs/CAMPAIGN_STATE_ASH_VEIL.md).

---

## 4. Strict system invariants

1. **Campaign-specific fiction belongs in data**, not engine code.
2. **Separate observed fact, source claim, analytic assessment, assumption and GM-only truth.** Unverified leads must not be rewritten as certainty.
3. **Track provenance.** Who reported it? Source, timestamp, evidence ID, confidence and status.
4. **Record events and corrections explicitly.** No invisible casualty, XP, resource or mission outcome updates.
5. **No unsanctioned rules.** Official BLKOUT rules and March 2026 Supplemental supersede superseded rules only where genuinely applicable. Fan *Tour of Duty*, mission special rules and invented custom units are separately labeled and opt-in.
6. **No inaccessible spoilers in player responses.** Hidden truths must remain on an authorized server/GM storage boundary, not merely hidden with CSS in the client.
7. **Puzzles must have solvable paths and failure branches.** Do not move goalposts after a player chooses.
8. **The campaign's event log is authoritative** for committed outcomes. User reports and GM adjudication are preserved with corrections linked to originals.
9. **Randomness must be replayable** when used: record seed, content version and choices; never fabricate die rolls.
10. **Every save is portable** with defined schema version, import validation and backup/restore.
11. **No real hacking.** The network console runs an allowlisted fictional command interpreter and acts only on local campaign objects. No real terminals, servers, shells, credentials, scanning or network access.
12. **No rights assumptions.** Do not commit third-party PDFs, unit cards, STLs, proprietary text, or artwork without permission.
13. **Meaningful noncombat choices.** Tactics, deceptions, source verification, collection method and disengagement decisions must matter.
14. **Readable and printable.** Keyboard navigation, contrast, mobile-ish responsive behavior, and 2-page mission output.
15. **No fake implementation status.** Proposed architecture is not shipped software until accepted and tested.

---

## 5. Suggested technical direction (open for review)

**Recommendation, not settled decision:** React + TypeScript browser client, ASP.NET Core/C# API (familiar to the owner), PostgreSQL database, JSON Schema validation for portable content, a deliberately constrained fictional-console interpreter, and pure deterministic domain functions covered by tests. A cohesive TypeScript stack is also acceptable if approved. Must work on **macOS without Docker**, using local tools (PostgreSQL via Homebrew if selected).

A multi-campaign domain could contain:
- Campaign, CampaignTemplate, ContentPack, SystemAdapter, Membership/Role.
- TheaterSector, Faction, UnitInstance, Operator, Injury, ResourceLedger.
- Operation, Mission, Objective, Tasking, Order, AAR.
- IntelligenceRequirement, Source, EvidenceArtifact, Assessment, Hypothesis, CorroborationLink.
- FictionalNetworkScenario, Node, Artifact, AccessRule, TraceState, PuzzleSession.
- NPC, Scene, Message, DecisionPoint, BranchTrigger.
- Immutable CampaignEvent and materialized CampaignProjection.

**App boundaries:** browser UI → authorization/API → campaign event service and validated domain functions → relational database; content pack import/export and printable rendering as separate service layers. Never send GM-only secrets with the player view of a campaign. See [architecture](docs/ARCHITECTURE.md).

**First useful vertical slice:** load the example ASH VEIL player state; display campaign situation / personnel statuses and K-17 evidence; launch one fair fictional SIGINT investigation; submit a confidence-rated assessment; create (not deploy) a COLD NORTH collection task; save, quit and resume. Demonstrate a second blank campaign to prove no pilot-specific engine assumptions.

### Proposed UI sections
**Campaign Library** · **Operations Center** · **Intel Board** · **GREY RELAY Console** · **Roster** · **Missions & Orders** · **After Action** · **GM Desk** · **Settings**

For MVP, implement only the essential screens rather than many empty tabs. Suggested first flow: **TOC → evidence → console → assessment → COLD NORTH draft tasking**.

### Example entirely fictional console
~~~text
GREY RELAY / UMP TACTICAL ACCESS CONSOLE
SIMULATED NETWORK: AUTHORITY / KAPPA INDUSTRIAL
ACCESS: PASSIVE COLLECTION

> help
  help | sources | inspect <artifact-id> | correlate <id> <id>
  hypotheses | assess <hypothesis-id> <rating> | report | disconnect

> sources
  TLM-017   Diagnostic fragment
  GATE-004  Gate Four access event
  MED-211   Restricted medical requisition
~~~
*Illustrative UI only; not shipped commands or a solved scenario. No real network requests are made.*

---

## 6. First-game design: GREY RELAY

**Briefing:** RAVEN wants to determine whether recovered casualty transport data and K-17 activity support a follow-on reconnaissance task. There is insufficient information to claim a rescue location.

**Inputs:** pre-authored simulated telemetry, gated logs, medical material requisitions, partial encrypted communications and a limited collection budget. Some artifacts should be misleading/irrelevant yet internally consistent.

**Playable challenge:** inspect records, identify which timestamps and identifiers truly correlate, distinguish independent corroboration from repeated reporting, rate a hypothesis and state what evidence would disconfirm it. An explanation is more important than simply choosing a “correct” answer.

**Outcomes:** a good assessment yields a defensible COLD NORTH PIR/collection plan; an overconfident or incomplete analysis produces an explicit intelligence gap and may add risk. It must **not** retroactively decide that Varga/Calder are prisoners.

**Future extensions:** sensor-installation unlocks, deeper invented access levels, fictional detection traces, source interview, transport itinerary reconstruction, composite HUMINT/SIGINT exercises, OP/LP interactive observation, captured-document exploitation, and optional table missions. Details: [docs/GREY_RELAY_GAMEPLAY.md](docs/GREY_RELAY_GAMEPLAY.md).

---

## 7. Development stages

| Stage | Output | Exit criteria |
| --- | --- | --- |
| **D0: Handoff** | README, continuity, architecture, data contract, gameplay, roadmap, working agreement | Reviewed by owner, clear proposed vs established |
| **S0: Foundation** | Agreed stack, runnable local web/API skeleton, formatting/testing/CI | Build/start/test on Mac without Docker |
| **S1: Persistence** | Multi-campaign save/load, schema migration, events, import/export | Restart, restore, optimistic-concurrency tests |
| **S2: TOC and Intel** | ASH VEIL roster, MIA status, evidence confidence, orders | Correct data and privacy of GM truth |
| **S3: GREY RELAY game** | Fictional console, puzzle, scoring and report | Full playthrough, failure path, no real execution |
| **S4: Tabletop/AAR** | Mission packet, status/AI tracker, audited AAR | Print 2-page OPORD; events reconcile correctly |
| **S5: Authoring** | Pack editor, branching narrative, content migration | Validation, safe packaging, campaign-independent templates |
| **S6: Scale** | Multiplayer groundwork, more game adapters, responsive UX | Campaign role isolation, robust upgrades |

See [docs/ROADMAP.md](docs/ROADMAP.md) for actionable tasks and acceptance criteria.

---

## 8. Developer rules and collaboration

- Branch from **main**, create focused commits and a PR; do **not** merge without owner review.
- Treat campaign continuity as a maintained source of truth; request approval before irreversible story events, retcons, prisoner status or new rules.
- Tests must cover event reductions, clue validation, import rejection, player-vs-GM visibility and simulated-console safety.
- No copyrighted BLKOUT content or third-party assets in this public repo by default; use metadata/reference links.
- Keep a record of unresolved decisions in PRs / ADRs. Separate **implemented**, **planned**, **proposed**, and **requires owner decision**.
- Avoid taking an infrastructure shortcut that requires Docker. Native Mac workflows should be first-class.
- Two agents can collaborate: one leads application code and tests; one maintains story canon and approves game outcomes. Exchange changes through versioned contracts and reviewed PRs.
- Do not hard-code challenge answers into public UI payloads when spoilers matter; player view and GM-only validation must be separate.

**Immediate suggested next PR:** S0 — agree on backend/runtime and create executable app skeleton with test/CI commands. Another agent can concurrently refine content-schema validation and seed data, provided both use the same schema version.

---

## 9. Source materials, licensing and safety

The player has reference PDFs for official **BLKOUT Universal Rules**, **March 2026 Supplemental**, **Impact**, **Conquest**, **Labyrinth**, UN/Task Force Boone/Harlow/Manticor unit cards, and the **fan-made Tour of Duty Campaign Guide**. These reside in the source ChatGPT campaign environment and are **not committed to this repository**. The game engine should store rule source citations and allow user-managed attachments rather than redistributing commercial material.

This is an unofficial, fan-created companion project; no affiliation or endorsement by Enemy Spotted Studios, BLKOUT, Near Present or other rightsholders is claimed. **No repository software license has yet been selected**; do not assume third-party rights.

GREY RELAY's “hacking” is a **game about invented Authority systems**. No real host, external network, account, password, scan, exploit chain, or arbitrary code execution belongs in its runtime.

---

## 10. Definition of the first playable release

A player launches the app locally, chooses the persistent ASH VEIL campaign, sees that **VEIL and Calder are MIA with uncertain fate**, inspects a dossier explaining the K-17 lead, plays an evidence/correlation puzzle in the fictional console, submits a justified assessment, receives a fair debrief, **drafts** (does not automatically execute) a COLD NORTH reconnaissance assignment, exits, and returns to find all previous state and decisions intact. A second independent campaign can be created or imported without changing code.

**North star:** *The player commands the operation. The platform preserves the world, exposes uncertainty honestly, makes intelligence work playable, and enables the GM to keep decisions consequential.*
