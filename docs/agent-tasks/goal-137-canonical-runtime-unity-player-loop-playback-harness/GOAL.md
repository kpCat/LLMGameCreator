# Goal 137 — Canonical Runtime Unity Player Loop Playback Harness

## Task ID

`goal-137-canonical-runtime-unity-player-loop-playback-harness`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Strategic intent

Goal134 proved the selected candidate through canonical Runtime playthrough, save/load/replay and Unity/player transcript consumption.

Goal135 proved PlayerAdapter readiness over the canonical runtime transcript/state.

Goal136 executed a Runtime-owned player command loop and produced per-command player-facing snapshots.

Goal137 must turn those canonical snapshots into a Unity/player playback harness:

```text
selected candidate
→ canonical runtime command-loop snapshots
→ Unity/player playback frames
→ deterministic HUD/player/quest/inventory/combat presentation summary
→ batchmode playback smoke
→ one-click report
```

Gameplay truth must remain Runtime-owned. Unity is a player/adapter consuming canonical runtime snapshots, not a separate gameplay implementation.

## Goal type

Aggressive product goal.

This is not docs-only, not projection-only, and not another passive report wrapper. It must add a playback harness that consumes the Goal136 canonical runtime snapshots and proves Unity/player can present the command loop.

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

.llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-state-summary.json
.llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-player-adapter-contract.json
.llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-player-loop-plan.json
.llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-result.json
.llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-snapshots.json
.llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-dashboard.json

