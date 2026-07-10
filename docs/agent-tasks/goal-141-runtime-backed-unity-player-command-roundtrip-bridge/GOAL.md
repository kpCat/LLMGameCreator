# Goal 141 — Runtime-backed Unity Player Command Roundtrip Bridge

## Task ID

`goal-141-runtime-backed-unity-player-command-roundtrip-bridge`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Strategic intent

Goal134-140 moved the project from projection-only proof into a runtime-backed player loop:

```text
Goal134: selected candidate executed by canonical Runtime
Goal135: PlayerAdapter readiness over canonical output
Goal136: Runtime-owned player command loop and snapshots
Goal137: Unity/player playback over runtime snapshots
Goal138: runtime-backed Unity stepper/HUD
Goal139: runtime-backed interactive controls
Goal140: controls UX polish and Unity editor-noise guard
```

Goal141 must make the next product seam explicit:

```text
Unity/PlayerAdapter control intent
-> command request artifact
-> canonical Runtime execution
-> updated runtime snapshots/result
-> Unity/player consumes the roundtrip result
-> one-click report
```

Unity must remain a PlayerAdapter/HUD/control surface only. Runtime remains gameplay truth.

## Required first deliverable — record Goal140 human acceptance

Record the user's acceptance from chat:

```text
Я принимаю Goal140 runtime_backed_unity_player_loop_controls_ux_polish_and_noise_guard_verification GREEN. selectedCandidate=minimal-map-game-balanced-baseline, frames=13, humanReadableFrameNumbering=true, stepOnceSemanticsClear=true, playAllToEndSemanticsClear=true, copyFrameSummaryStatusPresent=true, knownUnityEditorNoiseClassified=true, blockingUnityErrorCount=0, projectionOnly=false, runtimeAuthority=true, unityGameplayTruth=false.
```

Write it as bounded evidence, not raw `.llmgc/manual/**` input:

```text
.llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/goal140-human-acceptance-record.json
.llmgc/exports/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/goal140-human-acceptance-record.json
docs/manual-acceptance/runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard.md
```

Required fields:

```text
accepted=true
acceptedByHuman=true
acceptedByCodex=false
rawManualInputNotCommitted=true
selectedCandidate=minimal-map-game-balanced-baseline
frames=13
humanReadableFrameNumbering=true
stepOnceSemanticsClear=true
playAllToEndSemanticsClear=true
copyFrameSummaryStatusPresent=true
knownUnityEditorNoiseClassified=true
blockingUnityErrorCount=0
projectionOnly=false
runtimeAuthority=true
unityGameplayTruth=false
```

Goal141 itself must remain `accepted=false`.

## Required read-first

Read in order:

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

.llmgc/procedural/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard/runtime-backed-player-loop-controls-ux-model.json
.llmgc/procedural/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard/runtime-backed-player-loop-controls-ux-result.json
.llmgc/procedural/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard/runtime-backed-player-loop-controls-ux-script.json
.llmgc/procedural/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard/unity-editor-noise-classification.json

.llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/runtime-backed-player-loop-interactive-controls-model.json
.llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-snapshots.json
.llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-result.json
.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/package.json
.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/selected-candidate-handoff.json

