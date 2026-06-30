# Goal 057 Spec — Unity Alpha Multi-Family Playable Loop

## Goal id

`goal_057_unity_alpha_multifamily_playable_loop`

## Manual gate

`unity_alpha_multifamily_playable_loop_verification required`

## Purpose

Consume the accepted Goal 056 Unity Alpha media-bound package and the accepted multi-family generated template/runtime-preview pipeline into a real Unity Alpha multi-family playable loop proof.

This is an aggressive product-slice goal. It must move beyond Application-only evidence and prove that the repo-local Unity Alpha player can run bounded visible/automated loops for:

- `map_panel_rpg`;
- `survival_sandbox`;
- `first_person_grid_dungeon`.

Goal 057 must still remain bounded:
- no runtime LLM;
- no provider/RAG/media calls;
- no external media import;
- no GamePackage schema changes;
- no broad Runtime rewrite;
- no broad Unity architecture rewrite.

## Required source evidence

Goal 057 should consume existing evidence when present:

- Goal 043 multi-family generated template vertical slice;
- Goal 047 full generator without media dry-run;
- Goal 053 media asset campaign orchestration;
- Goal 054 media materialization review package;
- Goal 055 media-bound playable review package;
- Goal 056 Unity Alpha media-bound playable package.

## Required product proof

At minimum, the generated compact evidence must prove:

1. Goal 056 accepted by user handoff before Goal 057.
2. Unity Alpha staging payload exists and is deterministic.
3. Three family modes are present:
   - map/panel RPG;
   - survival sandbox;
   - first-person grid dungeon.
4. Each family has:
   - family id;
   - scenario/profile link;
   - source loop refs;
   - staged media refs;
   - visible panel/model records;
   - automated player command plan;
   - expected markers;
   - actual markers.
5. Unity Editor/player route executes with exit code 0, or the goal is committed as `BLOCKED`.
6. Player logs include family-specific markers proving:
   - scenario loaded;
   - media manifest loaded;
   - media hashes validated;
   - family mode selected;
   - family-specific loop started;
   - at least three ordered loop steps executed;
   - family-specific completion marker recorded.
7. Preview/export/review package manifests are updated or written as compact evidence.
8. Heavy Unity build/log/cache outputs stay ignored unless the existing repo intentionally tracks compact review-package files.

## Forbidden fake proof

Do not mark GREEN if the goal only writes JSON saying Unity would work.

A GREEN result needs either:
- real Unity Alpha Editor/player execution through the existing automated route; or
- a repository-existing player diagnostic route that executes and produces fresh logs with required markers.

If Unity cannot run or the existing route is unavailable, commit/push `BLOCKED Goal 057 unity alpha multifamily playable loop` with all partial implementation and diagnostic evidence.

## Expected artifact folder

`.llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/`

## Required artifacts

- `source-manifest.json`
- `family-mode-manifest.json`
- `unity-staging-manifest.json`
- `family-command-plan.json`
- `family-loop-proof-map-panel-rpg.json`
- `family-loop-proof-survival-sandbox.json`
- `family-loop-proof-first-person-grid-dungeon.json`
- `player-log-summary.json`
- `media-binding-validation.json`
- `preview-export-payload.json`
- `review-package-manifest.json`
- `invalid-matrix.json`
- `unity-alpha-multifamily-playable-loop-report.md`

More compact sidecars are allowed if useful, but avoid dumping heavy logs directly.

## Acceptance stance

Do not mark `unity_alpha_multifamily_playable_loop_verification` passed. The goal stops at:

`unity_alpha_multifamily_playable_loop_verification required`
