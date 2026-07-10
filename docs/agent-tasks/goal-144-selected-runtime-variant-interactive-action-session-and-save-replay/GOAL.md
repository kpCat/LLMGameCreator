# Goal 144 — Selected Runtime Variant Interactive Action Session + Save/Replay

## Task ID

`goal-144-selected-runtime-variant-interactive-action-session-and-save-replay`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

Aggressive product goal.

Goal143 proved that the selected Goal142 runtime-significant variant reaches PlayerAdapter/Unity without falling back to the old balanced baseline. Goal144 must stop treating that handoff as only a precomputed frame sequence and create a real stateful interactive session over the selected package.

This is still a bounded narrow-alpha session, not final gameplay and not a public schema expansion.

## Strategic result

Implement this path:

```text
Goal142 selected runtime variant handoff
→ selected package/hash validation
→ start persistent Runtime-owned session
→ derive bounded available actions from package + current Runtime state
→ user/operator chooses one action
→ Runtime executes exactly that action
→ correlated response + updated state/summaries
→ save checkpoint as journal + expected hash
→ reload by deterministic replay
→ verify checkpoint and final replay equivalence
→ WinForms interactive operator
→ Unity read-only live-session consumer
```

Runtime remains gameplay truth. WinForms and Unity remain adapters.

## Required first deliverable — record Goal143 human acceptance

Record exactly this user decision:

```text
Я принимаю Goal143 selected_runtime_variant_end_to_end_playeradapter_handoff_verification GREEN. selectedCandidate=minimal-map-game-exploration-resource-focus, selectedVariant=exploration_resource_focus, selectedScore=100, packageHashMatch=true, finalStateHashMatch=true, requestCount=6, snapshotCount=15, frameCount=15, selectedVariantEffectVisible=true, noBalancedBaselineFallback=true, operatorUsesInProcessService=true, operatorStatus=GREEN, unitySmoke=GREEN, projectionOnly=false, runtimeAuthority=true, unityGameplayTruth=false.
```

Write bounded evidence under both Goal144 roots and update the Goal143 manual-acceptance document.

Required fields:

```text
accepted=true
acceptedByHuman=true
acceptedByCodex=false
rawManualInputNotCommitted=true
selectedCandidate=minimal-map-game-exploration-resource-focus
selectedVariant=exploration_resource_focus
selectedScore=100
packageHashMatch=true
finalStateHashMatch=true
requestCount=6
snapshotCount=15
frameCount=15
selectedVariantEffectVisible=true
noBalancedBaselineFallback=true
operatorUsesInProcessService=true
operatorStatus=GREEN
unitySmoke=GREEN
projectionOnly=false
runtimeAuthority=true
unityGameplayTruth=false
```

Goal144 itself must remain `accepted=false`.

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

docs/agent-tasks/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/GOAL.md

.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/selected-runtime-variant/selected-runtime-variant-handoff.json
.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/selected-runtime-variant/package.json
.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/selected-runtime-variant/runtime-outcome-summary.json

.llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/selected-runtime-variant-playeradapter-handoff.json
.llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/selected-runtime-variant-playeradapter-model.json
.llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/selected-runtime-variant-playeradapter-frames.json
.llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/selected-runtime-variant-playeradapter-result.json

src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Runtime/RuntimeBackedPlayerCommandRoundtripService.cs
src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime.Abstractions/RuntimeBackedPlayerCommandRoundtripContracts.cs

src/LLMGameCreator.Application/Design/SelectedRuntimeVariantPlayerAdapter/SelectedRuntimeVariantPlayerAdapterValidator.cs
src/LLMGameCreator.Application/Design/SelectedRuntimeVariantPlayerAdapter/SelectedRuntimeVariantPlayerAdapterService.cs
src/LLMGameCreator.Application/Design/SelectedRuntimeVariantPlayerAdapter/SelectedRuntimeVariantPlayerAdapterOperatorRunner.cs

