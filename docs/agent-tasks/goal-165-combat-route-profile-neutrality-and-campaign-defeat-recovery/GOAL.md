# Goal 165 — Combat Route Profile Neutrality & Campaign Defeat Recovery

## Identity

- Task ID: `goal-165-combat-route-profile-neutrality-and-campaign-defeat-recovery`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `15a8f2abea06d3e7caa67f68f7d690708f612459`
- Required base message: `GREEN Goal164 generated encounter combat contract`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: this is a visible campaign vertical slice with one exact independent-audit P1. Goal164 already
established the build-time combat contract and real generated campaign. Goal165 must make that
contract neutral between BasicAttack-only, ability-only and both-route profiles, then add a truthful
player recovery workflow after defeat. No Runtime, GamePackage or save-schema change is required.

## Pre-approval and publication

- The owner approved execution by launching this task.
- Produce a concise internal plan; do not ask for confirmation.
- Do not request manual testing.
- Own all P0/P1 defects reproduced by the Goal165 matrix.
- Record P2/P3 debt without creating Goal165A.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- On BLOCKED/FAILED, commit and push the honest state; never leave publication to the user.
- No intermediate commits.

## Initial worktree

After unpacking, only these untracked files are allowed:

```text
docs/agent-tasks/goal-165-combat-route-profile-neutrality-and-campaign-defeat-recovery/GOAL.md
docs/agent-tasks/goal-165-combat-route-profile-neutrality-and-campaign-defeat-recovery/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-165-combat-route-profile-neutrality-and-campaign-defeat-recovery/README.md
```

Require:

```text
HEAD == origin/main == 15a8f2abea06d3e7caa67f68f7d690708f612459
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

## Goal164 independent-audit result

Record:

```text
goal164IndependentAuditResult=BLOCKED_AT_15A8F2AB
goal164IndependentAuditBlocker=combat_contract_resolver_accepts_single_player_route_but_qualification_and_history_unconditionally_require_basic_attack_and_package_ability
goal164AuditBlocker=closed_by_goal165 only on GREEN
goal164IndependentAuditSecondaryFinding=portable_core_only_claim_was_not_proven_on_a_physical_copy_without_operational_output
```

Goal164 implementation remains GREEN and `accepted=false`; no human gate.

### Secondary evidence gap

The real smoke creates and reopens a physical all-selectable portable copy. The core-only Goal164 test
builds and reads the same project through a capturing standalone service, but does not copy it to a new
project path and prove absence of the machine-local current pointer/run. Goal165 must add a real
physical core-only portable-copy regression:

```text
new project path
no operational pointer for the copied path
CAMPAIGN_CURRENT
generated save truth current
AcceptedMechanics incomplete
no false RC CURRENT/READY/PENDING
no execution
```

### Goal164 truths to preserve

```text
deterministic Lane A combat-contract resolution
build-time generated encounter overlay
controlled delta limited to generated participant combat fields
no definitions added/removed
exact package Runtime dispatch
v4 CAMPAIGN_CURRENT history
real generated flee/victory/reward/manual turn-in/travel
regeneration/history rollback combat current
save migration and post-migration generated combat
one cached standalone smoke
RC CURRENT
portable all-selectable/core-only
61/61 Goal164 tests
```

## Independent-audit P1

The contract resolver correctly defines a player route as:

```text
BasicAttack succeeds
OR
participant-owned exact package ability succeeds
```

It can therefore select a legitimate role with:

```text
BasicAttack=true, abilities=[]
```

or:

```text
BasicAttack=false, exact package ability=true
```

But `GameProjectGeneratedEncounterCombatQualificationService` currently requires:

```text
allBasic=true
AND
contract.PlayerRole.Abilities.Count > 0
AND
all package-ability routes=true
```

Every per-encounter route also requires both `basic` and `ability`.

`GameProjectBuildHistoryReader.CombatEligible()` likewise requires:

```text
BasicAttackPassed=true
PackageAbilityPassed=true
```

Therefore valid single-route contracts are accepted by the resolver but rejected by build/history.

This violates the Goal164 contract and the repository policy that mechanics/content counts are
profile-defined.

## Required profile-neutral combat truth

Add typed fields:

```text
GeneratedEncounterCombatPlayerRouteMode
  BASIC_ATTACK_ONLY
  PACKAGE_ABILITY_ONLY
  BASIC_ATTACK_AND_PACKAGE_ABILITY

