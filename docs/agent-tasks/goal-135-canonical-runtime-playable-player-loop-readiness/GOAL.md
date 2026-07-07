# Goal 135 — Canonical Runtime Playable Player Loop Readiness

## Task ID

`goal-135-canonical-runtime-playable-player-loop-readiness`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Strategic intent

Goal134 crossed the key line from projection-only into canonical runtime execution:

```text
selected candidate package
→ package validation
→ canonical Runtime-owned playthrough
→ save/load/replay proof
→ Unity/player consumes canonical transcript/state summary
```

Goal135 must turn that proof into a reusable player-loop readiness seam. This is still not final release packaging and not scene/prefab production. The goal is to prove that a PlayerAdapter can consume canonical runtime output as the gameplay source of truth and drive a deterministic step-by-step player-facing loop plan.

The player/presentation layer must not become gameplay authority.

## Product-line constraint

Preserve the Goal133A product-line strategy:

- LLMGameCreator is a data-driven game product-line combiner, not prompt-to-game.
- LLM is optional local authoring assistance only.
- Future expansion must keep FeatureModule / RuntimePrimitive / SemanticPack / VisualPartPack / WorldSourceAdapter / PlayerAdapter seams.
- Narrow alpha must be an expansion-safe kernel, not a hardcoded demo.

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

.llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-dashboard.json
.llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-transcript.json
.llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-state-summary.json
.llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-selected-candidate-playthrough-matrix-result.json
.llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/unity-player-canonical-transcript-smoke.json
.llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/selected-candidate-package-validation.json

