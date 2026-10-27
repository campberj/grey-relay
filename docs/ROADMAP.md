# Development roadmap and acceptance criteria

**Status:** proposed delivery plan. Owner approves each milestone through PR review.  
**Principle:** ship one useful end-to-end player experience before expanding scope.

## D0 — Documentation and handoff (this PR)

Scope:
- README product overview, functional map, user needs, first campaign continuity and definition of done.
- Architecture, content contract, scenario gameplay rules, agent handoff and example player-visible seed.
- Explicit open decisions and no premature production stack claim.

Acceptance:
- A developer new to the project can accurately explain the pilot campaign and launch the next development task.
- Every link in the README resolves on the feature branch.
- No source PDFs, proprietary artwork or GM-only secrets are committed.
- No suggestion that an app or database already exists.
- Changes are made in a branch for owner review.

## S0 — Technical foundation

**Outcome:** minimal runnable local app/API; no invented game mechanics.

Work:
1. Propose ADR for stack, local database choice and privacy boundary; owner reviews.
2. Establish repository layout, versions and lockfiles, lint/format/test commands, CI and CONTRIBUTING notes.
3. Provide starter React UI and API health endpoint if chosen.
4. Native macOS setup: document prerequisites, local launch, env example, tests and troubleshooting **without Docker**.
5. Show placeholder Campaign Library and TOC with no real campaign mutation; define health/debug command.
6. Implement a deterministic domain unit test demonstrating one independent campaign aggregate.

Acceptance:
- Fresh Mac clone follows documented commands to start in development.
- No real network/OS shell calls from the fictional console.
- Unit tests and CI pass.
- No arbitrary “backend already decided” if ADR not approved.
- Clean separation: web UI, API/domain, storage access, content.

Do not bundle a giant unfinished UI with S0.

## S1 — Campaign persistence and portable packs

**Outcome:** durable multiple campaigns and event journal.

Work:
1. Finalize v1 schema and TypeScript/C# models.
2. Create/import example ASH VEIL **player-visible** save via validated fixture.
3. Create blank unrelated campaign and prove engine is campaign-agnostic.
4. Store event history and derived projections; implement idempotency/revision conflict checks.
5. Export, import, backup, basic migration policy and failure reports.
6. Status handling: MIA is distinct from OOA/WIA/KIA; reject unearned status transitions.
7. Automated tests and restart roundtrip.

Acceptance:
- Save → shutdown → restart → same canonical state with identical history.
- Import/export roundtrip preserves unresolved personnel and intelligence qualifiers.
- Concurrent conflicting edits do not overwrite silently.
- No data leaks between campaign IDs.
- Invalid schema versions and bad references fail with actionable errors.
- Storage runs without Docker on supported development machines.

## S2 — TOC, roster, intelligence board

**Outcome:** campaign command view anchored to real saved state.

Work:
1. Surface ASH VEIL Day 02 current picture, alert/support metrics, RAVEN people and pending taskings.
2. Show evidence IDs, sources, times, confidence, inference and contradiction links.
3. Support search/filter and source lineage.
4. Draft a PIR/CCIR and save it without issuing an order.
5. Add simple communications log and distinction between source report and GM message.
6. Establish GM view privacy constraints; tests that player cannot read private rubric/secrets.

Acceptance:
- VEIL and Calder remain **MIA**, not dead or automatically prisoners.
- K-17 medical requisition does not imply named patient identity.
- All claims can show provenance and distinguish observation/inference.
- COLD NORTH shown available, **not deployed**.
- Player-only export omits future GM truth.
- UI keyboard navigation and clear uncertainty labels.

## S3 — Playable GREY RELAY

**Outcome:** first non-tabletop mission that genuinely exercises player analysis.

Work:
1. Author actual fictional artifacts for transport and medical-intelligence investigation.
2. Implement allowlisted console actions and parallel GUI controls.
3. Create scenario session, clue unlock, save/reload and action audit.
4. Collect hypothesis submissions with cited evidence and explainable debrief.
5. Implement complete/partial/inconclusive/failure-with-learning paths.
6. If sensitive hidden scoring exists, run it server-side, not in static browser JS.
7. Author change effects only for approved outputs (new intelligence entry and **draft** COLD NORTH requirement); no automatic deployment.
8. Add simulated console security tests including injection strings and forbidden URL inputs.