BasicAttackAvailable
BasicAttackRequired
BasicAttackPassed
PackageAbilityAvailable
PackageAbilityRequired
PackageAbilityPassed
PlayerRoutePassed
```

Apply them to:

```text
GeneratedEncounterCombatContractQualificationSummary
GeneratedEncounterCombatContract
GameProjectGeneratedEncounterCombatSummary
```

### Rules

```text
BasicAttack-only:
  BasicAttackAvailable=true
  BasicAttackRequired=true
  BasicAttackPassed=true
  PackageAbilityAvailable=false
  PackageAbilityRequired=false
  PackageAbilityPassed=true as vacuous/nonrequired truth
  PlayerRoutePassed=true

Ability-only:
  BasicAttackAvailable=false
  BasicAttackRequired=false
  BasicAttackPassed=true as vacuous/nonrequired truth
  PackageAbilityAvailable=true
  PackageAbilityRequired=true
  PackageAbilityPassed=true
  PlayerRoutePassed=true

Both:
  both Available/Required/Passed=true
  PlayerRoutePassed=true

Neither:
  PlayerRoutePassed=false
  contract/build blocked causally
```

Do not use false to mean “not applicable”.

Diagnostics:

```text
generated_combat.basic_attack_required_failed
generated_combat.package_ability_required_failed
generated_combat.player_route_missing
```

Human facts report one of:

```text
Обычная атака
Способность
Обычная атака и способность
```

according to the actual mode.

## History compatibility

Keep schema `unified_game_project_build_history_v4`.

New v4 rows persist the new route fields.

Reader behavior:

```text
new row with route mode:
  evaluate conditional Required/Passed truth

old Goal164 v4 row with empty route mode:
  infer legacy BOTH only when both old booleans are true
  otherwise reject causally

v3:
  COMBAT_PENDING as before
```

Regeneration seal and semantic validation already hash the complete combat summary; ensure new fields
are covered and exact.

## Product outcome: defeat and recovery

The player-facing campaign gains a truthful defeat state:

```text
generated encounter
→ player loses
→ Поражение
→ Последствия show defeat and resource changes
→ choose:
    Повторить встречу
    Продолжить с сохранения
    Начать новую игру
