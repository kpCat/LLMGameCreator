# Goal 064 spec — Living World NPC/Faction Simulation Matrix

## Goal id

```text
goal_064_living_world_npc_faction_simulation_matrix
```

## Manual gate

```text
living_world_npc_faction_simulation_matrix_verification required
```

## High-level intent

Goal 063 proved that the 9 family/seed rows have state-changing gameplay consequences. Goal 064 must convert those consequences into a deterministic living-world simulation layer: NPC/actor state, faction relations, social consequences, schedules/availability, world event propagation and replayable ticks.

This goal must not be another static manifest. It must prove a stateful simulation matrix.

## Input proof chain

Goal 064 consumes the accepted/proven chain through Goal 063:

- Goal 060 GamePackage materialization matrix.
- Goal 061 playable review package RC.
- Goal 062 constrained spatial detail generation.
- Goal 063 gameplay consequence depth matrix.

Goal 064 must first record acceptance of Goal 063 by user handoff:

```text
gameplay_consequence_depth_matrix_verification passed before Goal 064
```

## Required output

Produce deterministic compact evidence under:

```text
.llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix/
```

Required artifact intent:

1. source manifest tying Goal 060/061/062/063 evidence together;
2. actor/faction seed catalog;
3. relation/schedule/event rule summary;
4. 9 row simulation plans, one per family/seed;
5. 9 row simulation tick traces;
6. 9 row before/after living-world states;
7. 9 row save/load/replay proofs;
8. family variance metrics;
9. invalid/fake/leak diagnostics matrix;
10. preview/export payload for living-world state;
11. Unity Alpha command plan;
12. Unity/player proof summary;
13. compact markdown report containing the manual gate marker.

Exact file names may follow local evidence-service style, but the report must contain:

```text
living_world_npc_faction_simulation_matrix_verification required
implementationStatus=GREEN|BLOCKED|FAILED
accepted=false
```

## Required simulation content

Across the 9 family/seed rows, prove meaningful living-world state changes. At minimum:

### map_panel_rpg

- NPC availability or route state changes after travel/quest/event consequence.
- Faction/social relation changes from the Goal 063 consequence row.
- At least one event memory/rumor/quest pressure entry.

### survival_sandbox

- NPC/group availability or camp/settlement support changes from hazard/resource consequence.
- Faction/group trust or scarcity pressure changes.
- At least one world-event pressure such as weather, hunger, resource depletion, shelter or danger.

### first_person_grid_dungeon

- Dungeon actor/encounter pressure changes after traversal/loot/progression.
- Faction/monster-group aggression or alertness changes.
- At least one memory/event trace linked to spatial detail and blocked/valid movement.

## Determinism and replay

Each row must have:

- stable row id;
- stable seed id;
- ordered simulation ticks;
- before state hash;
- after state hash;
- before != after;
- save/load or serializer roundtrip proof;
- replay proof that rerunning from the same row input gives the same tick/state hashes;
- variance proof that rows are not only different by IDs/hash-only noise.

## Unity Alpha proof

A narrow extension to the repo-local Unity Alpha is allowed only in:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

Required player log markers should prove that Unity consumed the staged living-world command plan and emitted row/family markers such as:

```text
living_world_matrix_loaded=goal064
living_world_row=<row id>
living_world_tick=<tick index>
npc_state_changed=true
faction_relation_changed=true
world_event_resolved=true
living_world_row_completed=<row id>
living_world_matrix_completed=true
review_package_proof=goal064
```

The exact marker names may follow local Alpha conventions, but they must be deterministic and tested.

## Forbidden behavior

Goal 064 must not:

- call LLM/provider/RAG;
- generate or import media;
- execute arbitrary Lua;
- add ECS or other external dependencies;
- change public `GamePackage` schema;
- change `Runtime` or `Runtime.Abstractions` source;
- change WinForms UI;
- change Unity broadly beyond the narrow Alpha bootstrap marker/loader path;
- modify `.sln` or `.csproj`;
- create heavy build/log outputs in tracked files.

## Dependency policy

No new NuGet dependencies in Goal 064. ECS and simulation libraries remain future optional adapters.

## Success criteria

GREEN is allowed only if:

- 9 row living-world simulation matrix exists;
- all 9 rows have state-changing NPC/faction/world-event consequences;
- save/load/replay proof passes for all rows;
- family/seed variance is meaningful;
- Unity/player proof runs and matches all required living-world markers;
- check-all passes;
- artifact scope guard passes;
- final commit/push is done.

If only models/reports are added without real state-changing simulation rows and Unity/player proof, status must be BLOCKED or FAILED, not GREEN.
