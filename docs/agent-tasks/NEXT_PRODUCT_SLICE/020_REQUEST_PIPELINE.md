# Product Slice 020 Task: Unity Archive Asset/Audio/Lua Request Pipeline v1

## Task type

Large but bounded editor-side production pipeline slice.

## Goal

Create the first deterministic request pipeline for future Unity archive assets, audio and Lua/data modules.

The current Unity archive can now contain contract/meta and game-data payload. Slice 020 must add request queues that describe what needs to be generated/imported later, without actually generating anything.

## Recommended Codex/Kilo reasoning level

High.

For Kilo/free models: run as one macro-slice, but do not allow broad repository wandering. Follow read-first list and stop conditions.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/PRODUCT_SLICE_016_UNITY_TARGET_CONTRACT_FOUNDATION.md
docs/PRODUCT_SLICE_017_UNITY_ARCHIVE_EXPORT_DRY_RUN.md
docs/PRODUCT_SLICE_018_UNITY_ARCHIVE_MATERIALIZATION_V1.md
docs/PRODUCT_SLICE_019_UNITY_ARCHIVE_GAME_DATA_PAYLOAD_V1.md
docs/020_PRODUCT_SLICE.md
docs/020_SPEC.md
src/LLMGameCreator.Application/Composition/GameDesignBrief.cs
src/LLMGameCreator.Application/Composition/UnityTargetContractModels.cs
src/LLMGameCreator.Application/Composition/UnityTargetContractPresetProvider.cs
src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveGameDataPayloadService.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveGameDataPayloadTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveGameDataPayloadSmokeTests.cs
.devflow/scripts/run-product-smoke.ps1
```

Do not perform broad discovery unless one of these files clearly points to a required nearby file.

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

### 1. Request models

Add models under `Application/Composition`, suggested names:

```text
UnityArchiveRequestPipelineRequest
UnityArchiveRequestPipelineResult

UnityArchiveAssetRequest
UnityArchiveAudioRequest
UnityArchiveLuaModuleRequest

UnityArchiveRequestSourceRef
UnityArchiveRequestDiagnostic
UnityArchiveRequestReadiness
UnityArchiveRequestProviderKind
UnityArchiveAssetKind
UnityArchiveAudioKind
UnityArchiveLuaModuleKind
```

Provider kinds should be metadata only:

```text
manual_import
comfyui_future
suno_future
local_audio_future
procedural_future
none
```

### 2. Request service

Suggested service:

```text
UnityArchiveAssetAudioLuaRequestService
```

It should accept:
- project root;
- `GameDesignBrief`;
- `UnityTargetProfile`;
- `UnityGameArchiveManifest`;
- runtime module contracts;
- optional `GamePackageDefinition`;
- optional payload/index data from Slice 019 if useful.

It should produce deterministic request lists for:
- assets;
- audio;
- Lua/data modules.

### 3. Asset requests

Generate metadata requests from existing package data and design/target hints.

At minimum support:
- scene illustration/background requests from generated scenes/maps;
- NPC portrait requests when NPC entries exist;
- item icon requests when items exist;
- ability/mechanic icon requests when abilities/mechanics exist;
- tile/terrain texture requests from tile prototypes;
- UI theme/widget requests from Unity target/UI layout requirements.

Do not generate actual image files.

### 4. Audio requests

At minimum support:
- UI click/confirm/cancel metadata requests if dynamic UI is required;
- footstep surface requests from tile prototypes when movement/topdown target exists;
- ability/combat/effect request metadata when abilities/combat modules exist;
- scene ambience requests from scenes;
- music theme slots from design brief/target tone.

Do not generate actual audio files.

### 5. Lua/data module requests

Generate Lua module request metadata from target/runtime modules and existing package systems.

At minimum:
- inventory module request if inventory/items exist or module requested;
- quest module request if quests exist or quest journal module requested;
- dialogue module request if dialogues/NPCs exist or dialogue module requested;
- combat module request if combat/personal combat/ability modules requested;
- crafting module request if recipes/crafting requested;
- future metadata-only requests for transport/police/army/imported-map modules when target includes those future capabilities.

Do not execute Lua. Do not add generator-library files.

### 6. Materialization integration

Update `UnityArchiveMaterializationService` so a materialized archive can include request pipeline files.

Required archive files:

```text
.llmgc/unity-archive/assets/asset-requests.json
.llmgc/unity-archive/assets/asset-request-index.json
.llmgc/unity-archive/audio/audio-requests.json
.llmgc/unity-archive/audio/audio-request-index.json
.llmgc/unity-archive/lua/module-requests.json
.llmgc/unity-archive/lua/modules-index.json
```

These files must exist even if all arrays are empty.

### 7. Validation

Detect:
- duplicate request ids;
- blank request ids;
- unsafe relative paths;
- unknown source refs;
- future provider kinds used as warnings, not errors;
- requests that claim generated file paths outside allowed archive folders;
- timestamps/non-deterministic values.

### 8. Product smoke

Add scenario:

```text
unity-archive-request-pipeline
```

Smoke verifies:
1. materialized archive contains asset/audio/lua request files;
2. files are valid JSON;
3. request ids are stable/deterministic across two runs;
4. current top-down RPG target creates at least UI and tile/scene request metadata;
5. future mixed/real-map target creates future metadata warnings without crashing;
6. no provider/generator/LLM/Lua/Unity/Runtime/GamePackage schema execution.

### 9. State/docs

Update:
- `docs/PRODUCT_SMOKE_SCENARIOS.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `.devflow/CURRENT_RUN.md`

Mark Slice 019 accepted/completed and Slice 020 completed only after checks pass.

## Tests

Required tests:
1. service creates empty valid request files with no package data.
2. service creates tile/scene/UI asset request metadata from sample package.
3. service creates UI/footstep/ambience audio request metadata from sample package.
4. service creates Lua module request metadata from target modules.
5. future provider kinds produce warnings but not errors.
6. duplicate/blank ids are diagnostics.
7. unsafe paths are rejected.
8. materialization writes all request files.
9. deterministic output across two runs.
10. product smoke `unity-archive-request-pipeline` passes.
11. existing ProductSmoke tests pass.

## Required checks

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveRequest"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveMaterialization"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-target-contract
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-export-dry-run
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-materialization
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-game-data-payload
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-request-pipeline

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Stop conditions

Stop and report if:
- Unity implementation becomes necessary;
- Runtime/GamePackage schema/WinForms changes become necessary;
- provider/generator/LLM/Lua execution becomes necessary;
- `.sln` or `.csproj` changes are required;
- more than 22 files need changes;
- check-all fails after 2 repair attempts;
- task requires broad rewrite of existing materialization/payload services.

## Final report

Russian report with:
- files read;
- files changed;
- request model/service behavior;
- generated request files;
- deterministic proof;
- future-provider warnings;
- smoke/check results;
- confirmation that Unity/Runtime/UI/package schema/provider/generator/LLM/Lua execution were not implemented;
- recommended next slice.