```

`Повторить встречу` restores an exact in-memory pre-encounter checkpoint and starts the same encounter
again through existing Runtime commands.

No reward, quest progress or reputation from the lost attempt may survive the retry.

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

Do not persist recovery checkpoints into gameplay saves in Goal165.


## Mandatory architecture review

Read at most 16 primary files:

```text
GeneratedEncounterCombatContractModels.cs
GeneratedEncounterCombatContractService.cs
GameProjectGeneratedEncounterCombatQualificationService.cs
GameProjectBuildHistoryReader.cs
GameProjectWorkspaceModels.cs
GameProjectSeedRegenerationCandidateSealService.cs
GameProjectSeedRegenerationCommitValidator.cs
GeneratedCampaignSessionModels.cs
GeneratedCampaignSessionService.cs
GeneratedCampaignActionPlanner.cs
GeneratedCampaignProjectionService.cs
GeneratedCampaignConsequenceProjector.cs
GeneratedCampaignPageControl.cs
Goal164CombatContractResolutionTests.cs
Goal164GeneratedCampaignRouteTests.cs
Goal164StandaloneAndPortabilityTests.cs
```

Before production edits write:

```text
.llmgc/procedural/goal-165-combat-route-profile-neutrality-and-campaign-defeat-recovery/architecture-review.json
```

Required sections:

```text
goal164IndependentAudit
routeModeTruthTable
contractResolution
perEncounterQualification
historyCompatibility
regenerationSealCompatibility
preEncounterCheckpoint
defeatDetection
retryTransaction
saveAndNewGameRecovery
worldChangeInvalidation
consequenceProjection
uiRecoverySurface
failureMatrix
regressionImmutability
nonGoals
```

Every section names exact types, inputs, outputs and behavioral tests.

## A. Route-mode implementation

### A1. Contract resolution

For each candidate role pair, independently run:

```text
BasicAttack qualification
participant-owned exact package ability qualification
opponent AI qualification
```

Determine `PlayerRouteMode` from successful routes.

Candidate is valid when:

```text
PlayerRoutePassed=true
OpponentAiPassed=true
exact package reference/hash unchanged
```

Do not require an ability merely because the participant lists an ability that is not executable.

`PackageAbilityAvailable` means at least one participant-owned ability exists and passes actual Runtime
qualification.

`BasicAttackAvailable` means actual Runtime BasicAttack changes an opposing Runtime-health resource.

### A2. Deterministic selection

Keep existing canonical ordering.

The selected contract may be:

```text
basic-only
ability-only
both
```

Contract ID includes the route mode and all route truth fields.

### A3. Overlay

The generated participant overlay remains unchanged in structure:

```text
copy exact selected role combat fields
add/remove no package definitions
```

For ability-only contracts, generated player actions expose only package abilities.
For basic-only contracts, generated player actions expose BasicAttack and no fabricated ability.

### A4. Per-encounter qualification

For every generated encounter:

```text
run required routes only
run optional available routes for evidence but do not fail when unavailable/nonrequired
require at least one player route
require opponent AI
require flee and victory
```

Summary booleans follow the truth table.

### A5. History

`CombatEligible()` requires:

```text
PlayerRoutePassed=true
BasicAttackRequired -> BasicAttackPassed
PackageAbilityRequired -> PackageAbilityPassed
OpponentAi/Victory/Flee/Reward/TurnIn/etc. existing truth
```

Old Goal164 v4 both-route rows remain CURRENT.

## B. Recovery checkpoint

Create Application types:

```text
GeneratedCampaignRecoveryCheckpoint
GeneratedCampaignRecoveryState
GeneratedCampaignRecoveryService
```

Checkpoint fields:

```text
ProjectIdentityFingerprint
WorldId
PackageSha256
CompositionPackageSha256
QualifiedAuthoringFingerprint
EncounterId technical
EncounterTitle human
PreEncounterSessionJson
PreEncounterSessionSha256
MapStateSha256
GameplayStateSha256
CreatedFromActionId technical
```

No timestamp is required for truth.

### B1. Capture

Immediately before a successful `StartEncounter` Runtime dispatch:

```text
capture the exact current UnifiedRuntimeSession
serialize/deserialize roundtrip
validate every project-truth field
store in memory
```

Do not capture if StartEncounter is disabled or fails.

Only one current checkpoint is retained.

### B2. Clear

Clear checkpoint after:

```text
victory
flee
manual ClearSession
opening another project
successful world regeneration detected
successful save migration
new game
```

On defeat, retain it for retry.

### B3. Defeat detection

After a Runtime action/AI sequence, defeat is true only when:

```text
encounter inactive
at least one player-team participant existed
no player-team participant remains alive
exact Runtime events/state indicate loss
```

Set campaign status:

```text
DEFEATED
```

Do not show ordinary map/encounter actions.

Add a `Defeat` consequence based on exact state/event truth.

## C. Retry encounter

Add action kind:

```text
RetryEncounter
```

`RetryEncounter()` or `Execute(retryActionId)`:

1. require status DEFEATED;
2. recapture current project truth;
3. require checkpoint truth exact;
4. deserialize checkpoint;
5. validate session/package references against current package;
6. restore the exact pre-encounter session in memory;
7. execute existing `StartEncounter` exactly once through exact package dispatcher;
8. require active encounter with the same technical encounter ID;
9. set status ACTIVE;
10. project one `Retry` consequence.

No Runtime Start call.

### Retry invariants

After retry:

```text
inventory equals pre-encounter
quest state/readiness equals pre-encounter
reputation equals pre-encounter
map/position equals pre-encounter
no reward from lost attempt
no consequence from lost attempt remains except bounded human history entries Defeat/Retry
```

The new encounter Runtime state may differ only by deterministic StartEncounter initialization.

### Stale checkpoint

On truth mismatch:

```text
campaign.recovery_checkpoint_stale
```

Execute zero Runtime commands and set `STALE_PROJECT`.

## D. Other recovery choices

### D1. Continue from save

The existing save picker remains the authority.

On DEFEATED:

```text
CURRENT save -> exact Continue allowed
migration-required save -> explicit migration flow
invalid/legacy -> disabled
```

Successful Continue clears the checkpoint and returns ACTIVE.

No Runtime Start.

### D2. New game

Add action:

```text
StartNewGame
```

This clears checkpoint/session then invokes existing `StartNew()` explicitly.

Runtime Start count increments exactly once.

### D3. No save

When no CURRENT/migratable save exists:

```text
Продолжить с сохранения disabled with human reason
Повторить встречу remains available when checkpoint valid
Начать новую игру remains available
```

## E. Campaign models and UI

Add status:

```text
DEFEATED
```

Add recovery projection:

```text
GeneratedCampaignRecoveryProjection
  Available
  EncounterTitle
  RetryEnabled
  ContinueEnabled
  NewGameEnabled
  DisabledReason
