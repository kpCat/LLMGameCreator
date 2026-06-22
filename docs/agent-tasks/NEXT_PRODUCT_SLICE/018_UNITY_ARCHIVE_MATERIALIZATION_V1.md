# Product Slice 018 Task: Unity Archive Materialization v1

## Task type

Bounded editor-side archive materialization slice.

## Goal

Create the first real deterministic Unity-game-archive artifact folder, based on the existing Slice 016 contracts and Slice 017 dry-run exporter.

This must not implement Unity. It only writes the archive shape that a future Unity player/runtime will eventually load.

## Recommended Codex reasoning level

High.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/PRODUCT_SLICE_016_UNITY_TARGET_CONTRACT_FOUNDATION.md
docs/PRODUCT_SLICE_017_UNITY_ARCHIVE_EXPORT_DRY_RUN.md
docs/UNITY_ARCHIVE_EXPORT_DRY_RUN_SPEC.md
docs/UNITY_ARCHIVE_MATERIALIZATION_SPEC.md
src/LLMGameCreator.Application/Composition/**
tests/LLMGameCreator.Tests/Application/UnityArchiveExportTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveExportDryRunSmokeTests.cs
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
Do not create Unity project files.
Do not build Unity.
Do not call LLM/provider.
Do not execute generators.
Do not execute Lua.
Do not implement ComfyUI/Suno integration.
Do not change Runtime, WinForms, GamePackage schema or generator-library.

## Required implementation

### 1. Add materialization models

Suggested types:

```text
UnityArchiveMaterializationRequest
UnityArchiveMaterializationResult
UnityArchiveMaterializedFile
UnityArchiveMaterializationReadiness
UnityArchiveMaterializationDiagnostic
```

Readiness:

```text
MaterializedPlayableContract
MaterializedMetadataOnly
MaterializedWithWarnings
Blocked
Invalid
```

### 2. Add materialization service

Suggested type:

```text
UnityArchiveMaterializationService
```

It should:
- accept project root;
- accept design brief, target profile, archive manifest and runtime modules;
- call/consume `UnityArchiveExportDryRunService`;
- write deterministic files under `.llmgc/unity-archive/`;
- optionally create `.llmgc/unity-archive.zip`;
- never write outside project root;
- write UTF-8 without BOM where possible.

### 3. Required archive files

Write at least:

```text
manifest/unity-game-archive.json
composition/game-design-brief.json
composition/unity-target-profile.json
composition/runtime-modules-index.json
ui/layouts-index.json
assets/asset-requests.json
audio/audio-requests.json
localization/index.json
lua/modules-index.json
export-report.md
export-validation.json
```

These are contract/meta files. They do not need real Unity assets yet.

### 4. Future-blocked behavior

If dry-run readiness is `BlockedByFutureModules`, the service may still write a metadata-only archive folder, but result readiness must clearly say metadata-only/blocked, not playable.

If dry-run readiness is `MissingRequirements` or `Invalid`, do not write a playable archive. You may write only validation output if this is safer.

### 5. Optional zip

If implementing zip is small and safe:
- produce `.llmgc/unity-archive.zip`;
- stable entry ordering;
- no absolute paths;
- no `..` entries;
- deterministic content.

Do not add packages for zip.

### 6. Product smoke

Add scenario:

```text
unity-archive-materialization
```

Smoke verifies:
1. archive directory is created.
2. required manifest/meta files exist.
3. archive manifest JSON is valid.
4. runtime module index exists and contains current module ids.
5. asset/audio request files exist even if empty.
6. future target produces metadata-only/blocked result without crash.
7. optional zip, if produced, has safe relative entries.
8. no Unity/provider/generator/Runtime/GamePackage schema execution.

### 7. State/docs

Update:
- `docs/PRODUCT_SMOKE_SCENARIOS.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `.devflow/CURRENT_RUN.md`

Mark Slice 017 accepted/completed and Slice 018 completed only after checks pass.

## Tests

Required tests:
1. materialization creates required archive files.
2. materialized files are deterministic across two runs.
3. unsafe paths cannot escape output directory.
4. future target is metadata-only/blocked, not playable.
5. invalid/missing requirement blocks playable archive.
6. optional zip entries are safe and deterministic if zip is implemented.
7. product smoke `unity-archive-materialization` passes.
8. existing ProductSmoke tests pass.

## Required checks

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveMaterialization"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-target-contract
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-export-dry-run
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-materialization

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

No manual UI verification is required. This slice must not change WinForms UI.

## Stop conditions

Stop and report if:
- Unity implementation becomes necessary;
- Runtime/GamePackage schema/WinForms changes become necessary;
- provider/generator/Lua execution becomes necessary;
- `.sln` or `.csproj` changes are required;
- more than 18 files need changes;
- check-all fails after 2 repair attempts.

## Final report

Russian report with files read/changed, materialization service behavior, output files, future-blocked behavior, optional zip behavior, smoke/check results, confirmation that Unity/Runtime/UI/package schema/provider/generator/Lua execution were not implemented, and recommended next slice.
