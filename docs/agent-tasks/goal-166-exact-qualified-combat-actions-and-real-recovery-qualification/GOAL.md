# Goal 166 — Exact Qualified Combat Actions & Real Recovery Qualification

## Identity

- Task ID: `goal-166-exact-qualified-combat-actions-and-real-recovery-qualification`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `0027404db41cd5e5564aca03b83a92800a4e1016`
- Required base message: `GREEN Goal 165 combat route profile neutrality and campaign defeat recovery`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: Goal165 closes the route-mode truth table, but the exact package ability that qualified the
route is not persisted. A mixed ability list can contain an earlier successful utility action and a
later damage action; current qualification/victory selects the first merely successful ability. This
task records exact qualified combat actions, makes campaign tactical actions truthful and executes a
real end-to-end generated defeat/retry/save/new-game matrix through the production campaign service.

## Pre-approval and publication

- The owner approved the complete plan by launching this task.
- Do not ask for another confirmation because the task changes more than ten files.
- Produce a concise internal plan and proceed.
- Do not request manual testing.
- Own all P0/P1 defects reproduced by the Goal166 matrix.
- Record P2/P3 debt without creating Goal166A.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- On BLOCKED/FAILED, commit and push the honest state; never leave publication to the user.
- No intermediate commits.

## Initial worktree

After unpacking, only these untracked files are allowed:

```text
docs/agent-tasks/goal-166-exact-qualified-combat-actions-and-real-recovery-qualification/GOAL.md
docs/agent-tasks/goal-166-exact-qualified-combat-actions-and-real-recovery-qualification/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-166-exact-qualified-combat-actions-and-real-recovery-qualification/README.md
```

Require:

