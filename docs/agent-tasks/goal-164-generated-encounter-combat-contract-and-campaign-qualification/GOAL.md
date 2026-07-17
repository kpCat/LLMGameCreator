# Goal 164 — Generated Encounter Combat Contract & Campaign Qualification

## Identity

- Task ID: `goal-164-generated-encounter-combat-contract-and-campaign-qualification`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `d5d614a8a0d401113715c9a4b7152c2bc1095a0b`
- Required base message: Goal163 BLOCKED exact generated encounter package contract

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Sol
Reasoning effort: Extra High
```

Reason: this is a major generator/build/campaign vertical slice. It must repair the final generated
package contract without changing Runtime or injecting definitions at play time, preserve strict
source provenance, upgrade build/history/regeneration truth, prove real generated victory/manual
turn-in/save/migration, and requalify the cached standalone and RC.

## Pre-approval and publication

- The owner approved execution by launching this task.
- Produce a concise internal plan; do not ask for confirmation.
- Do not request manual testing.
- Own all P0/P1 defects reproduced by the mandatory Goal164 matrix.
- Record P2/P3 debt without creating Goal164A.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- On BLOCKED/FAILED, commit and push the honest state; never leave publication to the user.
- No intermediate commits.

## Initial worktree

After unpacking, only these untracked files are allowed:

```text
docs/agent-tasks/goal-164-generated-encounter-combat-contract-and-campaign-qualification/GOAL.md
docs/agent-tasks/goal-164-generated-encounter-combat-contract-and-campaign-qualification/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-164-generated-encounter-combat-contract-and-campaign-qualification/README.md
```

Require:

```text
HEAD == origin/main == d5d614a8a0d401113715c9a4b7152c2bc1095a0b
branch=main
no other tracked/staged/untracked changes
```

Never reset, revert, stash, merge or rebase.

## Unity and standalone budget

```text
Unity Editor invocation budget: 0
Unity host build budget: 0
hidden standalone smoke budget: exactly 1
corrective smoke retry budget: 0
visible automatic launch budget: 0
manual user test budget: 0
```

The host cache must be reused. No Unity source change is allowed.

## Goal163 independent-audit intake

Record:

```text
goal163IndependentAuditResult=BLOCKED_AT_D5D614A8
goal163IndependentAuditBlocker=generated_encounters_reference_namespaced_health_and_effectless_generated_action_without_executable_runtime_combat_contract
goal163AuditBlocker=closed_by_goal164 only on GREEN
```

Goal163 remains `accepted=false`; no human gate.

### Goal163 truths to preserve

```text
campaign Runtime dispatch uses exact captured package reference
package SHA and definition inventories stay unchanged during actions
BasicAttack remains BasicAttack
UseAbility accepts only an exact participant-owned package ability
no package clone
no campaign/session-compatible-attack
no fixed campaign attack power
generated quest readiness is read-only
controlled manual turn-in invokes CompleteQuest exactly once
AdvanceQuestObjective remains unused
flee/victory consequences are distinct
human Последствия projection
actual FinalStateHash is separate from selected history SHA
54/54 Goal163 tests
RC/standalone/source immutability
Player/Unity/standalone counts zero
```

### Proven final-package blocker

The real qualified package contains:

```text
ordinary qualified combat definitions and encounter/goblin_duel that execute exact-package combat
generated encounters whose participants reference generated/resource/health
generated participants assigned generated/ability/action_resolve_encounter
that generated ability has Kind=generated_action and no Runtime combat effects
Runtime BasicAttack fallback requires the ordinary health contract
```

Goal163 correctly disables those encounters:

```text
campaign.encounter_no_executable_player_action
```

Controlled fixtures prove the turn-in/consequence contract, but the real generated all-selectable and
core-only routes cannot reach victory.

## Architectural decision

Do not change the persisted generated source chain or Runtime.

Add a deterministic build-time overlay:

```text
strict source v2 and generated base remain byte-identical
Lane A qualified package supplies an existing executable combat contract
Lane B generated encounter participant combat fields are rebound to that exact existing contract
no definitions are added, removed or modified
campaign Runtime receives the exact resulting final package
```

This matches the existing architecture:

```text
generated start activation is build-time
generated travel gates are build-time
generated encounter combat compatibility is build-time
```

Existing generated projects become combat-capable on ordinary rebuild; world regeneration is not
required.

## Product outcome

```text
generated project build
→ all generated encounters receive executable exact-package combat roles
→ build/history reports combat current
→ Играть page exposes generated encounter
→ exact BasicAttack and package ability
→ bounded enemy AI
→ real generated victory
→ generated reward
→ generated quest ready but active
→ player clicks Завершить задание
→ CompleteQuest exactly once
→ reputation consequence
→ travel/save/exact continue
→ regeneration and explicit migration
→ post-migration generated combat/turn-in/travel
→ cached standalone
→ RC CURRENT
→ portable all-selectable and core-only
```

No raw IDs are needed in primary UI.

## Non-goals

Do not change:

```text
Runtime or Runtime.Abstractions
GamePackage/domain schema
FeatureModule definitions or catalog
ProceduralGamePlan schema
generated source v2 schema
GeneratedPackageMvp sidecars
Formula/effect/action rule-pack sidecars
Unity source/host
save migration policy
world generation algorithms
```

Do not delete the data-only generated action/resource definitions. They remain provenance records but
are no longer assigned as the combat role contract.


## Mandatory architecture review

Read at most 18 primary files:

```text
GameProjectBuildAndQualificationService.cs
GameProjectBuildHistoryReader.cs
GameProjectWorkspaceModels.cs
GameProjectGeneratedWorldSummaryService.cs
UnifiedGameProjectWorkspaceController.cs
GameProjectSeedRegenerationCandidateSealService.cs
GameProjectSeedRegenerationCommitValidator.cs
GeneratedProjectOverlayService.cs
GeneratedWorldTravelOverlayService.cs
GeneratedCampaignCombatReadinessService.cs
GeneratedCampaignSessionTruthService.cs
GeneratedCampaignSessionService.cs
GeneratedCampaignRuntimeDispatchService.cs
GeneratedCampaignQuestReadinessService.cs
GeneratedCampaignConsequenceProjector.cs
Goal163PackageTruthCombatTests.cs
Goal163QuestTurnInTests.cs
GeneratedPackageMvpService.cs
```

Before production edits write:

```text
.llmgc/procedural/goal-164-generated-encounter-combat-contract-and-campaign-qualification/architecture-review.json
```

Required resolved sections:

```text
goal163IndependentAudit
laneACombatContractAuthority
contractCandidateSelection
runtimeQualificationOfContract
generatedEncounterBinding
combatOverlayControlledDelta
buildPipelineOrder
generatedCombatQualification
buildHistoryVersioning
campaignReadiness
regenerationRollbackSeal
standaloneFacts
portableTruth
sourceSidecarImmutability
allSelectableAndCoreOnly
failureMatrix
nonGoals
```

Each section names exact types, inputs, outputs, hashes and behavioral tests.

## A. Canonical existing combat contract

Create:

```text
GeneratedEncounterCombatContractService
GeneratedEncounterCombatContract
GeneratedEncounterCombatRoleContract
GeneratedEncounterCombatContractResult
```

Input:

```text
exact Lane A qualified/composed GamePackageDefinition
GeneratedProjectOverlayDocument
IUnifiedGameRuntimeService
```

The overlay document distinguishes generated records from baseline/current-composition records.

### A1. Candidate source

Candidates are encounter definitions that:

```text
are not mapped by GeneratedContent.Encounters
exist exactly once
have at least one player-team participant
have at least one opposing participant
reference only definitions present in the exact Lane A package
```

Do not hardcode:

```text
encounter IDs
resource IDs
ability IDs
participant IDs
participant counts
health values
damage values
```

### A2. Health semantics

A role resource qualifies as a Runtime health resource only when its package definition matches the
existing Runtime contract:

```text
ID equals resource/health
or Kind equals health
or Tags contain health
```

Use the exact existing definition ID.

### A3. Player route

A candidate player role must have at least one executable route:

```text
BasicAttack as resolved by existing Runtime
or participant-owned exact package attack ability
```

Use actual Runtime on a cloned session—not package mutation—to prove:

```text
StartEncounter succeeds
one player action succeeds
an opposing health resource changes
package reference and SHA remain unchanged
```

### A4. Opponent route

A candidate opponent role must be usable by existing encounter AI.

Prove with actual Runtime:

```text
after a player turn/end-turn, bounded RunCurrentTurnAi succeeds
player health/resource state changes or a supported status/stat effect is applied
control can return to the player or the encounter terminates
```

No fake AI or direct state mutation.

### A5. Deterministic selection

Sort qualified role candidates by canonical source encounter/participant IDs and fingerprints.

Select exactly one canonical contract deterministically.

Contract fields:

```text
ContractId = SHA-256 of canonical contract document
SourcePackageSha256
SourceEncounterId technical
PlayerRoleFingerprint
OpponentRoleFingerprint
PlayerResources[]
PlayerStats[]
PlayerAbilities[]
PlayerInventoryId optional
PlayerCombatMetadata filtered
OpponentResources[]
OpponentStats[]
OpponentAbilities[]
OpponentInventoryId optional
OpponentCombatMetadata filtered
ExactDefinitionFingerprints[]
QualificationSummary
```

No absolute paths/timestamps.

### A6. Metadata filtering

Copy only metadata entries that:

```text
do not reference source encounter/participant IDs
reference no missing package definition
are actually consumed by existing encounter Runtime/AI
```

If none are required, copy no metadata.

Do not copy arbitrary baseline story/content metadata.

### A7. No contract

Fail build causally:

```text
generated_combat.contract_missing
generated_combat.player_route_missing
generated_combat.opponent_route_missing
generated_combat.definition_reference_invalid
```

Do not fall back to synthetic definitions.

## B. Generated encounter binding

Create:

```text
GeneratedEncounterCombatBindingService
GeneratedEncounterCombatBinding
```

Input:

```text
strict generated source validation
exact final pre-combat Lane B package
GeneratedEncounterCombatContract
```

Bind generated encounters only through exact provenance:

```text
GeneratedContent.Encounters.SourceId
GeneratedContent.Encounters.PackageEncounterId
strict regenerated plan EncounterSeedId
```

Do not infer by string prefix or title.

Require:

```text
every plan encounter maps to exactly one generated-content row
every row maps to exactly one package encounter
no baseline encounter is treated as generated
```

## C. Combat overlay

Create:

```text
GeneratedWorldEncounterCombatOverlayService
GeneratedWorldEncounterCombatOverlayDocument
GeneratedWorldEncounterCombatOverlayResult
```

### C1. Allowed participant changes

For every generated encounter:

```text
preserve encounter ID/name/kind/rewards/seed/tags/source metadata
preserve participant ID/name/kind/team/faction/entity prototype
replace participant combat fields by role:
  Abilities
  Resources
  Stats
  InventoryId
  filtered combat metadata only when required
