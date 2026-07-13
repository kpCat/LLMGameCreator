# Goal 153 — Active Abilities, Mana & Turn-Status FeatureModules Runtime/Standalone Vertical Slice

## Identity

- Task ID: `goal-153-active-abilities-mana-turn-status-featuremodules-runtime-standalone-vertical-slice`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `ac97859c8de861641e07f886250d053b5330fbe9`
- Required base message: `GREEN Goal 152C exact Unity generated settings cleanup and external workspace closure`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Sol
Reasoning effort: High
```

Reason: this Goal activates turn-status Runtime semantics from the existing GamePackage schema while integrating three data-driven FeatureModules, typed parameters, save/replay, WinForms and standalone payloads. It is bounded product architecture; Extra High is not required.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal plan, but do not ask for confirmation.
- Begin after base/worktree checks.
- Do not ask the owner for intermediate manual testing.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No validation-candidate commits.

## Unity execution budget

```text
Unity Editor invocation budget: 0
Cached standalone hidden-smoke budget: 1
```

Read and obey `docs/UNITY_EXECUTION_POLICY.md`.
Do not modify Unity-host source. Reuse the existing generic host cache. Starting `Unity.exe` is a P1 violation.

## Required first deliverable — record human acceptance

Record the exact owner statement:

```text
Я принимаю Goals152/152A/152C: standalone показал зелёную автопроверку, интерфейс читаемый, кнопки Далее/Назад/В конец/Сбросить работают, текст обновляется без наложения; host cache переиспользован без запуска Unity Editor.
```

Required state:

```text
goal152Accepted=true
goal152AcceptedByHuman=true
goal152AcceptedByCodex=false
goal152ManualReviewPerformed=true

goal152aAccepted=true
goal152aAcceptedByHuman=true
goal152aAcceptedByCodex=false
goal152aManualReviewPerformed=true

goal152cAccepted=true
goal152cAcceptedByHuman=true
goal152cAcceptedByCodex=false
goal152cManualReviewPerformed=true

goal152bAccepted=false
goal152bImplementationStatus=BLOCKED historical cleanup attempt

acceptedCommit=ac97859c8de861641e07f886250d053b5330fbe9
standaloneSelfCheckAccepted=true
standaloneReadable=true
standaloneNavigationAccepted=true
standaloneGhostingAbsent=true
hostCacheReusedWithoutUnityEditor=true
rawManualInputNotCommitted=true
```

Update the three relevant manual-acceptance documents, `CURRENT_GENERATOR_STATE.*`, `CONTEXT_INDEX.md` and `MILESTONE_GATES.md`. Write one compact normalized acceptance record under Goal153 procedural/export roots.

Goal153 itself remains `accepted=false`, `acceptedByHuman=false`, `acceptedByCodex=false`, `manualReviewPerformed=false` until a later explicit human decision.

## Product objective

Add three default-off, independently composable optional FeatureModules to the normal project flow:

```text
feature.combat.active_ability_loadout
  title: Активные способности

feature.magic.mana_spellcasting
  title: Мана и заклинания

feature.status.turn_effects
  title: Эффекты по ходам
```

Required user flow:

```text
Игры → Механики → select modules → Настройки → edit typed values → save/reopen
→ Собрать и проверить игру
→ configured active ability executes
→ mana is consumed
→ status is applied
→ status definition effects execute over turns
→ status expires
→ checkpoint/full replay remain equivalent
→ standalone payload exposes human-readable results
```

This must be a reusable capability bundle, not C# code for one spell.

## Architectural constraints

- Abilities, resources, statuses, effects and their counts are data.
- Runtime code branches only on generic effect/cost/command kinds, never on module ID, ability ID, status ID, project ID or Goal number.
- Fixture IDs and numeric values are regression examples, not product limits.
- Reuse existing GamePackage `abilities`, `resources`, `statuses`, costs and effects. No parallel public schema.
- Do not add one C# class per spell or status.
- New modules are default-off and do not alter existing projects until selected.
- Numeric values are user-defined through typed parameters.
- No fixed maximum number of abilities/statuses is introduced by Runtime architecture.
- Parameter validation ranges are authoring safeguards, not content-count limits.

## Existing platform facts

The Runtime already supports `UseAbility`, resource costs, damage/healing, `add_status`/`remove_status`, `StatusState.RemainingTicks`, status expiry, participant resources and encounter save/replay.

`StatusDefinition.Effects` exists but is not currently executed when a status ticks. Goal153 must activate this existing schema seam generically.

## Module A — active ability loadout

Create:

```text
catalogs/feature-modules/optional/combat-active-ability-loadout.featuremodule.json
```

Definition:

```text
moduleId=feature.combat.active_ability_loadout
defaultSelected=false
dependencies:
  feature.combat.turn_based_encounter
  feature.player_adapter.runtime_summary
