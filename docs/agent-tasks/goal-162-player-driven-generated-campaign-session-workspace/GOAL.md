# Goal 162 — Player-Driven Generated Campaign Session Workspace

## Identity

- Task ID: `goal-162-player-driven-generated-campaign-session-workspace`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `c1bd0b7a913f2926886d95d05df5ebc2ed639dd3`
- Required base message: `GREEN Goal 161T immutable standalone payload RC correlation and qualification closure`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: this is the next major visible product vertical slice. The generator can now create a durable
seeded project, compose mechanics, start and travel through generated regions, regenerate/restore
worlds, migrate gameplay saves and publish a qualified standalone. The remaining user-facing gap is
that actual play still happens through a technical Runtime Simulator full of raw IDs and low-level
commands. Goal162 creates a normal player workspace over the existing Runtime and save truth without
introducing new gameplay primitives.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal implementation plan; do not ask for confirmation.
- Do not request intermediate or final manual testing.
- Own all P0/P1 defects reproduced by the mandatory Goal162 matrix inside this Goal.
- Record P2/P3 debt without creating Goal162A.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No intermediate commits.
- Codex performs commit and standard push itself.

## Expected initial worktree

After unpacking, only these untracked files are permitted:

```text
docs/agent-tasks/goal-162-player-driven-generated-campaign-session-workspace/GOAL.md
docs/agent-tasks/goal-162-player-driven-generated-campaign-session-workspace/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-162-player-driven-generated-campaign-session-workspace/README.md
```

Require:

```text
HEAD == origin/main == c1bd0b7a913f2926886d95d05df5ebc2ed639dd3
branch=main
tracked diff count=0
staged diff count=0
unknown dirty/untracked count=0
```

Any other dirt blocks execution. Never use reset, stash, merge, rebase or destructive cleanup.

## Execution budget

```text
Unity Editor invocation budget: 0
Unity host build budget: 0
standalone build budget: 0
hidden Player smoke budget: 0
visible automated launch budget: 0
manual user test budget: 0
```

Goal162 is an Application/WinForms campaign-play slice. Preserve the qualified RC and standalone
run/pointer/history bytes.

## Goal161T independent-audit intake

Record:

```text
goal161tIndependentAuditResult=GREEN_ACCEPTABLE_CANDIDATE_AT_C1BD0B7A
goal161tIndependentAuditPassed=true
goal161tIndependentAuditRequired=false
```

Audited truths:

```text
immutable current.json/run is the current standalone payload authority
RC Write correlates pointer/run/result/payload hashes and facts
invalid current pointer never falls back to stale legacy payload
legacy project-local output is compatibility-only
portable project with no operational output retains project-local RC CURRENT
current standalone history selects one exact pointer-matching GREEN row
zero-execution finalization wrote and reread RC CURRENT
run/pointer/standalone-history/build-history/save bytes remained unchanged
Player/Unity/standalone Build counts were zero
portable all-selectable restored TRAVEL_CURRENT/save CURRENT/RC CURRENT without pointer
portable core-only restored TRAVEL_CURRENT/save CURRENT with no false RC readiness
```

Goal160's blocker is closed. Goal161/161T remain `accepted=false`; no human gate.

### Nonblocking audit debt

`FinalizeCurrentReleaseCandidate()` is a recovery-only mutation that does not explicitly acquire the
shared operation lease. Its current package/history/pointer checks and final reread prevent false
CURRENT, so this is P2 concurrency hygiene. Record it in debt; do not turn Goal162 into another RC
task.

## Product problem

`RuntimeSimulatorPageControl` is an engineering console:

```text
English title
dozens of low-level command buttons
raw package IDs in combo boxes
manual participant/quest/objective/dialogue/faction IDs
JSON state and technical logs
```

A user still cannot simply:

```text
Играть
→ Новая игра or Продолжить
→ see the generated map and player
→ move with arrows/WASD
→ talk to a generated character
→ resolve a generated encounter
→ advance and complete a generated quest
→ travel to another generated region
→ save and continue later
→ migrate the save after world regeneration
```

## Product outcome

Add a primary page:

```text
Играть
```

and a Projects-page action:

```text
Играть
or
Собрать и играть
```

Automated campaign route:

```text
open generated project
→ build when stale
→ start on generated map
→ interact with generated actor
→ choose dialogue response
→ start current-region encounter
→ attack/use ability and resolve
→ receive generated reward
→ refresh/complete generated quest
→ observe reputation change
→ navigate to generated gate
→ enter another region
→ interact with destination content
→ save
→ exact continue
→ regenerate world
→ active session becomes stale and save requires migration
→ migrate and continue
→ move/travel/interact again
```

