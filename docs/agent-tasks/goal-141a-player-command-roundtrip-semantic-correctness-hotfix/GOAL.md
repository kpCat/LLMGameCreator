# Goal 141A — Player Command Roundtrip Semantic Correctness Hotfix

## Task ID

`goal-141a-player-command-roundtrip-semantic-correctness-hotfix`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

P1 correctness hotfix. Do not add another product layer.

Goal141 is mechanically GREEN, but its current evidence is semantically incorrect. The service executes the complete canonical player command loop first and only afterward maps six UI control intents onto selected pre-existing steps/snapshots. It then sets every response's execution flag from the aggregate loop result:

```text
RuntimeExecuted = canonical.PlayerCommandLoopPassed
```

This creates false request-level semantics. In the committed result:

```text
load_model:
  runtimeExecuted=true
  canonicalStepRuntimeExecuted=false

copy_frame_summary:
  mapped to combat_round / BasicAttack
  runtimeExecuted=true
```

`load_model` and `copy_frame_summary` are presentation-only controls. Copying a frame summary must never trigger or claim a Runtime `BasicAttack`.

Goal141A must implement a real correlated sequence:

```text
create control request
→ classify route
→ execute Runtime-routed request against a persistent Runtime session
→ produce request-specific response/snapshot(s)
→ preserve state for presentation-only request
→ validate correlation/cursor/hash continuity
→ Unity consumes corrected result read-only
```

## Required read-first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md

docs/agent-tasks/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/GOAL.md

src/LLMGameCreator.Runtime.Abstractions/RuntimeBackedPlayerCommandRoundtripContracts.cs
src/LLMGameCreator.Runtime/RuntimeBackedPlayerCommandRoundtripService.cs
src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs

src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/RuntimeBackedPlayerCommandRoundtripModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/RuntimeBackedPlayerCommandRoundtripArtifactService.cs

.llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/runtime-backed-player-command-roundtrip-request.json
.llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/runtime-backed-player-command-roundtrip-result.json
.llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/runtime-backed-player-command-roundtrip-session.json

unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityPlayerCommandRoundtripHarness.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnityPlayerCommandRoundtripWindow.cs
```

## Allowed paths

```text
docs/agent-tasks/goal-141a-player-command-roundtrip-semantic-correctness-hotfix/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-runtime-backed-player-command-roundtrip.ps1
.devflow/scripts/run-runtime-backed-player-command-roundtrip.cmd

.llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/**
.llmgc/exports/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/**
.llmgc/procedural/goal-141a-player-command-roundtrip-semantic-correctness-hotfix/**
.llmgc/exports/goal-141a-player-command-roundtrip-semantic-correctness-hotfix/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/runtime-backed-unity-player-command-roundtrip-bridge.md

src/LLMGameCreator.Runtime.Abstractions/RuntimeBackedPlayerCommandRoundtripContracts.cs
src/LLMGameCreator.Runtime/RuntimeBackedPlayerCommandRoundtripService.cs
src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs

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
tests/LLMGameCreator.Tests/Runtime/CanonicalRuntimePlayerCommandLoopServiceTests.cs
tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/RuntimeBackedPlayerCommandRoundtripScriptProof.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal141Tests.cs
tests/LLMGameCreator.Tests/Devflow/RunRuntimeBackedPlayerCommandRoundtripScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

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

No public GamePackage schema change. No sample mutation. No provider/media/LLM/Lua/generator-library work.

## Required routing semantics

Every control request must explicitly declare one route:

```text
runtime_session
runtime_command
runtime_command_batch
presentation_only
```

Required controls:

```text
load_model:
  route=presentation_only
  runtimeExecuted=false
  runtimeMutation=false

reset_first:
  route=runtime_session
  runtimeExecuted=true
  operation=reset_or_initialize_session

step_once:
  route=runtime_command
  runtimeExecuted=true
  execute exactly the next Runtime-owned command from current cursor

next_frame:
  route=runtime_command
  runtimeExecuted=true
  execute exactly the next Runtime-owned command from current cursor

play_all_to_end:
  route=runtime_command_batch
  runtimeExecuted=true
  execute all remaining Runtime-owned commands from current cursor
  record executedCommandCount and produced snapshots/trace

copy_frame_summary:
  route=presentation_only
  runtimeExecuted=false
  runtimeMutation=false
  stateHashBefore == stateHashAfter
  eventCount=0
```

Never map presentation-only controls to gameplay commands.

## Persistent Runtime session/cursor

The request sequence must operate over one persistent canonical Runtime session and cursor.

1. Construct each request before its execution.
2. Execute in request-index order.
3. Preserve the session produced by the previous Runtime-routed request.
4. `step_once` and `next_frame` advance from the current cursor, not fixed snapshot indices.
5. `play_all_to_end` executes the remaining Runtime commands from the current cursor.
6. Presentation-only requests preserve current Runtime state/hash.
7. Response requestId/requestIndex must match its request.
8. Events and snapshots attributed to a request must be produced by that request, not selected after an unrelated full run.

A focused incremental/session API may be added to the existing canonical command-loop service. Do not create a parallel gameplay runtime.

## Required contract fields

Add fields or equivalent typed contracts:

```text
requestId
requestIndex
controlIntent
route
requestedOperation
runtimeCommandStartIndex
runtimeCommandEndIndex
runtimeExecuted
runtimeMutation
executedCommandCount
producedSnapshotCount
stateHashBefore
stateHashAfter
eventCount
correlationPassed
```

## Required aggregate proof

```text
totalControlRequestCount=6
runtimeRoutedRequestCount=4
presentationOnlyRequestCount=2
runtimeExecutedRequestCount=4
presentationOnlyRuntimeExecutionCount=0
runtimeMutatingPresentationRequestCount=0
responseCount=6
requestResponseCorrelationPassed=true
sequentialCursorContinuityPassed=true
stateHashContinuityPassed=true
copySummaryStateUnchanged=true
loadModelStateUnchanged=true
playAllExecutedRemainingCommands=true
noControlIntentMappedToUnrelatedGameplayCommand=true
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
```

`reset_first`, `step_once`, `next_frame`, `play_all_to_end` are Runtime-routed. `load_model` and `copy_frame_summary` are presentation-only.

## Explicit regressions that must fail

```text
copy_frame_summary -> BasicAttack
copy_frame_summary runtimeExecuted=true
copy_frame_summary changes state hash
load_model runtimeExecuted=true
load_model canonicalStepRuntimeExecuted=true
all response RuntimeExecuted values sourced from canonical.PlayerCommandLoopPassed
request created only after a complete Runtime loop
fixed control-to-snapshot index lookup used as execution proof
runtimeExecuted=true while executedCommandCount=0
request/response IDs mismatch
state-hash continuity break between sequential requests
```

Remove the anti-pattern:

```text
RuntimeExecuted = canonical.PlayerCommandLoopPassed
```

## Corrected Goal141 artifacts

Regenerate the existing Goal141 procedural/export artifacts with honest semantics:

```text
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

Also write compact Goal141A evidence:

```text
roundtrip-semantic-correctness-dashboard.json
roundtrip-semantic-correctness-regression-proof.json
roundtrip-semantic-correctness-report.md
roundtrip-semantic-correctness-file-index.json
```

under both Goal141A procedural and export roots.

## Unity consumer

Unity stays read-only. Update the smoke/harness to verify:

```text
presentationOnlyRequestCount=2
presentationOnlyRuntimeExecutionCount=0
requestResponseCorrelationPassed=true
sequentialCursorContinuityPassed=true
copySummaryStateUnchanged=true
loadModelStateUnchanged=true
noControlIntentMappedToUnrelatedGameplayCommand=true
runtimeAuthority=true
unityGameplayTruth=false
```

Do not add Unity gameplay execution.

## Current state

Goal141 remains:

```text
implementationStatus=GREEN
accepted=false
```

Add corrected markers:

```text
roundtripSemanticCorrectnessPassed=true
totalControlRequestCount=6
runtimeRoutedRequestCount=4
presentationOnlyRequestCount=2
runtimeExecutedRequestCount=4
presentationOnlyRuntimeExecutionCount=0
requestResponseCorrelationPassed=true
sequentialCursorContinuityPassed=true
stateHashContinuityPassed=true
copySummaryStateUnchanged=true
loadModelStateUnchanged=true
noControlIntentMappedToUnrelatedGameplayCommand=true
```

Also correct stale Goal140 current-state count to match its classifier/dashboard:

```text
knownUnityEditorNoiseCount=1
```

No manual check is required after this automated P1 hotfix. Manual Goal141 acceptance comes only after audit of the corrected evidence.

## Artifact-scope scenario

Add:

```text
goal-141a-player-command-roundtrip-semantic-correctness-hotfix
```

## Validation

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~RuntimeBackedPlayerCommandRoundtrip|FullyQualifiedName~CanonicalRuntimePlayerCommandLoop|FullyQualifiedName~Goal141|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\run-runtime-backed-player-command-roundtrip.ps1 -DryRun
.\.devflow\scripts\run-runtime-backed-player-command-roundtrip.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-141a-player-command-roundtrip-semantic-correctness-hotfix
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Forbidden diff:

```powershell
git diff --name-only -- samples/minimal-map-game src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity/LLMGameCreatorAlpha/Assets/Scenes unity/LLMGameCreatorAlpha/Assets/Prefabs unity/LLMGameCreatorAlpha/Assets/StreamingAssets unity/LLMGameCreatorAlpha/ProjectSettings unity/LLMGameCreatorAlpha/Packages
```

## Quality gate

GREEN requires all corrected semantic markers above.

BLOCKED if persistent incremental execution cannot be added without a bounded Runtime/Runtime.Abstractions extension.

FAILED if request claims are again derived from a completed aggregate run, if a presentation-only control maps to gameplay, or if forbidden changes are required.

## Commit / push policy

```text
GREEN Goal 141A player command roundtrip semantic correctness hotfix
BLOCKED Goal 141A player command roundtrip semantic correctness hotfix
FAILED Goal 141A player command roundtrip semantic correctness hotfix
```

Final report must include commit SHA, request routing counts, actual Runtime execution count, presentation-only proof, correlation/cursor/hash-continuity proof, Unity smoke result, Goal140 stale-count correction, forbidden-zone confirmation and final git status.
