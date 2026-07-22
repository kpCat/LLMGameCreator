# Goal 167C — Generated Choice Branching Completion & Qualification

## Identity

- Task ID: `goal-167c-generated-choice-branching-completion-and-qualification`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `0c15e19d4141febddd447e792acaecaa17a98f90`
- Required base message: `FAILED Goal 167 generated campaign choice branching and persistent consequences`
- Original task: `docs/agent-tasks/goal-167-generated-campaign-choice-branching-and-persistent-consequences/GOAL.md`

This is a new isolated Codex dialog. Complete the already-pushed FAILED Goal167 scaffold. Read this
file first, then the original Goal167 GOAL. This file overrides the original where more specific.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

The architecture and failure points are now bounded. The partial commit contains useful binding,
overlay, history and UI scaffolding, but qualification, readiness, regeneration, migration,
standalone and nearly all behavioral tests are incomplete.

## Approval and publication

The complete plan is approved by launching this task.

```text
do not ask for another confirmation because more than ten files are involved
do not stop after compile success
do not ask for manual testing
```

Create and push exactly one final commit for every outcome:

```text
GREEN Goal 167 generated campaign choice branching and persistent consequences
BLOCKED Goal 167 generated campaign choice branching and persistent consequences
FAILED Goal 167 generated campaign choice branching and persistent consequences
```

On BLOCKED/FAILED, commit all honest implementation, tests, diagnostics, evidence and state.

Forbidden:

```text
leaving product changes uncommitted
asking the user to commit or push
ending without final SHA and push attempt
```

No intermediate commits.

## Initial worktree

After unpacking, only these untracked files are allowed:

```text
docs/agent-tasks/goal-167c-generated-choice-branching-completion-and-qualification/GOAL.md
docs/agent-tasks/goal-167c-generated-choice-branching-completion-and-qualification/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-167c-generated-choice-branching-completion-and-qualification/README.md
```

Require:

```text
HEAD == origin/main == 0c15e19d4141febddd447e792acaecaa17a98f90
branch=main
no other tracked/staged/untracked changes
```

Do not reset, revert, stash, merge or rebase the FAILED commit.

## Failed-base status

Record:

```text
goal167FailedBaseSha=0c15e19d4141febddd447e792acaecaa17a98f90
goal167FailedBaseStatus=FAILED_INCOMPLETE_PUBLISHED
goal167FailedBaseAccepted=false
goal167FailedBaseClosedBy=goal167c only on GREEN
```

The base includes useful scaffolding:

```text
choice models
actor/dialogue/faction/quest/encounter bindings
dialogue overlay
Runtime qualification skeleton
preview and decision-journal skeleton
partial build/history v5 wiring
partial WinForms journal
FAILED evidence/state/publication
```

It does not qualify the product.

## Mandatory scaffold audit

Within the first 15 minutes inspect every Goal167 production/test file and write:

```text
.llmgc/procedural/goal-167-generated-campaign-choice-branching-and-persistent-consequences/partial-scaffold-audit.json
```

Classify each:

```text
KEEP_AND_COMPLETE
REFACTOR
REPLACE
REMOVE_AS_UNUSED
```

Fields:

```text
path
classification
implementedBehavior
falseOrMissingTruth
completionTests
```

Merge this audit into the final architecture review. The final evidence root still contains exactly
the original 15 files.

Do not delete the scaffold wholesale. Do not preserve placeholders just because they build.

## Execution budget

```text
Unity Editor starts: 0
Unity host builds: 0
hidden standalone smoke: exactly 1
corrective smoke retry: 0
visible automatic launch: 0
manual user test: 0
```

Host cache reuse only. No Unity source change.

## Proven scaffold defects

### P1-A — Challenge qualification is impossible

Current flow:

```text
Choose Challenge
→ encounter active
→ Reopen refuses while encounter active
→ alternatives_not_locked
```

Required:

```text
choose Challenge
verify exact encounter active and dialogue closed
verify branch flag and initial alternatives locked immediately
finish through actual flee or victory
reopen dialogue afterward
verify Challenge follow-up and no Support/Refuse initial choices
```

### P1-B — false qualification fields