src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimePlayerCommandLoopAdapter.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-137-canonical-runtime-unity-player-loop-playback-harness/**
.devflow/artifact-scope/artifact-scope-policy.json

.devflow/scripts/run-canonical-runtime-unity-player-loop-playback.ps1
.devflow/scripts/run-canonical-runtime-unity-player-loop-playback.cmd

.llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness/**
.llmgc/exports/goal-137-canonical-runtime-unity-player-loop-playback-harness/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/canonical-runtime-unity-player-loop-playback-harness.md

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/CanonicalRuntimeUnityPlayerLoopPlaybackModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/CanonicalRuntimeUnityPlayerLoopPlaybackArtifactService.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineService.cs

src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal137.cs

unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityPlayerLoopPlaybackAdapter.cs
unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/CanonicalRuntimeUnityPlayerLoopPlaybackScriptProof.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal137Tests.cs
tests/LLMGameCreator.Tests/DevFlow/RunCanonicalRuntimeUnityPlayerLoopPlaybackScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage, or commit:

```text
.llmgc/manual/**
samples/minimal-map-game/**
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

Do not change Runtime or Runtime.Abstractions in this goal unless a compile-only adapter type is strictly impossible without it. Goal136 already owns the Runtime command loop. Goal137 should consume its artifacts.

No public GamePackage schema changes. No sample package mutation. No `.llmgc/manual/**`. No provider/media/LLM/Lua/generator-library work.

Unity changes are limited to player playback adapter scripts/editor hook. No scenes, prefabs, project settings, packages or StreamingAssets.

## Required normal command

Add:

```bat
.devflow\scripts\run-canonical-runtime-unity-player-loop-playback.cmd
```

The `.cmd` must call the `.ps1` with sane defaults.

The PowerShell script should support:

```text
-CommandLoopSnapshotsPath
-CommandLoopResultPath
-PlayerAdapterContractPath
-StateSummaryPath
-OutputRoot
-UnityPath
-DryRun
-ApplyCleanup
```

Defaults:

```text
CommandLoopSnapshotsPath = .llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-snapshots.json
CommandLoopResultPath = .llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-result.json
PlayerAdapterContractPath = .llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-player-adapter-contract.json
StateSummaryPath = .llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-state-summary.json
OutputRoot = .llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness
```

The script must:

1. Validate all input paths stay under repo root.
2. Refuse `.llmgc/manual/**`.
3. Refuse writing outside Goal137 output root.
4. Build deterministic playback plan/frames from Goal136 snapshots.
5. Run Unity batchmode playback smoke.
6. Write compact artifacts.
7. Apply bounded cleanup when `-ApplyCleanup` is provided.
8. Return non-zero on failure.

## Playback harness deliverable

Create Application-side artifact service:

```text
CanonicalRuntimeUnityPlayerLoopPlaybackModels.cs
CanonicalRuntimeUnityPlayerLoopPlaybackArtifactService.cs
```

It must read the Goal136 command-loop snapshots and build:

```text
playbackFrameCount >= 13
playerPositionFrames present
hudFrames present
interaction/dialogue/quest/inventory/crafting/harvest/transaction/encounter/combat/final-state frames present
runtimeSnapshotSource=true
unityGameplayTruth=false
projectionOnly=false
selectedCandidateExecutedByRuntime=true
```

It must not recompute gameplay outcomes. It only derives player-facing playback frames from canonical runtime snapshots.

## Unity/player playback adapter

Add:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityPlayerLoopPlaybackAdapter.cs
```

It must read:

```text
-llmgcCanonicalRuntimePlaybackFramesPath
-llmgcCanonicalRuntimePlaybackResultPath
```

and verify:

```text
frames file exists
result file exists
frame count >= 13
required frame categories present
runtime authority markers present
Unity/player does not claim gameplay truth
```

Batchmode method:

```text
LLMGameCreatorAlpha.CanonicalRuntimeUnityPlayerLoopPlaybackAdapter.RunBatchmodeCanonicalRuntimeUnityPlayerLoopPlaybackSmoke
```

Pass marker:

```text
GOAL137_CANONICAL_RUNTIME_UNITY_PLAYER_LOOP_PLAYBACK_PASS
```

Fail marker:

```text
GOAL137_CANONICAL_RUNTIME_UNITY_PLAYER_LOOP_PLAYBACK_FAIL
```

The adapter should produce no scene/prefab/project modifications.

## Optional editor hook

If touching `AcceptedAlphaPlayableProjectionWindow.cs`, add only a small menu/window action to run or show Goal137 playback status. Do not create a scene, prefab or StreamingAssets dependency.

## Required artifacts

Under both procedural and export roots:

```text
canonical-runtime-unity-player-loop-playback-dashboard.json
canonical-runtime-unity-player-loop-playback-result.json
canonical-runtime-unity-player-loop-playback-plan.json
canonical-runtime-unity-player-loop-playback-frames.json
canonical-runtime-unity-player-loop-playback-matrix-result.json
unity-player-loop-playback-smoke.json
canonical-runtime-unity-player-loop-playback-negative-proof.json
canonical-runtime-unity-player-loop-playback-file-index.json
one-click-unity-player-loop-playback-report.json
one-click-unity-player-loop-playback-report.md
```

Raw Unity `.log` files may remain local/ignored if compact smoke artifacts prove pass/fail.

## Dashboard required fields

```text
goalId=goal_137_canonical_runtime_unity_player_loop_playback_harness
status=GREEN
candidateId=minimal-map-game-balanced-baseline
projectionOnly=false
canonicalRuntimeSource=true
runtimeSnapshotSource=true
playbackFrameCount>=13
requiredFrameCategoriesPresent=true
unityPlayerLoopPlaybackPassed=true
unityGameplayTruth=false
selectedCandidateExecutedByRuntime=true
unityConsumesRuntimeSnapshots=true
manualUnityOptional=true
accepted=false
noUnclassifiedErrorDiagnostics=true
```

## VisualWorld / WinForms proof surface

Add read-only Goal137 section showing:

```text
candidateId
playbackFrameCount
requiredFrameCategoriesPresent
unityPlayerLoopPlaybackPassed
runtimeSnapshotSource
unityGameplayTruth
projectionOnly
selectedCandidateExecutedByRuntime
normalCommand
reportPath
matrixResultPath
manualUnityOptional
```

Manual Unity inspection remains optional.

## Docs/current state

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
```

Current state must say Goal137 is the canonical runtime Unity/player loop playback harness.

Required current-state markers:

```text
projectionOnly=false
canonicalRuntimeSource=true
runtimeSnapshotSource=true
unityConsumesRuntimeSnapshots=true
unityPlayerLoopPlaybackPassed=true
unityGameplayTruth=false
manualUnityOptional=true
accepted=false
```

## Artifact-scope policy

Add scenario:

```text
goal-137-canonical-runtime-unity-player-loop-playback-harness
```

It must allow only expected Goal137 paths and exclude `.llmgc/manual/**`, samples, GamePackage schema, Generation, AssetPipeline, Scripting, generator-library, provider/media, Unity scenes/prefabs/settings/packages/StreamingAssets, solution/project/dependency files.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~CanonicalRuntimeUnityPlayerLoopPlayback|FullyQualifiedName~Goal137|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\run-canonical-runtime-unity-player-loop-playback.ps1 -DryRun
.\.devflow\scripts\run-canonical-runtime-unity-player-loop-playback.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-137-canonical-runtime-unity-player-loop-playback-harness
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Also verify forbidden diffs:

```powershell
git diff --name-only -- samples/minimal-map-game src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity/LLMGameCreatorAlpha/Assets/Scenes unity/LLMGameCreatorAlpha/Assets/Prefabs unity/LLMGameCreatorAlpha/Assets/StreamingAssets unity/LLMGameCreatorAlpha/ProjectSettings unity/LLMGameCreatorAlpha/Packages
```

## Quality gate

GREEN requires:

- Goal136 snapshots/result loaded.
- Playback frames derived from canonical runtime snapshots.
- Playback frame count >= 13.
- Required frame categories present.
- Unity/player playback smoke passed.
- Unity gameplay truth remains false.
- `projectionOnly=false`.
- `selectedCandidateExecutedByRuntime=true`.
- `unityConsumesRuntimeSnapshots=true`.
- No unclassified error diagnostics.
- Tests/checks pass.
- Artifact scope passes.
- No forbidden path changes.
- No `.llmgc/manual/**` tracked/staged.
- Final git status clean.

BLOCKED if Unity/player cannot consume Goal136 snapshots without forbidden scene/prefab/project changes. Write a concrete blocker report.

FAILED if build/tests break or forbidden changes are required.

## Commit / push policy

Commit and push with one of:

```text
GREEN Goal 137 canonical runtime Unity player loop playback harness
BLOCKED Goal 137 canonical runtime Unity player loop playback harness
FAILED Goal 137 canonical runtime Unity player loop playback harness
```

Push to `origin/main`.

Final report must include:

- commit SHA;
- selected candidate id;
- playback frame count;
- Unity/player playback smoke result;
- one-click report path;
- forbidden-zone confirmation;
- final git status.
