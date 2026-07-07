# Goal 140 — Runtime-backed Unity Player Loop Controls UX Polish + Unity Noise Guard

## Task ID

`goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Strategic intent

Goal139 added a runtime-backed Unity player-loop interactive controls harness. Human review accepted it with explicit follow-up UX debt:

```text
Goal139 accepted by human.
selectedCandidate=minimal-map-game-balanced-baseline
frames=13
interactiveControlsSmoke=GREEN
requiredControlsPresent=true
controlsWork=true
projectionOnly=false
runtimeAuthority=true
unityGameplayTruth=false
AutoStep/AutoPlayAll UX accepted with follow-up debt
```

Goal140 must record that acceptance and close the specific UX debt without drifting back to projection-only work.

This goal must improve the runtime-backed Unity controls surface:

- make frame numbering unambiguous for humans;
- make `Auto Step` / `Auto Play All` semantics explicit and less surprising;
- classify known Unity BuildProfileContext editor noise separately from harness failures;
- keep gameplay truth Runtime-owned.

## Required read-first

Read, in this order:

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md

.llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/runtime-backed-player-loop-interactive-controls-dashboard.json
.llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/runtime-backed-player-loop-interactive-controls-model.json
.llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/runtime-backed-player-loop-interactive-controls-script.json
.llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/unity-player-loop-interactive-controls-smoke.json

docs/manual-acceptance/runtime-backed-unity-player-loop-interactive-controls-harness.md

unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnityPlayerLoopInteractiveControlsWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityPlayerLoopInteractiveControlsHarness.cs
```

## Goal type

Hands-on product UX polish + evidence, not docs-only.

## Required normal command

Add:

```bat
.devflow\scripts\run-runtime-backed-unity-player-loop-controls-ux-polish.cmd
```

The `.cmd` must call the `.ps1` with `-ApplyCleanup` by default.

The `.ps1` must support:

```text
-InteractiveControlsModelPath
-InteractiveControlsResultPath
-InteractiveControlsScriptPath
-OutputRoot
-UnityPath
-DryRun
-ApplyCleanup
```

Defaults:

```text
InteractiveControlsModelPath = .llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/runtime-backed-player-loop-interactive-controls-model.json
InteractiveControlsResultPath = .llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/runtime-backed-player-loop-interactive-controls-result.json
InteractiveControlsScriptPath = .llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/runtime-backed-player-loop-interactive-controls-script.json
OutputRoot = .llmgc/procedural/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard
```

The script must validate repo-local paths, reject `.llmgc/manual/**`, run the Goal140 proof, run Unity batchmode smoke, classify Unity editor noise, apply bounded cleanup when requested, and write compact result artifacts.

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-runtime-backed-unity-player-loop-controls-ux-polish.ps1
.devflow/scripts/run-runtime-backed-unity-player-loop-controls-ux-polish.cmd

