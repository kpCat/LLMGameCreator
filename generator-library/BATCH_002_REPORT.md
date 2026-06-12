# Batch 002 Report — IDs, grid, coordinates

## Files generated

- `lua/core/id.lua`
- `lua/core/grid.lua`
- `lua/core/coordinates.lua`
- `docs/lua/core_grid_and_ids.md`
- `manifests/core_grid.manifest.json`
- `tests/core_grid_examples.lua`
- `BATCH_002_REPORT.md`

The report file uses the numbered naming convention requested after Batch 001.

## Contracts introduced

### Lowercase slash IDs

`core/id/v1` validates IDs such as:

- `world/chunk/cursed_forest`
- `entity/npc/elder`
- `quest/investigate_road`

The validator rejects uppercase letters, empty segments, leading/trailing slash, spaces, and unsupported characters.

### Position2D

The shared position shape is:

```lua
{ x = 0, y = 0 }
```

Both fields must be integers.

### Chunk and local coordinates

`core/coordinates/v1` introduces conversion between world positions and chunk/local coordinates:

```lua
world position -> chunk coord + local position
chunk coord + local position -> world position
```

Local positions inside a chunk are 0-based and non-negative. Chunk coordinates are integer grid coordinates and may be negative for open-world layouts.

### Sparse grid shape

`core/grid/v1` represents grid data as:

```lua
{
  width = 16,
  height = 16,
  unbounded = false,
  default_cell = {},
  overrides = {}
}
```

Sparse overrides are stored by deterministic position key, for example `"2,1"`.

### Facing and targeting

Supported facings are normalized to:

- `north`
- `south`
- `east`
- `west`

Accepted aliases: `up`, `down`, `left`, `right`.

Supported target modes:

- `same_cell`
- `cardinal_adjacent`
- `diagonal_adjacent`
- `radius`
- `facing_cell`

### Multiple target disambiguation

`coordinates.disambiguate_targets(actor, targets, options)` returns ambiguity diagnostics when multiple targets match and no deterministic selection rule is supplied.

Supported disambiguators:

- `target_id`
- `target_index`
- `prefer = "first"`
- `prefer = "nearest"`

## Dependencies between files

The Lua modules do not import or load each other. This keeps them sandbox-friendly and host-injected.

Logical dependency order for users/reviewers:

1. `lua/core/id.lua` — shared identifier contract.
2. `lua/core/coordinates.lua` — shared coordinate and targeting contract.
3. `lua/core/grid.lua` — grid helper using the same position/facing semantics.
4. `tests/core_grid_examples.lua` — manual examples expecting injected module tables.
5. `manifests/core_grid.manifest.json` — batch index and capability metadata.
6. `docs/lua/core_grid_and_ids.md` — human-readable contract documentation.

## How to validate manually

1. Inspect the ZIP structure and confirm the listed files are present.
2. Read `manifests/core_grid.manifest.json` and check it is valid JSON.
3. Review each Lua file and confirm:
   - it returns a table;
   - it defines `manifest`;
   - it defines `validate_config(config)`;
   - it does not use filesystem, dynamic loading, network, or direct random APIs;
   - normal validation failures return diagnostics instead of throwing errors.
4. In a Lua 5.4 host, inject the three modules into `tests/core_grid_examples.lua` as:

```lua
{
  id = id_module,
  coordinates = coordinates_module,
  grid = grid_module
}
```

Then call `T.run(core)` and inspect the returned report.

## Known limitations

- No pathfinding is implemented in this batch.
- No chunk generation is implemented in this batch.
- No entity system is implemented in this batch.
- No UI IR or Unity IR is emitted yet.
- Grid radius neighborhood uses integer radius over cell centers and returns compact cell metadata, not rendered tiles.
- Target disambiguation returns selected candidate metadata, not runtime object references.
- Lua runtime execution was not required for this artifact; the included test file is a manual/injected-host example.

## Next recommended batch

Batch 003 — Time, turn, mode model.

Expected files from the plan:

- `lua/core/time_model.lua`
- `lua/core/turn_system.lua`
- `lua/core/mode_transition.lua`
- `docs/lua/time_turn_modes.md`
- `manifests/time_turn.manifest.json`
- `tests/time_turn_examples.lua`
- numbered batch report for Batch 003

## Scope confirmation

- Batch 003 was not generated.
- No C# project files were created or modified.
- No repository changes were attempted.
- This batch only adds Lua library files, docs, manifest metadata, manual examples/tests, and this report.
