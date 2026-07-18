# Goal 167 — Generated Campaign Choice Branching & Persistent Consequences

## Identity

- Task ID: `goal-167-generated-campaign-choice-branching-and-persistent-consequences`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `d265e8094c59908989409e1b70f21f23d2fd579b`
- Required base message: `goal166 exact qualified combat actions and recovery`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: this is the next major visible product vertical slice. Existing Runtime already supports
dialogue conditions, requirements, costs, effects, rewards, flags, node transitions, quest
operations, transactions and encounter starts. Generated dialogues currently expose only one
`Continue` choice. Goal167 adds a deterministic build-time narrative choice overlay, real branch
qualification, a player-facing decision journal, exact save/continue/migration truth, regeneration
and standalone/RC qualification without changing Runtime or public package schemas.

## Pre-approval and publication

- The owner approved the complete plan by launching this task.
- Do not ask for another confirmation because more than ten files are involved.
- Produce a concise internal plan and proceed.
- Do not request manual testing.
- Own all P0/P1 defects reproduced by the Goal167 matrix.
- Record P2/P3 debt without creating Goal167A.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- On BLOCKED/FAILED, commit and push the honest state; never leave publication to the user.
- No intermediate commits.

## Initial worktree

After unpacking, only these untracked files are allowed:

```text
docs/agent-tasks/goal-167-generated-campaign-choice-branching-and-persistent-consequences/GOAL.md
docs/agent-tasks/goal-167-generated-campaign-choice-branching-and-persistent-consequences/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-167-generated-campaign-choice-branching-and-persistent-consequences/README.md
```

Require:

```text
HEAD == origin/main == d265e8094c59908989409e1b70f21f23d2fd579b
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

Use the existing cached host only. No Unity source change.

## Goal166 independent-audit result

Record:

```text
goal166IndependentAuditResult=GREEN_ACCEPTABLE_CANDIDATE_AT_D265E809
goal166IndependentAuditPassed=true
goal166IndependentAuditRequired=false
```

No product P0/P1 remains.

Audited truths:

```text
qualified action catalog stores exact BasicAttack/package-ability identities and canonical hashes
utility/no-op abilities are excluded from the victory catalog
per-encounter qualification replays exact descriptors and observed effects
mixed utility-first/damage-later selects the progressing descriptor
route/history/seal compatibility remains profile-neutral
production GeneratedCampaignSessionService reaches real Runtime DEFEATED
Retry uses exact pre-encounter checkpoint with Runtime Start delta zero
exact save load, New Game and stale checkpoint recovery are causal
defeat without checkpoint keeps Continue/New Game available
physical core-only portable copy has no operational pointer or false RC readiness
```

Goal166 remains `accepted=false`; no human gate.

### P2 evidence debt

Two focused determinism tests compare an already cached value with itself, and the committed dashboard
contains some aggregate/static fields rather than one typed real-route capture. Production
determinism and recovery are independently supported by code and real service tests, so this is P2
evidence hygiene. Goal167 must generate every dashboard value from typed captures and execute
determinism against independently rebuilt objects.

## Product problem

Current generated dialogues are minimal:

```text
one start node
one sentence
one choice: Continue
close dialogue
```

The player cannot make a lasting decision, lock an alternative, affect a faction, start a generated
encounter through dialogue, or return later to see a branch-specific response.

## Product outcome

For each generated actor with available package relationships, the campaign provides data-derived
branches such as:

```text
Support the actor/faction
Challenge the actor
Refuse involvement
```

Only branches supported by actual actor/faction/quest/encounter data are created.

A complete route:

```text
open generated actor dialogue
→ see multiple human choices with consequence previews
→ choose Support
→ branch flag + reputation consequence
→ alternatives lock
→ reopen dialogue and see Support follow-up
→ complete associated generated quest
→ reopen and see completed follow-up
→ save and exact continue
→ branch and decision journal remain exact
→ regenerate world
→ explicit migration preserves only branch flags whose exact generated dialogue remains compatible
→ incompatible branch flags are dropped with reasons
→ choose Challenge on a fresh branch
→ dialogue starts exact generated encounter
→ combat/recovery/turn-in remain functional
→ one cached standalone smoke
→ RC CURRENT
→ portable all-selectable/core-only
```

No raw IDs/hashes/paths are needed in primary UI.

## Non-goals

Do not change:

```text
Runtime or Runtime.Abstractions
GamePackage/domain public schema
FeatureModule catalog
ProceduralGamePlan schema
GeneratedPackageMvp source/sidecars
generated world topology
generated combat overlay
Unity source/host
cloud or multiplayer state
```

Existing Runtime semantics are authoritative. Goal167 composes them at build time and projects them to
the campaign UI.


## Mandatory architecture review

Read at most 18 primary files:

```text
GeneratedPackageMvpService.cs
ProceduralGameKernelModels.cs
GeneratedProjectOverlayService.cs
GeneratedWorldEncounterCombatOverlayService.cs
GameProjectBuildAndQualificationService.cs
GameProjectBuildHistoryReader.cs
GameProjectWorkspaceModels.cs
GameProjectSeedRegenerationCandidateSealService.cs
GameProjectSeedRegenerationCommitValidator.cs
GeneratedCampaignSessionTruthService.cs
GeneratedCampaignSessionService.cs
GeneratedCampaignActionPlanner.cs
GeneratedCampaignProjectionService.cs
GeneratedCampaignConsequenceProjector.cs
GeneratedGameplayDefinitionFingerprintService.cs
GeneratedGameplaySaveMigrationService.cs
DialogueRuntimeService.cs
DialogueDefinitions.cs
```

Before production edits write:

```text
.llmgc/procedural/goal-167-generated-campaign-choice-branching-and-persistent-consequences/architecture-review.json
```

Required sections:

```text
goal166IndependentAudit
existingDialogueRuntimeAuthority
generatedActorDialogueBinding
branchKindsAndDataSources
branchFlagIdentity
dialogueOverlayControlledDelta
buildPipelineOrder
branchRuntimeQualification
choiceAvailabilityPreview
decisionLedger
historyVersioning
campaignReadiness
saveExactContinue
migrationPreserveDrop
regenerationRollback
standaloneRcPortable
failureMatrix
nonGoals
```

Every section names exact types, inputs, outputs, hashes and behavioral tests.

## A. Generated campaign choice models

Create:

```text
GeneratedCampaignChoiceModels.cs
GeneratedCampaignChoiceBindingService.cs
GeneratedCampaignChoiceOverlayService.cs
GameProjectGeneratedCampaignChoiceQualificationService.cs
```

Core types:

```text
GeneratedCampaignBranchKind
  SUPPORT
  CHALLENGE
  REFUSE

