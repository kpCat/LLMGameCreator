# Goal 149 — Capability-Driven Runtime Playthrough + Equipment FeatureModule Vertical Slice

## Identity

- Task ID: `goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice`
- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: commit `e5303a7d055d3240e958d1fda1cdf0de7b6d8e57` or a direct descendant

This is a fresh Codex dialog. This file is the complete instruction source.

## Goal type

Major product goal. It must deliver both:

1. generic capability-driven Runtime qualification;
2. the first real new optional gameplay mechanic in the accepted `Игры` workspace: equipment/weapon loadout.

Do not create another Goal-number tab. Do not create manually maintained product combinations. Do not make this a proof-only goal.

## First deliverable — record Goal148 human acceptance

Record exactly:

```text
Я принимаю Goal148 unified_game_project_workspace_and_legacy_goal_diagnostics_isolation_verification GREEN. goal148Accepted=true, projectsPageIsPrimaryWorkflow=true, normalWorkspaceGoalNumberControlCount=0, legacyDiagnosticsHiddenByDefault=true, legacyDiagnosticsAvailableByExplicitToggle=true, projectLocalAuthoringPersistence=true, projectAuthoringRoundtripPassed=true, realNewProjectBuildPassed=true, currentPackageUiThreadDispatchPassed=true, crossThreadExceptionAbsent=true, unsafeCurrentChangedSubscriberCount=0, projectIdentityPreserved=true, projectIdentitySidecar=true, legacyAuthoringMigrated=true, projectScopedCompositionId=project-game-goal148-manual-b64404fafc75, projectPackageId=game/goal148-manual, projectTitle=Проверка конструктора, projectVersion=0.1.0, compositionPackageSha256=e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221, activatedProjectPackageSha256=c46826d8231951ab941f6ee1608d30273b1e186f920ea8cad58c58c25317eeeb, finalStateHash=95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8, supportFilesPrepared=true, stagedProjectValidationPassed=true, realProjectValidationPassed=true, packageActivationTransactional=true, failureRollbackPassed=true, heavyWorkRunsOffUiThread=true, uiPumpResponsive=true, projectionOnly=false, runtimeAuthority=true, unityGameplayTruth=false.
```

Update:

```text
docs/manual-acceptance/unified-game-project-workspace-and-legacy-goal-diagnostics-isolation.md
```

Write `goal148-human-acceptance-record.json` under both Goal149 artifact roots.

Required:

```text
goal148Accepted=true
acceptedByHuman=true
acceptedByCodex=false
manualRetryRequired=false
rawManualInputNotCommitted=true
```

Goal149 remains:

```text
accepted=false
acceptedByHuman=false
acceptedByCodex=false
manualReviewDeferred=true
```

Do not mark Goal141 accepted.

## Product problem A — fixed playthrough

The current `ProductLineRuntimeQualifier` has a fixed 13-action plan:

```text
start_runtime
move
interact
inspect_inventory
open_dialogue
start_or_update_quest
show_inventory
craft
harvest
transaction
begin_encounter
basic_attack
show_final_state
```

The canonical Runtime loop is also hardcoded to one minimal-map package and fixed IDs. This forces every game to contain crafting, harvest, trade, dialogue, quest and combat.

Goal149 must replace the normal unified-project qualification path with a capability-driven plan built from selected FeatureModules.

Legacy Goal145–148 calls without an explicit capability plan must remain byte/result compatible.

## Product problem B — no real optional mechanic

The accepted library currently has 10 required core modules and 3 optional numeric profile modules.

Add the first optional mechanic with a real Runtime state transition:

```text
feature.equipment.weapon_loadout
```

Friendly title:

```text
Экипировка и оружие
```

It must:

```text
open the starting chest
take item/rusty_knife
equip it into slot/weapon
persist equipment state through checkpoint/full replay
apply a configurable equipped-weapon damage bonus in combat
show the equipped weapon in the project build result
```

Reuse existing equipment slots, inventories, item definitions, `EquipItem`/`UnequipItem`, Equipment state and Runtime services. No public GamePackage schema change.

## Product-line rule

Correct:

```text
FeatureModule contract
→ declared capabilities/actions/effects
→ generic planner
→ Runtime primitive handlers
```

