# Goal 168 — Choice-Driven Relationships & Multi-Quest Arcs

## Identity

- Task ID: `goal-168-choice-driven-relationships-and-multi-quest-arcs`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `fd69bfc86f28b1261ec638c3d45d18d16689bf1e`
- Required base message: `GREEN Goal 167 generated campaign choice branching and persistent consequences`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: this is the next major visible campaign vertical slice. Goal167 adds persistent generated
choices, but its Support/Challenge qualification reimplements victory with BasicAttack followed by
the first merely successful ability instead of consuming Goal166's exact qualified-action catalog.
Goal168 closes that P1 and turns one-shot choices into data-derived actor/faction relationships with
sequential quest arcs, persistent UI, save/migration truth and one qualified standalone.

## Pre-approval and publication

- The complete plan is approved by launching this task.
- Do not ask for another confirmation because more than ten files are involved.
- Produce a concise internal plan and proceed.
- Do not request manual testing.
- Own every P0/P1 reproduced by the Goal168 matrix.
- Record P2/P3 debt without creating Goal168A.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- On BLOCKED/FAILED, commit and push the honest state; never leave publication to the user.
- No intermediate commits.

## Initial worktree

After unpacking, only these untracked files are allowed:

```text
docs/agent-tasks/goal-168-choice-driven-relationships-and-multi-quest-arcs/GOAL.md
docs/agent-tasks/goal-168-choice-driven-relationships-and-multi-quest-arcs/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-168-choice-driven-relationships-and-multi-quest-arcs/README.md
```

Require:

```text
HEAD == origin/main == fd69bfc86f28b1261ec638c3d45d18d16689bf1e
branch=main
no other tracked/staged/untracked changes
```

Never reset, revert, stash, merge or rebase.

## Execution budget

```text
Unity Editor invocation budget: 0
Unity host build budget: 0
hidden standalone smoke budget: exactly 1
corrective smoke retry budget: 0
visible automatic launch budget: 0
manual user test budget: 0
```

Use the existing cached host. No Unity source change.

## Goal167 independent-audit result

Record:

```text
goal167IndependentAuditResult=BLOCKED_AT_FD69BFC8
goal167IndependentAuditBlocker=choice_branch_qualification_reimplements_victory_without_goal166_exact_qualified_action_catalog
goal167AuditBlocker=closed_by_goal168 only on GREEN
```

Goal167 implementation remains GREEN and `accepted=false`; no human gate.

### Goal167 truths to preserve

```text
exact actor/dialogue/faction/quest/encounter bindings
deterministic controlled choice overlay
Support/Challenge/Refuse Runtime branches
Support active/completed follow-ups
Challenge flee/victory follow-ups
actual failing-choice rollback
two independent branch replays
state-backed preview/journal/consequences
v5 CHOICE_CURRENT and genuine v4 CHOICES_PENDING
old-project one-build upgrade
choice regeneration/rollback seals
save/continue and migration preserve/drop without ghost decisions
one cached hidden smoke
RC CURRENT
portable all-selectable/core-only
94/94 Goal167 tests
```

### Independent-audit P1

`GameProjectGeneratedCampaignChoiceQualificationService.WinEncounter()` currently:

```text
tries BasicAttack;
accepts it whenever Runtime returns Success, without checking encounter progress;
if BasicAttack fails, iterates raw participant abilities;
accepts the first Runtime Success, without Goal166 descriptor/effect matching.
```

A legal package can be:

```text
PACKAGE_ABILITY_ONLY
ability/a_utility -> Success, no progress
ability/z_damage  -> Success, exact qualified progress
```

Combat summary remains `CAMPAIGN_CURRENT`, but Support/Challenge qualification can stall or report
false truth.

### Required closure

Every choice/relationship combat route consumes only:

```text
GameProjectGeneratedEncounterCombatSummary.QualifiedActions
```

or an exact equivalent contract catalog from the selected current build.

Rules:

```text
no raw participant ability iteration for branch victory
no success-without-progress acceptance
BasicAttack only with exact BASIC_ATTACK descriptor and matching observed effect
UseAbility only with exact qualified descriptor ID/fingerprint/effect
package SHA/reference unchanged
bounded route from actual encounter/participant/resource state
```

Diagnostics:

```text
generated_relationship.qualified_combat_catalog_missing
generated_relationship.qualified_action_definition_changed
generated_relationship.qualified_action_no_progress
generated_relationship.arc_combat_failed
```

## Product problem

Goal167 decisions are persistent but mostly one-shot. A supported actor does not progress through a
sequence of generated quests, there is no relationship status surface, and completing one quest does
not unlock the next data-derived step.

## Product outcome

```text
meet actor
→ choose Support
→ relationship becomes Supported
→ first assigned generated quest starts
→ complete exact generated combat/manual turn-in
→ return to actor
→ next assigned quest becomes available
→ continue through every assigned quest
→ final relationship state Completed
→ save and exact continue
→ relationship and current arc step restore exactly
→ regenerate/migrate
→ compatible decision/reputation preserved
→ arc progress follows explicit typed migration policy
→ no ghost relationship rows
```

