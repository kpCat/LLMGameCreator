# Goal 169 — Profile-Neutral Relationships & Reactive Regional Events

## Identity

- Task ID: `goal-169-profile-neutral-relationships-and-reactive-regional-events`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `bbfd46a23cd6c6d2012626cac77bda316cb9c7a3`
- Required base message: `GREEN Goal 168 choice driven relationships and multi quest arcs`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: Goal168 delivered exact-catalog relationship arcs, but independent review found two
profile-neutral P1 defects in relationship qualification/history and exact combat execution.
Goal169 must close those defects and add the next visible product layer: deterministic regional
events that react to actual Support, Challenge and Refuse outcomes.

## Pre-approval and publication

- Launching this task approves the complete plan.
- Do not ask for another confirmation because many files are involved.
- Produce a concise internal plan and proceed.
- Do not request manual testing.
- Own every P0/P1 reproduced by the Goal169 matrix.
- Record P2/P3 debt without creating an automatic Goal169A.
- Create and push exactly one final `GREEN`, `BLOCKED` or `FAILED` commit.
- On `BLOCKED` or `FAILED`, commit and push the honest state.
- Never leave commit or push to the user.
- No intermediate commits.

## Initial worktree

After unpacking, only these untracked files are allowed:

```text
docs/agent-tasks/goal-169-profile-neutral-relationships-and-reactive-regional-events/GOAL.md
docs/agent-tasks/goal-169-profile-neutral-relationships-and-reactive-regional-events/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-169-profile-neutral-relationships-and-reactive-regional-events/README.md
```

Require before production edits:

```text
HEAD == origin/main == bbfd46a23cd6c6d2012626cac77bda316cb9c7a3
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
maximum concurrent testhost processes: 2
```

Use the existing cached host. Do not change Unity source.

---

# 1. Goal168 independent-audit intake

Record:

```text
goal168IndependentAuditResult=BLOCKED_AT_BBFD46A2
goal168ImplementationCommit=bbfd46a23cd6c6d2012626cac77bda316cb9c7a3
goal168P1A=relationship_qualification_and_v6_history_require_all_branches_and_nonempty_arc
goal168P1B=exact_relationship_combat_rejects_non_health_goal166_effects
goal168TruthDebt=build_save_continuation_facts_hardcoded_true
goal168AuditBlockers=closed_by_goal169 only on GREEN
```

Goal168 implementation remains `GREEN`, `accepted=false`, no human gate.

## 1.1 Goal168 truths to preserve

```text
exact relationship identity = generated dialogue ID
deterministic unique generated-quest assignment
data-derived arbitrary-length quest arcs
assigned arc quests AutoStart=false
Support sequential quest start/combat/manual turn-in/follow-up
Challenge flee/victory/recovery
Refuse negative reputation and no quest/encounter
exclusive branch locking
exact Goal166 qualified-action catalog input
ability-only and mixed utility-first proof
relationship projection and «Отношения» UI
history v6 RELATIONSHIPS_CURRENT
genuine v5 RELATIONSHIPS_PENDING
regeneration/rollback relationship seals
exact middle-arc continue
relationship migration preserve/reset/drop
one cached hidden smoke
RC CURRENT
portable all-selectable/core-only
Goal168 85/85
Goal167 94/94
Goal166 59/59
Goal165 55/55
Goal164 61/61
Goal142/Goal148/source sidecars byte-identical
```

## 1.2 P1-A — branch qualification is not profile-neutral

Current `GameProjectGeneratedCampaignRelationshipQualificationService.Qualify()` unconditionally
executes for every binding:

```text
ExecuteSupport twice
ExecuteChallenge FLEE
ExecuteChallenge VICTORY
ExecuteRefuse
```

Current `ExecuteSupport()` immediately reads `relationship.QuestArc[0]`.

Current v6 `RelationshipEligible()` unconditionally requires:

```text
SupportPassed
ChallengeFleePassed
ChallengeVictoryPassed
ChallengeRecoveryPassed
RefusePassed
MaximumObservedArcLength > 0
```

But Goal168 binding legally produces:

```text
SUPPORT + REFUSE, no CHALLENGE
CHALLENGE-only, QuestArc.Count=0
SUPPORT-only
REFUSE-only
no branches
all branches
```

Therefore current qualification/history rejects legal generated profiles.

## 1.3 Required P1-A closure

Add explicit per-relationship branch truth:

```text
Available
Required
Passed
ReplayEquivalent
RuntimeInvocationCount
Diagnostics
```

Rules:

```text
branch present in binding:
  Available=true
  Required=true
  execute and prove the real route
  Passed=result

branch absent:
  Available=false
  Required=false
  execute zero Runtime starts/commands
  Passed=true as NOT_APPLICABLE
  ReplayEquivalent=true as NOT_APPLICABLE
```

Do not infer an absent branch from a failed execution.

Mandatory fixtures:

```text
all branches, arc length > 0
CHALLENGE-only, arc length = 0
SUPPORT + REFUSE, no CHALLENGE
SUPPORT-only
REFUSE-only
no branches
```

Every legal fixture must qualify without false branch execution.

## 1.4 P1-B — exact combat is health-only

Current `GeneratedCampaignExactCombatRouteService.ExecuteQualifiedPlayerAction()` already:

```text
uses only Goal166 QualifiedActions
validates exact ability definition SHA
observes the Runtime effect
matches descriptor fingerprint and target IDs
requires encounter state change
```

but then adds the invalid restriction:

```text
observed.EffectClass == TARGET_HEALTH_DECREASE
```

Goal166 exact descriptors support:

```text
TARGET_HEALTH_DECREASE
TARGET_STAT_CHANGED
TARGET_STATUS_CHANGED
```

A status route may produce delayed damage after later turn progression.

## 1.5 Required P1-B closure

Remove the health-only restriction.

Accept a player action only when all are true:

```text
Runtime command succeeds
descriptor definition remains exact
TryObserveSupportedEffect succeeds
observed effect exactly matches descriptor
target resource/stat/status fingerprints match
active encounter canonical state changes
package reference and SHA remain exact
```

A successful utility/no-op command is not progress.

The complete route must still end with actual Runtime victory. A stat/status action may be one
progressing step, not an automatic victory claim.

Mandatory exact-effect fixtures:

```text
health decrease
stat change that causally progresses the route
status application
status application followed by delayed status damage/turn progression
mixed utility-first catalog
utility/no-op success rejected
ability-only route
package SHA/reference unchanged
```