```text
HEAD == origin/main == 0027404db41cd5e5564aca03b83a92800a4e1016
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

Preserve RC record, immutable standalone run/current pointer, standalone history, Goal142/Goal148 and
all generation sidecars byte-identical.

## Goal165 independent-audit result

Record:

```text
goal165IndependentAuditResult=BLOCKED_AT_0027404D
goal165IndependentAuditBlocker=qualified_package_ability_identity_is_discarded_and_later_qualification_accepts_or_selects_the_first_successful_nonprogressing_ability
goal165IndependentAuditEvidenceGap=defeat_retry_save_and_new_game_evidence_is_not_end_to_end_through_generated_campaign_session_service
goal165AuditBlocker=closed_by_goal166 only on GREEN
```

Goal165 implementation remains GREEN and `accepted=false`; no human gate.

### Goal165 truths to preserve

```text
BASIC_ATTACK_ONLY / PACKAGE_ABILITY_ONLY / BOTH route-mode semantics
optional unavailable route passes vacuously
old Goal164 v4 compatibility
exact in-memory pre-encounter checkpoint model
DEFEATED state and recovery UI
stale checkpoint causal rejection
physical core-only portable copy without operational pointer
55/55 Goal165 tests
RC/standalone/source/sidecar immutability
Player/Unity/standalone counts zero
```

## Independent-audit P1

The resolver's package-ability probe:

```text
iterates participant abilities
executes each exact package ability
requires opposing Runtime health to decrease
returns only bool/session
```

It does not record which ability passed.

The contract stores:

```text
PlayerRole.Abilities = every ability on the source role
PackageAbilityAvailable=true
```

but no exact qualified ability ID/fingerprint.

Later per-encounter qualification:

```text
iterates all abilities
returns true on the first result.Success
does not require health/effect progression
```

Ability-only victory likewise returns the first successful ability. Therefore a valid role:

```text
ability/a_utility  -> success, no encounter progress
ability/z_damage   -> success, damages health
```

passes resolver but can stall or misreport qualification/victory.

## Recovery evidence gap and product safety

The Goal165 runner writes recovery facts as constants after general tests. Goal165 tests exercise the
recovery helper directly, but do not drive the real `GeneratedCampaignSessionService` through:

```text
generated map/travel
→ successful StartEncounter checkpoint
→ real Runtime defeat
→ DEFEATED snapshot
→ Retry action
→ exact restore
→ real second encounter
→ victory/manual turn-in
```

Goal166 must execute and capture that route.

Also handle defeat when no valid pre-encounter checkpoint exists, for example after loading a
mid-encounter save:

```text
Retry disabled with human reason
Continue from compatible save remains available
New Game remains available
no empty/stuck defeat screen
```

## Product outcome

```text
exact qualified combat action catalog
→ tactical action descriptions show real cost/effect/target
→ utility abilities remain usable but are not mistaken for the victory route
→ real generated defeat
→ Retry exact checkpoint
→ victory after retry
→ save recovery
→ new-game recovery
→ loaded active encounter can lose without trapping the player
```

No raw IDs/hashes/paths appear in primary UI.

## Non-goals

Do not change:

```text
Runtime or Runtime.Abstractions
GamePackage/domain schema
FeatureModule catalog
generated source/MVP/overlay/travel/combat overlay
generated save schema or migration implementation
Unity/standalone/RC implementation
combat balance/content values
```

Do not persist recovery checkpoints across application restarts in Goal166. That remains a future
product option.


## Mandatory architecture review

Read at most 17 primary files:

```text
GeneratedEncounterCombatContractModels.cs
GeneratedEncounterCombatContractService.cs
GameProjectGeneratedEncounterCombatQualificationService.cs
GameProjectBuildHistoryReader.cs
GameProjectSeedRegenerationCandidateSealService.cs
GameProjectSeedRegenerationCommitValidator.cs
GeneratedCampaignCombatReadinessService.cs
GeneratedCampaignActionPlanner.cs
GeneratedCampaignSessionService.cs
GeneratedCampaignRecoveryService.cs
GeneratedCampaignProjectionService.cs
GeneratedCampaignConsequenceProjector.cs
GeneratedCampaignPageControl.cs
Goal165CombatRouteNeutralityTests.cs
Goal165DefeatCheckpointTests.cs
Goal165RetryRecoveryTests.cs
Goal165SaveNewGameRecoveryTests.cs
```

Before production edits write:

```text
.llmgc/procedural/goal-166-exact-qualified-combat-actions-and-real-recovery-qualification/architecture-review.json
```

Required sections:

```text
goal165IndependentAudit
qualifiedCombatActionIdentity
mixedAbilityFailure
contractAndHistoryCompatibility
perEncounterActionValidation
tacticalActionProjection
realGeneratedDefeatRoute
retryExactRestoration
defeatWithoutCheckpoint
loadedSessionStatus
saveAndNewGameRecovery
staleCheckpoint
uiRecoverySurface
regressionImmutability
failureMatrix
nonGoals
```

Every section names exact types, inputs, outputs and behavioral tests.

## A. Exact qualified combat action catalog

Add types:

```text
GeneratedEncounterCombatQualifiedAction
GeneratedEncounterCombatQualifiedActionKind
GeneratedEncounterCombatObservedEffect
```

Action kinds:

```text
BASIC_ATTACK
PACKAGE_ABILITY
```

Fields:

```text
ActionKind
AbilityId technical, empty for BasicAttack
AbilityDefinitionSha256
SourceParticipantRoleFingerprint
ObservedEffect
TargetResourceIds[]
TargetStatIds[]
TargetStatusIds[]
RuntimeCommandType
RuntimeQualificationPassed
```

No timestamps/paths.

### A1. Resolver

For each candidate player role:

```text
execute BasicAttack against each live opposing candidate
execute every participant-owned exact package ability against each valid opposing target
record an action only when Runtime success produces an actual supported encounter delta
```

A supported player-route delta is:

```text
opposing Runtime health decreases
or opposing stat changes through an exact package effect
or opposing status is added/changed through an exact package effect
```

Healing the enemy, no-op success, dialogue/events unrelated to encounter combat and self-only utility
do not qualify the victory route.

Record all exact qualified action descriptors in deterministic order:

```text
ActionKind
AbilityId
AbilityDefinitionSha256
observed effect fingerprint
```

`PackageAbilityAvailable=true` only when at least one exact package ability descriptor exists.

Contract ID includes the full catalog.

### A2. BasicAttack

When BasicAttack qualifies, store one `BASIC_ATTACK` descriptor with the observed exact health/resource
contract.

No fixed resource ID is introduced; use the actual Runtime-qualified target resources.

### A3. Package ability

Store the exact ability IDs and canonical fingerprints that qualified.

Do not persist merely successful utility abilities in the victory-route catalog.

The role may retain utility abilities in `PlayerRole.Abilities`; the qualified catalog is a separate
truth.

### A4. Route mode

Derive route mode from the catalog:

```text
basic descriptor only -> BASIC_ATTACK_ONLY
one or more package ability descriptors only -> PACKAGE_ABILITY_ONLY
both -> BOTH
empty -> NONE
```

Keep Goal165 required/vacuous truth semantics.

## B. Per-encounter qualification

`GameProjectGeneratedEncounterCombatQualificationService` must use the contract catalog, not the raw
role ability list.

### B1. Required route checks

For each generated encounter:

```text
BasicAttack required:
  execute BasicAttack
  require the same supported effect class observed by the contract