Challenge and Refuse remain exclusive:

```text
Challenge:
  exact generated encounter
  defeat/retry or victory
  relationship Challenged/Resolved

Refuse:
  negative reputation
  no quest starts
  relationship Refused
```

No raw IDs/hashes/paths in primary UI.

## Non-goals

Do not change:

```text
Runtime or Runtime.Abstractions
GamePackage/domain public schema
FeatureModule catalog
ProceduralGamePlan schema
GeneratedPackageMvp source/sidecars
world topology
generated combat overlay
Unity source/host
cloud/multiplayer
```

Build-time overlays may modify exact generated dialogue and generated quest fields only within the
controlled delta below.


## Mandatory architecture review

Read at most 20 primary files:

```text
GameProjectGeneratedCampaignChoiceQualificationService.cs
GeneratedCampaignChoiceModels.cs
GeneratedCampaignChoiceBindingService.cs
GeneratedCampaignChoiceOverlayService.cs
GeneratedEncounterCombatContractModels.cs
GeneratedEncounterCombatContractService.cs
GameProjectGeneratedEncounterCombatQualificationService.cs
GameProjectBuildAndQualificationService.cs
GameProjectBuildHistoryReader.cs
GameProjectWorkspaceModels.cs
GameProjectSeedRegenerationCandidateSealService.cs
GameProjectSeedRegenerationCommitValidator.cs
GeneratedCampaignSessionTruthService.cs
GeneratedCampaignSessionService.cs
GeneratedCampaignActionPlanner.cs
GeneratedCampaignDecisionJournalService.cs
GeneratedCampaignDialogueChoicePreviewService.cs
GeneratedGameplaySaveMigrationService.cs
Goal167 branch/runtime/save tests
Goal166 mixed-action/recovery tests
```

Before production edits write:

```text
.llmgc/procedural/goal-168-choice-driven-relationships-and-multi-quest-arcs/architecture-review.json
```

Required sections:

```text
goal167IndependentAudit
exactQualifiedCombatReuse
relationshipIdentity
questAssignment
arcOrdering
relationshipOverlayControlledDelta
supportArcLifecycle
challengeLifecycle
refuseLifecycle
relationshipProjection
historyV6
campaignReadiness
regenerationSeal
exactSaveContinue
migrationCompatibility
standaloneRcPortable
failureMatrix
regressionImmutability
nonGoals
```

Each section names exact types, inputs, outputs, hashes and behavioral tests.

## A. Reusable exact combat route executor

Create:

```text
GeneratedCampaignExactCombatRouteService
GeneratedCampaignExactCombatRouteRequest
GeneratedCampaignExactCombatRouteResult
```

Inputs:

```text
exact final GamePackageDefinition
exact generated encounter ID
GameProjectGeneratedEncounterCombatSummary
IUnifiedGameRuntimeService
initial UnifiedRuntimeSession
route goal: FLEE | VICTORY
```

### A1. Catalog validation

Require:

```text
summary Passed/CAMPAIGN_CURRENT
QualifiedActions nonempty when encounters exist
QualifiedActionCount/hash/count breakdown exact
every ability definition SHA current
descriptor observed effect supported
summary ExactPackageSha256 == actual package SHA
```

### A2. Victory execution

At each player turn:

```text
iterate exact QualifiedActions in canonical order
build BasicAttack or UseAbility only from descriptor
execute against a live opposing target
require Runtime success
require observed effect matches descriptor
require encounter progress
```

A successful no-op/utility command is rejected and another descriptor may be tried.

At opponent turn:

```text
RunCurrentTurnAi
```

Bound route using actual participant/resource/turn state. No fixed combat command count.

Success:

```text
encounter inactive
living player participant
all opponents defeated
no flee marker
package reference/SHA unchanged
```

### A3. Flee

Use exact `FleeEncounter`.

Require:

```text
encounter inactive
no reward
no generated quest readiness
no reputation delta
package unchanged
```

### A4. Reuse

Refactor choice qualification to use this service. Do not keep its old `WinEncounter`, `TryAbilities`
or raw participant-ability loop.

Goal166/164 combat qualification remains authoritative and unchanged unless an exact integration
signature is needed.

## B. Relationship arc models

Create:

```text
GeneratedCampaignRelationshipModels.cs
GeneratedCampaignRelationshipBindingService.cs
GeneratedCampaignRelationshipOverlayService.cs
GameProjectGeneratedCampaignRelationshipQualificationService.cs
```

Enums:

```text
GeneratedCampaignRelationshipBranch
  SUPPORT
  CHALLENGE
  REFUSE

GeneratedCampaignRelationshipStatus
  UNDECIDED
  SUPPORTED
  QUEST_ACTIVE
  QUEST_READY
  COMPLETED
  CHALLENGED
  CHALLENGE_RESOLVED
  REFUSED
```

