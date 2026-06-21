# Product Slice 015 Task: Read-only Composition Workbench UI

## Task type

Bounded read-only UI consumer slice.

## Goal

Add a read-only Composition Workbench page that consumes `GameCompositionDiagnosticsReport` and saved exported reports. It must not execute generators, plugins, Runtime, Lua, or providers.

## Recommended Codex reasoning level

High.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/WINFORMS_DESIGNER_RULES.md
docs/LLMGAMECREATOR_OFFICIAL_PRODUCT_PLAN.md
docs/GAME_ASSEMBLY_WORKBENCH_ARCHITECTURE.md
docs/PRODUCT_SLICE_015_READ_ONLY_COMPOSITION_WORKBENCH_UI.md
docs/COMPOSITION_WORKBENCH_UI_SPEC.md
src/LLMGameCreator.Application/Composition/**
src/LLMGameCreator.Application/Projects/**
src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/**/*
tests/LLMGameCreator.Tests/ProductSmoke/CompositionReportExportSmokeTests.cs
.devflow/scripts/run-product-smoke.ps1
```

## Allowed files

```text
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/**
src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/**/*
tests/LLMGameCreator.Tests/WinForms/**
tests/LLMGameCreator.Tests/ProductSmoke/**
.devflow/scripts/run-product-smoke.ps1
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

Only touch broader `Pages/**/*` files if needed to register or navigate to the new page.

## Forbidden files

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.Infrastructure/Generation/**
generator-library/**
LLMGameCreator.sln
*.csproj
.devflow/NEXT_TASK.md
.devflow/task-queue.json
```

Do not add NuGet packages. Do not call LLM/provider. Do not change Runtime, package schema or generator-library. Do not execute generator modules. Do not implement plugins, semantic world model, imported maps, lazy worlds, or procedural quests.

## Required implementation

### 1. Page

Create a Designer-safe UserControl page:

```text
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/
```

Suggested files:

```text
CompositionWorkbenchPageControl.cs
CompositionWorkbenchPageControl.Designer.cs
CompositionWorkbenchPresenter.cs
CompositionWorkbenchViewModels.cs
```

Minimum UI:
- preset selector/list;
- saved reports selector/list;
- readiness label;
- read-only diagnostics/actions summary;
- read-only markdown text area;
- buttons: Refresh reports, Build preview report, Export report.

### 2. Presenter

Presenter should use existing services:
- `GameBlueprintPresetProvider`
- `BuiltInCapabilityRegistry`
- `BuiltInGeneratorCatalog`
- `GameCompositionDiagnosticsService`
- `GameCompositionDiagnosticsMarkdownRenderer`
- `GameCompositionDiagnosticsExportService`
- content language policy model/service if needed.

Do not duplicate readiness logic in UI.

### 3. Behavior

- On load, show presets.
- Build report for selected preset and render markdown.
- Export writes under `.llmgc/composition-diagnostics`.
- Refresh reports reads index and loads selected markdown.
- If no current project root exists, show a clear read-only message and allow in-memory preview when safe.
- No provider/generator/runtime execution.

### 4. Registration

Register page in existing WinForms navigation/composition pattern with minimal changes.

Label:

```text
Composition Workbench
```

or Russian if existing navigation uses Russian.

### 5. Product smoke

Add scenario:

```text
composition-workbench-readonly
```

Smoke verifies:
1. presenter/page can be constructed headlessly;
2. baseline preset report can be built;
3. markdown is returned/displayed;
4. export creates report files in temp project root;
5. saved report refresh can see exported report;
6. no LLM/provider/generator/runtime execution;
7. existing ProductSmoke scenarios still pass.

### 6. State/docs

Update:
- `docs/PRODUCT_SMOKE_SCENARIOS.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `.devflow/CURRENT_RUN.md`

Mark Slice 014 accepted/completed and Slice 015 completed only after checks pass.

## Tests

Required tests:
1. presenter lists blueprint presets.
2. presenter builds baseline report.
3. presenter renders markdown.
4. presenter exports report.
5. presenter refreshes saved reports from index.
6. UserControl can be constructed without throwing.
7. product smoke `composition-workbench-readonly` passes.
8. existing ProductSmoke tests pass.

## Required checks

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~CompositionWorkbench"
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
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario composition-report-export
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario composition-workbench-readonly

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

Manual UI click-through is optional if headless tests and smoke pass.

## Stop conditions

Stop and report if:
- adding the page requires broad navigation rewrite;
- Runtime/package schema/generator execution becomes necessary;
- `.sln` or `.csproj` changes are required;
- Designer-safe split cannot be preserved;
- more than 20 files need changes;
- check-all fails after 2 repair attempts.

## Final report

Russian report with files read/changed, page/presenter behavior, registration point, export/refresh behavior, smoke/check results, confirmation that Runtime/package schema/generator execution were not changed, whether manual UI check was skipped and why, and recommended next slice.
