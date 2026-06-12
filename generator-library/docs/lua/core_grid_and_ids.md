# Batch 002 — Core grid and IDs

## Purpose

Batch 002 adds three compact core modules for data-driven game generation:

- `lua/core/id.lua` validates and builds lowercase slash IDs.
- `lua/core/coordinates.lua` handles 0-based 2D positions, chunk/local conversion, facing, adjacency, and target disambiguation.
- `lua/core/grid.lua` represents a finite or unbounded sparse 2D grid with default cells and sparse overrides.

These modules are intentionally low-level. They are not RPG, city-builder, automation, UI, Unity, or combat systems by themselves. They provide stable contracts that later batches can reuse for worlds, chunks, entities, interactions, dialogue-combat targeting, UI IR, Unity target IR, and future codegen IR.

## When to use

Use this batch when a generator needs:

- stable IDs like `world/chunk/cursed_forest`, `entity/npc/elder`, or `quest/investigate_road`;
- 0-based tile positions;
- conversion between world position, chunk coordinate, and local position inside a chunk;
- finite chunk-sized grids with sparse tile overrides;
- unbounded sparse grids for open-world or debug planning data;
- target cells in front of an actor;
- same-cell, cardinal-adjacent, diagonal-adjacent, radius, or facing-cell target checks;
- deterministic disambiguation when multiple targets are valid.

## When not to use

Do not use these modules as a full pathfinding system, full map generator, ECS runtime, combat resolver, UI renderer, Unity object builder, or content database. Later batches should build those concerns on top of these helpers.

## Manifest summary

### `core/id/v1`

- Category: `core`
- Capabilities: `core.id.validate`, `core.id.build`, `core.id.split`
- Deterministic: true
- Runtime targets: debug, Unity 2D/3D, simulation, codegen IR

### `core/coordinates/v1`

- Category: `core`
- Capabilities: position2d, chunk/local coordinates, facing, adjacency, target disambiguation
- Deterministic: true
- Runtime targets: debug, Unity 2D/3D, simulation, codegen IR

### `core/grid/v1`

- Category: `core`
- Capabilities: create grid, bounds, get/set cells, sparse overrides, neighborhood, facing target
- Deterministic: true
- Runtime targets: debug, Unity 2D/3D, simulation, codegen IR

## Input schema explained

These are helper modules rather than bulk generators. They expose functions that accept JSON-like Lua tables.

Common position shape:

```lua
{ x = 0, y = 0 }
```

Actor shape for facing operations:

```lua
{
  id = "entity/player/main",
  position = { x = 4, y = 7 },
  facing = "north"
}
```

Target shape for disambiguation:

```lua
{
  id = "entity/npc/guard",
  position = { x = 4, y = 6 }
}
```

Grid config shape:

```lua
{
  width = 16,
  height = 16,
  default_cell = { tile = "grass", walkable = true }
}
```

Unbounded sparse grid shape:

```lua
{
  unbounded = true,
  default_cell = { tile = "void", walkable = false }
}
```

## Config schema explained

Each module has `validate_config(config)` for module-level configuration checks.

`core/id/v1` supports:

- `max_length`: optional integer, 1..256.

`core/coordinates/v1` supports:

- `default_radius`: optional integer, 0..128;
- `default_adjacency_mode`: optional string: `same_cell`, `cardinal_adjacent`, `diagonal_adjacent`, `radius`, or `facing_cell`.

`core/grid/v1` supports:

- `width`: optional positive integer;
- `height`: optional positive integer;
- `unbounded`: optional boolean.

`grid.create(config)` performs stricter grid-spec validation than module-level `validate_config`: finite grids need `width` and `height`; unbounded grids do not.

## Output schema explained

Public operations return either direct primitive/helper values or result objects:

```lua
{
  ok = true,
  data = {},
  diagnostics = {},
  artifacts = {}
}
```

Diagnostics follow the shared format:

```lua
{
  severity = "error",
  code = "core.grid.out_of_bounds",
  message = "position is outside grid bounds.",
  target = "position"
}
```