Package ability required:
  iterate only Qualified PACKAGE_ABILITY descriptors
  require at least one exact descriptor to produce a supported effect
```

A merely successful no-op/utility ability does not set `PackageAbilityPassed=true`.

### B2. Victory action selection

During automatic representative victory:

```text
BASIC_ATTACK_ONLY -> exact BasicAttack descriptor
PACKAGE_ABILITY_ONLY -> deterministic first qualified descriptor that progresses current target
BOTH -> deterministic primary route:
  BasicAttack first when currently executable and progressing;
  otherwise exact qualified package ability
```

After every player command require:

```text
Runtime success
and encounter progress:
  target health/stat/status delta
  or target defeated
  or encounter completed
```

If a qualified ability is temporarily unavailable due to cost/cooldown:

```text
try another qualified descriptor
or EndTurn only when the existing Runtime can replenish/advance causally
```

Bound all attempts from actual participant/resource/turn state.

Diagnostics:

```text
generated_combat.qualified_action_missing
generated_combat.qualified_action_definition_changed
generated_combat.qualified_action_no_progress
generated_combat.victory_no_progress
```

### B3. Summary/history/seal

Add:

```text
QualifiedActionCount
QualifiedBasicAttackCount
QualifiedPackageAbilityCount
QualifiedActionsSha256
QualifiedActions[]
```

to the contract/summary as appropriate.

History v4:

```text
new rows require exact action catalog
old Goal165/Goal164 v4 rows with empty catalog remain compatible only when legacy route booleans are
internally consistent
```

Candidate seal and semantic validator cover exact catalog hash and count.

Tampering any ability ID/fingerprint/effect rejects the candidate.

## C. Tactical campaign actions

Add player-facing tactical projection, naming flexible:

```text
GeneratedCampaignTacticalAction
```

Fields:

```text
Title
TargetTitle
CostSummary
EffectSummary
AvailabilitySummary
ProgressesEncounter
Primary
```

Derive from exact package definitions and current Runtime state.

### C1. Action planner

The campaign planner may expose:

```text
qualified encounter-progressing actions
valid utility/support actions
```

but it must distinguish them:

```text
progressing action -> primary combat action
utility action -> secondary/support action
```

Do not label a utility action as the package ability that qualified the build contract.

Descriptions may show:

```text
resource cost
cooldown
damage/healing/status/stat effect
target
disabled reason
```

No raw IDs or formulas in primary UI.

### C2. Mixed ability route

Real tactical matrix:

```text
utility ability sorts before damage ability
both Runtime commands succeed
only damage ability is in qualified victory catalog
UI may show both with human descriptions
automated victory chooses the damage ability
```

## D. Real end-to-end defeat/recovery certification

Create a real Goal166 product fixture using:

```text
Goal164 current all-selectable generated project
real final package
GeneratedCampaignSessionService
actual IUnifiedGameRuntimeService
actual generated map/travel/action planner
actual generated save services
```

Do not prove recovery only through `GeneratedCampaignRecoveryService` unit calls.

### D1. Genuine defeat

Data-derived route:

1. start new campaign;
2. navigate/travel to a reachable generated encounter;
3. save the exact pre-encounter state in a CURRENT slot;
4. start encounter through the human action;
5. require checkpoint committed;
6. deliberately choose valid low-progress/end-turn actions until actual Runtime defeat;
7. derive the bound from player health, opponent effect and participant count;
8. require `DEFEATED`.

No direct session mutation.

Capture:

```text
pre-encounter session/hash
defeated session/hash
Runtime command sequence
checkpoint
snapshot/actions/consequences
```

### D2. Retry

Execute the actual `RetryEncounter` action from the defeated snapshot.

Require:

```text
Runtime Start delta=0
StartEncounter delta=1
checkpoint session restored exactly before StartEncounter
same encounter technical identity
map/position/inventory/quest/reputation exact
lost-attempt state removed
status ACTIVE or DEFEATED only if opponent legitimately defeats again during the immediate bounded AI
Retry consequence correlated
```

Then use tactical progressing actions to win and manually turn in the generated quest.

### D3. Save recovery

Fresh route:

```text
CURRENT save before encounter
real defeat
execute RecoveryLoad action
exact saved session restored
Runtime Start delta=0
checkpoint cleared
status derived from loaded session
```

### D4. New game recovery

Fresh route:

```text
real defeat
execute NewGame recovery action
Runtime Start delta=1
checkpoint cleared
generated start map
ACTIVE
NewGame consequence
```

### D5. Stale checkpoint

After real defeat, change current world/package/authoring truth through an isolated project copy or
approved test seam.

Execute Retry:

```text
zero Runtime commands
STALE_PROJECT
checkpoint invalidated
causal human reason
```


## E. Defeat without checkpoint and loaded-session truth

### E1. Recovery projection without checkpoint

A defeated session may lack a pre-encounter checkpoint, for example after loading a mid-encounter
save.

The recovery UI must never be empty.

Projection rules:

```text
DEFEATED + valid checkpoint:
  Retry available
  Continue based on saves
  New Game available