Forbidden:

```text
if compositionId contains equipment
if moduleId == feature.equipment.weapon_loadout inside generic planner
switch over known module combinations
separate equipment Runtime
```

A registry with one handler per Runtime primitive is allowed.

## Read first

```text
AGENTS.md
README.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md

docs/agent-tasks/goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation/GOAL.md
docs/agent-tasks/goal-148a-new-project-required-support-files-and-transactional-activation-hotfix/GOAL.md
docs/agent-tasks/goal-148b-current-package-ui-thread-dispatch-and-real-workspace-build-retry-hotfix/GOAL.md
docs/agent-tasks/goal-148c-project-identity-preservation-and-project-scoped-composition-hotfix/GOAL.md

catalogs/feature-modules/**
src/LLMGameCreator.Application/Design/FeatureModuleComposition/**
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/**
src/LLMGameCreator.Application/Design/FeatureModuleCertification/**
src/LLMGameCreator.Application/Design/ProductLineRuntimeQualification/**
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/**

src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Runtime/SelectedRuntimeVariantInteractiveSessionService.cs
src/LLMGameCreator.Runtime/EquipmentRuntimeService.cs
src/LLMGameCreator.Runtime/EncounterRuntimeService.cs
src/LLMGameCreator.Runtime/GameRuntimeService.cs
src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime.Abstractions/SelectedRuntimeVariantInteractiveSessionContracts.cs
src/LLMGameCreator.Runtime.Abstractions/RuntimeContracts.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
```

Inspect actual catalog manifest/file names and tests before coding.

# Part 1 — structured FeatureModule playthrough contract

Extend the internal FeatureModule definition with structured playthrough declarations.

Suggested model:

```text
FeatureModuleRuntimePlaythroughContract
  contractId
  capabilityId
  actionId
  category
  phase
  order
  runtimePrimitiveId
  targetSelector
  args{}
  dependsOnActionIds[]
  checkpointBoundaryAfter
  presentationOnly
  required
  expectedRuntimeEffects[]
```

Suggested phases:

```text
bootstrap
world
interaction
narrative
inventory
production
equipment
economy
combat
final
```

The exact model may differ, but it must be structured and deterministic. Do not parse free-form `SmokePlaythroughs` strings as executable truth.

Add to `FeatureModuleDefinition`:

```text
DefaultSelected
Description
RuntimePlaythroughContracts[]
```

Existing module files without new fields must remain readable with safe defaults.

## Core module declarations

Move the accepted normal action behavior into contracts owned by the relevant required modules:

```text
feature.world.grid_navigation
  start_runtime
  move

feature.interaction.basic
  interact

feature.dialogue.basic
  open_dialogue

feature.quest.objective_chain
  start_or_update_quest

feature.inventory.basic
  inspect_inventory / show_inventory

feature.crafting.recipes
  craft

feature.resources.harvest
  harvest

feature.economy.transaction
  transaction

feature.combat.turn_based_encounter
  begin_encounter
  basic_attack

feature.player_adapter.runtime_summary
  show_final_state / inspect_status
```

Package loading may remain a system bootstrap step. Gameplay actions must come from selected module contracts.

The equipment-disabled accepted project must retain the same action order and Runtime semantics as Goal148C.

## Target resolution

Resolve targets generically against the package. Support at least:

```text
manifest_package
start_map
entity_id
interaction_id
dialogue_id
quest_id
inventory_id
recipe_id
resource_node_id
transaction_id
encounter_id
encounter_participant_id
item_id
equipment_slot_id
container_inventory_id
```

Explicit narrow-alpha IDs may live in module data, but planner/runtime code must not branch on them.

Reject unresolved or ambiguous targets before Runtime execution.

# Part 2 — capability-driven planner

Create a reusable seam, preferably:

```text
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/
```

Suggested components:

```text
CapabilityDrivenRuntimePlaythroughPlanner
CapabilityDrivenRuntimePlaythroughValidator
CapabilityDrivenRuntimePlaythroughModels
CapabilityDrivenRuntimeQualificationService
```

Input:

```text
selected required + optional FeatureModuleDefinitions
materialized GamePackage
```

Output:

