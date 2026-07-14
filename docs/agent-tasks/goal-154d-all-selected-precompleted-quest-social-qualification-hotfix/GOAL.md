# Goal 154D — All-Selected Precompleted Quest Social Qualification Hotfix

## Identity

- Task ID: `goal-154d-all-selected-precompleted-quest-social-qualification-hotfix`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `2c95ee8f689ef104946859432706fd6d4b22deb2`
- Required base message: `GREEN Goal 154C3 real project standalone evidence and publication closure`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Sol
Reasoning effort: High
```

Reason: a real human-gate P1 exposed a cross-module qualification-state defect involving core quest
startup, inventory-derived objective refresh, optional alchemy content, social action orchestration,
effect correlation and user-project truth. The fix must remain generic and preserve the strict
Runtime command API and saved-authoring compatibility.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal implementation plan; do not ask for confirmation.
- Do not request intermediate human testing.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No candidate/intermediate commits.
- Codex performs commit and push itself.

## Expected initial worktree

After this ZIP is unpacked, the only permitted untracked files are:

```text
docs/agent-tasks/goal-154d-all-selected-precompleted-quest-social-qualification-hotfix/GOAL.md
docs/agent-tasks/goal-154d-all-selected-precompleted-quest-social-qualification-hotfix/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-154d-all-selected-precompleted-quest-social-qualification-hotfix/README.md
```

Required:

```text
HEAD == origin/main == 2c95ee8f689ef104946859432706fd6d4b22deb2
tracked diff count=0
staged diff count=0
unknown dirty/untracked count=0
```

The three task files are authorized and must be committed.
Any other dirty path blocks execution. Do not use destructive cleanup.

## Unity budget

```text
Unity Editor invocation budget: 0
Unity host build budget: 0
cached standalone hidden-smoke budget: maximum 1
visible automated standalone launch budget: 0
```

Use the existing host cache only. If a host rebuild would be required, publish BLOCKED without
starting Unity.

## Human-gate failure to reproduce exactly

The owner used the current WinForms executable built from `2c95ee8f689ef104946859432706fd6d4b22deb2` and the real saved project:

```text
%LOCALAPPDATA%\LLMGameCreator\Games\goal148-manual
```

Selected module count:

```text
22
```

Selected modules:

```text
feature.interaction.basic
feature.dialogue.basic
feature.quest.objective_chain
feature.inventory.basic
feature.world.grid_navigation
feature.economy.transaction
feature.player_adapter.runtime_summary
feature.combat.turn_based_encounter
feature.resources.harvest
feature.crafting.recipes
feature.combat.active_ability_loadout
feature.magic.mana_spellcasting
feature.quest.faction_reputation_consequences
feature.profile.exploration_resource_focus
feature.dialogue.reputation_gated_reward
feature.profile.alchemy_focus
feature.character.level_progression
feature.profile.combat_focus
feature.faction.reputation_standing
feature.character.attributes
feature.equipment.weapon_loadout
feature.status.turn_effects
```

Failure:

```text
composition.qualification.failed
Runtime qualification action failed:
advance_healer_objective:
goal144.runtime_execution_failed;
capability.advance_healer_objective:
quest.not_active:
Quest is not active: quest/help_healer
```

Attempt facts:

```text
configuredParameterCount=10
selectedModuleCount=22
capability/action counts unavailable because qualification stopped
active package remained unchanged
human acceptance not granted
```

## Proven root cause

The package quest declares:

```text
quest/help_healer
objective/collect_red_herbs
kind=has_item
target=item/red_herb
requiredAmount=3
```

The selected alchemy profile changes starting inventory:

```text
item/red_herb: 2 -> 4
```

The canonical `runtime.command.start_or_update_quest` handler executes:

```text
StartQuest
RefreshQuestObjectives
```

`RefreshQuestObjectives` reads the current inventory for `has_item` objectives. Therefore, with
Alchemy Focus enabled:

```text
start quest
→ refresh sees 4 herbs >= required 3
→ objective completes
→ quest completes
→ quest gold/reputation rewards are applied
```

Later the social action:

```text
advance_healer_objective
```

calls strict Runtime `AdvanceQuestObjective`, which correctly rejects a completed quest as
`quest.not_active`.

This is a qualification orchestration defect, not a QuestRuntimeService defect.

## Why previous automated proof missed it

Goal154C3 test setup explicitly disabled every selected module whose ID starts with:

```text
feature.profile.
```

before enabling the social modules.

That removed `feature.profile.alchemy_focus`, leaving only 2 starting herbs. The quest stayed active,
so only the explicit-advance path was tested.

The previous `all-current-optional` claims did not execute the exact real saved-project authoring
state through this social lifecycle.

## Product invariants

Both paths are legitimate:

### Path A — insufficient starting inventory

```text
starting herbs < quest required amount
start_or_update_quest -> quest active
advance_healer_objective -> executes
quest completes
```

### Path B — sufficient starting inventory

```text
starting herbs >= quest required amount
start_or_update_quest refresh -> quest completes immediately
advance_healer_objective -> truthful deterministic skip
```

Both must produce equivalent social product facts for default social parameters:

```text
quest completed
reputation 0 -> 10
quest gold 0 -> 10
trusted reward +7
final gold 17
claim flag true
socialOutcome=claimed
checkpoint/full replay equivalent
```

The exact action/event route may differ and must remain truthful.

## Hard constraints

Do not:

```text
make completed nonrepeatable quests restartable
weaken QuestRuntimeService.AdvanceQuestObjective
treat quest.not_active as generic Runtime success
remove RefreshQuestObjectives from start_or_update_quest
disable Alchemy Focus or any other selected module
change the owner's saved module selection
change alchemy starting-herb product semantics
change quest requiredAmount
hardcode quest/help_healer or item/red_herb in generic C#
bump social module versions merely to force cache invalidation
change FeatureModule JSON unless a new test proves it unavoidable
trigger saved-composition staleness
```

The strict direct Runtime command must still reject an explicit advance against a completed quest.
Only the capability-driven qualification orchestration may truthfully skip its redundant action.


## A. Generic qualification guard for already-completed quests

Implement in the capability-driven interactive qualification layer, not in `QuestRuntimeService`.

Preferred location:

```text
SelectedRuntimeVariantInteractiveSessionService.EvaluateConditionalAction
```

For every action whose primitive is:

```text
runtime.command.advance_quest_objective
```

derive:

```text
questId from action.Args.questId
objectiveId from action.Args.objectiveId or ResolvedTargetId
```

### A1. Active path

When the runtime contains exactly one matching quest state with:

```text
State=active
matching objective exists and is not completed
```

execute the existing canonical action unchanged.

Expected journal result:

```text
Status=EXECUTED
RuntimeExecuted=true
questCompletionPath=explicit_advance
```

Do not add a synthetic event merely for the path label. It may be a diagnostic/journal observation.

### A2. Already-completed path

When the runtime contains exactly one matching quest state with:

```text
State=completed
matching objective exists and is completed
```

require prior causal evidence in canonical snapshots:

```text
exactly one QuestCompleted event for questId
exactly one QuestRewardGranted event for questId
```

Then truthfully skip the redundant action:

```text
Status=SKIPPED
RuntimeExecuted=false
RuntimeMutation=false
RuntimeEventCount=0
StateHashAfter=StateHashBefore
diagnostics include:
  questCompletionPath=already_completed
  questAlreadyCompletedBeforeAdvance=true
