# Batch 007 Report — Roads, paths, barriers, reachability

## Files generated

- `lua/world/path_carver.lua`
- `lua/world/road_generator.lua`
- `lua/world/barrier_generator.lua`
- `lua/world/reachability.lua`
- `docs/lua/world_paths_barriers_reachability.md`
- `manifests/world_paths.manifest.json`
- `tests/world_paths_examples.lua`
- `BATCH_007_REPORT.md`

## Contracts introduced

### `world/path_carver/v1`

Creates a compact deterministic path between a start cell and objective cell, optionally through waypoints. It returns ordered path cells and sparse tile overrides.

Supported cases:

- simple paths;
- objective connection;
- waypoint routing;
- bridgeable blocked cells;
- unbridgeable blocked road diagnostics.

### `world/road_generator/v1`

Creates compact road segments between named nodes. It returns road graph metadata, road cells and sparse road/bridge/blocked-road tile overrides.

Supported cases:

- roads between named locations;
- blocked road case;
- bridge cells;
- small road graph metadata.

### `world/barrier_generator/v1`

Creates barriers, gates and bridges as compact sparse tile/passability overrides.

Supported cases:

- line barriers;
- rectangle/perimeter barriers;
- gates;
- bridges;
- road blocks.

### `world/reachability/v1`

Validates whether objective cells are reachable from a start cell.

Supported cases:

- cardinal or diagonal adjacency;
- blocked cells;
- passable override cells;
- gates/bridges as passable exceptions;
- reachable/unreachable diagnostics.

## Dependencies between files

The Lua modules are intentionally standalone and do not load each other. This keeps them compatible with a future host-side importer/sandbox that can inject module tables explicitly.

Conceptual flow:

1. `barrier_generator` emits barriers/gates/bridges.
2. `path_carver` or `road_generator` emits paths/roads.
3. `reachability` validates important objectives against compact walkability data.

## Manual validation

Recommended checks:

1. Confirm ZIP contains exactly the expected Batch 007 files.
2. Confirm each Lua file returns a table.
3. Confirm each module exposes:
   - `manifest`
   - `validate_config(config)`
   - `generate(input, ctx)`
4. Validate `manifests/world_paths.manifest.json` as JSON.
5. Search Lua files for forbidden API calls:
   - file system;
   - system access;
   - external module loading;
   - dynamic loading;
   - direct nondeterministic random calls.
6. In a Lua 5.4 sandbox, load modules through the host importer and call `tests/world_paths_examples.lua` by injecting module tables into `examples.run(...)`.

## Known limitations

- `path_carver` and `road_generator` use simple deterministic axis movement, not full A*.
- `road_generator.summary.blocked_road_count` only records whether at least one road was blocked, not a precise count per all blocked roads.
- `barrier_generator` supports compact line/rect/perimeter shapes only.
- `reachability` is validation-oriented and not a complete runtime pathfinding engine.
- No Unity objects, C# code, runtime integration or filesystem access are generated.
- No huge tile arrays are emitted.

## Next recommended batch

Next planned batch is Batch 008 — Entity and interaction foundation.

## Scope confirmation

- Batch 007 only.
- Batch 008 was not generated.
- C# project files were not modified.
- Output is intended as a standalone generator-library artifact.
