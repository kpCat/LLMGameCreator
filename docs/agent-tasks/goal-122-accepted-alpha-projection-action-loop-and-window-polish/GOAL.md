# Goal 122 — Accepted Alpha Projection Action Loop + Window Polish

## Task ID

`goal-122-accepted-alpha-projection-action-loop-and-window-polish`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive product goal.

This is not another proof-only/review goal. The primary deliverable is a more usable hands-on Unity projection with a projection-local interaction/action loop and a cleaned-up EditorWindow layout.

## User feedback driving this goal

Manual Goal121 verification worked, but the `Accepted Alpha Projection` window became hard to read:

- large text areas dominate the window;
- controls are stacked awkwardly at the bottom;
- status/result is difficult to interpret at a glance;
- manual checking should not require clicking every debug button after each goal.

Goal122 must improve the window as part of the main product task.

## What this must give hands-on

The user should open Unity and use one main path:

```text
LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection
Run Full Projection Verification
```

Then the window should be readable and support a projection-local action preview loop:

```text
Select Next Interaction Target
Preview Selected Action
Apply Preview Action To Projection State
Reset Projection State
```

This state is projection-only Editor state. It must not modify Runtime, GamePackage schema, StreamingAssets, scenes or prefabs.

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

.llmgc/procedural/goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification/accepted-alpha-interaction-drilldown-dashboard.json
.llmgc/procedural/goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification/accepted-alpha-interaction-drilldown-log-scan.json
.llmgc/procedural/goal-120a-clean-unity-editor-noise-empty-status-hotfix/clean-unity-editor-noise-empty-status-hotfix-dashboard.json
.llmgc/procedural/goal-120-accepted-alpha-projection-usability-and-cleanup/accepted-alpha-projection-usability-dashboard.json

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionActionPreview.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionDrilldown.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-122-accepted-alpha-projection-action-loop-and-window-polish/**
.devflow/artifact-scope/artifact-scope-policy.json

.llmgc/procedural/goal-122-accepted-alpha-projection-action-loop-and-window-polish/**
.llmgc/exports/goal-122-accepted-alpha-projection-action-loop-and-window-polish/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/accepted-alpha-projection-action-loop-and-window-polish.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal122.cs

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionController.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionActionPreview.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionDrilldown.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionModels.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionPrimitiveFactory.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AcceptedAlphaPlayableProjectionState.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage, or commit:

```text
.llmgc/manual/**
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
LFZ source/archive
```

No live geodata, no providers, no Runtime/schema/Lua/generator-library, no final art/atlas, no saved scene/prefab assets, no ProjectSettings/Packages/StreamingAssets writes, and no release packaging.

## Primary deliverable A — Window polish

Refactor `AcceptedAlphaPlayableProjectionWindow` so it is usable at typical editor sizes.

Required:

1. A compact top status area showing status line, baseline loaded/accepted, full verification pass/fail, selected marker id/kind and projection state status.
2. `Run Full Projection Verification` remains the prominent first/main action.
3. Debug buttons are visually separated from the main verification path.
4. Text panels do not dominate the entire window by default.
5. Use compact/collapsible or bounded-height sections for Smoke, Selected Marker Details, Interaction Preview, Objective / Replay Details and Verification Event Log.
6. Add a small manual check path hint: `Run Full Projection Verification` then `.devflow\scripts\clean-unity-editor-noise.cmd`.
7. Do not save layout/assets/scenes.

Keep existing debug controls, but the normal user path should be one primary button plus optional inspection.

## Primary deliverable B — Projection-local action loop

Add a projection-only state model, for example:

```text
AcceptedAlphaPlayableProjectionState
```

It must live under allowed Unity scripts only and must not touch Runtime/GamePackage schema.

Required behavior:

1. Track selected interaction target id.
2. Track selected/preview action summary.
3. Track projection-local applied action count.
4. Track a projection-local event log.
5. Support `Preview Selected Action`, `Apply Preview Action To Projection State`, and `Reset Projection State`.
6. Applying action must update the event log, update a visible diagnostics/state marker or text, update interaction preview text, write no files, mutate no StreamingAssets, save no scene/prefab, and call no Runtime/schema/providers/LLM/network.

The action can be a deterministic preview derived from Goal105 action/delta JSON. It does not need to be a real runtime transaction yet.

## Primary deliverable C — One-click verification includes action loop

Extend full verification so it verifies:

```text
fullVerificationPassed=True
selectedMarkerDetailsPresent=True
interactionPreviewPresent=True
objectiveReplayDetailsPresent=True
verificationEventLogPresent=True
projectionActionPreviewPresent=True
projectionActionApplyPassed=True
projectionStateResetPassed=True
windowLayoutPolishPresent=True
materialWarningGuardPresent=True
```

Add batchmode method:

```text
LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeProjectionActionLoopSmoke
```

It must log `GOAL122_ACTION_LOOP_SMOKE_PASS` or `GOAL122_ACTION_LOOP_SMOKE_FAIL`.

## Application evidence seam

Add/update BCL-only evidence under:

```text
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/
```

It must verify Goal121 remains green, Goal122 window polish markers exist, action loop markers/methods exist, batchmode action loop smoke log contains pass marker, material warning markers are absent, cleanup script remains available, and no forbidden path is expected.

## Required artifacts

Create deterministic artifacts under:

```text
.llmgc/procedural/goal-122-accepted-alpha-projection-action-loop-and-window-polish/
.llmgc/exports/goal-122-accepted-alpha-projection-action-loop-and-window-polish/
```

Recommended files:

```text
accepted-alpha-projection-action-loop-dashboard.json
accepted-alpha-projection-action-loop-script-inventory.json
accepted-alpha-projection-action-loop-smoke-plan.json
accepted-alpha-projection-action-loop-log-scan.json
accepted-alpha-projection-action-loop-report.md
accepted-alpha-projection-action-loop-negative-proof.json
accepted-alpha-projection-action-loop-file-index.json
unity-batchmode-action-loop-smoke.log
```

## Visual World Stream Preview Workspace

Add a read-only Goal122 section showing:

```text
actionLoopStatus
windowPolishStatus
unityMenuPath
oneClickVerificationStillPresent
projectionActionPreviewPresent
projectionActionApplyPresent
projectionStateResetPresent
windowLayoutPolishPresent
unitySmokeStatus
cleanupScriptAvailable
doNotStartAutomatically
evidencePath
exportPath
```

## Docs/current state

Update current-state and queue docs so they clearly say:

- Goal122 adds projection-local action loop and cleans up the Unity EditorWindow layout.
- It remains projection-only and does not authorize Runtime/provider/schema/Lua/generator-library/final-art/atlas/Unity scene/prefab/project-settings/StreamingAssets/release work.
- Manual checking should still use the one main verification path, not every debug button.
- After manual Unity checks, use `.devflow\scripts\clean-unity-editor-noise.cmd`.

## Artifact-scope policy

Add scenario:

```text
goal-122-accepted-alpha-projection-action-loop-and-window-polish
```

It must allow only Goal122 expected files and exclude `.llmgc/manual/**`, Unity scenes/prefabs/settings/Packages/StreamingAssets, Runtime/schema/provider/Lua/generator-library.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~Goal122|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-122-accepted-alpha-projection-action-loop-and-window-polish
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Run Unity batchmode smoke:

```powershell
Unity.exe -batchmode -quit -projectPath .\unity\LLMGameCreatorAlpha -executeMethod LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeProjectionActionLoopSmoke -logFile .\.llmgc\procedural\goal-122-accepted-alpha-projection-action-loop-and-window-polish\unity-batchmode-action-loop-smoke.log
```

If `Unity.exe` is not on PATH, use installed Unity 6000.1.10f1 if available.

After Unity batchmode, run cleanup:

```powershell
.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply
git status --short --untracked-files=all
```

Only stage allowed files. Final status must be clean after push.

## Quality gate

GREEN requires:

- batchmode log contains `GOAL122_ACTION_LOOP_SMOKE_PASS`;
- material warning markers absent;
- one-click verification remains present;
- action preview/apply/reset are present;
- window polish markers are present;
- cleanup script still works;
- no forbidden path changes;
- no `.llmgc/manual/**` staged/tracked;
- tests/checks pass;
- artifact scope passes;
- final git status clean.

BLOCKED if Unity cannot run or action-loop smoke cannot be verified honestly.

FAILED if build/tests break or forbidden changes are required.

## Commit / push policy

Before commit:

```powershell
git diff --cached --name-only
git diff --cached --check
git diff --cached --name-only | Select-String -SimpleMatch ".llmgc/manual"
```

The last command must produce no matches.

Commit and push with one of:

```text
GREEN Goal 122 accepted alpha projection action loop and window polish
BLOCKED Goal 122 accepted alpha projection action loop and window polish
FAILED Goal 122 accepted alpha projection action loop and window polish
```

Final report must include commit SHA, Unity action-loop smoke result, manual verification path, cleanup command, exact changed files grouped by area, final git status, and remaining debt.