Types:

```text
GeneratedCampaignRelationshipBinding
GeneratedCampaignQuestArcStep
GeneratedCampaignRelationshipOverlayDocument
GameProjectGeneratedCampaignRelationshipSummary
GeneratedCampaignRelationshipRuntimeFrame
GeneratedCampaignRelationshipHumanFact
```

## C. Exact relationship identity and quest assignment

Relationship identity is the exact generated dialogue ID.

```text
RelationshipId = DialogueDefinition.Id
ActorSeedId exact
ActorEntityId exact
FactionId exact
RegionId exact
DecisionFlagId = DialogueDefinition.Id
```

No new untracked flag namespace.

### C1. Candidate quests

Generated quest candidate must have exact provenance and satisfy:

```text
same source faction as actor
same region, or target encounter reachable from actor region through generated region graph
exact package quest definition
exact generated target encounter/item references
nonzero reputation reward for actor faction
```

### C2. Assign each quest exactly once

A generated quest may belong to at most one relationship.

Deterministic priority:

1. actor appears in target encounter `ActorSeedIds`;
2. actor and quest share faction and region;
3. canonical actor by ActorSeedId among remaining matching actors.

Ambiguity after deterministic rules is an error.

Do not use fixed actor/quest counts.

### C3. Arc ordering

For each relationship:

```text
shortest generated-region distance from actor home to quest region
then target encounter source ID
then quest source ID
```

Arc may contain zero, one or many quests.

Support exists only when arc count > 0.

Challenge/refuse follow Goal167 relationship rules.

## D. Relationship overlay

Input:

```text
exact Goal167 choice-overlay package
strict source
relationship bindings
```

Output modifies only:

```text
exact bound generated dialogues:
  nodes/choices
  generatedRelationship* metadata/tags

exact assigned generated quests:
  AutoStart
  generatedRelationship* metadata
```

### D1. Quest AutoStart

Assigned arc quests become:

```text
AutoStart=false
```

Unassigned generated quests retain existing behavior.

The Support initial choice:

```text
sets SUPPORT decision flag
applies exact positive reputation
StartQuestId = first arc quest
```

After each completed quest, the actor dialogue exposes exactly one human action to start the next
quest through existing `StartQuestId`.

After final completion, only the completed relationship follow-up remains.

### D2. Support follow-ups

```text
SUPPORT + current quest active:
  progress response

SUPPORT + current quest completed + next exists/not started:
  Начать следующее задание

SUPPORT + final quest completed:
  relationship completed response
```

All requirements use existing `flag_equals` and `quest_state`.

### D3. Challenge/refuse

Challenge and Refuse preserve Goal167 effects/exclusivity.

Challenge follow-up distinguishes:

```text
encounter not completed/fled
encounter resolved
```

using exact flag plus Runtime state where available.

### D4. Controlled delta

Require canonical equality for everything except:

```text
bound generated dialogue nodes and generatedRelationship metadata/tag
assigned generated quest AutoStart and generatedRelationship metadata
```

Preserve:

```text
quest ID/title/description/kind/objectives/rewards/stages/tags/source metadata
dialogue identity/source metadata
all nonbound dialogues
all unassigned quests
all non-dialogue/non-quest collections
GeneratedContent and Manifest
```

Definition collection counts unchanged.

Two independent builds and reordered inputs produce identical document/package hashes.


## E. Runtime qualification

Use exact final package, exact combat summary and existing Runtime.

### E1. Support full arc

For every relationship with Support:

1. Start exact package.
2. Open actor dialogue.
3. Choose Support.
4. Require exact positive reputation delta.
5. Require first arc quest active and later quests not started.
6. Complete current quest using:
   - exact combat route service;
   - generated reward;
   - read-only quest readiness;
   - manual CompleteQuest exactly once.
7. Reopen actor dialogue.
8. If next quest exists, choose exact StartQuest follow-up.
9. Repeat for all steps.
10. Require final relationship completed response.

No direct quest state mutation.

Capture:

```text
ordered quest IDs technical
quest states before/after
encounter actions
reward/reputation deltas
dialogue available IDs
final Runtime state hash
```

### E2. Challenge

Fresh session:

```text
choose Challenge
exact encounter active
exact combat catalog available
flee branch -> no rewards/quest/reputation
victory branch -> resolved follow-up
defeat/retry remains functional through production campaign service
```

### E3. Refuse

```text
negative exact reputation delta
no quest starts
no encounter
relationship status REFUSED
```

### E4. Independent replay

Execute each relationship branch and entire Support arc twice from independently started sessions.

Require equivalent:

```text
commands/events
arc step order
quest states
flags/reputation
encounter outcomes
final state hash
available follow-ups
```

### E5. Failure atomicity

Invalid next-quest start or invalid exact combat descriptor fixture:

```text
Runtime failure
decision/relationship flag unchanged from pre-command truth
quest/reputation/inventory/encounter state exact
package unchanged
```

## F. Relationship projection and UI

Create:

```text
GeneratedCampaignRelationshipProjectionService
GeneratedCampaignRelationshipProjection
GeneratedCampaignRelationshipRow
GeneratedCampaignQuestArcProjection
```

Projection derives only from:

```text
exact relationship overlay/bindings
Runtime decision flags
faction reputation
quest states/objective readiness
active encounter
```

No second mutable relationship store.

Rows:

```text
Actor
Faction
Branch
Status
Reputation
CurrentQuest
CompletedQuestCount
TotalQuestCount
NextAction
Consequences
```

Counts are data-derived.

### UI

Add or extend a visible tab:

```text
Отношения
```

The existing `Решения` tab remains concise; Relationships shows progression.

At 1100x720:

```text
no clipped primary controls
scrollable rows
human titles
no raw IDs/hashes/paths/diagnostic codes
```

Actor dialogue shows:

```text
Support decision preview
current quest progress
start-next-quest action
final completed response
Challenge/Refuse status
```

## G. Campaign service integration

`GeneratedCampaignSessionService`:

```text
projects Relationships in every snapshot
updates relationship consequences after dialogue/quest/combat actions
uses exact combat route catalog for any automated relationship qualification/test seam
does not auto-start assigned arc quests during StartNew
continues auto-starting only unassigned AutoStart quests
```

To determine assigned quests, consume the selected current v6 relationship summary/overlay from
history.

Add consequence kinds if absent:

```text
RelationshipStarted
RelationshipProgressed
RelationshipCompleted
RelationshipChallenged
RelationshipRefused
QuestArcAdvanced
```

Consequences require actual flag/reputation/quest/encounter deltas or Runtime events.

## H. Build pipeline

Required order:

```text
strict source
→ Lane A
→ generated start/travel
→ Goal167 choice overlay
→ relationship binding/overlay
→ generated combat overlay
→ generated combat qualification
→ Goal167 choice qualification using exact combat service/catalog
→ relationship full-arc qualification using exact combat catalog
→ final validation/history
```

Cases:

```text
relationship arcs + encounters:
  combat + choices + relationships current

Support/Refuse arc profile with zero generated encounters:
  only quests not requiring encounter can form arcs;
  otherwise no Support arc
  choices/relationships may still be current

no relationship arcs:
  relationship summary ABSENT
  campaign readiness does not require it
```

### H1. Goal167 P1 closure

Change `GameProjectGeneratedCampaignChoiceQualificationService.Qualify` signature to receive:

```text
GameProjectGeneratedEncounterCombatSummary? combatSummary
GeneratedCampaignExactCombatRouteService
```

Support/Challenge uses exact service. Remove old combat loop.

### H2. Primary runtime truth

When relationship arcs exist, primary build truth becomes deterministic full relationship route:

```text
Support decision
→ every assigned quest start/combat/turn-in
→ final follow-up
→ Challenge route representative
→ travel
```

Primary:

```text
FinalStateHash
RuntimeFrames
playthrough signature
replay flags
```

belongs to relationship qualification.

Combat and choice summaries retain their own independent final-state hashes.

## I. History v6

Create:

```text
unified_game_project_build_history_v6
```

Persist:

```text
GeneratedWorld
Activation
Travel
GeneratedEncounterCombat
GeneratedCampaignChoices
GeneratedCampaignRelationships
AcceptedMechanics
Compatibility
```

Reader:

```text
v6 + exact relationship summary -> RELATIONSHIPS_CURRENT
genuine v5 -> choices current, relationships RELATIONSHIPS_PENDING
v4 -> choices pending
v3/v2 -> existing behavior
```

Historical rows never rewritten.

### I1. Eligibility

For arc-bearing project require:

```text
Present=true
Passed=true
Status=RELATIONSHIPS_CURRENT
RelationshipCount == QualifiedRelationshipCount
ArcQuestCount == QualifiedArcQuestCount
ExactPackageSha256 == entry.PackageSha256
FinalStateHash == entry.FinalStateHash
ReplayPassed
ExclusiveBranchingPassed
ArcProgressionPassed
ExactCombatCatalogPassed
SaveContinuationFactsPassed when present
overlay/seal hashes exact
```

For v6:

```text
combat summary exact package and independently passed
choice summary exact package and independently passed
their final-state hashes need not equal v6 primary final hash
```

### I2. Campaign readiness

For project with relationship arcs:

```text
v6 RELATIONSHIPS_CURRENT required
```

Genuine v5:

```text
choices CHOICE_CURRENT
relationships RELATIONSHIPS_PENDING
PROJECT_NOT_READY
campaign.generated_relationships_not_current
Projects action Собрать и играть
```

Projects rows:

```text
Столкновения
Сюжетные решения
Отношения
Игровая кампания
```


## J. Regeneration, rollback and seals