No raw package ID is required from the player.

## Non-goals

Do not add:

```text
new Runtime command/state/event types
new GamePackage fields
new FeatureModules or parameter semantics
Unity changes or standalone rebuild
new generated content algorithms
automatic world regeneration
automatic save migration
real-time combat
audio/media rendering
cloud/multiplayer saves
final installer
```

The technical Runtime Simulator remains available unchanged.


## Mandatory architecture review

Read at most 18 primary files:

```text
RuntimeSimulatorPageControl.cs
UnifiedGameRuntimeService.cs
RuntimeContracts.cs
RuntimeStateSerializer.cs
GeneratedPackageMvpService.cs
SeededGeneratedProjectSourceService.cs
GeneratedWorldRegionMapBindingService.cs
GeneratedGameplaySaveService.cs
GeneratedGameplaySaveMigrationService.cs
GeneratedGameplaySavesSummaryService.cs
UnifiedGameProjectWorkspaceController.cs
GameProjectOperationCoordinator.cs
ProjectsPageControl.cs
MainForm.cs
CompositionRoot.cs
IEditorPage.cs / EditorPageRegistry
Goal158 travel tests
Goal161 migration/runtime-continuation tests
```

Before production edits write:

```text
.llmgc/procedural/goal-162-player-driven-generated-campaign-session-workspace/architecture-review.json
```

Required resolved sections:

```text
goal161tAuditIntake
runtimeSimulatorVersusPlayerBoundary
campaignSessionTruth
staleProjectDetection
contextProjection
generatedRegionContentBinding
actionPlanningAndDispatch
dialogueFlow
encounterFlow
questObjectiveFlow
travelFlow
saveLoadMigrationFlow
sessionLifetimeAcrossNavigation
projectsBuildAndPlay
winFormsNavigation
uiLayout
failureStateMatrix
legacyCompatibility
nonGoals
```

Every section names exact types, inputs, outputs, states and behavioral tests.

## A. Application campaign-session model

Create under a focused namespace/folder, naming flexible:

```text
src/LLMGameCreator.Application/Play/GeneratedCampaign/
```

Types:

```text
GeneratedCampaignSessionService
GeneratedCampaignSessionTruthService
GeneratedCampaignProjectionService
GeneratedCampaignActionPlanner
GeneratedCampaignEventPresenter
GeneratedCampaignSessionModels
```

Do not place player UX logic inside WinForms controls.

### A1. Session status

```text
NO_PROJECT
PROJECT_NOT_GENERATED
PROJECT_NOT_READY
READY
ACTIVE
STALE_PROJECT
SAVE_MIGRATION_REQUIRED
FAILED
```

### A2. Project truth

Create:

```text
GeneratedCampaignProjectTruth
```

Fields:

```text
ProjectFolder
ProjectIdentityFingerprint
WorldId
SourceRecordSha256
SourceRequestSha256
PlanSha256
GeneratedBasePackageSha256
PackageSha256
CompositionPackageSha256
FinalStateHash
QualifiedAuthoringFingerprint
SelectedBuildHistoryFileName
GeneratedStartMapId
RegionMapBindings
```

Ready requires:

```text
strict generated source Present/Passed
workspace current build/history
GeneratedWorld Status=TRAVEL_CURRENT
GeneratedWorldActivation Passed
GeneratedRegionTravel Passed
actual package bytes == activated document hash
authoring fingerprint current
```

A source-only, START_CURRENT, LAST_SUCCESS or invalid project is not campaign-ready.

### A3. Session truth

Capture at Start/Load/Migrate:

```text
ProjectIdentityFingerprint
WorldId
PackageSha256
CompositionPackageSha256
QualifiedAuthoringFingerprint
```

Before every Runtime action:

```text
recapture lightweight current truth
require identity/world/package/authoring exact
```

On mismatch:

```text
do not execute command
Status=STALE_PROJECT
retain in-memory session for display only
offer Save when current truth still permits it or Restart/Migrate
```

Never run an old session against a new package.

### A4. Session lifetime

Register `GeneratedCampaignSessionService` as a singleton.

The active in-memory session survives switching pages in the same application process.

Opening a different project:

```text
marks old session unavailable
does not silently reuse it
does not auto-save
```

## B. Player projection

Create:

```text
GeneratedCampaignSnapshot
```

Fields:

```text
Status
StatusTitle
StatusDescription
ProjectTitle
WorldTitle
WorldSeed
CurrentRegionTitle
CurrentMapTitle
SessionSha256
Map
Player
Nearby
Actions
Resources
Stats
Progressions
Inventory
Equipment
ActiveQuests
Dialogue
Encounter
Factions
RecentEvents
SaveState
TechnicalDetails
Diagnostics
```

Technical IDs/hashes live only in `TechnicalDetails`.

### B1. Map projection

Create data-derived:

```text
GeneratedCampaignMapProjection
GeneratedCampaignMapCell
GeneratedCampaignMapEntity
```

Support arbitrary finite map dimensions.

Each cell includes:

```text
X/Y
Walkable
PlayerPresent
PrimarySymbol
PrimaryTitle
EntityCount
InteractionAvailable
Blocked
```

Resolve titles from actual:

```text
map/tile/entity prototype
dialogue title
item title
travel destination title
GeneratedContent provenance
```

Do not display `generated/`, `entity/`, `region/` IDs in the primary map or tooltips.

Suggested symbols are presentation-only:

```text
player
actor/NPC
cache/item
travel gate
blocked tile
floor
```

No correctness depends on a fixed map size or symbol count.

### B2. Region binding

Use strict source validation and `GeneratedWorldRegionMapBindingService`.

Current region is derived from current map binding, not string parsing.

Current-region content:

```text
NPCs
encounters
quests
dialogues
factions
```

is resolved via package metadata/provenance and exact map/region links.

### B3. HUD projection

Human title/value rows for:

```text
health and resources
attributes/stats
level/progression
inventory item names/counts
equipment slot/item names
faction reputation
active quest titles/objectives/progress
```

No raw IDs in primary HUD.

## C. Context action planner

Create:

```text
GeneratedCampaignAction
GeneratedCampaignActionKind
```

Fields:

```text
ActionId technical
Kind
Title
Description
Enabled
DisabledReason
Primary
TargetTitle
```

Primary UI displays Title/Description only.

Supported kinds, mapped only to existing Runtime commands:

```text
MoveUp
MoveDown
MoveLeft
MoveRight
Interact
OpenDialogue
ChooseDialogue
CloseDialogue
StartEncounter
BasicAttack
UseAbility
EndTurn
RunEncounterAi
ResolveEncounter
FleeEncounter
CompleteQuest
UseItem
Save
Load
MigrateSave
RestartSession
```

Not every action must always be present.

### C1. Map mode

Show only valid movement directions plus context-sensitive Interact.

Movement enablement is derived from map bounds, walkability and collidable entities.

Nearby interaction title is derived from the actual adjacent entity.

Travel gate remains ordinary Interact and must surface a human destination title.

### C2. Dialogue mode

When active dialogue exists:

```text
hide irrelevant map/encounter actions
show current speaker/title/text
show each available choice as a human button
show Close only when Runtime permits it
```

Choosing a response uses existing `ChooseDialogueOption`.

### C3. Encounter mode

Show:

```text
participant names/teams/resources
current turn
player basic attack targets
available player abilities
End turn
Flee when available
Resolve only when Runtime state permits
```

Never ask the user to type participant IDs.

`End turn` may orchestrate the existing bounded AI step:

```text
EndTurn
then RunCurrentTurnAi until player turn or encounter completion
```

Bound by actual participant/turn state; no open loop.

### C4. Region activity

When no encounter/dialogue is active, show current-region generated encounters as human activities.

`StartEncounter` uses the exact package encounter ID internally.

Generated AutoStart quests must already be active after session start; if the existing Runtime does not
start them, Goal162 may invoke the existing StartQuest command once for current AutoStart quests as
session orchestration. Do not modify Runtime semantics.

### C5. Quest flow

After a successful command that can change encounter completion, inventory or quest state:

```text
run existing RefreshQuestObjectives at most once
```

Record follow-up events separately.

When all objectives are complete, show:

```text
Завершить задание
```

Completion uses existing `CompleteQuest` and displays reputation/reward changes.

No debug-only `AdvanceObjective` button in the player workspace.

## D. Command execution

`GeneratedCampaignSessionService.Execute(actionId)`:

1. validate active session;
2. recapture current project truth;
3. resolve action from the current planner snapshot;
4. reject disabled/stale/unknown actions causally;
5. execute exactly the mapped existing Runtime command;
6. execute bounded follow-up orchestration when defined;
7. update session;
8. project events and state;
9. return a new snapshot.

