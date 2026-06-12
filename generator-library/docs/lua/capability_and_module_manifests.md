# Batch 004 — Capability and generator module manifest helpers

## Purpose

This batch defines the manifest-side infrastructure used by the AI Game Builder generator library before a C# host imports or executes any trusted Lua module.

The batch does not execute game generation. It describes and validates the registry layer that lets an LLM or host choose modules by capability, runtime target, time mode, combat mode, dependencies, and incompatibilities.

## Files

- `lua/generation/capability_manifest.lua`
- `lua/generation/module_manifest.lua`
- `lua/generation/generator_plan.lua`
- `manifests/generation_manifest.manifest.json`
- `tests/generation_manifest_examples.lua`

## When to use

Use these modules when the host needs to:

- validate a capability definition before adding it to a capability registry;
- validate a Lua module `manifest` table before exposing it to LLM planning;
- build a deterministic mapping from capability id to module id;
- validate an ordered generator plan before a later trusted runner executes the steps;
- reject incompatible modules or steps early;
- check whether a plan is compatible with `realtime`, `turn_based`, `mixed`, `paused_planning`, `dialogue_combat`, `hybrid`, Unity-facing IR, simulation, or codegen IR targets.

## When not to use

Do not use this batch as:

- a sandbox or Lua executor;
- a topological dependency sorter for arbitrary graphs;
- a replacement for C# registry persistence;
- a validator for every future world/entity/quest schema;
- a module importer that reads files from disk.

The modules are data validators and manifest normalizers only.

## Manifest summary

### Capability id

Capability ids use lowercase dot notation:

```text
world.chunk.generate
generation.module_manifest.validate
combat.dialogue_combat.plan
```

This differs from module ids because existing Lua module manifests already use capability names like `world.chunk.generate`.

### Module id

Module ids use lowercase slash notation:

```text
generation/module_manifest/v1
world/chunk_generator/v1
ui/hud_layout/v1
```

### Supported runtime targets

The current shared set is:

```text
debug
unity2d
unity3d
simulation
codegen_ir
validation
editor
```

### Supported time modes

```text
realtime
turn_based
mixed
paused_planning
```

### Supported combat modes

```text
none
realtime
turn_based
tactical
dialogue_combat
hybrid
```

## Module contracts

Each Lua file returns a table and provides:

- `manifest`
- `validate_config(config)`
- `generate(input, ctx)`

The helpers return result objects shaped like:

```text
{
  ok = true | false,
  data = {},
  diagnostics = {},
  artifacts = {}
}
```

Diagnostics follow the shared format:

```text
{
  severity = "error" | "warning" | "info",
  code = "generation.module.invalid_id",
  message = "Human readable message",
  target = "manifest.id"
}
```

## Capability manifest schema explained

A capability manifest is a compact record:

```text
{
  id = "world.chunk.generate",
  title = "Chunk generation",
  purpose = "Generate chunk-level map IR",
  category = "world",
  inputs = {
    { id = "world_blueprint", schema = {}, required = true }
  },
  outputs = {
    { id = "chunk_ir", schema = {}, required = true }
  },
  config_schema = {},
  supported_runtime_targets = { "debug", "unity2d" },
  supported_time_modes = { "realtime", "mixed" },
  supported_combat_modes = { "none", "realtime" },
  dependencies = { "world.blueprint.generate" },
  incompatibilities = {},
  tags = { "world", "chunk" }
}
```

Inputs and outputs are port lists. A port can be a string shorthand or a table with `id`, `schema`, `required`, and `description`.

## Module manifest schema explained

A module manifest follows the shared Lua module contract and adds optional planning metadata:

```text
{
  id = "world/chunk_generator/v1",
  version = "0.1.0",
  category = "world",
  title = "Chunk generator",
  purpose = "Create chunk map IR",
  capabilities = { "world.chunk.generate" },
  input_schema = {},
  output_schema = {},
  config_schema = {},
  deterministic = true,
  runtime_targets = { "debug", "unity2d" },
  supported_time_modes = { "realtime", "mixed" },
  supported_combat_modes = { "none", "realtime" },
  dependencies = {
    modules = { "world/world_blueprint/v1" },
    capabilities = { "world.blueprint.generate" }
  },
  incompatibilities = {
    modules = {},
    capabilities = {}
  },
  unsafe_features = {}
}
```

## Generator plan schema explained

A generator plan is an ordered list of steps. Batch 004 intentionally does not reorder a graph. It validates that dependencies appear earlier in the list.

```text
{
  id = "generation/plan/demo",
  title = "Small world slice",
  runtime_target = "unity2d",
  turn_mode = "mixed",
  combat_mode = "dialogue_combat",
  inputs = {},
  expected_outputs = { "chunk_ir" },
  steps = {
    {
      id = "step/world_blueprint",
      module_id = "world/world_blueprint/v1",
      capability_id = "world.blueprint.generate",
      inputs = {},
      outputs = { "world_blueprint" },
      config = {},
      depends_on = {},
      incompatible_with = {},
      supported_runtime_targets = { "debug", "unity2d" },
      supported_time_modes = { "mixed" },
      supported_combat_modes = { "dialogue_combat" }
    }
  }
}
```

## Example output

A successful call to `generator_plan.generate({ plan = plan })` returns:

```text
{
  ok = true,
  data = {
    plan = {},
    execution_order = { "step/world_blueprint" },
    step_targets = {
      {
        step_id = "step/world_blueprint",
        module_id = "world/world_blueprint/v1",
        capability_id = "world.blueprint.generate",
        depends_on = {},
        outputs = { "world_blueprint" }
      }
    }
  },
  diagnostics = {},
  artifacts = {}
}
```

## LLM prompting hints

When asking an LLM to choose generator modules, provide:

1. the design goal;
2. selected `runtime_target`, `turn_mode`, and `combat_mode`;
3. available capability manifests;
4. available module manifests;
5. already generated artifacts;
6. explicit forbidden modules or incompatible capabilities.

Ask the LLM to output a compact generator plan, not raw world data.

## Validation rules

- Capability ids must be lowercase dot notation.
- Module ids must be lowercase slash notation.
- Runtime targets, time modes, and combat modes must come from supported sets unless explicitly relaxed by config.
- Dependencies and incompatibilities must use valid id syntax.
- A generator step must declare at least `module_id` or `capability_id`.
- A generator step dependency must refer to an existing previous step.
- If a present step declares another present step as incompatible, the plan is invalid.
- Step outputs must be an array of strings.
- User-facing validation problems return diagnostics rather than throwing runtime exceptions.

## Extension points

Future batches can extend this layer with:

- dependency sorting;
- artifact manifest validation;
- context pack planning;
- C# registry importer checks;
- Unity scene/UI/codegen IR validation;
- module contract validation across imported ZIP artifacts.

## Runtime target notes

The helpers are runtime-neutral and produce plain data. A host can use the output to drive a debug preview, Unity 2D/3D adapter, simulation layer, validation pipeline, or codegen IR pipeline.

## Unity/codegen notes

These helpers deliberately output abstract registry and plan metadata only. They do not generate Unity objects or C# source code. Later Unity/codegen batches should consume the same manifest metadata when deciding which IR modules can run for a selected target.