src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimeSelectedCandidatePlaythroughContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimeSelectedCandidatePlaythroughService.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/CanonicalRuntimeSelectedCandidatePlaythroughModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/CanonicalRuntimeSelectedCandidatePlaythroughArtifactService.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeSelectedCandidateTranscriptAdapter.cs
```

## Why Goal135 exists

Goal134 proved that Unity can parse/consume canonical transcript/state summary, but it is still a smoke consumer. Goal135 must add a concrete player-loop readiness layer:

```text
canonical runtime transcript/state summary
→ player adapter contract
→ deterministic player loop step plan
→ player loop replay/readiness smoke
→ WinForms/VisualWorld status
→ one-click readiness report
```

This is a bridge from canonical runtime proof toward an actual playable player loop.

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-135-canonical-runtime-playable-player-loop-readiness/**
.devflow/artifact-scope/artifact-scope-policy.json

.devflow/scripts/run-canonical-runtime-player-loop-readiness.ps1
.devflow/scripts/run-canonical-runtime-player-loop-readiness.cmd

.llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/**
.llmgc/exports/goal-135-canonical-runtime-playable-player-loop-readiness/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/canonical-runtime-playable-player-loop-readiness.md

src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/**
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal135.cs

unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeSelectedCandidateTranscriptAdapter.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimePlayerLoopReadinessAdapter.cs

tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/**
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**
tests/LLMGameCreator.Tests/Runtime/**
tests/LLMGameCreator.Tests/Runtime.Abstractions/**
tests/LLMGameCreator.Tests/DevFlow/RunCanonicalRuntimePlayerLoopReadinessScriptTests.cs
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

No public GamePackage schema changes. No sample package mutation. No `.llmgc/manual/**`. No provider/media/LLM/Lua/generator-library work. No Unity scene/prefab/settings/package changes.

## Required normal command

Add:

```bat
.devflow\scripts\run-canonical-runtime-player-loop-readiness.cmd
```

The `.cmd` must call the `.ps1` with sane defaults.

PowerShell options:

```text
-CanonicalRuntimeTranscriptPath
-CanonicalRuntimeStateSummaryPath
-CanonicalRuntimeDashboardPath
-OutputRoot
-UnityPath
-DryRun
-ApplyCleanup
```

Defaults:

```text
CanonicalRuntimeTranscriptPath = .llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-transcript.json
CanonicalRuntimeStateSummaryPath = .llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-state-summary.json
CanonicalRuntimeDashboardPath = .llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-dashboard.json
OutputRoot = .llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness
```

The script must:

1. Verify all input paths stay under repo root.
2. Refuse `.llmgc/manual/**`.
3. Refuse writes outside Goal135 output root.
4. Build a PlayerAdapter readiness model from Goal134 canonical transcript/state summary.
5. Run a Unity/player batchmode readiness smoke if Unity is available or through known Unity path discovery.
6. Apply bounded cleanup when `-ApplyCleanup` is used.
7. Write compact result/report artifacts.
8. Return non-zero on failure.

## PlayerAdapter contract deliverable

Add a focused BCL-only player-loop readiness seam. Suggested names; adjust to repository style:

```text
CanonicalRuntimePlayerLoopReadinessRequest
CanonicalRuntimePlayerLoopReadinessResult
CanonicalRuntimePlayerLoopStep
CanonicalRuntimePlayerAdapterContract
CanonicalRuntimePlayerLoopReadinessService
```

Required semantics:

- Input is Goal134 canonical transcript/state summary.
- Output is a deterministic player-loop step plan.
- The plan must be presentation-oriented but must preserve canonical runtime authority.
- It must not execute gameplay.
- It must map runtime events to player-facing steps.
- It must include feature/module coverage hints.

Required step categories, at minimum:

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

If a category cannot be produced from the transcript, fail the readiness result and list the missing category. Do not silently pass.

## Diagnostic severity cleanup

Goal134 artifacts currently may include package diagnostics such as `Warning:asset.path.missing` and `Error:script.path.missing` while the runtime proof passes because the canonical runtime path does not require those script assets.

Goal135 must make this explicit instead of leaving raw `Error:` strings ambiguous.

Add a normalized diagnostic classification artifact:

```text
canonical-runtime-diagnostic-classification.json
```

Required fields:

```text
rawDiagnosticCount
blockingDiagnosticCount
nonBlockingDiagnosticCount
blockingDiagnostics
nonBlockingDiagnostics
passAllowsNonBlockingDiagnostics=true
noUnclassifiedErrorDiagnostics=true
```

A GREEN result must not contain an unclassified raw `Error:` diagnostic in the dashboard/report. If `Error:script.path.missing` is intentionally non-blocking for this runtime/player-loop readiness path, classify it with a reason such as:

```text
nonBlockingForCanonicalRuntimePath=true
reason=script artifact path is not required by the selected canonical runtime command sequence
```

## Unity/player readiness smoke

Add minimal Unity/player batchmode consumer:

- It reads the player-loop plan and canonical state summary.
- It verifies the required step categories and canonical authority markers.
- It does not run gameplay logic.
- It logs:

```text
GOAL135_CANONICAL_RUNTIME_PLAYER_LOOP_READINESS_PASS
```

or:

```text
GOAL135_CANONICAL_RUNTIME_PLAYER_LOOP_READINESS_FAIL
```

This is Tier 4 player adapter readiness, not gameplay truth.

## Required artifacts

Write under both procedural and export roots:

```text
canonical-runtime-player-adapter-contract.json
canonical-runtime-player-loop-plan.json
canonical-runtime-player-loop-readiness-result.json
canonical-runtime-player-loop-readiness-dashboard.json
canonical-runtime-player-loop-readiness-matrix-result.json
canonical-runtime-diagnostic-classification.json
unity-player-loop-readiness-smoke.json
one-click-player-loop-readiness-report.json
one-click-player-loop-readiness-report.md
canonical-runtime-player-loop-negative-proof.json
canonical-runtime-player-loop-file-index.json
```

Raw Unity logs may remain local/ignored if compact smoke artifacts prove pass/fail.

## Required dashboard fields

```text
goalId=goal_135_canonical_runtime_playable_player_loop_readiness
status=GREEN
candidateId=minimal-map-game-balanced-baseline
projectionOnly=false
canonicalRuntimeSource=true
playerAdapterContractPresent=true
playerLoopPlanPresent=true
playerLoopStepCount >= 8
requiredStepCategoriesPresent=true
unityPlayerLoopReadinessPassed=true
unityGameplayTruth=false
manualUnityOptional=true
saveLoadReplayStillReferenced=true
selectedCandidateExecutedByRuntime=true
noUnclassifiedErrorDiagnostics=true
```

## VisualWorld / WinForms proof surface

Add a read-only Goal135 section showing:

```text
candidateId
playerAdapterContractPresent
playerLoopStepCount
requiredStepCategoriesPresent
unityPlayerLoopReadinessPassed
canonicalRuntimeSource
unityGameplayTruth
projectionOnly
noUnclassifiedErrorDiagnostics
normalCommand
reportPath
manualUnityOptional
```

No manual Unity inspection required.

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

Current state must say Goal135 proves canonical runtime player loop readiness.

Required markers:

```text
projectionOnly=false
canonicalRuntimeSource=true
playerAdapterCoverage=true
unityGameplayTruth=false
manualUnityOptional=true
noUnclassifiedErrorDiagnostics=true
```

## Artifact-scope policy

Add scenario:

```text
goal-135-canonical-runtime-playable-player-loop-readiness
```

It must allow only expected Goal135 paths and exclude `.llmgc/manual/**`, samples, GamePackage schema, Generation, AssetPipeline, Scripting, generator-library, provider/media, Unity scenes/prefabs/settings/packages/StreamingAssets, solution/project/dependency files.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~CanonicalRuntimePlayerLoopReadiness|FullyQualifiedName~Goal135|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\run-canonical-runtime-player-loop-readiness.ps1 -DryRun
.\.devflow\scripts\run-canonical-runtime-player-loop-readiness.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-135-canonical-runtime-playable-player-loop-readiness
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

- reads Goal134 canonical transcript/state summary;
- produces player adapter contract;
- produces player loop plan with required categories;
- Unity/player readiness smoke passes;
- diagnostics are classified and no unclassified `Error:` remains in GREEN dashboard/report;
- `projectionOnly=false`;
- `canonicalRuntimeSource=true`;
- `unityGameplayTruth=false`;
- tests/checks pass;
- artifact scope passes;
- no forbidden path changes;
- no `.llmgc/manual/**` tracked/staged;
- final git status clean.

BLOCKED if canonical transcript/state summary is insufficient to build a player-loop readiness plan without faking steps. Write a concrete blocker report and missing category list.

FAILED if build/tests break or forbidden changes are required.

## Commit / push policy

Commit and push with one of:

```text
GREEN Goal 135 canonical runtime playable player loop readiness
BLOCKED Goal 135 canonical runtime playable player loop readiness
FAILED Goal 135 canonical runtime playable player loop readiness
```

Push to `origin/main`.

Final report must include:

- commit SHA;
- candidate id;
- player loop step count;
- required step category result;
- diagnostic classification summary;
- Unity/player readiness smoke result;
- one-click report path;
- forbidden-zone confirmation;
- final git status.