Diagnostics:

```text
campaign.no_project
campaign.project_not_generated
campaign.project_not_ready
campaign.session_not_started
campaign.project_truth_changed
campaign.action_unknown
campaign.action_disabled
campaign.runtime_command_failed
campaign.follow_up_failed
```

On failed Runtime command:

```text
retain the returned truthful Runtime state/events
do not fake success
surface a human message and technical diagnostic
```

## E. New/continue/save/migrate

### E1. New game

```text
StartNew()
```

Uses current qualified package and existing `IUnifiedGameRuntimeService.Start()`.

Require:

```text
generated start map
valid player position
current region resolved
AutoStart generated quest truth
at least one player action
```

No save is written automatically.

### E2. Save

Use `GeneratedGameplaySaveService`.

Default UI slot text:

```text
campaign
```

It is user-editable and follows existing safe slot rules.

Save creates/deduplicates an immutable revision.

Snapshot shows:

```text
slot
status
revision count
last save result
```

### E3. Continue

Create a typed list from `GeneratedGameplaySaveService.List()`.

Statuses:

```text
CURRENT -> load exact session
PACKAGE_REBASE_REQUIRED / WORLD_MIGRATION_REQUIRED -> migration required
INVALID / LEGACY_RAW -> disabled
```

Exact load must not call Runtime Start/reset.

### E4. Migrate and continue

Use existing migration preview/apply.

Show human preview:

```text
position reset or preserved
preserved count
dropped count
source world title
target world title
```

Apply only after the user's explicit `Перенести и продолжить` action.

On success load the migrated session and return ACTIVE.

No automatic migration.

## F. Projects build-and-play

Add Projects-page button:

```text
Играть
```

State:

```text
TRAVEL_CURRENT -> Играть
valid generated source but stale/not built -> Собрать и играть
legacy/template/no project -> disabled with reason
operation busy -> disabled
```

`Собрать и играть`:

1. invoke existing `BuildAndQualify()` exactly once;
2. require GREEN and TRAVEL_CURRENT;
3. navigate to page `generated-campaign-player`;
4. do not start a session automatically;
5. do not run standalone.

A failed build remains on Projects and shows causal diagnostics.

## G. WinForms navigation

Create a minimal singleton navigation request service:

```text
IEditorPageNavigationService
EditorPageNavigationService
```

Contract:

```text
Request(pageId)
NavigationRequested event
```

`MainForm` subscribes and selects the matching registered page.

No direct MainForm reference in Projects or campaign page.

Unknown page IDs are ignored/logged without crashing.

Register the campaign page before Runtime Simulator in the page registry.


## H. Player workspace UI

Create:

```text
GeneratedCampaignPageControl.cs
GeneratedCampaignPageControl.Designer.cs
GeneratedCampaignMapControl.cs
GeneratedCampaignSavePickerDialog.cs
GeneratedCampaignSavePickerDialog.Designer.cs
```

Page identity:

```text
Id=generated-campaign-player
Title=Играть
SortOrder immediately before Runtime Simulator
```

### H1. Layout

At `1100x720` and normal Windows scaling:

```text
top bar:
  project/world/status
  Новая игра
  Продолжить
  Сохранить
  slot name
  collapsed Технические сведения

left:
  generated map viewport
  current region/map
  keyboard/movement help

center:
  current context
  dialogue/encounter/activity content
  context action buttons

right:
  tabs/cards:
    Персонаж
    Задания
    Инвентарь
    События
```

Requirements:

```text
no clipped primary controls
word wrap
scroll when content exceeds panel
map supports arbitrary finite dimensions
no horizontal page-level scrolling at 1100x720
```

### H2. Map interaction

Support:

```text
WASD
arrow keys
clicking a directly adjacent walkable cell
E or Enter for primary Interact
```

Keyboard shortcuts do not fire while editing text fields/dialogs.

Map rendering uses the pure map projection; it does not inspect package objects directly.

### H3. Actions

Use a scrolling FlowLayoutPanel or equivalent.

Buttons are rebuilt from `GeneratedCampaignAction`:

```text
button text = Title
tooltip/secondary label = Description
Tag = technical ActionId
```

Disabled reason is human-readable.

No raw IDs in button text.

### H4. Save picker

Dialog lists:

```text
slot
human status
saved world
current world
revision count
migration preserved/dropped summary
```

Actions:

```text
Продолжить
Перенести и продолжить
Отмена
```

Invalid/LEGACY_RAW entries cannot continue.

### H5. Technical details

