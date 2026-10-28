# Agent instructions — GREY RELAY

Read **README.md** and **docs/AGENT_HANDOFF.md** before implementing features. This repository is a **campaign-agnostic** platform for persistent tabletop narratives and playable fictional SIGINT/cyber investigations.

## Non-negotiable
- Do not commit to main; create focused branches and PRs for owner review.
- Do not invent tabletop rules, new campaign outcomes, casualty fate, or deployment orders.
- **VEIL and Calder are MIA, fate unknown.** **COLD NORTH is not deployed.** K-17 is an intelligence lead, not confirmed custody.
- Separate source observation, inference, proposed event, committed event and GM-hidden truth.
- Keep hidden puzzle answers and GM secrets out of player APIs, frontend bundles, exports, logs and player-state fixtures. GM content packs (`*.gm.json`) may be committed to the repo or supplied privately; never mix GM data into player-visible files (ADR 0003).
- No real hacking: console commands must be fictional, allowlisted application actions. No shell, arbitrary code execution, credential collection or scanning.
- No BLKOUT copyrighted PDFs, commercial cards/art/STLs, or third-party assets in public repo without explicit rights.
- Use macOS-native developer workflow; do not require Docker.
- Do not claim proposed docs are implemented functionality.
- Maintain versioned campaign data, explicit state changes and correction audit trails.

## Recommended next work
D0 docs and example state → S0 stack ADR and runnable skeleton → S1 persistence/events → S2 player-visible TOC/evidence board → S3 playable GREY RELAY fictional console → tabletop AAR/print and pack authoring.

Review:
- docs/CAMPAIGN_STATE_ASH_VEIL.md
- docs/ARCHITECTURE.md
- docs/CONTENT_CONTRACTS.md
- docs/GREY_RELAY_GAMEPLAY.md
- docs/ROADMAP.md

Each PR: summary, implemented vs proposed, assumptions, acceptance tests/results, local reproduction, screenshots if UI, data migration and story impact. Protect the player's authority over in-universe decisions.
