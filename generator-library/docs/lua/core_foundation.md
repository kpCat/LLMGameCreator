# Lua Core Foundation

Batch 001 introduces the minimum shared foundation for future Lua generator modules in LLMGameCreator / AI Game Builder.

The batch is intentionally small: diagnostics, deterministic RNG, and lightweight schema validation. It does not generate game content and does not integrate with the C# application.

## Shared constraints

- Lua 5.4-compatible.
- No external dependencies.
- No file system, network, dynamic loading, process, or debug APIs.
- Deterministic output only.
- No direct random calls.
- No global writes.
- Module outputs are plain JSON-serializable Lua data unless explicitly documented as a local runtime helper state.

## Module: `lua/core/diagnostics.lua`

### Purpose

Creates standard diagnostics and result envelopes used by future generator modules.

### When to use

Use this module when a generator, validator, planner, or adapter needs to report user/config/data problems without throwing runtime errors.

### When not to use

Do not use diagnostics as a replacement for fixing programmer errors inside a module. Invalid internal assumptions should be caught during review and tests.

### Manifest summary

- id: `core/diagnostics/v1`
- category: `core`
- deterministic: `true`
- capabilities:
  - `core.diagnostics.create`
  - `core.diagnostics.aggregate`

### Input schema explained

This helper module has no generator input schema. It exposes functions for creating and aggregating diagnostics.

### Config schema explained

Optional config:

```lua
{
  strict_severity = true
}
```

`strict_severity` is currently validated only as a boolean extension point. The current implementation normalizes unknown severities to `error`.

### Output schema explained

Diagnostics use this shape:

```lua
{
  severity = "error",
  code = "module.problem_code",
  message = "Human-readable message",
  target = "optional.path"
}
```

Result envelopes use this shape:

```lua
{
  ok = true,
  data = {},
  diagnostics = {},
  artifacts = {}
}
```

### Example config

```lua
{ strict_severity = true }
```

### Example input

```lua
local diagnostics = {}
diagnostics = Diagnostics.add_error(diagnostics, "world.missing_biome", "Biome id is missing.", "config.biome_id")
```

### Example output

```lua
{
  {
    severity = "error",
    code = "world.missing_biome",
    message = "Biome id is missing.",
    target = "config.biome_id"
  }
}
```

### LLM prompting hints

Ask the LLM to return diagnostics for invalid user requests, missing config, unsupported mode combinations, unreachable objectives, or unsafe generation requests.

### Validation rules

`validate_config(config)` accepts `nil` or a table. `strict_severity`, when provided, must be boolean.

### Extension points

Future modules can add diagnostic catalog IDs, localization keys, remediation hints, and severity policies without changing the base diagnostic shape.

### Runtime target notes

The shape is compatible with debug UIs, Unity adapters, and codegen review reports.

### Unity/codegen notes

Unity and codegen layers should consume diagnostics as data. They should not depend on Lua stack traces for normal user/config validation.

## Module: `lua/core/rng.lua`

### Purpose

Provides deterministic seed/state helpers for generator modules without using non-deterministic runtime randomness.

### When to use

Use this module when procedural generation needs repeatable choices, integer ranges, stable derived seeds, or deterministic shuffles.

### When not to use

Do not use it for cryptography, security, gambling fairness, or runtime systems that require platform-level entropy.

### Manifest summary

- id: `core/rng/v1`
- category: `core`
- deterministic: `true`
- capabilities:
  - `core.rng.seed`
  - `core.rng.next`
  - `core.rng.choice`
  - `core.rng.shuffle`

### Input schema explained

The module exposes helper functions. Generator modules should pass explicit seed/state data rather than implicit global random state.

### Config schema explained

Optional config:

```lua
{
  seed = 12345
}
```

`seed` must be an integer in `[1, 2147483646]` when validated as config.

### Output schema explained

RNG state is a plain table:

```lua
{ seed = 595905495 }
```

Most functions return `next_state` first, then the generated value, then diagnostics when relevant.

### Example config

```lua
{ seed = 12345 }
```

### Example input

```lua
local state = Rng.new(12345)
local next_state, value = Rng.range_int(state, 1, 6)
```

### Example output

```lua
{
  state = { seed = 595905495 },
  value = 4
}
```

### LLM prompting hints

Ask the LLM to include explicit seeds in generator configs when reproducibility matters. Use derived seeds for sub-generators such as `world`, `region`, `chunk`, `npc`, or `loot`.

### Validation rules

`validate_config(config)` accepts `nil` or a table. `seed`, when present, must be an integer in the supported Park-Miller seed range.

### Extension points

Future batches can add deterministic noise helpers, weighted choice, stable hashing for IDs, and chunk-local seed derivation.

### Runtime target notes

The state object is serializable and can be persisted between generation steps.

### Unity/codegen notes

Unity-facing generation should store seeds and generated IR, not hidden random state.

## Module: `lua/core/schema.lua`

### Purpose

Provides compact schema validation for JSON-like Lua data used by configs, inputs, outputs, and manifests.

### When to use

Use this module when validating generator config, user-selected capabilities, generated IR, compact manifests, and future runtime adapter inputs.

### When not to use

Do not use it as a complete JSON Schema implementation. It intentionally supports only a small subset that is easy to review.

### Manifest summary

- id: `core/schema/v1`
- category: `core`
- deterministic: `true`
- capabilities:
  - `core.schema.validate`
  - `core.schema.json_serializable`

### Input schema explained

`validate(value, schema, options)` accepts any Lua value plus a compact schema table.

Supported schema fields:

- `type`: `any`, `string`, `number`, `integer`, `boolean`, `table`, `array`, `object`
- `nullable`: boolean
- `enum`: array of allowed values
- `min`, `max`: numeric bounds
- `min_length`, `max_length`: string bounds
- `min_items`, `max_items`: array bounds
- `items`: array item schema
- `properties`: object property schemas
- `required`: array of required property names
- `allow_unknown`: boolean for object schemas

### Config schema explained

Optional config:

```lua
{
  max_depth = 16
}
```

`max_depth` must be an integer between `1` and `64`.

### Output schema explained

Validation returns:

```lua
{
  ok = true,
  diagnostics = {}
}
```

### Example config

```lua
{ max_depth = 16 }
```

### Example input

```lua
local config_schema = {
  type = "object",
  allow_unknown = false,
  required = { "seed", "world_scale" },
  properties = {
    seed = { type = "integer", min = 1 },
    world_scale = { type = "string", enum = { "single_map", "region", "infinite_chunks" } }
  }
}
```

### Example output

```lua
{
  ok = false,
  diagnostics = {
    {
      severity = "error",
      code = "core.schema.required_missing",
      message = "Required property is missing.",
      target = "config.seed"
    }
  }
}
```

### LLM prompting hints

Ask the LLM to produce compact configs and validate them before generation. Invalid fields should be reported through diagnostics instead of silently ignored when `allow_unknown = false`.

### Validation rules

The module validates type, enum, numeric ranges, string length, array length, required object properties, and unknown object properties.

### Extension points

Future batches can add reusable schema fragments for IDs, coordinates, time modes, combat modes, UI modes, entity components, quest objectives, formula IR, and Unity target IR.

### Runtime target notes

The validator is intended for small/medium config and IR validation. It is not a high-performance runtime validator for thousands of entities per frame.

### Unity/codegen notes

Use schema validation before exporting Unity-facing IR or codegen IR. The generated artifacts should remain data-only.