DEFEATED + no checkpoint:
  Retry disabled with human reason
  Continue based on saves
  New Game available
```

`GeneratedCampaignRecoveryService.Project()` must accept the current campaign status and save
availability; do not return a completely empty projection merely because checkpoint is absent.

### E2. Continue status derivation

After exact save load derive status from the loaded Runtime session:

```text
defeated encounter state -> DEFEATED
active/normal state -> ACTIVE
```

Do not unconditionally mark every loaded session ACTIVE.

If the loaded state is DEFEATED and no checkpoint exists:

```text
Retry disabled
Continue/New Game remain available
```

### E3. Save safety

The production UI already disables Save in DEFEATED.

The Application service must also reject direct save requests when status is not ACTIVE:

```text
campaign.save_not_available
```

This prevents a caller from creating a new defeated-state save through the service API.

Existing saves remain immutable and readable.

## F. Consequences and UI

Add consequence kinds only when not already present:

```text
TacticalAction
```

Keep Goal165:

```text
Defeat
Retry
RecoveryLoad
NewGame
```

### F1. Tactical consequence

After an ability/basic attack show state/event-derived:

```text
action title
target title
resource/stat/status before → after
cost paid
cooldown/status result
```

No success row for a no-op command.

### F2. Recovery UI

At 1100x720:

```text
defeat title
encounter title
Retry / Continue / New Game
human disabled reason when checkpoint missing/stale
tactical action descriptions and tooltips
```

No raw IDs, SHA values, absolute paths or diagnostic codes outside Technical Details.

Keyboard map shortcuts execute zero commands in DEFEATED.

## G. Real route matrices

### G1. Both-route regression

Real Goal164 all-selectable/core-only:

```text
qualified action catalog nonempty
BasicAttack descriptor present
package ability descriptor present
real generated victory/turn-in/travel/save remains GREEN
```

### G2. Basic-only

Use Goal165 basic-only fixture.

Require:

```text
catalog contains BasicAttack only
qualification CAMPAIGN_CURRENT
campaign planner exposes no package ability
victory/manual turn-in
```

### G3. Ability-only mixed list

Create a test fixture:

```text
ability/a_utility succeeds but does not progress encounter
ability/z_damage succeeds and damages exact Runtime health
BasicAttack cannot progress
```

Require:

```text
catalog contains z_damage only
a_utility absent from qualified catalog
route mode PACKAGE_ABILITY_ONLY
per-encounter qualification uses z_damage
representative victory uses z_damage
CAMPAIGN_CURRENT
```

Production code must not know these fixture IDs.

### G4. Utility-first BOTH

A both-route role with an earlier utility ability:

```text
BasicAttack and z_damage qualified
utility omitted from qualified catalog
victory remains deterministic
```

### G5. Neither

No qualified actions:

```text
resolver/build rejected
no synthetic fallback
```

### G6. Physical core-only portable copy

Repeat the Goal165 physical copy proof:

```text
new project path
no operational current pointer
CAMPAIGN_CURRENT
save truth current
AcceptedMechanics incomplete
no false RC readiness
zero execution
```

## H. Required tests

Create at least 48 Goal166 tests; at least 42 behavioral.

### Exact action catalog

1. basic descriptor recorded;
2. exact damage ability ID recorded;
3. canonical ability fingerprint recorded;
4. utility successful no-op excluded;
5. mixed ability ordering does not change catalog;
6. catalog deterministic;
7. contract ID changes when qualified ability changes;
8. basic-only catalog;
9. ability-only catalog;
10. both catalog;
11. neither rejected;
12. no fixed production IDs;
13. no package mutation;
14. exact package reference preserved.

### Qualification/history/seal

15. package ability pass requires exact qualified ID;
16. successful nonprogressing utility is not pass;
17. ability-only victory selects progressing ability;
18. both victory skips nonprogressing utility;
19. per-encounter supported effect verified;
20. cooldown/cost fallback uses another qualified action;
21. no-progress bound fails causally;
22. summary catalog count/hash exact;
23. new v4 history exact catalog current;
24. Goal165 v4 legacy row compatible;
25. candidate seal covers catalog;
26. ability fingerprint tamper rejected;
27. catalog count/hash tamper rejected.

### Tactical UX

28. tactical basic description;
29. tactical ability cost/effect/target;
30. utility marked secondary;
31. progressing action primary;
32. disabled cost/cooldown reason human;
33. primary UI no IDs/hashes/paths/codes.

### Real recovery

34. real generated StartEncounter captures checkpoint;
35. actual Runtime defeat reaches DEFEATED;
36. Defeat consequence exact;
37. defeated actions hide map actions;
38. Retry Runtime Start delta zero;
39. Retry StartEncounter delta one;
40. checkpoint exact before retry start;
41. inventory/quest/reputation/map restored;
42. lost attempt state removed;
43. victory/manual turn-in after retry;
44. save recovery exact with Start delta zero;
45. new game recovery Start delta one;
46. stale retry zero Runtime commands;
47. loaded defeated save derives DEFEATED;
48. defeat without checkpoint still shows Continue/New Game;
49. Retry disabled without checkpoint;
50. service rejects save while DEFEATED.

### Regressions/immutability

51. Goal165 55/55 GREEN;
52. Goal164 61/61 GREEN;
53. Goal163/162/161 regressions GREEN;
54. Runtime Simulator unchanged;
55. physical core-only portable copy;
56. RC/standalone/source/sidecars byte-identical;
57. Player/Unity/standalone counts zero.

No source-string-only assertion counts as behavioral product proof.

## I. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal166"
# require >=48 total / >=42 behavioral

dotnet test ... --filter "FullyQualifiedName~Goal166"
dotnet test ... --filter "FullyQualifiedName~Goal165"
dotnet test ... --filter "FullyQualifiedName~Goal164"
dotnet test ... --filter "FullyQualifiedName~Goal163"
dotnet test ... --filter "FullyQualifiedName~Goal162"
dotnet test ... --filter "FullyQualifiedName~Goal161"
dotnet test ... --filter "FullyQualifiedName~Goal160"
dotnet test ... --filter "FullyQualifiedName~Goal159"
dotnet test ... --filter "FullyQualifiedName~Goal158"
dotnet test ... --filter "FullyQualifiedName~Goal157"

dotnet test ... --filter "FullyQualifiedName~GeneratedCampaign"
dotnet test ... --filter "FullyQualifiedName~GeneratedGameplaySave"
dotnet test ... --filter "FullyQualifiedName~RuntimeSimulator"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~GameProjectOperationCoordinator"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then execute the real matrices in sections D and G.

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


## J. Evidence

Create exactly 12 files in each mirrored root:

```text
goal166-dashboard.json
architecture-review.json
goal165-independent-audit-finding.json
qualified-combat-action-catalog-proof.json
mixed-ability-qualification-proof.json
tactical-combat-ui-proof.json
real-defeat-retry-proof.json
save-new-game-recovery-proof.json
defeat-without-checkpoint-proof.json
regression-immutability-proof.json
artifact-scope-proof.json
goal166-report.md
```

Roots:

```text
.llmgc/procedural/goal-166-exact-qualified-combat-actions-and-real-recovery-qualification/
.llmgc/exports/goal-166-exact-qualified-combat-actions-and-real-recovery-qualification/
```

Twins byte-identical.

### Dashboard fields

```text
status
candidateStatus
goal166TestsDiscovered
goal166BehavioralTestsPassed

