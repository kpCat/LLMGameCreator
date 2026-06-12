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
```

The parent page owns only the tab layout and service coordination. Each tab owns its own layout and UI events.
