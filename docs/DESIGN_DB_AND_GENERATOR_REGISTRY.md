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

The initial implementation focuses on deterministic initialization, knowledge/decision/constraint upserts, generator module registry imports, import diagnostics, and query APIs for modules, capabilities, and issues.

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

## Future Flow

This baseline supports the planned LLM role:

```text
design knowledge -> capability selection -> GeneratorPlan -> validated artifacts -> GamePackage/runtime adapters
```

The next goal should let the LLM choose imported capabilities and produce a `GeneratorPlan` record. Lua execution, Unity/codegen IR, and real generator execution remain future work.

## WinForms UI

The Generator Library page follows the existing WinForms page pattern:

```text
GeneratorLibraryPageControl
GeneratorLibraryImportTabControl
GeneratorLibraryModulesTabControl
GeneratorLibraryCapabilitiesTabControl
GeneratorLibraryIssuesTabControl
```

The parent page owns only the tab layout and service coordination. Each tab owns its own layout and UI events.
