# Content, campaign state and scenario contracts

**Status:** normative **design proposal**, not implemented API or final schema.  
**Purpose:** enable other agents to build a reusable, safe, versioned campaign engine without inventing lore or tabletop rules.

## 1. The four data classes

1. **Content pack:** reusable author-owned definitions, potentially shareable: faction templates, map areas, narrative nodes, player-visible scenario artifacts and optional GM-only authoring materials.
2. **Campaign instance:** one ongoing playthrough with roster instances, ownership, dates, chosen decisions, resource balances and pinned content versions.
3. **Event journal:** immutable adjudicated history of actions, discoveries, casualties, message deliveries, GM rulings and corrections.
4. **Derived player view / GM view:** visibility-filtered projection; the same full campaign object is *never* blindly serialized to every user.

Do **not** treat narrative markdown as the only store of truth. Structured data drives the UI and a separate readable log explains the events.

## 2. Packaging/versioning

Recommended import archive:
~~~text
pack.json
campaign/
  definition.json
  sources.json
  missions.json
  puzzle-scenarios/
    grey-relay-001.public.json
    grey-relay-001.gm.json      # optional separate distribution, NEVER exposed to players
assets/                         # original licensed assets only
~~~

Pack manifest fields:
- schema_version, pack_id, pack_version, title, description, author, content_rights, supported_engine_range.
- dependencies with name, version range and optional reference-only rules adapter.
- checksum per file; maximum size and allowed MIME types.
- gm_sections declared for **server-only** install. Import preview must not include restricted file contents.

Do not treat unsigned user pack data as privileged instructions or executable code. Markdown/html must be sanitized before rendering. Stronger pack signing/provenance can be considered later.

## 3. Proposed CampaignInstance v1

Required concepts: id, schema_version, template_id, title, status, fictional_clock, current_revision, metrics, units/operators, intelligence, mission_history, pending_orders and refs to event journal. Recommend UUID IDs in real storage and human-friendly stable story keys in pack data.

~~~json
{
  "schema_version": "1.0.0",
  "campaign_id": "campaign-ash-veil-demo",
  "campaign_template_id": "ash-veil",
  "title": "Operation Ash Veil",
  "state_revision": 1,
  "fictional_clock": {"day": 2, "time_z": "18:45"},
  "metrics": {
    "intelligence_quality": {"value": 2, "scale_max": 5},
    "authority_alert": {"value": 4, "scale_max": 5},
    "logistical_support": {"value": 3, "scale_max": 5},
    "civilian_confidence": {"value": 1, "scale_max": 5}
  },
  "personnel": [
    {"operator_id": "op-veil", "display_name": "WO Ana Varga", "status": "mia"},
    {"operator_id": "op-calder", "display_name": "Spc Nia Calder", "status": "mia"}
  ],
  "current_operation_id": "ash-veil-recovery",
  "active_tasking_ids": ["needle-in-noise-completed"],
  "pending_decision_ids": ["authorize-grey-relay", "cold-north-method"]
}
~~~

This excerpt is illustrative; the committed [player-state example](../examples/ash-veil.player-state.v1.json) is the fuller sample. No hidden truth belongs in it.

### Unit / operator status

Prefer orthogonal fields rather than mutually exclusive story labels:
- tactical_state: active / downed / out_of_action / unknown
- accountability: present / evacuated / missing / recovered / confirmed_kia / unknown
- medical: uninjured / wounded / needs_assessment / cleared / unknown
- duty: available / restricted / unavailable / unknown
- confidence and recorded source

For example, Varga can be **tactical OOA** during M001, **missing** for accountability and **unknown** medically. Never collapse into “dead” or “captured” because a miniature was removed.

### Campaign metrics

A fictional quality score should specify units, min/max, source event and scope. Numeric change must be authorized by a scenario or adjudication; no arbitrary random punitive shifts. Display on TOC as **campaign** values, never as BLKOUT combat modifiers unless an accepted adapter explicitly provides that behavior.

## 4. EvidenceArtifact and confidence

Proposed fields:
~~~json
{
  "artifact_id": "sig-med-2124",
  "campaign_id": "campaign-ash-veil-demo",
  "type": "sigint_requisition",
  "title": "K-17 trauma module request",
  "observed_at": {"day": 1, "time_z": "21:24"},
  "received_at": {"day": 2, "time_z": "11:25"},
  "source_id": "kappa-logistics-intercept",
  "source_reliability": "unassessed",
  "visibility": "player",
  "raw_claim": "Two trauma stabilization modules requested",
  "confidence": "moderate",
  "supports": ["hyp-k17-medical-activity"],
  "does_not_establish": ["patient_identity", "detention_status"],
  "provenance": "Narrative SIGINT Product 002"
}
~~~

**Vocabulary guidance:**
- observation: an authenticated direct result (e.g. a logged event at a particular node).
- report: a source's statement, possibly false/incomplete.
- inference: an analyst conclusion drawn from one or more sources.
- hypothesis: a question/claim that can be supported, contradicted or left unresolved.
- GM truth: hidden adjudication reality, not inherently available to a character.

**Confidence** is confidence in a clearly specified proposition, not universal truth: confidence that a gate event occurred can be high while confidence it involved a particular vehicle remains moderate/low. Store assessments per claim; don't reuse one badge for all conclusions.

**Corroboration** requires independence. Two newspaper reports copying the same Authority statement count as one originating source, not two independent confirmations.

## 5. Intelligence requirements and orders

Each PIR/IR/CCIR should include:
- id, question, owner, priority, why it matters, deadline if set, required decision and observable confirmation criteria.
- collection feasibility, current evidence refs, evaluated confidence and remaining gaps.
- status: proposed / approved / tasked / collecting / answered / closed; never auto-task an element because a document is drafted.

