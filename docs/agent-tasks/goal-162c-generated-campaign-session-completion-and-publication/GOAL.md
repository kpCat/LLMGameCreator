# Goal 162C — Generated Campaign Session Completion & Publication

## Identity

- Task ID: `goal-162c-generated-campaign-session-completion-and-publication`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `92d419862443e84e2bfafa02f1a985edd96a5889`
- Required base message: `Add generated campaign player navigation`
- Original task: `docs/agent-tasks/goal-162-player-driven-generated-campaign-session-workspace/GOAL.md`

This is a new isolated Codex dialog. This continuation finishes the already-pushed partial Goal162.
Read this file first, then the original Goal162 GOAL.md. This file overrides the original only where
it explicitly says so.

## Recommended Codex configuration

```text
Model: GPT-5.6 Sol
Reasoning effort: Extra High
```

Reason: the visible product scope remains large, but an incomplete Application/WinForms skeleton is
already in `main`. The task now requires architecture correction, real Runtime orchestration,
save/migration integration, WinForms completion, full product evidence and publication discipline.
This is no longer a routine mechanical continuation.

## Owner decision and publication contract

The owner manually committed and pushed the partial scaffold because the previous Codex stopped after
14 minutes and refused to publish incomplete work.

Record:

```text
goal162PartialManualBase=92d419862443e84e2bfafa02f1a985edd96a5889
goal162PartialBaseStatus=FAILED_INCOMPLETE_MANUALLY_PUBLISHED
goal162PartialBaseAccepted=false
```

### Absolute publication rule

This continuation **must create and push exactly one final commit regardless of outcome**.

Allowed final messages:

```text
GREEN Goal 162 player driven generated campaign session workspace
BLOCKED Goal 162 player driven generated campaign session workspace
FAILED Goal 162 player driven generated campaign session workspace
```

Forbidden:

```text
refusing to commit because the implementation is partial
leaving product changes uncommitted
telling the user to commit or push manually
ending without a final SHA and push attempt
```

On BLOCKED/FAILED:

```text
commit all honest implementation, tests, diagnostics, evidence and state
record exactly what is incomplete
push to origin/main
leave worktree clean
```

No intermediate commits.

## Initial worktree

After unpacking, only these new untracked files are allowed:

```text
docs/agent-tasks/goal-162c-generated-campaign-session-completion-and-publication/GOAL.md
docs/agent-tasks/goal-162c-generated-campaign-session-completion-and-publication/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-162c-generated-campaign-session-completion-and-publication/README.md
```

Require:

```text
HEAD == origin/main == 92d419862443e84e2bfafa02f1a985edd96a5889
branch=main
no other tracked/staged/untracked changes
```

Never reset, revert, stash, merge or rebase the partial commit.

## Partial-base audit

The pushed base contains:

```text
original Goal162 task files
architecture-review draft
campaign Application models/services skeleton
navigation service
WinForms Играть page skeleton
Projects play action skeleton
CompositionRoot/MainForm wiring
one Goal162 test file
minimal Goal162 runner
```

It does not contain the required completion truth:

```text
real dialogue campaign route
real encounter turn/ability/AI/resolve route
reward/inventory consequence
quest refresh/completion/reputation route
real travel route
real regeneration stale-session detection
real save migration and continued play
core-only full route
legacy/runtime-simulator compatibility proof
complete UI/layout/no-raw-ID proof
14×2 evidence
current state/docs publication
artifact-scope closure
required focused regressions
```

Several production files are only scaffolds. Do not treat class/file existence as implementation.

## Preserve or replace deliberately

For every partial production file, classify before editing:

```text
KEEP_AND_COMPLETE
REFACTOR
REPLACE
REMOVE_AS_UNUSED
```

Write the classification to:

```text
.llmgc/procedural/goal-162c-generated-campaign-session-completion-and-publication/partial-base-audit.json
```

Required fields per file:

```text
path
classification
currentImplementedBehavior
missingBehavior
testsThatWillProveCompletion
```

Do not delete the entire scaffold and restart blindly. Do not preserve placeholder code merely because
it compiles.

## Execution budget

```text
Unity Editor starts: 0
Unity host builds: 0
standalone Build calls: 0
Player process starts: 0
visible application auto-launch: 0
manual user test: 0
```

Preserve project-local RC, immutable standalone run/current pointer and standalone history bytes.

## Goal outcome

Finish the original Goal162 product outcome completely:

```text
Projects → Играть / Собрать и играть
Играть page
new campaign session
human map/context/HUD
dialogue
encounter
reward
quest completion
reputation consequence
generated-region travel
save
exact continue
stale session after regeneration
explicit save migration
continued play in new world
core-only compatibility
legacy project unavailable
```

No raw package IDs are required in primary UI.

## Mandatory first phase

Within the first 15 minutes:

1. verify base/worktree;
2. read original Goal162 GOAL.md;
3. inspect every pushed Goal162 production/test file;
4. write `partial-base-audit.json`;
5. discover actual Goal162 tests;
6. run `dotnet build`;
7. run the current Goal162 filter once;
8. write a completion plan mapping every original mandatory route to exact types/tests.

