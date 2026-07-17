# Goal 163 — Package-Truth Campaign Combat, Manual Quest Turn-In & Consequence Depth

## Identity

- Task ID: `goal-163-package-truth-campaign-combat-turn-in-and-consequence-depth`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `8164185bb31bacfb9b0813b3fa726fdefcc095e4`
- Required base message: `Complete Goal162 generated campaign session workspace`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Sol
Reasoning effort: Extra High
```

Reason: this is a major player-facing consequence-depth vertical slice. It closes two independent
Goal162 product-truth findings, removes unqualified Runtime package mutation, makes quest turn-in a real
player action, and adds a human consequence ledger across combat, rewards, quests, reputation, travel,
save/continue and migration. It must preserve Runtime/GamePackage schemas and the already qualified
standalone/RC evidence.

## Pre-approval and publication

- The owner approved execution by launching this task.
- Produce a concise internal plan; do not ask for confirmation.
- Do not request manual testing.
- Fix all P0/P1 defects reproduced by the required Goal163 matrix inside this Goal.
- Record P2/P3 debt; do not create Goal163A.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- On BLOCKED/FAILED, commit and push the honest state; never leave changes for the user to publish.
- No intermediate commits.

## Initial worktree

After unpacking, only these untracked files are allowed:

```text
docs/agent-tasks/goal-163-package-truth-campaign-combat-turn-in-and-consequence-depth/GOAL.md
docs/agent-tasks/goal-163-package-truth-campaign-combat-turn-in-and-consequence-depth/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-163-package-truth-campaign-combat-turn-in-and-consequence-depth/README.md
```

Require:

```text
HEAD == origin/main == 8164185bb31bacfb9b0813b3fa726fdefcc095e4
branch=main
no other tracked/staged/untracked changes
```

Never reset, revert, stash, merge or rebase.

## Execution budget

```text
Unity Editor starts: 0
Unity host builds: 0
standalone Build calls: 0
Player process starts: 0
visible application auto-launch: 0
manual user tests: 0
```

RC record, immutable standalone run/current pointer and standalone history must remain byte-identical.

## Goal162 independent-audit result

Record:

```text
goal162IndependentAuditResult=BLOCKED_AT_8164185B
goal162IndependentAuditBlocker=campaign_combat_executes_against_synthetic_runtime_package_and_manual_quest_turn_in_not_proven
goal162AuditBlocker=closed_by_goal163 only on GREEN
```

Goal162 remains implementation GREEN and `accepted=false`; no human gate.

### Goal162 truths to preserve

```text
Играть page and Projects build-and-play
strict campaign project truth and stale-session blocking
human map/context/HUD
generated dialogue
generated travel
generated save/exact continue/migration
all-selectable/core-only/legacy compatibility
72/72 Goal162 tests
RC/standalone immutability
Player/Unity/standalone counts zero
```

## Independent-audit P1-A — synthetic combat package

`GeneratedCampaignActionPlanner` plans a real:

```text
GameRuntimeCommand.BasicAttack(source, target)
```

But `GeneratedCampaignSessionService.ExecuteRuntimeAction()` intercepts that action and:

```text
clones the authoritative package
marks or adds the target resource as health
adds ability/campaign/session-compatible-attack
sets fixed Power=3
executes UseAbility against the cloned package
```

The player therefore does not fight against the exact package whose bytes/hashes were qualified by:

```text
build history
generated gameplay save truth
standalone payload
RC record
```

This also hardcodes product combat power in an Application UI orchestration service.

Goal162 tests intentionally permit either `BasicAttack` or `UseAbility`, so the substitution is not
detected.

### Required closure

```text
all player Runtime calls receive the exact captured current GamePackageDefinition
no package clone
no transient ability/resource/status/stat definition
no campaign/session-compatible-attack ID
no fixed combat power in campaign services
BasicAttack action executes GameRuntimeCommandType.BasicAttack
UseAbility action executes only an ability that exists in the exact package
```

If the exact current package cannot execute a planned combat action, the action is disabled or the
project is campaign-not-ready with a causal diagnostic. Never repair package semantics at action time.

## Independent-audit P1-B — manual quest turn-in evidence is false

Goal162 required:

```text
objectives become ready
→ player sees Завершить задание
→ CompleteQuest succeeds
→ rewards and reputation are applied
```

But the current real fixture proves:

```text
RefreshQuestObjectives auto-completes the quest
CompleteQuest command count = 0
AfterComplete is the same snapshot as AfterFight
```

The dashboard's `campaignQuestCompleted=true` is true only for automatic Runtime completion, not for
the required player-facing turn-in action.

### Required closure

For generated turn-in quests:

```text
evaluate objective readiness read-only from current package/state
do not auto-refresh them into completion
show Завершить задание only when every required objective is satisfied
execute existing CompleteQuest exactly once
then show rewards/reputation consequences
```

Never call `AdvanceQuestObjective`.

Non-generated quests retain existing Runtime refresh behavior.

## Independent-audit P2 to close

`GeneratedCampaignSessionTruthService` currently assigns:

```text
FinalStateHash = SelectedBuildHistorySha256
```

This is conservative for stale detection but typed incorrectly.

Goal163 must expose both exact fields:

```text
FinalStateHash
SelectedBuildHistorySha256
```

and obtain the final state from the selected current build-history record.

## Product outcome

The player-facing consequence route becomes:

```text
start exact qualified campaign
→ choose encounter
→ use exact package BasicAttack or exact package ability
→ observe resource damage and turn flow
→ choose flee/loss or victory path
→ victory grants exact package reward
→ generated quest becomes ready for turn-in
→ player explicitly turns it in
→ reward/reputation consequences appear
→ travel consequence appears
→ save and exact continue preserve consequence truth
→ regeneration/migration records reset/preserved consequence summary
```

Add a visible panel/tab:

```text
Последствия
```

No raw IDs/hashes/paths in the primary consequence surface.

## Non-goals

Do not change:

```text
Runtime or Runtime.Abstractions
GamePackage schema/domain definitions
FeatureModule catalog/semantics
generated source/overlay/travel
generated gameplay save/migration implementation
Unity/standalone/RC implementation
generation algorithms
```

Do not invent richer generated content in this Goal. Consume the current qualified package honestly.


## Mandatory architecture review

Read at most 16 primary files:

```text
GeneratedCampaignSessionService.cs
GeneratedCampaignActionPlanner.cs
GeneratedCampaignProjectionService.cs
GeneratedCampaignSessionModels.cs
GeneratedCampaignSessionTruthService.cs
GeneratedCampaignEventPresenter.cs
Goal162CampaignEncounterQuestTests.cs
Goal162CampaignSaveMigrationTests.cs
Goal162CampaignTruthProjectionTests.cs
EncounterRuntimeService.cs
QuestRuntimeService.cs
GameProjectBuildHistoryReader.cs
GeneratedGameplaySaveService.cs
GeneratedGameplaySaveMigrationService.cs
GeneratedCampaignPageControl.cs
docs/CURRENT_GENERATOR_STATE.md
```

Before production edits write:

```text
.llmgc/procedural/goal-163-package-truth-campaign-combat-turn-in-and-consequence-depth/architecture-review.json
```

Required resolved sections:

```text
goal162IndependentAudit
authoritativePackageBoundary
basicAttackDispatch
packageAbilityDispatch
combatReadiness
generatedQuestTurnInContract
objectiveReadinessEvaluation
nonGeneratedQuestCompatibility
consequenceDeltaModel
consequenceEventCorrelation
sameWorldSaveContinue
migrationConsequenceReset
typedCampaignTruth
uiConsequenceSurface
failureMatrix
regressionAndImmutability
nonGoals
```

Each section names exact types, inputs, outputs and behavioral tests.

## A. Exact package command boundary

Create:

```text
GeneratedCampaignRuntimeDispatchService
GeneratedCampaignRuntimeDispatchResult
```

This is a thin Application orchestrator over `IUnifiedGameRuntimeService`.

Input:

```text
exact GamePackageDefinition captured in GeneratedCampaignSession
exact UnifiedRuntimeSession
planned action
```

Output:

```text
UnifiedRuntimeResult
CommandKind
PackageSha256Before
PackageSha256After
PackageReferencePreserved
DefinitionIdsUsed[]
Diagnostics[]
```

Rules:

```text
serialize/hash the package before and after dispatch
the same package reference must be passed to Runtime
hashes must remain identical
PlayerCommand routes call ExecutePlayerCommand directly
GameRuntimeCommand routes call ExecuteGameplayCommand directly
no package clone
no temporary definitions
```

If package hash changes:

```text
campaign.package_mutated_during_dispatch
```

and the campaign enters FAILED without pretending success.

### A1. Basic attack

For `GeneratedCampaignActionKind.BasicAttack`:

```text
Runtime command type must remain BasicAttack
source/target IDs must be exactly the current encounter participants
```

No conversion to `UseAbility`.

The existing Runtime fallback/basic-attack resolution is authoritative.

### A2. Ability

For `UseAbility`:

```text
ability ID exists exactly once in package.Game.Abilities
current player encounter participant contains that ability in the exact encounter definition
target is a live opposing participant
```

Otherwise disable before dispatch:

```text
campaign.ability_not_available
campaign.ability_target_invalid
```

### A3. Combat readiness

Create:

```text
GeneratedCampaignCombatReadinessService
```

For each current-region encounter prove before exposing Start:

```text
encounter definition exists
at least one player participant
at least one opposing participant
participant resources refer to package resources
at least one player route:
  valid BasicAttack through existing Runtime contract
  or exact package ability
