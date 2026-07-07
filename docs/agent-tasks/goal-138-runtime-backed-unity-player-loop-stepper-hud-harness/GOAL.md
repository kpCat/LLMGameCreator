# Goal 138 — Runtime-backed Unity Player Loop Stepper / HUD Harness

## Task ID

`goal-138-runtime-backed-unity-player-loop-stepper-hud-harness`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Strategic intent

Goal137 was manually accepted by the repository owner with this exact acceptance statement:

```text
Я принимаю Goal137 canonical_runtime_unity_player_loop_playback_harness_verification GREEN. selectedCandidate=minimal-map-game-balanced-baseline, playbackFrames=13, Unity playback smoke GREEN, projectionOnly=false, unityGameplayTruth=false.
```

Goal138 must record that human acceptance and then build the next product step on top of the canonical runtime/player seam:

```text
Goal136 Runtime-owned command-loop snapshots
→ Goal137 Unity/player playback frames
→ Runtime-backed Unity player-loop stepper/HUD harness
→ one-click automated report
```

This is not a projection-only feature. Unity must remain a player/presentation adapter over canonical runtime snapshots/playback frames. Gameplay truth stays in Runtime artifacts.

## Goal type

Product goal with manual acceptance recording.

This goal may update Unity player/presentation scripts and WinForms/VisualWorld read-only surfacing. It must not change Runtime, Runtime.Abstractions, GamePackage schema, providers, Lua, samples or Unity scene/prefab/settings assets.

## Required read-first

Read in this order:

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

.llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness/canonical-runtime-unity-player-loop-playback-dashboard.json
.llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness/canonical-runtime-unity-player-loop-playback-frames.json
.llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness/canonical-runtime-unity-player-loop-playback-result.json
.llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness/unity-player-loop-playback-smoke.json
.llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-snapshots.json
.llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-result.json
.llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-player-adapter-contract.json

unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityPlayerLoopPlaybackAdapter.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimePlayerCommandLoopAdapter.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimePlayerLoopReadinessAdapter.cs
unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-runtime-backed-unity-player-loop-stepper.ps1
.devflow/scripts/run-runtime-backed-unity-player-loop-stepper.cmd