## 1.6 Truth debt — SaveContinuationFacts

Current relationship build qualification writes:

```text
SaveContinuationFactsPassed=true
```

without executing save/continue in build qualification.

Replace with explicit honest truth:

```text
SaveContinuationFactsEvaluationStatus=NOT_EVALUATED_AT_BUILD
SaveContinuationFactsPassed=false
```

The real save/continue matrix remains separate post-build evidence. History eligibility must not
require an unexecuted build-time save proof.

Legacy v6 rows:

```text
old all-branch fully GREEN rows remain readable/current
old false/partial rows must not be reinterpreted as unavailable/N/A
historical rows are never rewritten
```

---

# 2. Product outcome

Goal169 adds reactive regional events derived from actual relationship outcomes:

```text
Support arc completed
→ regional gratitude/restoration event becomes available

Challenge won and resolved
→ conflict-aftermath event becomes available

Refuse chosen
→ refusal-fallout event becomes available
```

The player sees a deterministic marker on the generated map, walks to it, uses ordinary
interaction, opens an ordinary Runtime dialogue and resolves the event exactly once.

Lifecycle:

```text
LOCKED
→ actual prerequisite relationship outcome
→ AVAILABLE
→ ordinary Move/Interact/dialogue/choice
→ RESOLVED
→ exact save/continue
→ explicit regeneration/migration compatibility
```

No raw IDs, hashes, paths or diagnostic codes in primary UI.

---

# 3. Non-goals

Do not change:

```text
LLMGameCreator.Runtime
LLMGameCreator.Runtime.Abstractions
LLMGameCreator.GamePackage public schema
LLMGameCreator.Domain public definitions
FeatureModule catalogs
ProceduralGamePlan schema
GeneratedPackageMvp source or generation sidecars
world topology generation
existing travel overlay semantics
existing generated combat overlay semantics
Unity source or cached host
standalone implementation
release-candidate implementation
cloud/multiplayer
```

Do not introduce a second Runtime, event state store, scripting engine or event command.
Use existing package definitions and existing Runtime commands.

---

# 4. Mandatory architecture review

Read at most 22 primary production files before editing:

```text
GeneratedCampaignRelationshipModels.cs
GeneratedCampaignRelationshipBindingService.cs
GeneratedCampaignRelationshipOverlayService.cs
GameProjectGeneratedCampaignRelationshipQualificationService.cs
GeneratedCampaignExactCombatRouteService.cs
GeneratedEncounterCombatContractModels.cs
GeneratedEncounterCombatContractService.cs
GameProjectGeneratedEncounterCombatQualificationService.cs
GameProjectBuildAndQualificationService.cs
GameProjectWorkspaceModels.cs
GameProjectBuildHistoryReader.cs
GameProjectSeedRegenerationCandidateSealService.cs
GameProjectSeedRegenerationCommitValidator.cs
GeneratedGameplaySaveMigrationService.cs
GeneratedCampaignSessionTruthService.cs
GeneratedCampaignSessionService.cs
GeneratedCampaignSessionModels.cs
GeneratedCampaignProjectionService.cs
GeneratedCampaignActionPlanner.cs
GeneratedCampaignRelationshipProjectionService.cs
GeneratedCampaignPageControl.cs
Goal168 relationship/combat/history/save tests
```

Before production edits write:

```text
.llmgc/procedural/goal-169-profile-neutral-relationships-and-reactive-regional-events/architecture-review.json
```

Required sections:

```text
goal168IndependentAudit
relationshipBranchMatrix
legacyV6Compatibility
honestSaveContinuationTruth
exactEffectMatrix
regionalEventIdentity
regionalEventPrerequisites
regionalEventPlacement
regionalEventOverlayControlledDelta
regionalEventRuntimeLifecycle
regionalEventProjection
historyV7
campaignReadiness
regenerationRollbackSeal
exactSaveContinue
migrationCompatibility
standaloneRcPortable
failureMatrix
regressionImmutability
nonGoals
```

Each section names exact existing/new types, inputs, outputs, hashes, failure diagnostics and
behavioral tests.

---

# 5. Profile-neutral relationship qualification

## 5.1 Models

Extend `GeneratedCampaignRelationshipModels.cs` with a typed per-binding fact, for example:

```text
GeneratedCampaignRelationshipBranchQualification
  RelationshipId
  Branch
  Available
  Required
  Passed
  ReplayEquivalent
  RuntimeStartCount
  RuntimeCommandCount
  ArcLength
  FinalStateHash
  Diagnostics
```

The exact name may differ, but persisted truth must be typed and explicit.

Extend `GameProjectGeneratedCampaignRelationshipSummary` with:

```text
BranchQualifications[]
SupportAvailableCount
SupportRequiredCount
SupportQualifiedCount
ChallengeAvailableCount
ChallengeRequiredCount
ChallengeQualifiedCount
RefuseAvailableCount
RefuseRequiredCount
RefuseQualifiedCount
UnavailableBranchRuntimeStartCount
SaveContinuationFactsEvaluationStatus
```

Retain old aggregate flags for compatibility, but derive them honestly:

```text
SupportPassed = every Required SUPPORT row passed
ChallengeFleePassed = every Required CHALLENGE flee row passed
ChallengeVictoryPassed = every Required CHALLENGE victory row passed
ChallengeRecoveryPassed = every Required CHALLENGE recovery row passed
RefusePassed = every Required REFUSE row passed
```

If no row requires a branch, its aggregate is true as vacuous `NOT_APPLICABLE`.

## 5.2 Runtime execution

For each relationship:

```text
if SUPPORT present:
  execute two independent full Support replays
else:
  execute zero Support routes

if CHALLENGE present:
  execute FLEE and VICTORY/recovery
else:
  execute zero Challenge routes

if REFUSE present:
  execute Refuse
else:
  execute zero Refuse routes
```

`relationshipPassed` means:

```text
binding/overlay exact
every Required branch passed
all unavailable branches executed zero Runtime calls
```

`QualifiedArcQuestCount` includes only successful required Support arcs.

A Challenge-only relationship with `QuestArc.Count=0` is valid.
A no-branch relationship may qualify through exact binding/overlay truth with every branch
`NOT_APPLICABLE`; it must not manufacture gameplay.

## 5.3 Atomic rollback

Run branch atomicity only against an actually available branch.