```

Do not execute Runtime during readiness projection.

When no exact route exists:

```text
encounter action disabled
campaign.encounter_no_executable_player_action
```

Do not mark the whole generated project not-ready when another encounter remains playable.

## B. Generated quest turn-in readiness

Create:

```text
GeneratedCampaignQuestReadinessService
GeneratedCampaignQuestReadiness
GeneratedCampaignQuestObjectiveReadiness
```

This is read-only. It evaluates actual current package/session state.

### B1. Generated turn-in quest classification

A quest is a generated turn-in quest when:

```text
quest.Kind == generated_quest
or quest.Tags contains generated
and GeneratedContent.Quests maps exactly to quest.Id
```

Require exact unique mapping.

Do not classify unrelated quests by substring IDs.

### B2. Supported generated objective kinds

Support the kinds currently emitted by generated MVP:

```text
complete_encounter
has_item
```

`complete_encounter` is satisfied only when:

```text
the matching encounter runtime state exists
the encounter is inactive
at least one player participant remains alive
all opposing participants are defeated
```

Fleeing or forced unresolved inactive state does not satisfy it.

`has_item` is satisfied from current player-owned inventory amount.

Required amount is data-derived from definition.

Unsupported required kinds:

```text
quest remains not ready
campaign.quest_objective_kind_unsupported:<kind>
```

### B3. Runtime objective state versus computed readiness

For generated turn-in quests:

```text
do not call RefreshQuestObjectives after combat/inventory actions
project objective rows from computed readiness
runtime quest remains active until player turn-in
```

When all required objectives are satisfied:

```text
show GeneratedCampaignActionKind.CompleteQuest
title: Завершить задание: <human title>
```

On action:

```text
re-evaluate readiness immediately
if no longer ready -> reject with zero Runtime command
execute existing CompleteQuest exactly once
```

After success:

```text
quest state completed
rewards/reputation applied by Runtime
turn-in action disappears
```

### B4. Non-generated quests

Keep Goal162 behavior:

```text
one bounded RefreshQuestObjectives after causal commands
normal Runtime auto-completion/stage behavior
```

Do not weaken legacy/custom quest semantics.

## C. Consequence model

Add to campaign models:

```text
GeneratedCampaignActionOutcome
GeneratedCampaignConsequence
GeneratedCampaignConsequenceKind
GeneratedCampaignConsequenceTimeline
```

Kinds:

```text
Dialogue
Damage
Healing
Status
EncounterStarted
EncounterWon
EncounterLost
EncounterFled
Reward
Inventory
QuestReady
QuestCompleted
Reputation
MapTravel
Save
Load
Migration
Failure
```

Outcome fields:

```text
ActionTitle
Success
Summary
Consequences[]
BeforeSessionSha256
AfterSessionSha256
RuntimeEventCount
Diagnostics[]
```

Consequence fields:

```text
Kind
Title
BeforeValue
AfterValue
Delta
Description
Positive/Negative/Neutral
```

No raw IDs in these human fields.

Technical IDs may appear only in the existing TechnicalDetails.

### C1. Delta projection

Create:

```text
GeneratedCampaignConsequenceProjector
```

Inputs:

```text
exact package
before session
after session
Runtime events
planned action
quest readiness before/after
save/migration result optional
```

Derive and correlate:

```text
participant resource changes
inventory stack changes
quest state/readiness changes
faction reputation changes
encounter active/outcome changes
map/region change
save revision/dedup result
migration preserved/dropped/reset counts
```

Do not infer success from a message alone.

Every displayed consequence must be supported by:

```text
state delta
or exact Runtime event
or save/migration typed result
```

### C2. Timeline

`GeneratedCampaignSessionService` stores a bounded in-memory timeline.

Bound:

```text
data-independent maximum from configuration constant, default 64 entries
```

This fixed UI retention limit is operational, not product content count.

On same-world exact Continue:

```text
rebuild available recent consequences from exact persisted Runtime events
plus one Load consequence
```

On migration:

```text
old transient Runtime events are reset by Goal161 policy
start a new timeline with one Migration consequence
show preserved/dropped/map-reset counts
```

## D. Campaign service integration

Refactor `GeneratedCampaignSessionService`:

```text
remove ClonePackage
remove campaign/session-compatible-attack
remove all action-time ResourceDefinition/AbilityDefinition mutation
inject runtime dispatch
inject quest readiness
inject consequence projector
```

Execution order:

1. capture current project truth;
2. plan action;
3. validate exact action readiness;
4. snapshot before session;
5. direct dispatch against exact package;
6. update session from Runtime result;
7. run bounded AI when appropriate;
8. for generated quests recompute readiness without Refresh;
9. for non-generated quests run at most one Refresh;
10. project action outcome/consequences;
11. return snapshot.

`GeneratedCampaignSnapshot` adds:

```text
LastActionOutcome
Consequences
```

### D1. Flee path

A player-facing Flee action must prove:

```text
encounter inactive
no encounter reward
generated quest encounter objective not ready
no reputation reward
consequence says encounter left
```

### D2. Victory path

A separate new session or encounter run proves:

```text
exact package combat
victory
reward item
quest ready but still active
manual turn-in action
CompleteQuest exactly once
quest completed
reputation changed
```

## E. Correct typed truth

Extend `GeneratedCampaignProjectTruth`:

```text
FinalStateHash
SelectedBuildHistorySha256
```

`GeneratedCampaignSessionTruthService` must use the existing current build-history reader/result to
obtain:

```text
actual current build FinalStateHash
exact selected history file SHA
```

Require both nonempty and correlated with current document/package truth.

`Same()` compares both.

Diagnostic:

```text
campaign.current_build_history_missing
campaign.current_final_state_mismatch
```

Do not parse an arbitrary history row by filename without current-correlation validation.

## F. UI

Add a visible `Последствия` tab/card to `GeneratedCampaignPageControl`.

Show:

```text
last action summary
ordered consequence rows
before → after / delta
human failure reason when action fails
```

Primary UI restrictions:

```text
no IDs containing /
no SHA-like 64-hex values
no absolute paths
no Runtime diagnostic codes
```

Technical details remain collapsed and may show correlation IDs/codes.

Encounter controls must distinguish:

```text
Обычная атака
actual package ability names
Покинуть встречу
```

Quest section shows:

```text
Активно
Готово к завершению
Завершено
```

and the manual button only when ready.


## G. Real automated matrix

Use real disposable Goal162 all-selectable and core-only projects.

### G1. Exact package combat audit

Before action record:

```text
package reference identity
package JSON SHA
ability/resource/status/stat definition inventories
```

Execute BasicAttack.

Require:

```text
IUnifiedGameRuntimeService received the exact same package reference
command observed = BasicAttack
package JSON SHA unchanged
definition inventories unchanged
no campaign/session-compatible-attack
no temporary definitions
target resource changed through Runtime
turn flow remains bounded
```

Execute one actual package ability when the encounter participant exposes one.

Require:

```text
ability exists in exact package
command observed = UseAbility
ability ID belongs to participant
package unchanged
```

### G2. Flee consequence path

Start a current-region encounter and flee before victory.

Require:

```text
EncounterFled consequence
no reward item delta
generated quest remains active/not ready
reputation unchanged
```

### G3. Victory and turn-in path

Start a fresh session.

Require:

```text
exact package BasicAttack/ability route
encounter victory
reward item delta
generated quest objective readiness computed true
runtime quest still active
manual Завершить задание action present
CompleteQuest command count before click=0
click action
CompleteQuest command count after click=1
quest completed
reputation delta
turn-in action absent afterward
```

### G4. Save and exact continue

After manual turn-in and travel:

```text
save CURRENT
clear/recreate campaign service
Continue exact with Runtime Start count 0
current map/inventory/quest/reputation hashes exact
consequence timeline rebuilt truthfully
package truth exact
```

### G5. Migration

After regeneration and explicit migration:

```text
old session stale
migration preview/apply
Migration consequence includes map reset and preserved/dropped counts
post-migration combat action still uses exact new package
post-migration travel/interact succeeds
```

### G6. Core-only

Require:

```text
exact package combat
travel
save/continue
generated quest behavior when a quest exists
AcceptedMechanics remains incomplete
no false RC readiness
```

No all-selectable assumptions.

## H. Tests

Create at least 42 Goal163 tests; at least 36 behavioral.

### Package boundary

1. BasicAttack dispatch uses exact package reference;
2. BasicAttack command type is not rewritten;
3. package SHA before/after equal;
4. ability/resource/status/stat inventories unchanged;
5. synthetic campaign ability absent;
6. fixed Power=3 campaign path absent;
7. exact package ability dispatch succeeds;
8. unavailable ability disabled;
9. invalid target disabled;
10. nonplayable encounter disabled causally.

### Quest turn-in

11. generated quest classification exact;
12. complete_encounter readiness victory true;
13. flee does not satisfy encounter objective;
14. has_item amount readiness exact;
15. unsupported required kind blocks readiness;
16. generated causal command does not call Refresh;
17. generated quest remains active when ready;
18. manual turn-in action appears;
19. readiness rechecked before dispatch;
20. CompleteQuest exactly once;
21. no AdvanceQuestObjective;
22. reward/reputation after manual action;
23. turn-in action disappears;
24. non-generated quest still refreshes at most once.

### Consequences

25. damage delta;
26. encounter started;
27. flee path no reward/reputation;
28. victory outcome;
29. inventory reward delta;
30. quest ready;
31. quest completed;
32. reputation delta;
33. map travel;
34. failed action consequence;
35. human consequence text no IDs/hashes/paths;
36. consequence bounded retention;
37. exact continue rebuilds timeline;
38. migration summary reset/preserved/dropped.

### Truth/UI/regression

39. actual FinalStateHash loaded from current history;
40. SelectedBuildHistorySha256 separate;
41. stale comparison catches either field;
42. Последствия page section;
43. manual turn-in UI status/button;
44. all-selectable real route;
45. core-only route;
46. Goal162 72/72 regressions GREEN;
47. Goal161T/S/R/Q/161 regressions GREEN;
48. Goal160/159/158/157 regressions GREEN;
49. Runtime Simulator unchanged;
50. generated save/migration regressions GREEN;
51. RC/standalone bytes unchanged;
52. Player/Unity/standalone counts zero.

A source-string assertion alone does not count as behavioral.

## I. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal163"
# require >=42 total / >=36 behavioral

dotnet test ... --filter "FullyQualifiedName~Goal163"
dotnet test ... --filter "FullyQualifiedName~Goal162"
dotnet test ... --filter "FullyQualifiedName~Goal161T"
dotnet test ... --filter "FullyQualifiedName~Goal161S"
dotnet test ... --filter "FullyQualifiedName~Goal161R"
dotnet test ... --filter "FullyQualifiedName~Goal161Q"
dotnet test ... --filter "FullyQualifiedName~Goal161"
dotnet test ... --filter "FullyQualifiedName~Goal160"
dotnet test ... --filter "FullyQualifiedName~Goal159"
dotnet test ... --filter "FullyQualifiedName~Goal158"
dotnet test ... --filter "FullyQualifiedName~Goal157"

dotnet test ... --filter "FullyQualifiedName~RuntimeSimulator"
dotnet test ... --filter "FullyQualifiedName~GeneratedGameplaySave"
dotnet test ... --filter "FullyQualifiedName~DefaultGameRuntime"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
dotnet test ... --filter "FullyQualifiedName~GameProjectOperationCoordinator"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then run the real matrix in section G.

Do not run:

```text
full suite
85-case closure
all-ProductSmoke
standalone Build
Player
Unity
visible app launch
```

A zero-match filter is failure.

## J. Immutability

Before the real matrix hash:

```text
project-local RC record
immutable standalone run tree
current pointer
standalone history
Goal142 source
goal148-manual source
```

After matrix require byte-identical.

Player/Unity/standalone invocation counts remain zero.

## K. Evidence

Create exactly 13 files in each mirrored root:

```text
goal163-dashboard.json
architecture-review.json
goal162-independent-audit-finding.json
package-truth-combat-proof.json
manual-quest-turn-in-proof.json
flee-versus-victory-proof.json
campaign-consequence-depth-proof.json
save-continue-consequence-proof.json
migration-consequence-proof.json
campaign-ui-proof.json
regression-immutability-proof.json
artifact-scope-proof.json
goal163-report.md
```

Roots:

```text
.llmgc/procedural/goal-163-package-truth-campaign-combat-turn-in-and-consequence-depth/
.llmgc/exports/goal-163-package-truth-campaign-combat-turn-in-and-consequence-depth/
```

Twins byte-identical.

Dashboard:

```text
status
candidateStatus
goal163TestsDiscovered
goal163BehavioralTestsPassed

