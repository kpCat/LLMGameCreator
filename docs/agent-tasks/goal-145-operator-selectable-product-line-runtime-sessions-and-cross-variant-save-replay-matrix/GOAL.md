# Goal 145 — Operator-Selectable Product-Line Runtime Sessions + Cross-Variant Save/Replay Matrix

## Identity

- Task: `goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix`
- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Base: `4cf5b37f6c3fa30dd2d87adb6fe466556f9217bf` or a direct descendant

## Product result

Goal142 produced four Runtime-significant candidates. Goal143 delivered the selected candidate to PlayerAdapter. Goal144/144A produced a real Runtime-owned interactive session with exact action binding and deterministic checkpoint/full replay.

Goal145 must remove the remaining active-workflow hardcoding around the exploration candidate.

Implement:

```text
Goal142 candidate artifacts
→ discover/validate every candidate and package SHA
→ execute the same Runtime interactive-session kernel for every candidate
→ checkpoint reload + full replay for every candidate
→ compare fresh Runtime outcomes against baseline
→ prove alchemy/combat/exploration focus effects
→ let the operator select any passing candidate
→ start a real persistent selected-candidate Runtime session
→ WinForms in-process workflow
→ Unity read-only matrix consumer
```

This is product-line Runtime progress, not a projection/report-only goal.

## First deliverable: record Goal144 acceptance

Record exactly:

```text
Я принимаю Goal144 selected_runtime_variant_interactive_action_session_and_save_replay_verification GREEN. selectedCandidate=minimal-map-game-exploration-resource-focus, actionDescriptorCount=14, runtimeRoutedActionDescriptorCount=11, presentationOnlyActionDescriptorCount=3, executedRuntimeActionCount=11, actionDescriptorExecutionBindingPassed=true, harvestTarget=node/apple_tree, basicAttackTarget=goblin, invalidActionStateUnchanged=true, checkpointReloadByReplayPassed=true, checkpointReplayedActionCount=8, finalReplayActionCount=13, replayEvidenceFrozenBeforeContinuation=true, fullReplayEquivalent=true, finalStateHashMatchesGoal142=true, operatorStatus=GREEN, unitySmoke=GREEN, projectionOnly=false, runtimeAuthority=true, unityGameplayTruth=false.
```

Required acceptance fields:

```text
accepted=true
acceptedByHuman=true
acceptedByCodex=false
rawManualInputNotCommitted=true
```

Preserve all values from the decision. Write the record under both Goal145 artifact roots and update:

```text
docs/manual-acceptance/selected-runtime-variant-interactive-action-session-and-save-replay.md
```

Goal145 remains:

```text
accepted=false
acceptedByHuman=false
acceptedByCodex=false
```

Do not track `.llmgc/manual/**`.

## Read first

Read:

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

docs/agent-tasks/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/GOAL.md
docs/agent-tasks/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/GOAL.md
docs/agent-tasks/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/GOAL.md
docs/agent-tasks/goal-144a-live-session-action-target-binding-and-replay-evidence-hotfix/GOAL.md