```

Role assignment:

```text
Team=player -> canonical player role
every non-player team -> canonical opponent role
```

Process every participant; no fixed maximum.

### C2. Data-derived values

Resource amounts, capacities, stats and ability IDs are copied from the exact canonical role contract.

No numeric content constant may be introduced.

### C3. Existing definitions only

After overlay:

```text
no ability/resource/stat/status/item definition is added
no package collection count changes
every participant reference resolves exactly once
data-only generated ability/resource definitions remain present but unassigned when not executable
```

### C4. Controlled delta

Compare canonical JSON of the pre/post package.

Allowed differences:

```text
only game.encounters records whose IDs are exact generated encounter bindings
inside those records only participant combat fields listed in C1
```

Everything else must be canonical-equal:

```text
manifest/start
all baseline definitions
all non-encounter generated records
generated encounter rewards/identity/provenance
travel gates/maps/entities
GeneratedContent
```

Diagnostics:

```text
generated_combat.delta_baseline_changed
generated_combat.delta_unexpected_collection_change
generated_combat.delta_non_generated_encounter_changed
generated_combat.delta_forbidden_encounter_field
generated_combat.reference_invalid
```

Document fields:

```text
SchemaVersion=generated_encounter_combat_overlay_v1
SourcePackageSha256
OutputPackageSha256
ContractId
GeneratedEncounterCount
BoundEncounterCount
Before/after encounter fingerprints
AllowedFieldPaths
DefinitionCollectionCountsBefore/After
Diagnostics
Passed
```

No timestamps/paths.

### C5. Determinism

Same Lane A + same pre-combat package:

```text
same contract
same overlay document
same output package SHA
```

Reordering generated encounter arrays or participants before canonical sorting must not change output.


## D. Build pipeline integration

Add the overlay to `GameProjectBuildAndQualificationService`.

Required order:

```text
strict source validation
→ Lane A qualification/composition
→ generated start/travel package construction
→ resolve combat contract from exact Lane A
→ bind and apply generated combat overlay to Lane B
→ final package validation
→ generated start/travel activation
→ generated combat qualification
→ history transaction
```

The contract source is Lane A. The modified package is Lane B only.

### D1. Lane A invariants

Require byte/hash equality with the Goal163 Lane A result:

```text
AcceptedMechanics
Social
compatibility package/composition/final hashes
checkpoint/replay
selected modules/parameters
```

No Lane A encounter/definition mutation.

### D2. Lane B primary authority

Primary:

```text
PackageSha256
CompositionPackageSha256
FinalStateHash
RuntimeFrames
standalone payload
RC record
```

must belong to the combat-overlay package and qualification.

### D3. Build result

Add typed:

```text
GameProjectGeneratedEncounterCombatSummary
```

Fields:

```text
Present
Passed
Status
ContractId
ContractSourcePackageSha256
GeneratedEncounterCount
QualifiedEncounterCount
ExactPackageSha256
ExactPackageReferencePassed
PackageShaUnchangedDuringRuntime
BasicAttackPassed
PackageAbilityPassed
OpponentAiPassed
VictoryPassed
FleePassed
RewardPassed
GeneratedQuestReadyPassed
ManualTurnInPassed
CompleteQuestCommandCount
AdvanceObjectiveCommandCount
ConsequencePassed
ReplayPassed
FinalStateHash
RuntimeFrames
HumanReviewFacts
TechnicalDetails
Diagnostics
```

Human facts contain no IDs/hashes.

Status values:

```text
CAMPAIGN_CURRENT
COMBAT_PENDING
LAST_SUCCESS
INVALID
ABSENT
```

## E. Real generated combat qualification

Create:

```text
GameProjectGeneratedEncounterCombatQualificationService
```

Input:

```text
combat-overlay package
strict source/plan
generated encounter bindings
IUnifiedGameRuntimeService
GeneratedCampaignQuestReadinessService
GeneratedCampaignConsequenceProjector
```

### E1. Per-encounter contract

For every generated encounter:

```text
StartEncounter exact
at least one exact-package player BasicAttack
at least one exact participant-owned package ability when role has abilities
bounded opponent AI
flee path no rewards
fresh victory path
package SHA/reference unchanged
```

Do not require a fixed command count. Derive route from current health/resources/abilities/turn state.

### E2. Deterministic representative campaign

Select a generated quest/encounter deterministically:

```text
shortest reachable region route from start
then quest/encounter source ID ordinal
```

Execute through the actual `GeneratedCampaignSessionService` on a disposable current project copy:

```text
start
travel if needed
start generated encounter by human action
victory using exact package
reward
generated quest ready but active
manual turn-in
CompleteQuest exactly once
reputation consequence
destination interaction
save
exact continue
```

No direct state mutation.

### E3. Flee branch

A fresh session:

```text
start same generated encounter
flee
no reward
quest not ready
reputation unchanged
```

### E4. Replay

Fresh project/session repeat of the representative route must match:

```text
ordered action kinds
map transitions
encounter outcome
reward item/title/count
quest state/readiness
reputation delta
final session hash
consequence kinds
```

## F. History and reopen truth

Add build-history schema:

```text
unified_game_project_build_history_v4
```

Persist:

```text
GeneratedWorld
GeneratedWorldActivation
GeneratedRegionTravel
GeneratedEncounterCombat
AcceptedMechanics
AcceptedMechanicsCompatibility
```

### F1. Reader compatibility

```text
v4 current exact package/build/authoring + combat summary -> CAMPAIGN_CURRENT
v3 genuine Goal158–163 history -> GeneratedWorld TRAVEL_CURRENT and combat COMBAT_PENDING
v2 -> START_CURRENT and combat ABSENT
legacy -> existing behavior
```

A v3 row must never become CAMPAIGN_CURRENT merely because package hashes match.

Historical files are not rewritten.

### F2. Workspace snapshot

Expose:

```text
GeneratedEncounterCombat
GeneratedCampaignStatus
```

Projects card rows:

```text
Столкновения       готовы / требуется сборка / ошибка
Игровая кампания   готова / требуется сборка
```

No IDs/hashes in primary UI.

### F3. Campaign readiness

`GeneratedCampaignSessionTruthService` requires for projects that contain generated encounters:

```text
selected current v4 history
GeneratedEncounterCombat.Passed=true
Status=CAMPAIGN_CURRENT
summary exact package/final hashes match current truth
```

Otherwise:

```text
PROJECT_NOT_READY
campaign.generated_combat_not_current
```

Projects button shows:

```text
Собрать и играть
```

for old v3/COMBAT_PENDING history.

Generated projects with zero generated encounters may remain campaign-ready without a combat summary,
but this must be data-derived.

## G. Regeneration and history rollback

Generated regeneration/rollback candidates use the ordinary build pipeline and therefore receive the
combat overlay.

### G1. Candidate seal

Add canonical hashes:

```text
GeneratedEncounterCombatSummarySha256
GeneratedEncounterCombatOverlaySha256
GeneratedEncounterCombatContractId
```

Candidate tamper is rejected.

### G2. Semantic commit validation

For generated source with encounters require:

```text
v4 current history
GeneratedEncounterCombat CAMPAIGN_CURRENT
exact current package/final hashes
all generated encounter bindings qualified
```

For source with zero encounters:

```text
combat ABSENT is valid
```

### G3. History rollback

Historical generation artifacts do not store combat overlays.

After rollback build:

```text
current mechanics/Lane A resolve the current combat contract
target generated world receives a new deterministic combat overlay
candidate reopens CAMPAIGN_CURRENT
```

No stale historical package/ability contract is restored.

## H. Save migration

A new combat-overlay package changes package/definition fingerprints.

Require:

```text
existing Goal161/162 save becomes PACKAGE_REBASE_REQUIRED or WORLD_MIGRATION_REQUIRED
direct load rejected
preview/apply explicit
portable state preserved by existing policy
combat/encounter transient state reset
migrated revision CURRENT
post-migration generated encounter victory/manual turn-in/travel succeed
```

Do not modify `GeneratedGameplaySave*` implementation.

## I. Standalone and RC

After combat-current build run exactly one normal cached standalone build.

Require:

```text
HostReused=true
HostRebuilt=false
Unity Editor starts=0
hidden smoke exactly 1
payload preflight GREEN
actual payload package/composition/final hashes match combat-current build
human facts include generated combat/victory/manual turn-in/consequences
all five smoke markers
RC CURRENT
```

No standalone host/application implementation change is expected.

### I1. Portable all-selectable

Without operational output/Player/Runtime:

```text
v4 CAMPAIGN_CURRENT
generated combat summary passed
save CURRENT
RC CURRENT
```

### I2. Portable core-only

Require:

```text
v4 CAMPAIGN_CURRENT
generated combat/play/save CURRENT
AcceptedMechanics remains incomplete
no false RC READY/CURRENT when profile is not RC-complete
```

The standalone qualification can use all-selectable only; core-only portable truth must not invent RC.

## J. Source immutability

Before and after compare exact bytes for:

```text
Goal142 baseline source
goal148-manual source
Goal156/157 generated source and every persisted generation sidecar:
  request/source
  plan JSON/Markdown
  rule pack/validation
  tiny loop
  MVP package/reports
  overlay
  generated base
