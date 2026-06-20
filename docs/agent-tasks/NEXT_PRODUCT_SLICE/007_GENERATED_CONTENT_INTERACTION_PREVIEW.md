# Product Slice 007 Task: Generated Content Interaction Preview

## Task type

Large product UI/application slice, bounded to Runtime Preview generated-content interactions.

## Goal

Add an interactive generated content browser to Runtime Preview.

The current preview shows generated content as text. This slice should make it selectable and inspectable:

```text
category list
-> generated entry list
-> details/references
-> simple preview actions/log
```

## Recommended Codex reasoning level

High.

Do not use Max/Ultra on first attempt.
Do not use Medium because this touches UI, generatedContent projection, product smoke and tests.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/PRODUCT_SLICE_007_GENERATED_CONTENT_INTERACTION_PREVIEW.md
src/LLMGameCreator.Application/RuntimePreview/GeneratedPackageRuntimePreviewService.cs
src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.cs
src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.Designer.cs
src/LLMGameCreator.Runtime/DefaultGameRuntime.cs
src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
tests/LLMGameCreator.Tests/ProductSmoke/ExpandedContractBatchSmokeTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/GeneratedPackageRuntimePreviewSmokeTests.cs
.devflow/scripts/run-product-smoke.ps1
```

Then search narrowly for RuntimePreview tests and current product smoke fixture helpers.

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
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactContractCatalog.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactValidator.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs
src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
LLMGameCreator.sln
*.csproj
.devflow/NEXT_TASK.md
.devflow/task-queue.json
```

Do not add NuGet packages.
Do not call LLM/provider.
Do not execute generated effects.

## Required behavior

### 1. Add interaction/catalog model

Add a small read-only interaction model over `GeneratedPackageRuntimePreviewModel`.

Possible names:

```text
GeneratedContentInteractionPreviewService
GeneratedContentInteractionCatalog
GeneratedContentInteractionCategory
GeneratedContentInteractionEntry
GeneratedContentInteractionDetails
```

It should support categories:

```text
current_scene
regions
npcs
items
dialogues
quests
mechanics
encounters
applied_artifacts
warnings
```

Each entry should have:

```text
category id
entry id
title
subtitle/description
reference ids
details text
```

### 2. Selection details

Selecting an entry should produce readable details.

Minimum details:

- Scene: source id, package map id, title, description, purpose.
- Region: id, title, description, scene refs.
- NPC: id, name, description, region/scene refs.
- Item: id, name, description.
- Dialogue: id, title, description, NPC/scene refs, lines.
- Quest: id, title, description, steps/objectives.
- Mechanic: id, name, description, tags.
- Encounter: id, title, description, region/scene refs, NPC refs.
- Applied artifact: contract, artifact id, mapping, content hash.
- Warning: warning text.

### 3. Runtime Preview UI

Update Generated Content tab.

Preferred layout:

```text
Generated Content tab:
  SplitContainer or TableLayout
    left: ComboBox category + ListBox entries
    right: read-only multiline details TextBox
    bottom/top: buttons
```

Buttons:
- `Inspect selected` or auto-update details on selection;
- `Append to log`;
- optional `Focus linked scene`.

Keep existing raw/generated summary available if simple:
- either as a separate tab `Summary`;
- or as details fallback.

Do not break existing Log tab.

### 4. Actions

Minimum required action:

```text
Append selected to log
```

When used, it appends a readable message to Runtime Preview log.

Optional safe action:

```text
Focus linked scene
```

This should not rewrite runtime engine. If selected entry references a scene/map, it may just show details/log focus. Do not implement real map travel unless already trivial and safe.

### 5. Refresh behavior

After:
- Start;
- command execution;
- package/runtime reload;

the generated interaction catalog should refresh and keep a valid selection when possible.

### 6. Runtime behavior preserved

Existing start/move/interact command flow must remain intact.

Do not replace `DefaultGameRuntime`.

### 7. Product smoke scenario

Add or extend product smoke scenario:

```text
generated-content-interaction-preview
```

It should:
1. assemble expanded fixture package;
2. start runtime;
3. build generated preview projection;
4. build interaction catalog;
5. assert all expected categories exist;
6. select at least one region, NPC, item, dialogue, quest, mechanic, encounter;
7. assert details text is non-empty;
8. assert dialogue details include lines;
9. execute a movement command and assert runtime still works;
10. assert no LLM/provider dependency.

### 8. Script support

Extend `.devflow/scripts/run-product-smoke.ps1` with:

```powershell
-Scenario generated-content-interaction-preview
```

### 9. Docs/state

Update:
- `docs/PRODUCT_SMOKE_SCENARIOS.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `.devflow/CURRENT_RUN.md`

Do not mark Unity complete.

## Tests

Required:

1. interaction catalog contains categories for scenes/regions/npcs/items/dialogues/quests/mechanics/encounters.
2. selecting NPC returns details with region/scene refs.
3. selecting dialogue returns details with lines.
4. selecting quest returns details with steps/objectives.
5. selecting encounter returns details with refs.
6. selecting applied artifact returns contract/hash/mapping.
7. product smoke `generated-content-interaction-preview` passes.
8. existing product smoke scenarios still pass.

## Focused commands

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~InteractionPreview"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
```

## Required checks

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-runtime-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario expanded-contract-batch-smoke
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-content-interaction-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

Manual UI verification is expected.

Steps:

```text
1. Open app.
2. Open Ash Beacon / generated test project.
3. Runtime Preview.
4. Start.
5. Open Generated Content tab.
6. Select categories one by one:
   Current scene
   Regions
   NPCs
   Items
   Dialogues
   Quests
   Mechanics
   Encounters
   Applied artifacts
7. Select entries and confirm details appear.
8. Click Append selected to log.
9. Move player once.
10. Confirm log and generated content browser still work.
```

## Stop conditions

Stop and report if:
- `.sln` or `.csproj` changes are required;
- Runtime engine rewrite becomes necessary;
- Unity/Lua/effect execution becomes necessary;
- LLM/provider is needed;
- WinForms Designer becomes invalid;
- check-all fails after 2 repair attempts;
- more than 18 files need changes.

## Final report

Russian report with:
- files read;
- files changed;
- interaction catalog behavior;
- Runtime Preview UI behavior;
- actions added;
- smoke scenario;
- checks;
- manual verification status;
- remaining gaps and recommended next slice.
