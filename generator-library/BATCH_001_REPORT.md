# Batch 001 Report — Core foundation

## Files generated

- `lua/core/diagnostics.lua`
- `lua/core/rng.lua`
- `lua/core/schema.lua`
- `docs/lua/core_foundation.md`
- `manifests/core_foundation.manifest.json`
- `tests/core_foundation_examples.lua`
- `BATCH_REPORT.md`

## Contracts introduced

### Diagnostics contract

Standard diagnostic shape:

```lua
{
  severity = "error" | "warning" | "info",
  code = "module.problem_code",
  message = "Human-readable message",
  target = "optional.path"
}
```

Standard result envelope:

```lua
{
  ok = true,
  data = {},
  diagnostics = {},
  artifacts = {}
}
```

### Deterministic RNG contract

RNG state is explicit and serializable:

```lua
{ seed = 12345 }
```

Functions return the next state instead of mutating hidden global state. The implementation uses a Park-Miller style LCG and never calls direct runtime randomness.

### Lightweight schema contract

Compact schema validation supports:

- primitive types;
- integer checks;
- enum checks;
- numeric min/max;
- string length bounds;
- array item schemas;
- object properties;
- required properties;
- unknown-property rejection;
- JSON-serializable data validation.

## Dependencies between files

The three core Lua modules are intentionally independent and do not load each other.

`tests/core_foundation_examples.lua` expects a host/integration test runner to inject:

```lua
{
  diagnostics = Diagnostics,
  rng = Rng,
  schema = Schema
}
```

This keeps the artifact free from module loading assumptions and avoids direct dependency on a file loader.

## How to validate manually

1. Inspect every Lua file and confirm each one returns a table.
2. Confirm each module exposes `manifest` and `validate_config(config)`.
3. Confirm no module writes globals.
4. Confirm no direct runtime randomness is used.
5. Inject the three modules into `tests/core_foundation_examples.lua` from the host test harness and call `run(core)`.
6. Confirm the test report returns `ok = true` and JSON-serializable diagnostics/data/artifacts.
7. Validate `manifests/core_foundation.manifest.json` with any JSON parser.

## Known limitations

- `schema.lua` is not a full JSON Schema implementation.
- RNG is deterministic but not cryptographic and not suitable for security-sensitive use.
- Tests are manual/injected examples because this batch does not define a Lua module loader.
- No C# integration, Unity adapter, codegen pipeline, or runtime preview changes are included.
- No game-domain generators are implemented in this batch.

## Next recommended batch

Batch 002 — IDs, grid, coordinates.

Expected scope:

- lowercase slash id validation;
- 2D positions;
- chunk coordinates and local coordinates;
- grid bounds;
- get/set cells;
- sparse overrides;
- neighborhoods;
- facing direction;
- target cell in front of actor;
- adjacency and targeting disambiguation basics.

## Implemented scope only

This batch implements only the core foundation files listed above. It does not claim that later world, entity, dialogue, quest, combat, automation, UI IR, Unity IR, or codegen IR modules already exist.