```

All byte-identical.

Goal164 modifies only final build products/history and project-local save/RC artifacts in disposable
fixtures.


## K. Required behavioral tests

Create at least 52 Goal164 tests; at least 46 behavioral.

### Contract resolution

1. exact Lane A package resolves one deterministic contract;
2. generated encounters excluded from template candidates;
3. candidate requires player and opponent roles;
4. health semantics accept exact Runtime-recognized resource definitions;
5. missing health definition rejected;
6. actual Runtime player action qualifies contract;
7. actual Runtime opponent AI qualifies contract;
8. package SHA/reference unchanged during qualification;
9. same input produces same ContractId;
10. no fixed encounter/resource/ability IDs in production selection;
11. no fixed health/damage numbers in overlay;
12. missing contract blocks build causally.

### Binding and controlled delta

13. every plan encounter binds through exact provenance;
14. missing/duplicate mapping rejected;
15. player participants receive player role combat fields;
16. every non-player participant receives opponent role fields;
17. participant identity/story/faction fields preserved;
18. rewards/seed/tags/encounter metadata preserved;
19. no package definition added/removed;
20. data-only generated action/resource remain but are unassigned;
21. all participant references resolve exactly once;
22. baseline records canonical-equal;
23. non-encounter generated records canonical-equal;
24. travel maps/gates canonical-equal;
25. forbidden encounter-field mutation rejected;
26. repeat overlay deterministic.

### Build/history/campaign truth

27. Lane A hashes/AcceptedMechanics unchanged;
28. Lane B primary hashes change to combat package;
29. all generated encounters qualified;
30. v4 history persists combat summary;
31. v4 reopen CAMPAIGN_CURRENT;
32. genuine v3 reopen COMBAT_PENDING, never campaign current;
33. v2 remains START_CURRENT/combat absent;
34. old project Projects button says Собрать и играть;
35. rebuild upgrades old project without source rewrite;
36. campaign truth requires exact combat summary;
37. source with zero encounters accepts combat absent.

### Real campaign route

38. generated encounter action enabled by human title;
39. BasicAttack remains BasicAttack on exact final package;
40. exact package ability route succeeds;
41. opponent AI bounded and executable;
42. flee gives no reward/readiness/reputation;
43. fresh victory grants generated reward;
44. generated quest ready but active;
45. manual CompleteQuest exactly once;
46. AdvanceQuestObjective count zero;
47. reputation/consequence visible;
48. generated travel and destination interaction;
49. exact save/continue Runtime Start count zero;
50. representative replay equivalent.

### Regeneration/save/standalone/portable

51. regeneration candidate seal includes combat hashes;
52. tampered combat summary/overlay rejected;
53. history rollback rebuilds current combat contract;
54. old save becomes migration-required;
55. migration apply current;
56. post-migration generated victory/turn-in/travel;
57. exactly one hidden smoke;
58. host reused/not rebuilt and Unity zero;
59. actual payload combat facts and hashes;
60. RC CURRENT all-selectable;
61. portable all-selectable campaign/save/RC current;
62. portable core-only campaign/save current with no false RC readiness.

### Regressions

63. Goal163 exact-package/consequence regressions GREEN;
64. Goal162 campaign regressions GREEN;
65. Goal161T/S/R/Q/161 regressions GREEN;
66. Goal160/159/158/157 regressions GREEN;
67. Runtime Simulator and generated save regressions GREEN;
68. Goal142 and goal148-manual byte-identical;
69. generation sidecars byte-identical.

Do not claim counts unless tests are discovered and executed.

## L. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal164"
# require >=52 total / >=46 behavioral

dotnet test ... --filter "FullyQualifiedName~Goal164"
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

dotnet test ... --filter "FullyQualifiedName~GeneratedCampaign"
dotnet test ... --filter "FullyQualifiedName~GeneratedGameplaySave"
dotnet test ... --filter "FullyQualifiedName~RuntimeSimulator"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectStandaloneBuild"
dotnet test ... --filter "FullyQualifiedName~GameProjectOperationCoordinator"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then run real:

```text
existing v3 generated project rebuild
all-selectable full generated combat campaign
core-only full generated combat campaign
regeneration and history rollback candidate matrix
save migration
one cached standalone smoke
portable all-selectable/core-only
```

Do not run:

```text
full suite
85-case closure
all-ProductSmoke
Unity host build
more than one hidden smoke
corrective smoke retry
visible application/player launch
```

A zero-match filter is failure.

## M. Evidence

Create exactly 14 files in each mirrored root:

```text
goal164-dashboard.json
architecture-review.json
goal163-independent-audit-finding.json
combat-contract-resolution-proof.json
generated-combat-overlay-proof.json
controlled-delta-proof.json
build-history-campaign-current-proof.json
generated-victory-turn-in-proof.json
all-selectable-core-only-proof.json
regeneration-rollback-proof.json
save-migration-proof.json
standalone-portability-proof.json
artifact-scope-proof.json
goal164-report.md
```

Roots:

```text
.llmgc/procedural/goal-164-generated-encounter-combat-contract-and-campaign-qualification/
.llmgc/exports/goal-164-generated-encounter-combat-contract-and-campaign-qualification/
```

Twins byte-identical.

### Dashboard fields

```text
status
candidateStatus
goal164TestsDiscovered
goal164BehavioralTestsPassed

