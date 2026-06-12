# Batch 006 Report — Chunk/grid map generation

## Files generated

- `lua/world/chunk_generator.lua`
- `lua/world/tile_painter.lua`
- `lua/world/landmark_placer.lua`
- `docs/lua/chunk_generation.md`
- `manifests/chunk_generation.manifest.json`
- `tests/chunk_generation_examples.lua`
- `BATCH_006_REPORT.md`

## Contracts introduced

### Sparse chunk representation

The default map representation is:

```text
default_tile + sparse_tiles
```

This prevents huge repeated tile arrays for chunks where most cells share the same tile.

### Chunk output contract

`chunk_generator.generate(input, ctx)` returns:

- `ok`
- `data.chunk`
- `data.seed`
- `data.default_tile`
- `data.sparse_tiles`
- `data.landmarks`
- `data.walkability_overrides`
- `data.minimap_layer`
- `data.representation`
- `data.full_tiles` only when explicitly requested and below limit
- `diagnostics`
- `artifacts`

### Road/blocker contract

Road cells are sparse overrides with `layer = "road"`. Blocked road cells are sparse overrides with `layer = "blocker"`, `walkable = false`, and tags containing `road` and `blocked`.

### Minimap layer contract

Minimap output is compact:

- `width`
- `height`
- `default_key`
- `points`

It does not emit a full minimap texture or full cell array.

## Dependencies between files

There are no runtime `require` dependencies. The files are designed to be imported by a future C# Lua module registry/sandbox.

Contract-level alignment:

- Batch 001 diagnostics/result style;
- Batch 002 lowercase slash ids and 0-based grid coordinates;
- Batch 005 world blueprint chunked/infinite world assumptions.

## How to validate manually

1. Inspect the ZIP structure and verify all expected files exist.
2. Parse `manifests/chunk_generation.manifest.json` as JSON.
3. Load each Lua file in a sandboxed Lua 5.4 environment.
4. Confirm each module returns a table.
5. Confirm each module has `manifest`, `validate_config(config)`, and `generate(input, ctx)`.
6. Inject the three modules into `tests/chunk_generation_examples.lua` and call `run(modules)`.
7. Confirm chunk output uses sparse representation unless `include_full_tiles = true` and the chunk is below `max_full_tiles`.

## Known limitations

- Terrain variation uses a compact deterministic hash, not real Perlin/simplex noise.
- Road lines are simple orthogonal/step paths, not full graph roads.
- Blocked road cells are represented, but reachability is not proven in this batch.
- Landmark placement is deterministic and compact but not a full constraint solver.
- No direct Unity Tilemap output is generated; output remains adapter-friendly IR.

## Next recommended batch

Batch 007 — Roads, paths, barriers, reachability.

It should add path carving, road graph generation, barriers, gates, bridges, blocked-road diagnostics, and reachability checks from start to objective.

## Scope confirmation

- No C# project files were modified.
- No Batch 007 files were generated.
- No external dependencies were added.
- No huge hardcoded tile maps were generated.
