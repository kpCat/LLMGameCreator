# BATCH 005 REPORT — World blueprint

## Files generated

- `lua/world/world_blueprint.lua`
- `lua/world/region_graph.lua`
- `lua/world/biome_catalog.lua`
- `docs/lua/world_blueprint.md`
- `manifests/world_blueprint.manifest.json`
- `tests/world_blueprint_examples.lua`
- `BATCH_005_REPORT.md`

## Contracts introduced

### World blueprint IR

`world/world_blueprint/v1` emits compact blueprint data:

- `world`
- `maps`
- `biomes`
- `regions`
- `connections`
- `chunking`
- `global_map`
- `minimap`
- `generation_policy`
- `counts`

The output intentionally says `emit_huge_tile_arrays = false` and keeps chunk generation deferred when chunking is enabled.

### Blueprint modes

Supported modes:

- `finite_map`
- `multi_map`
- `region`
- `chunked_world`
- `infinite_seeded_world`

### World scales

Supported scales:

- `single_map`
- `multi_map`
- `region`
- `continent`
- `planet`
- `infinite_chunks`

### Biome catalog

`world/biome_catalog/v1` normalizes biome climate/resource/tag metadata and indexes it by:

- id
- tag
- resource
- climate band

### Region graph

`world/region_graph/v1` normalizes region nodes and connections, validates references, and emits adjacency data.

## Dependencies between files

The Lua files are intentionally standalone and do not load each other. This preserves the no-filesystem/no-external-dependency rule and allows the future host registry to inject modules explicitly.

Conceptual dependencies:

- `world_blueprint.lua` aligns with Batch 002 coordinate/id rules.
- `world_blueprint.lua` aligns with Batch 003 time/combat mode declarations through manifest metadata.
- `world_blueprint.lua` aligns with Batch 004 capability/module manifest shape.
- `biome_catalog.lua` can be used before `world_blueprint.lua` to prepare biome metadata.
- `region_graph.lua` can be used before or after `world_blueprint.lua` to validate region connectivity.

## How to validate manually

1. Inspect `manifests/world_blueprint.manifest.json` as JSON.
2. Inspect each Lua module and confirm it returns a table.
3. Confirm every Lua module exposes:
   - `manifest`
   - `validate_config(config)`
   - `generate(input, ctx)`
4. Inject the three modules into `tests/world_blueprint_examples.lua` as:

```lua
local examples = batch_005_examples
examples.run_examples({
  world_blueprint = world_blueprint_module,
  region_graph = region_graph_module,
  biome_catalog = biome_catalog_module
})
```

5. Confirm the examples produce:
   - a two-biome catalog,
   - a three-region graph,
   - a region blueprint,
   - an infinite seeded blueprint with chunking enabled.

## Known limitations

- This batch does not generate chunk tile data.
- This batch does not carve roads or validate reachability beyond basic region reference checks.
- This batch does not build Unity scenes, prefabs, or C# codegen IR.
- This batch does not simulate economy, NPC schedules, combat, or automation.
- Climate handling is normalized metadata only; it is not terrain simulation.

## Next recommended batch

Batch 006 — Chunk/grid map generation.

Expected next focus:

- chunk size config;
- seed;
- default tile;
- sparse overrides;
- landmarks;
- roads;
- blocked road case;
- walkability;
- minimap layer data;
- avoiding huge tile arrays when not needed.

## Implementation notes

- No C# project files were modified.
- No Batch 006 files were generated.
- The generated modules avoid external dependencies and file-system APIs.
- Randomness is not used in this batch; seeds are recorded as deterministic metadata for later generators.