GeneratedCampaignChoiceBinding
GeneratedCampaignChoiceBranch
GeneratedCampaignChoiceOverlayDocument
GameProjectGeneratedCampaignChoiceSummary
GeneratedCampaignChoiceRuntimeFrame
GeneratedCampaignChoiceHumanFact
```

No content count is fixed. Branch kinds are mechanics; actual branch rows are created only when the
package has exact supporting relationships.

## B. Exact generated dialogue binding

Input:

```text
strict SeededGeneratedProjectSourceValidationResult
exact pre-choice Lane B package
```

Bind through exact provenance:

```text
RegeneratedPlan.ActorSeeds.ActorSeedId
→ GeneratedProjectOverlayDocument generated record at game.dialogues
→ DialogueDefinition.Metadata["sourceActorSeedId"]
→ exactly one DialogueDefinition
```

Also resolve exact:

```text
actor faction from ProceduralActorSeed.FactionId
actor region from ProceduralActorSeed.RegionId
package faction definition
generated actor entity
generated interaction that opens the dialogue
same-region generated quests
same-region generated encounters
encounters containing the actor seed when available
```

Never infer from titles or string prefixes alone.

Diagnostics:

```text
generated_choice.source_invalid
generated_choice.dialogue_mapping_missing
generated_choice.dialogue_mapping_duplicate
generated_choice.actor_entity_missing
generated_choice.interaction_missing
generated_choice.faction_missing
generated_choice.relationship_ambiguous
```

A generated actor with no supported branch relationship remains with its original minimal dialogue
and is reported as `NO_BRANCH_RELATIONSHIP`, not as a build failure. Projects containing at least one
branchable generated dialogue must qualify all branchable dialogues.

## C. Stable branch flag identity

Use the exact generated `DialogueDefinition.Id` as the Runtime flag ID:

```text
Flag.Id = exact dialogue ID
Flag.Value = SUPPORT | CHALLENGE | REFUSE
```

Reason:

```text
the flag ID is already a generated dialogue definition reference;
save capture fingerprints it;
same-definition exact continue preserves it;
migration preserves it only when the exact dialogue definition remains compatible;
changed/missing generated dialogue causes the existing migration policy to drop it.
```

Do not invent a separate untracked actor-flag namespace.

Every initial branch choice requires:

```text
flag_equals(dialogue.Id, "")
```

Every follow-up choice requires:

```text
flag_equals(dialogue.Id, branch value)
```

Only one initial branch can be committed.

## D. Data-derived branches

### D1. Support

Create only when all exist uniquely:

```text
actor faction
same-region generated quest whose SourceFactionId equals actor faction
quest reward reputation output for that faction with nonzero amount
```

Choice:

```text
human title names actor/faction/quest
effect sets flag(dialogue.Id)=SUPPORT
reputation output amount = absolute value of the exact quest reputation reward amount
close dialogue
```

Do not start the quest when it is already AutoStart/active.

Follow-ups:

```text
SUPPORT + quest active:
  human progress response