```

The module declaratively adds/upserts:

```text
one fixture active ability definition
player encounter participant ability reference
Runtime playthrough contract that uses the configured ability
PlayerAdapter ability summary
```

Suggested fixture:

```text
ability ID: ability/arcane_impulse
human title: Магический импульс
kind: attack
target: hostile participant
direct resource damage
```

The exact fixture ID may change after repository discovery, but it remains stable data and is never referenced by Runtime branching.

Typed parameter:

```text
abilityBaseDamage
Title: Базовый урон способности
Default fixture value: 2
```

Bind atomically to package ability output/power, Runtime-effect expected value, playthrough expectation, build summary and standalone human-review fact.

## Module B — mana spellcasting

Create:

```text
catalogs/feature-modules/optional/magic-mana-spellcasting.featuremodule.json
```

Definition:

```text
moduleId=feature.magic.mana_spellcasting
defaultSelected=false
dependencies:
  feature.combat.active_ability_loadout
```

Declaratively add/upsert:

```text
resource/mana or equivalent data-defined resource
player encounter participant mana state
resource cost on the active ability
spell/magic tags or metadata
mana summary surface
```

Typed parameters:

```text
startingMana
Title: Начальная мана
Default: 12

abilityManaCost
Title: Стоимость способности
Default: 3
```

Required behavior:

```text
sufficient mana -> ability succeeds, cost event emitted, mana decreases exactly
insufficient mana -> failure, original state byte-equivalent, turn unchanged
```

## Module C — turn-based statuses

Create:

```text
catalogs/feature-modules/optional/status-turn-effects.featuremodule.json
```

Definition:

```text
moduleId=feature.status.turn_effects
defaultSelected=false
dependencies:
  feature.combat.active_ability_loadout
```

Declaratively add/upsert:

```text
one fixture status definition
add_status effect on the fixture active ability
status definition turn effects
status summary and Runtime semantic contracts
```

Suggested fixture:

```text
status ID: status/arcane_burn
human title: Магическое горение
turn effect: damage target health
```

Typed parameters:

```text
statusDurationTurns
Title: Длительность эффекта в ходах
Default: 2