```

`GeneratedCampaignSnapshot` adds `Recovery`.

### UI

In the central context area show:

```text
Поражение
<encounter title>
Повторить встречу
Продолжить с сохранения
Начать новую игру
```

The normal dynamic action list is hidden/disabled while defeated.

Keyboard movement/Interact shortcuts execute nothing in DEFEATED state.

Primary UI contains no IDs/hashes/paths/diagnostic codes.

## F. Consequences

Add kinds:

```text
Defeat
Retry
RecoveryLoad
NewGame
```

Projection rules:

```text
Defeat:
  exact EncounterLost event or exact dead-player state
  resource before/after rows

Retry:
  exact restored checkpoint hash and new StartEncounter success

RecoveryLoad:
  typed exact save load or migration result

NewGame:
  explicit Runtime Start result
```

Do not show a positive consequence for a failed retry/load/new game.


## G. Real automated matrix

### G1. Both-route regression

Use the real Goal164 all-selectable and core-only packages.

Require:

```text
route mode BOTH
Goal164 generated victory/turn-in/travel/save truth unchanged
old Goal164 v4 history remains CURRENT
```

### G2. BasicAttack-only profile

Create a package/profile fixture by removing participant-owned player abilities while preserving an
actual executable BasicAttack and opponent AI contract.

This is a test fixture, not production content.

Require:

```text
resolver selects BASIC_ATTACK_ONLY
build CAMPAIGN_CURRENT
all generated encounters playable
BasicAttackPassed=true
PackageAbilityRequired=false
PackageAbilityPassed=true
no UseAbility action exposed
victory/manual turn-in/save/continue
```

### G3. Ability-only profile

Create a fixture where:

```text
BasicAttack cannot damage the opposing health contract
participant-owned exact attack ability can
opponent AI can execute
```

Require:

```text
resolver selects PACKAGE_ABILITY_ONLY
build CAMPAIGN_CURRENT
BasicAttackRequired=false
BasicAttackPassed=true
PackageAbilityPassed=true
no BasicAttack action exposed
ability victory/manual turn-in
```

Do not modify Runtime to manufacture this.

### G4. Neither route

Require causal build failure:

```text
generated_combat.player_route_missing
```

No synthetic fallback.

### G5. Defeat and retry

Use a real generated encounter with data-derived player/opponent actions.

Drive a fresh session to genuine player defeat through existing Runtime actions.

Require:

```text
status DEFEATED
Defeat consequence
checkpoint retained
map actions disabled
reward/inventory/quest/reputation unchanged from pre-encounter except exact combat resource loss
```

Click Retry:

```text
Runtime Start count unchanged
StartEncounter count +1
status ACTIVE
same encounter title
pre-encounter map/inventory/quest/reputation restored
no lost-attempt reward
Retry consequence
```

Then win and manually turn in to prove normal route still works.

### G6. Save recovery

Create a CURRENT save before the encounter, lose, then Continue.

Require:

```text
Runtime Start count 0 for Continue
checkpoint cleared
exact saved map/gameplay hashes restored
status ACTIVE
RecoveryLoad consequence
```

### G7. New game recovery

Lose, then explicit New Game:

```text
checkpoint cleared
Runtime Start count +1
generated start map
status ACTIVE
NewGame consequence
```

### G8. World change

Lose, then regenerate or switch current world truth.

Retry:

```text
zero Runtime commands
STALE_PROJECT
checkpoint stale diagnostic
```

A migration flow remains explicit.

## H. Required tests

Create at least 44 Goal165 tests; at least 38 behavioral.

### Route neutrality

1. both-route mode inferred;
2. basic-only mode inferred;
3. ability-only mode inferred;
4. neither rejected;
5. basic-only contract ID deterministic;
6. ability-only contract ID deterministic;
7. basic-only overlay has no fabricated ability;
8. ability-only overlay exposes no invalid BasicAttack;
9. basic-only per-encounter qualification GREEN;
10. ability-only per-encounter qualification GREEN;
11. optional unavailable route is vacuous passed;
12. required failed route blocks;
13. old Goal164 v4 both row CURRENT;
14. new basic-only v4 CURRENT;
15. new ability-only v4 CURRENT;
16. regeneration seal covers route fields;
17. tampered route mode/required booleans rejected.

### Recovery checkpoint

18. checkpoint captured before StartEncounter;
19. failed/disabled start captures none;
20. checkpoint exact serialization roundtrip;
21. victory clears checkpoint;
22. flee clears checkpoint;
23. defeat retains checkpoint;
24. defeat detection requires no living player;
25. nondefeat encounter end does not enter DEFEATED;
26. project switch clears/invalidate checkpoint;
27. world/package/authoring drift makes checkpoint stale.

### Retry

28. retry executes zero Runtime Start;
29. retry dispatches StartEncounter once;
30. retry restores map/position;
31. retry restores inventory;
32. retry restores quest/readiness;
33. retry restores reputation;
34. lost reward does not survive;
35. retry active same encounter;
36. second defeat can retry again;
37. victory after retry clears checkpoint;
38. stale retry dispatches zero Runtime commands.

### Save/new game/UI

39. defeated CURRENT save continues exactly;
40. migration-required save remains explicit;
41. successful Continue clears checkpoint;
42. New Game invokes Start once;
43. no-save Continue disabled;
44. movement/interact disabled when defeated;
45. defeat/retry/load/new-game consequences truthful;
46. primary recovery UI no raw IDs/hashes/paths;
47. 1100x720 recovery controls unclipped.

### Regressions/immutability

48. Goal164 61/61 regressions GREEN;
49. Goal163/162 campaign regressions GREEN;
50. Goal161 save/migration regressions GREEN;
51. regeneration/rollback regressions GREEN;
52. Runtime Simulator unchanged;
53. RC/standalone/source/sidecars byte-identical;
54. Player/Unity/standalone counts zero;
55. physical core-only portable copy has no operational pointer and no false RC readiness.

## I. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal165"
# require >=44 total / >=38 behavioral

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

## J. Evidence

Create exactly 12 files in each mirrored root:

```text
goal165-dashboard.json
architecture-review.json
goal164-independent-audit-finding.json
combat-route-neutrality-proof.json
basic-only-ability-only-proof.json
defeat-checkpoint-proof.json
retry-recovery-proof.json
save-new-game-recovery-proof.json
campaign-recovery-ui-proof.json
regression-immutability-proof.json
artifact-scope-proof.json
goal165-report.md
```

Roots:

```text
.llmgc/procedural/goal-165-combat-route-profile-neutrality-and-campaign-defeat-recovery/
.llmgc/exports/goal-165-combat-route-profile-neutrality-and-campaign-defeat-recovery/
```

Twins byte-identical.

### Dashboard fields

```text
status
candidateStatus
goal165TestsDiscovered
goal165BehavioralTestsPassed