SUPPORT + quest completed:
  human completed response
```

Use existing `quest_state` requirements.

### D2. Challenge

Create only when an exact same-region generated encounter exists, preferring one whose strict plan
`ActorSeedIds` contains this actor; otherwise choose the canonical same-region encounter.

Choice:

```text
effect sets flag(dialogue.Id)=CHALLENGE
StartEncounterId = exact package encounter ID
close dialogue
```

Follow-up requires `flag_equals(..., CHALLENGE)` and reports that the challenge was chosen.

### D3. Refuse

Create only when the actor faction and a nonzero exact generated quest reputation reward are
available.

Choice:

```text
effect sets flag(dialogue.Id)=REFUSE
reputation output amount = negative absolute value of the exact quest reputation reward amount
close dialogue
```

Follow-up requires `flag_equals(..., REFUSE)`.

### D4. No fixed quantities

Do not assume:

```text
two actors
three branches
one quest
one encounter
one faction
specific reputation amount
```

Process every branchable generated actor and every supported relationship according to deterministic
selection rules.

## E. Dialogue overlay

The overlay clones the exact pre-choice package and modifies only exact bound generated
`DialogueDefinition` records.

Allowed changes:

```text
DialogueDefinition.Nodes
DialogueDefinition.Metadata entries with prefix generatedChoice
DialogueDefinition.Tags addition generated_choice_branching
```

Preserve exactly:

```text
dialogue ID/title/start node/background
sourceActorSeedId/sourceRegionId metadata
all non-generated dialogues
all maps/entities/interactions
all quests/encounters/factions/items/abilities
GeneratedContent
Manifest and catalogs
```

Do not modify generated source sidecars or GeneratedPackageMvp output.

Document:

```text
SchemaVersion=generated_campaign_choice_overlay_v1
SourcePackageSha256
OutputPackageSha256
GeneratedDialogueCount
BranchableDialogueCount
QualifiedBranchCount
Bindings[]
DialogueFingerprintsBefore[]
DialogueFingerprintsAfter[]
AllowedFieldPaths[]
DefinitionCollectionCountsBefore/After
Passed
Diagnostics[]
```

Controlled-delta validation rejects every out-of-scope difference.

Determinism:

```text
same strict source + same package → byte-identical overlay/package
reordered input collections → same canonical output
```


## F. Runtime branch qualification

Use actual `IUnifiedGameRuntimeService` against the exact choice-overlay package.

For every branchable dialogue:

1. Start exact package.
2. Open exact generated dialogue through the ordinary dialogue command.
3. Read Runtime-provided available `choiceIds`.
4. Prove every initial branch appears and no follow-up appears before selection.
5. Execute each branch in a fresh session.
6. Verify atomic state/event consequences.
7. Reopen the dialogue.
8. Verify alternatives are locked and only branch-specific follow-ups are available.
9. Replay independently and require equivalent branch state/event hashes.

### F1. Support proof

```text
flag(dialogue.Id)=SUPPORT
exact faction reputation delta
no encounter starts
alternatives locked
active/completed quest follow-up changes with actual quest state
```

### F2. Challenge proof

```text
flag(dialogue.Id)=CHALLENGE
exact generated encounter active
dialogue closes
combat contract/package remains exact
alternatives locked
```

### F3. Refuse proof

```text
flag(dialogue.Id)=REFUSE
exact negative faction reputation delta
no quest/encounter mutation
alternatives locked
```

### F4. Atomic rollback

For a deliberately invalid branch fixture:

```text
missing faction/encounter/quest reference
or failing cost/effect
```

Runtime must return failure with:

```text
flag unchanged
reputation unchanged
quest/encounter unchanged
dialogue state truthful
```

No direct state mutation in qualification.

## G. Choice availability preview

Create:

```text
GeneratedCampaignDialogueChoicePreviewService
GeneratedCampaignDialogueChoicePreview
```

Do not duplicate `RequirementEvaluator`.

For an open dialogue:

```text
clone the exact UnifiedRuntimeSession
execute each current-node choice through existing Runtime on the clone
record Passed/Diagnostics/observed consequences
discard clone
```

Properties:

```text
original session byte-identical
package SHA byte-identical
available choices match Runtime-provided choiceIds
conditions/costs are respected
human disabled reason derived from Runtime diagnostics
```

Exact continue of an open dialogue can therefore rebuild availability without requiring prior events.

## H. Player-facing choice projection

Add Application models:

```text
GeneratedCampaignChoiceOption
GeneratedCampaignChoicePreview
GeneratedCampaignDecision
GeneratedCampaignDecisionJournal
GeneratedCampaignDecisionStatus
```

Choice option fields:

```text
Title
Description
ActorTitle
FactionTitle
QuestTitle optional
EncounterTitle optional
ConsequencePreview[]
Enabled
DisabledReason
BranchKind
Primary
```

Technical choice/dialogue IDs remain only in Technical Details.

### H1. Campaign planner/session

`GeneratedCampaignActionPlanner` uses the preview service for dialogue mode.

Show only current-node choices and truthful availability.

On choice execution:

```text
recapture project truth
snapshot before
execute exact existing ChooseDialogueOption
project flag/reputation/quest/encounter changes
update decision journal
return truthful dialogue or encounter mode
```

No raw package ID is typed by the player.

### H2. Consequences

Add consequence kinds:

```text
Decision
BranchLocked
BranchFollowUp
```

Every consequence is backed by:

```text
flag state delta
faction reputation delta
quest state delta
encounter start event
dialogue node/choice event
```

No consequence is inferred only from metadata.

### H3. Decision journal

Add right-side tab/card:

```text
Решения
```

Rows:

```text
actor
chosen branch
human consequence
related faction/quest/encounter
current follow-up status
alternative branches locked
```

Journal is projected from current Runtime flags plus exact overlay summary/package definitions. It is
not a second mutable store.

## I. Build pipeline

Add choice overlay after generated travel and before generated combat overlay:

```text
strict source
→ Lane A qualification
→ generated start/travel package
→ generated choice overlay
→ generated combat overlay
→ generated combat qualification
→ generated choice qualification against the final package
→ final validation/history
```

Reason:

```text
combat overlay then remains the final package-producing overlay;
combat summary ExactPackageSha256 remains equal to primary package SHA;
choice dialogue fingerprints must remain unchanged across the later combat overlay.
```

Lane A remains byte/hash identical.

### I1. Build result

Add:

```text
GameProjectGeneratedCampaignChoiceSummary
```

Fields:

```text
Present
Passed
Status
OverlaySchemaVersion
SourcePackageSha256
ChoiceOverlayPackageSha256
FinalPackageSha256
GeneratedDialogueCount
BranchableDialogueCount
QualifiedDialogueCount
SupportBranchCount
ChallengeBranchCount
RefuseBranchCount
BranchFlagIds[]
ChoiceOverlaySha256
RuntimeQualificationPassed
ExclusiveBranchingPassed
FollowUpPassed
AtomicRollbackPassed
ReplayPassed
FinalStateHash
RuntimeFrames
HumanReviewFacts
TechnicalDetails
Diagnostics
Overlay
```

Statuses:

```text
CHOICE_CURRENT
CHOICES_PENDING
ABSENT
INVALID
```

Final package and build primary hashes belong to the combat-overlay package.

Primary final Runtime frames/hash for a generated project become the deterministic full branch route:

```text
open dialogue
→ choose branch
→ reopen/follow-up
→ challenge branch combat start
→ existing generated combat/travel route
```

Do not weaken existing combat summary truth.

## J. Build history v5

Create:

```text
unified_game_project_build_history_v5
```

Persist:

```text
GeneratedWorld
GeneratedWorldActivation
GeneratedRegionTravel
GeneratedEncounterCombat
GeneratedCampaignChoices
AcceptedMechanics
Compatibility
```

Reader:

```text
v5 + exact choice summary/current hashes → campaign choice current
genuine v4 generated project → GeneratedEncounterCombat CAMPAIGN_CURRENT + choices CHOICES_PENDING
v3 → COMBAT_PENDING / choices ABSENT
v2 → START_CURRENT
legacy → existing behavior
```

Historical files are never rewritten.

### J1. Campaign readiness

For projects with branchable generated dialogues require:

```text
selected current v5 history
GeneratedEncounterCombat CAMPAIGN_CURRENT
GeneratedCampaignChoices CHOICE_CURRENT
exact package/composition/final/authoring truth
```

Old v4 project:

```text
PROJECT_NOT_READY
campaign.generated_choices_not_current
Projects action = Собрать и играть
```

Projects UI rows:

```text
Боевая кампания      готова
Сюжетные решения     готовы / требуется сборка / ошибка
```


## K. Save, continue and migration

### K1. Exact save/continue

After choosing Support:

```text
save CURRENT
clear/recreate campaign service
Continue exact with Runtime Start count zero
branch flag exact
reputation exact
decision journal exact
alternative branches remain locked
follow-up choices exact
```

A save made before the choice restores the unchosen state and all initial choices.

### K2. Flag portability

Because branch flag ID equals exact generated dialogue ID:

```text
same canonical dialogue definition → flag can remain portable
changed/missing generated dialogue → existing fingerprint migration drops the flag
```

Do not modify migration implementation unless a real test proves the existing fingerprint path is
incorrect. Any required change must be narrow, typed and separately justified in evidence.

### K3. Package rebase

Rebuilding an old Goal166 project with the deterministic choice overlay changes package fingerprints.

Require:

```text
old save → PACKAGE_REBASE_REQUIRED
direct Continue rejected
explicit preview/apply
branchless old save migrates CURRENT
```

### K4. World migration

After a branch decision and world regeneration:

```text
source save tree unchanged
preview lists branch flag preserve/drop truth
compatible exact dialogue branch preserved
incompatible/missing generated dialogue branch dropped with causal reason
active dialogue reset
decision journal after migration reflects only retained flags
post-migration dialogue choice/combat/travel work
```

No ghost branch may survive for a missing generated dialogue.

## L. Regeneration and historical rollback

Ordinary regeneration and history rollback use the complete build pipeline and produce v5 choice
current candidates.

Candidate seal adds canonical hashes:

```text
GeneratedCampaignChoiceSummarySha256
GeneratedCampaignChoiceOverlaySha256
GeneratedCampaignChoiceFlagInventorySha256
```

Semantic validator requires:

```text
branchable dialogue count exact
all branchable dialogues qualified
choice status CHOICE_CURRENT
overlay/final package hashes exact
flag inventory exact
combat summary still exact
```

Tamper cases:

```text
choice branch removed
flag ID/value changed
choice target quest/encounter changed
reputation output amount changed
follow-up requirement changed
overlay hash changed
```

All reject before commit.

Historical rollback rebuilds choices from current mechanics plus historical generated source. It does
not restore historical final packages.

## M. Standalone and RC

After the v5 all-selectable build run exactly one cached hidden standalone build.

Require:

```text
HostReused=true
HostRebuilt=false
Unity Editor starts=0
hidden smoke exactly 1
corrective retry 0
payload self-check GREEN
actual payload package/composition/final hashes match v5 build
human facts include branching, exclusivity and persistence
runtime frames include dialogue open/choice/follow-up plus combat/travel
RC CURRENT
```

No standalone implementation change is expected.

### M1. Portable all-selectable

Copy project to a new path without operational output:

```text
v5 choice current
branch decision save CURRENT
decision journal restored
RC CURRENT
```

### M2. Portable core-only

```text
v5 choice current
branch/save/campaign current
AcceptedMechanics incomplete
no false RC CURRENT/READY/PENDING
no operational pointer
```

## N. Real automated product matrix

Use real disposable generated projects.

### N1. Old project upgrade

```text
Goal166 v4 project reopens combat current / choices pending
Projects shows Собрать и играть
one build creates v5 CHOICE_CURRENT
generation source/sidecars byte-identical
```

### N2. Initial branch surface

Navigate to every branchable generated actor.

Require:

```text
multiple choices when multiple relationships exist
human actor/faction/quest/encounter titles
consequence preview
no raw IDs/hashes/paths
Runtime preview availability equals displayed enabled choices
```

### N3. Support route

```text
choose Support through player-facing action
flag exact
positive data-derived reputation delta
alternatives locked
reopen active-quest follow-up
complete associated quest through existing combat/turn-in
reopen completed follow-up
decision journal updated
```

### N4. Challenge route

Fresh session/project copy:

```text
choose Challenge
flag exact
exact generated encounter starts
dialogue closes
qualified tactical combat actions
defeat/retry or victory path remains functional
alternatives locked
```

### N5. Refuse route

Fresh session/project copy:

```text
choose Refuse
flag exact
negative data-derived reputation delta
no encounter/quest mutation
alternatives locked
```

### N6. Failed branch atomicity

Use an isolated invalid fixture:

```text
choice command fails
flag/reputation/quest/encounter/session hashes unchanged
human causal failure
```

### N7. Save/continue

Run K1.

### N8. Regeneration/migration

Run K4 and prove no ghost flag.

### N9. Core-only/legacy

```text
core-only choice route and exact continue
no false RC readiness
legacy/template campaign behavior unchanged
Runtime Simulator unchanged
```

## O. Required behavioral tests

Create at least 62 Goal167 tests; at least 54 behavioral.

### Binding/overlay

1. actor-to-dialogue exact provenance;
2. missing/duplicate dialogue rejected;
3. actor entity/interaction exact;
4. support relationship data-derived;
5. challenge relationship data-derived;
6. refuse relationship data-derived;
7. actor without relationships remains minimal;
8. flag ID equals exact dialogue ID;
9. initial requirements require empty flag;
10. follow-ups require exact branch value;
11. non-generated dialogues byte-identical;
12. dialogue identity/source metadata preserved;
13. non-dialogue package collections byte-identical;
14. definition counts unchanged;
15. overlay deterministic from independent rebuilds;
16. reordered input canonical output equal;
17. forbidden delta rejected.

### Runtime qualification

18. initial available IDs exact;
19. follow-ups unavailable initially;
20. support branch atomic success;
21. support reputation amount data-derived;
22. support alternatives lock;
23. support active-quest follow-up;
24. support completed-quest follow-up;
25. challenge starts exact encounter;
26. challenge closes dialogue;
27. refuse negative reputation data-derived;
28. refuse does not mutate quest/encounter;
29. invalid branch rolls back exact;
30. independent replay equivalent.

### Preview/UI/journal

31. preview uses cloned session;
32. preview leaves original session byte-identical;
33. preview package SHA unchanged;
34. preview availability matches Runtime choiceIds;
35. disabled requirements humanized;
36. cost/effect/quest/encounter previews human;
37. decision consequence exact;
38. branch locked consequence exact;
39. decision journal projected from flags;
40. journal alternatives locked;
41. primary UI no IDs/hashes/paths/codes;
42. 1100x720 choices/journal unclipped.

### History/build/regeneration

43. choice overlay before combat overlay;
44. combat summary exact final package preserved;
45. v5 history choice current;
46. genuine v4 choices pending;
47. v3/v2 compatibility;
48. old project rebuild upgrades without source rewrite;
49. campaign readiness requires choice current;
50. candidate seal includes choice hashes;
51. branch tamper rejected;
52. regeneration v5 choice current;
53. rollback v5 choice current.

### Save/migration/standalone

54. exact save preserves Support flag/journal;
55. pre-choice save restores unchosen state;
56. old save package rebase required;
57. explicit rebase migration current;
58. compatible branch flag preserved;
59. incompatible/missing dialogue flag dropped;
60. no ghost decision journal row;
61. post-migration dialogue/combat/travel;
62. exactly one cached hidden smoke;
63. RC CURRENT;
64. portable all-selectable;
65. portable core-only without false RC.

### Regressions

66. Goal166 59/59 GREEN;
67. Goal165 55/55 GREEN;
68. Goal164 61/61 GREEN;
69. Goal163/162/161 regressions GREEN;
70. Runtime Simulator unchanged;
71. Goal142/Goal148/source sidecars byte-identical.

No source-string-only assertion counts as behavioral proof.


## P. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal167"
# require >=62 total / >=54 behavioral

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

Then run all real matrices in section N and the one authorized cached smoke.

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

## Q. Evidence

Create exactly 15 files in each mirrored root:

```text
goal167-dashboard.json
architecture-review.json
goal166-independent-audit-intake.json
generated-choice-binding-proof.json
choice-overlay-controlled-delta-proof.json
branch-runtime-qualification-proof.json
choice-preview-ui-proof.json
decision-journal-proof.json
history-regeneration-proof.json
save-exact-continue-proof.json
migration-branch-portability-proof.json
standalone-rc-portability-proof.json
regression-immutability-proof.json
artifact-scope-proof.json
goal167-report.md
```

Roots:

```text
.llmgc/procedural/goal-167-generated-campaign-choice-branching-and-persistent-consequences/
.llmgc/exports/goal-167-generated-campaign-choice-branching-and-persistent-consequences/
```

Twins byte-identical.

Every evidence value must come from typed test/real-route captures. Do not write unmeasured constants
such as `passed=true` merely because a filter completed.

### Dashboard fields

```text
status
candidateStatus
goal167TestsDiscovered
goal167BehavioralTestsPassed