If a binding has no branches:

```text
atomic branch rollback Required=false
Passed=true as NOT_APPLICABLE
Runtime calls=0
```

Never index `QuestArc[0]` unless Support is present and the arc is nonempty.

## 5.4 History compatibility

For new v7 rows, `RelationshipEligible()` consumes the explicit branch matrix.

For old v6 rows without this matrix:

```text
accept only the legacy fully proven all-branch shape
do not treat old false fields as unavailable
retain current exact package/inventory/frame/hash checks
```

Remove global requirement `MaximumObservedArcLength > 0` for new profile-neutral rows.
Require positive arc length only for each relationship where Support is required.

Diagnostics:

```text
generated_relationship.branch_matrix_missing
generated_relationship.unavailable_branch_executed
generated_relationship.required_branch_not_qualified
generated_relationship.support_arc_missing
generated_relationship.legacy_v6_partial_not_eligible
```

---

# 6. Effect-neutral exact combat

Update `GeneratedCampaignExactCombatRouteService`.

## 6.1 Exact action acceptance

Canonical action order remains Goal166 order.

For every descriptor:

```text
clone session
execute exact BasicAttack or UseAbility
observe supported effect
match exact descriptor
require encounter-state change
accept the first exact progressing action
```

Do not filter by health effect class.
Use the existing Goal166 contract service/canonical matcher instead of duplicating looser matching
when possible.

## 6.2 Delayed effects

A `TARGET_STATUS_CHANGED` action counts as an exact progressing step when the descriptor matches and
the encounter state changes.

Continue the bounded turn loop normally:

```text
opponent AI / turn progression
status tick or delayed damage
next exact qualified player action
actual victory
```

Do not synthesize damage and do not mutate participant state.

## 6.3 Failure semantics

```text
utility/no-op Runtime Success:
  descriptor observation fails or encounter state unchanged
  try the next descriptor

all descriptors no-op/inapplicable:
  generated_relationship.qualified_action_no_progress
  generated_relationship.arc_combat_failed

descriptor/package changed:
  generated_relationship.qualified_action_definition_changed
```

No false `EncounterProgressObserved`.

---

# 7. Regional event architecture

## 7.1 New types

Create:

```text
GeneratedCampaignRegionalEventModels.cs
GeneratedCampaignRegionalEventBindingService.cs
GeneratedCampaignRegionalEventOverlayService.cs
GameProjectGeneratedCampaignRegionalEventQualificationService.cs
```

Required conceptual types:

```text
GeneratedCampaignRegionalEventKind
  SUPPORT_GRATITUDE
  CHALLENGE_AFTERMATH
  REFUSAL_FALLOUT

GeneratedCampaignRegionalEventStatus
  LOCKED
  AVAILABLE
  RESOLVED

GeneratedCampaignRegionalEventBinding
GeneratedCampaignRegionalEventPlacement
GeneratedCampaignRegionalEventInventoryRow
GeneratedCampaignRegionalEventOverlayDocument
GeneratedCampaignRegionalEventRuntimeFrame
GeneratedCampaignRegionalEventHumanFact
GameProjectGeneratedCampaignRegionalEventSummary
GeneratedCampaignRegionalEventMigrationFact
```

## 7.2 Identity

Use deterministic generated identities:

```text
RegionalEventId = exact generated event DialogueDefinition.Id
ResolutionFlagId = exact generated event DialogueDefinition.Id
```

Binding also records exact:

```text
RelationshipId
RelationshipBranch
ActorSeedId
ActorEntityId
FactionId
RegionId
MapId
EntityPrototypeId
MapEntityId
InteractionId
DialogueId
EventKind
Prerequisite quest/encounter/flag truth
Placement X/Y
```

IDs must derive from exact relationship identity, event kind and existing deterministic ID helpers.
No hardcoded actor, faction, region, map or coordinate IDs.

Each relationship may generate at most one event per available outcome kind.

## 7.3 Event derivation

### Support gratitude

Create only when:

```text
SUPPORT Available=true
QuestArc.Count > 0
```

Prerequisite:

```text
decision flag == SUPPORT
every assigned arc quest completed
final Support follow-up reached/available from actual state
```

Target region:

```text
final arc quest RegionId when exact
otherwise relationship home RegionId
```

Resolution reputation delta:

```text
positive
derived from the final assigned quest faction reputation reward
not a hardcoded constant
```

Record exact source quest/reward fingerprint.

### Challenge aftermath

Create only when:

```text
CHALLENGE Available=true
exact ChallengeEncounterId exists
```

Prerequisite:

```text
decision flag == CHALLENGE
exact challenge encounter resolved by victory
```

Flee does not unlock the aftermath event.

Target region:

```text
challenge encounter generated region when exactly resolvable
otherwise relationship home RegionId
```

Event resolution must not duplicate the original Challenge reputation effect.

### Refusal fallout

Create only when `REFUSE Available=true`.

Prerequisite:

```text
decision flag == REFUSE
```

Target region is the exact relationship region.
Event resolution must not duplicate the original Refuse reputation penalty.

### No branch

No event is generated for an unavailable branch.
A relationship with no branches produces zero events.

---

# 8. Deterministic safe map placement

## 8.1 Map resolution

Resolve exact target map through current region/map bindings.
Reject ambiguous or missing region-to-map resolution.

## 8.2 Candidate cells

Calculate candidates from actual map dimensions and package walkability.

Exclude:

```text
out-of-bounds cells
nonwalkable cells
player start cell
existing blocking entity cells
travel-gate cells
cells whose event interaction cannot be reached
cells already assigned to another Goal169 event
```

Do not assume map width, height, coordinate, event count or gate count.

## 8.3 Reachability

Use deterministic bounded BFS/graph traversal over actual walkable map cells.

Anchor preference:

```text
exact relationship actor entity position when on target map
otherwise exact reachable map entry/start/gate anchor
```

A selected cell must be reachable from at least one valid gameplay entry anchor.

## 8.4 Ordering

Bind events in canonical order:

```text
RegionId
MapId
RelationshipId
EventKind
DialogueId
```

Choose placement by canonical score:

```text
reachable path distance from anchor
collision/critical-cell exclusions
Y
X
```

A deterministic hash rotation may distribute equal candidates only if reordered input produces the
same placement.

Require unique placements when enough valid cells exist. If the package cannot place all events
safely, fail causally rather than using fixed coordinates.