.llmgc/procedural/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness/**
.llmgc/exports/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/canonical-runtime-unity-player-loop-playback-harness.md
docs/manual-acceptance/runtime-backed-unity-player-loop-stepper-hud-harness.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/RuntimeBackedUnityPlayerLoopStepperModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/RuntimeBackedUnityPlayerLoopStepperArtifactService.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineService.cs

src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal138.cs

unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityPlayerLoopStepperHarness.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnityPlayerLoopStepperWindow.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/RuntimeBackedUnityPlayerLoopStepperScriptProof.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal138Tests.cs
tests/LLMGameCreator.Tests/DevFlow/RunRuntimeBackedUnityPlayerLoopStepperScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage or commit:

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

No public GamePackage schema changes. No sample package mutation. No Runtime changes. No Runtime.Abstractions changes. No Unity scene/prefab/settings/StreamingAssets changes. No provider/media/LLM/Lua/generator-library work.

## Primary deliverable A — record Goal137 human acceptance

Record the owner's acceptance in repo evidence without fabricating raw manual input.

Update or create:

```text
docs/manual-acceptance/canonical-runtime-unity-player-loop-playback-harness.md
```

Required fields/phrases:

```text
accepted=true
acceptedByHuman=true
acceptedByCodex=false
manualUnityOptional=true
selectedCandidate=minimal-map-game-balanced-baseline
playbackFrames=13
Unity playback smoke GREEN
projectionOnly=false
unityGameplayTruth=false
rawManualInputNotCommitted=true
```

Update current state so Goal137 is accepted while Goal138 remains `accepted=false` until reviewed.

## Primary deliverable B — runtime-backed Unity stepper/HUD model

Create an Application artifact service that consumes:

```text
Goal137 playback frames
Goal136 runtime snapshots/result
Goal135 player adapter contract
```

and writes a deterministic stepper model.

Minimum model shape:

```text
candidateId
frameCount
currentFrameIndex default 0
frames[]
  frameIndex
  frameCategory
  runtimeCommandId / commandStepId
  title
  playerFacingSummary
  canonicalStateHash
  runtimeEventCount
  mapPositionSummary
  interactionSummary
  dialogueSummary
  questSummary
  inventorySummary
  combatSummary
  hudLines[]
  sourceSnapshotPath/sourceFramePath markers
runtimeAuthority=true
unityGameplayTruth=false
projectionOnly=false
```

Required categories must include at least:

```text
load_package
show_start_state
show_map_position
show_interaction_result
show_dialogue
show_quest_state
show_inventory_state
show_crafting_result
show_harvest_result
show_transaction_result
show_encounter_state
show_combat_round
show_final_state
```

## Primary deliverable C — Unity stepper harness

Add Unity player/presentation harness:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityPlayerLoopStepperHarness.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnityPlayerLoopStepperWindow.cs
```

Requirements:

- Consumes the Goal138 stepper model JSON.
- Does not execute gameplay logic.
- Has a batchmode smoke method:

```text
LLMGameCreatorAlpha.CanonicalRuntimeUnityPlayerLoopStepperHarness.RunBatchmodeRuntimeBackedUnityPlayerLoopStepperSmoke
```

- Logs pass/fail markers:

```text
GOAL138_RUNTIME_BACKED_UNITY_PLAYER_LOOP_STEPPER_PASS
GOAL138_RUNTIME_BACKED_UNITY_PLAYER_LOOP_STEPPER_FAIL
```

- The EditorWindow should be accessible from a menu such as:

```text
LLMGameCreator/Accepted Alpha/Runtime Player Loop Stepper
```

- The window should be useful enough for a human later:
  - load default Goal138 stepper model;
  - previous/next frame buttons;
  - frame index/total;
  - candidate id;
  - frame category/title;
  - HUD lines;
  - canonical state hash;
  - explicit label `Gameplay truth: Runtime`.

Manual inspection is optional for Goal138, but the UI must be present and batch-smoked.

## Primary deliverable D — one-click command

Add:

```text
.devflow/scripts/run-runtime-backed-unity-player-loop-stepper.ps1
.devflow/scripts/run-runtime-backed-unity-player-loop-stepper.cmd
```

Defaults:

```text
PlaybackFramesPath = .llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness/canonical-runtime-unity-player-loop-playback-frames.json
PlaybackResultPath = .llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness/canonical-runtime-unity-player-loop-playback-result.json
CommandLoopSnapshotsPath = .llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-snapshots.json
PlayerAdapterContractPath = .llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-player-adapter-contract.json
OutputRoot = .llmgc/procedural/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness
```

Script supports:

```text
-DryRun
-ApplyCleanup
-UnityPath
```

It must:

1. validate paths under repo root;
2. reject `.llmgc/manual/**`;
3. write only under Goal138 output root/export root;
4. generate stepper artifacts via tests/Application proof or direct service call pattern already used by recent goals;
5. run Unity batchmode smoke;
6. scan logs for pass/fail markers;
7. write compact result JSON;
8. apply bounded cleanup when requested;
9. return non-zero on failure.

## Required artifacts

Under both procedural and export roots:

```text
goal137-human-acceptance-record.json
runtime-backed-player-loop-stepper-model.json
runtime-backed-player-loop-stepper-dashboard.json
runtime-backed-player-loop-stepper-result.json
runtime-backed-player-loop-stepper-frame-index.json
unity-player-loop-stepper-smoke.json
runtime-backed-player-loop-stepper-negative-proof.json
runtime-backed-player-loop-stepper-file-index.json
one-click-runtime-backed-player-loop-stepper-report.json
one-click-runtime-backed-player-loop-stepper-report.md
```

Dashboard required markers:

```text
status=GREEN
accepted=false
acceptedGoal137=true
candidateId=minimal-map-game-balanced-baseline
frameCount=13
requiredFrameCategoriesPresent=true
runtimeAuthority=true
unityGameplayTruth=false
projectionOnly=false
stepperWindowPresent=true
stepperBatchSmokePassed=true
manualUnityOptional=true
```

## VisualWorld / WinForms proof surface

Add a read-only Goal138 section showing:

```text
acceptedGoal137
candidateId
frameCount
requiredFrameCategoriesPresent
runtimeAuthority
unityGameplayTruth
projectionOnly
stepperWindowPresent
stepperBatchSmokePassed
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

Required current-state markers:

```text
goal137Accepted=true
goal138Accepted=false
projectionOnly=false
runtimeAuthority=true
unityGameplayTruth=false
runtimeBackedUnityStepper=true
stepperBatchSmokePassed=true
manualUnityOptional=true
```

Next product goal after Goal138 should be a manual review/acceptance checkpoint or an explicitly runtime-backed interactive HUD/player loop step, not projection-only work.

## Artifact-scope policy

Add scenario:

```text
goal-138-runtime-backed-unity-player-loop-stepper-hud-harness
```

It must allow only the Goal138 paths and exclude all forbidden zones.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~RuntimeBackedUnityPlayerLoopStepper|FullyQualifiedName~Goal138|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\run-runtime-backed-unity-player-loop-stepper.ps1 -DryRun
.\.devflow\scripts\run-runtime-backed-unity-player-loop-stepper.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-138-runtime-backed-unity-player-loop-stepper-hud-harness
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Also verify forbidden diffs are empty:

```powershell
git diff --name-only -- samples/minimal-map-game src/LLMGameCreator.Runtime src/LLMGameCreator.Runtime.Abstractions src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity/LLMGameCreatorAlpha/Assets/Scenes unity/LLMGameCreatorAlpha/Assets/Prefabs unity/LLMGameCreatorAlpha/Assets/StreamingAssets unity/LLMGameCreatorAlpha/ProjectSettings unity/LLMGameCreatorAlpha/Packages
```

## Quality gate

GREEN requires:

- Goal137 human acceptance recorded with `acceptedByHuman=true` and `acceptedByCodex=false`.
- Goal138 stepper artifacts generated from Goal136/137 runtime-backed artifacts.
- `frameCount=13`.
- Required frame categories present.
- Unity stepper window/harness exists.
- Unity batchmode stepper smoke passes.
- `runtimeAuthority=true`.
- `unityGameplayTruth=false`.
- `projectionOnly=false`.
- Tests/checks pass.
- Artifact scope passes.
- No forbidden path changes.
- No `.llmgc/manual/**` tracked/staged.
- Final git status clean.

BLOCKED if Unity batchmode cannot load the harness honestly.

FAILED if build/tests break or forbidden changes are required.

## Commit / push policy

Commit and push with one of:

```text
GREEN Goal 138 runtime-backed Unity player loop stepper HUD harness
BLOCKED Goal 138 runtime-backed Unity player loop stepper HUD harness
FAILED Goal 138 runtime-backed Unity player loop stepper HUD harness
```

Push to `origin/main`.

Final report must include:

- commit SHA;
- Goal137 acceptance record status;
- candidate id;
- frame count;
- stepper batch smoke result;
- normal command;
- one-click report path;
- forbidden-zone confirmation;
- final git status.