```text
planId
selectedModuleIds[]
capabilityIds[]
orderedActions[]
checkpointBoundaryActionId
runtimePrimitiveIds[]
resolvedTargets[]
actionPlanSignature
diagnostics[]
```

Requirements:

1. Deterministic dependency-aware ordering.
2. Duplicate action IDs rejected.
3. Dependency cycles rejected.
4. Missing primitive handler rejected.
5. Missing or ambiguous target rejected.
6. Presentation actions do not mutate Runtime.
7. No composition/project ID logic.
8. A new module using existing primitive IDs requires no planner code change.
9. The plan may omit absent systems.

## Variable checkpoint/replay

Remove normal-path assumptions:

```text
checkpoint action count == 8
final action count == 13
session complete when command index == 13
```

For capability plans:

```text
checkpointReplayedActionCount == planned checkpoint action count
finalReplayActionCount == planned final action count
session complete == all planned actions completed
```

Legacy no-plan calls retain historical 8/13 behavior.

# Part 3 — Runtime primitive execution

Refactor the canonical command loop so capability actions execute through a bounded primitive-handler registry.

Required current primitives:

```text
runtime.command.start
runtime.command.move
runtime.command.interact
runtime.command.open_dialogue
runtime.command.start_or_update_quest
runtime.command.show_inventory
runtime.command.craft_recipe
runtime.command.harvest_resource
runtime.command.execute_transaction
runtime.command.start_encounter
runtime.command.basic_attack
runtime.presentation.inspect_inventory
runtime.presentation.inspect_status
runtime.presentation.final_state
```

Add equipment primitives:

```text
runtime.command.open_container
runtime.command.take_from_container
runtime.command.equip_item
runtime.presentation.inspect_equipment
```

A handler receives a resolved action contract and builds/executes existing `PlayerCommand` or `GameRuntimeCommand`.

Handlers are primitive-specific, never module-specific.

Preserve exact action descriptor binding:

```text
actionId
commandKind
targetId
canonicalStepId
canonicalStepIndex
runtimeCommandStartIndex
runtimeCommandEndIndex
executionTargetId
executionBindingValidated
```

Replay must validate dynamic plan identity/signature plus package/candidate identity.

# Part 4 — equipment module

Add a file-based optional module:

```text
moduleId=feature.equipment.weapon_loadout
title=Экипировка и оружие
description=Позволяет получать, надевать и сохранять оружие в слотах экипировки.
category=Экипировка
moduleKind=optional_feature
required=false
selectable=true
defaultSelected=false
dependencies:
  feature.inventory.basic
```

Required playthrough:

```text
open inventory/chest_start
take item/rusty_knife into inventory/player_start
equip item/rusty_knife into slot/weapon
inspect equipment
```

If combat is selected, `basic_attack` must include the equipped weapon bonus.

## Typed parameter

Add:

```text
parameterId=weaponDamageBonus
title=Бонус урона оружия
valueType=integer
default=2
minimum=0
maximum=10
step=1
unit=damage
```

Bind it declaratively to a numeric item metadata value such as `combat_damage_bonus`.

Implement a generic mutation binding for item metadata numeric values. A missing metadata field may be created only through an explicit expected-missing contract; never silently create arbitrary paths.

## Combat effect

When the attacker is the player:

1. inspect current player equipment;
2. resolve equipped item definitions;
3. sum valid `combat_damage_bonus` metadata values;
4. add the bonus to basic-attack damage;
5. emit a bounded Runtime event/diagnostic summary.

Required:

```text
no equipped weapon → historical damage unchanged
weapon equipped → configured bonus applied
weapon unequipped → bonus absent
invalid metadata → deterministic validation failure
non-player attacks unchanged
save/replay preserves equipment and combat state
```

Do not add a second combat system.

## Runtime effects

Extend generic effect evaluation for:

```text
equipment_slot_item_equals
inventory_item_absent_or_decreased
combat_damage_delta
```

The equipment module passes only when:

```text
slot/weapon contains item/rusty_knife
item transfer is correct
combat bonus is observed when combat is selected
checkpoint/full replay preserve equipment
```

# Part 5 — additive catalog compatibility

Adding an unselected optional module must not make existing projects stale.