goal163AuditBlockerRecorded
goal163AuditBlockerClosed

combatContractResolved
combatContractId
contractSourcePackageSha256
contractPlayerRoutePassed
contractOpponentAiPassed
contractPackageShaUnchanged

generatedEncounterCount
boundGeneratedEncounterCount
qualifiedGeneratedEncounterCount
generatedParticipantsReboundCount
definitionCollectionCountUnchanged
baselineRecordsPreserved
nonEncounterGeneratedRecordsPreserved
travelOverlayPreserved
combatOverlayDeterministic

laneACompatibilityPassed
laneAHashesUnchanged
laneBCombatPackageSha256
combatSummaryPassed
historySchemaVersion
freshReopenCampaignCurrent
v3ReopenCombatPending
oldProjectRebuildWithoutSourceRewrite

generatedBasicAttackPassed
generatedPackageAbilityPassed
generatedOpponentAiPassed
generatedFleePassed
generatedVictoryPassed
generatedRewardReceived
generatedQuestReadyAndActive
completeQuestCommandCount
advanceObjectiveCommandCount
manualTurnInPassed
reputationConsequencePassed
representativeReplayEquivalent

regenerationCandidateCombatCurrent
historyRollbackCombatCurrent
saveMigrationRequired
saveMigrationApplyPassed
postMigrationGeneratedCombatPassed