Collapsed by default.

May show:

```text
session/project hashes
map/region/entity IDs
action IDs
raw event codes
diagnostics
```

Primary UI tests must prove those do not leak outside this section.

## I. Session and project changes

### I1. Page activation

On activation:

```text
refresh current project readiness
preserve active session only when its project truth is still current
otherwise mark STALE_PROJECT
do not auto-start
```

### I2. Build/regeneration while playing

Goal162 does not hold a long-lived project operation lease.

If project package/world/authoring changes:

```text
next refresh/action marks session stale
all Runtime action buttons disabled
Save/Restart/Migrate options remain truthful
```

No stale command executes.

### I3. RC/standalone immutability

During every Goal162 scenario:

```text
project-local RC record bytes unchanged
immutable standalone current pointer/run bytes unchanged
standalone history unchanged
Player/Unity/standalone Build counts=0
```

Gameplay saves may change as requested.

## J. Real automated product proof

Use a disposable real generated all-selectable project under a short root.

### J1. Project readiness and navigation

Require:

```text
Projects button shows Собрать и играть for source-ready stale project
one BuildAndQualify produces TRAVEL_CURRENT
navigation request opens Играть page
no session auto-start
```

Then reopen fresh and prove `Играть` without rebuild when current.

### J2. Start and map

Start a new session through `GeneratedCampaignSessionService`.

Require:

```text
generated start map/region
player shown on map
walkability/blocked cells accurate
human actor/cache/gate titles
valid actions only
primary page contains no raw IDs/hashes/paths
```

### J3. Dialogue

Navigate to a generated actor using player-facing move actions.

Interact:

```text
active dialogue appears
speaker/title/text human-readable
choices data-derived
choose Continue/actual generated choice
dialogue closes
```

No typed dialogue ID.

### J4. Encounter and quest

Select a generated current-region encounter by title.

Require:

```text
encounter starts
participant names/resources visible
player attack or ability succeeds
bounded turn/AI flow succeeds
encounter resolves through existing Runtime
generated reward enters inventory
quest objectives refresh
generated quest becomes completable
CompleteQuest succeeds
faction reputation changes
```

Do not call debug `AdvanceObjective`.

If the current generated fixture requires multiple actions, derive them from actual participant health,
ability power and turn state; no fixed action count in production.

### J5. Travel

Navigate to a generated travel gate through projected valid moves.

Require:

```text
gate action title names destination
Interact emits MapChanged
map/region title changes
destination generated interaction succeeds
visited regions/maps >=2
```

### J6. Save and exact continue

Save slot `campaign`.

Require:

```text
save status CURRENT
immutable revision written
page/session service recreated or cleared
Continue loads exact UnifiedRuntimeSession
same map/position/gameplay state/session hash
Runtime Start not called
```

### J7. World regeneration and migration

Using the existing real regeneration service:

```text
regenerate to a semantically different world
```

Require:

```text
save tree unchanged by regeneration
active campaign session becomes STALE_PROJECT
direct Continue rejected as migration required
picker shows human migration status
migration preview counts data-derived
Перенести и продолжить applies existing migration
new session ACTIVE on new generated start
map/transient reset truth shown
movement + travel + destination interaction succeed
```

Do not rebuild standalone or rewrite RC.

### J8. Core-only

Use a real current core-only generated project:

```text
start session
move/travel/interact
save CURRENT
exact continue
```

Require:

```text
campaign readiness does not depend on AcceptedMechanics.Passed
no false RC READY
```

### J9. Legacy/template

Require:

```text
Играть page explains generated campaign is unavailable
Projects play button disabled
technical Runtime Simulator behavior unchanged
legacy raw snapshot workflow unchanged
```

## K. Required behavioral tests

Create at least 50 Goal162 tests; at least 44 behavioral.

### Truth/lifecycle

1. valid TRAVEL_CURRENT generated project is READY;
2. source-only project is PROJECT_NOT_READY;
3. START_CURRENT is not ready;
4. LAST_SUCCESS is not ready;
5. legacy project is PROJECT_NOT_GENERATED;
6. StartNew captures exact truth;
7. same-project navigation preserves active session;
8. opening another project invalidates session;
9. package change marks stale before command;
10. world change marks stale before command;
11. authoring fingerprint change marks stale;
12. stale session executes zero Runtime commands.

### Projection