goal166IndependentAuditPassed
goal166P2EvidenceDebtRecorded

generatedDialogueCount
branchableDialogueCount
qualifiedDialogueCount
supportBranchCount
challengeBranchCount
refuseBranchCount
branchFlagCount

choiceOverlayDeterministic
choiceOverlaySourcePackageSha256
choiceOverlayPackageSha256
choiceOverlayControlledDeltaPassed
nonGeneratedDialoguesByteIdentical
nonDialogueCollectionsByteIdentical

runtimeInitialChoiceIdsPassed
supportBranchPassed
supportReputationDelta
supportAlternativesLocked
supportActiveFollowUpPassed
supportCompletedFollowUpPassed
challengeBranchPassed
challengeEncounterStarted
refuseBranchPassed
refuseReputationDelta
failedBranchAtomicRollbackPassed
branchReplayEquivalent

choicePreviewOriginalSessionByteIdentical
choicePreviewPackageShaUnchanged
choicePreviewMatchesRuntimeIds
choicePrimaryUiNoRawIds
decisionJournalPassed

historySchemaVersion
v5ChoiceCurrent
v4ChoicesPending
oldProjectBuildInvocationCount
oldProjectUpgradedWithoutSourceRewrite
regenerationChoiceCurrent
rollbackChoiceCurrent
choiceSealTamperRejected