```

The cursor advances deterministically through the action's canonical range.

The required action is considered successfully satisfied because its desired product state already
exists through an earlier causal Runtime path.

### A3. Invalid states

Return a causal qualification failure for:

```text
quest definition missing or ambiguous
runtime quest state missing or ambiguous
quest state=failed
quest state=completed but objective not completed
quest state=completed without prior QuestCompleted/QuestRewardGranted evidence
objective missing or ambiguous
unknown quest state
```

Do not turn these into skips.

### A4. Scope

This behavior is generic for capability-driven qualification actions using
`runtime.command.advance_quest_objective`.

It must not change:

```text
GameRuntimeService
QuestRuntimeService
direct GameRuntimeCommand.AdvanceQuestObjective behavior
manual/runtime simulator command behavior
```

A direct Runtime advance against a completed quest remains `quest.not_active`.

## B. Completion-event-correlated effect truth

The same quest may complete in either:

```text
capability.start_or_update_quest
capability.advance_healer_objective
```

Effect truth must follow the actual causal completion event, not assume one fixed action ID.

### B1. Faction reputation transition

Refactor `faction_reputation_transition_truthful` generically.

Derive the related quest ID from the unique capability action that declares the metric:

```text
ExpectedRuntimeEffects contains faction_reputation_transition_truthful
action.Args.questId
```

Then find canonical snapshots containing:

```text
QuestCompleted targetId=questId
```

Require exactly one completion snapshot.

Inside that same snapshot require exactly one:

```text
FactionReputationChanged targetId=contract.TargetId
```

Validate existing structured fields:

```text
factionId
before
requested
after
delta
clamped
final faction state
package min/max
```

Required paths:

```text
explicit advance path -> transition event in advance action snapshot
already completed path -> transition event in start/update action snapshot
```

Unrelated reputation events from other actions must not satisfy the contract.

Add causal diagnostics for missing or ambiguous completion/transition events.

### B2. Quest state

`quest_state_equals=completed` remains a final-state assertion.

Also require that the final completed state corresponds to the same quest ID found in the causal
completion snapshot.

### B3. No action-ID literals

Generic production code may not contain:

```text
start_or_update_quest
advance_healer_objective
quest/help_healer
objective/collect_red_herbs
feature.profile.alchemy_focus
```

as behavior branches.

Primitive vocabulary constants and data-derived action IDs are allowed.

## C. Social review projection correlation

Update `SocialRuntimeReviewProjectionService` so quest completion/gold facts are derived from the
actual causal completion snapshot.

### C1. Completion snapshot

Using the typed quest contract target:

```text
questId = quest_state_equals contract.TargetId
```

find exactly one snapshot containing:

```text
QuestCompleted targetId=questId
QuestRewardGranted targetId=questId
```

Zero or multiple candidates are causal projection failures.

### C2. Quest gold event

Within the completion snapshot require exactly one `ResourceChanged` event for the resource declared
by the trusted reward resource contract.

This produces:

```text
GoldBefore
GoldAfterQuest
```

It must work whether the completion snapshot belongs to the start/update action or explicit advance
action.

Do not count:

```text
encounter rewards
transaction resource events
trusted claim resource event
other quest rewards
```

### C3. Trusted claim

Keep trusted reward resource/flag evidence scoped to the declaring claim action exactly as Goal154B1
requires.

### C4. Presentation truth

The user-visible social facts remain:

```text
Репутация: 0 → 10
Квест: завершён
Доверенная реплика: недоступна → доступна → недоступна
Золото: 0 → 10 → 17
Награда за доверие: +7
Повторная награда: недоступна
Социальный итог: награда получена
```

Do not expose the internal completion path in the primary card. It belongs in technical evidence.

## D. Checkpoint and replay semantics

The existing social action remains the checkpoint boundary.

### Explicit path

```text
advance action executes
checkpoint captured after its mutation
```

### Already-completed path

```text
advance action is SKIPPED
checkpoint captured after the skipped boundary
state already includes quest completion rewards from the start/update action
```

For both paths require:

```text
checkpoint continuation equals uninterrupted continuation
full replay takes the same completion path
action journal statuses equivalent
Runtime events equivalent
final state hashes equal
social projection equal
```

A replay must not change an already-completed-path skip into an execution or vice versa.

## E. Saved-authoring compatibility

Do not modify FeatureModule JSON unless an unavoidable P0/P1 is demonstrated.

Expected result:

```text
catalog fingerprint unchanged
social module fingerprints unchanged
owner's saved authoring document remains CURRENT, not STALE
semantic authoring fingerprint unchanged for identical saved values
no migration prompt
no forced module toggle
no parameter loss
```

If implementation proves that FeatureModule JSON must change, stop and publish BLOCKED with a
complete migration design. Do not silently stale the owner's saved composition.


## F. Required behavioral test matrix

Create at least 18 Goal154D tests; at least 15 must be behavioral.

A behavioral test invokes actual planner/session/Runtime/evaluator/workspace services and asserts
state, events, action journal, replay or saved-project output. Source-string/reflection tests do not
count.

### F1. Exact owner all-selected reproduction

1. Copy the real read-only source project:
   ```text
   %LOCALAPPDATA%\LLMGameCreator\Games\goal148-manual
   ```
2. Do not reset its authoring document.
3. Do not disable any selected module.
4. Assert:
   ```text
   selectedModuleCount=22
   configuredParameterCount=10
   feature.profile.alchemy_focus selected=true
   feature.quest.faction_reputation_consequences selected=true
   feature.dialogue.reputation_gated_reward selected=true
   ```
5. Before the fix, a focused characterization path must reproduce:
   ```text
   quest completes during start_or_update_quest
   advance_healer_objective would receive completed quest
   ```
   Do not commit a deliberately failing test; preserve the characterization as explicit assertions.
6. After the fix require build GREEN.

### F2. Starting-herb boundary matrix

Use actual parameter binding and materialized package:

```text
startingRedHerbQuantity=2
  -> quest active after start/update
  -> advance action EXECUTED
  -> completionPath=explicit_advance