Current behavior treats any catalog fingerprint change as stale. Change semantics:

```text
selected module missing → UNRESOLVED
selected module fingerprint changed → STALE
required core module fingerprint changed → STALE
selected parameter/binding changed → STALE
only new unselected optional module added → ADDITIVE_COMPATIBLE / CURRENT
unrelated unselected optional module changed → CURRENT with informational catalog drift
```

On successful save/build, refresh stored catalog fingerprint without changing selected modules or values.

Required proof with accepted `goal148-manual`:

```text
new equipment module appears unselected
project opens without error
existing values preserved
existing selected module set remains 3
existing accepted hashes remain valid while equipment is disabled
no manual JSON edit
```

## Default selection

Use `DefaultSelected` for new documents:

```text
existing three profile modules defaultSelected=true
equipment module defaultSelected=false
```

Existing projects retain exact selections.

# Part 6 — accepted workspace integration

Do not add a top-level page or Goal tab.

In `Игры → Механики`, show catalog-driven:

```text
Экипировка и оружие
```

Do not add an `if moduleId` branch or mandatory central UI mapping for the new module.

In `Игры → Настройки`, when enabled show:

```text
Бонус урона оружия
```

When disabled, the parameter control is absent.

Successful equipment build summary includes:

```text
Экипировано: Ржавый нож
Слот: Оружие
Бонус урона: +N
```

Technical details include:

```text
Runtime playthrough plan ID
capability count
planned action count
checkpoint action count
playthrough signature
equipment slot summary
composition package SHA
activated project package SHA
final state hash
```

# Required proof matrix

## Accepted project, equipment disabled

Use:

```text
healingPotionOutput=3
basicAttackDamage=5
goblinStartingHealth=18
appleYield=4
logYield=4
transactionPotionOutput=3
equipment disabled
```

Required unchanged:

```text
compositionPackageSha256=e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221
activatedProjectPackageSha256=c46826d8231951ab941f6ee1608d30273b1e186f920ea8cad58c58c25317eeeb
finalStateHash=95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8
```

Equipment actions must be absent.

## Same project, equipment enabled

Enable `feature.equipment.weapon_loadout`, keep bonus `2`.

Required:

```text
new deterministic composition/activated/final hashes
all equipment actions exactly once
rusty knife moves from chest to slot/weapon
slot summary=slot/weapon:item/rusty_knife
basic attack includes +2 equipment bonus
checkpoint reload passes
full replay passes
action binding passes
identity stays Проверка конструктора / game/goal148-manual / 0.1.0
```

Report exact new hashes.

## Equipment without combat

Build a bounded composition with equipment enabled and combat absent:

```text
equip actions pass
combat actions absent
no combat bonus assertion required
qualification passes
```

## Combat without equipment

```text
combat actions pass
equipment actions absent
historical basic attack damage unchanged
```

## Missing equipment data

Remove one at a time in tamper tests:

```text
item/rusty_knife
slot/weapon
inventory/chest_start
inventory/player_start
```

Reject before Runtime mutation with actionable diagnostics.

## Module scalability

Register a synthetic optional module using only existing primitive contracts. Planner code remains unchanged and deterministic. Do not enumerate product combinations.

# Required artifacts

Write under both:

```text
.llmgc/procedural/goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice/
.llmgc/exports/goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice/
```

At minimum:

```text
goal148-human-acceptance-record.json
capability-runtime-playthrough-contract-catalog.json
capability-runtime-playthrough-plan.json
capability-runtime-playthrough-dashboard.json
legacy-project-additive-compatibility-proof.json
legacy-project-hash-regression-proof.json
equipment-module-definition-proof.json
equipment-enabled-build-proof.json
equipment-disabled-build-proof.json
equipment-without-combat-proof.json
combat-without-equipment-proof.json
equipment-save-replay-proof.json
equipment-negative-proof.json
goal149-regression-compatibility-proof.json
goal149-negative-proof.json
goal149-file-index.json
goal149-report.md
```

File index includes SHA-256.

## Dashboard markers

