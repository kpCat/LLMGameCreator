# BATCH 018 REPORT — Unity target IR and C# codegen IR

## Files generated

- `lua/unity/unity_runtime_plan.lua`
- `lua/unity/unity_scene_ir.lua`
- `lua/unity/unity_ui_ir.lua`
- `lua/unity/unity_csharp_codegen_ir.lua`
- `docs/lua/unity_target_ir.md`
- `manifests/unity_ir.manifest.json`
- `tests/unity_ir_examples.lua`
- `BATCH_018_REPORT.md`

## Current repo state read before generation

- Read `docs/LUA_GENERATION_PLAN_AND_PROMPTS.md` and used the exact Batch 018 definition.
- Read `generator-library/docs/lua/MANIFEST_CONTRACT.md` and followed the canonical manifest contract.
- Confirmed Batch 016 backfill exists by reading `generator-library/manifests/city_builder.manifest.json` and `generator-library/BATCH_016_REPORT.md`.
- Confirmed Batch 017 exists by reading `generator-library/manifests/ui_ir.manifest.json` and `generator-library/BATCH_017_REPORT.md`.
- Batch 016 and Batch 017 files are not included in this ZIP and were not modified.

## Manifest contract compliance

- Batch manifest uses required fields: `id`, `version`, `batch`, `title`, `purpose`, `files`, `modules`, `runtime_targets`, `supported_time_modes`, `supported_combat_modes`, `unsafe_features`.
- Module entries use required fields: `id`, `path`, `category`, `capabilities`, `depends_on`, `runtime_targets`, `supported_turn_modes`, `supported_combat_modes`, `deterministic`, `unsafe_features`.
- No alias fields are used.
- Batch number is `018`.
- Batch report path is `generator-library/BATCH_018_REPORT.md`.

## Forbidden API compliance

- Generated Lua sources avoid disallowed host/runtime access APIs.
- No external dependencies are used.
- No random source is used.
- No global writes are used; each module returns a table.
- No Unity runtime integration is included.
- No actual C# source generation is included.
- No `.cs` files are generated.
- No C# project files are modified.

## Module summary

- `unity_runtime_plan.lua`: generates abstract Unity-facing runtime plan IR with target id, scene refs, feature flags, adapter capability requirements, loop/input metadata, persistence metadata and declarative validation metadata.
- `unity_scene_ir.lua`: generates Unity-facing scene IR with scene ids/categories, map/world refs, prefab slots, entity slots, spawn points, camera metadata and environment metadata.
- `unity_ui_ir.lua`: maps Batch 017 UI IR references into Unity-facing UI adapter metadata with UI documents, canvas data, panel refs, bindings, actions and screen regions.
- `unity_csharp_codegen_ir.lua`: generates schema-validated CSharp codegen metadata only, with codegen unit ids, role descriptors, namespace/class metadata, hook descriptors and dependency refs.

## Tests/examples included

- `tests/unity_ir_examples.lua` provides manual example inputs for all four modules.
- It includes valid generation-call examples through injected module tables.
- It includes an invalid codegen config example that should produce diagnostics.
- It demonstrates JSON-serializable output shapes.
- It does not generate `.cs` source files and does not execute Unity or C# runtime behavior.

## Internal self-check

- PASS: all ZIP paths start with `generator-library/`.
- PASS: no root-level generated folders or files.
- PASS: manifest JSON valid.
- PASS: every manifest `files[]` path exists in the ZIP under `generator-library/`.
- PASS: every manifest `module.path` exists in the ZIP under `generator-library/`.
- PASS: no alias manifest fields are used.
- PASS: batch required fields exist.
- PASS: module required fields exist.
- PASS: report name is numbered `BATCH_018_REPORT.md`.
- PASS: docs path is `generator-library/docs/lua/unity_target_ir.md`.
- PASS: tests path is `generator-library/tests/unity_ir_examples.lua`.
- PASS: module IDs are unique inside batch.
- PASS: all module IDs use lowercase slash notation.
- PASS: capabilities use lowercase dot notation.
- PASS: no forbidden Lua APIs are present in generated Lua sources.
- PASS: no `.cs` files are generated.
- PASS: no C# project files are modified.
- PASS: Batch 016 files remain untouched.
- PASS: Batch 017 files remain untouched.
- PASS: Batch 019 was not generated.

## Known limitations

- This batch emits Unity-facing IR/config only; it does not create Unity assets or scenes.
- CSharp codegen IR is metadata only and does not contain source text or method bodies.
- Compile and smoke validation records are declarative metadata with expected not-run status; no validation runner is implemented here.
- Future validation modules and future Unity adapters must consume this IR later.

## Non-goals

- No C# integration.
- No Unity runtime integration.
- No Unity scene generation.
- No actual C# source generation.
- No compilation.
- No filesystem, network or process access.
- No changes to previous batches.
- No Batch 019 generation.