startingRedHerbQuantity=3
  -> quest completed during start/update
  -> advance action SKIPPED
  -> completionPath=already_completed

startingRedHerbQuantity=4
  -> same already-completed path

startingRedHerbQuantity=20
  -> same already-completed path
```

Do not hardcode the boundary `3` in production logic. Tests may use the package's current fixture
required amount.

### F3. Exact all-current optional composition

Select every currently selectable optional module from the catalog, with dependencies.

Require:

```text
planner GREEN
qualification GREEN
no module disabled by test setup
all selected-module IDs retained
quest completion path already_completed
social result claimed
reputation 0→10
gold 0→10→17
checkpoint/full replay GREEN
```

This test must derive the optional set from the catalog, not an old fixed module count.

### F4. Strict direct Runtime behavior

Call direct Runtime:

```text
StartQuest
RefreshQuestObjectives
AdvanceQuestObjective
```

with enough starting inventory.

Require:

```text
quest completed after refresh
direct AdvanceQuestObjective fails quest.not_active
state unchanged by failed direct advance
no success event from failed direct advance
```

This proves the hotfix did not weaken Runtime.

### F5. Qualification skip truth

For the already-completed path assert:

```text
advance action journal Status=SKIPPED
RuntimeExecuted=false
RuntimeMutation=false
RuntimeEventCount=0
beforeHash=afterHash
diagnostics contain questCompletionPath=already_completed
prior start/update snapshot contains QuestCompleted and QuestRewardGranted
```

### F6. Effect and projection correlation

For both explicit and already-completed paths assert:

```text
faction transition observation Passed=true
transition event is in the actual quest-completion snapshot
quest gold event is in the same completion snapshot
claim gold event is only in claim snapshot
unrelated reputation/resource events cannot satisfy either metric
missing completion event fails causally
duplicate completion snapshots fail causally
duplicate quest-gold events fail causally
```

### F7. Invalid quest states

Test qualification action against:

```text
missing runtime quest
failed quest
completed quest with incomplete objective
completed quest without completion events
ambiguous duplicate quest state
```

All fail causally; none skip.

### F8. Replay

For explicit and already-completed paths:

```text
checkpoint reload passes
full replay equivalent
same action journal EXECUTED/SKIPPED status
same final hash
same social HumanFacts
```

### F9. Saved project lifecycle

On an exact disposable copy of the owner's current project:

```text
open
build without changing selected modules or parameters
repeat build
fresh reopen
```

Require:

```text
both builds GREEN
package/composition/final hashes deterministic
SocialConfigurationStatus=CURRENT
five social values remain 0/10/5/10/7
all pre-existing profile parameters remain unchanged
source project byte-identical
```

### F10. Cached standalone

Using the exact all-selected disposable project after GREEN:

```text
maximum one hidden smoke
HostReused=true
HostRebuilt=false
Unity process count=0
5/5 checks
actual payload social facts match build
```

No standalone smoke is required if focused build/replay proof fails.

## G. Diagnostic truth

When a capability-driven advance is skipped because the quest already completed, technical evidence
must state:

```text
questCompletionPath=already_completed
completedDuringAction=<data-derived action ID>
redundantAdvanceSkipped=true
```

When qualification fails due an invalid quest state, diagnostics include:

```text
actionId
questId
objectiveId
observedQuestState
priorCompletionEventCount
priorRewardEventCount
```

Do not expose these details in the primary user social card.

## H. Human-gate state and source-of-truth

The manual attempt at commit:

```text
2c95ee8f689ef104946859432706fd6d4b22deb2
```

failed. Record it truthfully before publishing the hotfix result:

```text
goal154c3HumanGateAttempted=true
goal154c3HumanGatePassed=false
goal154c3HumanGateFailureStage=composition.qualification
goal154c3HumanGateFailureAction=advance_healer_objective
goal154c3HumanGateFailureDiagnostic=quest.not_active
goal154ManualGateReady=false
```

On Goal154D GREEN:

```text
goal154ImplementationStatus=GREEN
goal154Accepted=false
goal154AcceptedByHuman=false
goal154AcceptedByCodex=false
goal154ManualReviewPerformed=true
goal154ManualGateReady=true