Diagnostics:

```text
generated_regional_event.region_map_missing
generated_regional_event.safe_cell_missing
generated_regional_event.cell_not_reachable
generated_regional_event.placement_collision
generated_regional_event.placement_nondeterministic
```

---

# 9. Regional event overlay

Input:

```text
exact Goal168 relationship-overlay package
strict generated source/provenance
profile-neutral relationship bindings
regional event bindings/placements
```

Output may add only Goal169 event-owned records:

```text
event entity prototypes
event dialogues
event interactions
event map entities on exact target maps
generatedRegionalEvent* metadata/tags on newly added event records
```

Do not modify existing source records.

## 9.1 Ordinary interaction contract

Each event map entity uses an ordinary interaction definition that opens event dialogue.

Use existing Runtime flow:

```text
Move
Interact
OpenDialogue caused by interaction
ChooseDialogueOption
flag/reputation effects from ordinary dialogue choice
```

No new Runtime command.

## 9.2 Dialogue lifecycle

Before prerequisite:

```text
resolution choice unavailable
status LOCKED
zero event flag/reputation mutation
```

After prerequisite:

```text
resolution choice available
status AVAILABLE
```

On first resolution:

```text
ResolutionFlagId set to RESOLVED
Support gratitude applies exact derived positive reputation once
Challenge/Refuse apply no duplicate branch reputation
status RESOLVED
```

On second interaction:

```text
resolution choice unavailable
resolved human follow-up may remain
no duplicate flag/reputation/consequence
```

Use existing `flag_equals`, `quest_state` and available Runtime requirement forms. Do not change
public requirement schema.

## 9.3 Controlled delta

Require canonical equality for all pre-existing records.

Permitted additions only:

```text
new event prototypes/dialogues/interactions
new event map entities
their exact generatedRegionalEvent metadata/tags
```

Preserve exactly:

```text
Manifest
GeneratedContent and generation sidecars
all existing entity prototypes
all existing dialogues
all existing interactions
all existing map entities
all quests/rewards/factions
relationship overlay records
combat overlay records
travel gates
world topology
```

Collection counts may change only by exact event inventory additions.

Two independent builds and reordered input must produce identical:

```text
event bindings
placements
overlay document
output package hash
inventory hash
definition fingerprints
```

Diagnostics:

```text
generated_regional_event.overlay_delta_outside_scope
generated_regional_event.existing_definition_changed
generated_regional_event.inventory_mismatch
generated_regional_event.output_nondeterministic
```

---

# 10. Regional event Runtime qualification

Use exact final package and existing Runtime.

## 10.1 LOCKED route

For every generated event:

1. Start a fresh exact session.
2. Navigate to the event marker with data-derived Move commands.
3. Interact using ordinary Runtime.
4. Require status `LOCKED`.
5. Require resolution choice unavailable.
6. Require exact zero mutation of event flag, relationship flag, reputation, quests, inventory,
   encounter and package.

No direct `OpenDialogue` bypass counts as map-interaction proof.

## 10.2 AVAILABLE route

Execute the actual prerequisite relationship route:

```text
Support:
  full arc through every quest and final follow-up

Challenge:
  exact qualified victory, not flee

Refuse:
  exact Refuse branch
```

Then:

1. Navigate to marker.
2. Interact.
3. Require event dialogue open.
4. Require exact resolution choice available.
5. Require projected status `AVAILABLE`.

## 10.3 RESOLVED route

Choose ordinary dialogue option.

Require:

```text
ResolutionFlagId == RESOLVED
status RESOLVED
dialogue/event Runtime events captured
package unchanged
```

Support event:

```text
observed reputation delta == exact derived final quest reward amount
delta applied once
```

Challenge/Refuse event:

```text
event resolution reputation delta == 0
original branch consequence not duplicated
```

## 10.4 Exactly once

Reopen/re-interact after resolution.

Require:

```text
resolution choice unavailable
resolution flag unchanged
reputation unchanged
quests/inventory/encounter unchanged
no duplicate EventResolved consequence
```

## 10.5 Replay

Execute every event lifecycle twice from independent sessions.

Require equivalent:

```text
movement/interact/dialogue command sequence
Runtime event sequence
relationship prerequisite outcome
event status transitions
flag/reputation deltas
final state hash
available choices
```

## 10.6 Failure atomicity

Use malformed prerequisite/choice/placement fixtures.

Require:

```text
Runtime failure or unavailable action
event flag unchanged
relationship state unchanged
reputation/quest/inventory/encounter exact
package unchanged
```

No source-string-only proof.

---

# 11. Projection, map and UI

Create:

```text
GeneratedCampaignRegionalEventProjectionService
GeneratedCampaignRegionalEventProjection
GeneratedCampaignRegionalEventRow
```

Projection derives only from:

```text
selected current v7 event summary/overlay
exact package event records
Runtime relationship flags
Runtime event resolution flags
quest states
encounter outcome state
faction reputation
current map/player position
```

No second mutable event store.

Rows:

```text
Title
Kind
Source actor/faction
Region
Status: Заблокировано / Доступно / Завершено
Prerequisite
NextAction
Consequence
Distance/current-map hint
```

Add to `GeneratedCampaignSnapshot`:

```text
RegionalEvents
```

## 11.1 Map marker

The package map entity is marker authority.
Marker title/symbol must be human and data-derived.

Primary map/nearby UI may show:

```text
Событие мира
Благодарность региона
Последствия конфликта
Последствия отказа
```

Do not expose technical identity.

## 11.2 Tab

Add or extend visible tab:

```text
События мира
```

At `1100x720`:

```text
no clipped primary controls
scrollable rows
human titles
LOCKED/AVAILABLE/RESOLVED visible
no raw IDs/hashes/paths/diagnostic codes
```

Keep existing `Решения` and `Отношения` tabs.

## 11.3 State-backed preview/journal/consequences

Event descriptions and consequences must come from actual state, Runtime events or exact binding
facts.

Add consequence kinds if needed:

```text
WorldEventAvailable
WorldEventResolved
WorldEventLocked
```

Do not infer resolved status merely because a definition exists.

---

# 12. Campaign service integration

`GeneratedCampaignSessionService` must:

```text
project RegionalEvents on every active snapshot
recompute event status after dialogue/quest/combat actions
expose ordinary context actions near event marker
block stale-project commands before Runtime
preserve existing relationship/decision projections
```