goal164AuditBlockerRecorded
goal164AuditBlockerClosed

bothRoutePassed
basicAttackOnlyRoutePassed
abilityOnlyRoutePassed
neitherRouteRejected
oldV4CompatibilityPassed
basicOnlyCampaignCurrent
abilityOnlyCampaignCurrent
routeSealTamperRejected

defeatReached
defeatConsequencePassed
checkpointCaptured
checkpointRetainedOnDefeat
checkpointClearedOnFlee
checkpointClearedOnVictory

retryPassed
retryRuntimeStartInvocationCount
retryStartEncounterCommandCount
retryMapStateExact
retryInventoryExact
retryQuestExact
retryReputationExact
lostRewardRemoved
victoryAfterRetryPassed

saveRecoveryPassed
saveRecoveryRuntimeStartInvocationCount
newGameRecoveryPassed
newGameRuntimeStartDelta
staleCheckpointRejected
staleRetryRuntimeCommandCount
recoveryPrimaryUiNoRawIds

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
portableCoreOnlyPhysicalCopyPassed
portableCoreOnlyOperationalPointerAbsent
playerProcessStartCount
unityEditorProcessStartCount
standaloneBuildInvocationCount
artifactScopeViolationCount

goal165Accepted=false
goal165ManualReviewRequired=false
goal165IndependentAuditRequired=true
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
docs/manual-acceptance/goal164-generated-encounter-combat-contract.md
```

Create:

```text
docs/manual-acceptance/goal165-combat-profile-neutrality-defeat-recovery.md
```

No human gate.

Required GREEN state:

```text
goal164IndependentAuditResult=BLOCKED_AT_15A8F2AB
goal164IndependentAuditBlocker=combat_contract_resolver_accepts_single_player_route_but_qualification_and_history_unconditionally_require_basic_attack_and_package_ability
goal164AuditBlocker=closed_by_goal165