goal154c3ImplementationStatus=GREEN
goal154c3Accepted=false
goal154c3HumanGateAttempted=true
goal154c3HumanGatePassed=false
goal154c3AuditBlocker=closed_by_goal154d

goal154dImplementationStatus=GREEN
goal154dAccepted=false
goal154dAcceptedByHuman=false
goal154dAcceptedByCodex=false
goal154dManualReviewPerformed=false
goal154dManualGateReady=true

goal154AllSelectedModuleCount=22
goal154AllSelectedConfiguredParameterCount=10
goal154AllSelectedBuildPassed=true
goal154AlchemyPrecompletedQuestPathPassed=true
goal154ExplicitAdvanceQuestPathPassed=true
goal154DirectRuntimeStrictnessPreserved=true
goal154AllSelectedCheckpointReplayPassed=true
goal154AllSelectedHostReused=true
goal154AllSelectedHostRebuilt=false
goal154AllSelectedUnityProcessStartCount=0
nextAction=retry_goal154_combined_human_gate
```

Preserve all Goals153/153A/153B/153C human acceptance exactly.

Do not claim Goal154 human acceptance.

## I. Evidence

Create exactly 9 files in each mirrored root:

```text
goal154d-dashboard.json
root-cause-and-design-proof.json
all-selected-real-project-proof.json
quest-completion-path-matrix.json
runtime-strictness-and-invalid-state-proof.json
effect-projection-correlation-proof.json
checkpoint-replay-proof.json
artifact-scope-proof.json
goal154d-report.md
```

Procedural/export twins must be byte-identical.

Dashboard fields:

```text
status
goal154dTestsDiscovered
goal154dBehavioralTestsPassed
ownerSelectedModuleCount
ownerConfiguredParameterCount
ownerExactSelectionPreserved
alchemyStartingHerbs
questRequiredHerbs
startUpdateCompletedQuest
advanceActionStatus
advanceRuntimeExecuted
advanceRuntimeMutation
advanceRuntimeEventCount
explicitAdvancePathPassed
alreadyCompletedPathPassed
directRuntimeStillRejectsCompletedAdvance
defaultReputationBefore
defaultReputationAfter
defaultGoldAfterQuest
defaultGoldAfterClaim
checkpointReplayPassed
fullReplayEquivalent
sourceProjectByteIdentical
hostReused
hostRebuilt
unityProcessStartCount
hiddenSmokeInvocationCount
artifactScopeViolationCount
goal154Accepted=false
manualGateReady=true
```

No GREEN field may be `null`, `PARTIAL`, `NOT_EXECUTED` or a source-string-only assertion.

## J. Current docs

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

docs/manual-acceptance/goal154-faction-reputation-social-consequences.md
docs/manual-acceptance/goal154c3-final-publication-closure.md
docs/manual-acceptance/goal154d-all-selected-precompleted-quest-hotfix.md
```

