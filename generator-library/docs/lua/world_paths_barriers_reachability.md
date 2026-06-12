# Batch 007 — World paths, barriers, reachability

## Purpose

This batch adds compact deterministic infrastructure for path, road, barrier, gate, bridge and reachability IR generation.

The modules are designed for AI Game Builder / LLMGameCreator as reusable generator-library assets. They should help an LLM select capabilities and configure generation, while the program/runtime keeps validation, preview and later Unity adaptation under control.

## Files

- `lua/world/path_carver.lua`
- `lua/world/road_generator.lua`
- `lua/world/barrier_generator.lua`
- `lua/world/reachability.lua`
- `manifests/world_paths.manifest.json`
- `tests/world_paths_examples.lua`

## Shared design rules

All generated outputs are compact IR tables, not full scene objects and not large raw map dumps. Coordinates are 0-based. The modules use sparse tile overrides and small summaries so they can be used with finite maps, chunked worlds and future infinite seeded worlds.

Each module follows the common contract:

- `manifest`
- `validate_config(config)`
- `generate(input, ctx)`
- result object: `{ ok, data, diagnostics, artifacts }`
- diagnostics instead of normal validation exceptions
- JSON-serializable outputs

No module performs file access, networking, external loading or global mutation.

## Module: path_carver

### Purpose

`world/path_carver/v1` carves a deterministic path from a start cell to an objective cell, optionally through waypoints. It can mark bridge cells when a blocked cell is explicitly listed as bridgeable.

### When to use

Use it when a generator plan needs a guaranteed simple route between two important points, for example:

- player spawn to quest objective;
- village gate to dungeon entrance;
- tactical entry cell to extraction point;
- road stub inside one chunk.

### When not to use

Do not use it as a complete pathfinding implementation, traffic simulation, navigation mesh builder or large-world road network planner. Use it for compact deterministic IR and validation-friendly examples.

### Input schema explained

Required:

- `bounds = { width, height }`
- `start = { x, y }`
- `objective = { x, y }` or `goal = { x, y }`

Optional:

- `waypoints = { { x, y }, ... }`
- `blocked_cells = { { x, y }, ... }`
- `bridge_cells = { { x, y }, ... }`

### Config schema explained

- `road_tile`: tile id for path cells.
- `bridge_tile`: tile id for bridge cells.
- `path_order`: `horizontal_first`, `vertical_first` or `alternating`.
- `allow_bridges`: when false, bridgeable blocked cells still block the route.
- `max_cells`: safety limit for compact generation.

### Output schema explained

- `path`: ordered compact cells with role and tile metadata.
- `sparse_tiles`: path tile overrides for a map/chunk layer.
- `blocked`: true when the route could not be completed.
- `summary`: small metadata for logs/preview.

### Validation rules

The module validates bounds, integer positions, config field types, route bounds and unbridgeable blocked cells. Normal failures are returned as diagnostics.

### Extension points

Later batches can replace the simple axis route with A*, weighted paths, terrain costs, nav profiles, faction territory costs or city-builder road cost rules while keeping the same compact output shape.

## Module: road_generator

### Purpose

`world/road_generator/v1` generates deterministic road segments between named nodes. It outputs road graph metadata and sparse road/bridge/blocked-road tiles.

### When to use

Use it when world generation needs roads between locations or landmarks:

- village to forest;
- settlement to mine;
- city district roads;
- region-level travel graph preview.

### When not to use

Do not use it for high-fidelity civil engineering, traffic lanes or full simulation. It does not calculate road economics or terrain terraforming.

### Input schema explained

Required:

- `bounds`
- `nodes = { { id, x, y }, ... }`
- `roads = { { from, to, kind }, ... }`

Optional:

- `blocked_cells`
- `bridge_cells`

### Config schema explained

- `road_tile`
- `bridge_tile`
- `blocked_road_tile`
- `allow_bridges`
- `max_cells_per_road`

### Output schema explained

- `road_segments`: compact per-road cell arrays.
- `sparse_tiles`: road layer overrides.
- `road_graph`: nodes and edges for later validation/runtime.
- `summary`: count metadata.

### Validation rules

The module reports duplicate nodes, missing endpoints, out-of-bounds nodes and unbridgeable blocked roads.

### Extension points

Future extensions can add road classes, path costs, biome-aware routing, roads across chunks, city-builder zoning integration and runtime pathfinding hints.

