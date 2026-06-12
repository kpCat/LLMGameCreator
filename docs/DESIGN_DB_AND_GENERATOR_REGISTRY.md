# Design DB and Generator Registry

## Purpose

The Design DB is a local SQLite database for editor-side design knowledge and generator-library registry data.

It does not replace `GamePackage` and it is not a runtime data source for the final game. `GamePackage` remains the source of truth for playable content. The DB exists so the editor can store design knowledge, decisions, constraints, imported generator module metadata, generator plans, artifacts, validation results, and prompt context packs without asking an LLM to hold the whole project in context.

## Location

When a game project is open, the database is stored under:

```text
<projectFolder>/.llmgc/design.db
```

When no project is open, callers may initialize a database at an explicit path. The WinForms page uses an app-data fallback:

```text
%LOCALAPPDATA%/LLMGameCreator/design.db
```

The database is never stored inside `generator-library/`.

## Schema v1

Schema v1 creates these tables:

```text
design_metadata
knowledge_items
knowledge_relations
design_decisions
design_constraints
capability_modules
generator_modules
generator_module_files
generator_configs
generator_plans
generator_plan_steps
generated_artifacts
validation_results
prompt_context_packs
import_issues
```

The implementation focuses on deterministic initialization, knowledge/decision/constraint upserts, generator module registry imports, import diagnostics, query APIs for modules/capabilities/issues, registry-backed draft `GeneratorPlan` creation, and saved plan review/lifecycle status updates.

## Generator Library Import

The importer reads:

```text
generator-library/manifests/*.manifest.json
```

It imports manifest metadata, module ids, declared module paths, capability ids, dependencies, runtime targets, turn modes, combat modes, UI modes, world scales, manifest file entries, and diagnostics.

It intentionally does not:

```text
execute Lua
load arbitrary code
run code generation
change GamePackage format
generate Unity code
copy all Lua source into SQLite
```

Lua files remain library assets. The registry stores paths and normalized manifest data so future editor workflows can select capabilities and create plans.

## Normalization

The importer accepts the current Batch 001-004 manifest variants:

```text
runtime_targets + supported_runtime_targets -> runtime_targets_json
architecture_notes.turn_modes + supported_time_modes -> turn_modes_json
architecture_notes.combat_modes + supported_combat_modes -> combat_modes_json
architecture_notes.ui_modes -> ui_modes_json
architecture_notes.world_scales -> world_scales_json
dependencies + depends_on -> dependencies_json
```

If a module category is missing, the importer infers it from the first slash-delimited segment of the module id. Unknown manifest/module fields are stored in `metadata_json` so future batches can add fields without immediately changing the schema.

Invalid manifests produce `import_issues` and do not stop valid manifests from importing. Re-running import is idempotent because registry rows use stable primary keys and upserts.

## Generator Library Integrity Validation

Integrity validation checks the physical `generator-library/` folder before a Lua batch is committed or pushed.

It validates:

- repository root or direct `generator-library` root resolution;
- `generator-library/manifests/*.manifest.json` discovery, ignoring `MANIFEST_CONTRACT.schema.example.json`;
- valid JSON and canonical manifest fields such as `id`, `batch`, `title`, `purpose`, `files`, `modules`, module `id`, module `path`, `category`, and `capabilities`;
- legacy alias fields such as `module_id`, `file`, and `depends_on_contracts` as warnings;
- declared `files[]` and module `path` entries exist under `generator-library`;
- obvious root-level leakage such as `lua/`, `manifests/`, root `BATCH_*.md`, or root Lua files outside `generator-library`;
- duplicate batch manifest ids and duplicate module ids;
- numbered batch reports such as `BATCH_012_REPORT.md`;
- manifest contract docs and schema example presence;
- `unsafe_features` shape and claims that Lua execution is enabled.

The WinForms Generator Library page exposes this as the `Integrity` tab. Use `Validate generator-library` before importing or before pushing a new Lua batch. The current library should have zero integrity errors.

Integrity validation is not the same as manifest import:

- Integrity validation checks files and contracts on disk and returns a deterministic report.
- Manifest import stores registry metadata in the Design DB.
- Import may still run when integrity errors exist, but the Import tab warns that the import may be incomplete.
- Neither path executes Lua, loads dynamic code, changes GamePackage format, or generates Unity/codegen output.

## Generator Plans

Registry-backed draft `GeneratorPlan` creation is implemented for editor-side planning. The LLM may propose a draft plan from imported registry metadata and compact design context, but C# owns strict JSON parsing, validation, storage, and lifecycle status updates.

Saved plans can be reviewed in the WinForms `Plans` tab:

```text
draft -> approved
draft -> rejected
draft -> archived
```

Approval is deterministic and human-triggered. Before a plan is approved, the saved plan is rebuilt from `generator_plans` and `generator_plan_steps`, revalidated against the current imported registry, and rejected if validation has errors. Warnings do not block approval.

Approved plans are not executed. Approval only means the saved plan was human-reviewed and currently valid enough for a future deterministic execution/apply pipeline.

