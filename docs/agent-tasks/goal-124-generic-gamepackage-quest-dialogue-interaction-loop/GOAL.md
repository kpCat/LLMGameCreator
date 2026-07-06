# Goal 124 — Generic GamePackage Quest/Dialogue/Interaction Loop Projection

## Task ID

`goal-124-generic-gamepackage-quest-dialogue-interaction-loop`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive product goal. The primary deliverable is a projection-only generic GamePackage loop over `samples/minimal-map-game/package.json` that demonstrates package gameplay semantics without touching Runtime, public schema, providers, Lua, generator-library, scenes, prefabs, ProjectSettings, Packages, or StreamingAssets.

## Why this goal exists

Goal123 moved the Unity projection away from geoworld-only assumptions and added a read-only visual projection of `samples/minimal-map-game/package.json`.

Goal124 must move from static package projection to a projection-local gameplay loop:

- inspect an interactable object;
- show an interaction effect/log/flag;
- show dialogue data;
- show quest objective status from inventory;
- show inventory/resource summaries;
- show a single one-click verification path.

This is still Editor projection only. It must not become Runtime or schema work.

## Read first

```text
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
.devflow/artifact-scope/artifact-scope-policy.json

samples/minimal-map-game/package.json

.llmgc/procedural/goal-123-generic-gamepackage-playable-projection-adapter/generic-gamepackage-projection-dashboard.json
.llmgc/procedural/goal-122-accepted-alpha-projection-action-loop-and-window-polish/accepted-alpha-projection-action-loop-dashboard.json
.llmgc/procedural/goal-120a-clean-unity-editor-noise-empty-status-hotfix/clean-unity-editor-noise-empty-status-hotfix-dashboard.json

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionAdapter.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-124-generic-gamepackage-quest-dialogue-interaction-loop/**
.devflow/artifact-scope/artifact-scope-policy.json

.llmgc/procedural/goal-124-generic-gamepackage-quest-dialogue-interaction-loop/**
.llmgc/exports/goal-124-generic-gamepackage-quest-dialogue-interaction-loop/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/generic-gamepackage-quest-dialogue-interaction-loop.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal124.cs

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionAdapter.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionState.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionLoop.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage, or commit:

```text
.llmgc/manual/**
samples/minimal-map-game/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
unity/LLMGameCreatorAlpha/Assets/Scenes/**
unity/LLMGameCreatorAlpha/Assets/**/*.unity
unity/LLMGameCreatorAlpha/Assets/**/*.prefab
unity/LLMGameCreatorAlpha/ProjectSettings/**
unity/LLMGameCreatorAlpha/Packages/**
unity/LLMGameCreatorAlpha/Assets/StreamingAssets/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
provider / LLM / RAG / media provider code
public GamePackage schema files
Lua / Scripting code
generator-library/**
*.sln
*.csproj
Directory.Build.*
dependency/package files
```

No Runtime/schema/provider/Lua/generator-library work. Do not mutate the sample package. Do not save scenes/prefabs. Do not write StreamingAssets.

## Primary deliverable A — Generic package loop state

Add projection-only Unity state classes, for example:

```text
GenericGamePackageProjectionState
GenericGamePackageProjectionLoop
```

They must track:

```text
selectedEntityId
selectedInteractionId
selectedDialogueId
selectedQuestId
inventorySummary
resourceSummary
questObjectiveSummary
interactionEffectPreview
projectionEventLog
appliedInteractionCount
startedQuestCount
```

State must be in-memory only. No file writes.

## Primary deliverable B — One-click generic gameplay loop verification

Extend the Unity window with one prominent button or clearly placed action:

```text
Run Generic Package Gameplay Loop Verification
```

It should perform a deterministic projection-local sequence over `samples/minimal-map-game/package.json`:

1. Load sample package.
2. Build generic projection.
3. Select the sign entity (`entity/village/sign`) or first inspectable interaction target.
4. Preview `interaction/sign_inspect`.
5. Apply the preview interaction to projection state:
   - set flag-like projection state;
   - append effect/log preview;
   - increment applied interaction count.
6. Select old guard entity (`entity/village/old_guard`) and show `dialogue/old_guard_intro` summary.
7. Show quest `quest/help_healer` objective summary:
   - required `item/red_herb` amount 3;
   - inventory has 2 from `inventory/player_start`;
   - status incomplete.
8. Show inventory/resource summary.
9. Update scene diagnostics/state marker.
10. Produce a readable event log.

The button must not require the user to click every debug button.

## Primary deliverable C — Visible projection panels/markers

Add/update visible projection markers/text for:

```text
generic package loop status
selected entity
interaction preview/applied effect
dialogue summary
quest objective status
inventory summary
resource summary
event log summary
```

These can be TextMesh objects under the generated projection root.

## Primary deliverable D — batchmode smoke

Add:

```text
LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageLoopSmoke
```

It must log:

```text
GOAL124_GENERIC_GAMEPACKAGE_LOOP_PASS
```

or:

```text
GOAL124_GENERIC_GAMEPACKAGE_LOOP_FAIL
```

The smoke must require at least:

```text
genericLoopPassed=True
samplePackageLoaded=True
genericProjectionBuilt=True
interactionPreviewPresent=True
interactionApplyPassed=True
dialogueSummaryPresent=True
questObjectiveSummaryPresent=True
inventorySummaryPresent=True
resourceSummaryPresent=True
eventLogPresent=True
zeroFatalErrors=True
```

## Application evidence seam

Add/update BCL-only evidence under:

```text
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/
```

It must verify:

- Goal123 remains green.
- Generic loop source markers exist.
- Unity batchmode log contains `GOAL124_GENERIC_GAMEPACKAGE_LOOP_PASS`.
- Sample package is read-only.
- No forbidden paths are expected.
- Cleanup script remains available.

## Required artifacts

Create deterministic artifacts under:

```text
.llmgc/procedural/goal-124-generic-gamepackage-quest-dialogue-interaction-loop/
.llmgc/exports/goal-124-generic-gamepackage-quest-dialogue-interaction-loop/
```

Recommended files:

```text
generic-gamepackage-loop-dashboard.json
generic-gamepackage-loop-script-inventory.json
generic-gamepackage-loop-smoke-plan.json
generic-gamepackage-loop-log-scan.json
generic-gamepackage-loop-report.md
generic-gamepackage-loop-negative-proof.json
generic-gamepackage-loop-file-index.json
unity-batchmode-generic-gamepackage-loop.log
```

## Visual World Stream Preview Workspace

Add a read-only Goal124 section showing:

```text
genericLoopStatus
samplePackagePath
packageId
mapId
interactionPreviewPresent
interactionApplyPassed
dialogueSummaryPresent
questObjectiveSummaryPresent
inventorySummaryPresent
resourceSummaryPresent
unitySmokeStatus
cleanupScriptAvailable
projectionOnly
evidencePath
exportPath
```

## Docs/current state

Update current-state/queue docs so they clearly say:

- Goal124 adds projection-local quest/dialogue/interaction loop over the generic sample GamePackage.
- It still does not authorize Runtime/provider/schema/Lua/generator-library/final-art/atlas/Unity scene/prefab/project-settings/StreamingAssets/release work.
- Manual verification remains one main button for the loop.
- After manual Unity checks, use `.devflow\scripts\clean-unity-editor-noise.cmd`.

## Artifact-scope policy

Add scenario:

```text
goal-124-generic-gamepackage-quest-dialogue-interaction-loop
```

It must allow only Goal124 expected files and exclude `.llmgc/manual/**`, samples/minimal-map-game, Unity scenes/prefabs/settings/Packages/StreamingAssets, Runtime/schema/provider/Lua/generator-library.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~Goal124|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-124-generic-gamepackage-quest-dialogue-interaction-loop
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Run Unity batchmode smoke:

```powershell
Unity.exe -batchmode -quit -projectPath .\unity\LLMGameCreatorAlpha -executeMethod LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageLoopSmoke -logFile .\.llmgc\procedural\goal-124-generic-gamepackage-quest-dialogue-interaction-loop\unity-batchmode-generic-gamepackage-loop.log
```

After Unity batchmode, run cleanup:

```powershell
.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply
git status --short --untracked-files=all
```

Only stage allowed files. Final status must be clean.

## Quality gate

GREEN requires:

- batchmode log contains `GOAL124_GENERIC_GAMEPACKAGE_LOOP_PASS`;
- generic loop smoke required fields are true;
- sample package remains unmodified;
- no forbidden path changes;
- no `.llmgc/manual/**` staged/tracked;
- tests/checks pass;
- artifact scope passes;
- final git status clean.

BLOCKED if Unity cannot run or generic loop smoke cannot be verified honestly.

FAILED if build/tests break or forbidden changes are required.

## Commit / push policy

Before commit:

```powershell
git diff --cached --name-only
git diff --cached --check
git diff --cached --name-only | Select-String -SimpleMatch ".llmgc/manual"
```

Commit and push with one of:

```text
GREEN Goal 124 generic gamepackage quest dialogue interaction loop
BLOCKED Goal 124 generic gamepackage quest dialogue interaction loop
FAILED Goal 124 generic gamepackage quest dialogue interaction loop
```

Final report must include commit SHA, Unity loop smoke result, manual verification path, cleanup command, changed files grouped by area, final git status, and remaining debt.