hostCacheKey
hostReused
hostRebuilt
unityEditorProcessStartCount
hiddenSmokeInvocationCount
hiddenSmokePassed
actualPayloadCombatFactsPassed
releaseCandidateRecordCurrent
portableAllSelectablePassed
portableCoreOnlyPassed
coreOnlyNoFalseRcReady

goal163RegressionPassed
goal162RegressionPassed
goal161RegressionPassed
goal160RegressionPassed
goal159RegressionPassed
goal158RegressionPassed
goal157RegressionPassed
generatedSaveRegressionPassed
runtimeSimulatorRegressionPassed

goal142SourceByteIdentical
sourceGoal148ByteIdentical
generationSidecarsByteIdentical
artifactScopeViolationCount

goal164Accepted=false
goal164ManualReviewRequired=false
goal164IndependentAuditRequired=true
```

No required GREEN field may be null/PARTIAL/NOT_EXECUTED.


## N. State and docs

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
docs/manual-acceptance/goal163-package-truth-campaign-consequences.md
```

Create:

```text
docs/manual-acceptance/goal164-generated-encounter-combat-contract.md
```

No human gate.

Required GREEN state:

```text
goal163IndependentAuditResult=BLOCKED_AT_D5D614A8
goal163IndependentAuditBlocker=generated_encounters_reference_namespaced_health_and_effectless_generated_action_without_executable_runtime_combat_contract
goal163AuditBlocker=closed_by_goal164

goal163ImplementationStatus=GREEN
goal163CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal163Accepted=false
goal163IndependentAuditRequired=false

goal164ImplementationStatus=GREEN
goal164CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal164Accepted=false
goal164AcceptedByHuman=false
goal164AcceptedByCodex=false
goal164ManualReviewRequired=false
goal164ManualGateReady=false
goal164IndependentAuditRequired=true

goal164CombatContractResolved=true
goal164GeneratedCombatOverlayPassed=true
goal164CampaignCurrentPassed=true
goal164GeneratedVictoryPassed=true
goal164ManualTurnInPassed=true
goal164RegenerationRollbackPassed=true
goal164SaveMigrationPassed=true
goal164HostReused=true
goal164HostRebuilt=false
goal164UnityEditorProcessStartCount=0
goal164HiddenSmokeInvocationCount=1
goal164PortableAllSelectablePassed=true
goal164PortableCoreOnlyPassed=true
goal164ArtifactScopeViolationCount=0

nextAction=independent_goal164_audit_and_plan_campaign_choice_branching_or_failure_recovery
```

