# Batch 006 — Chunk/grid map generation

## Purpose

This batch introduces compact chunk and grid map generation modules for AI Game Builder / LLMGameCreator. The modules produce deterministic chunk IR, sparse tile overrides, road/blocker data, landmark placements, walkability metadata, and minimap layer data.

The batch is intentionally not a full terrain engine. It is a reviewable generator library layer that can be called by a future C# sandbox/importer after manifest validation.

## Files

- `lua/world/chunk_generator.lua`
- `lua/world/tile_painter.lua`
- `lua/world/landmark_placer.lua`
- `docs/lua/chunk_generation.md`
- `manifests/chunk_generation.manifest.json`
- `tests/chunk_generation_examples.lua`
- `BATCH_006_REPORT.md`

## Module summaries

### `world/chunk_generator/v1`

Generates a chunk descriptor with:

- chunk coordinates and size;
- seed metadata;
- default tile;
- deterministic terrain sparse overrides;
- explicit sparse overrides;
- roads;
- blocked road cells;
- landmarks;
- walkability overrides;
- minimap layer points;
- optional full tile array only when explicitly requested and below a configured limit.

### `world/tile_painter/v1`

Applies compact paint operations:

- `set` — one tile;
- `rect` — rectangular sparse area;
- `line` — simple orthogonal line;
- `road` — road polyline with optional blocked cells.

### `world/landmark_placer/v1`

Places landmark descriptors deterministically inside a chunk. It can use fixed positions or seeded candidates and avoids blocked/unwalkable cells unless the caller explicitly allows blocked placement at a higher layer.

## When to use

Use this batch when the game needs:

- finite map chunks;
- infinite seeded chunks;
- compact chunk previews;
- sparse terrain variation;
- road/minimap previews;
- landmarks such as towns, ruins, shrines, exits, caves, factories, or resource nodes;
- walkability hints for future pathfinding and runtime adapters.

## When not to use

Do not use these modules for:

- final high-fidelity procedural terrain;
- thousands of baked tile entries when sparse output is enough;
- direct Unity object generation;
- pathfinding proof of reachability; that belongs to the next path/reachability batch;
- raw C# generation.

## Input and config model

The modules accept compact Lua tables and return JSON-serializable tables. They do not depend on file system, network, external libraries, or direct Unity APIs.

### Example chunk generator config

```lua
{
  chunk_size = { width = 16, height = 16 },
  seed = 12345,
  default_tile_id = "tile/grass",
  default_minimap_key = "grass",
  terrain_rules = {
    { tile_id = "tile/forest", threshold = 1800, walkable = true, minimap_key = "forest", tags = { "forest" } },
    { tile_id = "tile/water", threshold = 500, walkable = false, minimap_key = "water", tags = { "water" } }
  },
  roads = {
    {
      points = { { x = 0, y = 8 }, { x = 15, y = 8 } },
      tile_id = "tile/road",
      blocked_cells = { { x = 7, y = 8 } },
      blocked_tile_id = "tile/blocked_road"
    }
  },
  landmarks = {
    { id = "landmark/old_well", tile_id = "tile/landmark_well", position = { x = 4, y = 5 }, minimap_key = "landmark" }
  }
}
```

## Output schema explained

Common output fields:

- `chunk` — chunk coordinate and size metadata.
- `seed` — deterministic seed used by the module.
- `default_tile` — tile assumed for all unspecified cells.
- `sparse_tiles` — explicit cell overrides.
- `walkability_overrides` — compact list of cells that differ from default walkability.
- `minimap_layer` — default minimap key plus sparse minimap points.
- `landmarks` — placed landmark descriptors.
- `representation` — text marker indicating sparse representation.
- `full_tiles` — optional full tile array only when requested and allowed by limit.
- `full_tiles_omitted` — true when full array is intentionally omitted.

## Sparse representation rule

The default representation is:

```text
default_tile + sparse_tiles
```

This is deliberate. A 128x128 chunk should not be emitted as 16,384 repeated tile objects just to say most cells are grass. Runtime preview, Unity adapter, or codegen can expand the chunk later when needed.

## Roads and blocked road case

Roads are modeled as sparse road cells. Blocked road cells are modeled as `layer = "blocker"`, `walkable = false`, and `tags = { "road", "blocked" }`.

This is enough for:

- world preview;
- future path validation;
- quest hooks such as “clear the road”; 
- minimap warning markers;
- runtime interaction with blocked passage.

Actual reachability validation is intentionally deferred to Batch 007.

## Walkability

Walkability is stored at the tile override level and summarized in `walkability_overrides`. A default grass tile can be walkable while water, cliff, blocked road, or barrier overrides can be non-walkable.

## LLM prompting hints

When selecting these modules, prompt the LLM to provide:

1. world scale: finite, chunked, infinite seeded;
2. chunk size;
3. default tile;
4. 2–5 terrain rules maximum;
5. road endpoints and blocked road cells if needed;
6. landmark prototypes or fixed key positions;
7. whether full tile arrays are needed for a tiny debug example only.

Avoid asking the LLM to print a complete map. Ask it for generator config and constraints.

## Validation rules

- ids must be lowercase slash ids;
- chunk width/height must be positive integers;
- seed must be an integer when supplied;
- sparse cells outside chunk bounds are skipped with warnings;
- invalid tile ids create diagnostics;
- full tile arrays are omitted when chunk size exceeds `max_full_tiles`.

## Extension points

Future batches can add:

- path carving;
- road graph generation;
- barriers and gates;
- reachability validation;
- biome-weighted terrain rules;
- resource node placement;
- Unity Tilemap IR output;
- pathfinding masks;
- streaming chunk registry.

## Runtime target notes

The output is suitable for debug preview, simulation, Unity 2D/3D adapters, and codegen IR preparation. It does not create Unity objects directly.

## Unity/codegen notes

A Unity adapter should read:

- `default_tile` as the base fill;
- `sparse_tiles` as Tilemap overrides;
- `walkability_overrides` as collision/path masks;
- `minimap_layer` as minimap texture or marker metadata;
- `landmarks` as prefab slot hints, not raw prefab references.
