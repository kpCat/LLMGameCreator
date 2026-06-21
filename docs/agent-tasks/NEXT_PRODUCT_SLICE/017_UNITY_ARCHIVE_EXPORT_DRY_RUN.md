# Product Slice 017 Task: Unity Archive Validation/Export Dry Run

## Task type

Bounded editor-side export planning slice.

## Goal

Add a deterministic dry-run exporter for Unity game archives.

The service should consume the Slice 016 contracts and produce a project-local export plan/manifest/validation report under `.llmgc/unity-export-dry-run/`.

It must not create a Unity project, build an executable, execute generators, call providers, change Runtime, or change GamePackage schema.

## Recommended Codex reasoning level

High.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/PRODUCT_SLICE_016_UNITY_TARGET_CONTRACT_FOUNDATION.md
docs/UNITY_TARGET_RUNTIME_CONTRACT_SPEC.md
docs/UNITY_ARCHIVE_EXPORT_DRY_RUN_SPEC.md
src/LLMGameCreator.Application/Composition/**
tests/LLMGameCreator.Tests/Application/UnityTargetContractTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityTargetContractSmokeTests.cs
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

Do not add NuGet packages. Do not create Unity project files. Do not call LLM/provider. Do not execute generators. Do not implement ComfyUI/Suno integration. Do not change Runtime, WinForms, GamePackage schema or generator-library.

## Required implementation

### 1. Add dry-run export models

Suggested types under `Application/Composition`:

```text
UnityArchiveExportDryRunRequest
UnityArchiveExportDryRunResult
UnityArchiveExportPlan
UnityArchivePlannedFile
UnityArchiveExportDiagnostic
UnityArchiveExportReadiness
```

Readiness values:

```text
ExportableNow
ExportableWithWarnings
BlockedByFutureModules
MissingRequirements
Invalid
```

### 2. Add dry-run export service

Suggested type:

```text
UnityArchiveExportDryRunService
```

It should accept project root, `GameDesignBrief`, `UnityTargetProfile`, `UnityGameArchiveManifest`, runtime module contracts/catalog from preset provider, consume `UnityTargetContractValidator`, build deterministic planned file list, detect future modules and missing requirements, write dry-run files to `.llmgc/unity-export-dry-run/`, and never write outside project root.

### 3. Add markdown renderer

Suggested type:

```text
UnityArchiveExportPlanMarkdownRenderer
```

Required sections:

```text
# Unity Archive Export Dry Run
Readiness
Design brief
Target profile
Runtime modules
Planned files
Diagnostics
Blocked/future modules
```

No timestamps.

### 4. Output files

Write at least:

```text
.llmgc/unity-export-dry-run/unity-archive-plan.json
.llmgc/unity-export-dry-run/unity-archive-plan.md
.llmgc/unity-export-dry-run/unity-archive-manifest.json
.llmgc/unity-export-dry-run/validation-report.json
```

Use UTF-8. Stable ordering. Deterministic output.

### 5. Product smoke

Add scenario:

```text
unity-archive-export-dry-run
```

Smoke should verify:
1. dry-run directory is created;
2. JSON plan exists;
3. markdown plan exists;
4. archive manifest JSON exists;
5. validation report JSON exists;
6. generic top-down target is exportable or warning-only;
7. mixed/future target is blocked by future modules but does not crash;
8. no Unity/provider/generator/Runtime/GamePackage schema execution occurs.

### 6. State/docs

Update:
- `docs/PRODUCT_SMOKE_SCENARIOS.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `.devflow/CURRENT_RUN.md`

Mark Slice 016 accepted/completed and Slice 017 completed only after checks pass.

## Tests

Required tests:
1. dry-run export creates expected files.
2. planned files are deterministic.
3. unsafe paths cannot escape output directory.
4. generic top-down target produces exportable/warning readiness.
5. future target reports blocked/future modules.
6. markdown renderer is deterministic.
7. product smoke `unity-archive-export-dry-run` passes.
8. existing ProductSmoke tests pass.

## Required checks

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveExport"
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
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-target-contract
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-export-dry-run

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

No manual UI verification is required. This slice must not change WinForms UI.

## Stop conditions

Stop and report if Unity implementation, Runtime/GamePackage schema/WinForms changes, provider/generator execution, `.sln`/`.csproj` changes become necessary, more than 18 files need changes, or check-all fails after 2 repair attempts.

## Final report

Russian report with files read/changed, export dry-run service behavior, output files, readiness rules, future-module handling, smoke/check results, confirmation that Unity/Runtime/UI/package schema/provider/generator execution were not implemented, and recommended next slice.