exactSaveChoiceFlagPassed
exactContinueRuntimeStartCount
preChoiceSaveUnchosenPassed
oldSaveRebaseRequired
rebaseMigrationPassed
compatibleBranchFlagPreserved
incompatibleBranchFlagDropped
ghostBranchAbsent
postMigrationChoiceCombatTravelPassed

hostCacheKey
hostReused
hostRebuilt
unityEditorProcessStartCount
hiddenSmokeInvocationCount
hiddenSmokePassed
correctiveSmokeRetryCount
actualPayloadChoiceFactsPassed
releaseCandidateRecordCurrent
portableAllSelectablePassed
portableCoreOnlyPassed
coreOnlyNoFalseRcReady

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

goal167Accepted=false
goal167ManualReviewRequired=false
goal167IndependentAuditRequired=true
```

No required GREEN value may be null/PARTIAL/NOT_EXECUTED.

## R. State and docs

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
docs/manual-acceptance/goal166-exact-combat-actions-real-recovery.md
```

Create:

```text
docs/manual-acceptance/goal167-generated-choice-branching.md
```

No human gate.

Required GREEN state:

```text
goal166IndependentAuditResult=GREEN_ACCEPTABLE_CANDIDATE_AT_D265E809
goal166IndependentAuditPassed=true
goal166IndependentAuditRequired=false

goal166ImplementationStatus=GREEN
goal166CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal166Accepted=false

goal167ImplementationStatus=GREEN
goal167CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal167Accepted=false
goal167AcceptedByHuman=false
goal167AcceptedByCodex=false
goal167ManualReviewRequired=false
goal167ManualGateReady=false
goal167IndependentAuditRequired=true

goal167ChoiceOverlayPassed=true
goal167BranchRuntimeQualificationPassed=true
goal167DecisionJournalPassed=true
goal167SaveContinuePassed=true
goal167MigrationPassed=true
goal167HostReused=true
goal167HostRebuilt=false
goal167UnityEditorProcessStartCount=0
goal167HiddenSmokeInvocationCount=1
goal167PortableAllSelectablePassed=true
goal167PortableCoreOnlyPassed=true
goal167ArtifactScopeViolationCount=0

nextAction=independent_goal167_audit_and_plan_campaign_relationships_and_multi_quest_arcs
```