goal164ImplementationStatus=GREEN
goal164CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal164Accepted=false
goal164IndependentAuditRequired=false

goal165ImplementationStatus=GREEN
goal165CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal165Accepted=false
goal165AcceptedByHuman=false
goal165AcceptedByCodex=false
goal165ManualReviewRequired=false
goal165ManualGateReady=false
goal165IndependentAuditRequired=true

goal165BothRoutePassed=true
goal165BasicAttackOnlyPassed=true
goal165AbilityOnlyPassed=true
goal165DefeatRecoveryPassed=true
goal165SaveRecoveryPassed=true
goal165NewGameRecoveryPassed=true
goal165PlayerProcessStartCount=0
goal165UnityEditorProcessStartCount=0
goal165StandaloneBuildInvocationCount=0
goal165ArtifactScopeViolationCount=0

nextAction=independent_goal165_audit_and_plan_campaign_choice_branching
```

Release risk statement:

```text
Generated combat contracts now distinguish BasicAttack-only, package-ability-only and both-route
profiles without treating an unavailable optional action as failure. The campaign player captures an
exact pre-encounter checkpoint and offers truthful retry/save/new-game recovery after defeat. Recovery
checkpoints are intentionally in-memory and are not persisted across application restarts.
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
.devflow/scripts/run-goal165-combat-recovery.ps1
.devflow/scripts/run-goal165-combat-recovery.cmd