Do not spend this phase rewriting docs or evidence.

## Application completion

### 1. Campaign truth and session lifecycle

`GeneratedCampaignSessionTruthService` must provide the original exact readiness contract:

```text
strict generated source
TRAVEL_CURRENT
activation/travel passed
current package/document hashes
current authoring fingerprint
world/package/composition identity
```

`GeneratedCampaignSessionService` must be a real singleton orchestration service, not a thin wrapper.

Required methods, naming flexible:

```text
Refresh()
StartNew()
Execute(actionId)
Save(slot)
ListSaves()
Continue(slot)
PreviewMigration(slot)
MigrateAndContinue(preview)
ClearSession()
```

Every action recaptures project truth. World/package/authoring drift marks `STALE_PROJECT` and executes
zero Runtime commands.

### 2. Projection

Complete human projections for:

```text
arbitrary finite map
player position
walkability and blockers
actor/cache/travel-gate titles
current map and region
resources/stats/progression
inventory/equipment
quests/objectives/progress
dialogue speaker/text/choices
encounter participants/turn/resources
faction reputation
recent events
save status
```

Raw IDs/hashes/absolute paths only in TechnicalDetails.

### 3. Action planner

Generate data-derived actions from current package/session:

```text
valid movement only
nearby interact
dialogue choices
encounter selection by human title
basic attack and abilities with human targets
end-turn bounded AI orchestration
resolve/flee when valid
complete quest only when objectives complete
save/load/migrate/restart
```

Action IDs may be opaque internal tokens.

### 4. Runtime execution

Use only existing `IUnifiedGameRuntimeService` and existing commands.

Required bounded orchestration:

```text
dialogue interaction → choose → close
encounter start → player attack/ability → end turn → bounded AI → resolve
causal RefreshQuestObjectives at most once
quest completion without AdvanceQuestObjective
travel gate Interact → MapChanged
```

Do not modify Runtime or GamePackage.

### 5. Save and migration

Use existing generated save services.

Require:

```text
exact CURRENT load without Runtime Start
migration-required direct load rejected
preview zero-write
explicit apply
migrated session ACTIVE
post-migration move/travel/destination interaction
```

### 6. Build-and-play

Projects page:

```text
TRAVEL_CURRENT → Играть
generated stale → Собрать и играть
legacy/template → disabled with human reason
busy operation → disabled
```

Build-and-play invokes `BuildAndQualify()` once and requests navigation only after GREEN/TRAVEL_CURRENT.

## WinForms completion

The pushed controls are placeholders and must become functional.

At `1100x720`:

```text
top: status, Новая игра, Продолжить, Сохранить, slot, technical toggle
left: map + region/map title + controls hint
center: dialogue/encounter/activity context + dynamic actions
right: tabs for character, quests, inventory, events
```

Required:

```text
WASD/arrows
adjacent-cell click
E/Enter primary interaction
shortcuts ignored in text input/dialogs
scrollable dynamic content
no clipped primary controls
no page-level horizontal scroll
technical section collapsed by default
```

Save picker must support Continue and Migrate-and-continue based on real statuses.

Runtime Simulator remains unchanged.

## Mandatory real product matrix

Use real disposable generated projects.

### All-selectable route

Prove in order:

```text
source-ready stale project shows Собрать и играть
one build → navigation
page does not auto-start
new session
generated map/region/player
actor interaction
dialogue opened/choice/closed
current-region encounter started
player attack or ability
bounded turn/AI
encounter resolved
reward in inventory
quest objectives refreshed
quest completed without debug advance
reputation changed
path to gate
human destination title
MapChanged
destination interaction
save campaign CURRENT
clear/recreate session
exact continue with Runtime Start count 0
regenerate semantically different world
active session becomes stale before command
save status requires migration
preview/apply migration
new session active at target start
post-migration move/travel/interaction
```

### Core-only route

```text
TRAVEL_CURRENT readiness
new session
move/travel/interact
save CURRENT
exact continue
AcceptedMechanics remains incomplete
no false RC READY/PENDING/CURRENT
```

### Legacy route

```text
campaign unavailable
Projects play disabled
Runtime Simulator/raw snapshot behavior unchanged
```

## Tests

The base claims 62 discovered cases, but discovery count is not completion.

Final requirements:

```text
Goal162 discovered >=60
Goal162 behavioral passed >=52
all discovered Goal162 tests pass
```

Create the missing focused files from the original task:

```text
Goal162CampaignDialogueTests.cs
Goal162CampaignEncounterQuestTests.cs
Goal162CampaignTravelTests.cs
Goal162CampaignSaveMigrationTests.cs
Goal162ProjectsNavigationTests.cs
Goal162WinFormsWorkspaceTests.cs
Goal162RegressionImmutabilityTests.cs
```

The original `Goal162CampaignTruthProjectionTests.cs` may be completed/refactored.

Tests must cover real service state and filesystem behavior. Source-string assertions do not count as
behavioral completion.