Release risk statement:

```text
Generated NPC dialogues now contain deterministic data-derived choices with mutually exclusive branch
flags, faction/quest/encounter consequences, branch-specific follow-ups and a player-facing decision
journal. Exact saves preserve decisions; migration preserves only flags tied to compatible generated
dialogues and drops incompatible choices causally. Multi-quest authored arcs and relationship systems
remain future work.
```

## S. Text integrity

Scan changed/task/evidence/docs files:

```text
valid UTF-8
no NUL
no forbidden C0 except CR/LF/TAB
no mojibake markers
no escaped Cyrillic where forbidden
no absolute disposable paths in committed evidence
```

Historical evidence immutable.

## T. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal167-generated-choice-branching.ps1
.devflow/scripts/run-goal167-generated-choice-branching.cmd

src/LLMGameCreator.Application/Generation/Procedural/GeneratedCampaignChoiceModels.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedCampaignChoiceBindingService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedCampaignChoiceOverlayService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedCampaignChoiceQualificationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCandidateSealService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCommitValidator.cs

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildHistoryReader.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectGeneratedWorldSummaryService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs

src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionModels.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignDialogueChoicePreviewService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignDecisionJournalService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignActionPlanner.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignProjectionService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignConsequenceProjector.cs

src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignPageControl.cs
src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/Goal167/Goal167ChoiceBindingOverlayTests.cs
tests/LLMGameCreator.Tests/Application/Goal167/Goal167BranchRuntimeQualificationTests.cs
tests/LLMGameCreator.Tests/Application/Goal167/Goal167ChoicePreviewJournalTests.cs
tests/LLMGameCreator.Tests/Application/Goal167/Goal167HistoryRegenerationTests.cs
tests/LLMGameCreator.Tests/Application/Goal167/Goal167SaveMigrationTests.cs
tests/LLMGameCreator.Tests/Application/Goal167/Goal167StandalonePortabilityTests.cs
tests/LLMGameCreator.Tests/Application/Goal167/Goal167RegressionImmutabilityTests.cs