goal162AuditBlockerRecorded
goal162AuditBlockerClosed

exactPackageReferencePassed
packageShaUnchanged
packageDefinitionInventoryUnchanged
basicAttackCommandObserved
basicAttackNotRewritten
syntheticCampaignAbilityAbsent
fixedCampaignPowerAbsent
exactPackageAbilityPassed

fleePathPassed
fleeRewardCount
fleeReputationDelta
fleeQuestReady=false

victoryPathPassed
victoryRewardReceived
generatedQuestReadyForTurnIn
generatedQuestStillActiveBeforeTurnIn
completeQuestCommandCountBefore
completeQuestCommandCountAfter
manualTurnInPassed
questCompletedAfterTurnIn
reputationChangedAfterTurnIn
advanceObjectiveCommandCount=0

damageConsequencePassed
rewardConsequencePassed
questReadyConsequencePassed
questCompletionConsequencePassed
reputationConsequencePassed
travelConsequencePassed
consequencePrimaryUiNoRawIds
consequenceTimelineBounded

actualFinalStateHashPassed
selectedBuildHistoryShaSeparated
sameWorldConsequenceContinuePassed
migrationConsequencePassed
postMigrationExactPackageCombatPassed

allSelectableRoutePassed
coreOnlyRoutePassed
coreOnlyNoFalseRcReady