```text
status=GREEN
goal148Accepted=true
capabilityDrivenRuntimePlaythrough=true
fixedNormalActionPlanAbsent=true
legacyNoPlanCompatibility=true
requiredCoreModuleCount=10
optionalModuleCount=4
equipmentModulePresent=true
equipmentDefaultSelected=false
additiveCatalogCompatibilityPassed=true
legacyProjectNotStaleAfterOptionalAddition=true
legacyCompositionHashPreserved=true
legacyActivatedHashPreserved=true
legacyFinalHashPreserved=true
equipmentEnabledBuildPassed=true
equipmentWithoutCombatPassed=true
combatWithoutEquipmentPassed=true
equipmentSlotItem=item/rusty_knife
weaponDamageBonus=2
equipmentBonusApplied=true
dynamicCheckpointCountPassed=true
dynamicFinalReplayCountPassed=true
allActionBindingsPassed=true
projectIdentityPreserved=true
normalWorkspaceGoalNumberControlCount=0
newTopLevelPageAdded=false
manualReviewDeferred=true
goal149Accepted=false
accepted=false
```

# Negative proof

Executable where practical:

```text
duplicateActionIdRejected
actionDependencyCycleRejected
unknownRuntimePrimitiveRejected
unresolvedTargetRejected
ambiguousTargetRejected
missingEquipmentItemRejected
missingEquipmentSlotRejected
missingSourceInventoryRejected
missingTargetInventoryRejected
invalidWeaponBonusRejected
equipmentActionAbsentWhenModuleDisabled
combatActionAbsentWhenCombatModuleDisabled
presentationActionDoesNotMutateState
legacyFixed13FallbackNotUsedByUnifiedProjectPath
moduleOrCompositionIdSwitchAbsent
newModuleDoesNotStaleUnrelatedProject
failedBuildPreservesProjectIdentityAndPackage
noChildToolProcessStarted
historicalArtifactsRewritten=false
```

# Backward compatibility

Preserve:

```text
Goal148 acceptance evidence
Goal148A support-file path and rollback
Goal148B UI-thread dispatch
Goal148C identity and honest hash semantics
Goal145/146/147 historical artifacts and hashes
legacy ProductLineRuntimeQualifier no-plan behavior
public GamePackage schema
```

Do not rewrite Goal142–148C historical artifact roots.

# Allowed paths

Only create/modify:

```text
docs/agent-tasks/goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-capability-runtime-equipment-slice.ps1
.devflow/scripts/run-capability-runtime-equipment-slice.cmd

.llmgc/procedural/goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice/**
.llmgc/exports/goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/unified-game-project-workspace-and-legacy-goal-diagnostics-isolation.md
docs/manual-acceptance/capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice.md

catalogs/feature-modules/manifest.json
catalogs/feature-modules/core/**
catalogs/feature-modules/optional/**

src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/**
src/LLMGameCreator.Application/Design/ProductLineRuntimeQualification/**
src/LLMGameCreator.Application/Design/FeatureModuleComposition/**
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/**
src/LLMGameCreator.Application/Design/FeatureModuleCertification/**
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/**

src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime.Abstractions/SelectedRuntimeVariantInteractiveSessionContracts.cs
src/LLMGameCreator.Runtime.Abstractions/RuntimeContracts.cs

src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Runtime/SelectedRuntimeVariantInteractiveSessionService.cs
src/LLMGameCreator.Runtime/EquipmentRuntimeService.cs
src/LLMGameCreator.Runtime/EncounterRuntimeService.cs
src/LLMGameCreator.Runtime/GameRuntimeService.cs
src/LLMGameCreator.Runtime/RuntimeStateHelpers.cs

src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs

tests/LLMGameCreator.Tests/Application/CapabilityDrivenRuntimePlaythrough/**
tests/LLMGameCreator.Tests/Application/ProductLineRuntimeQualification/**
tests/LLMGameCreator.Tests/Application/FeatureModuleAuthoring/**
tests/LLMGameCreator.Tests/Application/FeatureModuleCertification/**
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/**
tests/LLMGameCreator.Tests/Runtime/CapabilityDrivenEquipmentRuntimeTests.cs
tests/LLMGameCreator.Tests/Runtime/SelectedRuntimeVariantInteractiveSessionServiceTests.cs
tests/LLMGameCreator.Tests/Runtime/CanonicalRuntimePlayerCommandLoopServiceTests.cs
tests/LLMGameCreator.Tests/WinForms/UnifiedGameProjectWorkspaceTests.cs
tests/LLMGameCreator.Tests/Devflow/RunCapabilityRuntimeEquipmentSliceScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/ProjectsPageProductSmokeTests.cs
```