## Regression matrix

Run exactly the original required filters:

```text
Goal162
Goal161T
Goal161S
Goal161R
Goal161Q
Goal161
Goal160
Goal159
Goal158
Goal157
RuntimeSimulator
GeneratedGameplaySave
DefaultGameRuntime
UnifiedGameProjectWorkspace
ProjectsPage
ProjectLifecycle
GameProjectOperationCoordinator
FeatureModuleLibrary
FeatureModuleCertification
```

Then:

```text
run-capability-runtime-equipment-slice.ps1
run-character-attributes-level-progression-slice.ps1
check-current-goal.ps1
```

No full suite, 85-case closure or all-ProductSmoke.

A zero-match filter is failure.

## Immutability proof

Before real matrix hash:

```text
RC record
immutable standalone run tree
current pointer
standalone history
Goal142 source
goal148-manual source
```

After matrix require byte-identical.

Generated gameplay save changes are allowed only in disposable product fixtures.

Record:

```text
Player starts=0
Unity starts=0
standalone Build calls=0
```

## Evidence

Create exactly 14 files in each mirrored root:

```text
goal162-dashboard.json
architecture-review.json
partial-base-audit.json
goal161t-independent-audit-intake.json
campaign-session-truth-proof.json
campaign-map-context-proof.json
campaign-dialogue-proof.json
campaign-encounter-quest-proof.json
campaign-travel-proof.json
campaign-save-resume-migration-proof.json
projects-build-and-play-proof.json
campaign-ui-workspace-proof.json
regression-immutability-artifact-scope-proof.json
goal162-report.md
```

Roots:

```text
.llmgc/procedural/goal-162-player-driven-generated-campaign-session-workspace/
.llmgc/exports/goal-162-player-driven-generated-campaign-session-workspace/
```

The original procedural architecture-review file may be replaced with the completed version.
Twins must be byte-identical.

Dashboard must contain all original Goal162 fields plus:

```text
partialManualBaseSha
partialBaseAudited
partialScaffoldCompleted
publicationContractPassed
```

No GREEN-required field may be null/PARTIAL/NOT_EXECUTED.

## Docs/state

Complete every original Goal162 docs/state update.

Record the manual partial commit honestly:

```text
goal162PartialManualBase=92d419862443e84e2bfafa02f1a985edd96a5889
goal162PartialBaseStatus=FAILED_INCOMPLETE_MANUALLY_PUBLISHED
goal162PartialBaseClosedBy=goal162c
```

On GREEN:

```text
goal162ImplementationStatus=GREEN
goal162CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal162Accepted=false
goal162ManualReviewRequired=false
goal162IndependentAuditRequired=true
nextAction=independent_goal162_audit_and_plan_generated_campaign_consequence_depth
```

No human gate.

## Artifact scope

Use the original Goal162 allowed paths plus:

```text
docs/agent-tasks/goal-162c-generated-campaign-session-completion-and-publication/
.llmgc/procedural/goal-162c-generated-campaign-session-completion-and-publication/ only for continuation-specific intake if needed
.llmgc/exports/goal-162c-generated-campaign-session-completion-and-publication/ only for continuation-specific intake if needed
```

Preferred final evidence remains under the original Goal162 roots.

Update artifact-scope policy before final validation.

Forbidden remain:

```text
Runtime
Runtime.Abstractions
GamePackage schema
FeatureModule catalog
Unity
generated source/overlay/travel
generated save/migration implementation
standalone/RC implementation
```

## Command budget

```text
partial-base audit and current tests: 15 minutes
Application truth/projection/actions: 22 minutes
dialogue/encounter/quest/travel: 28 minutes
save/migration/build-and-play: 18 minutes
WinForms completion: 24 minutes
behavioral tests: 30 minutes
real matrix: 18 minutes
regressions/evidence/docs/scope: 20 minutes
target wall clock: 150 minutes
maximum two testhost processes
```

Rules:

```text
do not stop after creating scaffolds
do not count compile success as product completion
do not skip real routes because unit skeletons pass
no unchanged command retries
no timeout escalation
after failure isolate exact class/test
```

## Final quality gate

GREEN only when:

```text
all mandatory real routes pass
Goal162 >=60 discovered / >=52 behavioral / all pass
required regressions pass
14×2 evidence complete
docs/state complete
artifact scope 0
RC/standalone immutable
Player/Unity/standalone counts 0
one final GREEN commit pushed
```

Otherwise publish honest BLOCKED/FAILED commit and push it.

## Final report

Include:

- model/reasoning;
- exact initial base and manual-partial intake;
- partial scaffold classifications;
- build/test inventory before completion;
- completed Application services;
- completed WinForms page/navigation;
- all-selectable real route;
- core-only route;
- legacy route;
- save/continue/migration;
- no-raw-ID and layout proof;
- immutability/process counts;
- test and regression counts;
- evidence/docs/artifact scope;
- final status;
- final SHA;
- push result;
- HEAD==origin/main;
- clean worktree;
- explicit confirmation that Codex, not the user, committed and pushed.