Extend candidate seal:

```text
GeneratedCampaignRelationshipSummarySha256
GeneratedCampaignRelationshipOverlaySha256
GeneratedCampaignRelationshipInventorySha256
```

Inventory rows:

```text
RelationshipId
ActorSeedId
FactionId
BranchKinds[]
OrderedQuestSourceIds[]
```

Semantic validator requires:

```text
v6 for arc-bearing candidate
RELATIONSHIPS_CURRENT
counts exact
overlay/final package hashes exact
inventory exact
combat and choices still exact
```

Tamper cases:

```text
relationship actor/faction changed
quest assigned twice
quest order changed
AutoStart changed outside assigned set
next-quest StartQuestId changed
branch flag changed
reputation amount changed
overlay/inventory hash changed
```

Every tamper rejects before commit.

Regeneration and historical rollback rebuild arcs from current mechanics and candidate/historical strict
source. Never restore historical final package.

## K. Save, exact continue and migration

### K1. Exact continue

Save during a middle Support arc.

After recreating campaign service:

```text
Runtime Start count=0
decision flag exact
reputation exact
completed/current/not-started quest states exact
relationship row exact
next action exact
```

Pre-decision save remains undecided.

### K2. Old v5 package rebase

Rebuild v5 project to v6:

```text
old save PACKAGE_REBASE_REQUIRED
direct load rejected
preview zero-write
explicit apply CURRENT
```

### K3. Compatibility policy

No public save schema change.

Use selected source/target build histories and relationship inventories to decide branch compatibility.

Preserve decision flag when:

```text
same exact RelationshipId
same ActorSeedId
same FactionId
same supported branch kind
```

Preserve faction reputation through existing exact faction fingerprint rules.

Generated quest arc state policy:

```text
same-world PACKAGE_REBASE_REQUIRED:
  preserve a generated quest state only when source/target quest source ID,
  objectives, rewards, target encounter/item and relationship assignment are canonically compatible;
  AutoStart-only overlay difference is ignored for compatibility

WORLD_MIGRATION_REQUIRED:
  preserve decision flag/reputation when relationship identity compatible
  reset generated quest arc progress
  active dialogue/encounter reset
```

This is a narrow permitted modification to migration implementation if required.

Record per relationship:

```text
decisionPreserved
arcProgressPreserved
arcProgressReset
droppedReason
```

### K4. No ghost relationships

After migration:

```text
relationship journal/projection contains only retained exact relationships
missing/incompatible actor/dialogue relationship absent
no stale current quest
post-migration dialogue/combat/travel works
```

## L. Standalone and RC

After real v6 all-selectable build:

```text
one cached hidden standalone smoke
retry=0
HostReused=true
HostRebuilt=false
Unity starts=0
exit=0
self-checks GREEN
payload hashes match v6 build
human facts include relationships and multi-quest arc
runtime frames include decision/quest progression/combat/travel
RC CURRENT
```

No standalone/RC implementation change expected.

Portable all-selectable:

```text
v6 relationships current
middle/completed relationship save current
RC CURRENT
no operational pointer
```

Portable core-only:

```text
v6 relationships current
campaign/save current
AcceptedMechanics incomplete
no false RC readiness
no operational pointer
```

## M. Mandatory real product matrix

Use real disposable all-selectable/core-only projects.

### M1. Goal167 exact-combat closure

Build mixed utility-first and ability-only relationship fixtures.

Require:

```text
choice Support/Challenge uses exact qualified damage descriptor
utility ability ignored for victory
package unchanged
```

### M2. Multi-quest Support

Use every relationship with more than one assigned quest when available. Also create a fixture with
multiple assigned quests if the current real seed has only one; fixture counts are test data only.

Require full Support arc through all steps.

### M3. Challenge and Refuse

Run E2/E3.

### M4. Relationship UI

Open/reopen actor dialogue at:

```text
undecided
quest active
between quests
final completed
challenged
refused
```

Require human status and next action.

### M5. Old v5 upgrade

```text
v5 choices current
relationships pending
campaign not ready
one build -> v6 current
source/sidecars unchanged
```

### M6. Regeneration/rollback

Both v6 current.

### M7. Save/migration

Run K1-K4.

### M8. Standalone/portable

Run L.

### M9. Branchless/no-arc

Project with no relationship arcs:

```text
relationships ABSENT
campaign readiness remains based on combat/choices
no false failure
```

## N. Required behavioral tests

Create at least 66 Goal168 tests; at least 58 behavioral.

### Exact combat reuse

1. choice qualification receives exact catalog;
2. old raw WinEncounter removed;
3. utility success excluded;
4. ability-only Support combat succeeds;
5. ability-only Challenge combat succeeds;
6. BasicAttack descriptor observed-effect matched;
7. ability descriptor definition SHA checked;
8. no-progress command rejected;
9. package SHA/reference unchanged;
10. bounded route failure causal.

### Binding/overlay

