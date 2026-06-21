# Product Slice 013 Task: Catalog-backed Composition Diagnostics Foundation

## Task type

Bounded foundation slice.

## Goal

Add a consolidated composition diagnostics/reporting service that combines the GameBlueprint capability validator and GeneratorCatalog planning layer.

The goal is to produce a single deterministic report for a blueprint/preset:

```text
can this game be built now?
which capabilities are requested?
which requirements are missing?
which generators are selected?
which generators are planned/future?
what generator support is missing?
which actions should happen next?
```

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
docs/PRODUCT_SLICE_012_GENERATOR_CATALOG_CONTRACT_FOUNDATION.md
docs/GAME_COMPOSITION_DIAGNOSTICS_SPEC.md
src/LLMGameCreator.Application/Composition/**
src/LLMGameCreator.Application/Projects/ContentLanguagePolicy.cs
tests/LLMGameCreator.Tests/Application/GameBlueprintCapabilityTests.cs
tests/LLMGameCreator.Tests/Application/GeneratorCatalogTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/GameBlueprintCapabilityCompatibilitySmokeTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/GeneratorCatalogContractSmokeTests.cs
.devflow/scripts/run-product-smoke.ps1
```

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
Do not change Runtime, WinForms or package schema.
Do not implement UI.
Do not execute generator modules.
Do not implement plugins.
Do not implement semantic world model/imported maps/lazy worlds/procedural quests.

## Required implementation

### 1. Add diagnostics report models

Suggested types:

```text
GameCompositionDiagnosticsReport
GameCompositionReadiness
GameCompositionDiagnosticItem
GameCompositionRecommendedAction
```

Readiness values:

```text
BuildableNow
BuildableWithWarnings
PlannedFuture
MissingRequirements
Conflict
Invalid
```

Report should include at least:

```text
BlueprintId
Title
GameKind
ContentLanguage
Readiness
CapabilityValidationResult
GeneratorCatalogValidationResult
GeneratorPlanningResult
SelectedCurrentGeneratorIds
RelatedPlannedGeneratorIds
MissingGeneratorCapabilityIds
RecommendedActions
```

### 2. Add diagnostics service

Suggested type:

```text
GameCompositionDiagnosticsService
```

It should depend on / use:
- `GameBlueprintCompositionValidator`
- `GeneratorCatalogValidator`
- `GeneratorPlanResolver`

It should not execute any generator.

### 3. Add markdown renderer

Suggested type:

```text
GameCompositionDiagnosticsMarkdownRenderer
```

Output should be deterministic and useful for future UI/report display.

Minimum sections:

```text
# Game Composition Diagnostics
Blueprint
Readiness
Content language
Capability diagnostics
Generator catalog diagnostics
Selected current generators
Related planned generators
Missing generator support
Recommended actions
```

No timestamps.

### 4. Recommended actions

Generate simple deterministic recommended actions.

Examples:
- missing capability -> `Add or request capability '<id>'.`
- conflict -> `Remove one of conflicting capabilities '<a>' / '<b>'.`
- planned generator -> `Implement planned generator '<id>' before runtime use.`
- missing generator support -> `Add generator support for capability '<id>'.`

### 5. Product smoke scenario

Add:

```text
composition-diagnostics-report
```

Smoke should verify:
1. baseline generated RPG preset report is `BuildableNow` or `BuildableWithWarnings`, but not error.
2. baseline report has selected current generators.
3. realistic city survival imported-map future preset report is `PlannedFuture` or `MissingRequirements`, not crash.
4. broken blueprint report is `MissingRequirements`, `Conflict`, or `Invalid`.
5. markdown renderer output is non-empty and deterministic.
6. no LLM/provider call.

### 6. State/docs

Update:
- `docs/PRODUCT_SMOKE_SCENARIOS.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `.devflow/CURRENT_RUN.md`

Mark Slice 012 accepted/completed and Slice 013 completed only after checks pass.

## Tests

Required tests:
1. baseline report is buildable or warning-only.
2. future imported-map report returns planned/missing diagnostics.
3. broken blueprint returns error readiness.
4. selected generator ids are deterministic.
5. markdown renderer output is deterministic.
6. recommended actions are deterministic.
7. product smoke `composition-diagnostics-report` passes.
8. existing product smoke tests pass.

## Required checks

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~CompositionDiagnostics"
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
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario composition-diagnostics-report

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

No manual UI verification is required. This slice must not change WinForms UI.

## Stop conditions

Stop and report if:
- Runtime/WinForms/package schema changes become necessary;
- generator execution becomes necessary;
- plugin system becomes necessary;
- semantic model/imported map/lazy world implementation becomes necessary;
- `.sln` or `.csproj` changes are required;
- more than 18 files need changes;
- check-all fails after 2 repair attempts.

## Final report

Russian report with:
- files read;
- files changed;
- report model types added;
- readiness rules;
- recommended action rules;
- markdown renderer behavior;
- smoke/check results;
- confirmation that Runtime/UI/package schema were not changed;
- recommended next slice.