Remove or rewrite stale active prose saying the Goal154 human gate is still merely pending without
mentioning the failed attempt.

Historical evidence remains immutable.


## K. Required validation

### Build and discovery

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal154D"
```

Require:

```text
Goal154D discovered >=18
Goal154D behavioral >=15
```

A zero-match filter is FAILED.

### Focused tests

```powershell
dotnet test ... --filter "FullyQualifiedName~Goal154D"
dotnet test ... --filter "FullyQualifiedName~Goal154C3"
dotnet test ... --filter "FullyQualifiedName~Goal154C2"
dotnet test ... --filter "FullyQualifiedName~Goal154C1"
dotnet test ... --filter "FullyQualifiedName~Goal154B1"
dotnet test ... --filter "FullyQualifiedName~Goal154B"
dotnet test ... --filter "FullyQualifiedName~Goal153C"
dotnet test ... --filter "FullyQualifiedName~CapabilityDrivenRuntimePlaythrough"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectStandaloneBuild"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleLibrary"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleCertification"
dotnet test ... --filter "FullyQualifiedName~RuntimeNarrative"
```

### Existing focused slices

```powershell
.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

### External project proof

```text
copy exact goal148-manual including its saved authoring
do not reset authoring
assert 22 selected / 10 configured
build and repeat
fresh reopen
optional one cached hidden smoke
verify source manifest
```