tests/LLMGameCreator.Tests/Application/Goal166/Goal166QualifiedActionCatalogTests.cs
tests/LLMGameCreator.Tests/Application/Goal166/Goal166RealDefeatRetryTests.cs
tests/LLMGameCreator.Tests/Application/Goal164/Goal164GeneratedCampaignRouteTests.cs
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
docs/manual-acceptance/goal166-exact-combat-actions-real-recovery.md
docs/manual-acceptance/goal167-generated-choice-branching.md

docs/agent-tasks/goal-167-generated-campaign-choice-branching-and-persistent-consequences/
.llmgc/procedural/goal-167-generated-campaign-choice-branching-and-persistent-consequences/
.llmgc/exports/goal-167-generated-campaign-choice-branching-and-persistent-consequences/
```

Conditional production exception:

```text
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplayDefinitionFingerprintService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplaySaveMigrationService.cs
```

These two may change only when a real branch-flag migration test proves the existing exact-dialogue
fingerprint path incorrect. Record the exact failure before editing.

Forbidden:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Domain/**
catalogs/feature-modules/**
unity/**
src/LLMGameCreator.Application/Generation/Procedural/ProceduralGameKernel*
src/LLMGameCreator.Application/Generation/Procedural/GeneratedPackageMvp*
src/LLMGameCreator.Application/Generation/Procedural/GeneratedProjectOverlay*
src/LLMGameCreator.Application/Generation/Procedural/GeneratedWorldEncounterCombatOverlay*
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/**
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectReleaseCandidate*
```


