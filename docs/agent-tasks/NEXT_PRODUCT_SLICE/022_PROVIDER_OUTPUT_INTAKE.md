# Product Slice 022 Task: Provider Output Intake & Fulfillment State v1

## Executor decision

Use Kilo Code first.

Reason: this is deterministic backend/file-system metadata work: models, scanner service, JSON materialization, validation and smoke tests.

Use Codex only for review or repair if Kilo fails.

## Branch / git policy

Work in the repository as it is currently checked out.

Do not create branches.
Do not switch branches.
Do not merge.
Do not rebase.
Do not cherry-pick.
Do not run git commands.
Branch management is handled manually by the user.

## Prerequisite

Run this only after S021 Provider Job Plan is reviewed and accepted in the current working line.

Do not run this task as part of S021 hardening.

## Goal

Add a metadata-only fulfillment scanner for provider job plan outputs.

After S021 the Unity archive has:
- request metadata;
- fulfillment slots;
- provider-specific jobs;
- expected output relative paths.

S022 must add a scanner/state layer that checks whether expected future output files exist, validates them lightly, and writes deterministic fulfillment state/index files.

No provider execution is allowed.

## Required archive files

Materialization must write these files under `.llmgc/unity-archive/`:

```text
production/fulfillment-state.json
production/fulfilled-assets-index.json
production/fulfilled-audio-index.json
production/fulfilled-lua-index.json
production/invalid-outputs.json
```

These files must exist even when all arrays are empty.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/PRODUCT_SLICE_021_PROVIDER_JOB_PLAN.md

src/LLMGameCreator.Application/Composition/UnityArchiveRequestPipelineModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveProviderJobPlanModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveProviderJobPlanService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationService.cs

tests/LLMGameCreator.Tests/Application/UnityArchiveProviderJobPlanTests.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveMaterializationTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveProviderJobPlanSmokeTests.cs

.devflow/scripts/run-product-smoke.ps1
docs/PRODUCT_SMOKE_SCENARIOS.md
```

Do not perform broad repository discovery. If another file is needed, read only that file and explain why.

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
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.Infrastructure/**
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

### 1. Fulfillment state models

Add models under `Application/Composition`, suggested names:

```text
UnityArchiveFulfillmentStateRequest
UnityArchiveFulfillmentStateResult
UnityArchiveFulfillmentStateReport
UnityArchiveFulfillmentStateEntry
UnityArchiveFulfilledAssetEntry
UnityArchiveFulfilledAudioEntry
UnityArchiveFulfilledLuaEntry
UnityArchiveInvalidOutputEntry
UnityArchiveFulfillmentStatus
UnityArchiveFulfillmentStateDiagnostic
```

Status enum:

```text
missing
available
invalid
```

### 2. Fulfillment scanner service

Suggested service:

```text
UnityArchiveFulfillmentStateService
```

Inputs:
- archive output directory path;
- `UnityArchiveProviderJobPlanResult`.

It should:
- inspect expected output relative paths from asset/audio/Lua slots;
- validate path safety using existing provider job plan path safety helper if possible;
- check physical files under archive output directory;
- produce deterministic fulfillment state.

It must not create expected output files.

### 3. Status logic

For every asset slot:
- expected `.png`;
- file missing -> `missing`;
- file exists and is safe, regular file, non-empty, `.png` -> `available`;
- otherwise -> `invalid`.

For every audio slot:
- expected `.wav`;
- same status logic.

For every Lua slot:
- expected `.lua`;
- same status logic.

Invalid cases:
- unsafe path;
- rooted path;
- `..`;
- backslash;
- colon;
- expected output path points to directory;
- wrong extension;
- empty file.

### 4. Independent scanner behavior

The scanner must be usable after materialization.

Tests must be able to:
1. materialize archive;
2. manually create a fake expected `.png` / `.wav` / `.lua` under the expected output path;
3. call the fulfillment scanner;
4. observe status `available`.

### 5. Materialization integration

After provider job plan is built, materialization should write initial fulfillment state files.

Since materialization must not create expected outputs, initial status is usually `missing`.

### 6. Determinism

- stable ordering by slot id / request id / expected path;
- no timestamps;
- no absolute paths in JSON;
- repeated scan with unchanged files returns byte-identical JSON.

### 7. Validation / diagnostics

Diagnostics should detect:
- unsafe expected paths;
- duplicate expected output paths;
- duplicate fulfillment entry ids;
- missing provider plan input;
- invalid existing files.

Severity:
- unsafe paths -> error;
- duplicate ids/paths -> error;
- invalid existing files -> error;
- missing files -> not diagnostic by default, just status `missing`.

### 8. Product smoke

Add product smoke scenario:

```text
unity-archive-fulfillment-state
```

Smoke verifies:
1. materialization writes all five fulfillment state files;
2. all files are valid JSON with schemaVersion;
3. initial state has missing entries for current expected outputs;
4. no expected output files are physically created by materialization;
5. scanner can detect manually created fake `.png`, `.wav`, `.lua` as `available`;
6. repeated scan is deterministic;
7. existing provider job plan smoke still passes.

### 9. State/docs

Update:
- `docs/PRODUCT_SMOKE_SCENARIOS.md`;
- `docs/CURRENT_GENERATOR_STATE.md`;
- `docs/CURRENT_GENERATOR_STATE.json`;
- `.devflow/CURRENT_RUN.md`.

Mark S022 completed only after checks pass.

## Tests

Required tests:
1. empty provider job plan creates empty fulfillment state and indexes.
2. missing expected outputs are marked `missing`.
3. manually created `.png`, `.wav`, `.lua` expected files are marked `available`.
4. empty existing file is marked `invalid`.
5. wrong extension is marked `invalid`.
6. unsafe expected path is diagnostic error.
7. duplicate expected output path is diagnostic error.
8. materialization writes all five fulfillment state files.
9. materialization does not create expected output files.
10. repeated scan/materialization is deterministic.
11. product smoke `unity-archive-fulfillment-state` passes.
12. existing provider job plan smoke still passes.

## Required checks

Focused:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveFulfillment"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveProviderJob"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveMaterialization"
```

Smoke:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-provider-job-plan
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-fulfillment-state
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
```

Final:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Stop conditions

Stop and report if:
- provider execution becomes necessary;
- Unity implementation becomes necessary;
- Runtime/GamePackage schema/WinForms changes become necessary;
- `.sln` or `.csproj` changes are required;
- task requires changing storage format outside `.llmgc/unity-archive`;
- more than 24 files need changes;
- check-all fails after 2 repair attempts;
- task turns into actual asset/audio/Lua generation.

## Final report

Russian report with:

```text
files read
files changed
fulfillment state models/services
state/index files written
scanner behavior
missing/available/invalid behavior
path safety behavior
determinism proof
tests/checks results
confirmation that Unity/Runtime/WinForms/GamePackage schema/provider/generator/LLM/Lua execution were not touched
recommendation: merge / cleanup / reject
```