Run artifact-scope validation last.

Do not run:

```text
full suite
85-case historical closure
all-ProductSmoke
Unity host build
```

## L. Command and investigation budget

```text
read-first and reproduction: maximum 10 primary files / 7 minutes
qualification guard: maximum 8 minutes
effect/projection correlation: maximum 8 minutes
behavioral tests: maximum 15 minutes
real-project/cache proof: maximum 7 minutes
evidence/docs/artifact scope: maximum 8 minutes
total target wall clock: 48 minutes
maximum two testhost processes
Unity process count: 0
```

Rules:

```text
write all-selected characterization tests before production edits
no unchanged command repetition
no timeout escalation loop
after a concrete failure run only the exact failing test/class
do not mutate source goal148-manual
do not disable profile modules in the main regression
```

## M. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal154d-all-selected-quest-hotfix.ps1
.devflow/scripts/run-goal154d-all-selected-quest-hotfix.cmd

src/LLMGameCreator.Runtime/SelectedRuntimeVariantInteractiveSessionService.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectEvaluator.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/SocialRuntimeReviewProjectionService.cs

tests/LLMGameCreator.Tests/Application/Goal154D/Goal154DAllSelectedRealProjectTests.cs
tests/LLMGameCreator.Tests/Application/Goal154D/Goal154DQuestCompletionPathTests.cs
tests/LLMGameCreator.Tests/Application/Goal154D/Goal154DEffectProjectionCorrelationTests.cs
tests/LLMGameCreator.Tests/Application/Goal154D/Goal154DReplayAndStandaloneTests.cs
tests/LLMGameCreator.Tests/Runtime/Goal154D/Goal154DDirectRuntimeStrictnessTests.cs
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal154SocialConsequenceWorkspaceTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/goal154-faction-reputation-social-consequences.md
docs/manual-acceptance/goal154c3-final-publication-closure.md
docs/manual-acceptance/goal154d-all-selected-precompleted-quest-hotfix.md