Release risk statement:

```text
Generated encounter participants are now bound at build time to an existing exact qualified combat
contract from Lane A. No Runtime or campaign-time definition injection occurs. Every generated
encounter is executable, persisted as CAMPAIGN_CURRENT, and survives regeneration, history rollback,
save migration, standalone and portable recovery. Rich authored combat diversity and branching
consequences remain future work.
```

Record any unused data-only generated action/resource definitions as intentional provenance, not a
blocker.

## O. Text integrity

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

## P. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal164-generated-combat-contract.ps1
.devflow/scripts/run-goal164-generated-combat-contract.cmd

src/LLMGameCreator.Application/Generation/Procedural/GeneratedEncounterCombatContractModels.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedEncounterCombatContractService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedEncounterCombatBindingService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedWorldEncounterCombatOverlayService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedEncounterCombatQualificationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCandidateSealService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCommitValidator.cs

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildHistoryReader.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectGeneratedWorldSummaryService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs

src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionModels.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionTruthService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignActionPlanner.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionService.cs

src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignPageControl.cs

tests/LLMGameCreator.Tests/Application/Goal164/Goal164CombatContractResolutionTests.cs
tests/LLMGameCreator.Tests/Application/Goal164/Goal164GeneratedCombatOverlayTests.cs
tests/LLMGameCreator.Tests/Application/Goal164/Goal164BuildHistoryCampaignCurrentTests.cs
tests/LLMGameCreator.Tests/Application/Goal164/Goal164GeneratedCampaignRouteTests.cs
tests/LLMGameCreator.Tests/Application/Goal164/Goal164RegenerationRollbackTests.cs
tests/LLMGameCreator.Tests/Application/Goal164/Goal164SaveMigrationTests.cs
tests/LLMGameCreator.Tests/Application/Goal164/Goal164StandaloneAndPortabilityTests.cs
tests/LLMGameCreator.Tests/Application/Goal164/Goal164RegressionImmutabilityTests.cs