statusTickDamage
Title: Урон эффекта за ход
Default: 1
```

The defaults are not architecture limits.

## Generic definition-upsert capability

Existing scalar mutations are insufficient when a module adds a new ability/resource/status. Extend the internal FeatureModule mutation layer with a generic deterministic definition-upsert contract.

Required collections:

```text
game.abilities
game.resources
game.statuses
encounter participant ability references
encounter participant resource entries
```

Required behavior:

```text
missing definition -> add
byte-equivalent existing definition -> idempotent
conflicting existing definition -> causal failure
reverse module order -> byte-identical package
```

Implementation naming is flexible. Forbidden: module-ID switches, fixture-ID switches, or raw JSON editing in WinForms.

## Runtime status-effect semantics

### Application provenance

When `add_status` is applied, persist optional safe metadata:

```text
sourceParticipantId
sourceAbilityId
appliedRound
```

Old saves/statuses without these fields remain valid.

### Deterministic tick policy

Use and document this policy:

```text
Status definition effects execute when the affected participant's turn is ended,
before RemainingTicks is decremented and before turn advancement.
```

An equivalent timing is acceptable only if it is consistently documented, tested and replay-stable.

### Tick execution

For each active status on the current participant:

1. resolve `StatusDefinition` by ID;
2. clone working state before mutation;
3. map each `StatusDefinition.Effects` through existing effect/output semantics;
4. apply effects to the affected participant;
5. emit a structured status-tick event plus underlying effect events;
6. decrement `RemainingTicks`;
7. remove and emit expiry at zero.

Support at least generic damage, healing, resource change, stat change and log effects where valid.

Unknown/missing status definitions or effects must fail transactionally with no partial mutation, no turn/round advance and no duration decrement.

### Duplicate application

Define deterministic default behavior:

```text
same status + same target -> refresh duration, keep one logical status
```

Optional stack metadata may support bounded stacks later, but repeated application may not create uncontrolled duplicates.

### Save/replay

A checkpoint taken after application and before expiry preserves status ID, target, optional provenance, remaining ticks, stacks, mana, resources/stats and turn/round cursor. Reload plus continuation must match uninterrupted event sequence and final hash.

## Capability-driven playthrough

Add playthrough contracts through module definitions and generic planner/executor seams.

Required configured sequence with all three modules:

```text
start encounter
use configured active ability
observe direct damage
observe mana cost
observe status added
advance turns until one status tick is observed
checkpoint while status remains active
reload checkpoint
continue until status expires
show ability/mana/status summary
show final state
```

Do not hardcode a product-wide total action count. Keep the test target alive long enough to observe tick and expiry; adjust only fixture values if baseline health requires it.

Required bindings:

```text
ability ID from module contract
source/target selectors resolve actual participants
mana/status references resolve package definitions
no fallback to basic attack for the active-ability action
```

## Runtime evidence and result models

Add stable observations and summaries without hardcoding fixture IDs in generic code.

Required observations include:

```text
active ability used
configured direct damage observed
mana before/cost/after observed
status applied with configured duration
status tick damage observed
status remaining ticks observed
status expired
checkpoint continuation equivalent
```

Extend generic Runtime-effect metric vocabulary as needed, for example:

```text
ability_direct_damage_equals
participant_resource_equals
status_present
status_remaining_ticks_equals
status_tick_damage_equals
status_absent_after_expiry
```

Names may differ, but every selected module must have at least one concrete Runtime observation and
`satisfiedSelectedModuleCount` must include all selected modules.

Add build-result fields/summaries such as:

```text
Ability summary
Mana summary
Status summary
Ability direct damage
Mana spent
Mana remaining
Status tick damage
Status remaining ticks
Status expired
```

These are attempt-specific Runtime facts, not Application-side gameplay calculations.

## Normal Games UI

In `Игры → Механики`, expose the three modules in Russian with clear descriptions and dependencies.

In `Настройки`, expose typed controls for all five parameters.

Required UX:

```text
selecting mana/status clearly resolves or explains active-ability dependency
deselecting a dependency cannot silently leave invalid selected state
invalid numbers show causal validation and do not mutate the package
Save/close/reopen preserves selections and values
```

After GREEN build, the primary result should contain a concise human card:

```text
Способность: <name>
Прямой урон: <configured/observed>
Мана: <before> → <after> (стоимость <cost>)
Эффект: <status name>, <duration> ходов
Урон эффекта: <tick damage> за ход
Эффект завершён: да
Сохранение/повтор: пройдено
```

Raw IDs, hashes and detailed events remain in Technical Details.

Do not require the user to inspect all event rows manually.

## Standalone integration — cache reuse only

Do not change:

```text
unity/LLMGameCreatorAlpha/**
```

Use the existing generic payload `humanReviewFacts`/frame mechanism.

Add data-driven standalone facts:

```text
Способность
Прямой урон
Начальная мана
Потрачено маны
Осталось маны
Эффект
Длительность
Урон за ход
Эффект завершён
```

The standalone payload must include the new Runtime frames from the current project action journal.
The existing host cache must be reused:

```text
HostReused=true
HostRebuilt=false
Unity process start count=0
hidden smoke all required markers GREEN
```

If cache is unavailable, publish BLOCKED. Do not run Unity Editor.

## Existing-project lifecycle and migration

Use a disposable copy of the accepted `goal148-manual` project.

Required sequence:

```text
open existing accepted project
new modules visible and default-off
build unchanged
accepted Goals149/150 hashes and results unchanged
select three Goal153 modules
set custom fixture values
save
close/reopen
build
checkpoint/reload/replay
standalone payload assembly using existing cache
change one Goal153 parameter
rebuild
prove package/final hash changes while old module selections persist
```

Original source project remains byte-identical.

Suggested custom fixture:

```text
abilityBaseDamage=2
startingMana=12
abilityManaCost=3
statusDurationTurns=2
statusTickDamage=1
```

These numbers are test inputs only. The product must use the user-selected values.

## Compatibility requirements

### Disabled path

With all Goal153 modules disabled, preserve the accepted current project behavior and exact historical
hashes where the existing regression fixtures require them:

```text
Goal149 disabled/equipment paths
Goal150 all-optional defaults
Goal150A custom 3/8/2/12
Goal151/152 current-project qualification behavior
```

Do not change the old hashes merely because catalog modules were added default-off.

### Composition matrix

Required focused combinations:

```text
active ability only
active ability + mana
active ability + status
active ability + mana + status
all Goal153 modules + equipment + attributes + progression
all current optional modules
```

Do not enumerate every powerset.
Use pairwise/dependency-focused coverage.

### Independence

Required:

```text
mana cost does not affect basic attack
status ticking does not add equipment/stat bonus unless data explicitly requests it
basic attack equipment/stat totals remain 3/6/9 in accepted fixture
progression remains 2/12
unselected Goal153 modules have zero Runtime evidence and zero package mutation
```

## Negative tests

Required:

```text
unknown ability reference rejected
unknown mana resource reference rejected
unknown status definition rejected
conflicting definition upsert rejected
insufficient mana leaves state unchanged
invalid negative duration/tick damage rejected by authoring
status tick unknown effect kind leaves state unchanged
status tick missing participant resource leaves state unchanged
repeated status application follows refresh policy
module dependency drift rejected or repaired through explicit UI action
package changed after qualification rejected by standalone assembly
```

No test may weaken existing validation to get GREEN.

## Manual gate policy

Goal153 automated evidence should cover all exact values, hashes, events and replay equivalence.

The later human gate must be short:

```text
1. Open existing project.
2. Enable the three new mechanics and confirm their settings appear.
3. Build and confirm one concise ability/mana/status GREEN summary.
4. Save, reopen and confirm the values remain.
```

Standalone is automated for this Goal; do not require another human standalone review unless the
standalone presentation code is changed, which is forbidden here.

## Command and investigation budget

Mandatory:

```text
read-first: maximum 12 primary files
architecture/root discovery before first edit: maximum 8 minutes
.NET build + Goal153 focused tests: maximum 15 minutes
Runtime/save/replay matrices: maximum 12 minutes
real-project lifecycle: maximum 8 minutes
cached standalone hidden smoke: maximum 3 minutes
total target wall clock: 45 minutes
maximum two testhost processes
Unity Editor process count: 0
```

Rules:

- Do not repeat an unchanged failing command.
- Rerun only affected focused tests after a concrete fix.
- No full suite.
- No 85-case historical closure.
- No all-ProductSmoke sweep.
- No repair of historical snapshot debt.
- Raw logs/TRX remain ignored under `.devflow/runs` or LocalAppData.
- Commit compact evidence only.

## Required validation

### T0

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal153"
dotnet test ... --filter "FullyQualifiedName~EncounterRuntimeService"
dotnet test ... --filter "FullyQualifiedName~CapabilityDrivenRuntimePlaythrough"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
```

### Accepted regressions

```powershell
.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

### Goal153 command

Add:

```text
.devflow/scripts/run-goal153-active-abilities-mana-status-slice.ps1
.devflow/scripts/run-goal153-active-abilities-mana-status-slice.cmd
```

It runs only bounded Goal153/accepted regression lanes and one cached hidden standalone smoke.

## Artifact scope

Add an exact Goal153 scenario.

Initially allowed:

```text
catalogs/feature-modules/optional/combat-active-ability-loadout.featuremodule.json
catalogs/feature-modules/optional/magic-mana-spellcasting.featuremodule.json
catalogs/feature-modules/optional/status-turn-effects.featuremodule.json

src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionService.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionPlanner.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryLoader.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterBindingService.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterizedCompositionService.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/**
src/LLMGameCreator.Application/Design/ProductLineRuntimeQualification/**
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildModels.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildService.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs

src/LLMGameCreator.Runtime/EncounterRuntimeService.cs
src/LLMGameCreator.Runtime/RuntimeStateHelpers.cs
src/LLMGameCreator.Runtime.Abstractions/RuntimeContracts.cs

.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal153-active-abilities-mana-status-slice.ps1
.devflow/scripts/run-goal153-active-abilities-mana-status-slice.cmd

tests/LLMGameCreator.Tests/Application/Goal153/**
tests/LLMGameCreator.Tests/Runtime/Goal153/**
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal153AbilityManaStatusWorkspaceTests.cs
tests/LLMGameCreator.Tests/WinForms/Goal153ProjectsPageAbilityManaStatusTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal153ActiveAbilitiesManaStatusScriptTests.cs

AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/project-scoped-windows-standalone-build-launch.md
docs/manual-acceptance/standalone-playeradapter-ux-framebuffer-refresh-hotfix.md
docs/manual-acceptance/exact-unity-generated-settings-cleanup-and-external-workspace-closure.md
docs/manual-acceptance/active-abilities-mana-turn-status-featuremodules.md

docs/agent-tasks/goal-153-active-abilities-mana-turn-status-featuremodules-runtime-standalone-vertical-slice/**
.llmgc/procedural/goal-153-active-abilities-mana-turn-status-featuremodules-runtime-standalone-vertical-slice/**
.llmgc/exports/goal-153-active-abilities-mana-turn-status-featuremodules-runtime-standalone-vertical-slice/**
```

If exact discovery proves another file is required:

1. record the concrete reason;
2. add only the exact path;
3. do not add broad Runtime/GamePackage prefixes silently.

Forbidden:

```text
unity/LLMGameCreatorAlpha/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
Lua/Scripting provider code
public GamePackage format-version change
```

## Compact evidence

Maximum 12 files per root:

```text
accepted-goal152-family-record.json
goal153-dashboard.json
featuremodule-definitions-proof.json
parameter-synchronization-proof.json
runtime-ability-mana-status-proof.json
status-transactionality-proof.json
checkpoint-replay-proof.json
real-project-lifecycle-proof.json
standalone-cache-reuse-proof.json
focused-regression-proof.json
artifact-scope-proof.json
goal153-report.md
```

Do not commit copied user projects, standalone output, host cache, screenshots, raw logs or TRX.

## Publication policy

Create exactly one final commit:

```text
GREEN Goal 153 active abilities mana and turn-status FeatureModules vertical slice
```

or honest BLOCKED/FAILED.

Codex must push it.

Required final state:

```text
HEAD == origin/main
worktree clean
Unity process start count=0
Goals152/152A/152C acceptedByHuman=true
Goal153 accepted=false
Goal153 manualGateReady=true only on GREEN
```

## GREEN criteria

```text
Goals152/152A/152C acceptance recorded exactly
three new modules visible/default-off/data-driven
five typed parameters persist through save/reopen
ability direct damage uses configured value
mana cost uses configured value and is transactional
status definition effects tick generically
status duration decrements and expires
status failure is transactional
checkpoint/reload/full replay equivalent
module order independence preserved
existing accepted hashes/results preserved when disabled
all focused compatibility combinations GREEN
normal Games UI concise and human-readable
standalone host reused with zero Unity Editor starts
hidden smoke GREEN
artifact scope 0 violations
Goal153 manualGateReady=true
one final commit pushed
```

## Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning used;
- acceptance-record fields;
- module IDs/titles/dependencies;
- configured custom fixture values;
- direct damage, mana before/cost/after;
- status applied/ticked/expired evidence;
- checkpoint/replay result;
- compatibility matrix summary;
- existing hash regression summary;
- real-project save/reopen result;
- Unity process count and host-cache reuse;
- hidden smoke markers;
- focused test counts;
- artifact scope;
- Goal153 manualGateReady/acceptance flags;
- final commit/push/HEAD/worktree;
- confirmation no Goal153 human acceptance was claimed.