src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnityPlayerLoopInteractiveControlsWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityPlayerLoopInteractiveControlsHarness.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/**
.devflow/artifact-scope/artifact-scope-policy.json

.devflow/scripts/run-runtime-backed-unity-player-command-roundtrip.ps1
.devflow/scripts/run-runtime-backed-unity-player-command-roundtrip.cmd

.llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/**
.llmgc/exports/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard.md
docs/manual-acceptance/runtime-backed-unity-player-command-roundtrip-bridge.md

src/LLMGameCreator.Runtime.Abstractions/RuntimeBackedPlayerCommandRoundtripContracts.cs
src/LLMGameCreator.Runtime/RuntimeBackedPlayerCommandRoundtripService.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/RuntimeBackedPlayerCommandRoundtripModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/RuntimeBackedPlayerCommandRoundtripArtifactService.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineService.cs

src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal141.cs

unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityPlayerCommandRoundtripHarness.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnityPlayerCommandRoundtripWindow.cs

tests/LLMGameCreator.Tests/Runtime/RuntimeBackedPlayerCommandRoundtripServiceTests.cs
tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/RuntimeBackedPlayerCommandRoundtripScriptProof.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal141Tests.cs
tests/LLMGameCreator.Tests/Devflow/RunRuntimeBackedPlayerCommandRoundtripScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage or commit:

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

No public GamePackage schema changes. No sample package mutation. No provider/media/LLM/Lua/generator-library work. Unity must not become gameplay truth.

## Required normal command

Add:

```bat
.devflow\scripts\run-runtime-backed-player-command-roundtrip.cmd
```

PowerShell script:

```text
.devflow/scripts/run-runtime-backed-player-command-roundtrip.ps1
```

Must support:

```text
-SelectedCandidatePackagePath
-SelectedCandidateHandoffPath
-ControlsUxModelPath
-ControlsUxResultPath
-ControlsUxScriptPath
-CommandLoopSnapshotsPath
-CommandLoopResultPath
-OutputRoot
-UnityPath
-DryRun
-ApplyCleanup
```

Defaults must point to Goal131 selected candidate, Goal136 snapshots/result and Goal140 UX artifacts.

Script behavior:

1. Validate all input paths stay under repo root.
2. Refuse `.llmgc/manual/**`.
3. Refuse writing outside Goal141 output root.
4. Execute runtime-owned command roundtrip proof.
5. Run Unity/player roundtrip consumption smoke.
6. Write compact artifacts and one-click report.
7. Apply bounded Unity cleanup when `-ApplyCleanup` is provided.
8. Return non-zero on failure.

## Runtime roundtrip deliverable

Add a Runtime-owned command roundtrip seam.

Minimum acceptable behavior:

- Build a command request set from Goal140 controls UX model/script.
- Map control intents to canonical runtime command requests.
- Execute at least 6 requests through Runtime-owned code.
- Produce new runtime snapshots/state hash chain.
- Produce a roundtrip result proving request -> runtime execution -> snapshot response.
- Reuse existing `CanonicalRuntimePlayerCommandLoopService` where appropriate.
- Do not execute gameplay in Unity.

Required request/control categories:

```text
load_model
reset_first
step_once
next_frame
play_all_to_end
copy_frame_summary
```

Required runtime command coverage must include at least:

```text
load_package_or_session
show_or_select_start_state
advance_to_interaction
advance_to_dialogue_or_quest
advance_to_inventory_or_crafting
advance_to_combat_or_final_state
```

GREEN requires:

```text
roundtripRequestCount >= 6
runtimeExecutedRequestCount >= 6
roundtripSnapshotCount >= runtimeExecutedRequestCount
stateHashChainPresent=true
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
unityConsumesRoundtripResult=true
controlRequestBridgePresent=true
noUnclassifiedErrorDiagnostics=true
```

## Unity/player command roundtrip consumer

Add Unity player adapter/harness:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityPlayerCommandRoundtripHarness.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnityPlayerCommandRoundtripWindow.cs
```

Menu path:

```text
LLMGameCreator/Accepted Alpha/Runtime Player Command Roundtrip
```

Unity window should show:

```text
Gameplay truth: Runtime
Unity mode: PlayerAdapter command request/response only
Candidate
Request count
Executed request count
Snapshot count
Current request
Current response snapshot
Status
```

The window may be read-only over Goal141 model. It should not execute gameplay truth.

Unity batchmode smoke must validate:

```text
modelPathExists=true
roundtripRequestCountPassed=true
runtimeSnapshotResponsePresent=true
runtimeAuthorityMarkersPresent=true
unityConsumesRoundtripResult=true
unityGameplayTruth=false
passMarkerPresent=true
failMarkerPresent=false
```

Pass marker:

```text
GOAL141_RUNTIME_BACKED_PLAYER_COMMAND_ROUNDTRIP_PASS
```

Fail marker:

```text
GOAL141_RUNTIME_BACKED_PLAYER_COMMAND_ROUNDTRIP_FAIL
```

## Required artifacts

Under both procedural and export roots:

```text
goal140-human-acceptance-record.json
runtime-backed-player-command-roundtrip-request.json
runtime-backed-player-command-roundtrip-result.json
runtime-backed-player-command-roundtrip-session.json
runtime-backed-player-command-roundtrip-snapshots.json
runtime-backed-player-command-roundtrip-model.json
runtime-backed-player-command-roundtrip-dashboard.json
runtime-backed-player-command-roundtrip-negative-proof.json
runtime-backed-player-command-roundtrip-file-index.json
unity-player-command-roundtrip-smoke.json
one-click-runtime-backed-player-command-roundtrip-report.json
one-click-runtime-backed-player-command-roundtrip-report.md
```

## VisualWorld / WinForms proof surface

Add a read-only Goal141 section showing:

```text
goal140Accepted
candidateId
roundtripRequestCount
runtimeExecutedRequestCount
roundtripSnapshotCount
controlRequestBridgePresent
stateHashChainPresent
runtimeAuthority
projectionOnly
unityGameplayTruth
unityConsumesRoundtripResult
normalCommand
reportPath
manualUnityOptional
accepted
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
goal140Accepted=true
goal141Accepted=false
runtimeBackedPlayerCommandRoundtrip=true
controlRequestBridgePresent=true
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
unityConsumesRoundtripResult=true
manualUnityOptional=true
```

## Artifact-scope policy

Add scenario:

```text
goal-141-runtime-backed-unity-player-command-roundtrip-bridge
```

Allow only expected Goal141 paths and exclude forbidden paths.

## Validation

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~RuntimeBackedPlayerCommandRoundtrip|FullyQualifiedName~Goal141|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\run-runtime-backed-player-command-roundtrip.ps1 -DryRun
.\.devflow\scripts\run-runtime-backed-player-command-roundtrip.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-141-runtime-backed-unity-player-command-roundtrip-bridge
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Forbidden diff check:

```powershell
git diff --name-only -- samples/minimal-map-game src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity/LLMGameCreatorAlpha/Assets/Scenes unity/LLMGameCreatorAlpha/Assets/Prefabs unity/LLMGameCreatorAlpha/Assets/StreamingAssets unity/LLMGameCreatorAlpha/ProjectSettings unity/LLMGameCreatorAlpha/Packages
```

Also check changed files for mojibake and escaped Cyrillic markers.

## Quality gate

GREEN requires:

- Goal140 human acceptance recorded.
- Goal141 remains accepted=false.
- Runtime-owned command roundtrip executes.
- Request count >= 6.
- Runtime executed request count >= 6.
- Snapshot count >= executed request count.
- Unity/player consumes roundtrip result.
- `runtimeAuthority=true`.
- `projectionOnly=false`.
- `unityGameplayTruth=false`.
- No unclassified error diagnostics.
- Tests/checks pass.
- Artifact scope passes.
- No forbidden path changes.
- No `.llmgc/manual/**` tracked/staged.
- Final git status clean.

BLOCKED if existing Runtime command-loop services cannot support a real request -> runtime -> snapshot response without forbidden schema changes.

FAILED if build/tests break or forbidden changes are required.

## Commit / push policy

Commit and push with one of:

```text
GREEN Goal 141 runtime-backed Unity player command roundtrip bridge
BLOCKED Goal 141 runtime-backed Unity player command roundtrip bridge
FAILED Goal 141 runtime-backed Unity player command roundtrip bridge
```

Final report must include commit SHA, Goal140 acceptance status, candidate id, request/executed/snapshot counts, Unity smoke result, one-click report path, forbidden-zone confirmation and final git status.
