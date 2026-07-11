# Goal 144A — Live Session Action Target Binding + Replay Evidence Freeze Hotfix

## Task ID

`goal-144a-live-session-action-target-binding-and-replay-evidence-hotfix`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Goal type

P1 semantic-correctness hotfix.

Do not add a new gameplay/product layer. Repair Goal144 so every advertised live-session action is bound to the exact Runtime operation/target that is executed, and so replay evidence is immutable and numerically honest.

Goal144 itself must remain `accepted=false` until this hotfix is audited and the WinForms live session is manually reviewed.

## Why this hotfix is required

Goal144 correctly created a persistent Runtime-owned session, independent action execution, journal checkpointing and deterministic replay. However, two evidence/correlation defects remain.

### P1 — advertised action target is not the executed Runtime target

The committed Goal144 action catalog and journal currently advertise:

```text
harvest.targetId = node/diesel_generator
```

but `CanonicalRuntimePlayerCommandLoopService` executes:

```text
harvest_apple_tree
HarvestResourceNode("node/apple_tree", ...)
```

The root cause is that `BuildCatalog` independently chooses:

```text
game.ResourceNodes.FirstOrDefault()
```

while `ExecuteAction` separately maps `harvest` through a hardcoded canonical range. The descriptor target is therefore decorative rather than execution-binding truth.

A similar ambiguity exists for `basic_attack`: the Goal144 descriptor advertises `ability/basic_attack`, while the canonical Runtime operation attacks target `goblin`.

Goal144 promised:

```text
user/operator chooses one action
→ Runtime executes exactly that action
→ correlated response
```

The action descriptor, response and journal must therefore identify the exact canonical step and target actually executed.

### P2 — checkpoint reload count is mutated after the proof

The checkpoint contains 8 actions and `expectedActionIndex=8`, but the committed checkpoint-reload artifact reports:

```text
replayedActionCount=13
```

The checkpoint reload itself succeeds and restores the correct hash. The count becomes incorrect because the replay result retains a mutable session reference; the drill continues that same session to the final state, and `ToReplay(...)` reads the journal count later.

Checkpoint evidence must be frozen at the moment of reload:

```text
checkpoint replayedActionCount = 8
final replayedActionCount = 13
```

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

docs/agent-tasks/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/GOAL.md

.llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/selected-runtime-variant-live-session-action-catalog.json
.llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/selected-runtime-variant-live-session-journal.json
.llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/selected-runtime-variant-live-session-checkpoint.json
.llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/selected-runtime-variant-live-session-checkpoint-reload-result.json
.llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/selected-runtime-variant-live-session-final-replay-result.json

src/LLMGameCreator.Runtime.Abstractions/SelectedRuntimeVariantInteractiveSessionContracts.cs
src/LLMGameCreator.Runtime/SelectedRuntimeVariantInteractiveSessionService.cs
src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs

src/LLMGameCreator.Application/Design/SelectedRuntimeVariantInteractiveSession/SelectedRuntimeVariantInteractiveSessionService.cs
src/LLMGameCreator.Application/Design/SelectedRuntimeVariantInteractiveSession/SelectedRuntimeVariantInteractiveSessionController.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/SelectedRuntimeVariantInteractiveSessionModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/SelectedRuntimeVariantInteractiveSessionArtifactService.cs