Current code writes `AtomicRollbackPassed=true` without testing rollback.

Current replay has one frame per `(dialogue, branch)`, so distinct-state count is always one.

Required:

```text
real invalid branch Runtime failure
exact state/package rollback
two independent fresh-session executions per branch
ordered commands/events/state hashes equivalent
```

### P1-C — branch consequences are not proven

Required actual Runtime truth:

```text
Support:
  exact positive data-derived faction reputation delta
  no encounter starts
  active quest follow-up
  completed quest follow-up after real quest completion

Refuse:
  exact negative data-derived reputation delta
  no quest/encounter mutation

Challenge:
  exact generated encounter starts
  combat/recovery remains usable
```

### P1-D — campaign readiness ignores choices

`GeneratedCampaignSessionTruthService` was not integrated.

Required:

```text
branchable generated project:
  v5 CHOICE_CURRENT required for READY

genuine v4:
  combat CAMPAIGN_CURRENT
  choices CHOICES_PENDING
  PROJECT_NOT_READY
  campaign.generated_choices_not_current
  Projects action Собрать и играть
```

### P1-E — choice build is incorrectly gated by encounters

Choice overlay currently runs only when combat runtime exists and generated encounters are nonempty.

Required:

```text
choice binding/overlay/qualification runs for every generated project with branchable dialogues
Support/Refuse-only project can reach CHOICE_CURRENT
combat overlay remains optional/data-derived
Challenge exists only when an encounter exists
```

### P1-F — regeneration seal and migration are absent

Choice summary/overlay/flag inventory are not sealed or semantically validated.

No real branch save/migration test exists.

### P1-G — preview/journal rely on metadata

Metadata may provide labels, but availability, success, consequence and status require:

```text
Runtime choiceIds
cloned command result
flag delta
reputation delta
quest state
encounter state/event
```

### Test boundary

The only executable Goal167 test checks the length of a string inventory. It is not behavioral proof
and must be replaced.

## Preserve Goal166

Do not regress:

```text
exact qualified combat catalog
real generated combat
DEFEATED/retry/save/new-game recovery
tactical UI
physical core-only portable copy
59/59 Goal166
```

Goal166 independent audit is GREEN and no human gate exists.


## Mandatory read-first architecture review

Read at most 20 primary files:

```text
GeneratedCampaignChoiceModels.cs
GeneratedCampaignChoiceBindingService.cs
GeneratedCampaignChoiceOverlayService.cs
GameProjectGeneratedCampaignChoiceQualificationService.cs
GameProjectBuildAndQualificationService.cs
GameProjectBuildHistoryReader.cs
GameProjectWorkspaceModels.cs
GameProjectSeedRegenerationCandidateSealService.cs
GameProjectSeedRegenerationCommitValidator.cs
GeneratedCampaignSessionTruthService.cs
GeneratedCampaignDialogueChoicePreviewService.cs
GeneratedCampaignDecisionJournalService.cs
GeneratedCampaignActionPlanner.cs
GeneratedCampaignSessionService.cs
GeneratedCampaignProjectionService.cs
GeneratedCampaignConsequenceProjector.cs
GeneratedGameplayDefinitionFingerprintService.cs
GeneratedGameplaySaveMigrationService.cs
DialogueRuntimeService.cs
Goal167 test files
```

Before production edits replace the one-line architecture review with a complete review containing:

```text
failedScaffoldAudit
bindingTruth
branchFlagIdentity
overlayControlledDelta
supportRuntimeFlow
challengeRuntimeFlow
refuseRuntimeFlow
invalidBranchRollback
independentReplay
previewAvailability
decisionJournalTruth
buildPipelineOrder
primaryRuntimeTruth
historyV5Compatibility
campaignReadiness
regenerationSeal
saveExactContinue
migrationPreserveDrop
standaloneRcPortable
failureMatrix
nonGoals
```

Each section names exact types, inputs, outputs, hashes and behavioral tests.

## A. Complete exact bindings

Refactor `GeneratedCampaignChoiceBindingService`.

### A1. Actor/dialogue provenance

Require exact unique chain:

```text
RegeneratedPlan.ActorSeeds.ActorSeedId
→ GeneratedProjectOverlayDocument generated record for game.dialogues
→ DialogueDefinition.Metadata[sourceActorSeedId]
→ exactly one generated dialogue
→ exactly one actor entity with interactable.dialogueId
→ exactly one interactionId and InteractionDefinition
```

Do not derive correctness from titles or prefixes.

Do not silently canonicalize arbitrary values with a blanket `"generated/" + value` rule. Use the
same exact source-to-package mapping semantics used by the existing generated overlay. A source ID may
already be namespaced; double-prefix and guessed IDs must be rejected.

### A2. Faction/quest/encounter relationships

Resolve exact actor faction and region from strict plan, then package definitions through source
metadata/provenance.

Support relationship exists only when:

```text
same-region generated quest
quest source faction equals actor faction
exact quest definition
exactly one nonzero reputation reward for exact faction
```

Challenge relationship exists only when:

```text
same-region generated encounter
prefer exact encounter whose plan ActorSeedIds contains actor
otherwise deterministic canonical same-region encounter
exact package encounter exists
```

Refuse requires the same faction/reputation source as Support.

Ambiguity is causal:

```text
generated_choice.relationship_ambiguous
```

Do not select the first ambiguous quest.

Actor with no valid relationship remains minimal and does not fail the entire build.

### A3. Choice identity

Choice IDs must be unique within the dialogue and stable per branch.

Avoid the same generic IDs across unrelated dialogue records only if the Runtime/package requires
global uniqueness; otherwise dialogue-local identity is allowed. Tests must prove package validation.

Flag truth remains:

```text
Flag.Id = exact DialogueDefinition.Id
Flag.Value = SUPPORT | CHALLENGE | REFUSE
```

## B. Complete controlled overlay

Refactor `GeneratedCampaignChoiceOverlayService`.

### B1. Preserve original dialogue semantics

The generated MVP dialogue may contain original nodes/choices. Do not silently delete meaningful
non-placeholder content.

For the known minimal generated dialogue:

```text
replace the single placeholder Continue choice with branch choices/follow-ups
```

For future generated dialogue content:

```text
preserve non-placeholder nodes/choices
add branching nodes/choices deterministically
```

Classify placeholder only through exact generated provenance/shape, not arbitrary English text.

### B2. Initial branches

Each initial branch requires:

```text
flag_equals(dialogue.Id, "")
```

Support:

```text
set flag SUPPORT
exact positive faction reputation output derived from quest reward magnitude
no StartQuest when AutoStart/current
close dialogue
```

Challenge:

```text
set flag CHALLENGE
StartEncounterId exact
close dialogue
```

Refuse:

```text
set flag REFUSE
exact negative faction reputation output
no quest/encounter mutation
close dialogue
```

### B3. Follow-ups

Support:

```text
flag SUPPORT + quest active → active follow-up
flag SUPPORT + quest completed → completed follow-up
```

Challenge:

```text
flag CHALLENGE + no active encounter → chosen/resolved follow-up
```

Refuse:

```text
flag REFUSE → refused follow-up
```

No initial branch remains available after a flag is set.

### B4. Controlled delta

Allowed only:

```text
bound generated dialogue Nodes
generatedChoice* metadata
generated_choice_branching tag
```

Require canonical equality for:

```text
Manifest
GeneratedContent
all non-dialogue collections
all non-generated dialogues
dialogue identity/title/start/background/source metadata
maps/entities/interactions/quests/encounters/factions/items/abilities
```

Definition collection counts unchanged.

Independently rebuild twice from separately cloned/reordered inputs and require:

```text
same canonical overlay document
same output package SHA
```

## C. Build pipeline correction

Choice processing is independent of generated encounter count.

Required generated flow:

```text
strict source
→ generated start/travel package
→ choice binding/overlay when branchable dialogues exist
→ combat contract/overlay when generated encounters exist
→ combat qualification when generated encounters exist
→ choice qualification against final package when branchable dialogues exist
→ final package validation/history
```

Cases:

```text
branchable dialogues + zero encounters:
  choices CHOICE_CURRENT
  combat ABSENT
  build can pass

encounters + no branchable dialogues:
  combat CAMPAIGN_CURRENT
  choices ABSENT
  build can pass

both:
  both summaries current
```

