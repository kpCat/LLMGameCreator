# Product Slice 012 Task: Generator Catalog Contract Foundation

## Task type

Bounded foundation slice.

## Goal

Add the first machine-readable Generator Catalog contract layer.

Slice 011 introduced GameBlueprint and Capability Graph compatibility. Slice 012 should introduce generator module manifests and catalog validation without dynamic plugin execution.

## Recommended Codex reasoning level

High.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/LLMGAMECREATOR_OFFICIAL_PRODUCT_PLAN.md
docs/GAME_ASSEMBLY_WORKBENCH_ARCHITECTURE.md
docs/CAPABILITY_GRAPH_AND_GENERATOR_CATALOG_PLAN.md
docs/PRODUCT_SLICE_011_GAME_BLUEPRINT_CAPABILITY_GRAPH.md
docs/GAME_BLUEPRINT_CAPABILITY_GRAPH_SPEC.md
docs/PRODUCT_SLICE_012_GENERATOR_CATALOG_CONTRACT_FOUNDATION.md
docs/GENERATOR_CATALOG_CONTRACT_SPEC.md
src/LLMGameCreator.Application/Composition/**
tests/LLMGameCreator.Tests/Application/GameBlueprintCapabilityTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/GameBlueprintCapabilityCompatibilitySmokeTests.cs
.devflow/scripts/run-product-smoke.ps1
```

Then inspect strict LLM artifact contract ids and existing product smoke style.

## Allowed files

```text
src/LLMGameCreator.Application/Composition/**
tests/LLMGameCreator.Tests/Application/**
tests/LLMGameCreator.Tests/ProductSmoke/**
.devflow/scripts/run-product-smoke.ps1
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

## Forbidden files

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.Infrastructure/Generation/**
src/LLMGameCreator.WinForms/**
generator-library/**
LLMGameCreator.sln
*.csproj
.devflow/NEXT_TASK.md
.devflow/task-queue.json
```

Do not add NuGet packages.
Do not call real LLM/provider.
Do not change package schema.
Do not implement dynamic plugins.
Do not execute generators.
Do not implement semantic world model.
Do not implement imported map, lazy world, or procedural quest engine.
Do not change UI.

## Required implementation

### 1. Generator module models

Add models under `src/LLMGameCreator.Application/Composition`.

Suggested types:

```text
GeneratorModuleManifest
GeneratorCatalog
BuiltInGeneratorCatalog
GeneratorCatalogValidator
GeneratorCatalogValidationResult
GeneratorCatalogDiagnostic
GeneratorPlanningResult
GeneratorPlanResolver
```

Suggested enums:

```text
GeneratorMaturity
GeneratorRuntimeCost
GeneratorDiagnosticSeverity
```

### 2. GeneratorModuleManifest fields

Minimum fields:

```text
GeneratorId
Title
Description
Maturity
UsesLlm
Deterministic
CanRunOffline
CanRunAtRuntime
InputContracts
OutputContracts
RequiresCapabilities
ProvidesCapabilities
OptionalCapabilities
ConflictsWithGenerators
SupportedGameKinds
SupportedWorldSources
SupportedPresentations
SupportedGenerationModes
RuntimeCost
ValidationRules
Notes
```

Use immutable records or init-only models where practical.

### 3. Built-in current generator manifests

Minimum current generator ids:

```text
generator.strict_llm.game_profile_v1
generator.strict_llm.region_pack_v1
generator.strict_llm.scene_pack_v1
generator.strict_llm.npc_pack_v1
generator.strict_llm.quest_pack_v1
generator.strict_llm.dialogue_pack_v1
generator.strict_llm.mechanics_pack_v1
generator.strict_llm.encounter_pack_v1
generator.strict_llm.item_pack_v1
generator.package.assembly_v1
generator.package.activation_v1
generator.runtime_preview.generated_map_markers_v1
```

Expected notes:
- strict LLM artifact generators use LLM and are not deterministic;
- package assembly/activation are deterministic and offline;
- runtime_preview.generated_map_markers is deterministic, offline, current.

### 4. Built-in planned generator manifests

Minimum planned ids:

```text
generator.semantic.world_model_seed_v1
generator.procedural.quest_templates_v1
generator.procedural.dialogue_realizer_v1
generator.world.lazy_region_cache_v1
generator.events.offscreen_scheduler_v1
generator.imported_map.osm_like_classifier_v1
generator.population.households_v1
generator.schedule.daily_life_v1
```

These should reference planned capabilities from Slice 011 where appropriate:
- `quest.procedural_templates`
- `dialogue.semantic_realizer`
- `world_source.imported_real_map`
- `population.households`
- `schedule.daily_life`
- `event.offscreen_scheduler`
- `time.calendar`

### 5. Contract ids

Use stable string contract ids.

Minimum known contract ids:

```text
game_profile_v1
region_pack_v1
scene_pack_v1
npc_pack_v1
quest_pack_v1
dialogue_pack_v1
mechanics_pack_v1
encounter_pack_v1
item_pack_v1
package.assembled_game_package
runtime_preview.generated_map_markers
semantic.world_model_seed
procedural.quest_templates
procedural.dialogue_realizer
world.lazy_region_cache
events.offscreen_scheduler
imported_map.classified_map
population.households
schedule.daily_life
```

### 6. Generator catalog validator

Must detect:
- duplicate generator ids;
- blank generator ids;
- unknown required capabilities;
- unknown optional capabilities;
- unknown provided capabilities where not registered;
- conflicts pointing to unknown generator ids;
- current generators depending on planned generator ids as error if direct conflict/dependency is modelled;
- duplicate output contracts among current generators as warning unless explicitly safe.

Keep deterministic ordering of diagnostics.

### 7. Generator plan resolver

Given a `GameBlueprint`, capability registry and generator catalog:
- select current generators that provide capabilities requested by blueprint or output contracts needed by existing current capability chain;
- report planned generators related to requested planned capabilities;
- report missing generator support for requested capabilities;
- do not execute anything.

The resolver can be simple; correctness and stability matter more than completeness.

### 8. Product smoke

Add smoke scenario:

```text
generator-catalog-contract
```

It should verify:
1. built-in generator ids are unique;
2. catalog validator has no errors for current catalog;
3. all current strict LLM contract generators are present;
4. package assembly and activation generators are present;
5. planned future generator manifests are present;
6. baseline generated RPG blueprint resolves to current generator modules;
7. realistic city survival imported map future preset returns planned/missing generator diagnostics without crashing;
8. no LLM/provider call.

### 9. State/docs

Update:
- `docs/PRODUCT_SMOKE_SCENARIOS.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `.devflow/CURRENT_RUN.md`

Mark Slice 011 accepted/completed and Slice 012 completed only after checks pass.

## Tests

Required tests:
1. built-in generator ids are unique.
2. current strict LLM generator manifests exist.
3. package assembly/activation manifests exist.
4. planned future generator manifests exist.
5. catalog validator catches duplicate ids.
6. catalog validator catches unknown capability references.
7. baseline generated RPG blueprint resolves current generator plan.
8. future imported-map blueprint reports planned/missing generator diagnostics without crashing.
9. product smoke `generator-catalog-contract` passes.
10. existing product smoke tests pass.

## Required checks

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~GeneratorCatalog"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-runtime-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario expanded-contract-batch-smoke
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-content-interaction-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario active-package-quest-dialogue-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-map-placement-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario content-language-policy
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario game-blueprint-capability-compatibility
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generator-catalog-contract

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

No manual UI verification is required. This slice must not change WinForms UI.

## Stop conditions

Stop and report if:
- plugin execution becomes necessary;
- package schema change becomes necessary;
- runtime or WinForms changes become necessary;
- `.sln` or `.csproj` changes are required;
- more than 18 files need changes;
- check-all fails after 2 repair attempts.

## Final report

Russian report with:
- files read;
- files changed;
- generator manifest types added;
- current generator ids;
- planned generator ids;
- catalog validation rules;
- plan resolver behavior;
- smoke/check results;
- confirmation that runtime/UI/package schema were not changed;
- recommended next slice.
