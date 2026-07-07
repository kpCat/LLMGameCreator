# Goal 139 — Runtime-backed Unity Player Loop Interactive Controls Harness

## Task ID

`goal-139-runtime-backed-unity-player-loop-interactive-controls-harness`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Strategic intent

Goal138 was manually accepted by the repository owner after opening the Unity Editor window `LLMGameCreator/Accepted Alpha/Runtime Player Loop Stepper`, verifying that the runtime-backed stepper model loads, candidate is `minimal-map-game-balanced-baseline`, frame count is `13`, and `Previous` / `Next` switch HUD frames.

Goal139 must record that Goal138 human acceptance and add the next product layer: interactive player-loop controls over the runtime-backed model. Unity remains a player/presentation adapter; gameplay truth remains Runtime-owned snapshots/playback frames.

This is not a projection-only goal.

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
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

.llmgc/procedural/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness/runtime-backed-player-loop-stepper-model.json
.llmgc/procedural/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness/runtime-backed-player-loop-stepper-result.json
.llmgc/procedural/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness/unity-player-loop-stepper-smoke.json
.llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness/canonical-runtime-unity-player-loop-playback-frames.json
.llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-snapshots.json
.llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-player-adapter-contract.json

unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnityPlayerLoopStepperWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityPlayerLoopStepperHarness.cs
```

## Goal boundary

You may extend the Unity Editor/player harness and Application proof surfaces. Do not change Runtime, Runtime.Abstractions, public GamePackage schema, providers, Lua, generator-library, or sample package.

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/**
.devflow/artifact-scope/artifact-scope-policy.json

.devflow/scripts/run-runtime-backed-unity-player-loop-interactive-controls.ps1
.devflow/scripts/run-runtime-backed-unity-player-loop-interactive-controls.cmd

.llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/**
.llmgc/exports/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/runtime-backed-unity-player-loop-stepper-hud-harness.md
docs/manual-acceptance/runtime-backed-unity-player-loop-interactive-controls-harness.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/RuntimeBackedUnityPlayerLoopInteractiveControlsModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/RuntimeBackedUnityPlayerLoopInteractiveControlsArtifactService.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineService.cs

src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal139.cs

unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnityPlayerLoopInteractiveControlsWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityPlayerLoopInteractiveControlsHarness.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/RuntimeBackedUnityPlayerLoopInteractiveControlsScriptProof.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal139Tests.cs
tests/LLMGameCreator.Tests/Devflow/RunRuntimeBackedUnityPlayerLoopInteractiveControlsScriptTests.cs
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

Do not commit the manual screenshots. Record only a compact acceptance summary.

## Required manual acceptance recording

Record the owner's Goal138 acceptance in compact artifacts and docs:

```text
accepted=true
acceptedByHuman=true
acceptedByCodex=false
selectedCandidate=minimal-map-game-balanced-baseline
stepperFrames=13
stepperBatchSmoke=GREEN
projectionOnly=false
runtimeAuthority=true
unityGameplayTruth=false
rawManualInputNotCommitted=true
```

Update current state so Goal138 is accepted by human and Goal139 remains `accepted=false`.

## Required normal command

Add:

```bat
.devflow\scripts\run-runtime-backed-unity-player-loop-interactive-controls.cmd
```

The `.cmd` should call the `.ps1` with `-ApplyCleanup` by default.

The PowerShell script should support:

```text
-StepperModelPath
-StepperResultPath
-PlaybackFramesPath
-CommandLoopSnapshotsPath
-PlayerAdapterContractPath
-OutputRoot
-UnityPath
-DryRun
-ApplyCleanup
```

Defaults:

```text
StepperModelPath = .llmgc/procedural/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness/runtime-backed-player-loop-stepper-model.json
StepperResultPath = .llmgc/procedural/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness/runtime-backed-player-loop-stepper-result.json
PlaybackFramesPath = .llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness/canonical-runtime-unity-player-loop-playback-frames.json
CommandLoopSnapshotsPath = .llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-snapshots.json
PlayerAdapterContractPath = .llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-player-adapter-contract.json
OutputRoot = .llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness
```

The script must:

1. Validate input paths stay under repo root.
2. Refuse `.llmgc/manual/**`.
3. Refuse writing outside Goal139 output root.
4. Generate an interactive controls model and deterministic control script.
5. Run Unity batchmode smoke.
6. Write compact result/report artifacts.
7. Apply bounded Unity cleanup when requested.
8. Return non-zero on failure.

## Interactive controls model

Build a model over the Goal138 stepper frames. It must include:

```text
candidateId
frameCount=13
currentFrameIndex default 0
runtimeAuthority=true
unityGameplayTruth=false
projectionOnly=false
controls:
  load_model
  first
  previous
  next
  last
  autoplay_tick
  autoplay_all
  copy_current_frame_summary
  show_runtime_hash
  show_hud_lines
```

Generate a deterministic control script that exercises at least:

```text
load_model
assert_frame_count
first
next
next
previous
last
first
autoplay_tick
autoplay_tick
autoplay_all
copy_current_frame_summary
assert_final_frame_reachable
assert_runtime_authority_markers
```

Write a session/result artifact proving the cursor transitions are deterministic.

## Unity Editor window

Add a Unity Editor window:

```text
LLMGameCreator/Accepted Alpha/Runtime Player Loop Controls
```

It must load the Goal139 interactive controls model and expose at least:

```text
Load Goal139 Controls Model
First
Previous
Next
Last
Auto Step
Auto Play All
Copy Frame Summary
```

The window must visibly state:

```text
Gameplay truth: Runtime
Unity mode: PlayerAdapter/HUD controls only
```

The window should show:

```text
candidate id
current frame / total frames
frame category
title
canonical state hash
HUD lines
last control action
```

No Unity gameplay mutation is allowed. Controls move through runtime-backed frames/model only.

## Unity batchmode smoke

Add a batchmode harness with markers:

```text
GOAL139_RUNTIME_BACKED_UNITY_PLAYER_LOOP_INTERACTIVE_CONTROLS_PASS
GOAL139_RUNTIME_BACKED_UNITY_PLAYER_LOOP_INTERACTIVE_CONTROLS_FAIL
```

The smoke must verify:

```text
unityAvailable=true
interactiveModelPathExists=true
controlScriptPathExists=true
frameCountPassed=true
requiredControlsPresent=true
controlScriptPassed=true
runtimeAuthorityMarkersPresent=true
interactiveControlsWindowPresent=true
unityGameplayTruth=false
passed=true
```

## Required artifacts

Under both procedural and export roots:

```text
goal138-human-acceptance-record.json
runtime-backed-player-loop-interactive-controls-model.json
runtime-backed-player-loop-interactive-controls-script.json
runtime-backed-player-loop-interactive-controls-session.json
runtime-backed-player-loop-interactive-controls-result.json
runtime-backed-player-loop-interactive-controls-dashboard.json
runtime-backed-player-loop-interactive-controls-negative-proof.json
runtime-backed-player-loop-interactive-controls-file-index.json
unity-player-loop-interactive-controls-smoke.json
one-click-runtime-backed-player-loop-interactive-controls-report.json
one-click-runtime-backed-player-loop-interactive-controls-report.md
```

Raw Unity `.log` files may remain local/ignored if compact smoke artifacts prove pass/fail.

## VisualWorld / WinForms proof surface

Add read-only Goal139 section showing:

```text
acceptedGoal138
candidateId
frameCount
requiredControlsPresent
controlScriptPassed
interactiveControlsWindowPresent
unityInteractiveControlsSmokePassed
runtimeAuthority
projectionOnly
unityGameplayTruth
normalCommand
reportPath
manualUnityOptional
```

## Docs/current state

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
```

Required markers:

```text
goal138Accepted=true
goal139Accepted=false
runtimeAuthority=true
runtimeBackedUnityInteractiveControls=true
interactiveControlsWindowPresent=true
unityInteractiveControlsSmokePassed=true
projectionOnly=false
unityGameplayTruth=false
manualUnityOptional=true
```

## Artifact-scope policy

Add scenario:

```text
goal-139-runtime-backed-unity-player-loop-interactive-controls-harness
```

It must allow only expected Goal139 paths and exclude forbidden zones.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~Goal139|FullyQualifiedName~RuntimeBackedUnityPlayerLoopInteractiveControls|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\run-runtime-backed-unity-player-loop-interactive-controls.ps1 -DryRun
.\.devflow\scripts\run-runtime-backed-unity-player-loop-interactive-controls.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-139-runtime-backed-unity-player-loop-interactive-controls-harness
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Also verify forbidden diffs:

```powershell
git diff --name-only -- .llmgc/manual samples/minimal-map-game src/LLMGameCreator.Runtime src/LLMGameCreator.Runtime.Abstractions src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity/LLMGameCreatorAlpha/Assets/Scenes unity/LLMGameCreatorAlpha/Assets/Prefabs unity/LLMGameCreatorAlpha/Assets/StreamingAssets unity/LLMGameCreatorAlpha/ProjectSettings unity/LLMGameCreatorAlpha/Packages
```

## Quality gate

GREEN requires:

- Goal138 human acceptance recorded.
- Runtime-backed interactive controls model exists.
- Required controls exist.
- Deterministic control script passes.
- Unity interactive controls smoke passes.
- Unity gameplay truth remains false.
- `projectionOnly=false`.
- Tests/checks pass.
- Artifact scope passes.
- No forbidden path changes.
- No `.llmgc/manual/**` tracked/staged.
- Final git status clean.

BLOCKED if Unity batchmode cannot load the controls model honestly.

FAILED if build/tests break or forbidden changes are required.

## Commit / push policy

Commit and push with one of:

```text
GREEN Goal 139 runtime-backed Unity player loop interactive controls harness
BLOCKED Goal 139 runtime-backed Unity player loop interactive controls harness
FAILED Goal 139 runtime-backed Unity player loop interactive controls harness
```

Push to `origin/main`.

Final report must include:

- commit SHA;
- Goal138 acceptance record status;
- candidate id;
- frame count;
- required controls status;
- Unity smoke status;
- one-click report path;
- forbidden-zone confirmation;
- final git status.