goal165AuditBlockerRecorded
goal165AuditBlockerClosed
goal165RecoveryEvidenceGapClosed

qualifiedActionCatalogPassed
qualifiedBasicActionCount
qualifiedPackageAbilityCount
qualifiedActionsSha256
mixedAbilityUtilityExcluded
mixedAbilityDamageSelected
catalogDeterministic
catalogSealTamperRejected

bothRoutePassed
basicOnlyRoutePassed
abilityOnlyMixedRoutePassed
neitherRouteRejected
legacyV4CompatibilityPassed

realGeneratedDefeatReached
realDefeatRuntimeCommandCount
checkpointCapturedBeforeStart
defeatConsequencePassed
retryPassed
retryRuntimeStartDelta
retryStartEncounterDelta
retryMapExact
retryInventoryExact
retryQuestExact
retryReputationExact
lostAttemptStateRemoved
victoryAfterRetryPassed
manualTurnInAfterRetryPassed

saveRecoveryPassed
saveRecoveryRuntimeStartDelta
newGameRecoveryPassed
newGameRuntimeStartDelta
staleCheckpointRejected
staleRetryRuntimeCommandCount

loadedDefeatedSaveStatusPassed
defeatWithoutCheckpointRecoveryAvailable
retryDisabledWithoutCheckpoint
continueOrNewGameAvailableWithoutCheckpoint
defeatedSaveRejected

