# GREY RELAY gameplay specification

**Status:** proposed first playable vertical slice. **Not yet an executed story event or working game.**

## Narrative fit

RAVEN's Day 02 SIGINT assessment points to **Kappa Industrial Annex K-17**. WO Ana Varga (VEIL) and Spc Nia Calder remain missing after Habitat Nine. The user has ordered credible intelligence development and HUMINT source screening before sending the Anchorage-derived **COLD NORTH** recon element to an OP/LP or forward ISR emplacement.

The first playable experience should answer **“what can we actually support from these intercepts, and what should COLD NORTH collect?”** It should **not** grant direct access to a military detainee registry, confirm the missing people's fate, or deploy COLD NORTH on its own.

## 1. Mission design goals

- Real **investigation**, not a button that advances a scripted plot.
- 10–30 minute first run; repeatable as an authored training scenario but story decisions only committed once.
- Multiple defensible outcomes: supported hypothesis, incomplete evidence, correct rejection of an overclaim, and credible partial success.
- Three or more fictional artifacts; at least one ambiguity and one tempting but invalid leap of inference.
- Feedback states the evidence used and exactly what remains unresolved.
- Successful findings unlock **draft** PIRs/tasking, not automatic tactical action.
- Use fictional-system interaction. All commands and addresses are inert game data, not actual networking.

## 2. Example GREY RELAY operator workflow

~~~text
UMP INTELLIGENCE SECTION / GREY RELAY
CAMPAIGN: ASH VEIL
SUBJECT: KAPPA K-17 / TRAFFIC ANALYSIS
ACCESS: PASSIVE / AUTHORIZED

> help
  sources, inspect [id], correlate [a] [b], hypotheses,
  assess [hypothesis] [confidence], report, disconnect

> sources
  TLM-017   HABITAT NINE TRANSIT TELEMETRY
  GATE-004  KAPPA GATE FOUR CONTROLLER
  MED-211   RESTRICTED MATERIAL REQUISITION
  MSG-052   FRAGMENTARY ADMINISTRATIVE SIGNAL

> inspect GATE-004
  [DISPLAY RECORD ACCORDING TO AUTHORIZED CONTENT]
~~~

The **syntax** is illustrative and may change via ADR. No complete future GREY RELAY evidence/secret outcome is pre-revealed here.

A user should also be able to perform everything by keyboard-accessible GUI controls. Terminal and GUI invoke the same actions and use the same audit log.

## 3. Three intertwined puzzles

### Investigation A — Transport correlation
The player examines a reported **2050Z** Habitat Nine departure, a **2116Z** Kappa Gate Four passage and fragmentary subsystem signatures. They must distinguish:
- confirmed passage of *a* vehicle;
- evidence that two records plausibly refer to the same vehicle;
- proof that the vehicle carried two named missing personnel.

Those are **three different claims**. A good result can be “probable route, passengers unknown.”

### Investigation B — Medical / detention inference
A **2124Z** request for two trauma modules and a fragmentary **medical clearance pending** message are relevant but do **not** prove two specific missing people are alive at K-17, or that the annex is a prison. The player identifies which hypotheses are supported, possible or unsupported.

### Investigation C — HUMINT contamination
The original informant's credentials were flagged at **1756Z** and corridor controls changed at **1842Z**. The user should distinguish pre-existing Authority suspicion from a confirmed security leak; analyze whether two reports share an originating source and request independent verification. No character is declared a traitor because of matching times.

These are thematic inputs, not invented proof of enemy behavior. Any first playable pack needs authored raw records before the exercise runs.

## 4. Objectives and debrief

First objective: produce a short analyst product with:
- assessment of whether K-17 deserves priority reconnaissance;
- evidence IDs and notes supporting it;
- explicit statements of what remains unknown;
- recommended COLD NORTH PIRs, collection windows or observation types (at a **game abstraction** level);
- risk and confidence caveats.

Suggested analytic categories:
**confirmed observation** / **supported inference** / **plausible but unconfirmed** / **unsupported claim** / **insufficient data**.

Example feedback:
- “Correctly cites GATE-004 as evidence of a Kappa gate passage.”
- “Link to Habitat Nine transport is consistent but unconfirmed.”
- “Insufficient evidence to identify patients. Do not elevate medical requisition into proof of captivity.”
- “Recommended ISR collection requirement created in DRAFT; awaiting RAVEN ACTUAL's approval.”

Do **not** reward making overconfident assertions or hide legitimate “insufficient evidence” results behind binary failure.

## 5. Core mechanic — fictional trace/exposure (future slice)

