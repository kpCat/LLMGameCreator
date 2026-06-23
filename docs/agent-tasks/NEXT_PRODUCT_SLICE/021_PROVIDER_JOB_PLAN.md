# Product Slice 021 Task: Unity Archive Provider Job Plan v1

## Task type

Large but bounded editor-side production pipeline slice.

## Recommended executor

Use Kilo Code / free model first.

Reason: this is deterministic backend work: models, JSON materialization, validation, smoke tests. It is a good fit for Kilo if boundaries are respected.

Use Codex only for review or emergency repair if Kilo fails.

## Branch

Start from current `main`.

Suggested branch name:

```text
kilo-s021-provider-plan
```

Do not work directly on `main`.

## Goal

Turn Slice 020 request metadata into deterministic provider-specific job plans and fulfillment slot manifests.

Current archive has Unity target contract, materialized archive metadata, game-data payload and asset/audio/Lua request pipeline. Slice 021 adds:

```text
request metadata
→ fulfillment slots
→ provider job plan
```

No actual provider execution is allowed.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/PRODUCT_SLICE_020_UNITY_ARCHIVE_ASSET_AUDIO_LUA_REQUEST_PIPELINE_V1.md
docs/PRODUCT_SLICE_021_PROVIDER_JOB_PLAN.md
docs/UNITY_ARCHIVE_PROVIDER_JOB_PLAN_SPEC.md
src/LLMGameCreator.Application/Composition/UnityArchiveRequestPipelineModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveRequestPipelineService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveRequestBuildContext.cs
src/LLMGameCreator.Application/Composition/UnityArchiveAssetRequestBuilder.cs
src/LLMGameCreator.Application/Composition/UnityArchiveAudioRequestBuilder.cs
src/LLMGameCreator.Application/Composition/UnityArchiveLuaModuleRequestBuilder.cs
src/LLMGameCreator.Application/Composition/UnityArchiveRequestDiagnosticsBuilder.cs
src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationModels.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveRequestPipelineTests.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveMaterializationTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveRequestPipelineSmokeTests.cs
.devflow/scripts/run-product-smoke.ps1
docs/PRODUCT_SMOKE_SCENARIOS.md
```

Do not perform broad repository discovery. If another file is needed, read only that file and explain why in final report.

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
Do not run git commands.

## Required implementation

### 1. Add provider job plan models

Suggested names:

```text
UnityArchiveProviderJobPlanRequest
UnityArchiveProviderJobPlanResult
UnityArchiveProviderJobPlanDiagnostic
UnityArchiveFulfillmentPlan
UnityArchiveFulfillmentSlot
UnityArchiveAssetSlot
UnityArchiveAudioSlot
UnityArchiveLuaModuleSlot
UnityArchiveProviderJob
UnityArchiveProviderJobBatch
UnityArchiveProviderJobIndex
UnityArchiveProviderReadinessReport
UnityArchiveProviderJobReadiness
```

Reuse `UnityArchiveRequestProviderKind` where possible.

### 2. Add provider job plan service

Suggested service:

```text
UnityArchiveProviderJobPlanService
```

Inputs:
- project root;
- request pipeline result;
- archive manifest;
- design brief;
- target profile.

Outputs:
- deterministic slot manifests;
- deterministic provider job batches;
- diagnostics;
- readiness.

### 3. Create slots

For every asset request, create an asset slot.
For every audio request, create an audio slot.
For every Lua module request, create a Lua module slot.

Slot fields should include at minimum:
- slot id;
- request id/module id;
- provider kind;
- expected output relative path;
- required/optional flag;
- status `missing`;
- source ref.

Do not write expected output files. Only write metadata.

### 4. Expected output paths

Use safe deterministic archive-relative paths.

Examples:

```text
assets/generated/portrait/portrait.npc.npc-alpha.png
assets/generated/icon/icon.item.item-key.png
assets/generated/tile_texture/tile.tile-grass.png
audio/generated/ui_sfx/sfx.ui.click.wav
audio/generated/music/music.theme.short_sfx.wav
lua/generated/lua-request.inventory.lua
```

Rules:
- no absolute paths;
- no `..`;
- normalize unsafe characters;
- image slots use `.png`;
- audio slots use `.wav` for now;
- Lua slots use `.lua`;
- paths are future expected outputs only.

### 5. Create provider jobs

Group jobs by provider kind:

```text
manual_import
comfyui_future
suno_future
local_audio_future
procedural_future
none
```

Write provider job files:

```text
providers/manual-import/jobs.json
providers/comfyui/jobs.json
providers/suno/jobs.json
providers/local-audio/jobs.json
providers/procedural/jobs.json
```

Requests with provider `none` should not create executable jobs. They should appear in `lua/module-slots.json` and fulfillment plan as metadata-only.

Provider job fields should include:
- job id;
- provider kind;
- request id;
- slot id;
- expected output relative path;
- prompt/instruction copied from request;
- source ref;
- tags/metadata where available;
- readiness `planned_not_executed`.

No provider execution.

### 6. Materialization integration

Update `UnityArchiveMaterializationService` to include provider job plan files after request pipeline is built.

Required files:

```text
production/fulfillment-plan.json
production/readiness-report.json
assets/asset-slots.json
audio/audio-slots.json
lua/module-slots.json
providers/manual-import/jobs.json
providers/comfyui/jobs.json
providers/suno/jobs.json
providers/local-audio/jobs.json
providers/procedural/jobs.json
```

These files must be written even when request arrays are empty.

### 7. Validation

Detect:
- duplicate slot ids;
- duplicate job ids;
- unsafe expected output relative paths;
- unknown provider kind mapping;
- missing slot for a request;
- job with provider `none`;
- executable provider claims.

Future provider kinds should be warnings, not errors, unless paths/ids are invalid.

### 8. Product smoke

Add product smoke scenario:

```text
unity-archive-provider-job-plan
```

Smoke verifies:
1. materialization writes all required provider job plan files;
2. all files are valid JSON with schemaVersion;
3. slot counts match request counts for assets/audio/Lua;
4. provider job files exist for manual/comfyui/suno/local-audio/procedural;
5. no expected output file is physically generated;
6. expected output paths are relative and safe;
7. repeated materialization is deterministic;
8. existing request pipeline smoke still passes.

### 9. State/docs

Update:
- `docs/PRODUCT_SMOKE_SCENARIOS.md`;
- `docs/CURRENT_GENERATOR_STATE.md`;
- `docs/CURRENT_GENERATOR_STATE.json`;
- `.devflow/CURRENT_RUN.md`.

Mark Slice 020 accepted/completed and Slice 021 completed only after checks pass.

## Tests

Required tests:
1. provider job service creates empty valid manifests with empty request pipeline.
2. asset requests produce asset slots and correct provider jobs.
3. audio requests produce audio slots and correct provider jobs.
4. Lua module requests produce Lua slots and no executable provider jobs for provider `none`.
5. expected output paths are safe and deterministic.
6. duplicate slot/job ids produce diagnostics.
7. materialization writes all required files.
8. provider job plan output deterministic across two runs.
9. product smoke `unity-archive-provider-job-plan` passes.
10. existing request pipeline smoke still passes.

## Required checks

Focused:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveProviderJob"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveRequestPipeline"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveMaterialization"
```

Smoke:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-request-pipeline
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-provider-job-plan
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
- more than 24 files need changes;
- check-all fails after 2 repair attempts;
- task starts turning into actual ComfyUI/Suno integration.

## Final report

Russian report with:

```text
files read
files changed
provider job plan models/services
slot files written
provider job files written
path safety behavior
determinism proof
tests/checks results
confirmation that Unity/Runtime/WinForms/GamePackage schema/provider/generator/LLM/Lua execution were not touched
recommendation: merge / cleanup / reject
```
