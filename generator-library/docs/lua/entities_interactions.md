# Batch 008 — Entity and interaction foundation

## Purpose

This batch introduces the first compact entity and interaction foundation for the Lua generator library. It does not implement a full runtime. It emits deterministic, JSON-serializable IR that a future host runtime can validate, import and execute.

## Files

- `lua/entity/entity_factory.lua`
- `lua/interaction/targeting.lua`
- `lua/interaction/inspect_object.lua`
- `lua/interaction/talk_to_npc.lua`
- `manifests/entities_interactions.manifest.json`
- `tests/entities_interactions_examples.lua`

## Entity factory

### When to use

Use `entity/entity_factory/v1` when a game recipe needs normalized entity prototypes and placed instances before world, dialogue, quest or runtime interaction systems consume them.

### When not to use

Do not use it as a full ECS runtime, physics layer, AI scheduler or Unity prefab instantiator. It only creates compact data contracts.

### Manifest summary

- Category: `entity`
- Capabilities: `entity.prototype.define`, `entity.instance.create`, `interaction.component.index`
- Runtime targets: `debug`, `unity2d`, `unity_tilemap`
- Turn modes: realtime, turn-based, mixed and paused planning

### Input schema

```lua
{
  prototypes = {
    {
      id = "entity/npc/elder",
      kind = "npc",
      title = "Village Elder",
      tags = { "npc", "quest" },
      components = {
        interactable = { actions = { "talk", "inspect" }, prompt = "Talk", priority = 10 },
        dialogue_source = { dialogue_id = "dialogue/npc/elder", speaker_name = "Elder" },
        inspectable = { title = "Village Elder", summary = "Looks worried." },
        quest_target = { quest_id = "quest/investigate_road", objective_id = "talk_elder" },
        collidable = { blocks_movement = true }
      }
    }
  },
  instances = {
    { id = "entity/npc/elder/main", prototype_id = "entity/npc/elder", x = 2, y = 3, facing = "south" }
  }
}
```

### Config schema

```lua
{
  allowed_components = { "interactable", "collidable", "dialogue_source", "inspectable", "quest_target" },
  require_position = true,
  default_facing = "south",
  max_prototypes = 128,
  max_instances = 512
}
```

### Output schema

The module returns normalized `prototypes`, normalized `instances`, and compact indexes:

- `instance_index_by_id`
- `instance_ids_by_prototype`
- `instance_ids_by_component`

The output is suitable for future runtime interaction lookup and Unity-facing importer work.

## Targeting

### When to use

Use `interaction/targeting/v1` to select a candidate target for inspect/talk/use actions based on actor position and entity placement.

### Supported target modes

- `facing_cell`
- `same_cell`
- `cardinal_adjacent`
- `diagonal_adjacent`
- `radius`

### Multiple target disambiguation

The module returns:

- `candidates`
- `selected`
- `needs_disambiguation`
- diagnostics warning when multiple valid candidates exist

Supported deterministic disambiguation:

- `nearest`
- `highest_priority`
- `first`
- `explicit_only`

If `explicit_only` is used, the runtime can show a target picker instead of accepting the first candidate.

## Inspect object

### When to use

Use `interaction/inspect_object/v1` when the selected target has `components.inspectable` and the runtime needs an inspection event IR.

### Output

The output contains:

- `interaction.type = "inspect"`
- `facts_revealed`
- small `ui` hint
- summary metadata

It does not create long prose. The inspect summary is taken from entity data and can be length-limited by config.

## Talk to NPC

### When to use

Use `interaction/talk_to_npc/v1` when the selected target has `components.dialogue_source` and the runtime needs a dialogue start request.

### Output

The output contains:

- `interaction.type = "talk"`
- `dialogue_start.dialogue_id`
- `dialogue_start.opening_node_id`
- speaker metadata
- optional dialogue-combat bridge flags

It does not generate dialogue nodes. Dialogue generation starts in later dialogue batches.

## Validation rules

- Entity ids, prototype ids, quest ids and dialogue ids use lowercase slash ids.
- Coordinates are integer and 0-based.
- Components must be tables.
- Normal user validation failures return diagnostics, not thrown errors.
- Outputs contain only strings, numbers, booleans, arrays and dictionaries.

## LLM prompting hints

Good prompt shape:

```text
Create entity prototypes for a small RPG village: elder NPC, blocked gate, suspicious shrine.
Use only compact inspect summaries and component metadata.
Do not write dialogue text yet; reference dialogue ids only.
```

Bad prompt shape:

```text
Generate every NPC conversation and every object description in the full open world.
```

## Extension points

Future batches can extend this foundation with:

- dialogue node generation;
- quest objective binding;
- inventory/item interactions;
- combat and dialogue-combat effects;
- NPC schedules and pathfinding;
- UI IR for selection prompts.

## Runtime target notes

The modules intentionally output IR, not Unity objects. A future C# importer can map entity instances to prefabs, colliders, interaction triggers and dialogue launchers.

## Unity/codegen notes

Unity-facing layers should treat these outputs as declarative data:

- `components.collidable` can map to collider settings;
- `components.interactable` can map to an interaction trigger;
- `components.inspectable` can map to an inspection panel request;
- `components.dialogue_source` can map to a dialogue graph lookup.

No direct C# or Unity scene generation is performed in this batch.