tacticalUiPassed
tacticalPrimaryUiNoRawIds
physicalCoreOnlyPortableCopyPassed
portableCoreOnlyOperationalPointerAbsent

goal165RegressionPassed
goal164RegressionPassed
goal163RegressionPassed
goal162RegressionPassed
goal161RegressionPassed
runtimeSimulatorRegressionPassed

releaseCandidateRecordByteIdentical
standaloneRunByteIdentical
standalonePointerByteIdentical
standaloneHistoryByteIdentical
goal142SourceByteIdentical
sourceGoal148ByteIdentical
generationSidecarsByteIdentical
playerProcessStartCount
unityEditorProcessStartCount
standaloneBuildInvocationCount
artifactScopeViolationCount

goal166Accepted=false
goal166ManualReviewRequired=false
goal166IndependentAuditRequired=true
```

No GREEN-required field may be null/PARTIAL/NOT_EXECUTED.

## K. State and docs

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
docs/manual-acceptance/goal165-combat-profile-neutrality-defeat-recovery.md
```

Create:

```text
docs/manual-acceptance/goal166-exact-combat-actions-real-recovery.md
```

No human gate.

Required GREEN state:

```text
goal165IndependentAuditResult=BLOCKED_AT_0027404D
goal165IndependentAuditBlocker=qualified_package_ability_identity_is_discarded_and_later_qualification_accepts_or_selects_the_first_successful_nonprogressing_ability
goal165IndependentAuditEvidenceGap=defeat_retry_save_and_new_game_evidence_is_not_end_to_end_through_generated_campaign_session_service
goal165AuditBlocker=closed_by_goal166

goal165ImplementationStatus=GREEN
goal165CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal165Accepted=false
goal165IndependentAuditRequired=false

goal166ImplementationStatus=GREEN
goal166CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal166Accepted=false
goal166AcceptedByHuman=false
goal166AcceptedByCodex=false
goal166ManualReviewRequired=false
goal166ManualGateReady=false
goal166IndependentAuditRequired=true

goal166QualifiedActionCatalogPassed=true
goal166MixedAbilityQualificationPassed=true
goal166RealDefeatRetryPassed=true
goal166SaveRecoveryPassed=true
goal166NewGameRecoveryPassed=true
goal166DefeatWithoutCheckpointPassed=true
goal166PlayerProcessStartCount=0
goal166UnityEditorProcessStartCount=0
goal166StandaloneBuildInvocationCount=0
goal166ArtifactScopeViolationCount=0

nextAction=independent_goal166_audit_and_plan_campaign_choice_branching
```

