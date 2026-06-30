# Goal 062 — Constrained Spatial Detail Generation

## Goal id

`goal_062_constrained_spatial_detail_generation`

## Manual gate

`constrained_spatial_detail_generation_verification`

## Why this goal now

Goal 061 produced a playable review package RC with 9 package rows. The next risk is that those rows are playable/reviewable but spatially shallow. Goal 062 adds a deterministic spatial-detail layer that consumes the 9 package rows and produces validated local map/chunk/detail patches for the three supported families.

This goal should make the generator less "paper-like" by proving real spatial detail artifacts, reachability, repair/fallback diagnostics and Unity/player markers.

## Scope summary

Implement a BCL-only Application seam for constrained spatial detail generation.

It should consume Goal 061/060/059 evidence and produce deterministic spatial detail for:

- `map_panel_rpg`
- `survival_sandbox`
- `first_person_grid_dungeon`

Across 3 seeds each, for 9 package rows total.

## mxgmn/WFC/MarkovJunior/TextureSynthesis position

Use the mxgmn repositories as design inspiration and scouting references only.

Do not add dependency.

Do not copy source.

Do not import sample assets.

Implement a small in-house model:
- spatial patch descriptors;
- tile palette;
- adjacency rules;
- rewrite/repair rules;
- deterministic planner;
- contradiction and fallback diagnostics;
- reachability/path validation;
- Unity/player marker proof.

## Required outputs

Under:

`.llmgc/procedural/goal-062-constrained-spatial-detail-generation/`

Required artifacts:

- `source-manifest.json`
- `spatial-palette-catalog.json`
- `rewrite-rule-catalog.json`
- `constraint-rule-catalog.json`
- `spatial-detail-matrix.json`
- `spatial-detail-row-map_panel_rpg-seed_alpha.json`
- `spatial-detail-row-map_panel_rpg-seed_beta.json`
- `spatial-detail-row-map_panel_rpg-seed_gamma.json`
- `spatial-detail-row-survival_sandbox-seed_alpha.json`
- `spatial-detail-row-survival_sandbox-seed_beta.json`
- `spatial-detail-row-survival_sandbox-seed_gamma.json`
- `spatial-detail-row-first_person_grid_dungeon-seed_alpha.json`
- `spatial-detail-row-first_person_grid_dungeon-seed_beta.json`
- `spatial-detail-row-first_person_grid_dungeon-seed_gamma.json`
- `reachability-proof-matrix.json`
- `spatial-repair-fallback-matrix.json`
- `unity-spatial-detail-command-plan.json`
- `unity-spatial-detail-proof-summary.json`
- `preview-export-spatial-payload.json`
- `invalid-spatial-detail-diagnostics-matrix.json`
- `artifact-scope-report.json`
- `constrained-spatial-detail-generation-report.md`

Optional but desirable if BCL-only helper exists or can be safely implemented inside scope:
- deterministic compact PNG thumbnails for each family or row;
- thumbnail sidecar hashes.

## Required proofs

1. 9/9 row spatial details exist.
2. Each row has a meaningful tile/detail layout, not only different ids.
3. Each row is deterministic under repeat generation.
4. Each row differs meaningfully by seed/family.
5. Reachability proof exists:
   - map_panel_rpg: entry -> NPC/quest/objective/item/exit route;
   - survival_sandbox: shelter/resource/hazard/water/exit route;
   - first_person_grid_dungeon: start -> corridor/door/encounter/objective/exit route.
6. Repair/fallback proof exists:
   - invalid constraints produce diagnostics;
   - contradiction-like scenario is detected;
   - fallback/relaxation is explicit and deterministic.
7. Unity Alpha proof:
   - narrow AlphaRuntimeBootstrap extension can load command plan;
   - player emits family/seed spatial markers;
   - all required markers are matched in evidence.
8. No external asset/source/dependency import.
9. No public GamePackage schema change.

## Gate status

Goal 062 must stop at:

`constrained_spatial_detail_generation_verification required`

Do not mark it passed.

## Final status policy

Codex must commit/push final state in all cases:
- `GREEN Goal 062 constrained spatial detail generation`
- `BLOCKED Goal 062 constrained spatial detail generation`
- `FAILED Goal 062 constrained spatial detail generation`

Do not pretend non-green work is accepted.
