# Goal 031 spec — Semantic Pack Composition Blueprint

## Goal id

`goal-031-semantic-pack-composition-blueprint-v1`

## Gate marker

`semantic_pack_composition_blueprint_verification`

## Purpose

Goal 030 introduced a semantic artifact contract registry and compatibility planner. Goal 031 must make that usable as a real composition layer.

The generator needs a deterministic way to take selected semantic packs and produce a coherent, cross-linked generation blueprint plan before any low-level artifact materialization happens.

This blueprint plan is still pre-runtime and pre-GamePackage. It should describe how semantic packs drive world regions, factions, NPC archetypes, quest motives, dialogue tones, economy chains, combat pressures, item/loot themes, settlements, events, and biome hazards.

## Non-goals

Goal 031 must not:

- mutate public GamePackage schema;
- touch WinForms/UI;
- touch Runtime/Unity;
- call LLM/provider/RAG;
- execute Lua;
- generate media;
- add NuGet dependencies;
- implement concrete final NPC/dialogue/quest/runtime systems.

## Expected new capability

Given:

- a profile/family id;
- selected semantic pack ids;
- optional scale/complexity request;
- existing Goal 030 registry and compatibility planner;

the system should produce a deterministic `SemanticBlueprintPlan` or local equivalent containing:

- selected packs;
- resolved semantic facts;
- relation graph;
- cross-artifact links;
- feature/module coverage;
- world/biome/faction/NPC/quest/dialogue/economy/combat/settlement/event blueprint sections;
- conflicts/blockers/future-required diagnostics;
- stable evidence JSON.

## Minimum semantic pack composition model

A semantic pack composition model should include these meanings, even if class names differ:

- pack id;
- pack family/profile support;
- provided scopes;
- theme tags;
- semantic facts;
- relation hints;
- exclusions/conflicts;
- expansion intents;
- priority/order key;
- optional/future flags.

Facts should be deterministic simple data, not free-form unstructured prose only. For example:

- `region_pressure:winter_hazard`
- `faction_role:merchant_guild`
- `quest_motive:debt_escape`
- `npc_archetype:frontier_healer`
- `economy_chain:furs_to_medicine`
- `dialogue_tone:suspicious_polite`
- `combat_pressure:wild_beasts`
- `settlement_pattern:trade_outpost`

## Minimum blueprint sections

At minimum, produce sections for:

1. world regions / route pressure;
2. biome/weather/hazard/event pressure;
3. factions and reputation/social relation anchors;
4. NPC archetype variation anchors;
5. quest motive/objective/reward pattern anchors;
6. dialogue tone/localization/string-table hints;
7. economy/resource/recipe/loot chains;
8. combat/progression/ability pressures;
9. settlement/building/landmark anchors;
10. cross-artifact coverage and missing/future-required gaps.

## Determinism requirements

- Stable ordering everywhere.
- No wall-clock timestamps.
- No absolute paths.
- Same request and seed packs must produce byte-equivalent evidence or structurally equivalent output.
- Diagnostics must have stable codes.

## Evidence output

Evidence should be written under:

`.llmgc/procedural/goal-031-semantic-pack-composition-blueprint/`

Required files:

- `pack-catalog-summary.json`
- `composition-matrix.json`
- `semantic-blueprint-plan-frontier.json`
- `semantic-blueprint-plan-gothic.json`
- `semantic-blueprint-plan-caravan.json`
- `cross-artifact-linkage-report.json`
- `semantic-pack-composition-blueprint-report.md`

The markdown report must contain:

`semantic_pack_composition_blueprint_verification required`

## Acceptance posture

Goal 031 should finish with the gate required for manual review, not accepted.