When future fictional network-access scenarios are authored, player may choose:
- passive collection (slow, lower exposure, incomplete records);
- targeted collection (uses a limited **game resource**, opens relevant records);
- intrusive simulated access (greater chance of alert / loss of access, may reveal different records).

No real-world procedures, shell instructions, scan targets, credential harvesting or exploit techniques should appear; represent these as abstract authorized gameplay decisions with explicit fictional consequences.

Expose the costs **before** action. A scenario cannot secretly penalize an operator for a choice with no telegraphed risk or opportunity to react. Limit wall-clock timers to optional atmosphere; choose deterministic turn/action budgets by default.

### Session state machine

~~~mermaid
stateDiagram-v2
  [*] --> Briefing
  Briefing --> Collecting
  Collecting --> Collecting: inspect/correlate
  Collecting --> Assessment: submit
  Assessment --> Debrief: finalize
  Assessment --> Collecting: revise if allowed
  Collecting --> Withdrawn: disconnect
  Withdrawn --> Debrief
  Debrief --> [*]
~~~

Scenario versions pin clue definitions, assessment rubric, action costs, and unlock policies. A session is not allowed to mutate enemy posture or MIA status except through GM-approved campaign event effects.

## 6. Example authored objectives as data

~~~json
{
  "id": "assess-k17-priority",
  "question": "Do the available intercepts justify prioritizing reconnaissance of K-17?",
  "initial_artifacts": ["tlm-017", "gate-004", "med-211"],
  "required_response": {
    "claim_rating": ["supported", "plausible", "unsupported", "insufficient"],
    "citations_min": 1,
    "unknowns_min": 1
  },
  "effect_on_approval": "draft-collection-request"
}
~~~

This is a contract sketch; it intentionally does not encode a hidden answer or novel fact about Varga/Calder.

For a fair playable implementation the GM/author separately precommits assessment rules, available artifacts, distractors, fail-forward path, and reveal conditions **before** a player submits answers. Store them privately and version them.

## 7. UX requirements

### Intel board
Each artifact has a stable ID, source, observation clock, receipt clock, source independence indicators, report/observation/inference classification, confidence and attachments. Compare artifacts side by side. Allow notes, tags, hypotheses and saved correlations.

### Console
Readable fixed-width transcript, keyboard actions with accessible help, recent command history, side panel listing discovered sources, and “assess” action distinct from “read”. An incorrect command displays suggestions and never runs host code.

### Debrief
Show the player's claim, what evidence supports/undermines it, consequences, unrevealed remaining requirements and any authorized draft tasking. Do not spoil missing-operator status. “Exit without finalizing” must not destroy work.

### TOC integration
After report approval, TOC shows new findings, the source trail, changed metric/event if authorized, and proposed COLD NORTH tasking. The next tabletop operation is scheduled **only upon user command**.

## 8. GM/authoring and replay

The GM authors scenario nodes, artifacts, gates, objective rubric and optional alternatives. Engine can replay the same scenario for testing, but the campaign's canonical event ledger should mark which run was actually committed. Supports a sandbox/rehearsal mode (no canonical effects) and a live authorized mode with explicit confirmation.

**Authoring quality gate:**
- Test at least one path to supported assessment.
- Test a defensible inconclusive outcome.
- Test a confident but unsupported leap.
- Test early withdrawal.
- Test saved session reload.
- Test that player can never fetch hidden rubric/undiscovered evidence via API.
- Test that COLD NORTH remains **not deployed** even after full success.

## 9. Future non-tabletop games

- Intercept reconstruction from noisy message fragments.
- Source interview comparison and reliability scoring.
- Document exploitation with metadata and authentication.
- Interactive ISR observation using pre-authored shift reports, changing visibility and patrol patterns.
- OP/LP planning exercise with limited coverage and asset allocation, represented at the story/game level.
- Orders review and COA wargaming using commanders' tasks, abort criteria, attrition and logistical constraints.
- Fictional cyber-access puzzles that unlock campaign information rather than performing real network actions.

These must plug into the generic campaign state, not become separate incompatible minigames.

## 10. Scenario-specific caution

The original **Local Watchers** event acted as forced alarm without counterplay, and Pointmen positioning made Silent Resident's stealth path near-impossible. Future investigations should not repeat that pattern. Give clear rules, possible counters, failure with understandable causes, and at least one reasonable way to meet objectives without combat.

**Core experience target:** the user should feel like RAVEN ACTUAL making defensible intelligence and command decisions that sometimes produce incomplete answers—rather than an actor being handed a predetermined script.