releaseCandidateRecordByteIdentical
standaloneRunByteIdentical
standalonePointerByteIdentical
standaloneHistoryByteIdentical
playerProcessStartCount
unityEditorProcessStartCount
standaloneBuildInvocationCount

goal162RegressionPassed
goal161RegressionPassed
goal160RegressionPassed
goal159RegressionPassed
goal158RegressionPassed
goal157RegressionPassed
runtimeSimulatorRegressionPassed
generatedSaveRegressionPassed

goal142SourceByteIdentical
sourceGoal148ByteIdentical
artifactScopeViolationCount

goal163Accepted=false
goal163ManualReviewRequired=false
goal163IndependentAuditRequired=true
```

No required GREEN field may be null/PARTIAL/NOT_EXECUTED.


## L. State and docs

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/manual-acceptance/goal162-player-driven-generated-campaign-session.md
```

Create:

```text
docs/manual-acceptance/goal163-package-truth-campaign-consequences.md
```

No human gate.

Required GREEN state:

```text
goal162IndependentAuditResult=BLOCKED_AT_8164185B
goal162IndependentAuditBlocker=campaign_combat_executes_against_synthetic_runtime_package_and_manual_quest_turn_in_not_proven
goal162AuditBlocker=closed_by_goal163

goal162ImplementationStatus=GREEN
goal162CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal162Accepted=false
goal162IndependentAuditRequired=false

goal163ImplementationStatus=GREEN
goal163CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal163Accepted=false
goal163AcceptedByHuman=false
goal163AcceptedByCodex=false
goal163ManualReviewRequired=false
goal163ManualGateReady=false
goal163IndependentAuditRequired=true

goal163ExactPackageCombatPassed=true
goal163ManualQuestTurnInPassed=true
goal163ConsequenceDepthPassed=true
goal163SaveContinueConsequencePassed=true
goal163MigrationConsequencePassed=true
goal163CoreOnlyPassed=true
goal163PlayerProcessStartCount=0
goal163UnityEditorProcessStartCount=0
goal163StandaloneBuildInvocationCount=0
goal163ArtifactScopeViolationCount=0

nextAction=independent_goal163_audit_and_plan_campaign_choice_branching_or_failure_recovery
```