All result `data` fields are intended to be JSON-serializable.

## Example config

```lua
local grid_config = {
  width = 8,
  height = 8,
  default_cell = { tile = "floor", walkable = true }
}
```

## Example input

```lua
local actor = {
  id = "entity/player/main",
  position = { x = 2, y = 2 },
  facing = "east"
}

local targets = {
  { id = "entity/chest/old", position = { x = 3, y = 2 } },
  { id = "entity/npc/elder", position = { x = 2, y = 1 } }
}
```

## Example output

For `coordinates.disambiguate_targets(actor, targets, { mode = "facing_cell" })`:

```lua
{
  ok = true,
  data = {
    selected = {
      index = 1,
      target_id = "entity/chest/old",
      position = { x = 3, y = 2 },
      adjacency = "cardinal_adjacent",
      distance_manhattan = 1,
      distance_squared = 1
    },
    candidates = {},
    ambiguous = false
  },
  diagnostics = {},
  artifacts = {}
}
```

The actual `candidates` array contains the same candidate objects that matched the requested mode.

## LLM prompting hints

When asking an LLM to use these modules, keep prompts contract-focused:

- Ask for IDs in lowercase slash form.
- Ask for positions as integer `{ x, y }` tables.
- Ask for chunk sizes explicitly.
- Ask for sparse overrides instead of full tile matrices when only a few cells differ from the default.
- Ask for a target disambiguation policy when more than one object can be interacted with.

Good instruction style:

```text
Use lowercase slash IDs. Use 0-based coordinates. Return sparse overrides only. If several interaction targets match, prefer target_id chosen by the player; otherwise report ambiguity.
```

## Validation rules

### IDs

Valid IDs:

- `world/chunk/cursed_forest`
- `entity/npc/elder`
- `quest/investigate_road`

Invalid IDs:

- `World/Chunk`
- `/world/chunk`
- `world//chunk`
- `world/chunk/`
- `world/chunk with spaces`

### Coordinates

- Position fields `x` and `y` must be integers.
- Local coordinates inside a chunk are 0-based and non-negative.
- Local coordinates can be checked against chunk width/height.
- Chunk coordinates are integer grid coordinates and may be negative for large/open worlds.

### Grid

- Finite grids use 0-based bounds: valid x is `0 <= x < width`, valid y is `0 <= y < height`.
- Sparse overrides are stored by position key and only override changed cells.
- `default_cell` and override `cell` values must be provided and should be JSON-like tables/primitives.
- Out-of-bounds access returns diagnostics instead of throwing normal validation errors.

### Targeting and disambiguation

Supported modes:

- `same_cell`
- `cardinal_adjacent`
- `diagonal_adjacent`
- `radius`
- `facing_cell`

If several targets match and no `target_id`, `target_index`, `prefer = "first"`, or `prefer = "nearest"` rule resolves the selection, `coordinates.disambiguate_targets` returns an ambiguity diagnostic.

## Extension points

Later batches can build on these contracts:

- world/chunk generation can emit sparse overrides using `core/grid/v1` semantics;
- entity and interaction modules can use `core/coordinates/v1` targeting modes;
- dialogue-combat can treat nearby conversation targets as interactable combat targets;
- city-builder and Factorio-like modules can use finite grids for zones, machines, belts, services, and coverage;
- UI IR and Unity IR can preserve positions and IDs without depending on concrete Unity objects.

## Runtime target notes

These modules do not perform rendering, physics, pathfinding, file access, network access, or runtime scheduling. They are suitable as pure helper modules for debug preview, simulation, Unity adapters, and codegen IR builders.

## Unity/codegen notes

- Positions are engine-neutral integer grid coordinates.
- Facing names are normalized to `north`, `south`, `east`, `west`.
- Sparse grid overrides map cleanly to Unity tilemap patches or codegen IR patch operations.
- IDs are stable textual keys for cross-artifact references; they are not Unity instance IDs.
