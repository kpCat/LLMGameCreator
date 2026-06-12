# Batch 014 — NPC, schedule and pathfinding

## Purpose

This batch adds compact Lua generator modules for NPC archetypes, schedules, pathfinding configuration and faction-role metadata.

The modules emit JSON-serializable IR/config tables only. They do not execute a live simulation, run pathfinding algorithms, instantiate Unity objects, load external code or access the file system.

## Files

- `lua/npc/npc_archetype_generator.lua`
- `lua/npc/schedule_generator.lua`
- `lua/pathfinding/pathfinding_config.lua`
- `lua/faction/faction_model.lua`
- `manifests/npc_pathfinding.manifest.json`
- `tests/npc_pathfinding_examples.lua`
- `BATCH_014_REPORT.md`

## Module summary

### `npc/npc_archetype_generator/v1`

Generates NPC archetype IR for:

- static NPCs;
- walking NPCs;
- scheduled NPCs;
- faction membership and faction role references;
- interaction components;
- dialogue source references;
- schedule and pathfinding profile references.

It intentionally does not create full entity instances. It produces reusable archetype definitions that can be consumed by future entity placement, interaction and Unity adapter modules.

### `npc/schedule_generator/v1`

Generates schedule IR with:

- tick, turn, clock or day-phase time units;
- daily, weekly, scenario or non-looping schedules;
- location references;
- pathfinding goal references;
- interaction availability windows;
- fallback idle entry.

It supports realtime, turn-based and mixed games by expressing time as metadata, not by executing ticks.

### `pathfinding/pathfinding_config/v1`

Generates pathfinding profile IR for:

- orthogonal 4-way grids;
- diagonal 8-way grids;
- axial hex grids;
- movement costs;
- passable and blocked tags;
- dynamic obstacle classes;
- replanning policy;
- movement-intent and reachability-request metadata.

This module does not solve paths. It describes how a host or future runtime adapter should request pathfinding.

### `faction/faction_model/v1`

Generates faction IR for:

- faction definitions;
- role catalog entries;
- role capabilities;
- relationship matrix;
- reputation track references.

The faction model is designed for NPC role selection, dialogue gating, quest state checks and combat attitude metadata.

## Data contracts

### NPC behavior types

```text
static
walking
scheduled
```

### Schedule time units

```text
tick
turn
clock
day_phase
```

### Pathfinding grid types

```text
orthogonal_4
diagonal_8
hex_axial
```

### Relation states

```text
ally
friendly
neutral
tense
hostile
```

## Compatibility

The batch supports:

- `realtime`;
- `turn_based`;
- `mixed`;
- `paused_planning`.

Combat modes are metadata-compatible with:

- `none`;
- `realtime`;
- `turn_based`;
- `tactical`;
- `dialogue_combat`;
- `hybrid`.

NPC and faction output can be used by no-combat games, tactical RPGs, dialogue-combat games and mixed realtime exploration with turn-based combat.

## Non-goals

- No C# integration.
- No Unity object generation.
- No Lua sandbox/executor integration.
- No live pathfinding algorithm.
- No live schedule simulation.
- No large NPC, faction or dialogue content dumps.
- No filesystem, network or process access.
