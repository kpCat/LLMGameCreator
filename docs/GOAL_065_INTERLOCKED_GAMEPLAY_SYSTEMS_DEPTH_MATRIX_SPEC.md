# Goal 065 — Interlocked Gameplay Systems Depth Matrix

## Purpose

Consume the accepted living-world and full-campaign proof chain and add deeper interlocked gameplay consequences across all three generated families and three seeds.

Goal 065 must prove that generated rows are not only valid, spatially detailed, package-bound, media-bound, and NPC/faction-reactive, but also carry meaningful interlocked gameplay systems:

- economy/resource changes;
- crafting/recipe conversions;
- combat/encounter resolution;
- progression/skill/status/effect changes;
- loot/reward/equipment changes;
- family-specific consequences;
- save/load/replay determinism;
- Unity Alpha marker proof.

## Gate

```text
interlocked_gameplay_systems_depth_matrix_verification required
```

## Non-goals

Do not change public GamePackage schema. Do not modify Runtime/Runtime.Abstractions, WinForms UI, provider/LLM/RAG paths, arbitrary Lua execution, generator-library, .sln, .csproj, or add external dependencies.

Goal 065 is not a full tactics engine, full economy simulator, or final combat system. It is a deterministic interlocked gameplay matrix proving that the current full generator pipeline can produce state-changing cross-system gameplay rows.

## Required rows

Produce 9 rows:

```text
map_panel_rpg x seed_alpha
map_panel_rpg x seed_beta
map_panel_rpg x seed_gamma
survival_sandbox x seed_alpha
survival_sandbox x seed_beta
survival_sandbox x seed_gamma
first_person_grid_dungeon x seed_alpha
first_person_grid_dungeon x seed_beta
first_person_grid_dungeon x seed_gamma
```

## Family expectations

### map_panel_rpg

Required state-changing proof should include:

- travel or location context from prior spatial/living-world rows;
- NPC/faction interaction;
- economy or trade/work contract ledger;
- combat or conflict pressure resolution;
- inventory/reward/progression delta;
- reputation or social consequence;
- save/load/replay proof.

### survival_sandbox

Required state-changing proof should include:

- hazard/need/resource pressure;
- resource collection or conversion;
- crafting/recipe outcome;
- condition/status effect change;
- inventory/equipment or shelter/tool delta;
- recovery/failure pressure;
- save/load/replay proof.

### first_person_grid_dungeon

Required state-changing proof should include:

- orientation/grid traversal context;
- encounter/combat pressure;
- resource spend or ability use;
- loot/key/reward/progression delta;
- status/effect or blocked/valid movement consequence;
- save/load/replay proof.

## Required artifacts

Write compact deterministic evidence under:

```text
.llmgc/procedural/goal-065-interlocked-gameplay-systems-depth-matrix/
```

Required artifacts:

```text
source-manifest.json
system-rule-catalog.json
row-plan-matrix.json
row-map-panel-rpg-seed-alpha.json
row-map-panel-rpg-seed-beta.json
row-map-panel-rpg-seed-gamma.json
row-survival-sandbox-seed-alpha.json
row-survival-sandbox-seed-beta.json
row-survival-sandbox-seed-gamma.json
row-first-person-grid-dungeon-seed-alpha.json
row-first-person-grid-dungeon-seed-beta.json
row-first-person-grid-dungeon-seed-gamma.json
economy-crafting-ledger.json
combat-progression-ledger.json
status-effect-ledger.json
save-load-replay-proof.json
variance-metrics.json
unity-command-plan.json
unity-player-proof.json
preview-export-gameplay-payload.json
invalid-diagnostics-matrix.json
interlocked-gameplay-systems-depth-matrix-report.md
```

Do not include nondeterministic timestamps, absolute paths, heavy logs, or Unity build output in tracked artifacts.

## GREEN criteria

GREEN is allowed only if:

- Goal 064 acceptance is recorded by user handoff before Goal 065.
- 9/9 rows are produced.
- 9/9 rows are state-changing.
- Each row has at least one economy/resource/crafting delta and one combat/progression/status/reward delta, adapted to the family.
- Save/load/replay passes for all rows.
- Row hashes are deterministic and distinct where expected.
- Family-specific variance is meaningful, not just id/hash changes.
- Invalid/fake/leak matrix passes.
- Unity Alpha proof route executes and records all required interlocked gameplay markers.
- `check-all.ps1` passes.
- Artifact scope guard passes.

If these criteria cannot be met honestly, commit/push BLOCKED.