11. relationship identity exact dialogue ID;
12. quest assigned exactly once;
13. actor-specific encounter assignment preferred;
14. faction/region fallback deterministic;
15. arbitrary quest count supported;
16. arc order deterministic;
17. no Support when no valid quest;
18. Challenge/Refuse independent;
19. assigned AutoStart false;
20. unassigned AutoStart preserved;
21. Support starts first quest;
22. next follow-up starts next quest;
23. final follow-up no next quest;
24. controlled delta only dialogue/assigned quest fields;
25. source metadata/objectives/rewards preserved;
26. independent overlay deterministic;
27. reordered inputs deterministic;
28. forbidden delta rejected.

### Runtime arcs

29. Support positive reputation exact;
30. first quest active;
31. later quests not started;
32. exact combat/manual turn-in step one;
33. next quest starts through dialogue;
34. all steps complete in order;
35. final relationship completed;
36. Support independent replay;
37. Challenge flee no reward/progress;
38. Challenge victory exact catalog;
39. Challenge defeat/retry compatibility;
40. Refuse negative reputation;
41. Refuse starts no quest/encounter;
42. failure atomicity exact.

### Projection/UI

43. undecided row;
44. supported row;
45. quest active row;
46. between-quest next action;
47. completed row;
48. challenged/resolved row;
49. refused row;
50. counts data-derived;
51. decision and relationship journals consistent;
52. primary UI no technical values;
53. 1100x720 relationships fit;
54. state-backed consequences.

### History/regeneration

55. v6 relationship current;
56. genuine v5 pending;
57. v4/v3/v2 compatibility;
58. old project not ready;
59. one build upgrades;
60. branchless summary absent valid;
61. seal summary/overlay/inventory;
62. tamper assignment/order/autostart rejected;
63. regeneration v6;
64. rollback v6.

### Save/migration/standalone

65. exact middle-arc continue;
66. pre-decision continue;
67. v5 rebase required;
68. same-world compatible quest progress preserved;
69. world migration decision preserved/arc reset;
70. incompatible relationship dropped;
71. no ghost row;
72. post-migration dialogue/combat/travel;
73. one hidden smoke;
74. RC current;
75. portable all-selectable;
76. portable core-only no false RC.

### Regressions

77. Goal167 94/94;
78. Goal166 59/59;
79. Goal165 55/55;
80. Goal164 61/61;
81. Goal163/162/161;
82. Runtime Simulator;
83. source/sidecar immutability.

No source-string-only assertion counts as behavioral proof.


## O. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal168"
# require >=66 total / >=58 behavioral

dotnet test ... --filter "FullyQualifiedName~Goal168"
dotnet test ... --filter "FullyQualifiedName~Goal167"
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
dotnet test ... --filter "FullyQualifiedName~ProjectStandaloneBuild"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then execute M and exactly one hidden smoke.

Do not run:

```text
full suite
85-case historical closure
all-ProductSmoke
Unity host build
more than one hidden smoke
corrective retry
visible app/player launch
```

A zero-match filter is failure.

## P. Evidence

Create exactly 15 files in each mirrored root:

```text
goal168-dashboard.json
architecture-review.json
goal167-independent-audit-finding.json
exact-choice-combat-reuse-proof.json
relationship-binding-overlay-proof.json
support-multi-quest-arc-proof.json
challenge-refuse-proof.json
relationship-ui-proof.json
history-regeneration-proof.json
save-exact-continue-proof.json
relationship-migration-proof.json
standalone-rc-portability-proof.json
regression-immutability-proof.json
artifact-scope-proof.json
goal168-report.md
```

Roots:

```text
.llmgc/procedural/goal-168-choice-driven-relationships-and-multi-quest-arcs/
.llmgc/exports/goal-168-choice-driven-relationships-and-multi-quest-arcs/
```

Twins byte-identical.

Every field comes from typed captures, not unmeasured constants.

### Dashboard fields

