# Cleanup Task: Slice 020 Request Pipeline Quality Pass

## Task type

Narrow cleanup / quality pass on branch `kilo-free`.

This is not a new product slice. Do not expand functionality. Improve the quality of the already implemented Slice 020 before merge decision.

## Goal

Clean up `UnityArchiveAssetAudioLuaRequestService` and its materialization integration so Slice 020 is safer to merge into `main`.

Target fixes:

1. Avoid double `BuildRequests(...)` in `UnityArchiveMaterializationService`.
2. Rename/repair misleading readiness `BlockedByFutureProviders`.
3. Remove unused/duplicated model `UnityArchiveLuaModuleRequestEntry` or make it actually used.
4. Aggregate future-provider diagnostics instead of producing one warning per request.
5. Add focused tests for duplicate request IDs / blank or unknown source ID collisions / aggregated warnings.
6. Preserve current behavior and all smoke scenarios.

## Read first only these files

```text
AGENTS.md
docs/CURRENT_GENERATOR_STATE.md
docs/PRODUCT_SMOKE_SCENARIOS.md
src/LLMGameCreator.Application/Composition/UnityArchiveRequestPipelineModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveRequestPipelineService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationService.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveRequestPipelineTests.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveMaterializationTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveRequestPipelineSmokeTests.cs
.devflow/scripts/run-product-smoke.ps1
.devflow/CURRENT_RUN.md
docs/CURRENT_GENERATOR_STATE.json
```

Do not perform broad repository discovery. If another file seems necessary, mention why in final report and read only that file.

## Allowed files

```text
src/LLMGameCreator.Application/Composition/UnityArchiveRequestPipelineModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveRequestPipelineService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationService.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveRequestPipelineTests.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveMaterializationTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveRequestPipelineSmokeTests.cs
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

Only touch `docs/PRODUCT_SMOKE_SCENARIOS.md` or `.devflow/scripts/run-product-smoke.ps1` if tests prove they are inconsistent. Prefer no change there.

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

Do not add packages. Do not create Unity project files. Do not execute provider/generator/LLM/Lua. Do not change GamePackageDefinition. Do not run git commands.

## Required cleanup

### 1. Remove double BuildRequests

Current materialization builds request pipeline once for diagnostics and again during file writing.

Refactor so it is built once in `MaterializeAsync` and passed into file writing.

Suggested shape:

```text
var pipelineResult = _requestPipelineService.BuildRequests(...);
var diagnostics = CreateDiagnostics(..., pipelineResult);
...
await WriteArchiveFilesAsync(..., pipelineResult, ...);
```

Acceptance:
- `BuildRequests` called once per materialization.
- Existing request files are identical or semantically identical.
- Existing materialization tests still pass.

### 2. Fix misleading readiness

Current enum value `BlockedByFutureProviders` is misleading when diagnostics contain real errors like duplicate request ids.

Replace with preferred value:

```text
BlockedByErrors
```

Acceptance:
- warnings from future providers keep readiness `ReadyWithWarnings`;
- duplicate/blank request errors produce `BlockedByErrors`;
- normal future-provider-only case never produces blocked readiness.

### 3. Remove unused Lua request entry model

`UnityArchiveLuaModuleRequestEntry` appears unused while `UnityArchiveLuaModuleRequests` stores `UnityArchiveLuaModuleRequest`.

Preferred: remove it if unused.

Acceptance:
- no dead model remains.
- JSON shape of `lua/module-requests.json` remains useful and stable.

### 4. Aggregate future provider warnings

Do not emit one warning per asset/audio request for future provider kinds.

Instead emit one warning per provider kind and request category, e.g.:

```text
request.diagnostic.future_provider_kind.asset.comfyui_future
Asset requests use future provider 'comfyui_future' for 12 request(s).
```

and:

```text
request.diagnostic.future_provider_kind.audio.local_audio_future
Audio requests use future provider 'local_audio_future' for 8 request(s).
```

Future Lua module warnings may remain per module because there are few of them.

Acceptance:
- large package does not create hundreds of identical provider warnings.
- diagnostics stay deterministic and sorted.
- smoke still verifies future metadata warnings exist.

### 5. Add focused validation tests

Add/adjust tests for:

1. Duplicate generated source ids produce duplicate request id diagnostics and `BlockedByErrors`.
2. Blank source ids normalize to `unknown`, and two blank source ids produce deterministic duplicate diagnostics.
3. Future provider warnings are aggregated, not one per request.
4. Existing current top-down package still returns `ReadyWithWarnings`, not blocked.
5. Existing future mixed profile still returns warning-only unless actual duplicate/blank id errors are present.

Do not overbuild validation infrastructure.

### 6. Preserve deterministic output

After cleanup:
- request IDs stable;
- request sorting stable;
- diagnostics sorting stable;
- no timestamps;
- UTF-8 without BOM behavior unchanged.

## Required checks

Focused first:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveRequestPipeline"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveMaterialization"
```

Product smoke:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-request-pipeline
```

Final:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Stop conditions

Stop and report if:
- cleanup requires changing forbidden files;
- changing readiness would cascade into more than 8 files;
- tests require broad rewrite;
- check-all fails after 2 repair attempts;
- task starts turning into a new feature.

## Final report

Report in Russian:

```text
files read
files changed
what was cleaned
readiness behavior before/after
future warning aggregation behavior
tests run and results
confirmation that Unity/Runtime/WinForms/GamePackage schema/provider/generator/LLM/Lua were not touched
recommendation: merge / do not merge / needs another cleanup
```