Plan creation, revalidation, approval, rejection, and archiving intentionally do not:

```text
execute Lua
execute generator modules
run code generation
change GamePackage format
mutate GamePackage content
generate Unity code
```

## GeneratorPlan Preview / Staged Artifacts

Approved plans can be compiled into deterministic preview artifacts. This is an editor-side staging and audit step that turns the saved `GeneratorPlan` and current registry metadata into a `generator_plan_preview` artifact row in the Design DB.

Preview artifacts are stored in:

```text
generated_artifacts
validation_results
```

They are not written to `GamePackage`, do not create package files, and do not change the `GamePackage` JSON format. The preview JSON records the plan id/title/goal/status, ordered module ids, module paths, categories, step configs, dependencies, and explicit no-execution policy flags.

The preview compiler intentionally does not:

```text
execute Lua
execute generator modules
interpret module source files
run code generation
mutate GamePackage content
generate Unity code
call an LLM
```

Before saving an artifact, the service requires the plan status to be `approved` and revalidates the plan against the current registry. Plans with current validation errors do not create staged artifacts. Validation results for a saved preview are stored as Design DB rows for audit and future execution/apply pipeline work.

## GamePackage Patch Artifacts

The first safe apply layer is represented by generated artifacts with kind:

```text
game_package_patch_v1
```

Patch artifacts are strict data-only contracts. They can be created from `generator_plan_preview` artifacts only when a preview step config contains an explicit `package_operations` array. The patch service extracts those operations deterministically from config JSON; it does not infer operations from module ids, module paths, Lua source, or generator metadata.

Supported operations in schema version 1 are intentionally small:

- `upsert_tile_prototype`
- `upsert_map`
- `upsert_entity_prototype`
- `update_manifest`

Delete operations, arbitrary JSON Patch/RFC6902 paths, reflection-based mutation, nested object merge, script edits, asset file writes, Lua changes, map chunks, and tile-grid edits are not supported by this layer.

Dry-run loads the current in-memory `GamePackage`, deep-clones it, applies the patch to the clone only, validates the clone with the existing package validator, and returns deterministic readable diff lines. Dry-run does not save `package.json` and does not mutate the current package.

Apply is explicit and human-triggered through the application service or the WinForms `Artifacts` tab. Apply runs dry-run first, creates a rollback snapshot before mutation/save, applies the same data-only operations, validates after apply, then saves through the existing package save path only when validation passes.

Rollback snapshots are stored under:

```text
<projectFolder>/.llmgc/backups/package-YYYYMMDD-HHMMSS-fff.json
```

Apply writes an audit generated artifact with kind `game_package_patch_apply_result_v1` and validation/audit rows in the Design DB. The layer still does not execute Lua, execute generator modules, call an LLM, run Unity, run codegen, or change the `GamePackage` JSON format.

## Patch-Capable Generator Plans

Generator plan step configs may now include an optional data-only field:

```json
{
  "package_operations": []
}
```

This field uses the same strict `game_package_patch_v1` operation shapes that patch artifacts use. The supported operations remain:

- `upsert_tile_prototype`
- `upsert_map`
- `upsert_entity_prototype`
- `update_manifest`

`package_operations` may originate from an LLM-proposed draft plan, but C# owns strict JSON parsing, registry validation, patch operation validation, human approval, preview artifact creation, patch artifact extraction, dry-run, explicit apply, rollback, audit and package validation.

Plan validation rejects invalid package operations before approval. Plans without `package_operations` remain valid when they otherwise satisfy registry and dependency rules; they simply cannot produce a patch artifact through the safe patch pipeline.

The end-to-end safe creator flow is:

```text
draft plan
 -> human approval
 -> prepare patch pipeline
 -> generator_plan_preview artifact
 -> game_package_patch_v1 artifact
 -> dry-run diff
 -> explicit apply
 -> rollback/audit artifact
 -> package validation
 -> runtime preview can use the updated package
```

Prepare does not mutate `GamePackage` and does not apply automatically. Apply is data-only, allowlisted and human-triggered.

This flow still does not:

```text
execute Lua
execute generator modules
run Unity
run code generation
support arbitrary JSON Patch/RFC6902
support delete operations
edit scripts
write asset files
auto-apply after LLM generation
```

## Future Flow

This baseline supports the planned LLM role:

```text
design knowledge -> capability selection -> GeneratorPlan -> approval -> deterministic execution/apply pipeline -> validated artifacts -> GamePackage/runtime adapters
```

Lua execution, Unity/codegen IR, real generator execution, and applying approved plans to `GamePackage` remain future work.

## WinForms UI

The Generator Library page follows the existing WinForms page pattern:

```text
GeneratorLibraryPageControl
GeneratorLibraryImportTabControl
GeneratorLibraryModulesTabControl
GeneratorLibraryCapabilitiesTabControl
GeneratorLibraryIssuesTabControl
GeneratorLibraryPlansTabControl
GeneratorLibraryArtifactsTabControl
```

The parent page owns only the tab layout and service coordination. Each tab owns its own layout and UI events.
