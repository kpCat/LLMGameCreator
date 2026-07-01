# GOAL 066 — Settlement Construction Destruction Production Matrix

## Intent

Goal 066 should convert the existing full campaign and interlocked gameplay proof into a deeper settlement/city-builder-style layer:

```text
Goal 060 package matrix
+ Goal 061 playable review package RC
+ Goal 062 constrained spatial detail rows
+ Goal 063 gameplay consequences
+ Goal 064 living-world NPC/faction simulation
+ Goal 065 interlocked gameplay systems
-> 9 settlement/construction/destruction/production rows
-> building placement/construction/upgrade/damage/repair/production/defense deltas
-> save/load/replay proof
-> meaningful variance
-> Unity Alpha settlement markers
```

This is an aggressive gameplay-depth goal. It is not a documentation-only goal.

## Required manual handoff acceptance

At the beginning of the task, record the user handoff acceptance:

```text
interlocked_gameplay_systems_depth_matrix_verification passed before Goal 066
```

Then implement Goal 066 and leave its gate open:

```text
settlement_construction_destruction_production_matrix_verification required
accepted=false
```

## Scope intent

BCL-only Application-layer seam plus focused tests, product smoke, compact artifacts and a narrow Unity Alpha marker/proof extension.

Goal 066 may consume prior artifacts and source loaders, but must not change public GamePackage schema or broad Runtime/UI/Unity architecture.

## Families

Prove all three existing families:

- `map_panel_rpg`
- `survival_sandbox`
- `first_person_grid_dungeon`

Across three deterministic seeds:

- `seed_alpha`
- `seed_beta`
- `seed_gamma`

Total required rows: 9.

## Required row behavior

Each row must have real state-changing deltas, not only ids/hashes.

Minimum per-row systems:

1. Settlement/site identity.
2. Building slot or footprint.
3. Placement/build action.
4. Construction cost/resource delta.
5. Production or service output delta.
6. Damage/destruction or threat pressure event.
7. Repair/upgrade/defense response.
8. NPC/faction/living-world consequence linkage.
9. Save/load/replay proof.
10. Unity marker proof.

Family-specific expectations:

### map_panel_rpg

- Settlement hub, inn/outpost/workshop/shrine/market style building.
- NPC/faction consequence.
- Work/trade/quest support service.
- Threat or damage event that changes settlement state.
- Reward/progression/faction tie-in.

### survival_sandbox

- Shelter/camp/workbench/water collector/trap/garden style building.
- Resource cost and production/repair loop.
- Hazard/weather/need consequence.
- Damage/decay/repair pressure.
- Survival status/recovery tie-in.

### first_person_grid_dungeon

- Safe room/gate/key mechanism/trap room/shrine/cache style structure.
- Construction or activation state.
- Destruction/disable/repair/lock-state consequence.
- Encounter/progression/loot tie-in.
- Navigation/blocking/unblocking consequence.

## Required artifacts

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-066-settlement-construction-destruction-production-matrix/
```

Required minimum artifacts:

```text
settlement-construction-source-manifest.json
settlement-construction-row-matrix.json
settlement-building-catalog.json
settlement-production-ledger.json
settlement-destruction-repair-ledger.json
settlement-defense-threat-ledger.json
settlement-living-world-linkage.json
settlement-save-load-replay-proof.json
settlement-unity-command-plan.json
settlement-unity-player-proof-summary.json
settlement-invalid-diagnostics-matrix.json
settlement-preview-export-payload.json
settlement-construction-destruction-production-matrix-report.md
```

Additional per-row artifacts are allowed if compact and deterministic.

Evidence constraints:

- no absolute machine paths in JSON/MD evidence;
- stable ordering;
- no timestamps unless existing deterministic convention is used;
- no heavy Unity build/log artifacts staged;
- report must include the exact gate string.

## Unity Alpha proof

A narrow `AlphaRuntimeBootstrap.cs` extension is allowed only to read a staged Goal 066 command/marker plan and emit deterministic markers.

Required markers should prove:

- row id;
- family id;
- seed id;
- settlement id;
- build/construction action;
- production/resource delta;
- destruction/damage/repair/defense consequence;
- living-world/faction/NPC linkage;
- save/load/replay row proof;
- final `settlement_row_completed` marker.

Do not implement broad Unity UI/building system/physics/destruction. This is a marker/proof route, not a playable city-builder UI.

## Invalid/fake/leak matrix

Must reject causally at least:

- missing Goal065 source;
- fake family/seed/row id;
- missing spatial detail row;
- missing living-world linkage;
- missing interlocked gameplay dependency;
- illegal building footprint or blocked placement;
- insufficient construction cost/resources;
- invalid production output;
- repair without damage;
- destruction without affected structure;
- missing save/load/replay trace;
- duplicate settlement/building id;
- unsafe relative path;
- nondeterministic ordering;
- provider/LLM/RAG/media-generation claim;
- arbitrary Lua execution claim;
- broad Runtime/UI/Unity/GamePackage schema mutation claim.

## Quality bar

GREEN is allowed only if:

- 9/9 rows are state-changing;
- settlement construction/production/destruction/repair/defense ledgers pass;
- save/load/replay passes for all 9 rows;
- meaningful variance passes across families and seeds;
- Unity/player proof passes with all required markers;
- check-all passes;
- artifact scope guard passes;
- final state is committed and pushed.

If only descriptions or hash-only differences are produced, commit/push as BLOCKED.