## Module: barrier_generator

### Purpose

`world/barrier_generator/v1` creates compact barrier, gate and bridge overrides.

### When to use

Use it when a map needs obstacles and explicit passable exceptions:

- walls with gates;
- rivers with bridges;
- blocked roads;
- perimeter boundaries;
- tactical cover/blocking layer.

### When not to use

Do not use it for destructive terrain simulation, physics collision meshes or raw Unity GameObject placement. It is IR-only.

### Input schema explained

Required:

- `bounds`

Optional:

- `barriers`: array of barrier specs.
- `gates`: passable positions that override barriers.
- `bridges`: passable positions that override barriers or water-like blocks.

Barrier shapes:

- `line`: requires `from` and `to` positions.
- `rect`: requires `x`, `y`, `width`, `height`; emits perimeter unless `filled = true`.
- `perimeter`: emits map border.

### Config schema explained

- `wall_tile`
- `gate_tile`
- `bridge_tile`
- `road_block_tile`
- `max_tiles`

### Output schema explained

- `sparse_tiles`: compact barrier/gate/bridge tile overrides.
- `passability_overrides`: walkability metadata for reachability and runtime validators.
- `barriers`: normalized summary of emitted barrier specs.

### Validation rules

The module validates bounds, barrier shape data, passable point positions and config field types. Out-of-bounds emitted tiles are skipped with warnings.

### Extension points

Future extensions can add locked gates, destructible barriers, faction doors, keys, bridge repair states, water crossings and dynamic obstacles.

## Module: reachability

### Purpose

`world/reachability/v1` validates whether one or more objectives are reachable from a start cell using compact walkability data.

### When to use

Use it after path, road, barrier or chunk generation to verify that critical gameplay targets are reachable:

- spawn to objective;
- town entrance to NPC;
- quest item behind a gate;
- blocked road diagnostics.

### When not to use

Do not use it as a full runtime pathfinding engine. It produces validation diagnostics and compact metadata only.

### Input schema explained

Required:

- `bounds`
- `start`
- `objectives`

Optional:

- `blocked_cells`
- `passable_cells`
- `gates`
- `bridges`
- `sparse_tiles` with `walkable` metadata

### Config schema explained

- `adjacency`: `cardinal` or `diagonal`.
- `default_walkable`: default for cells without overrides.
- `max_visited`: safety limit for scan size.

### Output schema explained

- `reachable`: true when all objectives are reachable.
- `reachable_objectives`
- `unreachable_objectives`
- `visited_count`
- `diagnostics_summary`

### Validation rules

The module validates bounds, start, objectives, adjacency mode and safety limits. It returns `reachability.objective_unreachable` for every unreachable target.

### Extension points

Future modules can add movement profiles, terrain costs, actor sizes, doors, keys, one-way links, tactical grid rules and city-builder road access layers.

## Example config

```lua
local ctx = {
  config = {
    road_tile = "tile/road/dirt",
    bridge_tile = "tile/bridge/wood",
    path_order = "horizontal_first",
    allow_bridges = true
  }
}
```

## Example input

```lua
local input = {
  bounds = { width = 8, height = 6 },
  start = { x = 1, y = 1 },
  objective = { x = 6, y = 4 },
  blocked_cells = { { x = 3, y = 1 } },
  bridge_cells = { { x = 3, y = 1 } }
}
```

## Example output shape

```lua
{
  ok = true,
  data = {
    sparse_tiles = {
      { x = 1, y = 1, tile = "tile/road/dirt", walkable = true }
    },
    summary = { connected = true }
  },
  diagnostics = {},
  artifacts = {}
}
```

## LLM prompting hints

When asking an LLM to configure these modules, prefer small semantic constraints:

- start/objective landmarks;
- intended blockages;
- gates/bridges as explicit exceptions;
- road graph nodes rather than full tile maps;
- validation objective list.

Avoid asking the LLM to print entire world maps. Let these modules produce compact IR from configs.

## Runtime target notes

These modules output data that a C# importer or Unity adapter can translate later into tilemap layers, navigation metadata, minimap overlays and validation messages.

## Unity/codegen notes

The modules do not emit Unity objects or C# code. Future Unity IR/codegen modules can consume:

- `sparse_tiles` as tilemap overrides;
- `passability_overrides` as collider/nav metadata;
- `road_graph` as route/minimap metadata;
- reachability diagnostics as editor warnings.