tests/LLMGameCreator.Tests/Application/Goal163/Goal163PackageTruthCombatTests.cs
tests/LLMGameCreator.Tests/Application/Goal163/Goal163QuestTurnInTests.cs
tests/LLMGameCreator.Tests/Application/Goal162/Goal162CampaignEncounterQuestTests.cs
tests/LLMGameCreator.Tests/WinForms/UnifiedGameProjectWorkspaceTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/manual-acceptance/goal163-package-truth-campaign-consequences.md
docs/manual-acceptance/goal164-generated-encounter-combat-contract.md

docs/agent-tasks/goal-164-generated-encounter-combat-contract-and-campaign-qualification/
.llmgc/procedural/goal-164-generated-encounter-combat-contract-and-campaign-qualification/
.llmgc/exports/goal-164-generated-encounter-combat-contract-and-campaign-qualification/
```

One exact additional existing Application regeneration/history/standalone test/model path may be added
only after a concrete compile/test failure and with a recorded reason.

Forbidden:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
catalogs/feature-modules/**
unity/**
src/LLMGameCreator.Application/Generation/Procedural/ProceduralGameKernel*
src/LLMGameCreator.Application/Generation/Procedural/GeneratedPackageMvp*
src/LLMGameCreator.Application/Generation/Procedural/FormulaEffectAction*
src/LLMGameCreator.Application/Generation/Procedural/SeededGeneratedProjectSourceService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplaySave*
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/**
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectReleaseCandidate*
```

