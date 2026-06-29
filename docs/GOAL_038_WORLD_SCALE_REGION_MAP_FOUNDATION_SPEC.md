# Goal 038 — World-scale region graph, finite map pack and chunk-config foundation

## Goal id

`goal_038_world_scale_region_map_foundation`

## Manual gate

`world_scale_region_map_foundation_verification required`

## Intent

Goal 038 is an aggressive composite world-scale slice. It intentionally combines the next world-scale queue items into one implementation goal:

1. accept Goal 037 by user handoff;
2. create deterministic region graph and reachability validation;
3. create compact finite map packs with landmarks, routes, passability and scenario-specific placement summaries;
4. create chunked-world config prelude without runtime chunk deltas;
5. prove all of this across `frontier_survival`, `gothic_intrigue`, `caravan_trade` and `metamodule_kingdoms`.

This goal must move the generator toward playable/simulatable world scale. It must not become a paper-only registry.

## Non-goals

- No Runtime changes.
- No Unity changes.
- No WinForms/UI changes.
- No public GamePackage schema changes.
- No provider/LLM/RAG calls.
- No new Lua execution work beyond consuming Goal 037 evidence/contracts as input facts.
- No generator-library changes.
- No external dependency additions.
- No huge tile-array dumps.

## Required design

The Application-layer implementation should model:

- world scenario id/profile id;
- kingdom/region/biome/settlement/landmark records;
- directed or undirected travel edges with costs and constraints;
- required and optional reachability targets;
- route categories such as road, trail, river, pass, sea lane, caravan route, dungeon descent or magical gate;
- semantic tags from previous Goal 030–037 systems;
- finite map pack records with compact cell/patch summaries, passability summaries, route polylines and landmark placements;
- chunked-world config prelude records with deterministic chunk ids, chunk size, region-to-chunk coverage and generation rule refs;
- traversal itinerary records proving selected generated starts can reach required gameplay targets.

## Required scenarios

Use the same broad scenario set unless local accepted ids differ:

- `frontier_survival`
- `gothic_intrigue`
- `caravan_trade`
- `metamodule_kingdoms`

`metamodule_kingdoms` must stay high-complexity: preserve seven kingdoms/regions and at least 112 species/archetype slot references as compact metadata, not full giant content.

## Required evidence files

Write compact deterministic files under:

`.llmgc/procedural/goal-038-world-scale-region-map-foundation/`

Required files:

- `region-graph-summary.json`
- `reachability-matrix.json`
- `finite-map-pack-frontier.json`
- `finite-map-pack-gothic.json`
- `finite-map-pack-caravan.json`
- `finite-map-pack-metamodule-kingdoms.json`
- `chunked-world-config-prelude.json`
- `traversal-itinerary-matrix.json`
- `invalid-world-scale-diagnostics-matrix.json`
- `world-scale-region-map-foundation-report.md`

Evidence must be deterministic, compact, timestamp-free unless the repo already has a deterministic timestamp convention, and free of absolute paths.

## Acceptance proof

The final report must contain:

- `world_scale_region_map_foundation_verification required`
- `accepted=false`
- `implementationStatus: GREEN` or the honest BLOCKED/FAILED status
- scenario counts
- region counts
- required-reachability counts
- finite-map-pack counts
- chunk-config prelude proof
- invalid/fake/leak matrix summary
- confirmation that no Runtime/UI/Unity/GamePackage/provider/LLM/RAG/generator-library/external-dependency changes were made.