Release risk statement:

```text
The campaign player now executes combat against the exact qualified package with no Application-time
definition injection. Generated quest readiness is evaluated read-only and completed through one real
player turn-in command. Human consequences are correlated with state deltas and Runtime/save events.
Richer authored dialogue branching and long-lived consequence history remain future work.
```

Record any remaining entity-title suffix heuristic or finalizer lease concern as P2, not Goal163
blockers unless a mandatory behavioral test reproduces false player truth.

## M. Text integrity

Scan changed/task/evidence/docs:

```text
valid UTF-8
no NUL
no forbidden C0 except CR/LF/TAB
no mojibake markers
no escaped Cyrillic where forbidden
no absolute disposable paths in evidence
```

Historical evidence immutable.

## N. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal163-campaign-consequence-depth.ps1
.devflow/scripts/run-goal163-campaign-consequence-depth.cmd

src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionModels.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionTruthService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignActionPlanner.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignEventPresenter.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignProjectionService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignRuntimeDispatchService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignCombatReadinessService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignQuestReadinessService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignConsequenceProjector.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionService.cs

src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignPageControl.cs
src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/Goal163/Goal163PackageTruthCombatTests.cs
tests/LLMGameCreator.Tests/Application/Goal163/Goal163QuestTurnInTests.cs
tests/LLMGameCreator.Tests/Application/Goal163/Goal163FleeVictoryConsequenceTests.cs
tests/LLMGameCreator.Tests/Application/Goal163/Goal163SaveMigrationConsequenceTests.cs
tests/LLMGameCreator.Tests/Application/Goal163/Goal163CampaignTruthUiTests.cs
tests/LLMGameCreator.Tests/Application/Goal163/Goal163RegressionImmutabilityTests.cs
tests/LLMGameCreator.Tests/Application/Goal162/Goal162CampaignEncounterQuestTests.cs
tests/LLMGameCreator.Tests/Application/Goal162/Goal162CampaignTruthProjectionTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/manual-acceptance/goal162-player-driven-generated-campaign-session.md
docs/manual-acceptance/goal163-package-truth-campaign-consequences.md