13. arbitrary map dimensions project correctly;
14. player/current region/map titles correct;
15. blocked and walkable cells correct;
16. actor title resolved without raw ID;
17. cache/item title resolved;
18. travel destination title resolved;
19. resources/stats/progression human rows;
20. inventory/equipment titles/counts;
21. active quests/objectives human projection;
22. primary projection has no raw IDs/hashes/absolute paths.

### Action planner/execution

23. only valid movement actions enabled;
24. blocked movement disabled causally;
25. nearby Interact targets actual entity;
26. dialogue mode shows choices and hides unrelated actions;
27. dialogue choice executes existing Runtime;
28. encounter list is current-region data-derived;
29. StartEncounter uses internal ID but human title;
30. attacks/abilities use data-derived participants;
31. bounded EndTurn/AI flow;
32. encounter reward enters inventory;
33. quest refresh runs at most once per causal command;
34. quest completes without debug objective advance;
35. faction reputation reward visible;
36. travel gate performs MapChanged;
37. destination interaction succeeds;
38. failed action preserves truthful returned state/events.

### Save/continue/migrate

39. save writes CURRENT generated revision;
40. identical save deduplicates;
41. exact continue does not call Runtime Start;
42. exact continue restores map/gameplay hashes;
43. migration-required direct continue rejected;
44. migration preview zero-write;
45. migrate-and-continue loads migrated session;
46. migrated session moves/travels/interacts;
47. invalid and LEGACY_RAW entries disabled;
48. active session becomes stale after real regeneration.

### UI/navigation

49. page registered as Играть before Runtime Simulator;
50. Projects shows Собрать и играть when stale;
51. Build-and-play builds once and navigates;
52. current project shows Играть without rebuild;
53. MainForm navigation request selects page;
54. unknown navigation request does not crash;
55. keyboard shortcuts map correctly and ignore text fields;
56. map adjacent click maps to move action;
57. save picker status/actions truthful;
58. 1100x720 primary layout has no clipped core controls;
59. primary controls contain no raw IDs/hashes/paths;
60. technical details retain technical truth.

### Regression/immutability

61. RC record bytes unchanged;
62. immutable standalone run/pointer/history unchanged;
63. Player/Unity/standalone Build counts zero;
64. core-only campaign works without AcceptedMechanics Passed;
65. Goal161T/S/R/Q/161 regressions GREEN;
66. Goal160/159/158/157 regressions GREEN;
67. Runtime Simulator existing tests GREEN;
68. generated save/migration tests GREEN;
69. operation coordinator tests GREEN;
70. Goal142 and goal148-manual byte-identical.

Do not claim counts unless tests are discovered and executed.


## L. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal162"
# require >=50 total and >=44 behavioral

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
dotnet test ... --filter "FullyQualifiedName~ProjectLifecycle"
dotnet test ... --filter "FullyQualifiedName~GameProjectOperationCoordinator"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleLibrary"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleCertification"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then execute the real all-selectable/core-only campaign route described in section J.

Run artifact scope last.

Do not run:

```text
full suite
85-case historical closure
all-ProductSmoke
Unity host build
standalone Build
Player executable
visible automatic application launch
```

A zero-match filter is failure.

## M. Evidence

Create exactly 14 files in each mirrored root:

```text
goal162-dashboard.json
architecture-review.json
goal161t-independent-audit-intake.json
campaign-session-truth-proof.json
campaign-map-context-proof.json
campaign-dialogue-proof.json
campaign-encounter-quest-proof.json
campaign-travel-proof.json
campaign-save-resume-migration-proof.json
projects-build-and-play-proof.json
campaign-ui-workspace-proof.json
regression-immutability-proof.json
artifact-scope-proof.json
goal162-report.md
```

Roots:

```text
.llmgc/procedural/goal-162-player-driven-generated-campaign-session-workspace/
.llmgc/exports/goal-162-player-driven-generated-campaign-session-workspace/
```

Twins byte-identical.

### Dashboard fields

