# Goal 136 — Canonical Runtime Player Command Loop Execution Matrix

## Task ID

`goal-136-canonical-runtime-player-command-loop-execution-matrix`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Strategic intent

Goal134 proved a selected candidate can run through canonical runtime playthrough, save/load/replay and Unity/player transcript consumption.

Goal135 proved a PlayerAdapter readiness plan and diagnostic classification over canonical runtime output.

Goal136 must move from readiness to an actual canonical runtime player command loop:

```text
selected candidate package
→ canonical runtime session
→ PlayerAdapter input commands
→ runtime-owned command execution
→ per-step player-facing snapshots
→ command-loop matrix
→ Unity/player consumes snapshots as presentation data
→ one-click report
```

This must not be another projection-only wrapper. Runtime remains gameplay truth. Unity/player consumes snapshots/state summaries and verifies presentation readiness only.

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

.llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-dashboard.json
.llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-transcript.json
.llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-state-summary.json
.llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-player-loop-readiness-dashboard.json
.llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-player-loop-plan.json
.llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-player-adapter-contract.json
.llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-diagnostic-classification.json
.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/package.json
.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/selected-candidate-handoff.json

src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimeSelectedCandidatePlaythroughContracts.cs
src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerLoopReadinessContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimeSelectedCandidatePlaythroughService.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerLoopReadinessService.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimePlayerLoopReadinessAdapter.cs
```

## Goal boundary

This goal may extend Runtime and Runtime.Abstractions with a focused player command-loop seam. It may not change public GamePackage schema.

Goal135 accepted status must remain `accepted=false` unless the owner explicitly provides human acceptance. Do not fake manual acceptance.

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-136-canonical-runtime-player-command-loop-execution-matrix/**
.devflow/artifact-scope/artifact-scope-policy.json

.devflow/scripts/run-canonical-runtime-player-command-loop.cmd
.devflow/scripts/run-canonical-runtime-player-command-loop.ps1

.llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/**
.llmgc/exports/goal-136-canonical-runtime-player-command-loop-execution-matrix/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/canonical-runtime-player-command-loop-execution-matrix.md

src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/CanonicalRuntimePlayerCommandLoopModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/CanonicalRuntimePlayerCommandLoopArtifactService.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineService.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal136.cs

unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimePlayerCommandLoopAdapter.cs

tests/LLMGameCreator.Tests/Runtime/CanonicalRuntimePlayerCommandLoopServiceTests.cs
tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/CanonicalRuntimePlayerCommandLoopScriptRuntimeProof.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal136Tests.cs
tests/LLMGameCreator.Tests/DevFlow/RunCanonicalRuntimePlayerCommandLoopScriptTests.cs
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

No public GamePackage schema changes. No sample package mutation. No `.llmgc/manual/**`. No provider/media/LLM/Lua/generator-library work.

Unity changes are limited to a player/presentation consumer of canonical command-loop snapshots. Unity must not execute gameplay truth.

## Required normal command

Add:

```bat
.devflow\scripts\run-canonical-runtime-player-command-loop.cmd
```

The `.cmd` must call the `.ps1` with `-ApplyCleanup` by default.

The `.ps1` should support:

```text
-SelectedCandidateHandoffPath
-SelectedCandidatePackagePath
-Goal134TranscriptPath
-Goal134StateSummaryPath
-Goal135PlayerLoopPlanPath
-Goal135PlayerAdapterContractPath
-OutputRoot
-UnityPath
-DryRun
-ApplyCleanup
```

Defaults should point to Goal131 selected candidate, Goal134 canonical runtime artifacts and Goal135 readiness artifacts.

The script must:

1. Validate all input paths stay under repo root.
2. Refuse `.llmgc/manual/**`.
3. Refuse writing outside Goal136 output root.
4. Run a runtime-owned player command-loop proof.
5. Write per-step player-facing snapshots.
6. Build a command-loop matrix result.
7. Run Unity/player command-loop snapshot smoke if Unity is available through existing path discovery.
8. Apply bounded Unity cleanup when `-ApplyCleanup` is supplied.
9. Return non-zero on failure.

## Runtime deliverable

Add a focused canonical runtime player command-loop service.

Minimum shape:

```text
CanonicalRuntimePlayerCommandLoopRequest
CanonicalRuntimePlayerCommandLoopInput
CanonicalRuntimePlayerCommandLoopStep
CanonicalRuntimePlayerCommandLoopSnapshot
CanonicalRuntimePlayerCommandLoopResult
ICanonicalRuntimePlayerCommandLoopService
CanonicalRuntimePlayerCommandLoopService
```

Requirements:

- Runtime owns command execution.
- Runtime starts or receives canonical runtime session derived from selected candidate package.
- Player input commands are deterministic and adapter-friendly.
- Every player command produces a player-facing snapshot.
- Snapshot includes enough data for player presentation:
  - step index/id/category;
  - command label;
  - state hash before/after;
  - map/player position or map summary;
  - visible interaction/dialogue/quest/inventory/combat/diagnostic summary;
  - runtime events emitted by the step.
- No LLM/provider/network/Unity calls.
- `projectionOnly=false`.
- `unityGameplayTruth=false`.

The command-loop can reuse Goal134/135 scripts, but it must execute through Runtime-owned services and produce snapshots from Runtime output, not from projection-local fake state.

## Required command-loop categories

The command loop must include at least these categories:

```text
load_package
start_runtime
move
interact
show_dialogue
start_or_update_quest
show_inventory
craft
harvest
transaction
encounter
combat_round
final_state
```

Minimum pass thresholds:

```text
playerCommandCount >= 10
playerSnapshotCount == playerCommandCount
runtimeEventCount >= 10
stateHashChainPresent = true
allRequiredCategoriesPresent = true
selectedCandidateExecutedByRuntime = true
projectionOnly = false
unityGameplayTruth = false
```

If a runtime primitive is missing, report it explicitly as `runtimePrimitiveMissing=true` and list it. Do not hide missing primitives.

## Matrix deliverable

Write:

```text
canonical-runtime-player-command-loop-matrix-result.json
```

Minimum one row for selected candidate:

```text
candidateId
packagePath
playerCommandLoopPassed
playerCommandCount
snapshotCount
runtimeEventCount
allRequiredCategoriesPresent
unityPlayerConsumedCommandLoopSnapshots
passed
```

Keep the data shape ready for more candidates later.

## Unity/player command-loop snapshot consumer

Add a Unity batchmode consumer:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimePlayerCommandLoopAdapter.cs
```

It must read Goal136 command-loop snapshot/result artifacts and verify the presentation-side requirements.

It must log:

```text
GOAL136_CANONICAL_RUNTIME_PLAYER_COMMAND_LOOP_PASS
```

or:

```text
GOAL136_CANONICAL_RUNTIME_PLAYER_COMMAND_LOOP_FAIL
```

Unity must not execute gameplay truth. It only consumes snapshots produced by canonical runtime.

## Diagnostics

Reuse the Goal135 diagnostic classification policy.

GREEN dashboard/report must not contain unclassified raw `Error:` diagnostics.

Required fields:

```text
rawDiagnosticCount
blockingDiagnosticCount
nonBlockingDiagnosticCount
noUnclassifiedErrorDiagnostics
```

## Required artifacts

Under both procedural and export roots:

```text
canonical-runtime-player-command-loop-dashboard.json
canonical-runtime-player-command-loop-inputs.json
canonical-runtime-player-command-loop-plan.json
canonical-runtime-player-command-loop-snapshots.json
canonical-runtime-player-command-loop-result.json
canonical-runtime-player-command-loop-matrix-result.json
canonical-runtime-player-command-loop-diagnostic-classification.json
unity-player-command-loop-smoke.json
one-click-player-command-loop-report.json
one-click-player-command-loop-report.md
canonical-runtime-player-command-loop-negative-proof.json
canonical-runtime-player-command-loop-file-index.json
```

Raw Unity `.log` files may remain local/ignored if compact smoke artifacts prove pass/fail.

## VisualWorld / WinForms proof surface

Add a read-only Goal136 section showing:

```text
candidateId
playerCommandLoopPassed
playerCommandCount
snapshotCount
runtimeEventCount
allRequiredCategoriesPresent
unityPlayerConsumedCommandLoopSnapshots
projectionOnly
unityGameplayTruth
noUnclassifiedErrorDiagnostics
normalCommand
reportPath
matrixResultPath
manualUnityOptional
accepted
```

Do not require manual Unity inspection.

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

Required current-state markers:

```text
gate_status=canonical_runtime_player_command_loop_execution_matrix_verification
accepted=false
projectionOnly=false
canonicalRuntimeSource=true
playerCommandLoopCoverage=true
playerAdapterCoverage=true
unityGameplayTruth=false
unityConsumesRuntimeSnapshots=true
selectedCandidateExecutedByRuntime=true
manualUnityOptional=true
```

## Artifact-scope policy

Add scenario:

```text
goal-136-canonical-runtime-player-command-loop-execution-matrix
```

It must allow only expected Goal136 paths and exclude `.llmgc/manual/**`, samples, GamePackage schema, Generation, AssetPipeline, Scripting, generator-library, provider/media, Unity scenes/prefabs/settings/packages/StreamingAssets, solution/project/dependency files.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~CanonicalRuntimePlayerCommandLoop|FullyQualifiedName~Goal136|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\run-canonical-runtime-player-command-loop.ps1 -DryRun
.\.devflow\scripts\run-canonical-runtime-player-command-loop.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-136-canonical-runtime-player-command-loop-execution-matrix
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

- Runtime-owned player command-loop service exists.
- Selected candidate is executed by runtime.
- Player command-loop output snapshots are written.
- All required categories are present.
- Command-loop matrix passes selected candidate row.
- Unity/player consumes command-loop snapshots and logs pass marker.
- `projectionOnly=false`.
- `unityGameplayTruth=false`.
- `noUnclassifiedErrorDiagnostics=true`.
- Tests/checks pass.
- Artifact scope passes.
- No forbidden path changes.
- No `.llmgc/manual/**` tracked/staged.
- Final git status clean.

BLOCKED if existing runtime services cannot support an actual command-loop without public schema changes. In that case, write a blocker report with missing primitives.

FAILED if build/tests break or forbidden changes are required.

## Commit / push policy

Commit and push with one of:

```text
GREEN Goal 136 canonical runtime player command loop execution matrix
BLOCKED Goal 136 canonical runtime player command loop execution matrix
FAILED Goal 136 canonical runtime player command loop execution matrix
```

Push to `origin/main`.

Final report must include:

- commit SHA;
- selected candidate id;
- command count;
- snapshot count;
- runtime event count;
- Unity/player command-loop smoke result;
- one-click report path;
- forbidden-zone confirmation;
- final git status.