docs/agent-tasks/goal-163-package-truth-campaign-combat-turn-in-and-consequence-depth/
.llmgc/procedural/goal-163-package-truth-campaign-combat-turn-in-and-consequence-depth/
.llmgc/exports/goal-163-package-truth-campaign-combat-turn-in-and-consequence-depth/
```

One exact additional existing Application campaign/build-history test/model path may be added only after
a concrete compile/test failure and with recorded reason.

Forbidden:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
catalogs/feature-modules/**
unity/**
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplaySave*
src/LLMGameCreator.Application/Generation/Procedural/SeededGenerated*
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/**
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectReleaseCandidate*
```

## O. Command budget

```text
read-first/architecture: 12 minutes
package dispatch/combat readiness: 18 minutes
quest readiness/manual turn-in: 20 minutes
consequence model/projector: 22 minutes
campaign/UI integration: 18 minutes
behavioral tests: 28 minutes
real matrix: 16 minutes
regressions/evidence/docs/scope: 18 minutes
target wall clock: 125 minutes
maximum two concurrent testhost processes
```

Rules:

```text
write test inventory before production edits
write evidence script before real matrix
no unchanged command retries
no timeout escalation
after failure isolate exact class/test
do not stop at compile success
P0/P1 fixed inside Goal163
P2/P3 debt only
```