`GeneratedCampaignActionPlanner` derives event interaction actions from actual nearby package
entities. No typed event IDs in primary UI.

Exact continue restores projections without `Runtime.Start`.

---

# 13. Build pipeline

Required order:

```text
strict source
→ Lane A
→ generated start/travel
→ choice overlay
→ profile-neutral relationship binding/overlay
→ regional event binding/placement/overlay
→ generated combat overlay
→ generated combat qualification
→ choice qualification using exact catalog
→ profile-neutral relationship qualification
→ regional event qualification
→ final validation/history v7
```

All qualification summaries refer to the exact final package.
Regional event overlay must not invalidate combat, choice or relationship package correlation.

Primary v7 runtime truth:

```text
actual relationship prerequisite
→ regional event movement/interaction
→ event resolution
→ post-resolution travel/interaction
```

When events are present, v7 primary `FinalStateHash`, `RuntimeFrames`, playthrough signature and replay
flags belong to regional event qualification. Combat, choice and relationship summaries retain their
own exact package hashes and independent route hashes.

Cases:

```text
events present:
  REGIONAL_EVENTS_CURRENT required

relationship branch should produce event but event binding invalid:
  build fails causally; do not claim ABSENT

no relationship/outcome branches:
  event summary ABSENT and valid

core-only:
  event campaign may be current
  AcceptedMechanics may remain incomplete
  no false RC readiness
```

---

# 14. History v7 and readiness

Create:

```text
unified_game_project_build_history_v7
```

Persist:

```text
GeneratedWorld
Activation
Travel
GeneratedEncounterCombat
GeneratedCampaignChoices
GeneratedCampaignRelationships
GeneratedCampaignRegionalEvents
AcceptedMechanics
Compatibility
```

Reader:

```text
v7 + exact event summary -> REGIONAL_EVENTS_CURRENT
genuine v6 -> relationships current, regional events REGIONAL_EVENTS_PENDING
v5 -> relationships pending
v4/v3/v2 -> existing behavior
```

Historical rows are never rewritten.

## 14.1 Event eligibility

For event-bearing v7 project require:

```text
Present=true
Passed=true
Status=REGIONAL_EVENTS_CURRENT
EventCount == QualifiedEventCount
per-kind counts exact
PlacementPassed=true
OverlayControlledDeltaPassed=true
RuntimeQualificationPassed=true
LockedStatePassed=true
AvailableStatePassed=true
ResolvedStatePassed=true
ExactlyOncePassed=true
ReplayPassed=true
ExactPackageSha256 == entry.PackageSha256
FinalStateHash == entry.FinalStateHash
event overlay/inventory hashes nonempty and exact
relationship summary exact/current
branch prerequisites match event inventory
```

For zero-event v7 project:

```text
Present=false
Passed=true
Status=ABSENT
EventCount=0
no relationship branch capable of producing event
```

## 14.2 Campaign readiness

For project with branch-derived event inventory:

```text
v7 REGIONAL_EVENTS_CURRENT required
```

Genuine v6:

```text
relationships RELATIONSHIPS_CURRENT
regional events REGIONAL_EVENTS_PENDING
PROJECT_NOT_READY
campaign.generated_regional_events_not_current
Projects action = Собрать и играть
```

One ordinary build upgrades project to v7 without source rewrite.

Projects rows:

```text
Столкновения
Сюжетные решения
Отношения
События мира
Игровая кампания
```

---

# 15. Regeneration, rollback and seals

Extend candidate seal:

```text
GeneratedCampaignRegionalEventSummarySha256
GeneratedCampaignRegionalEventOverlaySha256
GeneratedCampaignRegionalEventInventorySha256
GeneratedCampaignRelationshipBranchMatrixSha256
```

Event inventory rows include:

```text
RegionalEventId
EventKind
RelationshipId
RelationshipBranch
RegionId
MapId
EntityPrototypeId
MapEntityId
InteractionId
DialogueId
ResolutionFlagId
Placement X/Y
Prerequisite fingerprint
Derived reputation source fingerprint
```

Semantic validator requires:

```text
v7 for event-bearing candidate
REGIONAL_EVENTS_CURRENT
profile-neutral relationship matrix exact
event counts/kinds exact
placement reachable/safe/exact
overlay/final package hashes exact
inventory exact
combat/choices/relationships still exact
```

Tamper cases:

```text
event relationship/kind changed
prerequisite branch changed
event region/map changed
placement changed to blocked/unreachable/collision cell
dialogue/interaction/entity reference changed
resolution flag changed
Support reward derivation changed
Challenge/Refuse duplicate reputation added
branch Available/Required changed
overlay/inventory hash changed
```

Every tamper rejects before commit.
Regeneration and historical rollback rebuild events from current mechanics and candidate/historical
strict source. Never restore historical final package.

---

# 16. Save, exact continue and migration

## 16.1 Exact continue — AVAILABLE

Save after actual relationship prerequisite and before event resolution.

After recreating campaign service:

```text
Runtime Start count=0
relationship flag exact
quest/encounter prerequisite exact
event status AVAILABLE
event resolution flag absent
map/player position exact
event marker/action exact
```

## 16.2 Exact continue — RESOLVED

Save after event resolution.

Require after continue:

```text
Runtime Start count=0
event status RESOLVED
resolution flag exact
reputation exact
resolution choice unavailable
no duplicate consequence
```

## 16.3 Old v6 package rebase

Rebuild genuine v6 project to v7:

```text
old save PACKAGE_REBASE_REQUIRED
direct load rejected
preview zero-write
explicit apply CURRENT
```

## 16.4 Compatibility policy

No public save schema change.

Preserve event resolution flag only when source/target match exactly:

```text
RegionalEventId
EventKind
RelationshipId
RelationshipBranch
ActorSeedId
FactionId
RegionId
event dialogue/interaction/prototype fingerprints
prerequisite semantic fingerprint
```

Same-world package rebase:

```text
preserve compatible relationship/quest progress by existing policy
preserve compatible resolved event flag
preserve compatible reputation
```

World migration:

```text
preserve compatible relationship decision/reputation by existing policy
relationship arc follows Goal168 reset policy
preserve resolved event only when exact event semantic identity remains compatible
otherwise drop it
active event dialogue resets
transient encounter resets
```

Record per event:

```text
resolutionPreserved
statusReset
droppedReason
sourceEventFingerprint
targetEventFingerprint
```