No source-sidecar or Runtime change is expected.

## Q. Command budget

```text
read-first/architecture: 14 minutes
contract resolver/runtime qualification: 22 minutes
binding/controlled overlay: 22 minutes
build/history/regeneration integration: 24 minutes
campaign real route/save migration: 20 minutes
behavioral tests: 32 minutes
one standalone/portable matrix: 14 minutes
regressions/evidence/docs/artifact scope: 20 minutes
target wall clock: 145 minutes
maximum two concurrent testhost processes
Unity starts: 0
```

Rules:

```text
write complete test inventory before production edits
write evidence/publication script before real matrix
no unchanged command retries
no timeout escalation
after failure isolate exact class/test
P0/P1 fixed inside Goal164
P2/P3 debt only
do not stop at compile success
do not defer evidence/docs/scope
```

## R. Publication

Create exactly one final commit:

```text
GREEN Goal 164 generated encounter combat contract and campaign qualification
```

or honest BLOCKED/FAILED.

Codex must push `origin/main`.

Required final:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three task files tracked
host cache reused
Unity starts=0
hidden smoke=1
Goal142/goal148 and generation sidecars unchanged
Goal163/164 accepted=false
no human gate
```

## S. GREEN criteria

```text
Goal163 blocker recorded and closed
Goal164 >=52 discovered / >=46 behavioral / all pass
canonical exact existing combat contract resolved
no fixed IDs/values
all generated encounters rebound
controlled delta only generated participant combat fields
no definitions added/removed
Lane A unchanged
Lane B combat package primary
v4 CAMPAIGN_CURRENT history
old v3 COMBAT_PENDING
real generated victory/reward/manual turn-in/reputation
CompleteQuest exactly once / AdvanceObjective zero
flee branch no rewards/readiness
save exact continue
regeneration and rollback combat current
explicit save migration and post-migration combat
one cached standalone smoke
RC CURRENT
portable all-selectable/core-only
required regressions GREEN
source/sidecars byte-identical
14+14 evidence
text integrity GREEN
artifact scope 0
one final commit pushed
```

## T. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning;
- Goal163 audit intake;
- contract candidate and deterministic selection;
- exact player/opponent role contract without raw IDs in primary report;
- controlled delta and definition-count proof;
- Lane A/Lane B hashes;
- v4/v3/v2 history behavior;
- real generated flee/victory/manual turn-in;
- BasicAttack/ability/AI command proof;
- reward/reputation/consequence;
- save/continue/migration;
- regeneration/rollback;
- host/Unity/smoke/payload/RC;
- portable all-selectable/core-only;
- tests/regressions;
- source/sidecar immutability;
- evidence/text/artifact scope;
- state/no-human-gate;
- SHA/push/HEAD/worktree;
- confirmation Codex committed and pushed on any final status.
