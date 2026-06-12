# Generator orchestration IR

Batch 020 adds deterministic orchestration and artifact manifest metadata for the Lua generator-library.

This batch does not run a generator pipeline. It does not load Lua modules dynamically, mutate game packages, call C#, call Unity, compile code, or call external commands. The modules only consume plain tables passed to `generate(input, ctx)` and return JSON-serializable IR plus diagnostics.

## What orchestration IR provides

Orchestration IR is planning data for a future host-side pipeline. It can describe module dependency order, generated artifacts, validation result references, a future pipeline-runner plan, and LLM context-pack planning metadata.

It is not a runtime runner. A later adapter may consume these plans, but this batch only produces and validates metadata.

## Module responsibilities

### `dependency_sort.lua`

Creates a deterministic topological order from plain `modules`, `steps`, or `items` metadata.

Input shape:

- `plan_id`: optional lowercase id.
- `modules`, `steps`, or `items`: array of tables with `id` and `depends_on`.

Output shape:

- `ordered_ids`.
- `ordered_items`.
- `duplicate_ids`.
- `cyclic_ids`.
- diagnostics.

The ordering policy is deterministic: original order first, then id for ties.

### `artifact_manifest.lua`

Creates generated artifact manifest IR.

Input shape:

- `manifest_id`.
- `producer_ids`: optional array of producer ids.
- `artifacts`: array of artifact records.

Artifact record shape:

- `id`.
- `kind`.
- `logical_path`.
- `produced_by`.
- `validation_state`.
- `validation_result_refs`.
- `depends_on_artifacts`.
- `metadata`.

Output shape:

- manifest id.
- artifact count.
- normalized artifact records.
- validation result index metadata.
- diagnostics.

### `pipeline_runner_plan.lua`

Creates a plan/schema/config for a future deterministic pipeline runner.

Input shape:

- `plan_id`.
- `selected_module_ids`.
- `validation_checkpoint_ids`.
- `expected_artifacts`.
- `steps`.

Step shape:

- `id`.
- `module_id`.
- `config_ref`.
- `inline_config`.
- `expected_artifacts`.
- `validation_checkpoints`.
- `depends_on_steps`.
- `dry_run`.
- `failure_policy`.

This module does not execute steps. It only validates and normalizes plan metadata.

### `context_pack_plan.lua`

Creates LLM context-pack planning metadata.

Input shape:

- `context_pack_id`.
- `purpose`.
- `token_budget`.
- `included_knowledge_ids`.
- `included_module_ids`.
- `included_artifact_ids`.
- `exclusions`.
- `hints`.

Token budget fields:

- `max_input_tokens`.
- `max_output_tokens`.
- `reserved_tokens`.
- `target_tokens`.

The module does not call an LLM, read files, summarize files, or fetch external data.

## Diagnostics strategy

All normal validation failures are returned as diagnostics. Modules do not throw for routine invalid data.

Diagnostic shape:

- `severity`: `error`, `warning`, or `info`.
- `code`: stable lowercase diagnostic id.
- `message`: human-readable message.
- `target`: field or id reference.

## Dependency ordering

Dependency ordering operates on plain metadata passed as input. It does not inspect the filesystem, require modules, or execute dependency modules.

The module reports:

- missing dependency diagnostics;
- cyclic dependency diagnostics;
- duplicate id diagnostics;
- deterministic order for valid acyclic graphs.

## Artifact manifest concept

Artifact manifests provide a stable index of generated outputs. Artifacts can reference validation states and validation result ids without embedding large validation data.

This supports later import, audit, UI review, and host-side registry work.

## Pipeline runner plan concept

`pipeline_runner_plan.lua` defines a future runner plan IR. The name means plan metadata only. It is intentionally not a runner.

It can declare selected modules, ordered steps, configs, expected artifacts, checkpoints, and failure policy. It must not apply outputs or run modules.

## Context pack plan concept

`context_pack_plan.lua` describes which knowledge ids, module ids, and artifact ids should be included in a future LLM context pack. Compression and summarization settings are hints only.

## Future C# design DB / GeneratorPlan support

A future C# design database, `GeneratorPlan`, or execution pipeline could consume these IR structures later. This batch does not add that integration and does not modify any C# project files.

## Boundaries and forbidden behavior

This batch does not:

- dynamically load Lua modules;
- run a generator pipeline;
- mutate game packages;
- execute C#;
- integrate with the C# app;
- integrate Unity;
- compile anything;
- call external commands;
- read files;
- become a runtime pipeline runner.
