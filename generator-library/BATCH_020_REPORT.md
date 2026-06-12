# BATCH 020 REPORT — Orchestration and artifact manifest

## Files generated

- `lua/generation/dependency_sort.lua`
- `lua/generation/artifact_manifest.lua`
- `lua/generation/pipeline_runner_plan.lua`
- `lua/generation/context_pack_plan.lua`
- `docs/lua/generator_orchestration.md`
- `manifests/orchestration.manifest.json`
- `tests/orchestration_examples.lua`
- `BATCH_020_REPORT.md`

## Current repo state read before generation

- Read `docs/LUA_GENERATION_PLAN_AND_PROMPTS.md` and used the exact Batch 020 definition.
- Read `generator-library/docs/lua/MANIFEST_CONTRACT.md` and followed the canonical manifest contract.
- Confirmed Batch 016 exists by reading `generator-library/manifests/city_builder.manifest.json`.
- Confirmed Batch 017 exists by reading `generator-library/manifests/ui_ir.manifest.json`.
- Confirmed Batch 018 exists by reading `generator-library/manifests/unity_ir.manifest.json`.
- Confirmed Batch 019 exists by reading `generator-library/manifests/validation.manifest.json`.
- Read nearby generation manifest style from `generator-library/manifests/generation_manifest.manifest.json`.

## Confirmation about previous batches

- Batch 016 files are not included in this ZIP.
- Batch 017 files are not included in this ZIP.
- Batch 018 files are not included in this ZIP.
- Batch 019 files are not included in this ZIP.
- Previous batch files were not modified or regenerated.

## Manifest contract compliance

- Batch manifest uses required fields: `id`, `version`, `batch`, `title`, `purpose`, `files`, `modules`, `runtime_targets`, `supported_time_modes`, `supported_combat_modes`, `unsafe_features`.
- Module entries use required fields: `id`, `path`, `category`, `capabilities`, `depends_on`, `runtime_targets`, `supported_turn_modes`, `supported_combat_modes`, `deterministic`, `unsafe_features`.
- No alias fields are used.
- Batch number is `020`.
- Batch report path is `generator-library/BATCH_020_REPORT.md`.

## Forbidden API compliance

- Generated Lua sources avoid host access APIs disallowed by the Batch 020 request.
- No external dependencies are used.
- No random source is used.
- No global writes are used; each module returns a table.
- No runtime pipeline execution is included.
- No C# integration is included.
- No Unity integration is included.
- No actual code generation or compilation is included.

## Module summary

- `dependency_sort.lua`: deterministic dependency ordering over plain module or plan-step metadata with missing dependency, duplicate id and cycle diagnostics.
- `artifact_manifest.lua`: generated artifact manifest IR with artifact ids, kinds, logical paths, producer refs, validation states and validation result refs.
- `pipeline_runner_plan.lua`: plan/schema/config IR for a future pipeline runner, including selected modules, ordered steps, expected artifacts, checkpoints and failure policy metadata.
- `context_pack_plan.lua`: LLM context-pack planning metadata with token budgets, included knowledge/module/artifact ids, exclusions and planning hints.

## Tests/examples included

- `tests/orchestration_examples.lua` provides manual example call specs for all four modules.
- It includes a valid dependency ordering example.
- It includes a cyclic dependency example.
- It includes an artifact manifest with validation result refs.
- It includes a pipeline runner plan with ordered steps.
- It includes a context pack plan with token budget and selected module ids.
- It includes an invalid unsafe pipeline flag example that should produce diagnostics.
- Examples are data-only and do not run generated plans or load modules dynamically.

## Internal self-check

- PASS: all ZIP paths start with `generator-library/`.
- PASS: no root-level generated folders or files.
- PASS: manifest JSON valid.
- PASS: every manifest `files[]` path exists in the ZIP under `generator-library/`.
- PASS: every manifest `module.path` exists in the ZIP under `generator-library/`.
- PASS: no alias manifest fields are used.
- PASS: batch required fields exist.
- PASS: module required fields exist.
- PASS: report name is numbered `BATCH_020_REPORT.md`.
- PASS: docs path is `generator-library/docs/lua/generator_orchestration.md`.
- PASS: tests path is `generator-library/tests/orchestration_examples.lua`.
- PASS: module IDs are unique inside batch.
- PASS: all module IDs use lowercase slash notation.
- PASS: capabilities use lowercase dot notation.
- PASS: no forbidden Lua APIs are present in generated Lua sources.
- PASS: no `.cs` files are generated.
- PASS: no C# project files are modified.
- PASS: previous batch files remain untouched.
- PASS: Batch 021 was not generated.

## Known limitations

- The batch emits planning/orchestration IR only; it does not run any pipeline.
- Dependency ordering consumes supplied metadata only and does not inspect files.
- Artifact validation states are metadata; validation execution remains host-side or future-module work.
- Context pack planning stores ids and token-budget metadata only; it does not read or summarize content.
- Future C# design database or GeneratorPlan integration is intentionally not added in this batch.

## Non-goals

- No C# integration.
- No Unity integration.
- No runtime pipeline runner.
- No dynamic Lua module loading.
- No filesystem, network, process or external command access.
- No changes to previous batches.
- No Batch 021 generation.