## 16.5 No ghost events

After migration:

```text
event projection contains only target inventory events
incompatible/missing event absent
no stale marker/action/dialogue
no orphan event flag
post-migration relationship/dialogue/event/travel works
```

---

# 17. Standalone and release candidate

After real v7 all-selectable build:

```text
one cached hidden standalone smoke
retry=0
HostReused=true
HostRebuilt=false
Unity starts=0
exit=0
self-checks GREEN
payload hashes match v7 build
human facts include profile-neutral relationships and regional events
runtime frames include prerequisite, event interaction/resolution and travel
RC CURRENT
```

Do not change standalone or RC implementation except exact compile signature propagation if strictly
necessary; behavioral implementation changes remain forbidden.

Portable all-selectable:

```text
v7 regional events current
AVAILABLE/RESOLVED save current
RC CURRENT
no operational pointer required
```

Portable core-only:

```text
v7 regional events current
campaign/save current
AcceptedMechanics incomplete when profile says so
no false RC readiness
no operational pointer required
```

---

# 18. Mandatory real product matrix

Use disposable real all-selectable/core-only projects and bounded fixtures.

## 18.1 Relationship profiles

Run all six:

```text
all branches
Challenge-only / zero arc
Support+Refuse / no Challenge
Support-only
Refuse-only
no branches
```

Require exact branch counts, zero unavailable-branch Runtime calls and current history.

## 18.2 Exact effects

Run:

```text
health
stat
status
delayed status damage
mixed utility-first
ability-only
no-op-only causal failure
```

## 18.3 Event binding and placement

Require:

```text
all data-derived event kinds available in fixture
arbitrary event count
arbitrary map dimensions
unique safe reachable placement
reordered-input determinism
exact controlled delta
```

## 18.4 Runtime lifecycle

For every event kind:

```text
LOCKED
actual prerequisite
AVAILABLE
ordinary movement/interact/dialogue
RESOLVED
second interaction exactly-once proof
```

## 18.5 UI

Open campaign at:

```text
event locked
event available
event resolved
different current region
event on current map
```

Require human status and no technical values.

## 18.6 Genuine v6 upgrade

```text
v6 relationships current
events pending
campaign not ready
one build -> v7 current
source/sidecars unchanged
```

## 18.7 Regeneration/rollback

Both produce v7 current events with exact seals.

## 18.8 Save/migration

Run all section 16 routes.

## 18.9 Standalone/portable

Run section 17 exactly once.

## 18.10 No-event profile

```text
event summary ABSENT
campaign readiness valid
no false marker/tab row
```

---

# 19. Required behavioral tests

Create at least 72 Goal169 tests; at least 64 execute real behavior.

## Profile neutrality

1. all-branch relationship qualifies;
2. Challenge-only zero-arc qualifies;
3. Support+Refuse without Challenge qualifies;
4. Support-only qualifies;
5. Refuse-only qualifies;
6. no-branch relationship qualifies without Runtime;
7. unavailable Support starts zero Runtime sessions;
8. unavailable Challenge starts zero Runtime sessions;
9. unavailable Refuse starts zero Runtime sessions;
10. required branch failure remains failure;
11. Support never indexes empty arc;
12. aggregate flags are vacuous only for unavailable branches;
13. explicit branch matrix persisted;
14. old v6 all-branch row remains eligible;
15. old v6 false/partial row rejected;
16. new profile-neutral row does not require max arc > 0;
17. Support row requires positive arc length;
18. atomic rollback uses only available branch.

## Exact effects

19. health descriptor accepted;
20. stat descriptor accepted;
21. status descriptor accepted;
22. delayed status damage reaches actual victory;
23. utility success rejected;
24. mixed utility-first selects progressing descriptor;
25. ability-only route succeeds;
26. descriptor effect fingerprint exact;
27. ability definition SHA exact;
28. package reference/SHA unchanged;
29. repeated encounter state rejected;
30. no-op-only route fails causally.

## Event binding/placement

31. Support event derived from completed arc;
32. Challenge event derived from challenge branch;
33. Refuse event derived from refuse branch;
34. absent branch creates no event;
35. no-branch relationship creates no event;
36. RegionalEventId equals event dialogue ID;
37. ResolutionFlagId equals event dialogue ID;
38. event identities deterministic;
39. Support target region derived from final quest;
40. Challenge target region derived from encounter;
41. Refuse target region derived from relationship;
42. arbitrary map size supported;
43. arbitrary event count supported;
44. selected cell walkable;
45. selected cell reachable;
46. player start/gate/blocking cells excluded;
47. event placements unique;
48. reordered input deterministic;
49. insufficient safe placement fails causally;
50. event inventory exact.

## Overlay

51. only event records/map entities added;
52. existing definitions canonical-identical;
53. Manifest/GeneratedContent preserved;
54. relationship/combat/travel records preserved;
55. event references resolve exactly;
56. Support reward derivation fingerprint exact;
57. Challenge/Refuse event has no duplicate reputation effect;
58. overlay deterministic across two builds;
59. forbidden delta rejected.

## Runtime events

60. Support event locked before arc completion;
61. Support event available after full arc;
62. Support event resolves through ordinary interaction;
63. Support event reputation delta exact and once;
64. Challenge flee does not unlock event;
65. Challenge victory unlocks event;
66. Challenge event resolves with zero duplicate penalty;
67. Refuse unlocks fallout event;
68. Refuse event resolves with zero duplicate penalty;
69. resolution flag exact;
70. resolved event cannot resolve twice;
71. locked route zero mutation;
72. malformed resolution atomic;
73. event replay equivalent;
74. map movement/interact commands captured;
75. no direct state mutation;
76. state-backed event consequence.

## Projection/UI

77. locked event row;
78. available event row;
79. resolved event row;
80. current-map marker human;
81. other-region event human;
82. «События мира» tab present;
83. no raw IDs/hashes/paths;
84. 1100x720 layout fits;
85. decisions/relationships/events consistent;
86. no-event profile shows no false row.

## History/regeneration

87. v7 events current;
88. genuine v6 events pending;
89. v6 campaign not ready;
90. one build upgrades v6;
91. v5/v4/v3/v2 behavior retained;
92. event-absent v7 valid;
93. seal includes branch matrix/event summary/overlay/inventory;
94. placement/prerequisite/reward tamper rejected;
95. regeneration v7 current;
96. rollback v7 current.

## Save/migration/standalone