```text
status
candidateStatus
goal168TestsDiscovered
goal168BehavioralTestsPassed

goal167AuditBlockerRecorded
goal167AuditBlockerClosed

exactCombatCatalogReusePassed
rawChoiceWinEncounterRemoved
abilityOnlySupportPassed
abilityOnlyChallengePassed
utilityAbilityIgnored
packageShaUnchanged

relationshipCount
qualifiedRelationshipCount
arcQuestCount
qualifiedArcQuestCount
maximumObservedArcLength
arbitraryArcLengthPassed
questAssignmentUnique
arcOrderingDeterministic
relationshipOverlayControlledDeltaPassed

supportRelationshipPassed
supportReputationDelta
supportArcStarted
supportCompletedQuestCount
supportFinalCompleted
supportReplayEquivalent
challengeFleePassed
challengeVictoryPassed
challengeRecoveryPassed
refusePassed
refuseReputationDelta
relationshipFailureAtomicRollbackPassed

relationshipProjectionPassed
relationshipPrimaryUiNoRawIds
decisionRelationshipConsistencyPassed

historySchemaVersion
v6RelationshipsCurrent
v5RelationshipsPending
v5CampaignNotReady
oldProjectBuildInvocationCount
oldProjectUpgradedWithoutSourceRewrite
relationshipPrimaryFinalStatePassed
combatChoiceSummariesPreserved
regenerationRelationshipsCurrent
rollbackRelationshipsCurrent
relationshipSealTamperRejected

exactMiddleArcContinuePassed
exactContinueRuntimeStartCount
preDecisionContinuePassed
oldV5SaveRebaseRequired
sameWorldQuestProgressPreserved
worldMigrationDecisionPreserved
worldMigrationArcReset
incompatibleRelationshipDropped
ghostRelationshipAbsent
postMigrationDialogueCombatTravelPassed

hostCacheKey
hostReused
hostRebuilt
unityEditorProcessStartCount
hiddenSmokeInvocationCount
hiddenSmokePassed
correctiveSmokeRetryCount
actualPayloadRelationshipFactsPassed
releaseCandidateRecordCurrent
portableAllSelectablePassed
portableCoreOnlyPassed
coreOnlyNoFalseRcReady

goal167RegressionPassed
goal166RegressionPassed
goal165RegressionPassed
goal164RegressionPassed
goal163RegressionPassed
goal162RegressionPassed
goal161RegressionPassed
runtimeSimulatorRegressionPassed
generatedSaveRegressionPassed

goal142SourceByteIdentical
sourceGoal148ByteIdentical
generationSidecarsByteIdentical
artifactScopeViolationCount

goal168Accepted=false
goal168ManualReviewRequired=false
goal168IndependentAuditRequired=true
```

No required GREEN value null/PARTIAL/NOT_EXECUTED.

## Q. State and docs

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
docs/manual-acceptance/goal167-generated-choice-branching.md
```

Create:

```text
docs/manual-acceptance/goal168-choice-driven-relationships-multi-quest-arcs.md
```

Required GREEN state:

```text
goal167IndependentAuditResult=BLOCKED_AT_FD69BFC8
goal167IndependentAuditBlocker=choice_branch_qualification_reimplements_victory_without_goal166_exact_qualified_action_catalog
goal167AuditBlocker=closed_by_goal168

goal167ImplementationStatus=GREEN
goal167CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal167Accepted=false
goal167IndependentAuditRequired=false

goal168ImplementationStatus=GREEN
goal168CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal168Accepted=false
goal168AcceptedByHuman=false
goal168AcceptedByCodex=false
goal168ManualReviewRequired=false
goal168ManualGateReady=false
goal168IndependentAuditRequired=true

goal168ExactCombatReusePassed=true
goal168RelationshipOverlayPassed=true
goal168MultiQuestArcPassed=true
goal168RelationshipSaveContinuePassed=true
goal168RelationshipMigrationPassed=true
goal168HostReused=true
goal168HostRebuilt=false
goal168UnityEditorProcessStartCount=0
goal168HiddenSmokeInvocationCount=1
goal168PortableAllSelectablePassed=true
goal168PortableCoreOnlyPassed=true
goal168ArtifactScopeViolationCount=0

nextAction=independent_goal168_audit_and_plan_world_events_or_relationship_consequences
```

No human gate.

Release risk statement:

```text
Generated decisions now form actor/faction relationships and deterministic quest arcs of arbitrary
data-defined length. Support unlocks assigned quests sequentially; Challenge and Refuse remain
exclusive. All branch combat consumes the exact Goal166 qualified-action catalog. Exact saves preserve
relationship progress, while migration applies explicit compatible-preserve/reset/drop rules.
```

## R. Text integrity

Scan changed/task/evidence/docs:

```text
valid UTF-8
no NUL
no forbidden C0 except CR/LF/TAB
no mojibake markers
no escaped Cyrillic where forbidden
no absolute disposable paths in evidence
```

## S. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal168-relationship-arcs.ps1
.devflow/scripts/run-goal168-relationship-arcs.cmd

src/LLMGameCreator.Application/Generation/Procedural/GeneratedCampaignRelationshipModels.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedCampaignRelationshipBindingService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedCampaignRelationshipOverlayService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedCampaignRelationshipQualificationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedCampaignChoiceQualificationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCandidateSealService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCommitValidator.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationModels.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationRecordService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplaySaveMigrationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplayDefinitionFingerprintService.cs

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildHistoryReader.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectGeneratedWorldSummaryService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs

src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignExactCombatRouteService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionModels.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignRelationshipProjectionService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionTruthService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignActionPlanner.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignDialogueChoicePreviewService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignDecisionJournalService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignConsequenceProjector.cs

src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignPageControl.cs
src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/Goal168/Goal168ExactCombatReuseTests.cs
tests/LLMGameCreator.Tests/Application/Goal168/Goal168RelationshipBindingOverlayTests.cs
tests/LLMGameCreator.Tests/Application/Goal168/Goal168SupportArcTests.cs
tests/LLMGameCreator.Tests/Application/Goal168/Goal168ChallengeRefuseTests.cs
tests/LLMGameCreator.Tests/Application/Goal168/Goal168RelationshipUiTests.cs
tests/LLMGameCreator.Tests/Application/Goal168/Goal168HistoryRegenerationTests.cs
tests/LLMGameCreator.Tests/Application/Goal168/Goal168SaveMigrationTests.cs
tests/LLMGameCreator.Tests/Application/Goal168/Goal168StandalonePortabilityTests.cs
tests/LLMGameCreator.Tests/Application/Goal168/Goal168RegressionImmutabilityTests.cs

tests/LLMGameCreator.Tests/Application/Goal167/Goal167BranchRuntimeQualificationTests.cs
tests/LLMGameCreator.Tests/Application/Goal167/Goal167SaveMigrationTests.cs
tests/LLMGameCreator.Tests/Application/Goal166/Goal166MixedAbilityQualificationTests.cs
tests/LLMGameCreator.Tests/Application/Goal166/Goal166RealDefeatRetryTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/manual-acceptance/goal167-generated-choice-branching.md
docs/manual-acceptance/goal168-choice-driven-relationships-multi-quest-arcs.md

docs/agent-tasks/goal-168-choice-driven-relationships-and-multi-quest-arcs/
.llmgc/procedural/goal-168-choice-driven-relationships-and-multi-quest-arcs/
.llmgc/exports/goal-168-choice-driven-relationships-and-multi-quest-arcs/
```