```text
status
candidateStatus
goal162TestsDiscovered
goal162BehavioralTestsPassed

goal161tIndependentAuditPassed
goal160AuditBlockerClosed

campaignProjectReady
campaignStartPassed
campaignStartRegionTitle
campaignStartMapTitle
campaignPrimaryUiNoRawIds
campaignMapProjectionPassed
campaignValidMovementPassed

campaignActorInteractionPassed
campaignDialogueOpened
campaignDialogueChoicePassed
campaignDialogueClosed

campaignEncounterStarted
campaignAttackOrAbilityPassed
campaignEncounterTurnFlowPassed
campaignEncounterResolved
campaignRewardReceived
campaignQuestAutoStartPassed
campaignQuestRefreshBounded
campaignQuestCompleted
campaignReputationChanged

campaignTravelGateTitlePassed
campaignMapChangedPassed
campaignVisitedRegionCount
campaignVisitedMapCount
campaignDestinationInteractionPassed

campaignSaveCurrent
campaignSaveDeduplicated
campaignExactContinuePassed
campaignExactContinueStartInvocationCount
campaignSessionStaleAfterRegeneration
campaignMigrationPreviewPassed
campaignMigrationApplyPassed
campaignPostMigrationMovePassed
campaignPostMigrationTravelPassed
campaignPostMigrationInteractionPassed

projectsBuildAndPlayPassed
projectsBuildInvocationCount
projectsNavigationPassed
campaignPageRegistered
runtimeSimulatorUnchanged
legacyCampaignUnavailablePassed
coreOnlyCampaignPassed
coreOnlyNoFalseRcReady

releaseCandidateRecordByteIdentical
standaloneRunByteIdentical
standalonePointerByteIdentical
standaloneHistoryByteIdentical
playerProcessStartCount
unityEditorProcessStartCount
standaloneBuildInvocationCount

goal161tRegressionPassed
goal161sRegressionPassed
goal161rRegressionPassed
goal161qRegressionPassed
goal161RegressionPassed
goal160RegressionPassed
goal159RegressionPassed
goal158RegressionPassed
goal157RegressionPassed
generatedSaveRegressionPassed
runtimeSimulatorRegressionPassed

goal142SourceByteIdentical
sourceGoal148ByteIdentical
artifactScopeViolationCount
goal162Accepted=false
goal162ManualReviewRequired=false
goal162IndependentAuditRequired=true
```

No required GREEN value may be null/PARTIAL/NOT_EXECUTED.

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
docs/manual-acceptance/goal161-generated-gameplay-save-migration.md
docs/manual-acceptance/goal161t-immutable-payload-rc-closure.md
```

Create:

```text
docs/manual-acceptance/goal162-player-driven-generated-campaign-session.md
```

No human gate.

Required GREEN state:

```text
goal161tIndependentAuditResult=GREEN_ACCEPTABLE_CANDIDATE_AT_C1BD0B7A
goal161tIndependentAuditPassed=true
goal161tIndependentAuditRequired=false

goal161ImplementationStatus=GREEN
goal161CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal161QualificationStatus=GREEN
goal161Accepted=false
goal161IndependentAuditRequired=false

goal162ImplementationStatus=GREEN
goal162CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal162Accepted=false
goal162AcceptedByHuman=false
goal162AcceptedByCodex=false
goal162ManualReviewRequired=false
goal162ManualGateReady=false
goal162IndependentAuditRequired=true

goal162CampaignSessionPassed=true
goal162DialoguePassed=true
goal162EncounterQuestPassed=true
goal162TravelPassed=true
goal162SaveContinuePassed=true
goal162MigrationContinuePassed=true
goal162BuildAndPlayPassed=true
goal162CoreOnlyCampaignPassed=true
goal162PrimaryUiNoRawIds=true
goal162PlayerProcessStartCount=0
goal162UnityEditorProcessStartCount=0
goal162StandaloneBuildInvocationCount=0
goal162ArtifactScopeViolationCount=0