Release risk statement:

```text
Combat contracts now retain the exact BasicAttack/package-ability actions that actually progressed an
encounter. Mixed utility and damage abilities no longer produce false qualification or deterministic
victory stalls. Recovery is proven end-to-end through the production campaign service, including
defeat without a retry checkpoint. Recovery checkpoints remain in-memory and are lost on application
restart by design.
```

## L. Text integrity

Scan changed/task/evidence/docs files:

```text
valid UTF-8
no NUL
no forbidden C0 except CR/LF/TAB
no mojibake markers
no escaped Cyrillic where forbidden
no absolute disposable paths in evidence
```

Historical evidence immutable.

## M. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal166-exact-actions-recovery.ps1
.devflow/scripts/run-goal166-exact-actions-recovery.cmd

src/LLMGameCreator.Application/Generation/Procedural/GeneratedEncounterCombatContractModels.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedEncounterCombatContractService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedEncounterCombatQualificationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCandidateSealService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCommitValidator.cs

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildHistoryReader.cs

src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionModels.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignCombatReadinessService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignActionPlanner.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignRecoveryService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignProjectionService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignConsequenceProjector.cs

src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignPageControl.cs
src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/Goal166/Goal166QualifiedActionCatalogTests.cs
tests/LLMGameCreator.Tests/Application/Goal166/Goal166MixedAbilityQualificationTests.cs
tests/LLMGameCreator.Tests/Application/Goal166/Goal166RealDefeatRetryTests.cs
tests/LLMGameCreator.Tests/Application/Goal166/Goal166SaveNewGameRecoveryTests.cs
tests/LLMGameCreator.Tests/Application/Goal166/Goal166DefeatWithoutCheckpointTests.cs
tests/LLMGameCreator.Tests/Application/Goal166/Goal166TacticalCombatUiTests.cs
tests/LLMGameCreator.Tests/Application/Goal166/Goal166RegressionImmutabilityTests.cs