.llmgc/procedural/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard/**
.llmgc/exports/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/runtime-backed-unity-player-loop-interactive-controls-harness.md
docs/manual-acceptance/runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/RuntimeBackedUnityPlayerLoopControlsUxPolishModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/RuntimeBackedUnityPlayerLoopControlsUxPolishArtifactService.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineService.cs

src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal140.cs

unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnityPlayerLoopInteractiveControlsWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityPlayerLoopInteractiveControlsHarness.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/RuntimeBackedUnityPlayerLoopControlsUxPolishScriptProof.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal140Tests.cs
tests/LLMGameCreator.Tests/Devflow/RunRuntimeBackedUnityPlayerLoopControlsUxPolishScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage, or commit:

```text
.llmgc/manual/**
samples/minimal-map-game/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Generation/**
src/LLMGameCreator.AssetPipeline/**
src/LLMGameCreator.Scripting/**
provider / LLM / RAG / media provider code
Lua / Scripting code
generator-library/**
unity/LLMGameCreatorAlpha/Assets/Scenes/**
unity/LLMGameCreatorAlpha/Assets/Prefabs/**
unity/LLMGameCreatorAlpha/Assets/StreamingAssets/**
unity/LLMGameCreatorAlpha/ProjectSettings/**
unity/LLMGameCreatorAlpha/Packages/**
*.sln
*.csproj
Directory.Build.*
dependency/package files
```

No Runtime changes. No GamePackage schema changes. No sample mutation. No `.llmgc/manual/**`.

## Deliverable A — record Goal139 human acceptance

Update the existing manual acceptance note and write compact acceptance artifacts:

```text
.llmgc/procedural/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard/goal139-human-acceptance-record.json
.llmgc/exports/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard/goal139-human-acceptance-record.json
```

Required fields:

```text
goalId=goal_139_runtime_backed_unity_player_loop_interactive_controls_harness
accepted=true
acceptedByHuman=true
acceptedByCodex=false
selectedCandidate=minimal-map-game-balanced-baseline
frames=13
interactiveControlsSmoke=GREEN
requiredControlsPresent=true
controlsWork=true
projectionOnly=false
runtimeAuthority=true
unityGameplayTruth=false
autoStepAutoPlayAllUxAcceptedWithFollowUpDebt=true
rawManualInputNotCommitted=true
```

Goal140 itself remains `accepted=false`.

## Deliverable B — controls UX polish

Update the Unity interactive controls window/harness so the manual confusion from Goal139 is addressed.

Required UX improvements:

1. Display frame index as human-readable `Current Frame: 1/13`, not only zero-based `0` or `12`.
2. Display raw zero-based frame index separately if useful, e.g. `Frame Index: 0`.
3. Make single-step and auto-play semantics explicit:
   - rename or relabel `Auto Step` to `Step Once` OR clearly show `Auto Step = one frame tick` in the UI;
   - rename or relabel `Auto Play All` to `Play All To End` OR clearly show it is instant-to-final-frame behavior;
   - include `Last Control Action` text that distinguishes `step_once`, `play_all_to_end`, `next`, `previous`, etc.
4. Add `Reset/First` equivalent clearly visible; `First` may stay if already present, but status must show it resets to first frame.
5. `Copy Frame Summary` must show a visible status after copying, e.g. `copied_frame_summary`.
6. Keep the top-of-window authority labels:

```text
Gameplay truth: Runtime
Unity mode: PlayerAdapter/HUD controls only
```

Do not add live gameplay execution in Unity. Unity remains a PlayerAdapter/HUD consumer only.

## Deliverable C — Unity editor noise guard

The user observed Unity Console errors that look like Unity BuildProfileContext editor noise:

```text
BuildProfileContext asset exists but could not be loaded
NullReferenceException: Object reference not set to an instance of an object
UnityEditor.Build.Profile.BuildProfileContext.CreateOrLoad
```

Goal140 must not blindly ignore all `NullReferenceException`.

Add a bounded classifier in Goal140 evidence/runner:

- classify the known BuildProfileContext/CreateOrLoad pattern as `knownUnityEditorBuildProfileNoise` when paired with `BuildProfileContext` / `CreateOrLoad` markers;
- classify harness/player-loop exceptions as blocking;
- classify any unpaired `NullReferenceException` as blocking unless explicitly matched to known editor noise;
- write a compact `unity-editor-noise-classification.json` artifact;
- include `knownUnityEditorNoiseCount`, `blockingUnityErrorCount`, `unclassifiedUnityErrorCount` in the dashboard/report.

GREEN requires:

```text
blockingUnityErrorCount=0
unclassifiedUnityErrorCount=0
knownUnityEditorBuildProfileNoiseClassified=true
```

If the current Unity log does not contain the noise, the classifier still must be present and tested with fixture strings.

## Deliverable D — compact artifacts

Under both procedural and export roots, write:

```text
goal139-human-acceptance-record.json
runtime-backed-player-loop-controls-ux-dashboard.json
runtime-backed-player-loop-controls-ux-result.json
runtime-backed-player-loop-controls-ux-model.json
runtime-backed-player-loop-controls-ux-script.json
unity-player-loop-controls-ux-smoke.json
unity-editor-noise-classification.json
one-click-runtime-backed-player-loop-controls-ux-report.json
one-click-runtime-backed-player-loop-controls-ux-report.md
runtime-backed-player-loop-controls-ux-negative-proof.json
runtime-backed-player-loop-controls-ux-file-index.json
```

Dashboard required fields:

```text
goalId=goal_140_runtime_backed_unity_player_loop_controls_ux_polish_and_noise_guard
status=GREEN
accepted=false
acceptedGoal139=true
selectedCandidate=minimal-map-game-balanced-baseline
frameCount=13
humanReadableFrameNumbering=true
stepOnceSemanticsClear=true
playAllToEndSemanticsClear=true
copyFrameSummaryStatusPresent=true
requiredControlsPresent=true
controlsUxPolished=true
unityControlsUxSmokePassed=true
runtimeAuthority=true
unityGameplayTruth=false
projectionOnly=false
knownUnityEditorNoiseClassified=true
blockingUnityErrorCount=0
unclassifiedUnityErrorCount=0
manualUnityOptional=true
```

## Deliverable E — Unity batchmode smoke

Add/update batchmode method in the Unity harness to validate:

```text
GOAL140_RUNTIME_BACKED_UNITY_PLAYER_LOOP_CONTROLS_UX_PASS
GOAL140_RUNTIME_BACKED_UNITY_PLAYER_LOOP_CONTROLS_UX_FAIL
```

Smoke must verify:

```text
modelPathExists=true
frameCountPassed=true
requiredControlsPresent=true
humanReadableFrameNumberingPresent=true
stepOnceSemanticsClear=true
playAllToEndSemanticsClear=true
copyFrameSummaryStatusPresent=true
runtimeAuthorityMarkersPresent=true
unityGameplayTruth=false
```

## VisualWorld / WinForms surface

Add a read-only Goal140 section showing:

```text
acceptedGoal139
selectedCandidateId
frameCount
humanReadableFrameNumbering
stepOnceSemanticsClear
playAllToEndSemanticsClear
knownUnityEditorNoiseClassified
blockingUnityErrorCount
unclassifiedUnityErrorCount
unityControlsUxSmokePassed
runtimeAuthority
unityGameplayTruth
projectionOnly
normalCommand
reportPath
```

## Docs/current state

Update current state and queue to reflect:

```text
goal139Accepted=true
goal140Accepted=false
runtimeBackedUnityControlsUxPolish=true
knownUnityEditorNoiseClassified=true
projectionOnly=false
runtimeAuthority=true
unityGameplayTruth=false
nextProductGoal=review_goal_140_runtime_backed_unity_player_loop_controls_ux_polish_and_noise_guard
```

## Artifact-scope policy

Add scenario:

```text
goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard
```

It must allow only expected Goal140 paths and exclude all forbidden zones.

## Validation

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~Goal140|FullyQualifiedName~RuntimeBackedUnityPlayerLoopControlsUxPolish|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\run-runtime-backed-unity-player-loop-controls-ux-polish.ps1 -DryRun
.\.devflow\scripts\run-runtime-backed-unity-player-loop-controls-ux-polish.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Forbidden path diff check:

```powershell
git diff --name-only -- samples/minimal-map-game src/LLMGameCreator.Runtime src/LLMGameCreator.Runtime.Abstractions src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity/LLMGameCreatorAlpha/Assets/Scenes unity/LLMGameCreatorAlpha/Assets/Prefabs unity/LLMGameCreatorAlpha/Assets/StreamingAssets unity/LLMGameCreatorAlpha/ProjectSettings unity/LLMGameCreatorAlpha/Packages
```

## Quality gate

GREEN requires:

- Goal139 human acceptance recorded;
- Goal140 accepted=false;
- human-readable frame numbering present;
- Step Once / Play All To End semantics clear;
- Copy Frame Summary shows status;
- known Unity BuildProfileContext noise classifier exists and is tested;
- no blocking/unclassified Unity errors in Goal140 proof;
- Unity batchmode smoke passes;
- runtimeAuthority=true;
- projectionOnly=false;
- unityGameplayTruth=false;
- tests/checks pass;
- artifact scope passes;
- no forbidden path changes;
- final git status clean.

BLOCKED if Unity cannot batchmode-run the controls UX smoke.

FAILED if build/tests break or forbidden paths must be changed.

## Commit / push policy

Commit and push with one of:

```text
GREEN Goal 140 runtime-backed Unity player loop controls UX polish and noise guard
BLOCKED Goal 140 runtime-backed Unity player loop controls UX polish and noise guard
FAILED Goal 140 runtime-backed Unity player loop controls UX polish and noise guard
```

Final report must include:

- commit SHA;
- Goal139 acceptance status;
- frame count;
- UX semantics status;
- Unity noise classification counts;
- Unity smoke status;
- one-click report path;
- forbidden-zone confirmation;
- final git status.