No `if generated encounter count > 0` may suppress Support/Refuse choices.

### C1. Primary runtime truth

For v5 generated projects with choices, primary build truth becomes the deterministic complete choice
route:

```text
dialogue open
→ branch choice
→ follow-up
→ associated challenge combat/travel when present
```

Set:

```text
primaryFinalStateHash = choice qualification FinalStateHash
primary RuntimeFrames = choice qualification frames
checkpoint/replay/action binding = actual choice qualification truth
```

Combat summary keeps its own final-state hash and remains independently validated.

Update v5 history eligibility so:

```text
choice.FinalStateHash == entry.FinalStateHash
combat exact package/hash/passed remains required
combat.FinalStateHash need not equal v5 entry FinalStateHash
```

For v4, retain old combat final-state behavior.

## D. Real branch qualification

Replace the current qualification scaffold with typed per-branch captures.

Create or extend:

```text
GeneratedCampaignChoiceBranchQualification
GeneratedCampaignChoiceReplayCapture
GeneratedCampaignChoiceFailureCapture
```

Every branch uses a fresh exact package session.

### D1. AutoStart quests

Before opening dialogue, start all exact `AutoStart` quests through existing Runtime commands, matching
the production campaign service. Do not assume Runtime Start does it automatically.

### D2. Initial availability

After open:

```text
Runtime event choiceIds equals exact initial branch IDs
no follow-up ID appears
```

### D3. Support

Record before/after:

```text
flag
faction reputation
quest state
active encounter
inventory
```

Require:

```text
flag SUPPORT
positive delta exactly equals branch.ReputationAmount
no encounter starts
quest remains active
reopen → exactly SUPPORT active follow-up
Support/Challenge/Refuse initial choices absent
```

Then complete the associated generated quest through real existing commands:

```text
use current exact combat contract/actions
gain required item
manual CompleteQuest exactly once
```

Reopen and require exactly SUPPORT completed follow-up.

No direct quest-state mutation.

### D4. Challenge

Require immediately after choice:

```text
flag CHALLENGE
exact encounter active
dialogue closed
no reputation/quest mutation unless actual Runtime events define one
```

Do not attempt to reopen while encounter is active.

Prove alternatives locked from the flag by evaluating cloned dialogue conditions or after finishing
the encounter.

Run one fresh flee branch:

```text
flee
no reward/quest readiness/reputation
reopen → CHALLENGE follow-up only
```

Run one fresh victory/recovery-compatible branch:

```text
qualified combat/recovery remains functional
reopen after encounter completion → CHALLENGE follow-up only
```

### D5. Refuse

Require:

```text
flag REFUSE
negative delta exactly equals branch.ReputationAmount
no encounter
quest state/inventory unchanged
reopen → REFUSE follow-up only
```

### D6. Atomic rollback

Build an isolated invalid choice fixture, not production content:

```text
set flag then failing exact effect/cost/encounter reference in the same choice
```

Execute through Runtime.

Require exact canonical before/after equality for:

```text
flags
reputation
quest
encounter
inventory
dialogue state
```

Set `AtomicRollbackPassed` only from this capture.

### D7. Independent replay

For every branch execute two separately created sessions.

Compare:

```text
ordered Runtime command types
ordered event types/target IDs/normalized args
branch flag
reputation delta
quest/encounter outcome
final state hash
available follow-up IDs
```

Set `ReplayPassed` only from these two captures.

## E. Preview and decision journal truth

### E1. Preview

`GeneratedCampaignDialogueChoicePreviewService`:

1. clone exact current session per choice;
2. execute exact choice;
3. capture actual before/after deltas and events;
4. discard clone;
5. confirm original session/package byte-identical.

Enabled choices must equal the latest Runtime-provided `choiceIds` for the active dialogue/node when
that event evidence is present. If an exact-continue session lacks a matching event, clone execution
is the source of availability; record that source explicitly.

Human consequence preview is derived from:

```text
flag delta
reputation delta
quest state delta
encounter started
inventory/cost delta
```

Metadata supplies labels only.

### E2. Journal

Project decisions from actual Runtime flags.

For each exact branching dialogue flag:

```text
resolve exact initial branch definition
resolve related human faction/quest/encounter labels
derive current status from actual quest/encounter state
derive locked alternatives from current available choice IDs/requirements
```

Statuses, naming flexible:

```text
CHOSEN
QUEST_ACTIVE
QUEST_COMPLETED
CHALLENGE_ACTIVE
CHALLENGE_RESOLVED
REFUSED
```

Do not mark every branch `FollowUpAvailable` merely because a follow-up definition exists.

### E3. Campaign consequences

Update `GeneratedCampaignConsequenceProjector` and session execution.

On `ChooseDialogue` produce state/event-backed:

```text
Decision
BranchLocked
BranchFollowUp
Reputation
EncounterStarted
Quest state when changed
```

No metadata-only success consequence.

## F. Campaign readiness and Projects UI

Modify `GeneratedCampaignSessionTruthService`.

If strict source has branchable generated dialogues:

```text
selected current history schema v5
GeneratedCampaignChoices.Passed=true
Status=CHOICE_CURRENT
choice final/package/authoring hashes exact
```

Otherwise return:

```text
PROJECT_NOT_READY
campaign.generated_choices_not_current
```

Old v4 project must show `Собрать и играть`.

Add Projects rows:

```text
Столкновения
Сюжетные решения
Игровая кампания
```

Human statuses only; technical details elsewhere.


## G. History v5 and compatibility

Complete `unified_game_project_build_history_v5`.

Persist:

```text
GeneratedWorld
GeneratedWorldActivation
GeneratedRegionTravel
GeneratedEncounterCombat
GeneratedCampaignChoices
AcceptedMechanics
AcceptedMechanicsCompatibility
```

Reader:

```text
v5 branchable choice summary valid/current:
  CHOICE_CURRENT

genuine v4 generated project:
  combat CAMPAIGN_CURRENT
  choices CHOICES_PENDING
  campaign not READY

v3:
  combat COMBAT_PENDING
  choices ABSENT

v2:
  START_CURRENT
  choices ABSENT
```

Historical rows are never rewritten.

### G1. Choice eligibility

Require:

```text
Present=true
Passed=true
Status=CHOICE_CURRENT
BranchableDialogueCount == QualifiedDialogueCount
branch flag IDs unique and exact
RuntimeQualificationPassed
ExclusiveBranchingPassed
FollowUpPassed
AtomicRollbackPassed
ReplayPassed
FinalPackageSha256 == entry.PackageSha256
FinalStateHash == entry.FinalStateHash
overlay hash/counts exact
```

Do not accept hardcoded booleans without typed captures.

### G2. Combat eligibility in v5

For v5:

```text
combat exact package SHA == entry package SHA
combat summary passed
qualified actions/encounters exact
combat final-state hash remains its own qualification hash
```

Do not require `combat.FinalStateHash == entry.FinalStateHash` when the v5 primary final state belongs
to the full choice route.

For v4 retain historical equality.

## H. Regeneration and rollback sealing

Extend:

```text
GameProjectSeedRegenerationCandidateSealService
GameProjectSeedRegenerationCommitValidator
```

Candidate seal fields:

```text
GeneratedCampaignChoiceSummarySha256
GeneratedCampaignChoiceOverlaySha256
GeneratedCampaignChoiceFlagInventorySha256
```

Flag inventory canonical rows:

```text
DialogueId
SupportedBranchKinds[]
```

No chosen player value is sealed at build time.

Semantic validation requires:

```text
v5 for branchable project
CHOICE_CURRENT
branchable/qualified counts exact
choice final/package hashes exact
choice overlay canonical hash exact
flag inventory exact
combat summary still exact
```

Tamper tests:

```text
remove branch
change branch flag value
change quest/encounter target
change reputation amount
change follow-up requirement
change overlay hash
change flag inventory
```

Every tamper rejects before promotion.

Regeneration and historical rollback:

```text
rebuild choices from current mechanics + candidate/historical strict source
produce v5 CHOICE_CURRENT
never restore historical final package
old source/sidecars immutable
```

## I. Save, exact continue and migration

### I1. Exact continue

Support route:

```text
save CURRENT after branch
clear/recreate campaign service
Continue exact
Runtime Start count=0
flag exact
reputation exact
journal exact
initial alternatives remain locked
active/completed follow-up exact
```

Pre-choice save:

```text
restores empty branch flag
all initial choices available
journal empty
```

### I2. Old-save package rebase

Rebuilding old v4 with choices changes package fingerprints.

Require:

```text
old save PACKAGE_REBASE_REQUIRED
direct Continue rejected
preview zero-write
explicit apply
migrated save CURRENT
branchless state remains branchless
```

### I3. Branch migration

First test existing migration implementation before editing it.

Because branch flag ID is the exact generated dialogue ID:

```text
same canonical dialogue definition:
  flag preserved

changed or missing dialogue definition:
  flag dropped with reason
```

If this fails due to actual fingerprint/reference handling, modify only the conditional allowed files
and record exact failing test/diagnostic in architecture review and evidence.

After migration:

```text
active dialogue reset
journal includes only retained branch flags
no ghost row for dropped dialogue
dialogue/combat/travel work
```

Source save tree remains unchanged by regeneration/preview.

## J. Standalone, RC and portable qualification

After the real v5 all-selectable build:

```text
exactly one cached hidden standalone smoke
corrective retry=0
HostReused=true
HostRebuilt=false
Unity Editor starts=0
payload preflight/self-check GREEN
actual payload package/composition/final hashes match v5
human facts include branching/exclusivity/persistence
runtime frames include dialogue choice/follow-up and combat/travel
RC CURRENT
```

No standalone or RC implementation change is expected.

Portable all-selectable copy without operational output:

```text
v5 CHOICE_CURRENT
branch save CURRENT
decision journal exact
RC CURRENT
```

Portable core-only:

```text
v5 CHOICE_CURRENT
branch/campaign/save current
AcceptedMechanics incomplete
no false RC CURRENT/READY/PENDING
no operational pointer
```

## K. Mandatory real product matrix

Use real disposable projects.

### K1. Old v4 upgrade

```text
open genuine Goal166 v4 project
combat CAMPAIGN_CURRENT
choices CHOICES_PENDING
campaign PROJECT_NOT_READY
Projects Собрать и играть
one BuildAndQualify
v5 CHOICE_CURRENT
generation source/sidecars byte-identical
```

### K2. Branch surfaces

Visit every branchable generated actor.

Require:

```text
actual Runtime available choiceIds
human actor/faction/quest/encounter titles
state-backed consequence preview
no raw IDs/hashes/paths/codes
```

### K3. Support

Execute full D3 through production campaign service, including actual generated quest completion and
manual turn-in.

### K4. Challenge

Execute full D4 through production campaign service.

### K5. Refuse

Execute full D5 through production campaign service.

### K6. Failure rollback

Execute D6 with actual Runtime.

### K7. Save/continue and migration

Execute I1–I3.

### K8. Regeneration/rollback

Both candidates reopen v5 CHOICE_CURRENT and pass semantic seals.

### K9. Standalone/portable

Execute J.

### K10. Zero-encounter branch profile

Create a focused fixture with branchable Support/Refuse relationships and no generated encounter.

Require:

```text
choices CHOICE_CURRENT
combat ABSENT
campaign READY
Challenge absent
```

No production fixed IDs/counts.

## L. Behavioral tests

Replace all placeholder three-line files.

Require at least:

```text
Goal167 discovered >=64
Goal167 behavioral passed >=56
all discovered Goal167 tests pass
```

### Binding and overlay

1. exact actor/dialogue provenance;
2. no blanket prefix guessing;
3. missing/duplicate dialogue rejected;
4. entity/interaction exact;
5. support relationship exact;
6. ambiguous support rejected;
7. challenge actor-specific preference;
8. challenge canonical fallback;
9. refuse exact;
10. no-relationship actor minimal;
11. flag ID exact dialogue ID;
12. initial empty flag requirement;
13. follow-up exact branch requirement;
14. original placeholder replaced;
15. non-placeholder dialogue content preserved;
16. non-generated dialogues unchanged;
17. non-dialogue collections unchanged;
18. definition counts unchanged;
19. independent rebuild deterministic;
20. reordered input deterministic;
21. forbidden delta rejected.