One exact additional existing campaign/history test/model path may be added after a concrete
compile/test failure with recorded reason.

Forbidden:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Domain/**
catalogs/feature-modules/**
unity/**
ProceduralGameKernel*
GeneratedPackageMvp*
GeneratedProjectOverlay*
GeneratedWorldEncounterCombatOverlay*
ProjectStandaloneBuild implementation
GameProjectReleaseCandidate implementation
```


## T. Command budget

```text
read-first/architecture: 12 minutes
exact combat route reuse: 16 minutes
relationship binding/overlay: 22 minutes
arc Runtime qualification: 24 minutes
projection/UI/campaign integration: 18 minutes
history/regeneration/migration: 24 minutes
behavioral tests: 32 minutes
real matrix/smoke/portable: 18 minutes
regressions/evidence/docs/scope: 20 minutes
target wall clock: 145 minutes
maximum two concurrent testhost processes
Unity host builds: 0
hidden smoke: exactly 1
```

Rules:

```text
write full test inventory before production edits
write evidence/publication runner before real matrix
do not ask for extra plan confirmation
no unchanged command retries
no timeout escalation
after failure isolate exact class/test
P0/P1 fixed inside Goal168
P2/P3 debt only
do not stop at compile success
do not defer evidence/docs/scope
```

## U. Publication

Create exactly one final commit:

```text
GREEN Goal 168 choice driven relationships and multi quest arcs
```

or honest BLOCKED/FAILED.

Codex must push `origin/main`.

Required final:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three task files tracked
HostReused=true
HostRebuilt=false
Unity starts=0
hidden smoke=1
corrective retry=0
Goal142/Goal148/source sidecars unchanged
Goal167/168 accepted=false
no human gate
```

## V. GREEN criteria

```text
Goal167 audit blocker recorded and closed
Goal168 >=66 discovered / >=58 behavioral / all pass
all relationship combat reuses exact Goal166 catalog
ability-only and utility-first branch qualification
data-derived arbitrary-length quest assignment/order
controlled deterministic dialogue/quest overlay
full Support multi-quest progression
Challenge flee/victory/recovery
Refuse consequence
relationship projection/UI
v6 RELATIONSHIPS_CURRENT
v5 pending/campaign not ready
regeneration/rollback sealed
exact middle-arc continue
explicit migration preserve/reset/drop
no ghost relationship
one cached hidden smoke
RC CURRENT
portable all-selectable/core-only
required regressions GREEN
source/sidecars immutable
15+15 typed evidence
text integrity GREEN
artifact scope 0
one final commit pushed
```

## W. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning;
- Goal167 independent-audit intake;
- exact combat-catalog reuse and removed duplicate victory logic;
- ability-only/utility-first branch results;
- relationship/quest assignment and maximum observed arc length;
- controlled delta/determinism;
- Support full arc;
- Challenge flee/victory/recovery;
- Refuse;
- relationship UI/projection;
- v6/v5/v4/v3/v2 history/readiness;
- regeneration/rollback seals;
- save/continue and migration policy/results;
- host/Unity/smoke/payload/RC;
- portable all-selectable/core-only;
- tests/regressions/source immutability;
- evidence/text/artifact scope;
- state/no-human-gate;
- SHA/push/HEAD/worktree;
- explicit confirmation Codex committed and pushed for any status.
