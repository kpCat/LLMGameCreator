# Batch 018 — Unity target IR and C# codegen IR

## Purpose

This batch adds Unity-facing target metadata for the generator library. It creates deterministic, JSON-serializable IR/config data only. It does not integrate Unity, does not create Unity scene assets, does not compile anything, and does not write C# source files.

`C# codegen IR` means a declarative schema/plan that a future adapter may inspect. It is not generated source text and it does not include method bodies, build commands, shell commands, or executable snippets.

## Modules

### `lua/unity/unity_runtime_plan.lua`

Generates an abstract runtime plan with:

- target runtime id;
- scene references;
- feature flags;
- required adapter capabilities;
- game loop mode metadata;
- input mode metadata;
- persistence requirement metadata;
- compile/smoke validation metadata as declarative status/check records.

Validation returns diagnostics for invalid target ids, invalid modes and missing scene references.

### `lua/unity/unity_scene_ir.lua`

Generates scene IR, not scene files. Output may include:

- scene ids and categories;
- world/map references;
- entity placement slots;
- prefab slot references;
- spawn point metadata;
- camera metadata;
- lighting/environment metadata as plain tables.

Validation reports invalid scene ids, duplicate slots and invalid references.

### `lua/unity/unity_ui_ir.lua`

Bridges Batch 017 UI IR into Unity-facing adapter metadata. Output may include:

- UI document ids;
- canvas/panel metadata;
- references to HUD, minimap, inventory and quest journal IR;
- binding/action metadata;
- screen region layout metadata.

Validation reports invalid UI references, invalid bindings and duplicate documents or bindings.

### `lua/unity/unity_csharp_codegen_ir.lua`

Defines schema-validated CSharp codegen metadata only. Output may include:

- codegen unit ids;
- component/script role descriptors;
- namespace/class-name metadata;
- method/event hook descriptors as plain data;
- dependency references;
- compile/smoke validation metadata.

It must not output `.cs` files, raw source text, method bodies, executable snippets, commands or project-file edits.

## Input/config/output shape

All modules accept a table config directly or under `input.config` and return:

- `ok`: boolean;
- `data`: JSON-serializable tables;
- `diagnostics`: array of `{ severity, code, message, target }`;
- `artifacts`: empty array for this batch.

Normal validation failures return `ok = false` with diagnostics. They are not thrown as runtime failures.

## Example pipeline

1. Game design data defines world, entities, UI and gameplay needs.
2. A generator plan selects Unity target modules.
3. `unity_scene_ir.lua` produces scene metadata and prefab/entity slots.
4. `unity_ui_ir.lua` maps Batch 017 UI documents into Unity-facing UI adapter metadata.
5. `unity_csharp_codegen_ir.lua` emits metadata describing future components/scripts and validation expectations, without source text.
6. Future validation modules and a future Unity adapter may consume the IR later.

## Boundaries

- No Unity runtime integration.
- No Unity scene creation.
- No C# source generation.
- No compilation.
- No shell/build commands.
- No Lua execution support in C#.
- No filesystem, network or process access.

## Compatibility

This batch can consume earlier schema/core/UI module ids through manifest dependencies. Batch 016 and Batch 017 may already exist in the repository and are not changed by this batch. Batch 019 validation modules may later validate this IR, but Batch 018 does not depend on future batches.
