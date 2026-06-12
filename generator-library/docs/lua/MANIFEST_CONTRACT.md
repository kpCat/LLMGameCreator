# Lua Generator Manifest Contract

Status: canonical for generator-library manifests after manifest stabilization patch.
Version: 0.2

This contract exists to prevent drift between generated Lua batch manifests and the C# Generator Library Registry importer.
The importer should be able to read every `*.manifest.json` file without supporting historical aliases.

## Batch manifest required fields

Each batch manifest object must contain:

```json
{
  "id": "batch/category_name/v1",
  "version": "0.1.0",
  "batch": "001",
  "title": "Human readable title",
  "purpose": "What this batch contributes",
  "files": [],
  "modules": [],
  "runtime_targets": [],
  "supported_time_modes": [],
  "supported_combat_modes": [],
  "unsafe_features": []
}
```

Required semantics:

- `id` is the manifest/batch id. It is not a module id.
- `version` is the manifest contract/content version.
- `batch` is a zero-padded string when the batch has a numeric batch number.
- `title` is human readable.
- `purpose` is the canonical explanatory field. Do not use `description` as a replacement.
- `files` lists generated/relevant files exactly as they exist inside the batch/repository layout.
- `modules` is an array of canonical module entries.
- `runtime_targets` lists host/runtime targets supported by the batch.
- `supported_time_modes` lists supported game time modes for the batch.
- `supported_combat_modes` lists supported combat modes for the batch.
- `unsafe_features` lists unsafe features. It must be an array and should normally be empty.

## Module entry required fields

Each entry in `modules` must contain:

```json
{
  "id": "category/module_name/v1",
  "path": "lua/category/module_name.lua",
  "category": "category",
  "capabilities": [],
  "depends_on": [],
  "runtime_targets": [],
  "supported_turn_modes": [],
  "supported_combat_modes": [],
  "deterministic": true,
  "unsafe_features": []
}
```

Required semantics:

- `id` is the canonical module id. Do not use `module_id`.
- `path` is the Lua source path. Do not use `file`.
- `category` is the top-level capability category such as `core`, `world`, `dialogue`, `quest`, `item`, or `progression`.
- `capabilities` is preserved exactly from the generated module intent. Do not invent capabilities during manifest stabilization.
- `depends_on` lists module ids required by the module. Do not use `depends_on_contracts` or `dependencies`.
- `runtime_targets` lists supported host/runtime targets.
- `supported_turn_modes` is the per-module turn/time compatibility list.
- `supported_combat_modes` is the per-module combat compatibility list.
- `deterministic` must be explicit.
- `unsafe_features` must be explicit and normally empty.

## Prohibited alias fields

Do not use these fields in new or patched manifests:

- `module_id`
- `file`
- `depends_on_contracts`
- `dependencies`
- `description` as a replacement for `purpose`
- `supported_runtime_targets` as a replacement for `runtime_targets`
- `supports.turn_modes` as a replacement for `supported_time_modes`
- `supports.combat_modes` as a replacement for `supported_combat_modes`
- `supports.runtime_targets` as a replacement for `runtime_targets`

## Migration rules

When stabilizing existing manifests:

1. Replace module `module_id` with module `id`.
2. Replace module `file` with module `path`.
3. Replace module `depends_on_contracts` or `dependencies` with module `depends_on`.
4. If a batch has `description` and no `purpose`, copy the same text into `purpose` and remove `description`.
5. Replace top-level `supported_runtime_targets` with `runtime_targets`.
6. Flatten `supports.turn_modes` into `supported_time_modes`.
7. Flatten `supports.combat_modes` into `supported_combat_modes`.
8. Flatten `supports.runtime_targets` into `runtime_targets`.
9. Preserve all existing module ids, capabilities, file paths, runtime targets, turn modes, combat modes, and unsafe feature declarations.
10. Do not change Lua source modules as part of manifest stabilization unless a source module itself contains a broken manifest contract.

## Compatibility notes

- Batch-level `supported_time_modes` and module-level `supported_turn_modes` intentionally use different names because the batch describes the whole manifest, while module entries describe the module contract used by the registry.
- Additional metadata such as `contracts`, `quality`, `generator_plan_steps`, or `next_recommended_batch` may exist, but importers must only require the canonical fields listed above.
- New batch generation must use numbered batch reports: `BATCH_001_REPORT.md`, `BATCH_002_REPORT.md`, and so on.