### Runtime qualification

22. auto-start quests started;
23. initial Runtime choice IDs exact;
24. no follow-ups initially;
25. Support exact positive reputation;
26. Support no encounter;
27. Support active follow-up;
28. Support completed follow-up after real quest completion;
29. Support alternatives locked;
30. Challenge starts exact encounter;
31. Challenge does not prematurely reopen;
32. Challenge flee follow-up;
33. Challenge victory/recovery compatibility;
34. Refuse exact negative reputation;
35. Refuse no quest/encounter mutation;
36. invalid branch rollback exact;
37. independent branch replay equivalent;
38. package SHA/reference unchanged.

### Preview/journal/UI

39. preview clones session;
40. preview original byte-identical;
41. preview package unchanged;
42. preview matches Runtime choiceIds;
43. preview state-backed reputation;
44. preview state-backed encounter;
45. disabled reason human;
46. journal from flags;
47. journal quest active status;
48. journal quest completed status;
49. journal challenge active/resolved status;
50. journal refuse status;
51. ghost flag absent;
52. primary UI no technical values;
53. 1100x720 choices/journal fit;
54. branch consequences state-backed.

### Build/history/readiness

55. choices independent of encounters;
56. zero-encounter Support/Refuse current;
57. choice overlay before combat overlay;
58. v5 primary final state is choice route;
59. v5 combat remains independently valid;
60. v5 history current;
61. v4 choices pending;
62. v3/v2 compatibility;
63. old project campaign not ready;
64. one build upgrades old project;
65. Projects human rows;
66. branchless project choices absent valid.

### Regeneration/save/standalone

67. seal includes choice summary/overlay/flags;
68. each tamper rejected;
69. regeneration v5 current;
70. rollback v5 current;
71. exact save branch/journal;
72. pre-choice save unchosen;
73. old save rebase required;
74. explicit rebase current;
75. compatible flag preserved;
76. incompatible flag dropped;
77. no ghost journal;
78. post-migration dialogue/combat/travel;
79. one hidden smoke;
80. RC current;
81. portable all-selectable;
82. portable core-only no false RC.

### Regressions

83. Goal166 59/59;
84. Goal165 55/55;
85. Goal164 61/61;
86. Goal163/162/161;
87. Runtime Simulator;
88. source/sidecar immutability.

String inventory/source assertions do not count as behavioral proof.


## M. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal167"
# require >=64 discovered and >=56 behavioral

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

Then execute the real product matrix K and exactly one cached hidden smoke.

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

## N. Evidence

Replace the FAILED placeholder evidence with exactly the original 15 files in each mirrored root:

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

Delete the temporary `partial-scaffold-audit.json` after merging it into architecture-review so the
final roots contain exactly 15 files.

Twins byte-identical.

Every dashboard field must be generated from typed captures produced by tests/real routes. No
unmeasured `true`, symbolic `"verified"`, or static aggregate claim.

### Dashboard fields

```text
status
candidateStatus
goal167TestsDiscovered
goal167BehavioralTestsPassed

goal166IndependentAuditPassed
failedBaseSha
failedScaffoldAudited
failedScaffoldClosed

generatedDialogueCount
branchableDialogueCount
qualifiedDialogueCount
supportBranchCount
challengeBranchCount
refuseBranchCount
branchFlagCount
zeroEncounterBranchProfilePassed

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
challengeFleeFollowUpPassed
challengeVictoryFollowUpPassed
refuseBranchPassed
refuseReputationDelta
failedBranchAtomicRollbackPassed
branchReplayEquivalent

choicePreviewOriginalSessionByteIdentical
choicePreviewPackageShaUnchanged
choicePreviewMatchesRuntimeIds
choicePreviewStateBacked
choicePrimaryUiNoRawIds
decisionJournalPassed
decisionJournalStateBacked
branchConsequencesStateBacked

historySchemaVersion
v5ChoiceCurrent
v4ChoicesPending
v4CampaignNotReady
oldProjectBuildInvocationCount
oldProjectUpgradedWithoutSourceRewrite
choicePrimaryFinalStatePassed
combatSummaryPreservedInV5
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

## O. State and docs

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
docs/manual-acceptance/goal167-generated-choice-branching.md
```