97. exact AVAILABLE continue;
98. exact RESOLVED continue;
99. continue Runtime Start count zero;
100. v6 save rebase required;
101. compatible event flag preserved;
102. incompatible event dropped;
103. world migration compatibility applied;
104. no ghost event/flag/marker;
105. post-migration event/travel works;
106. exactly one hidden smoke;
107. host reused/no rebuild/Unity zero;
108. RC current;
109. portable all-selectable;
110. portable core-only no false RC.

## Regressions

111. Goal168 85/85;
112. Goal167 94/94;
113. Goal166 59/59;
114. Goal165 55/55;
115. Goal164 61/61;
116. Goal163 through Goal157 focused regressions;
117. GeneratedCampaign;
118. GeneratedGameplaySave;
119. RuntimeSimulator;
120. UnifiedGameProjectWorkspace/coordinator;
121. standalone filters;
122. Goal142/Goal148/source-sidecar immutability.

No source-string-only assertion counts as behavioral proof.

---

# 20. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal169"
# require >=72 total / >=64 behavioral

dotnet test ... --filter "FullyQualifiedName~Goal169"
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

Then execute mandatory real matrix and exactly one hidden smoke.

Do not run:

```text
full suite
85-case historical closure
all-ProductSmoke
Unity host build
more than one hidden smoke
corrective smoke retry
visible app/player launch
```

A zero-match filter is failure.

---

# 21. Evidence

Create exactly 15 files in each mirrored root:

```text
goal169-dashboard.json
architecture-review.json
goal168-independent-audit-finding.json
relationship-profile-matrix-proof.json
exact-effect-matrix-proof.json
regional-event-binding-placement-proof.json
regional-event-runtime-routes-proof.json
regional-event-ui-proof.json
history-regeneration-proof.json
save-exact-continue-proof.json
regional-event-migration-proof.json
standalone-rc-portability-proof.json
regression-immutability-proof.json
artifact-scope-proof.json
goal169-report.md
```

Roots:

```text
.llmgc/procedural/goal-169-profile-neutral-relationships-and-reactive-regional-events/
.llmgc/exports/goal-169-profile-neutral-relationships-and-reactive-regional-events/
```

Twins must be byte-identical. Every field comes from typed captures, not unmeasured constants.

## 21.1 Dashboard fields

```text
status
candidateStatus
goal169TestsDiscovered
goal169BehavioralTestsPassed

goal168AuditResultRecorded
goal168P1AClosed
goal168P1BClosed
goal168SaveTruthDebtClosed

relationshipProfileCount
allBranchesProfilePassed
challengeOnlyZeroArcPassed
supportRefuseNoChallengePassed
supportOnlyPassed
refuseOnlyPassed
noBranchesPassed
unavailableBranchRuntimeStartCount
branchMatrixSha256
legacyV6AllBranchCompatible
legacyV6PartialRejected

healthEffectPassed
statEffectPassed
statusEffectPassed
delayedStatusDamagePassed
utilityNoOpRejected
abilityOnlyEffectNeutralPassed
exactEffectPackageShaUnchanged

saveContinuationFactsEvaluationStatus
saveContinuationFactsPassed

regionalEventCount
qualifiedRegionalEventCount
supportEventCount
challengeEventCount
refuseEventCount
eventIdentityExact
eventPlacementPassed
eventPlacementReachable
eventPlacementUnique
eventPlacementDeterministic
eventOverlayControlledDeltaPassed
existingPackageRecordsPreserved

lockedRoutesPassed
availableRoutesPassed
resolvedRoutesPassed
eventExactlyOncePassed
supportEventReputationDelta
challengeEventDuplicateReputationDelta
refuseEventDuplicateReputationDelta
eventReplayEquivalent
eventFailureAtomicRollbackPassed

regionalEventProjectionPassed
regionalEventPrimaryUiNoRawIds
regionalEventMapMarkersPassed
decisionRelationshipEventConsistencyPassed

historySchemaVersion
v7RegionalEventsCurrent
v6RegionalEventsPending
v6CampaignNotReady
oldProjectBuildInvocationCount
oldProjectUpgradedWithoutSourceRewrite
regionalEventPrimaryFinalStatePassed
combatChoiceRelationshipSummariesPreserved
regenerationRegionalEventsCurrent
rollbackRegionalEventsCurrent
regionalEventSealTamperRejected

exactAvailableEventContinuePassed
exactResolvedEventContinuePassed
exactContinueRuntimeStartCount
oldV6SaveRebaseRequired
compatibleEventResolutionPreserved
incompatibleEventDropped
ghostEventAbsent
postMigrationEventTravelPassed

hostCacheKey
hostReused
hostRebuilt
unityEditorProcessStartCount
hiddenSmokeInvocationCount
hiddenSmokePassed
correctiveSmokeRetryCount
actualPayloadRegionalEventFactsPassed
releaseCandidateRecordCurrent
portableAllSelectablePassed
portableCoreOnlyPassed
coreOnlyNoFalseRcReady

goal168RegressionPassed
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

goal169Accepted=false
goal169ManualReviewRequired=false
goal169IndependentAuditRequired=true
```

No required GREEN value may be null, `PARTIAL` or `NOT_EXECUTED`, except intentionally honest:

```text
saveContinuationFactsEvaluationStatus=NOT_EVALUATED_AT_BUILD
saveContinuationFactsPassed=false
```

Real save matrix fields must still be GREEN.

---

# 22. State and docs

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
docs/manual-acceptance/goal168-choice-driven-relationships-multi-quest-arcs.md
```

Create:

```text
docs/manual-acceptance/goal169-profile-neutral-relationships-reactive-regional-events.md
```

Required GREEN state:

```text
goal168IndependentAuditResult=BLOCKED_AT_BBFD46A2
goal168P1A=closed_by_goal169
goal168P1B=closed_by_goal169
goal168SaveTruthDebt=closed_by_goal169

goal168ImplementationStatus=GREEN
goal168CandidateStatus=BLOCKED_AT_BBFD46A2
goal168Accepted=false
goal168IndependentAuditRequired=false

goal169ImplementationStatus=GREEN
goal169CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal169Accepted=false
goal169AcceptedByHuman=false
goal169AcceptedByCodex=false
goal169ManualReviewRequired=false
goal169ManualGateReady=false
goal169IndependentAuditRequired=true

