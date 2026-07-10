# Goal 143 — Selected Runtime Variant End-to-End PlayerAdapter Handoff

## Task ID

`goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive product goal.

Goal142 proved four runtime-significant product-line variants and selected:

```text
minimal-map-game-exploration-resource-focus
```

Goal143 must remove the remaining hardcoded balanced-baseline seam from the active player-adapter path and carry the actual Goal142 selected handoff end to end:

```text
Goal142 selected-runtime-variant handoff
→ selected package integrity validation
→ corrected Runtime request/response execution
→ selected-variant PlayerAdapter model/frames
→ WinForms operator surface
→ Unity/player read-only consumer
→ batchmode smoke
```

Runtime remains gameplay truth. Unity remains a PlayerAdapter/HUD/control consumer.

## Required first deliverable — record Goal142 human acceptance

Record the user's exact decision as bounded evidence:

```text
Я принимаю Goal142 runtime_significant_product_line_variant_matrix_and_selection_handoff_verification GREEN. candidateCount=4, passedCandidateCount=4, runtimeSignificantCandidateCount=4, distinctFinalStateHashCount=4, selectedCandidate=minimal-map-game-exploration-resource-focus, selectedScore=100, sourceTemplateUnmodified=true, operatorUsesInProcessService=true, operatorExitCode=0, projectionOnly=false, runtimeAuthority=true.
```

Required fields:

```text
accepted=true
acceptedByHuman=true
acceptedByCodex=false
rawManualInputNotCommitted=true
candidateCount=4
passedCandidateCount=4
runtimeSignificantCandidateCount=4
distinctFinalStateHashCount=4
selectedCandidate=minimal-map-game-exploration-resource-focus
selectedScore=100
sourceTemplateUnmodified=true
operatorUsesInProcessService=true
operatorExitCode=0
projectionOnly=false
runtimeAuthority=true
```

Write under Goal143 procedural/export roots and update:

```text
docs/manual-acceptance/runtime-significant-product-line-variant-matrix-and-selection-handoff.md
```

Goal143 itself must remain `accepted=false`.

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

docs/agent-tasks/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/GOAL.md
docs/agent-tasks/goal-142a-winforms-operator-self-lock-and-atomic-regeneration-hotfix/GOAL.md

.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/selected-runtime-variant/selected-runtime-variant-handoff.json
.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/selected-runtime-variant/package.json
.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/selected-runtime-variant/runtime-outcome-summary.json
.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/product-line-runtime-variant-matrix-result.json
.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/product-line-runtime-variant-scoreboard.json
.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/matrix/minimal-map-game-exploration-resource-focus/roundtrip-result.json

src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantMatrixOperatorRunner.cs
src/LLMGameCreator.Runtime/RuntimeBackedPlayerCommandRoundtripService.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Runtime.Abstractions/RuntimeBackedPlayerCommandRoundtripContracts.cs

unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityPlayerCommandRoundtripHarness.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnityPlayerCommandRoundtripWindow.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/**
.devflow/artifact-scope/artifact-scope-policy.json

.devflow/scripts/run-selected-runtime-variant-playeradapter-handoff.ps1
.devflow/scripts/run-selected-runtime-variant-playeradapter-handoff.cmd

.llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/**
.llmgc/exports/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/runtime-significant-product-line-variant-matrix-and-selection-handoff.md
docs/manual-acceptance/selected-runtime-variant-end-to-end-playeradapter-handoff.md

src/LLMGameCreator.Application/Design/SelectedRuntimeVariantPlayerAdapter/**
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/SelectedRuntimeVariantPlayerAdapterModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/SelectedRuntimeVariantPlayerAdapterArtifactService.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineService.cs

src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal143.cs

unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnitySelectedVariantPlayerAdapterHarness.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnitySelectedVariantPlayerAdapterWindow.cs

src/LLMGameCreator.Runtime.Abstractions/RuntimeBackedPlayerCommandRoundtripContracts.cs
src/LLMGameCreator.Runtime/RuntimeBackedPlayerCommandRoundtripService.cs
src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs

tests/LLMGameCreator.Tests/Application/SelectedRuntimeVariantPlayerAdapter/**
tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/SelectedRuntimeVariantPlayerAdapterScriptProof.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal143Tests.cs
tests/LLMGameCreator.Tests/Devflow/RunSelectedRuntimeVariantPlayerAdapterHandoffScriptTests.cs
tests/LLMGameCreator.Tests/Runtime/RuntimeBackedPlayerCommandRoundtripServiceTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

Runtime files may be changed only if bounded parameterization is required to execute the selected package. Do not add a parallel runtime.

## Forbidden paths

Do not modify, stage, or commit:

```text
.llmgc/manual/**
samples/minimal-map-game/**
.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/**
.llmgc/exports/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/**
.llmgc/procedural/goal-142a-winforms-operator-self-lock-and-atomic-regeneration-hotfix/**
.llmgc/exports/goal-142a-winforms-operator-self-lock-and-atomic-regeneration-hotfix/**

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

No public GamePackage schema change. No sample mutation. Do not overwrite Goal142 historical evidence.

## Required normal command

Add:

```bat
.devflow\scripts\run-selected-runtime-variant-playeradapter-handoff.cmd
```

PowerShell script:

```text
.devflow/scripts/run-selected-runtime-variant-playeradapter-handoff.ps1
```

Supported parameters:

```text
-SelectedHandoffPath
-SelectedPackagePath
-SelectedOutcomePath
-SelectedRoundtripResultPath
-OutputRoot
-UnityPath
-DryRun
-ApplyCleanup
```

Defaults must point to the Goal142 selected-runtime-variant artifacts and selected matrix roundtrip result.

Script requirements:

1. Validate all input/output paths stay under repository root.
2. Refuse `.llmgc/manual/**`.
3. Refuse writing outside Goal143 output root.
4. Hash and validate the selected package against Goal142 handoff.
5. Execute/build the selected-variant PlayerAdapter handoff proof.
6. Run Unity batchmode consumer smoke.
7. Write procedural/export artifacts and report.
8. Apply bounded Unity cleanup when requested.
9. Return non-zero on mismatch or smoke failure.
10. Use transactional output replacement or rollback; do not destroy previous GREEN artifacts on failure.

The external script may use `dotnet test` for automation. Any WinForms button added in this goal must use an in-process Application service and must not launch PowerShell, compiler, or test child processes.

## Selected handoff integrity

The Goal142 selected handoff is the authoritative product-line selection input.

Required assertions:

```text
selectedCandidateId=minimal-map-game-exploration-resource-focus
selectedRecipeId=exploration_resource_focus
selectedVariantKind=exploration_resource_focus
selectedScore=100
selectedHandoffAccepted=false
selectedRuntimeSignificant=true
selectedProjectionOnly=false
selectedRuntimeAuthority=true
selectedPackageExists=true
selectedPackageSha256MatchesHandoff=true
selectedRoundtripResultExists=true
selectedOutcomeExists=true
selectedOutcomeCandidateMatches=true
selectedRoundtripCandidateMatches=true
selectedFinalStateHashMatches=true
```

Do not silently fall back to:

```text
minimal-map-game-balanced-baseline
Goal131 selected-candidate package
samples/minimal-map-game/package.json
```

Fail if selected-handoff/package/result paths disagree.

## End-to-end Application service

Implement a BCL-only service in Application code.

Recommended files:

```text
src/LLMGameCreator.Application/Design/SelectedRuntimeVariantPlayerAdapter/SelectedRuntimeVariantPlayerAdapterService.cs
src/LLMGameCreator.Application/Design/SelectedRuntimeVariantPlayerAdapter/SelectedRuntimeVariantPlayerAdapterValidator.cs
src/LLMGameCreator.Application/Design/SelectedRuntimeVariantPlayerAdapter/SelectedRuntimeVariantPlayerAdapterOperatorRunner.cs
```

Required flow:

1. Read Goal142 selected handoff.
2. Validate candidate/recipe/variant/score and all paths.
3. Recompute selected package SHA-256 and compare with handoff.
4. Deserialize the selected package.
5. Execute the corrected Runtime roundtrip against this selected package, or validate/reuse the Goal142 selected roundtrip result and rerun to prove determinism.
6. Require the rerun final state hash to match Goal142 selected outcome/hash.
7. Build a PlayerAdapter model and ordered frames from the request-correlated Runtime responses/snapshots.
8. Preserve Runtime authority markers.
9. Write compact handoff/result artifacts.

No gameplay execution in WinForms or Unity.

## PlayerAdapter model and frames

Produce an ordered model from the selected variant Runtime results.

Required fields:

```text
candidateId
recipeId
variantKind
score
packagePath
packageSha256
finalStateHash
frameCount
requestCount
snapshotCount
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
sourceGoal142Handoff=true
```

Each frame must include at least:

```text
frameIndex
humanFrameNumber
requestId
requestIndex
controlIntent
route
requestedOperation
canonicalStepIndex
canonicalStepId
stateHashBefore
stateHashAfter
mapSummary
inventorySummary
questSummary
combatSummary
runtimeExecuted
runtimeMutation
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
```

Requirements:

```text
frameCount >= 6
requestCount=6
runtimeRoutedRequestCount=4
presentationOnlyRequestCount=2
presentationOnlyRuntimeExecutionCount=0
requestResponseCorrelationPassed=true
sequentialCursorContinuityPassed=true
stateHashContinuityPassed=true
selectedVariantEffectVisible=true
noBalancedBaselineFallback=true
```

`selectedVariantEffectVisible=true` must be derived from the selected Goal142 outcome/model comparison, not merely from package hash inequality.

## Selected PlayerAdapter handoff

Write:

```text
selected-runtime-variant-playeradapter-handoff.json
```

Required fields:

```text
candidateId=minimal-map-game-exploration-resource-focus
recipeId=exploration_resource_focus
variantKind=exploration_resource_focus
score=100
sourceSelectedHandoffPath
sourcePackagePath
sourcePackageSha256
sourceRoundtripResultPath
sourceOutcomePath
playerAdapterModelPath
playerAdapterFramesPath
playerAdapterResultPath
finalStateHash
selectedPackageSha256MatchesHandoff=true
selectedFinalStateHashMatches=true
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
accepted=false
```

## Unity/player consumer

Add:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnitySelectedVariantPlayerAdapterHarness.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnitySelectedVariantPlayerAdapterWindow.cs
```

Menu path:

```text
LLMGameCreator/Accepted Alpha/Selected Runtime Variant PlayerAdapter
```

The Unity window is read-only over Goal143 model/frames. It must show:

```text
Gameplay truth: Runtime
Unity mode: Selected PlayerAdapter consumer only
Candidate
Variant
Score
Package SHA-256 status
Final state hash status
Frame 1/N
Control intent / route
Canonical step
Inventory summary
Quest summary
Combat summary
Previous / Next / Reload
```

Unity must not execute gameplay or mutate package/runtime state.

Batchmode smoke must validate:

```text
modelPathExists=true
framesPathExists=true
candidateIsGoal142Selection=true
selectedPackageSha256MatchesHandoff=true
selectedFinalStateHashMatches=true
frameCountPassed=true
selectedVariantEffectVisible=true
noBalancedBaselineFallback=true
runtimeAuthorityMarkersPresent=true
unityConsumesSelectedVariantPlayerAdapter=true
unityGameplayTruth=false
passMarkerPresent=true
failMarkerPresent=false
```

Pass marker:

```text
GOAL143_SELECTED_RUNTIME_VARIANT_PLAYERADAPTER_PASS
```

Fail marker:

```text
GOAL143_SELECTED_RUNTIME_VARIANT_PLAYERADAPTER_FAIL
```

## WinForms / VisualWorld surface

Add a Goal143 tab/section showing:

```text
status
selectedCandidateId
selectedVariantKind
selectedScore
packageHashMatch
finalStateHashMatch
frameCount
selectedVariantEffectVisible
noBalancedBaselineFallback
unitySmokePassed
runtimeAuthority
projectionOnly
unityGameplayTruth
normalCommand
handoffPath
accepted
```

Add one primary button:

```text
Build Selected Variant PlayerAdapter
```

The button must:

- call an in-process Application operator runner;
- use `Task.Run` or another safe asynchronous pattern;
- disable while running;
- use transactional output regeneration/rollback;
- refresh workspace after success;
- capture a bounded diagnostic summary;
- never start `Process`, PowerShell, compiler, `dotnet build`, or `dotnet test`.

## Required artifacts

Under both Goal143 procedural and export roots:

```text
goal142-human-acceptance-record.json
selected-runtime-variant-playeradapter-handoff.json
selected-runtime-variant-playeradapter-model.json
selected-runtime-variant-playeradapter-frames.json
selected-runtime-variant-playeradapter-result.json
selected-runtime-variant-playeradapter-dashboard.json
selected-runtime-variant-playeradapter-negative-proof.json
selected-runtime-variant-playeradapter-file-index.json
unity-selected-runtime-variant-playeradapter-smoke.json
one-click-selected-runtime-variant-playeradapter-report.json
one-click-selected-runtime-variant-playeradapter-report.md
```

## Negative proof

Must explicitly prove:

```text
noBalancedBaselineFallback=true
noGoal131SelectedCandidateFallback=true
noSampleTemplateFallback=true
selectedCandidateMatchesGoal142Handoff=true
selectedPackageHashMismatchRejected=true
selectedFinalStateHashMismatchRejected=true
presentationOnlyControlsStillDoNotExecuteRuntime=true
unityDoesNotExecuteGameplay=true
winFormsStartsNoCompilerOrTestProcess=true
previousArtifactsPreservedOnFailure=true
```

## Current state

Update state/docs with:

```text
goal142Accepted=true
goal143Accepted=false
selectedRuntimeVariantPlayerAdapterHandoff=true
selectedRuntimeVariantId=minimal-map-game-exploration-resource-focus
selectedRuntimeVariantKind=exploration_resource_focus
selectedRuntimeVariantScore=100
selectedPackageSha256MatchesHandoff=true
selectedFinalStateHashMatches=true
selectedVariantEffectVisible=true
noBalancedBaselineFallback=true
unityConsumesSelectedRuntimeVariantPlayerAdapter=true
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
manualUnityOptional=true
```

Do not fabricate Goal141 acceptance. Goal141 remains unaccepted unless an explicit human acceptance is provided later.

## Artifact-scope scenario

Add:

```text
goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff
```

## Validation

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~SelectedRuntimeVariantPlayerAdapter|FullyQualifiedName~Goal143|FullyQualifiedName~RuntimeBackedPlayerCommandRoundtrip|FullyQualifiedName~VisualWorldStreamPreviewWorkspace|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection"
.\.devflow\scripts\run-selected-runtime-variant-playeradapter-handoff.ps1 -DryRun
.\.devflow\scripts\run-selected-runtime-variant-playeradapter-handoff.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Forbidden diff:

```powershell
git diff --name-only -- samples/minimal-map-game .llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff .llmgc/exports/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff .llmgc/procedural/goal-142a-winforms-operator-self-lock-and-atomic-regeneration-hotfix .llmgc/exports/goal-142a-winforms-operator-self-lock-and-atomic-regeneration-hotfix src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity/LLMGameCreatorAlpha/Assets/Scenes unity/LLMGameCreatorAlpha/Assets/Prefabs unity/LLMGameCreatorAlpha/Assets/StreamingAssets unity/LLMGameCreatorAlpha/ProjectSettings unity/LLMGameCreatorAlpha/Packages
```

Also check changed files for mojibake and escaped Cyrillic markers.

## Quality gate

GREEN requires:

- Goal142 human acceptance recorded;
- Goal143 remains accepted=false;
- selected candidate comes only from Goal142 handoff;
- package SHA matches Goal142 handoff;
- rerun/final Runtime state hash matches Goal142 selected outcome;
- corrected request/response semantics remain true;
- selected variant-specific runtime effect is visible;
- no balanced-baseline/Goal131/sample fallback;
- PlayerAdapter model/frames are built from selected Runtime results;
- Unity batchmode consumer smoke passes;
- WinForms operator uses in-process service and transactional regeneration;
- no forbidden changes;
- no `.llmgc/manual/**` tracked/staged;
- final git status clean.

BLOCKED if Goal142 selected handoff/package/result are internally inconsistent.

FAILED if the implementation silently falls back to balanced baseline, mutates Goal142 historical evidence, lets Unity/WinForms execute gameplay truth, or requires forbidden changes.

## Commit / push policy

Commit and push with one of:

```text
GREEN Goal 143 selected runtime variant end-to-end PlayerAdapter handoff
BLOCKED Goal 143 selected runtime variant end-to-end PlayerAdapter handoff
FAILED Goal 143 selected runtime variant end-to-end PlayerAdapter handoff
```

Final report must include:

- commit SHA;
- Goal142 acceptance status;
- selected candidate/variant/score;
- package hash match;
- final state hash match;
- frame/request/snapshot counts;
- selected variant effect proof;
- no-fallback proof;
- Unity smoke result;
- in-process WinForms operator confirmation;
- forbidden-zone confirmation;
- final git status.
