# Product Slice 009 Task: Generated NPC/Encounter Map Placement

## Task type

Large bounded product slice.

## Goal

Place generated NPCs and encounters on the Runtime Preview map as preview markers.

Generated NPCs/encounters already exist in `generatedContent` and are visible in the Browser. This task connects them to the map visually and through simple preview interactions.

## Recommended Codex reasoning level

High.

Do not use Max/Ultra on first attempt.
Do not use Medium because this crosses preview projection, deterministic placement, Runtime Preview canvas/UI, smoke tests and interaction behavior.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/PRODUCT_SLICE_009_GENERATED_NPC_ENCOUNTER_MAP_PLACEMENT.md
src/LLMGameCreator.Application/RuntimePreview/GeneratedPackageRuntimePreviewService.cs
src/LLMGameCreator.Application/RuntimePreview/GeneratedContentInteractionPreviewService.cs
src/LLMGameCreator.Application/RuntimePreview/GeneratedQuestDialoguePreviewService.cs
src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.cs
src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimeMapCanvas.cs
src/LLMGameCreator.Runtime/DefaultGameRuntime.cs
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
tests/LLMGameCreator.Tests/ProductSmoke/ActivePackageQuestDialoguePreviewSmokeTests.cs
.devflow/scripts/run-product-smoke.ps1
```

Then search narrowly for map/tile/passability helpers and runtime/canvas tests.

## Allowed files

```text
src/LLMGameCreator.Application/RuntimePreview/**
src/LLMGameCreator.WinForms/Pages/RuntimePreview/**
src/LLMGameCreator.WinForms/CompositionRoot.cs
tests/LLMGameCreator.Tests/ProductSmoke/**
tests/LLMGameCreator.Tests/Runtime/**
tests/LLMGameCreator.Tests/WinForms/*RuntimePreview*Tests.cs
.devflow/scripts/run-product-smoke.ps1
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

Only touch `CompositionRoot.cs` if registering a new service is required.

## Forbidden files

```text
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.Infrastructure/Generation/**
generator-library/**
src/LLMGameCreator.Application/Design/GeneratorPlans/**
src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
src/LLMGameCreator.Runtime/DefaultGameRuntime.cs
LLMGameCreator.sln
*.csproj
.devflow/NEXT_TASK.md
.devflow/task-queue.json
```

Do not add NuGet packages.
Do not call LLM/provider.
Do not execute generated effects.
Do not change package schema.
Do not rewrite runtime engine.

## Required behavior

### 1. Deterministic placement service

Add a service/model under Application RuntimePreview.

Possible names:

```text
GeneratedMapPlacementPreviewService
GeneratedRuntimeMapMarker
GeneratedRuntimeMapMarkerType
GeneratedMapPlacementPreviewModel
```

It should accept:

```text
GamePackageDefinition package
GameState state
GeneratedPackageRuntimePreviewModel preview
```

and return markers for generated NPCs and encounters.

Each marker should include:

```text
marker id
type: npc | encounter
title
description
map id
position
reference ids
details text
source id
```

### 2. Resolve marker map id

Rules:
- NPC SceneId should resolve to generated scene SourceId, then scene PackageMapId.
- Encounter SceneId should resolve the same way.
- If scene is missing but region maps to scenes, use first linked scene/map if possible.
- If still unresolved, use current map id and emit warning/diagnostic in marker details.
- Never crash on missing references.

### 3. Resolve marker position

Rules:
- deterministic by marker id/source id;
- place within current map bounds;
- avoid player current/start position if possible;
- avoid overlapping markers if possible;
- prefer passable tiles if map passability is available;
- stable across refreshes.

Do not require perfect spatial design yet.

### 4. RuntimeMapCanvas overlay

Update RuntimeMapCanvas or Runtime Preview UI so NPC/encounter markers are visible on the map.

Markers must distinguish:
- player;
- NPC;
- encounter.

Do not break current player movement rendering.

### 5. Runtime Preview marker integration

On Start and command execution:
- rebuild marker placement for active current map;
- update canvas overlay;
- keep Generated Content Browser working.

Minimum acceptable interaction:
- Browser remains source of selection;
- map shows matching markers;
- `Append selected to log` logs marker-related refs/details when selected entry has a marker.

Preferred if simple:
- add `Inspect nearby generated marker` button;
- if existing Interact command exists, intercept nearby generated marker in UI layer and append details.

### 6. NPC dialogue integration

If marker is NPC and linked dialogue exists:
- details/log should show linked dialogue ids/titles when available;
- existing Preview dialogue flow should remain usable.

Do not implement real dialogue execution.

### 7. Encounter preview

For encounter markers:
- details/log should show setup/description, participants/NPC refs, scene/region refs;
- optional button/action `Preview encounter` may append details to log.

Do not implement combat/outcome resolution.

### 8. Product smoke scenario

Add scenario:

```text
generated-map-placement-preview
```

It should:
1. assemble expanded approved artifacts;
2. activate assembled generated package if needed or use package directly;
3. start runtime;
4. build placement markers;
5. assert NPC marker count equals generated NPC count;
6. assert encounter marker count equals generated encounter count;
7. assert markers have map id and valid positions;
8. assert positions are deterministic across two builds;
9. assert movement still works;
10. assert Browser/interaction catalog still has NPCs/encounters;
11. assert no LLM/provider dependency.

### 9. Script support

Extend `.devflow/scripts/run-product-smoke.ps1` with:

```powershell
-Scenario generated-map-placement-preview
```

Existing scenarios must remain working.

### 10. Docs/state

Update:
- `docs/PRODUCT_SMOKE_SCENARIOS.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `.devflow/CURRENT_RUN.md`

Do not mark Unity complete.

## Tests

Required:
1. marker placement creates NPC markers from generatedContent.npcs.
2. marker placement creates encounter markers from generatedContent.encounters.
3. scene id resolves to PackageMapId.
4. missing references do not crash and produce fallback marker.
5. marker positions are deterministic.
6. marker positions avoid player current position if possible.
7. RuntimeMapCanvas/player rendering remains compatible.
8. generated-map-placement-preview smoke passes.
9. existing product smoke scenarios pass.

## Focused commands

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~MapPlacement"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
```

## Required checks

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-runtime-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario expanded-contract-batch-smoke
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-content-interaction-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario active-package-quest-dialogue-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-map-placement-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

```text
1. Generate/apply full_small_rpg_seed.
2. Artifact Review -> Save decisions + apply.
3. Use assembled package as current.
4. Runtime Preview -> Start.
5. Confirm NPC/encounter markers appear on the map.
6. Confirm player marker still appears.
7. Select NPC in Browser and compare refs with marker details/log.
8. Preview dialogue still works.
9. Select Encounter in Browser.
10. Append/preview encounter to log if implemented.
11. Move player.
12. Confirm markers remain stable and movement still works.
```

## Stop conditions

Stop and report if:
- package schema changes become necessary;
- DefaultGameRuntime rewrite becomes necessary;
- `.sln` or `.csproj` changes are required;
- Unity/Lua/effect execution is needed;
- LLM/provider is needed;
- WinForms Designer becomes invalid;
- check-all fails after 2 repair attempts;
- more than 18 files need changes.

## Final report

Russian report with:
- files read;
- files changed;
- marker placement strategy;
- map id resolution strategy;
- marker position strategy;
- RuntimeMapCanvas rendering change;
- interaction/log behavior;
- smoke scenario results;
- check-all/check-devflow results;
- manual verification status;
- remaining gaps and recommended next slice.