.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/**
.llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/**
.llmgc/procedural/goal-144a-live-session-action-target-binding-and-replay-evidence-hotfix/**

src/LLMGameCreator.Runtime/SelectedRuntimeVariantInteractiveSessionService.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Runtime.Abstractions/SelectedRuntimeVariantInteractiveSessionContracts.cs
src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Application/Design/SelectedRuntimeVariantInteractiveSession/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal144.cs
```

Inspect actual Goal142 filenames and paths. Do not guess them.

## Candidate discovery

Discover candidates from Goal142 matrix/catalog/scoreboard artifacts, not from a new hardcoded package list.

Current expected candidates:

```text
minimal-map-game-balanced-baseline
minimal-map-game-alchemy-focus
minimal-map-game-combat-focus
minimal-map-game-exploration-resource-focus
```

For every candidate validate:

```text
candidateId
recipeId
variantKind
score
packagePath
packageSha256
Goal142 metadata/path consistency
package file exists
package SHA matches
candidate path stays under Goal142 root
```

Reject:

```text
duplicate candidate ID
duplicate package path
missing package
SHA mismatch
metadata mismatch
path escape
unknown selected candidate
failed selected candidate
```

Use deterministic candidate ordering.

Introduce clear Goal145 terminology:

```text
runtimeEvaluated=true for all executed candidates
runtimeMutated=false for balanced baseline control
runtimeMutated=true for the three focus variants
controlCandidate=true only for baseline
```

Do not rewrite historical Goal142 artifacts.

## Shared Runtime session kernel

Reuse the existing canonical Runtime and Goal144 interactive-session service.

All candidates must use:

```text
same Runtime service type/instance seam
same canonical action-plan builder
same bounded action sequence
same save/replay implementation
```

No candidate-specific Runtime implementations and no Runtime branch on candidate ID.

Required session actions include:

```text
start_runtime
move
interact
inspect_inventory or inspect_status
open_dialogue
start_or_update_quest
show_inventory
craft
harvest
transaction
begin_encounter
basic_attack
show_final_state
```

Preserve Goal144A binding for every Runtime action:

```text
actionId
commandKind
targetId
canonicalStepId
canonicalStepIndex
runtimeCommandStartIndex
runtimeCommandEndIndex
executionTargetId
executionBindingValidated=true
```

Descriptors must come from canonical steps plus package target validation. No `FirstOrDefault()` target fallback and no separate action→range truth.

## Per-candidate drill

For every discovered candidate:

1. Start a fresh Runtime session.
2. Reject one invalid action without mutation, journal change, or cursor advance.
3. Execute the common sequence one action at a time.
4. Execute at least one presentation-only action.
5. Save checkpoint after craft.
6. Execute at least two later Runtime actions.
7. Reload checkpoint by journal replay.
8. Freeze checkpoint replay evidence before continuing.
9. Continue through harvest/transaction/encounter/combat/final state.
10. Full-replay the final journal into a fresh session.
11. Verify correlation, exact action binding, state-hash continuity, checkpoint hash and final hash.
12. Write per-candidate state/catalog/journal/checkpoint/replay artifacts.

Per-candidate result must contain:

```text
candidateId
recipeId
variantKind
score
packagePath
packageSha256
runtimeEvaluated
runtimeMutated
controlCandidate
actionDescriptorCount
runtimeRoutedActionDescriptorCount
presentationOnlyActionDescriptorCount
executedRuntimeActionCount
invalidActionStateUnchanged
actionDescriptorExecutionBindingPassed
checkpointReplayedActionCount
finalReplayActionCount
checkpointStateHashRestored
fullReplayEquivalent
finalStateHash
inventorySummary
questSummary
combatSummary
focusKind
focusEffectObserved
passed
diagnostics[]
```

## Fresh cross-variant proof

Do not copy Goal142 outcome summaries as Goal145 execution evidence.

Required:

```text
candidateCount>=4
passedCandidateCount==candidateCount
failedCandidateCount=0
runtimeEvaluatedCandidateCount==candidateCount
runtimeMutatedCandidateCount>=3
controlCandidateCount>=1
distinctFinalStateHashCount>=4
allCandidatePackageHashesDistinct=true
allCandidateCheckpointReloadsPassed=true
allCandidateFullReplaysEquivalent=true
allCandidateActionBindingsPassed=true
sameRuntimeServiceUsedForAllCandidates=true
sameCanonicalActionPlanUsedForAllCandidates=true
```

Compare fresh Goal145 Runtime results against fresh baseline result.

Required semantic effects:

```text
baseline: controlCandidate=true, runtimeMutated=false
alchemy: crafting/economy result differs semantically from baseline
combat: combat result differs semantically from baseline
exploration_resource: harvest/resource/inventory result differs semantically from baseline
allFocusEffectsObserved=true
```

A different hash alone is insufficient. Record the actual changed quantities/state.

## Operator-selectable active candidate

Default candidate must be resolved from the accepted Goal142 selected-handoff artifact. Do not hardcode exploration ID/path in the Goal145 normal path.

`-SelectedCandidateId` overrides the default.

Create a selection handoff:

```text
selectionId
selectionMode=human_operator
selectedCandidateId
selectedRecipeId
selectedVariantKind
selectedScore
selectedPackagePath
selectedPackageSha256
selectedFinalStateHash
selectedCheckpointHash
selectedComparisonToBaseline
availableCandidateIds[]
candidateMatrixResultPath
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
accepted=false
```

Changing selected candidate must reset the in-memory session and checkpoint. Cross-candidate checkpoint replay must fail.

## WinForms

Add tab:

```text
Goal145 Variant Sessions
```

Show:

```text
candidate list from Goal142 artifacts
candidate ID / variant / score / SHA / pass status
fresh focus comparison against baseline
selected session state and summaries
available actions as:
actionId | target=<id> | step=<canonicalStepId> | route=<route>
checkpoint/replay status
all-candidate matrix status
```

Controls:

```text
Load Candidate Matrix
Start Selected Variant
Execute Selected Action
Save Checkpoint
Reload Checkpoint
Replay Verify
Run All Variant Sessions
```

`Run All Variant Sessions` is the single primary acceptance action.

WinForms must use in-process Application services:

```text
operatorUsesInProcessService=true
operatorStartsCompilerProcess=false
operatorStartsDotnetTestProcess=false
operatorStartsPowerShellProcess=false
```

Disable relevant controls while running. Preserve transactional rollback and the Goal142A self-lock fix.

## Unity

Add read-only window:

```text
LLMGameCreator/Accepted Alpha/Product-Line Runtime Session Matrix
```

Unity reads Goal145 artifacts only. It may browse candidate results but must not execute gameplay, select a winner, mutate selection, or load GamePackage as gameplay truth.

Batch smoke:

```text
candidateCount>=4
passedCandidateCount==candidateCount
distinctFinalStateHashCount>=4
selectedCandidateExists=true
selectedCandidatePackageHashMatches=true
allCandidateCheckpointReloadsPassed=true
allCandidateFullReplaysEquivalent=true
allCandidateActionBindingsPassed=true
allFocusEffectsObserved=true
runtimeAuthority=true
unityGameplayTruth=false
passMarkerPresent=true
failMarkerPresent=false
unityExitCode=0
```

## Artifacts

Write under both:

```text
.llmgc/procedural/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/
.llmgc/exports/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/
```

Required:

```text
goal144-human-acceptance-record.json
product-line-interactive-session-candidate-catalog.json
product-line-interactive-session-matrix-result.json
product-line-interactive-session-comparison.json
product-line-interactive-session-dashboard.json
product-line-interactive-session-negative-proof.json
product-line-interactive-session-selection-handoff.json
product-line-interactive-session-file-index.json
one-click-product-line-interactive-session-report.json
one-click-product-line-interactive-session-report.md
unity-product-line-interactive-session-matrix-smoke.json

candidates/<candidateId>/session-state.json
candidates/<candidateId>/action-catalog.json
candidates/<candidateId>/journal.json
candidates/<candidateId>/checkpoint.json
candidates/<candidateId>/checkpoint-replay-result.json
candidates/<candidateId>/final-replay-result.json
candidates/<candidateId>/focus-effect-proof.json
```

File index includes SHA-256.

## Dashboard

Required:

```text
status=GREEN
productLineInteractiveSessionMatrix=true
candidateCount>=4
passedCandidateCount==candidateCount
failedCandidateCount=0
runtimeEvaluatedCandidateCount==candidateCount
runtimeMutatedCandidateCount>=3
controlCandidateCount>=1
distinctFinalStateHashCount>=4
allCandidatePackageHashesDistinct=true
allCandidateCheckpointReloadsPassed=true
allCandidateFullReplaysEquivalent=true
allCandidateActionBindingsPassed=true
sameRuntimeServiceUsedForAllCandidates=true
sameCanonicalActionPlanUsedForAllCandidates=true
allFocusEffectsObserved=true
operatorSelectableCandidateCount>=4
activeSelectionResolved=true
activeSelectedCandidateExists=true
crossCandidateCheckpointRejected=true
noHardcodedExplorationOnlyPath=true
noBalancedBaselineFallback=true
noGoal131Fallback=true
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
unitySmokePassed=true
goal144Accepted=true
goal145Accepted=false
accepted=false
```

## Negative proof

Prove:

```text
unknownCandidateRejected
failedCandidateSelectionRejected
candidatePackageHashMismatchRejected
candidateMetadataMismatchRejected
candidatePathEscapeRejected
duplicateCandidateIdRejected
duplicatePackagePathRejected
crossCandidateCheckpointRejected
baselineFallbackRejected
goal131FallbackRejected
sampleTemplateFallbackRejected
hardcodedExplorationOnlySelectionRejected
precomputedGoal142OutcomeCannotCountAsGoal145Execution
candidateSpecificRuntimeImplementationAbsent
unityDoesNotExecuteGameplay
winFormsStartsNoCompilerOrTestProcess
previousArtifactsPreservedOnFailure
```

Prefer executable tamper tests over source-text assertions.

## Normal command

Add:

```text
.devflow\scripts\run-product-line-interactive-session-matrix.cmd
.devflow/scripts/run-product-line-interactive-session-matrix.ps1
```

Parameters:

```text
-Goal142Root
-OutputRoot
-SelectedCandidateId
-UnityPath
-DryRun
-ApplyCleanup
```

Empty `SelectedCandidateId` resolves from Goal142 selected handoff.

Script requirements:

```text
repo-root path guards
refuse .llmgc/manual
Goal145-only output roots
Goal142 candidate/package validation
Application matrix proof
Unity batch smoke
second proof requiring Unity smoke
transactional backup/rollback outside repo
non-zero on semantic/hash/replay/Unity failure
```

The external script may call `dotnet test` and Unity. WinForms may not.

## Suggested application layer

Create:

```text
src/LLMGameCreator.Application/Design/ProductLineInteractiveSessionMatrix/
```

Suggested classes:

```text
Goal142CandidateDiscovery
ProductLineInteractiveSessionMatrixValidator
ProductLineInteractiveSessionMatrixService
ProductLineInteractiveSessionMatrixOperatorRunner
ProductLineInteractiveSessionSelectionController
```

Reuse the existing Runtime interactive-session service. Do not create another gameplay runtime.

## Backward compatibility

Goal144/144A must remain GREEN:

```text
exact action binding
harvest target node/apple_tree
basic attack target goblin
checkpoint replay count 8
final replay count 13
final hash d7c04179...
```

Do not modify Goal142/143/144/144A historical artifacts. Only update the Goal144 manual-acceptance document.

## Allowed paths

Only:

```text
docs/agent-tasks/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-product-line-interactive-session-matrix.ps1
.devflow/scripts/run-product-line-interactive-session-matrix.cmd
.llmgc/procedural/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/**
.llmgc/exports/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/selected-runtime-variant-interactive-action-session-and-save-replay.md
docs/manual-acceptance/operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix.md

src/LLMGameCreator.Runtime.Abstractions/SelectedRuntimeVariantInteractiveSessionContracts.cs
src/LLMGameCreator.Runtime/SelectedRuntimeVariantInteractiveSessionService.cs
src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Application/Design/ProductLineInteractiveSessionMatrix/**
src/LLMGameCreator.Application/Design/SelectedRuntimeVariantInteractiveSession/**
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineInteractiveSessionMatrixModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineInteractiveSessionMatrixArtifactService.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineService.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal145.cs

unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityProductLineInteractiveSessionMatrixHarness.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnityProductLineInteractiveSessionMatrixWindow.cs

tests/LLMGameCreator.Tests/Runtime/SelectedRuntimeVariantInteractiveSessionServiceTests.cs
tests/LLMGameCreator.Tests/Runtime/CanonicalRuntimePlayerCommandLoopServiceTests.cs
tests/LLMGameCreator.Tests/Application/ProductLineInteractiveSessionMatrix/**
tests/LLMGameCreator.Tests/Application/SelectedRuntimeVariantInteractiveSession/**
tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/ProductLineInteractiveSessionMatrixScriptProof.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal145Tests.cs
tests/LLMGameCreator.Tests/Devflow/RunProductLineInteractiveSessionMatrixScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden

Do not modify/stage:

```text
.llmgc/manual/**
samples/minimal-map-game/**
Goal142/143/144/144A procedural or export roots
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Generation/**
src/LLMGameCreator.AssetPipeline/**
src/LLMGameCreator.Scripting/**
generator-library/**
provider/**
LLM/**
RAG/**
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

No public GamePackage schema changes, sample mutation, provider/network/LLM/Lua work, candidate-specific Unity gameplay, or new dependency.

## Validation

Run sequentially:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore

dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~ProductLineInteractiveSessionMatrix|FullyQualifiedName~Goal145|FullyQualifiedName~SelectedRuntimeVariantInteractiveSession|FullyQualifiedName~Goal144|FullyQualifiedName~CanonicalRuntimePlayerCommandLoop|FullyQualifiedName~VisualWorldStreamPreviewWorkspace|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection"

.\.devflow\scripts\run-product-line-interactive-session-matrix.ps1 -DryRun
.\.devflow\scripts\run-product-line-interactive-session-matrix.ps1 -ApplyCleanup
.\.devflow\scripts\run-product-line-interactive-session-matrix.ps1 -SelectedCandidateId minimal-map-game-combat-focus -ApplyCleanup
.\.devflow\scripts\run-product-line-interactive-session-matrix.ps1 -ApplyCleanup

.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun

git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Required build: 0 warnings/errors.

Historical validation churn outside Goal145 allowlist must be restored only by exact paths computed from the Goal145 scenario policy. No `git reset --hard`, broad restore, branch switch, merge, rebase, cherry-pick or clean.

Check changed text for mojibake and escaped Cyrillic: zero matches.
Forbidden diff: empty.

## State updates

After GREEN:

```text
goal144Accepted=true
goal144AcceptedByHuman=true
goal144AcceptedByCodex=false
goal145Accepted=false
productLineInteractiveSessionMatrix=true
operatorSelectableCandidateCount>=4
allCandidateRuntimeSessionsPassed=true
allCandidateCheckpointReloadsPassed=true
allCandidateFullReplaysEquivalent=true
allCandidateActionBindingsPassed=true
distinctFinalStateHashCount>=4
allFocusEffectsObserved=true
activeSelectedCandidateId=<resolved Goal142 default>
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
nextProductGoal=review_goal_145_operator_selectable_product_line_runtime_sessions
```

Do not mark Goal141 accepted.

## Publish

Stage only explicit Goal145 allowlisted paths.

Commit:

```text
GREEN Goal 145 operator-selectable product-line runtime sessions and cross-variant save replay matrix
```

Push `origin main`.

Final report must include commit SHA, Goal144 acceptance, candidate/pass counts, default selection, explicit combat-selection test, distinct hashes, per-focus differences, replay/action-binding status for all candidates, WinForms/Unity status, test counts, scope, forbidden diff and clean `HEAD == origin/main`.

Do not report GREEN if any candidate, focus effect, replay, action binding, Unity smoke or scope check fails.