## P. Publication

Create exactly one final commit:

```text
GREEN Goal 163 package truth campaign combat turn in and consequence depth
```

or honest BLOCKED/FAILED.

Codex must push `origin/main`.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three task files tracked
Player/Unity/standalone counts zero
RC/standalone bytes unchanged
Goal142 and goal148-manual unchanged
Goal162/163 accepted=false
no human gate
```

## Q. GREEN criteria

```text
Goal162 audit blockers recorded and closed
Goal163 >=42 discovered / >=36 behavioral / all pass
exact package reference and hash for every Runtime command
no package clone/transient ability/fixed campaign power
BasicAttack remains BasicAttack
actual package abilities exact
flee and victory consequences distinct
generated quest ready but active before turn-in
manual CompleteQuest exactly once
reward/reputation after turn-in
no AdvanceQuestObjective
human consequence surface
actual FinalStateHash and separate history SHA
save/continue/migration consequence truth
all-selectable/core-only routes
Goal162 and required regressions GREEN
RC/standalone immutable
Player/Unity/standalone zero
13+13 evidence
text integrity GREEN
artifact scope 0
one final commit pushed
```

## R. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning;
- Goal162 independent-audit intake;
- exact synthetic-package finding and removal;
- package reference/hash/definition inventory proof;
- BasicAttack and exact ability command proof;
- flee path;
- victory/reward path;
- generated objective readiness;
- manual turn-in and CompleteQuest count;
- reputation consequence;
- consequence UI/timeline;
- final-state/history truth;
- save/continue/migration;
- all-selectable/core-only;
- zero process and immutability;
- tests/regressions;
- evidence/text/artifact scope;
- state/no-human-gate;
- SHA/push/HEAD/worktree;
- confirmation Codex committed and pushed for any final status.