## U. Command budget

```text
read-first/architecture: 12 minutes
binding/overlay/controlled delta: 22 minutes
Runtime qualification/preview: 22 minutes
campaign UI/decision journal: 18 minutes
history/regeneration/save migration: 24 minutes
behavioral tests: 32 minutes
real branch/standalone/portable matrix: 18 minutes
regressions/evidence/docs/scope: 20 minutes
target wall clock: 145 minutes
maximum two concurrent testhost processes
Unity host builds: 0
hidden smoke: exactly 1
```

Rules:

```text
write complete test inventory before production edits
write evidence/publication script before the real matrix
do not ask for additional plan confirmation
no unchanged command retries
no timeout escalation
after failure isolate exact class/test
P0/P1 fixed inside Goal167
P2/P3 debt only
do not stop at compile success
do not defer evidence/docs/scope
```

## V. Publication

Create exactly one final commit:

```text
GREEN Goal 167 generated campaign choice branching and persistent consequences
```

or honest:

```text
BLOCKED Goal 167 generated campaign choice branching and persistent consequences
FAILED Goal 167 generated campaign choice branching and persistent consequences
```

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
Goal142/Goal148 and generation sidecars unchanged
Goal166/167 accepted=false
no human gate
```

## W. GREEN criteria

```text
Goal166 independent audit recorded GREEN
Goal167 >=62 discovered / >=54 behavioral / all pass
exact generated actor/dialogue/faction/quest/encounter bindings
deterministic controlled choice overlay
data-derived Support/Challenge/Refuse branches
mutually exclusive branch flag using exact dialogue ID
actual Runtime branch qualification and atomic rollback
choice availability preview matches Runtime without session mutation
human choice previews and Решения journal
Support active/completed follow-ups
Challenge starts exact generated combat
Refuse applies exact negative reputation
v5 CHOICE_CURRENT history
old v4 CHOICES_PENDING and one-build upgrade
regeneration/rollback v5 current
exact save/continue branch persistence
explicit migration preserve/drop with no ghost branch
one cached standalone smoke
RC CURRENT
portable all-selectable/core-only
required regressions GREEN
source/sidecars immutable
15+15 evidence from typed captures
text integrity GREEN
artifact scope 0
one final commit pushed
```

## X. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning;
- Goal166 independent-audit intake;
- exact actor/dialogue relationship bindings;
- branch counts and data sources;
- flag identity and controlled delta;
- Support/Challenge/Refuse actual Runtime results;
- preview session/package immutability;
- decision journal and primary UI;
- v5/v4/v3/v2 history;
- old-project upgrade;
- regeneration/rollback;
- save/exact continue;
- migration preserve/drop and ghost-branch rejection;
- host/Unity/smoke/payload/RC;
- portable all-selectable/core-only;
- tests/regressions;
- source/sidecar immutability;
- evidence/text/artifact scope;
- state/no-human-gate;
- SHA/push/HEAD/worktree;
- confirmation Codex committed and pushed on any final status.