src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal143.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnitySelectedVariantPlayerAdapterHarness.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnitySelectedVariantPlayerAdapterWindow.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/**
.devflow/artifact-scope/artifact-scope-policy.json

.devflow/scripts/run-selected-runtime-variant-live-session.ps1
.devflow/scripts/run-selected-runtime-variant-live-session.cmd

.llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/**
.llmgc/exports/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/selected-runtime-variant-end-to-end-playeradapter-handoff.md
docs/manual-acceptance/selected-runtime-variant-interactive-action-session-and-save-replay.md

src/LLMGameCreator.Runtime.Abstractions/SelectedRuntimeVariantInteractiveSessionContracts.cs
src/LLMGameCreator.Runtime/SelectedRuntimeVariantInteractiveSessionService.cs
src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs

src/LLMGameCreator.Application/Design/SelectedRuntimeVariantInteractiveSession/**
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/SelectedRuntimeVariantInteractiveSessionModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/SelectedRuntimeVariantInteractiveSessionArtifactService.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineService.cs

src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal144.cs

unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnitySelectedVariantLiveSessionHarness.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnitySelectedVariantLiveSessionWindow.cs

tests/LLMGameCreator.Tests/Runtime/SelectedRuntimeVariantInteractiveSessionServiceTests.cs
tests/LLMGameCreator.Tests/Application/SelectedRuntimeVariantInteractiveSession/**
tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/SelectedRuntimeVariantInteractiveSessionScriptProof.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal144Tests.cs
tests/LLMGameCreator.Tests/Devflow/RunSelectedRuntimeVariantLiveSessionScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

A bounded modification to the canonical command-loop contracts/service is allowed only when needed to expose safe incremental action/session primitives. Do not create a second gameplay runtime.

## Forbidden paths

Do not modify, stage, or commit:

```text
.llmgc/manual/**
samples/minimal-map-game/**
.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/**
.llmgc/exports/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/**
.llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/**
.llmgc/exports/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/**

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

No public GamePackage schema changes. No sample mutation. No Goal142/143 historical artifact rewriting. No provider/network/LLM/Lua work. Unity must not execute gameplay truth.

## Required normal command

Add:

```bat
.devflow\scripts\run-selected-runtime-variant-live-session.cmd
```

PowerShell:

```text
.devflow/scripts/run-selected-runtime-variant-live-session.ps1
```

Supported parameters:

```text
-SelectedHandoffPath
-SelectedPackagePath
-SelectedOutcomePath
-Goal143HandoffPath
-OutputRoot
-UnityPath
-DryRun
-ApplyCleanup
```

Defaults must point to the accepted Goal142 selected variant and Goal143 handoff.

The script must:

1. Validate all inputs stay under repository root.
2. Refuse `.llmgc/manual/**`.
3. Refuse writing outside the Goal144 roots.
4. Validate Goal142/Goal143 candidate, package SHA and final-state hash.
5. Run the deterministic interactive-session acceptance drill.
6. Run Unity read-only live-session smoke.
7. Write compact procedural/export artifacts.
8. Use transactional backup/rollback outside the repository.
9. Return non-zero on any failed semantic/hash/replay/Unity check.

## Runtime-owned interactive session

Add a single persistent Runtime-owned session service for the selected package.

The service must not merely index Goal143 frames. It must execute actual Runtime commands against a live `UnifiedRuntimeSession` or the existing canonical incremental session.

Required session state:

```text
sessionId
candidateId
variantKind
packageSha256
currentActionIndex
runtimeCommandExecutionCount
presentationOnlyActionCount
currentStateHash
runtimeStarted
completed
availableActions[]
actionJournal[]
latestSnapshot
latestSummaries
```

## Data-driven action catalog

Derive bounded action descriptors from the selected package and current Runtime state. Do not hardcode a fallback package or Goal131 candidate.

Required action categories must cover at least:

```text
start_runtime
move
interact
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

Requirements:

- at least 10 total descriptors;
- at least 8 Runtime-routed mutating/executing actions;
- at least 2 presentation-only actions;
- target IDs come from the selected package or validated selected handoff;
- every descriptor declares route, command kind, target ID, prerequisites and whether it may mutate state;
- unavailable actions are reported with a reason;
- no fixed lookup into Goal143 frames may count as execution.

A bounded canonical acceptance action sequence may be defined for automated smoke, but each action must still execute independently through the session API.

## Action request/response contract

Every action execution must have:

```text
actionRequestId
sessionId
actionIndex
actionId
category
route
targetId
stateHashBefore
stateHashAfter
runtimeExecuted
runtimeMutation
runtimeEventCount
correlationPassed
status
diagnostics[]
```

Presentation-only actions must satisfy:

```text
runtimeExecuted=false
runtimeMutation=false
runtimeEventCount=0
stateHashBefore == stateHashAfter
```

Invalid/unavailable actions must be rejected without state mutation and must not advance the action journal.

## Save, reload and deterministic replay

Checkpoint persistence must be journal-based, not a blind serialization of internal Runtime objects.

Required checkpoint fields:

```text
checkpointId
sessionId
candidateId
variantKind
packageSha256
actionJournal
runtimeCommandExecutionCount
expectedStateHash
expectedActionIndex
summaries
createdAtUtc
```

Required reload behavior:

1. Revalidate the selected package SHA.
2. Start a fresh Runtime session.
3. Replay the checkpoint journal in order.
4. Verify request correlation and state-hash continuity.
5. Verify the reconstructed state hash equals `expectedStateHash`.
6. Restore available actions and summaries.

Required full replay behavior:

- replay the complete final journal into a fresh session;
- resulting final state hash equals the original final state hash;
- resulting final state hash equals the accepted Goal142 selected final-state hash;
- selected exploration-resource effect remains visible;
- no balanced-baseline or Goal131 fallback occurs.

## Deterministic acceptance drill

The automated drill must include at least:

```text
1. start/reset selected session
2. execute several individual Runtime actions
3. execute a presentation-only inventory/status action
4. save checkpoint before the final systems segment
5. execute at least two more Runtime actions
6. reload the checkpoint by replay
7. verify the saved state hash is restored exactly
8. continue with individual actions through harvest/transaction/encounter/combat/final state
9. save final journal
10. replay full journal in a fresh session
11. verify final hash equals Goal142 selected final hash
```

Required result markers:

```text
selectedRuntimeVariantInteractiveSession=true
selectedCandidateId=minimal-map-game-exploration-resource-focus
selectedVariantKind=exploration_resource_focus
selectedPackageSha256Matches=true
actionDescriptorCount>=10
runtimeRoutedActionDescriptorCount>=8
presentationOnlyActionDescriptorCount>=2
executedRuntimeActionCount>=8
rejectedInvalidActionCount>=1
invalidActionStateUnchanged=true
checkpointSavePassed=true
checkpointReloadByReplayPassed=true
checkpointStateHashRestored=true
journalCorrelationPassed=true
stateHashContinuityPassed=true
fullReplayEquivalent=true
finalStateHashMatchesGoal142=true
selectedVariantEffectVisible=true
noBalancedBaselineFallback=true
noGoal131Fallback=true
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
```

## WinForms interactive operator

Add a new tab:

```text
Goal144 Live Session
```

Required UI:

```text
Candidate / variant / package hash status
Session ID
Current action index
Runtime command count
Current state hash
Runtime started / completed
Available-action list or combo
Latest map/inventory/quest/combat summaries
Checkpoint status
Last action result
```

Controls:

```text
Start / Reset Session
Execute Selected Action
Save Checkpoint
Reload Checkpoint
Replay Verify
Run Selected Variant Session Drill
```

`Run Selected Variant Session Drill` is the one primary acceptance action.

Requirements:

- all execution is in-process through Application/Runtime services;
- no PowerShell, compiler or `dotnet test` child process;
- disable relevant controls while an operation is running;
- bounded output/diagnostic tail;
- refresh status after every action;
- preserve previous committed artifacts on failed drill through transactional rollback;
- UI never edits JSON directly and never owns gameplay truth.

## Unity read-only live-session consumer

Add:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnitySelectedVariantLiveSessionHarness.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnitySelectedVariantLiveSessionWindow.cs
```

Menu:

```text
LLMGameCreator/Accepted Alpha/Selected Runtime Variant Live Session
```

The window is read-only. It may refresh current Goal144 session/checkpoint/journal artifacts and show:

```text
Gameplay truth: Runtime
Candidate / variant
Session ID
Current state hash
Action progress
Last action
Latest summaries
Checkpoint/replay status
```

It must not execute Runtime gameplay.

Unity batchmode smoke must verify:

```text
session artifacts exist
selected candidate matches Goal142/143
package hash matches
checkpoint reload passed
full replay equivalent
final hash matches Goal142
selected variant effect visible
no fallback
runtimeAuthority=true
unityGameplayTruth=false
pass marker present
fail marker absent
```

## Required artifacts

Under both procedural and export roots:

```text
goal143-human-acceptance-record.json
selected-runtime-variant-live-session-action-catalog.json
selected-runtime-variant-live-session-state.json
selected-runtime-variant-live-session-journal.json
selected-runtime-variant-live-session-checkpoint.json
selected-runtime-variant-live-session-checkpoint-reload-result.json
selected-runtime-variant-live-session-final-replay-result.json
selected-runtime-variant-live-session-dashboard.json
selected-runtime-variant-live-session-negative-proof.json
selected-runtime-variant-live-session-file-index.json
unity-selected-runtime-variant-live-session-smoke.json
one-click-selected-runtime-variant-live-session-report.json
one-click-selected-runtime-variant-live-session-report.md
```

The procedural root may additionally contain per-action correlated request/response records.

## Negative proof

Prove at minimum:

```text
invalidActionRejectedWithoutMutation=true
presentationOnlyActionsDoNotExecuteRuntime=true
checkpointPackageHashMismatchRejected=true
checkpointCandidateMismatchRejected=true
checkpointJournalTamperRejected=true
checkpointExpectedHashMismatchRejected=true
balancedBaselineFallbackRejected=true
goal131FallbackRejected=true
sampleTemplateFallbackRejected=true
unityDoesNotExecuteGameplay=true
winFormsStartsNoCompilerOrTestProcess=true
previousArtifactsPreservedOnFailure=true
```

## Docs/current state

Update source-of-truth docs with:

```text
goal143Accepted=true
goal144Accepted=false
selectedRuntimeVariantInteractiveSession=true
selectedRuntimeVariantId=minimal-map-game-exploration-resource-focus
selectedVariantLiveSessionActionCount=<actual>
checkpointReloadByReplayPassed=true
fullReplayEquivalent=true
finalStateHashMatchesGoal142=true
selectedVariantEffectVisible=true
noBalancedBaselineFallback=true
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
```

Goal141 remains unaccepted unless there is an explicit human acceptance record. Do not fabricate one.

## Artifact-scope scenario

Add:

```text
goal-144-selected-runtime-variant-interactive-action-session-and-save-replay
```

## Validation

Run sequentially:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~SelectedRuntimeVariantInteractiveSession|FullyQualifiedName~Goal144|FullyQualifiedName~SelectedRuntimeVariantPlayerAdapter|FullyQualifiedName~RuntimeBackedPlayerCommandRoundtrip|FullyQualifiedName~VisualWorldStreamPreviewWorkspace|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection"
.\.devflow\scripts\run-selected-runtime-variant-live-session.ps1 -DryRun
.\.devflow\scripts\run-selected-runtime-variant-live-session.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-144-selected-runtime-variant-interactive-action-session-and-save-replay
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Forbidden diff:

```powershell
git diff --name-only -- samples/minimal-map-game .llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff .llmgc/exports/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff .llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff .llmgc/exports/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity/LLMGameCreatorAlpha/Assets/Scenes unity/LLMGameCreatorAlpha/Assets/Prefabs unity/LLMGameCreatorAlpha/Assets/StreamingAssets unity/LLMGameCreatorAlpha/ProjectSettings unity/LLMGameCreatorAlpha/Packages
```

Also check changed/staged files for mojibake and escaped Cyrillic markers.

Full/product tests may regenerate historical artifacts. Restore only exact tracked paths outside the Goal144 scenario allowlist, then repeat all final guards against the final tree.

## Quality gate

GREEN requires:

- Goal143 human acceptance recorded;
- Goal144 remains accepted=false;
- selected Goal142 package/hash integrity passes;
- at least 10 package/state-derived action descriptors;
- individual Runtime action execution is real and correlated;
- invalid action rejection does not mutate state;
- checkpoint save and reload-by-replay pass;
- checkpoint state hash is restored exactly;
- full journal replay is equivalent;
- final state hash matches the accepted Goal142 selected variant;
- exploration-resource effect remains visible;
- no balanced/Goal131/sample fallback;
- WinForms action/drill is in-process;
- Unity is read-only and smoke is GREEN;
- tests/checks/artifact scope pass;
- no forbidden changes;
- no `.llmgc/manual/**` tracked/staged;
- final git status clean.

BLOCKED if the existing Runtime cannot support individual action execution and replay without a bounded Runtime/Runtime.Abstractions extension.

FAILED if the implementation only steps through precomputed Goal143 frames, serializes opaque internal Runtime objects as the sole checkpoint mechanism, silently falls back to balanced/Goal131/sample data, or requires forbidden changes.

## Commit / push policy

Commit and push with one of:

```text
GREEN Goal 144 selected runtime variant interactive action session and save replay
BLOCKED Goal 144 selected runtime variant interactive action session and save replay
FAILED Goal 144 selected runtime variant interactive action session and save replay
```

Final report must include:

- commit SHA;
- Goal143 acceptance status;
- selected candidate/variant/package SHA;
- action descriptor and executed-action counts;
- checkpoint save/reload/hash-restoration result;
- full replay/final-hash result;
- invalid-action negative proof;
- WinForms in-process result;
- Unity smoke result;
- forbidden-zone confirmation;
- final git status.
