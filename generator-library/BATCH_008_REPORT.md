# BATCH 008 REPORT — Entity and interaction foundation

## Files generated

- `lua/entity/entity_factory.lua`
- `lua/interaction/targeting.lua`
- `lua/interaction/inspect_object.lua`
- `lua/interaction/talk_to_npc.lua`
- `docs/lua/entities_interactions.md`
- `manifests/entities_interactions.manifest.json`
- `tests/entities_interactions_examples.lua`
- `BATCH_008_REPORT.md`

## Contracts introduced

### Entity prototype contract

A prototype is a compact declarative record:

```text
id, kind, title, tags, components, defaults
```

Supported first-class components:

- `interactable`
- `collidable`
- `dialogue_source`
- `inspectable`
- `quest_target`

Unknown component tables are copied as generic metadata with a warning, so future batches can introduce new components without breaking old recipes.

### Entity instance contract

An instance references a prototype:

```text
id, prototype_id, x, y, map_id, region_id, facing, tags, state
```

Coordinates are integer and 0-based.

### Targeting contract

Target selection supports:

- `facing_cell`
- `same_cell`
- `cardinal_adjacent`
- `diagonal_adjacent`
- `radius`

Targeting returns candidates, selected target, target cell, and a `needs_disambiguation` flag.

### Interaction IR contracts

`inspect_object` emits:

```text
interaction.type = inspect
facts_revealed
ui panel hint
```

`talk_to_npc` emits:

```text
interaction.type = talk
dialogue_start request
optional dialogue_combat bridge flags
```

## Dependencies between files

The Lua modules are intentionally self-contained and do not call `require`, `dofile`, `loadfile`, `package`, filesystem, network or host APIs.

Logical dependencies:

- `targeting.lua` consumes entity-like instances produced by `entity_factory.lua`.
- `inspect_object.lua` expects a selected target with `components.inspectable`.
- `talk_to_npc.lua` expects a selected target with `components.dialogue_source`.

## How to validate manually

1. Load each Lua file through the host importer/sandbox.
2. Verify that every file returns a table.
3. Verify each table has `manifest`, `validate_config(config)` and `generate(input, ctx)`.
4. Bind the four module tables to `tests/entities_interactions_examples.lua` through the host test harness.
5. Call:

```text
examples.run(entity_factory, targeting, inspect_object, talk_to_npc)
```

Expected result shape:

- `results.entities.ok == true`
- `results.targeting.data.selected.id == "entity/npc/elder/main"`
- `results.inspect.data.interaction.type == "inspect"`
- `results.talk.data.interaction.type == "talk"`

## Known limitations

- This batch does not implement a real ECS runtime.
- It does not generate dialogue nodes; it only emits dialogue start IR.
- It does not execute quest state changes; quest ids are references for later validation.
- It does not implement Unity prefab mapping; Unity-facing mapping is left to later adapter/codegen IR batches.
- Targeting is grid-based and 2D only.

## Next recommended batch

Batch 009 — Dialogue generation foundation.

## No broad claims

This batch only implements compact entity normalization, target selection, inspection IR and dialogue start IR. It does not claim to solve dialogue generation, quest progression, inventory, combat, AI, pathfinding or Unity integration.