goal169RelationshipProfilesPassed=true
goal169ExactEffectMatrixPassed=true
goal169RegionalEventsPassed=true
goal169RegionalEventSaveContinuePassed=true
goal169RegionalEventMigrationPassed=true
goal169HostReused=true
goal169HostRebuilt=false
goal169UnityEditorProcessStartCount=0
goal169HiddenSmokeInvocationCount=1
goal169PortableAllSelectablePassed=true
goal169PortableCoreOnlyPassed=true
goal169ArtifactScopeViolationCount=0

nextAction=independent_goal169_audit_and_plan_next_visible_campaign_system
```

No human gate.

---

# 23. Text integrity

Scan changed/task/evidence/docs:

```text
valid UTF-8
no NUL
no forbidden C0 except CR/LF/TAB
no mojibake markers
no escaped Cyrillic where forbidden
no absolute disposable paths in evidence
```

---

# 24. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal169-regional-events.ps1
.devflow/scripts/run-goal169-regional-events.cmd

src/LLMGameCreator.Application/Generation/Procedural/GeneratedCampaignRelationshipModels.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedCampaignRelationshipQualificationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedCampaignRegionalEventModels.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedCampaignRegionalEventBindingService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedCampaignRegionalEventOverlayService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedCampaignRegionalEventQualificationService.cs
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
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignRegionalEventProjectionService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignProjectionService.cs
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
src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignMapControl.cs

tests/LLMGameCreator.Tests/Application/Goal169/**
tests/LLMGameCreator.Tests/Application/Goal168/**
tests/LLMGameCreator.Tests/Application/Goal167/Goal167BranchRuntimeQualificationTests.cs
tests/LLMGameCreator.Tests/Application/Goal167/Goal167SaveMigrationTests.cs
tests/LLMGameCreator.Tests/Application/Goal166/Goal166MixedAbilityQualificationTests.cs
tests/LLMGameCreator.Tests/Application/Goal166/Goal166RealDefeatRetryTests.cs
tests/LLMGameCreator.Tests/Application/Goal164/Goal164BuildHistoryCampaignCurrentTests.cs
tests/LLMGameCreator.Tests/Application/Goal162/Goal162CampaignTruthProjectionTests.cs
tests/LLMGameCreator.Tests/Application/Goal162/Goal162CampaignTravelTests.cs
tests/LLMGameCreator.Tests/Application/Goal162/Goal162WinFormsWorkspaceTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/manual-acceptance/goal168-choice-driven-relationships-multi-quest-arcs.md
docs/manual-acceptance/goal169-profile-neutral-relationships-reactive-regional-events.md

docs/agent-tasks/goal-169-profile-neutral-relationships-and-reactive-regional-events/
.llmgc/procedural/goal-169-profile-neutral-relationships-and-reactive-regional-events/
.llmgc/exports/goal-169-profile-neutral-relationships-and-reactive-regional-events/
```

One exact additional existing campaign/history test/model path may be added only after a concrete
compile/test failure with the reason recorded in architecture review and artifact evidence.

Forbidden:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Domain/**
catalogs/feature-modules/**
unity/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
ProceduralGameKernel*
GeneratedPackageMvp*
GeneratedProjectOverlay*
GeneratedWorldEncounterCombatOverlay*
ProjectStandaloneBuild implementation
GameProjectReleaseCandidate implementation
```

---

# 25. Command budget

```text
read-first/architecture: 12 minutes
branch/effect P1 closure: 18 minutes
event binding/placement/overlay: 24 minutes
event Runtime qualification: 24 minutes
projection/UI/campaign integration: 18 minutes
history/regeneration/save/migration: 24 minutes
behavioral tests: 34 minutes
real matrix/smoke/portable: 18 minutes
regressions/evidence/docs/scope: 20 minutes
target wall clock: 150 minutes
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
P0/P1 fixed inside Goal169
P2/P3 debt only
do not stop at compile success
do not defer evidence/docs/scope
```

---

# 26. Publication

Create exactly one final commit:

```text
GREEN Goal 169 profile neutral relationships and reactive regional events
```

or honest `BLOCKED`/`FAILED` equivalent.

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
Goal168/Goal169 accepted=false
no human gate
```

---

# 27. GREEN criteria

```text
Goal168 independent-audit blockers recorded and closed
profile-neutral branch matrix exact
all six mandatory relationship profiles GREEN
zero Runtime execution for unavailable branches
legacy v6 all-branch compatibility without false reinterpretation
honest NOT_EVALUATED_AT_BUILD save truth
health/stat/status/delayed exact effects accepted
utility/no-op rejected
data-derived event kinds/counts/identity
safe reachable deterministic event placement
controlled additive event overlay
ordinary Runtime event interaction
LOCKED/AVAILABLE/RESOLVED
exactly-once resolution
state-backed event UI/map/consequences
v7 REGIONAL_EVENTS_CURRENT
v6 pending/campaign not ready
regeneration/rollback sealed
exact AVAILABLE and RESOLVED continue
explicit migration preserve/drop
no ghost event
one cached hidden smoke
RC CURRENT
portable all-selectable/core-only
Goal169 >=72 discovered / >=64 behavioral / all pass
required regressions GREEN
source/sidecars immutable
15+15 typed evidence
text integrity GREEN
artifact scope 0
one final commit pushed
```

---

# 28. Final report

Return `GREEN`, `BLOCKED` or `FAILED` and include:

- model/reasoning;
- required base and initial worktree;
- Goal168 independent-audit intake;
- relationship Available/Required/Passed matrix and six profile results;
- unavailable branch Runtime counts;
- legacy v6 compatibility;
- removed health-only restriction and exact effect matrix;
- honest SaveContinuationFacts status;
- regional event counts/kinds/identity;
- safe placement/reachability/determinism;
- controlled overlay and preserved records;
- LOCKED/AVAILABLE/RESOLVED routes;
- ordinary movement/interaction/dialogue proof;
- exactly-once and reputation results;
- regional event UI/map/projection;
- v7/v6/v5/v4/v3/v2 history/readiness;
- regeneration/rollback seals;
- save/continue and migration results;
- host/Unity/smoke/payload/RC;
- portable all-selectable/core-only;
- tests/regressions/source immutability;
- evidence/text/artifact scope;
- state/no-human-gate;
- final SHA, commit message and push;
- `HEAD == origin/main`;
- clean worktree;
- explicit confirmation that Codex committed and pushed for any final status.