tests/LLMGameCreator.Tests/Application/Goal165/Goal165CombatRouteNeutralityTests.cs
tests/LLMGameCreator.Tests/Application/Goal165/Goal165RetryRecoveryTests.cs
tests/LLMGameCreator.Tests/Application/Goal165/Goal165SaveNewGameRecoveryTests.cs
tests/LLMGameCreator.Tests/Application/Goal164/Goal164GeneratedCampaignRouteTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/manual-acceptance/goal165-combat-profile-neutrality-defeat-recovery.md
docs/manual-acceptance/goal166-exact-combat-actions-real-recovery.md

docs/agent-tasks/goal-166-exact-qualified-combat-actions-and-real-recovery-qualification/
.llmgc/procedural/goal-166-exact-qualified-combat-actions-and-real-recovery-qualification/
.llmgc/exports/goal-166-exact-qualified-combat-actions-and-real-recovery-qualification/
```

One exact additional existing campaign/history test/model path may be added only after a concrete
compile/test failure and with a recorded reason.

Forbidden:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
catalogs/feature-modules/**
unity/**
src/LLMGameCreator.Application/Generation/Procedural/GeneratedProjectOverlay*
src/LLMGameCreator.Application/Generation/Procedural/GeneratedWorldEncounterCombatOverlay*
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplaySave*
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/**
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectReleaseCandidate*
```

## N. Command budget

```text
read-first/architecture: 10 minutes
qualified action catalog/history: 20 minutes
mixed ability/tactical UI: 18 minutes
real defeat/recovery integration: 24 minutes
behavioral tests: 28 minutes
real matrix: 16 minutes
regressions/evidence/docs/scope: 18 minutes
target wall clock: 115 minutes
maximum two concurrent testhost processes
Player/Unity/standalone counts: 0
```

Rules:

```text
write complete test inventory before production edits
write evidence script before real matrix
do not ask for plan confirmation
no unchanged command retries
no timeout escalation
after failure isolate exact class/test
P0/P1 fixed inside Goal166
P2/P3 debt only
do not stop at compile success
do not defer evidence/docs/scope
```

## O. Publication

Create exactly one final commit:

```text
GREEN Goal 166 exact qualified combat actions and real recovery qualification
```

or honest BLOCKED/FAILED.

Codex must push `origin/main`.

Required final:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three task files tracked
Player/Unity/standalone counts zero
RC/standalone/source/sidecar bytes unchanged
Goal165/166 accepted=false
no human gate
```

## P. GREEN criteria

```text
Goal165 audit blocker/evidence gap recorded and closed
Goal166 >=48 discovered / >=42 behavioral / all pass
exact qualified BasicAttack/ability action catalog
mixed utility ability excluded from victory catalog
deterministic damaging ability selected
catalog persisted in v4 history/seal
both/basic-only/ability-only/neither truthful
tactical human action projection
real generated Runtime defeat through production session service
real Retry exact restore with Start delta zero
victory/manual turn-in after retry
real save recovery and new-game recovery
defeat without checkpoint remains recoverable
loaded defeated save derives DEFEATED
service rejects defeated save
physical core-only portable copy
required regressions GREEN
RC/standalone/source/sidecars immutable
Player/Unity/standalone zero
12+12 evidence
text integrity GREEN
artifact scope 0
one final commit pushed
```

## Q. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning;
- Goal165 audit intake;
- exact qualified action catalog;
- mixed utility/damage ability result;
- route/history/seal matrix;
- tactical UI result;
- actual generated defeat command route;
- checkpoint/retry exact restoration;
- victory/manual turn-in after retry;
- save/new-game recovery;
- defeat without checkpoint and loaded defeated save;
- process counts and immutability;
- tests/regressions;
- evidence/text/artifact scope;
- state/no-human-gate;
- SHA/push/HEAD/worktree;
- confirmation Codex committed and pushed on any final status.