An order includes issuer, addressee, intent, tasks, constraints, authority level, contingencies, status and approval/refused state. Suggested lifecycle:
**draft → reviewed → authorized → issued → acknowledged → executing → completed/aborted**, with auditable transitions.

COLD NORTH currently has a **potential reconnaissance requirement**, not an issued deployment order. The app must reflect that difference.

## 6. GREY RELAY scenario and session objects

### Scenario definition
~~~json
{
  "scenario_schema_version": "1.0.0",
  "scenario_id": "grey-relay-001",
  "mode": "fictional-sigint",
  "title": "Needle Trace",
  "required_campaign_flags": ["needle-in-noise-completed"],
  "initial_access": ["passive-inbox"],
  "public_artifact_ids": ["tlm-017", "gate-004", "med-211"],
  "allowed_commands": ["help", "sources", "inspect", "correlate", "hypotheses", "assess", "report", "disconnect"],
  "success_effects": ["offer-draft-cold-north-collection-requirement"]
}
~~~

Do not put a real campaign success answer or undisclosed source in this public example. GM outcome criteria are held separately, released only through server evaluation.

### Session record
- session_id, campaign_id, scenario_id, pinned scenario version, current phase, accessed nodes, discovered artifact IDs.
- player commands/actions log with idempotency key and sequence.
- evidence correlations/assessments submitted by player.
- limited fictional trace resource **only when designed and disclosed**.
- objective verdict, explainable reasons, partial results and final report.
- consumed choices, decisions, narrative effects, final status.

**No content-dependent side effects in the UI**: a user pressing “Assess” sends an action to the domain logic; the backend validates and commits once.

### Safe interpreter

A command is a declarative **game action**:
~~~json
{
  "session_id": "session-001",
  "expected_revision": 4,
  "action_id": "command-0005",
  "verb": "inspect",
  "arguments": {"artifact_id": "gate-004"}
}
~~~

Do not execute arbitrary terminal strings. Parser converts a strict allowlist of fictional commands into typed actions; unknown input yields a help/error message without shell/network evaluation.

### Puzzle evaluation

Per objective, pre-author:
- claims or investigative questions;
- evidence available initially and unlock triggers;
- relevant/distractor evidence;
- supported conclusions and **explicit overclaims** (e.g. two trauma devices = two specific prisoners);
- partial success, failure and recovery path;
- actionable reconnaissance deliverable and downstream uncertainty.

An analyst assessment must be explainable with evidence references. Store scoring version separately. Good analysis can still conclude “insufficient evidence”; this should never be treated as automatic failure.

## 7. Narrative and triggers

Define a narrative node with identifier, scene content, when eligible, who can see it, what prior actions authorize it, and any **proposed** next choices. Trigger expressions should be a declarative allowlist (all, any, event_exists, metric_threshold, status_equals) with fixed interpretation. No arbitrary JS string evaluation.

Authors/AI may propose a scene, but gameplay truth changes require an explicit GM/story authority command. GM-only outcomes should remain secret until revealing events are committed.

## 8. Event envelope and correction

~~~json
{
  "event_id": "evt-001",
  "campaign_id": "campaign-ash-veil-demo",
  "sequence": 24,
  "event_type": "operator.accountability_changed",
  "schema_version": 1,
  "actor": {"kind": "gm", "id": "overwatch"},
  "causation_id": "aar-m001",
  "occurred_fictional": {"day": 1, "time_z": "20:30"},
  "recorded_at_utc": "2026-10-27T22:00:00Z",
  "payload": {
    "operator_id": "op-veil",
    "from": "deployed",
    "to": "missing",
    "reason": "Unrecovered after withdrawal",
    "certainty": "confirmed-accountability-only"
  }
}
~~~

The sample timestamp is illustrative of the *developer event format*, not a new historical claim. In a real seed use the original conversation event timing when known; otherwise mark unknown rather than fabricate precision.

For corrections, emit event_type **event.corrected** with target_event_id, replacement/reference and reason. Recompute projections, preserve original history and prevent direct hidden data mutation.

## 9. Import/export requirements

- Validate schemas, pack version, referential integrity, ID uniqueness, allowed roles, file content types, sizes and integrity hashes.
- Produce a dry-run report listing conflicts, missing dependencies, migration needs and visibility/security issues.
- Never overwrite a live campaign implicitly. Support new campaign instance or explicit merge strategy with revision check.
- Export campaign data separately from third-party reference files, secrets and licensed assets. For player exports, exclude GM truth by default.
- Roundtrip should preserve outcomes, incomplete clues, player-authored reports and unresolved statuses.
- Separate source references from proprietary content; no third-party BLKOUT rulebook or card copied into packs.

## 10. Test cases that must exist before S3 release

1. A downed missing operator never automatically becomes KIA, captured or rescued.
2. “Two stabilization modules” supports restricted medical activity, **not** patient names.
3. A player cannot retrieve unrevealed evidence or GM scoring keys through any player endpoint.
4. A wrong conclusion receives an explained counterargument, not an arbitrary dead-end.
5. Same starting data + same action log + same random seed yields identical results.
6. Unauthorized action on another campaign returns denial; no data leaks.
7. Importing malformed/malicious content is rejected; never interpreted/executed.
8. A validated report may create **draft COLD NORTH tasks** but not deployment.
9. Corrected AAR remains auditable, with no silent enemy casualty/loss edits.
10. A user can export, import, resume a second campaign with independent IDs and source references.

**Design intent:** data schemas protect a persistent fiction from accidental retcons while giving the player real choices and the GM room to improvise.