src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal144.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnitySelectedVariantLiveSessionHarness.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnitySelectedVariantLiveSessionWindow.cs
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-144a-live-session-action-target-binding-and-replay-evidence-hotfix/**
.devflow/artifact-scope/artifact-scope-policy.json

.devflow/scripts/run-selected-runtime-variant-live-session.ps1
.devflow/scripts/run-selected-runtime-variant-live-session.cmd

.llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/**
.llmgc/exports/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/**
.llmgc/procedural/goal-144a-live-session-action-target-binding-and-replay-evidence-hotfix/**
.llmgc/exports/goal-144a-live-session-action-target-binding-and-replay-evidence-hotfix/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
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
tests/LLMGameCreator.Tests/Runtime/CanonicalRuntimePlayerCommandLoopServiceTests.cs
tests/LLMGameCreator.Tests/Application/SelectedRuntimeVariantInteractiveSession/**
tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/SelectedRuntimeVariantInteractiveSessionScriptProof.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal144Tests.cs
tests/LLMGameCreator.Tests/Devflow/RunSelectedRuntimeVariantLiveSessionScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage or commit:

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

No public GamePackage schema change. No sample mutation. No Goal142/143 historical artifact rewriting. No provider/network/LLM/Lua/generator-library work. Unity must remain read-only.

## Required correction 1 — descriptor is the execution binding

Every Runtime-routed descriptor must identify the canonical execution it is bound to.

Add or equivalent internal contract fields:

```text
canonicalStepId
canonicalStepIndex
runtimeCommandStartIndex
runtimeCommandEndIndex
executionTargetId
executionBindingValidated
```

For a single-step Runtime action:

```text
descriptor.targetId == canonicalStep.targetId
descriptor.commandKind == canonicalStep.runtimeCommandKind
descriptor.canonicalStepId == executed step id
descriptor.canonicalStepIndex == executed step index
runtimeCommandStartIndex == runtimeCommandEndIndex == canonicalStepIndex
```

`start_runtime` may remain a bounded two-step operation over load/start, but its primary execution binding must be explicit and its range must come from the descriptor rather than a second independent switch.

### Required exact bindings

```text
start_runtime:
  range=0..1
  primaryStepId=start_canonical_runtime
  targetId=map/village

move:
  stepId=move_to_sign
  targetId=entity/village/sign

interact:
  stepId=interact_with_sign
  targetId=interaction/sign_inspect

open_dialogue:
  stepId=show_old_guard_dialogue
  targetId=dialogue/old_guard_intro

start_or_update_quest:
  stepId=start_or_update_help_healer_quest
  targetId=quest/help_healer

show_inventory:
  stepId=show_inventory_state
  targetId=inventory/player_start

craft:
  stepId=craft_healing_potion
  targetId=recipe/healing_potion

harvest:
  stepId=harvest_apple_tree
  targetId=node/apple_tree

transaction:
  stepId=execute_transaction
  targetId=transaction/buy_healing_potion

begin_encounter:
  stepId=start_encounter
  targetId=encounter/goblin_duel

basic_attack:
  stepId=combat_round
  targetId=goblin
```

The old committed mismatch is forbidden:

```text
harvest -> node/diesel_generator
```

For `basic_attack`, `ability/basic_attack` may be recorded separately as an ability/source identifier if useful, but it must not replace the actual execution target `goblin` in `targetId`.

## Required correction 2 — remove independent target/range lookup

Do not maintain two independent sources of truth like:

```text
BuildCatalog chooses FirstOrDefault target
CanonicalRange(actionId) chooses a fixed step
```

Required behavior:

1. Build the descriptor from the exact canonical session step/range.
2. Validate every referenced target exists in the selected package or active Runtime state.
3. Execute the range stored in the selected descriptor.
4. Build response/journal binding fields from the actual execution result/step.
5. Fail if descriptor target, canonical step target and executed operation target disagree.

Remove or replace `CanonicalRange(actionId)` as an independent execution mapping.

Do not use `FirstOrDefault()` package ordering to choose Runtime action targets when a canonical bound step already identifies the exact target.

## Required correction 3 — Runtime command uses the bound step target

Where the canonical Runtime currently hardcodes target strings, use the validated `step.TargetId` for the corresponding command when the command API accepts that target.

At minimum:

```text
OpenDialogue(step.TargetId)
StartQuest(step.TargetId)
CraftRecipe(step.TargetId, inventory/player_start)
HarvestResourceNode(step.TargetId, ...)
ExecuteTransaction(step.TargetId, ...)
StartEncounter(step.TargetId, ...)
BasicAttack("player", step.TargetId)
```

Keep any required inventory/tool/source IDs explicit and separately validated. Do not change gameplay semantics or final accepted hash.

After the correction, the final state hash must still equal:

```text
d7c04179cb76ca48ba9694905e491bead014c0f56f446f66331becd5e3211e54
```

## Required correction 4 — request/response/journal target correlation

Every successful Runtime action must prove:

```text
actionRequestId matches
sessionId matches
actionIndex matches
actionId matches
canonicalStepId matches executed step
canonicalStepIndex matches executed step
targetId matches executed step target
commandKind matches executed step command kind
executionBindingValidated=true
```

Replay must revalidate the same binding. A tampered target/step binding must be rejected or fail replay proof without mutating accepted state.

Add negative tests for:

```text
harvest descriptor target changed to node/diesel_generator
basic_attack descriptor target changed to ability/basic_attack
canonicalStepId changed to unrelated step
runtime range changed to unrelated index
journal target changed after execution
```

## Required correction 5 — freeze checkpoint replay evidence

The checkpoint has 8 journal entries. The final journal has 13 entries.

Required committed artifacts:

```text
selected-runtime-variant-live-session-checkpoint-reload-result.json:
  replayedActionCount=8
  expectedStateHash=cb819cb474f7019646de72de59a85cbe1fd0909a476e218b389864fb92fb53c6
  actualStateHash=cb819cb474f7019646de72de59a85cbe1fd0909a476e218b389864fb92fb53c6

selected-runtime-variant-live-session-final-replay-result.json:
  replayedActionCount=13
  expectedStateHash=d7c04179cb76ca48ba9694905e491bead014c0f56f446f66331becd5e3211e54
  actualStateHash=d7c04179cb76ca48ba9694905e491bead014c0f56f446f66331becd5e3211e54
```

Freeze the checkpoint reload summary immediately after reload, before continuing the returned session, or add immutable scalar replay metadata to the Runtime replay result.

A previously produced replay summary must not change when its returned session later continues.

Add a regression test that:

1. reloads the 8-action checkpoint;
2. captures the replay summary/count;
3. continues the returned session to 13 actions;
4. verifies the captured checkpoint summary remains `8`.

## Corrected Goal144 artifacts

Regenerate both Goal144 procedural/export roots with corrected binding and replay counts:

```text
selected-runtime-variant-live-session-action-catalog.json
selected-runtime-variant-live-session-journal.json
selected-runtime-variant-live-session-checkpoint.json
selected-runtime-variant-live-session-checkpoint-reload-result.json
selected-runtime-variant-live-session-final-replay-result.json
selected-runtime-variant-live-session-state.json
selected-runtime-variant-live-session-dashboard.json
selected-runtime-variant-live-session-negative-proof.json
unity-selected-runtime-variant-live-session-smoke.json
one-click-selected-runtime-variant-live-session-report.json
one-click-selected-runtime-variant-live-session-report.md
selected-runtime-variant-live-session-file-index.json
```

## Goal144A evidence

Write under both Goal144A roots:

```text
action-execution-binding-proof.json
replay-evidence-freeze-proof.json
goal144a-correctness-dashboard.json
goal144a-correctness-report.md
goal144a-file-index.json
```

Required dashboard markers:

```text
status=GREEN
actionDescriptorExecutionBindingPassed=true
allRuntimeActionTargetsMatchExecutedSteps=true
allRuntimeActionCommandKindsMatchExecutedSteps=true
harvestActionTargetId=node/apple_tree
harvestExecutedTargetId=node/apple_tree
basicAttackActionTargetId=goblin
basicAttackExecutedTargetId=goblin
noFirstResourceNodeFallback=true
noIndependentCanonicalRangeLookup=true
checkpointReplayedActionCount=8
finalReplayActionCount=13
replayEvidenceFrozenBeforeContinuation=true
checkpointStateHashRestored=true
fullReplayEquivalent=true
finalStateHashMatchesGoal142=true
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
goal144Accepted=false
accepted=false
```

## WinForms

Keep the existing Goal144 tab and controls.

Improve the action selector text or status surface so the operator can see at least:

```text
actionId
targetId
canonicalStepId
route
```

The operator must still execute the in-process Runtime session without compiler/test/PowerShell child processes.

No manual acceptance is required for Goal144A itself. The corrected Goal144 UI will be manually reviewed after this hotfix audit.

## Unity

Unity remains a read-only artifact consumer.

Update batchmode smoke to validate corrected artifacts:

```text
harvest target node/apple_tree
basic attack target goblin
all binding markers true
checkpoint replay count 8
final replay count 13
final hash match
runtimeAuthority=true
unityGameplayTruth=false
```

Unity must not execute gameplay.

## Current state

Goal144 remains:

```text
implementationStatus=GREEN
accepted=false
goal144Accepted=false
```

Add:

```text
goal144ActionExecutionBindingCorrected=true
actionDescriptorExecutionBindingPassed=true
allRuntimeActionTargetsMatchExecutedSteps=true
harvestActionTargetId=node/apple_tree
basicAttackActionTargetId=goblin
checkpointReplayedActionCount=8
finalReplayActionCount=13
replayEvidenceFrozenBeforeContinuation=true
```

Do not fabricate human acceptance.

## Artifact-scope scenario

Add:

```text
goal-144a-live-session-action-target-binding-and-replay-evidence-hotfix
```

## Validation

Run sequentially:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~SelectedRuntimeVariantInteractiveSession|FullyQualifiedName~CanonicalRuntimePlayerCommandLoop|FullyQualifiedName~Goal144|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
.\.devflow\scripts\run-selected-runtime-variant-live-session.ps1 -DryRun
.\.devflow\scripts\run-selected-runtime-variant-live-session.ps1 -ApplyCleanup
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-144a-live-session-action-target-binding-and-replay-evidence-hotfix
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Forbidden diff must be empty:

```powershell
git diff --name-only -- samples/minimal-map-game .llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff .llmgc/exports/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff .llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff .llmgc/exports/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff src/LLMGameCreator.GamePackage src/LLMGameCreator.Generation src/LLMGameCreator.AssetPipeline src/LLMGameCreator.Scripting generator-library unity/LLMGameCreatorAlpha/Assets/Scenes unity/LLMGameCreatorAlpha/Assets/Prefabs unity/LLMGameCreatorAlpha/Assets/StreamingAssets unity/LLMGameCreatorAlpha/ProjectSettings unity/LLMGameCreatorAlpha/Packages
```

Check changed files for mojibake and escaped Cyrillic markers.

## Quality gate

GREEN requires:

- exact descriptor → canonical step → executed target binding;
- harvest advertises and executes `node/apple_tree`;
- basic attack advertises and executes target `goblin`;
- no independent first-resource-node target fallback;
- no independent actionId-to-range execution lookup;
- tampered target/step/range binding is rejected by tests;
- checkpoint replay artifact reports 8 actions;
- final replay artifact reports 13 actions;
- replay summaries remain immutable after session continuation;
- final Runtime hash remains the accepted Goal142 hash;
- Unity read-only smoke passes;
- tests/checks/scope pass;
- no forbidden changes;
- no `.llmgc/manual/**` tracked/staged;
- final git status clean.

BLOCKED if exact binding cannot be added without a bounded Runtime/Runtime.Abstractions extension.

FAILED if the implementation preserves cosmetic descriptor targets unrelated to actual execution, weakens replay/hash validation, changes the accepted final hash, or requires forbidden changes.

## Commit / push policy

Commit and push with one of:

```text
GREEN Goal 144A live session action target binding and replay evidence hotfix
BLOCKED Goal 144A live session action target binding and replay evidence hotfix
FAILED Goal 144A live session action target binding and replay evidence hotfix
```

Final report must include:

- commit SHA;
- harvest descriptor/executed target;
- basic-attack descriptor/executed target;
- action binding proof result;
- checkpoint/final replay counts;
- replay evidence freeze result;
- final hash;
- Unity smoke result;
- forbidden-zone confirmation;
- final git status.
