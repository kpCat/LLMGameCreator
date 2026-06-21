# Product Slice 014 Task: Headless Composition Report Export

## Goal

Add a headless export service for `GameCompositionDiagnosticsReport`.

The service should render and persist deterministic composition diagnostics markdown and an index under the project `.llmgc` folder. This prepares a stable artifact contract for a future read-only Composition Workbench UI.

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
docs/PRODUCT_SLICE_013_CATALOG_BACKED_COMPOSITION_DIAGNOSTICS.md
docs/GAME_COMPOSITION_DIAGNOSTICS_SPEC.md
docs/PRODUCT_SLICE_014_HEADLESS_COMPOSITION_REPORT_EXPORT.md
docs/COMPOSITION_REPORT_EXPORT_SPEC.md
src/LLMGameCreator.Application/Composition/**
src/LLMGameCreator.Application/Projects/**
tests/LLMGameCreator.Tests/Application/CompositionDiagnosticsTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/CompositionDiagnosticsReportSmokeTests.cs
.devflow/scripts/run-product-smoke.ps1
```

Inspect existing project settings persistence style, especially `ContentLanguagePolicy`.

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

Do not add NuGet packages. Do not call LLM/provider. Do not change Runtime, WinForms, package schema, generator-library, Lua/Scripting, solution or project files. Do not execute generators or implement plugins.

## Required implementation

### 1. Export service

Add under `Application/Composition`:

```text
GameCompositionDiagnosticsExportService
GameCompositionDiagnosticsExportRequest
GameCompositionDiagnosticsExportResult
GameCompositionDiagnosticsExportIndex
GameCompositionDiagnosticsExportIndexEntry
```

The service should:
- accept project root path;
- accept a `GameCompositionDiagnosticsReport`;
- use `GameCompositionDiagnosticsMarkdownRenderer`;
- write `.llmgc/composition-diagnostics/<safe-blueprint-id>.composition-report.md`;
- write/update `.llmgc/composition-diagnostics/index.json`.

### 2. Safe paths

Must:
- reject empty project root;
- create directory if missing;
- prevent path traversal from blueprint id;
- sanitize unsafe file name chars;
- write UTF-8;
- keep files under `.llmgc/composition-diagnostics`.

### 3. Determinism

Markdown content must be deterministic and timestamp-free.

Index ordering must be deterministic by blueprint id. Avoid index timestamps unless a fixed/injected clock is used in tests.

### 4. Product smoke

Add scenario:

```text
composition-report-export
```

Smoke should:
1. build baseline generated RPG diagnostics report;
2. export it to temp project root;
3. verify markdown exists;
4. verify index exists;
5. verify markdown contains readiness and selected generators sections;
6. verify export is deterministic across two runs;
7. verify no files are written outside project root;
8. verify no LLM/provider/generator execution.

### 5. State/docs

Update:
- `docs/PRODUCT_SMOKE_SCENARIOS.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `.devflow/CURRENT_RUN.md`

Mark Slice 013 accepted/completed and Slice 014 completed only after checks pass.

## Tests

Required tests:
1. export creates composition diagnostics directory.
2. export writes markdown.
3. export writes index.
4. blueprint id is sanitized for file name.
5. path traversal in blueprint id cannot escape output directory.
6. repeated export is deterministic.
7. index entries are sorted deterministically.
8. product smoke `composition-report-export` passes.
9. existing ProductSmoke tests pass.

## Required checks

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~CompositionDiagnosticsExport"
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

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

No manual UI verification is required. This slice must not change WinForms UI.

## Stop conditions

Stop and report if:
- Runtime/WinForms/package schema changes become necessary;
- generator execution or plugin system becomes necessary;
- semantic model/imported map/lazy world implementation becomes necessary;
- `.sln` or `.csproj` changes are required;
- more than 16 files need changes;
- check-all fails after 2 repair attempts.

## Final report

Russian report with files read/changed, export service behavior, output paths, index format, path-safety handling, deterministic export proof, smoke/check results, confirmation that Runtime/UI/package schema were not changed, and recommended next slice.