src/LLMGameCreator.Application/Generation/Procedural/GeneratedEncounterCombatContractModels.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedEncounterCombatContractService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedEncounterCombatQualificationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCandidateSealService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCommitValidator.cs

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildHistoryReader.cs

src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionModels.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignRecoveryService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignActionPlanner.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignProjectionService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignConsequenceProjector.cs

src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignPageControl.cs
src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/Goal165/Goal165CombatRouteNeutralityTests.cs
tests/LLMGameCreator.Tests/Application/Goal165/Goal165DefeatCheckpointTests.cs
tests/LLMGameCreator.Tests/Application/Goal165/Goal165RetryRecoveryTests.cs
tests/LLMGameCreator.Tests/Application/Goal165/Goal165SaveNewGameRecoveryTests.cs
tests/LLMGameCreator.Tests/Application/Goal165/Goal165CampaignRecoveryUiTests.cs
tests/LLMGameCreator.Tests/Application/Goal165/Goal165RegressionImmutabilityTests.cs

tests/LLMGameCreator.Tests/Application/Goal164/Goal164CombatContractResolutionTests.cs
tests/LLMGameCreator.Tests/Application/Goal164/Goal164GeneratedCampaignRouteTests.cs
tests/LLMGameCreator.Tests/Application/Goal164/Goal164BuildHistoryCampaignCurrentTests.cs
tests/LLMGameCreator.Tests/Application/Goal163/Goal163PackageTruthCombatTests.cs
tests/LLMGameCreator.Tests/Application/Goal162/Goal162CampaignEncounterQuestTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/manual-acceptance/goal164-generated-encounter-combat-contract.md
docs/manual-acceptance/goal165-combat-profile-neutrality-defeat-recovery.md

docs/agent-tasks/goal-165-combat-route-profile-neutrality-and-campaign-defeat-recovery/
.llmgc/procedural/goal-165-combat-route-profile-neutrality-and-campaign-defeat-recovery/
.llmgc/exports/goal-165-combat-route-profile-neutrality-and-campaign-defeat-recovery/
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
route-mode truth and history: 18 minutes
recovery checkpoint/service: 20 minutes
campaign/UI integration: 18 minutes
behavioral tests: 26 minutes
real matrix: 14 minutes
regressions/evidence/docs/scope: 18 minutes
target wall clock: 105 minutes
maximum two concurrent testhost processes
Player/Unity/standalone counts: 0
```

Rules:

```text
write complete test inventory before production edits
write evidence script before real matrix
no unchanged command retries
no timeout escalation
after failure isolate exact class/test
P0/P1 fixed inside Goal165
P2/P3 debt only
do not stop at compile success
do not defer evidence/docs/scope
```

## O. Publication

Create exactly one final commit:

```text
GREEN Goal 165 combat route profile neutrality and campaign defeat recovery
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
Goal164/165 accepted=false
no human gate
```

## P. GREEN criteria

```text
Goal164 audit blocker recorded and closed
Goal165 >=44 discovered / >=38 behavioral / all pass
both/basic-only/ability-only route modes truthful
neither route rejected
old Goal164 v4 history remains current
new single-route v4 histories current
seal tamper rejection
real generated defeat
exact pre-encounter checkpoint
retry with zero Runtime Start
lost reward/quest/reputation removed by restore
save recovery exact
new-game recovery explicit
stale checkpoint zero-dispatch rejection
human recovery UI/consequences
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
- Goal164 audit intake;
- route-mode truth table;
- both/basic-only/ability-only/neither results;
- history/seal compatibility;
- defeat route and checkpoint;
- retry exact restoration and command counts;
- save recovery;
- new-game recovery;
- stale checkpoint rejection;
- recovery UI/consequences;
- process counts and immutability;
- tests/regressions;
- evidence/text/artifact scope;
- state/no-human-gate;
- SHA/push/HEAD/worktree;
- confirmation Codex committed and pushed on any final status.