docs/agent-tasks/goal-154d-all-selected-precompleted-quest-social-qualification-hotfix/
.llmgc/procedural/goal-154d-all-selected-precompleted-quest-social-qualification-hotfix/
.llmgc/exports/goal-154d-all-selected-precompleted-quest-social-qualification-hotfix/
```

If an exact compile/test failure proves one additional existing Application/Runtime/test path is
required, record the reason and add only that exact path.

Forbidden:

```text
src/LLMGameCreator.Runtime/QuestRuntimeService.cs
src/LLMGameCreator.Runtime/GameRuntimeService.cs
catalogs/feature-modules/**
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/**
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/**
src/LLMGameCreator.WinForms/**
unity/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
public GamePackage schema changes
```

## N. Text integrity

Scan all changed/task/evidence/docs files:

```text
valid UTF-8
no NUL
no forbidden C0 control characters except CR/LF/TAB
no mojibake markers
no escaped Cyrillic in user-facing JSON/Markdown where repository policy forbids it
```

Historical evidence is not rewritten.

## O. Publication

Create exactly one final commit:

```text
GREEN Goal 154D all-selected precompleted quest social qualification hotfix
```

or honest:

```text
BLOCKED Goal 154D all-selected precompleted quest social qualification hotfix
FAILED Goal 154D all-selected precompleted quest social qualification hotfix
```

Codex performs commit and push itself.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three Goal154D task files tracked
source project byte-identical
Unity process start count=0
Goal154 family accepted=false
Goal154ManualGateReady=true only on GREEN
```

## GREEN criteria

```text
exact owner 22-module/10-parameter composition reproduces the precompleted path and builds GREEN
no profile module disabled
starting herb boundary matrix 2/3/4/20 passes
explicit advance and already-completed paths both pass
advance action skip is state/event atomic
direct Runtime still rejects completed quest advance
effect evaluator follows actual completion snapshot
social projection follows actual completion snapshot
unrelated/duplicate events rejected causally
checkpoint/full replay equivalent for both paths
owner saved authoring remains CURRENT/non-stale
repeat build deterministic
source goal148-manual byte-identical
optional cached standalone reuses host with Unity 0
Goal154C3/B1/B and Goal153C regressions GREEN
9+9 evidence files mirrored byte-identically
new text integrity GREEN
artifact scope 0 violations
human-gate failure recorded and blocker closed
Goal154D manual gate ready
one final commit pushed
```

## P. Manual gate after independent GREEN audit

The retry must remain only four user actions:

```text
1. In the already configured goal148-manual project, press “Собрать и проверить игру”.
2. Confirm the social card shows reputation 0→10 and gold 0→10→17.
3. Save, close/reopen and confirm the values/card remain.
4. Build/launch the cached standalone and confirm the same social facts.
```

The owner does not need to:

```text
disable Alchemy Focus
disable any profile
change starting herbs
inspect action journals
inspect quest events
inspect hashes
test both completion paths manually
```

## Q. Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning used;
- exact root-cause reproduction;
- confirmation previous test setup disabled profiles;
- Goal154D discovered/behavioral counts;
- exact owner selected/configured counts;
- starting-herb boundary matrix;
- explicit versus already-completed action journal results;
- direct Runtime strictness result;
- effect/projection completion-snapshot correlation;
- checkpoint/replay results;
- exact saved-project build/repeat/reopen result;
- source immutability;
- host reuse/Unity/smoke facts if run;
- Goal154C3/B1/B and Goal153C regressions;
- evidence mirror/text integrity;
- artifact scope;
- Goal154 family status/manual gate;
- final SHA/push/HEAD/worktree;
- confirmation no human acceptance claimed.