Record the FAILED base historically, not as current truth.

Required GREEN state:

```text
goal166IndependentAuditResult=GREEN_ACCEPTABLE_CANDIDATE_AT_D265E809
goal166IndependentAuditPassed=true
goal166IndependentAuditRequired=false

goal167FailedBaseSha=0c15e19d4141febddd447e792acaecaa17a98f90
goal167FailedBaseStatus=FAILED_INCOMPLETE_PUBLISHED
goal167FailedBaseClosedBy=goal167c

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

No human gate.

## P. Text integrity

Scan actual changed/task/evidence/docs files:

```text
valid UTF-8
no NUL
no forbidden C0 except CR/LF/TAB
no mojibake markers
no escaped Cyrillic where forbidden
no absolute disposable paths in committed evidence
```

Historical evidence outside Goal167 remains immutable.

## Q. Artifact scope

Use the original Goal167 allowed paths plus these missing required paths:

```text
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCandidateSealService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCommitValidator.cs

src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionTruthService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignConsequenceProjector.cs

src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs

docs/agent-tasks/goal-167c-generated-choice-branching-completion-and-qualification/
```

Conditional migration paths remain allowed only after a real test proves the current behavior wrong:

```text
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplayDefinitionFingerprintService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplaySaveMigrationService.cs
```

Record exact failing test before editing either.

Forbidden remain:

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

Update artifact-scope policy before final validation.

## R. Command budget

```text
scaffold audit/current build and test: 15 minutes
binding/overlay correction: 18 minutes
branch qualification/replay/rollback: 24 minutes
preview/journal/campaign readiness/UI: 18 minutes
history/seal/regeneration/migration: 22 minutes
behavioral tests: 32 minutes
real product/standalone/portable matrix: 18 minutes
regressions/evidence/docs/scope: 20 minutes
target wall clock: 135 minutes
maximum two concurrent testhost processes
Unity host builds: 0
hidden smoke: exactly 1
```

Rules:

```text
write concrete test classes before completing production edits
replace placeholder test inventory
write evidence/publication runner before real matrix
do not ask for extra plan confirmation
no unchanged command retries
no timeout escalation
after failure run only exact class/test
do not stop at compile success
P0/P1 fixed inside Goal167C
P2/P3 debt only
do not defer evidence/docs/scope
```

## S. Publication

Create exactly one final commit:

```text
GREEN Goal 167 generated campaign choice branching and persistent consequences
```

or honest BLOCKED/FAILED.

Codex must push `origin/main`.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three Goal167C task files tracked
HostReused=true
HostRebuilt=false
Unity starts=0
hidden smoke=1 on GREEN
retry=0
Goal142/Goal148/source sidecars unchanged
Goal166/167 accepted=false
no human gate
```

## T. GREEN criteria

```text
FAILED scaffold audited and closed
Goal167 >=64 discovered / >=56 behavioral / all pass
exact binding with no guessed prefix mapping
Support/Challenge/Refuse data-derived
controlled deterministic overlay
Challenge qualification completes encounter before follow-up
Support active and completed follow-ups
Refuse exact negative consequence
actual atomic rollback
independent replay
state-backed preview/journal/consequences
choice processing independent of encounter count
v5 primary choice runtime truth
v4 choices pending and campaign not ready
regeneration/rollback seals
exact save/continue
explicit migration preserve/drop with no ghost decision
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

## U. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning;
- initial FAILED-base intake;
- scaffold classifications;
- tests before/after completion;
- exact bindings and branch counts;
- controlled delta/determinism;
- Support/Challenge/Refuse real Runtime routes;
- actual rollback and independent replay;
- preview/journal/consequence truth;
- zero-encounter branch profile;
- v5/v4/v3/v2 history and campaign readiness;
- regeneration/rollback seals;
- save/continue/migration preserve/drop;
- host/Unity/smoke/payload/RC;
- portable all-selectable/core-only;
- regressions and source immutability;
- evidence/text/artifact scope;
- final SHA/push/HEAD/worktree;
- explicit confirmation Codex committed and pushed for any final status.
