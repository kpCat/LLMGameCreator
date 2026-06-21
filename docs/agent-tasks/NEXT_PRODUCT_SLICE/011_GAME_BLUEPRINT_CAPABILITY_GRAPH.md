# Product Slice 011 Task: GameBlueprint + Capability Graph Compatibility Foundation

## Task type

Bounded foundation slice.

## Goal

Add the first machine-readable `GameBlueprint` and capability compatibility foundation.

The program should be able to say:
- what kind of game is being assembled;
- which capabilities are requested;
- which capabilities are available;
- which requirements are missing;
- which capabilities conflict;
- which future capabilities are planned/unsupported without crashing.

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
docs/PRODUCT_SMOKE_SCENARIOS.md
src/LLMGameCreator.Application/Projects/ContentLanguagePolicy.cs
tests/LLMGameCreator.Tests/Application/ContentLanguagePolicyTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/ContentLanguagePolicySmokeTests.cs
.devflow/scripts/run-product-smoke.ps1
```

## Allowed files

```text
src/LLMGameCreator.Application/Composition/**
src/LLMGameCreator.Application/Projects/**
tests/LLMGameCreator.Tests/Application/**
tests/LLMGameCreator.Tests/ProductSmoke/**
.devflow/scripts/run-product-smoke.ps1
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

If `src/LLMGameCreator.Application/Composition` does not exist, create it.

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

Do not add NuGet packages. Do not call LLM/provider. Do not change runtime, UI, or package schema.

## Required implementation

### 1. Models

Add:

```text
GameBlueprint
GameKind
WorldSourceKind
PresentationKind
GenerationMode
CapabilityDefinition
CapabilityRuntimeCost
CapabilityMaturity
CompositionCompatibilityStatus
CompositionDiagnostic
CompositionValidationResult
```

Use simple immutable models/records where appropriate.

### 2. Built-in registry

Add a built-in `CapabilityRegistry` / `BuiltInCapabilityRegistry`.

Minimum capability ids:

```text
localization.content_language_policy
generation.strict_llm_artifacts
package.artifact_review
package.assembly
package.activation
world_source.procedural_package
presentation.topdown_2d_runtime_preview
runtime.preview_movement
dialogue.preview_lines
quest.preview_journal
map.generated_marker_placement
content.generated_npcs
content.generated_quests
content.generated_dialogues
content.generated_encounters
```

Also register planned future ids:

```text
world_source.imported_real_map
time.calendar
population.households
schedule.daily_life
event.offscreen_scheduler
quest.procedural_templates
dialogue.semantic_realizer
```

### 3. Validator

Add `GameBlueprintCompositionValidator`.

It must detect:
- unknown requested capability;
- missing required capability;
- direct conflicts;
- optional missing requirements as warnings;
- planned/unsupported capabilities without crashing.

### 4. Presets

Add a preset provider with:

```text
baseline_generated_rpg_preview
realistic_city_survival_imported_map_future
zombie_city_survival_imported_map_future
```

`baseline_generated_rpg_preview` should validate OK using current capabilities.

The two future imported-map presets should produce diagnostics for future/missing capabilities but must not crash.

### 5. Product smoke

Add scenario:

```text
game-blueprint-capability-compatibility
```

Smoke should verify:
- registry ids are unique;
- baseline preset validates OK;
- future realistic city preset returns useful diagnostics;
- intentionally broken blueprint reports missing requirements;
- no LLM/provider call.

### 6. State/docs

Update:
- `docs/PRODUCT_SMOKE_SCENARIOS.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `.devflow/CURRENT_RUN.md`

Mark Slice 010 accepted/completed and Slice 011 completed only after checks pass.

## Tests

Required tests:

1. built-in capability ids are unique.
2. registry can resolve built-ins.
3. baseline generated RPG blueprint validates OK.
4. unknown capability produces diagnostic.
5. missing required capability produces error.
6. conflict produces error.
7. optional missing capability produces warning.
8. future imported-map blueprint reports planned/missing capabilities without crashing.
9. product smoke passes.
10. existing product smoke tests pass.

## Required checks

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Capability"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~GameBlueprint"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-runtime-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario expanded-contract-batch-smoke
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-content-interaction-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario active-package-quest-dialogue-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-map-placement-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario content-language-policy
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario game-blueprint-capability-compatibility

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

No manual UI verification is required unless Codex changes WinForms UI.

## Stop conditions

Stop and report if:
- package schema changes become necessary;
- runtime changes become necessary;
- WinForms changes become necessary;
- `.sln` or `.csproj` changes are required;
- dynamic plugin system becomes necessary;
- more than 18 files need changes;
- check-all fails after 2 repair attempts.

## Final report

Russian report with:
- files read;
- files changed;
- model types added;
- built-in capability ids;
- compatibility rules;
- preset blueprints;
- smoke/check results;
- confirmation that runtime/UI/package schema were not changed;
- recommended next slice.