nextAction=independent_goal162_audit_and_plan_generated_campaign_consequence_depth
```

Release risk statement:

```text
Generated projects now have a player-facing WinForms campaign session over the existing qualified
Runtime. The page supports generated map exploration, dialogue, encounters, quests, travel and
save/resume/migration without exposing raw package IDs. This remains a development-player workspace;
consumer-grade Unity presentation, richer authored consequences and media remain future work.
```

Record the Goal161T finalizer operation-lease concern as P2.

## O. Text integrity

Scan actual changed/task/evidence/docs files:

```text
valid UTF-8
no NUL
no forbidden C0 except CR/LF/TAB
no mojibake markers
no escaped Cyrillic where policy forbids
no absolute disposable paths in committed evidence
```

Historical evidence immutable.

## P. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal162-generated-campaign-session.ps1
.devflow/scripts/run-goal162-generated-campaign-session.cmd

src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionModels.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionTruthService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignProjectionService.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignActionPlanner.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignEventPresenter.cs
src/LLMGameCreator.Application/Play/GeneratedCampaign/GeneratedCampaignSessionService.cs

src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/MainForm.cs
src/LLMGameCreator.WinForms/Pages/EditorPageNavigationService.cs
src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignPageControl.cs
src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignMapControl.cs
src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignSavePickerDialog.cs
src/LLMGameCreator.WinForms/Pages/GeneratedCampaign/GeneratedCampaignSavePickerDialog.Designer.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/Goal162/Goal162CampaignTruthProjectionTests.cs
tests/LLMGameCreator.Tests/Application/Goal162/Goal162CampaignDialogueTests.cs
tests/LLMGameCreator.Tests/Application/Goal162/Goal162CampaignEncounterQuestTests.cs
tests/LLMGameCreator.Tests/Application/Goal162/Goal162CampaignTravelTests.cs
tests/LLMGameCreator.Tests/Application/Goal162/Goal162CampaignSaveMigrationTests.cs
tests/LLMGameCreator.Tests/Application/Goal162/Goal162ProjectsNavigationTests.cs
tests/LLMGameCreator.Tests/Application/Goal162/Goal162WinFormsWorkspaceTests.cs
tests/LLMGameCreator.Tests/Application/Goal162/Goal162RegressionImmutabilityTests.cs
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
docs/manual-acceptance/goal161-generated-gameplay-save-migration.md
docs/manual-acceptance/goal161t-immutable-payload-rc-closure.md
docs/manual-acceptance/goal162-player-driven-generated-campaign-session.md

docs/agent-tasks/goal-162-player-driven-generated-campaign-session-workspace/
.llmgc/procedural/goal-162-player-driven-generated-campaign-session-workspace/
.llmgc/exports/goal-162-player-driven-generated-campaign-session-workspace/
```

One exact additional existing WinForms page-registry/navigation test path may be added after a concrete
compile/test failure with recorded reason.

Forbidden without a newly reproduced Goal162 P0/P1:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
catalogs/feature-modules/**
unity/**
generated source/overlay/travel implementation
generated save/migration implementation
standalone/RC implementation
```

Goal162 consumes those systems; it does not alter them.

## Q. Command budget

```text
read-first/architecture review: 14 minutes
campaign truth/projection/action models: 22 minutes
session execution/dialogue/encounter/quest/travel: 24 minutes
save/load/migration integration: 16 minutes
WinForms page/map/navigation: 24 minutes
behavioral tests: 30 minutes
real campaign matrix: 18 minutes
regressions/evidence/docs/artifact scope: 18 minutes
target wall clock: 135 minutes
maximum two concurrent testhost processes
Unity/Player/standalone counts: 0
```

Rules:

```text
write complete test inventory before production edits
write publication/evidence script before long real matrix
no unchanged command repetition
no timeout escalation
after failure run only exact class/test
P0/P1 fixed inside Goal162
P2/P3 debt only
do not defer evidence/docs/artifact scope
```

## R. Publication

Create exactly one final commit:

```text
GREEN Goal 162 player driven generated campaign session workspace
```

or honest BLOCKED/FAILED.

Codex performs standard push.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three Goal162 task files tracked
Goal142 and goal148-manual unchanged
RC/standalone bytes unchanged
Player/Unity/standalone counts=0
Goal161 accepted=false/no human gate
Goal162 accepted=false/no human gate
```

## S. GREEN criteria

```text
Goal161T independent audit recorded GREEN
Goal162 tests >=50 / behavioral >=44 / all pass

player-facing Играть page registered
Projects Играть / Собрать и играть truthful
generated campaign readiness and stale detection
human map/region/context projection without raw IDs
new session start
dialogue interaction/choice
generated encounter player turn flow
reward and quest objective refresh/completion
faction reputation consequence
generated region travel and destination interaction
save/current exact continue
world regeneration stale state
explicit save migration and continued play
core-only campaign works without false RC readiness
legacy project unavailable and Runtime Simulator unchanged
RC/standalone immutable
Player/Unity/standalone zero
required regressions GREEN
14+14 evidence
text integrity GREEN
artifact scope 0
goal162CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
one final commit pushed
```

## T. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning;
- architecture review;
- Goal161T audit intake;
- discovered/behavioral test counts;
- campaign readiness and project truth;
- page/navigation/build-and-play;
- map/context projection and no-raw-ID proof;
- dialogue route;
- encounter/quest/reward/reputation route;
- travel route;
- save/exact continue;
- regeneration stale detection and migrate/continue;
- core-only and legacy results;
- RC/standalone immutability and zero process counts;
- focused regressions;
- evidence/text/artifact scope;
- state/no-human-gate;
- final SHA/push/HEAD/worktree.