Use actual existing test directories when equivalent. Do not modify project files.

# Forbidden paths

Do not modify/stage:

```text
.llmgc/manual/**
.llmgc/workspace/**
samples/minimal-map-game/**
all historical Goal142–148C procedural/export roots

src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Domain/**
src/LLMGameCreator.Generation/**
src/LLMGameCreator.AssetPipeline/**
src/LLMGameCreator.Scripting/**
generator-library/**
provider/**
LLM/**
RAG/**
unity/**

*.sln
*.csproj
Directory.Build.*
dependency/package files
```

No public GamePackage schema change. No sample mutation. No Unity work. No provider/network/LLM/Lua work. No new dependency.

# Validation

Run sequentially:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
```

Required: 0 warnings, 0 errors.

Focused tests:

```powershell
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~Goal149|FullyQualifiedName~CapabilityDrivenRuntimePlaythrough|FullyQualifiedName~Equipment|FullyQualifiedName~ProductLineRuntimeQualification|FullyQualifiedName~UnifiedGameProjectWorkspace|FullyQualifiedName~FeatureModule"
```

Normal command:

```powershell
.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1 -DryRun
.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1 -ApplyCleanup
```

Regressions:

```powershell
.\.devflow\scripts\run-goal148c-project-identity-hotfix.ps1 -DryRun
.\.devflow\scripts\run-goal148b-current-package-ui-thread-hotfix.ps1 -DryRun
.\.devflow\scripts\run-goal148a-new-project-support-files-hotfix.ps1 -DryRun
.\.devflow\scripts\run-unified-game-project-workspace.ps1 -DryRun
.\.devflow\scripts\run-featuremodule-authoring-persistence-and-certification.ps1 -DryRun
```

Guards:

```powershell
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun

git diff --check
git diff --cached --check
git status --short --untracked-files=all
git diff --name-only
git diff --cached --name-only
git ls-files .llmgc/manual .llmgc/workspace
```

Check changed text for mojibake and escaped Cyrillic: zero matches. Forbidden diff: empty.

Restore validation churn only through exact policy-derived paths. Do not use reset-hard, clean, broad restore, branch switching, merge, rebase or cherry-pick.

# Current-state update

After GREEN:

```text
goal148Accepted=true
goal148AcceptedByHuman=true
goal148AcceptedByCodex=false
goal149Accepted=false
goal149ManualReviewDeferred=true
capabilityDrivenRuntimePlaythrough=true
fixedNormalActionPlanAbsent=true
optionalFeatureModuleCount=4
equipmentFeatureModule=true
equipmentDefaultSelected=false
additiveCatalogCompatibility=true
legacyGoal148HashesPreserved=true
nextProductGoal=goal_150_character_stats_and_progression_featuremodule_vertical_slice
```

Do not mark Goal141 accepted.

# Publication

Before staging:

```text
Goal148 acceptance recorded
legacy project opens/builds unchanged with equipment disabled
equipment-enabled project qualifies and replays
equipment works without combat
combat works without equipment
additive catalog compatibility passed
Goal148A/B/C regressions GREEN
Goal149 accepted=false
artifact scope clean
forbidden diff empty
```

Commit:

```text
GREEN Goal 149 capability-driven Runtime playthrough and equipment FeatureModule vertical slice
```

Push `origin main`.

Final report must include commit SHA, Goal148 acceptance, core/optional module counts, legacy-plan compatibility, accepted legacy hashes, equipment-enabled composition/activated/final hashes, planned/checkpoint/final action counts for disabled/enabled cases, equipment slot/item, configured bonus and combat delta, equipment-without-combat result, additive catalog compatibility, WinForms normal-workspace result, tests, scope, forbidden diff and clean `HEAD == origin/main`.

Do not claim GREEN if the unified path still uses the fixed 13-action plan, if adding equipment makes unrelated projects stale, or if equipment state/bonus is not preserved by save/replay.