Acceptance:
- Player with only documented tools can reach a defensible assessment.
- Other plausible conclusions produce differentiated feedback, not arbitrary hard stop.
- Evidence trail is inspectable, and scenario results persist on restart.
- No arbitrary code, shell, credential or real host access from game.
- Unauthorized hidden artifact fetch is rejected.
- No MIA fate resolved or prisoner rescue pre-scripted.

## S4 — Tactical mission / AAR / print

**Outcome:** consistent bridge to tabletop BLKOUT and future game systems.

Work:
1. Versioned missions and rules-adapter references; source and house-rule badges.
2. Force selection, handler association, roster readiness and objective descriptions.
3. Publish US Letter landscape **2-page mission-specific OPORD** separate from reusable two-page mission board/AI sheet.
4. In-game tracker for round, alarm, objectives, CP/Armory where adapter supports them, reactions/AI guidance and casualties.
5. AAR ingestion that reflects user-report facts and staged GM adjudication; correction events for inconsistencies.
6. Optional event and reinforcement policies must be playtested for solvable stealth approaches.

Acceptance:
- PDF page count verified across fixture missions.
- Official vs fan/house rules explicit; no copied commercial unit cards.
- User's reported die outcomes never silently rewritten.
- OOA vs MIA vs KIA fidelity.
- Mid-game AAR state can be saved/resumed.
- Replay of M001 example preserves exact withdrawal/uncertain casualty disposition.

## S5 — Narrative authoring and user content packs

**Outcome:** authors create new campaigns/fictional puzzles without code changes.

Work:
1. Validated authoring workflow for scenes, missions, clue graphs and GM-only truth.
2. Trigger/condition DSL with no executable scripting.
3. Versioned content packs, import dry run, conflict/migration preview.
4. Campaign/pack licensing metadata and permission-aware assets.
5. GM control of event proposals, approvals and reveals; narrative-to-state consistency.
6. User export/restore with audit and spoiler controls.

Acceptance:
- Another campaign plays one simple cyber/intel scenario with same engine.
- Malicious/malformed packs rejected; private secrets never shipped to player.
- Pack upgrade never retroactively changes a completed investigation.
- State transitions reviewed and logged.

## S6 — Broader platform capabilities (future)

- More game systems / player force adapters.
- Multiplayer roles and campaign permissions.
- Collaborative GM/planning tools.
- Geography and visual planning interfaces.
- Optional AI-generated narrative/content under review.
- Hostable deployment and robust operational monitoring.

No dates, effort budgets or hosting obligations are authorized yet.

---

## Priority backlog: agent-ready first issues

### Issue 1 — S0 ADR: choose runtime and local persistence
**Input:** README, architecture, user .NET familiarity, Mac without Docker preference.  
**Deliverables:** short ADR with 2–3 alternatives and tradeoffs; explicit choice for approval; minimum supported versions; security assumptions.  
**Acceptance:** owner can make an informed choice; no scaffolding before decision if implementation depends on it.

### Issue 2 — S0 scaffold
**Dependency:** Issue 1 ADR approved.  
**Deliverables:** reproducible dev environment, API health route, placeholder UI, test scripts, CI.  
**Acceptance:** commands run locally and in CI.

### Issue 3 — S1 schemas + fixture validator
**Can proceed in parallel with S0 planning.**  
**Deliverables:** versioned schema, player-visible example, CLI/validation tests, explicit UNKNOWN fields, no secrets.  
**Acceptance:** example valid; malformed corpus rejected.

### Issue 4 — S1 event journal and projection
**Dependency:** chosen storage.  
**Deliverables:** revisioned events, events query, projector, conflict/idempotency tests and two campaign isolates.  
**Acceptance:** persistence, replay and correction tests.

### Issue 5 — S3 investigation content prototype
**Can be authored before UI, but not put GM answers in client files.**  
**Deliverables:** at least four coherent fictional source artifacts, a hypothesis rubric, partial/failure branches, and a neutral debrief; no unwarranted plot resolution.  
**Acceptance:** human playtest demonstrates one supported and one inconclusive path.

---

## Operating agreement for PRs

Each PR must include: scope, exact files, implemented vs proposed features, acceptance checklist, tests run/results, user-visible changes, migrations, screenshots for UI and any canon/lore impact. Review and merge belong to the owner. Never auto-merge.

**Avoid scope creep:** a “reusable campaign tool” does not require a full live video game, automated BLKOUT battles, multiplayer networking or a real hacking sandbox in its first release.

**Measure success by:** Can the user play a non-tabletop intelligence mission, learn meaningful information, issue an informed COLD NORTH tasking, and continue the same campaign without losing any history?
