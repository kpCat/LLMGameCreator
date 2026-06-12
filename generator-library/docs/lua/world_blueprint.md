# Batch 005 — World blueprint

## Purpose

This batch introduces compact world blueprint infrastructure for AI Game Builder / LLMGameCreator. It does not generate full tile maps. It generates reusable, validated metadata and intermediate representation for later modules that will create chunks, roads, landmarks, reachability data, UI IR, Unity target IR, and future codegen IR.

## Files

- `lua/world/world_blueprint.lua`
- `lua/world/region_graph.lua`
- `lua/world/biome_catalog.lua`
- `manifests/world_blueprint.manifest.json`
- `tests/world_blueprint_examples.lua`
- `BATCH_005_REPORT.md`

## Modules

### `world/world_blueprint/v1`

Builds a compact world blueprint IR. It supports:

- `finite_map`
- `multi_map`
- `region`
- `chunked_world`
- `infinite_seeded_world`

It also records global map and minimap metadata, zero-based coordinate conventions, sparse-generation policy, chunking metadata, biome references, region references, and region connections.

Use it when the LLM has discussed the game's world shape with the user and needs a small structured artifact for generator orchestration.

Do not use it to print a large tile array. Batch 006 is responsible for chunk/grid map generation, and it must still avoid huge tile arrays when sparse data is enough.

### `world/region_graph/v1`

Validates and normalizes region nodes and connections. It can be used by RPG, open-world, multi-map, city-builder district, or strategy-region workflows.

A region can have:

- `id`
- `title`
- `map_id`
- `biome_id`
- `position`
- `tags`
- `minimap`
- `metadata`

A connection can have:

- `from`
- `to`
- `type`
- `bidirectional`
- `blocked`
- `gate_id`
- `tags`
- `metadata`

The module emits adjacency data suitable for later validation and path/reachability modules.

### `world/biome_catalog/v1`

Normalizes biome definitions and indexes them by id, tag, resource, and climate band.

Each biome supports:

- lowercase slash `id`
- `title`
- `temperature` from `0` to `1`
- `humidity` from `0` to `1`
- `danger` from `0` to `1`
- `tags`
- `resources`
- `minimap` metadata

This is intentionally compact. It is a catalog/IR helper, not a procedural terrain painter.

## Example config

```lua
local config = {
  world_id = "world/cursed_valley",
  title = "Cursed Valley",
  blueprint_mode = "region",
  world_scale = "region",
  seed = 12345,
  biomes = {
    {
      id = "biome/dark_forest",
      title = "Dark Forest",
      temperature = 0.38,
      humidity = 0.75,
      danger = 0.65,
      tags = { "forest", "shadow" },
      resources = { "resource/wood", "resource/mushroom" }
    }
  },
  maps = {
    {
      id = "map/valley_overworld",
      title = "Valley Overworld",
      bounds = { x = 0, y = 0, width = 128, height = 96 },
      default_biome_id = "biome/dark_forest"
    }
  },
  regions = {
    { id = "region/old_road", title = "Old Road", map_id = "map/valley_overworld", biome_id = "biome/dark_forest" },
    { id = "region/ruined_gate", title = "Ruined Gate", map_id = "map/valley_overworld", biome_id = "biome/dark_forest" }
  },
  connections = {
    { from = "region/old_road", to = "region/ruined_gate", type = "road", bidirectional = true }
  }
}
```

## Example input

```lua
local input = {
  blueprint_mode = "region"
}
```

## Example output shape

```lua
{
  ok = true,
  data = {
    world = {
      id = "world/cursed_valley",
      blueprint_mode = "region",
      world_scale = "region",
      seed = 12345,
      coordinate_system = { origin = "zero_based" }
    },
    maps = {},
    biomes = {},
    regions = {},
    connections = {},
    chunking = {},
    global_map = {},
    minimap = {},
    generation_policy = {
      emit_huge_tile_arrays = false,
      prefer_sparse_overrides = true
    },
    counts = {}
  },
  diagnostics = {},
  artifacts = {}
}
```

## Validation rules

- World, map, biome, region, resource, and gate identifiers use lowercase slash id style.
- Coordinates are zero-based by convention.
- Bounds use integer `x`, `y`, positive integer `width`, and positive integer `height`.
- Biome climate values are numeric and normalized into the `0..1` interval during generation.
- Region connections must reference existing regions.
- Map references and biome references are checked where supplied.
- Infinite seeded world mode records seed and chunk metadata but does not generate infinite content immediately.

## LLM prompting hints

Good prompt shape:

```text
Create a compact world blueprint for a dark fantasy village RPG.
World scale: region.
Use 3-5 regions, 2 biomes, a global map, and minimap metadata.
Do not generate tile arrays.
```

Bad prompt shape:

```text
Generate a 500x500 tile world as raw JSON.
```

The LLM should choose the blueprint module, fill compact config, and let later modules generate chunks, paths, landmarks, UI IR, and Unity IR.

## Extension points

- Batch 006 can consume `world_blueprint.data.chunking`, `maps`, `biomes`, and `regions`.
- Batch 007 can consume `region_graph.data.adjacency` and `connections`.
- Batch 017 can consume `global_map` and `minimap` metadata for UI IR.
- Batch 018 can consume the blueprint for Unity scene and codegen IR planning.
- Batch 019 can validate missing references, reachability, and module contract mismatch.

## Runtime target notes

The modules target debug tools, Unity adapters, simulation planners, and codegen IR planners. They intentionally avoid direct Unity object construction.

## Unity/codegen notes

The output is Unity-facing only as abstract IR. A future Unity adapter can translate maps, regions, minimap layers, and chunking metadata into scenes, ScriptableObjects, prefabs, or generated glue code. This batch does not produce raw C#.
