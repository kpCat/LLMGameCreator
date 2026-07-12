# Goal 150 — Character Attributes + Level Progression FeatureModules Vertical Slice

## Identity

- Task ID: `goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice`
- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `47870201b6daaaf6db6fd08816f7796011866680` or a direct descendant

This is a fresh Codex dialog. This file is the complete instruction source.

## Goal type

Major product goal. Add two real default-off FeatureModules to the accepted `Игры` workspace:

```text
feature.character.attributes
feature.character.level_progression
```

Do not add Goal-number tabs or a new top-level page.
Do not change the public GamePackage schema.
Do not start factions/weather/SemanticPack/export work.

## Current state

```text
Goal148 accepted=true by human
Goal149 GREEN, accepted=false, manualReviewDeferred=true
Goal141 accepted=false
```

Goal150 remains:

```text
accepted=false
acceptedByHuman=false
acceptedByCodex=false
manualReviewRequired=true
```

After GREEN Goal150, the next step is a bundled human review of Goals149/150.

## Product objective

The normal user path must become:

```text
Игры
→ Механики
  → Экипировка и оружие
  → Характеристики персонажа
  → Уровни и опыт
→ Настройки
→ Собрать и проверить игру
```

Goal149 capability-driven playthrough remains the only normal project qualification path. New mechanics contribute declarative actions and effects; Composer/Runtime must not branch on module/composition IDs.

## Module A — Character Attributes

Create:

```text
catalogs/feature-modules/optional/character-attributes.featuremodule.json
```

Metadata:

```text
moduleId=feature.character.attributes
title=Характеристики персонажа
category=Персонаж
required=false
selectable=true
defaultSelected=false
moduleVersion=1.0.0
```

Parameters:

```text
startingStrength
  title=Начальная сила
  integer, default=7, min=0, max=20, step=1

damagePerStrengthPoint
  title=Урон за очко силы
  numeric, default=1, min=0, max=5
```

Package effects, atomically:

```text
game.stats[id=stat/strength].defaultValue
game.encounters[id=encounter/goblin_duel].participants[id=player].stats[id=stat/strength].amount
```

Add generic basic-attack metadata:

```text
source_stat_damage_stat_id=stat/strength
source_stat_damage_baseline=5
source_stat_damage_per_point=<parameter>
```

No hardcoded `feature.character.attributes` or `stat/strength` branch in generic orchestration/combat. Package metadata chooses the stat.

### Runtime attributes

Add player/global stats to `GameRuntimeState` when needed:

```text
Stats[]
```

Initialize from package stat defaults. At encounter start, deterministically propagate current player stat state into the player participant. Document and test precedence for explicit participant values.

Combat scaling:

```text
stat bonus = (source stat - baseline) * per-point multiplier
```

Requirements:

- generic stat ID from ability metadata;
- combines additively with Goal149 equipment bonus;
- structured event args: `statId`, `statValue`, `statDamageBonus`;
- malformed metadata rejects before mutation;
- default result: strength 7, baseline 5, multiplier 1 => +2 damage.

Playthrough contribution:

```text
inspect_character_attributes
runtime.presentation.inspect_attributes
```

It must pass without combat and expose `stat/strength=7`.

Runtime effects:

```text
player_stat_equals
combat_stat_damage_delta (required only when combat capability exists)
```

## Module B — Level Progression

Create:

```text
catalogs/feature-modules/optional/character-level-progression.featuremodule.json
```

Metadata:

```text
moduleId=feature.character.level_progression
title=Уровни и опыт
category=Персонаж
required=false
selectable=true
defaultSelected=false
moduleVersion=1.0.0
```

Parameter:

```text
level2RequiredExperience
  title=Опыт для второго уровня
  integer, default=10, min=1, max=1000, step=1
```

Bind to:

```text
game.progressions[id=progression/character_level].stages[id=level/2].requiredAmount
```

Add generic Runtime command:

```text
GameRuntimeCommandType.ChangeProgression
GameRuntimeCommand.ChangeProgression(progressionId, amount)
runtime.command.change_progression
```

It must delegate to existing `OutputApplier` progression handling; do not duplicate stage resolution.

Playthrough actions:

```text
gain_character_experience
  target progression/character_level
  amount=10

inspect_character_progression
  runtime.presentation.inspect_progression
  dependsOn=gain_character_experience
```

Default result:

```text
amount=10
stageId=level/2
```

Must work without combat, attributes or equipment and survive checkpoint/full replay.

Runtime effects:

```text
progression_amount_equals
progression_stage_equals
```

## Generic extended mutation layer

Goal149 added `FeatureModuleItemMetadataMutationService`. Do not add a bespoke service per field.

Refactor or add one deterministic handler registry supporting:

```text
item_metadata_numeric
stat_default_value
encounter_participant_stat_amount
ability_metadata_string
ability_metadata_numeric
progression_stage_required_amount
```

Requirements:

- route only by target kind, never module ID;
- exact target cardinality;
- expected-old-value checks;
- explicit expected-missing for new metadata;
- stable operation ordering;
- forward/reverse module order yields byte-identical packages;
- Goal149 equipment package hashes stay unchanged.

A compatibility wrapper for `FeatureModuleItemMetadataMutationService` may remain.

## Capability-driven additions

Add:

```text
runtime.presentation.inspect_attributes
runtime.command.change_progression
runtime.presentation.inspect_progression
```

Requirements:

- plan generated from selected module contracts;
- no fixed normal action list;
- checkpoint boundary remains contract-derived;
- plan ID/signature remains in checkpoint identity;
- presentation actions do not mutate state;
- session/snapshot expose:
  `AttributesSummary`, `ProgressionSummary`.

## Required scenarios

### 1. Goal149 legacy project, new modules disabled

Selected optional modules remain the three accepted profile modules.

Hashes must remain exactly:

```text
composition=e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221
activated=c46826d8231951ab941f6ee1608d30273b1e186f920ea8cad58c58c25317eeeb
final=95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8
```

### 2. Goal149 equipment enabled, Goal150 modules disabled

Hashes must remain:

```text
composition=94a47ab896b425a76c2e523acef3ab87d538bb8f0c754b2402b0127e5ad82bf5
activated=147f88ac026f006ab5fbe93dc6c7cb039e85189fcb3421a71a1fd99284d3a5c1
final=51bba1ffada4ce9ffccfa9132e7e7c007afcbcec8632d7de13d26ce961b3ea0d
```

### 3. Attributes without combat

```text
strength=7
inspect action present
combat actions absent
combat assertion not required
checkpoint/replay/binding GREEN
```

### 4. Progression without combat or attributes

```text
amount=10
stage=level/2
combat/attribute actions absent
checkpoint/replay/binding GREEN
```

### 5. Attributes + combat, no equipment

```text
statDamageBonus=2
equipmentDamageBonus=0
totalAdditionalDamage=2
```

### 6. Equipment + combat, no attributes

Preserve Goal149:

```text
equipmentDamageBonus=2
statDamageBonus=0
totalAdditionalDamage=2
```

### 7. Attributes + equipment + combat

```text
statDamageBonus=2
equipmentDamageBonus=2
totalAdditionalDamage=4
```

Order-independent.

### 8. Attributes + progression without combat/equipment

Both effects pass independently.

### 9. Full current optional set

Enable all six optional modules:

```text
feature.profile.alchemy_focus
feature.profile.combat_focus
feature.profile.exploration_resource_focus
feature.equipment.weapon_loadout
feature.character.attributes
feature.character.level_progression
```

Record deterministic composition/activated/final hashes. Package validation, checkpoint, replay, binding and project identity must be GREEN.

## Additive compatibility

Adding the two default-off modules must not stale `game/goal148-manual`.

Required:

```text
new unselected optional modules → ADDITIVE_COMPATIBLE or CURRENT
stale=false
no manual JSON edit
no auto-selection
```

Saving refreshes catalog and tracked fingerprints. A changed selected module becomes STALE; removed selected module becomes UNRESOLVED.

## Unified `Игры` UI

No new page/tab.

Dynamic UI shows:

```text
Характеристики персонажа
Уровни и опыт
```

Settings controls come only from metadata.

Successful summary, when enabled:

```text
Сила: 7
Бонус урона от силы: +2
Уровень: 2
Опыт: 10
```

Disabled modules produce no summary lines.

Technical details may show attributes/progression summaries, stat/equipment/total damage bonuses, plan ID/signature.

Normal workspace Goal-number controls remain 0.

## Incremental certification

Required:

```text
first run executes both new modules
second run reuses both
attributes change invalidates attributes and dependents only
progression threshold change invalidates progression and dependents only
equipment remains reusable when unrelated
```

Preserve Goal147A dependency closure/cycle behavior.

## Required artifacts

Write under both:

```text
.llmgc/procedural/goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice/
.llmgc/exports/goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice/
```

At minimum:

```text
goal150-dashboard.json
character-attributes-module-proof.json
level-progression-module-proof.json
extended-mutation-engine-proof.json
attributes-runtime-state-proof.json
progression-runtime-state-proof.json
attributes-without-combat-proof.json
progression-without-combat-proof.json
attributes-combat-proof.json
equipment-attributes-additivity-proof.json
attributes-progression-composition-proof.json
full-current-optional-set-proof.json
goal149-disabled-hash-regression-proof.json
goal149-equipment-hash-regression-proof.json
additive-catalog-compatibility-proof.json
goal150-save-replay-proof.json
goal150-certification-proof.json
goal150-negative-proof.json
goal150-regression-compatibility-proof.json
goal150-file-index.json
goal150-report.md
```

File index includes SHA-256.

## Dashboard

```text
status=GREEN
goal148Accepted=true
goal149Accepted=false
goal150Accepted=false
requiredCoreModuleCount=10
optionalModuleCount=6
characterAttributesModule=true
levelProgressionModule=true
bothDefaultSelected=false
capabilityDrivenRuntimePlaythrough=true
extendedMutationEngine=true
attributesWithoutCombatPassed=true
progressionWithoutCombatPassed=true
attributesCombatPassed=true
equipmentAttributesAdditive=true
defaultStrength=7
defaultStatDamageBonus=2
defaultEquipmentDamageBonus=2
combinedDamageBonus=4
defaultProgressionAmount=10
defaultProgressionStage=level/2
goal149DisabledHashesPreserved=true
goal149EquipmentHashesPreserved=true
additiveCatalogCompatibilityPassed=true
allCheckpointReloadsPassed=true
allFullReplaysEquivalent=true
allActionBindingsPassed=true
projectIdentityPreserved=true
normalWorkspaceGoalNumberControlCount=0
newTopLevelPageAdded=false
manualReviewRequired=true
accepted=false
```

## Negative proof

Executable where practical:

```text
unknownStatRejected
missingStatRejected
invalidStatMetadataRejected
invalidStatMultiplierRejected
missingProgressionRejected
invalidProgressionAmountRejected
missingProgressionStageRejected
duplicateActionIdRejected
missingActionDependencyRejected
capabilityCycleRejected
unknownRuntimePrimitiveRejected
attributesActionAbsentWhenDisabled
progressionActionAbsentWhenDisabled
attributeCombatAssertionSkippedWithoutCombat
equipmentBonusAbsentWithoutEquipment
statBonusAbsentWithoutAttributes
presentationActionDoesNotMutateState
tamperedCapabilityPlanRejected
failedBuildPreservesProjectIdentity/package/authoring
newModulesDoNotStaleUnrelatedProject
moduleOrCompositionIdSwitchAbsent
noChildToolProcessStarted
historicalArtifactsRewritten=false
```

## Backward compatibility

Preserve Goal148 acceptance, Goal149 legacy and capability paths, all Goal149 hashes, Goal148A/B/C regressions, Goal147 certification closure and historical Goal142–149 artifacts. Unity remains unchanged.

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
docs/agent-tasks/goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice/GOAL.md

catalogs/feature-modules/manifest.json
catalogs/feature-modules/optional/equipment-weapon-loadout.featuremodule.json
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/**
src/LLMGameCreator.Application/Design/FeatureModuleComposition/**
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/**
src/LLMGameCreator.Application/Design/FeatureModuleCertification/**
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/**
src/LLMGameCreator.Runtime.Abstractions/RuntimeContracts.cs
src/LLMGameCreator.Runtime/OutputApplier.cs
src/LLMGameCreator.Runtime/GameRuntimeStateFactory.cs
src/LLMGameCreator.Runtime/GameRuntimeService.cs
src/LLMGameCreator.Runtime/EncounterRuntimeService.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Runtime/SelectedRuntimeVariantInteractiveSessionService.cs
```

## Allowed paths

Only:

```text
docs/agent-tasks/goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-character-attributes-level-progression-slice.ps1
.devflow/scripts/run-character-attributes-level-progression-slice.cmd
.llmgc/procedural/goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice/**
.llmgc/exports/goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice/**

catalogs/feature-modules/manifest.json
catalogs/feature-modules/optional/character-attributes.featuremodule.json
catalogs/feature-modules/optional/character-level-progression.featuremodule.json

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice.md
docs/manual-acceptance/character-attributes-and-level-progression-featuremodules-vertical-slice.md

src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/**
src/LLMGameCreator.Application/Design/FeatureModuleComposition/**
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/**
src/LLMGameCreator.Application/Design/FeatureModuleCertification/**
src/LLMGameCreator.Application/Design/ProductLineRuntimeQualification/**
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/**
src/LLMGameCreator.Runtime.Abstractions/RuntimeContracts.cs
src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime.Abstractions/SelectedRuntimeVariantInteractiveSessionContracts.cs
src/LLMGameCreator.Runtime/GameRuntimeStateFactory.cs
src/LLMGameCreator.Runtime/GameRuntimeService.cs
src/LLMGameCreator.Runtime/OutputApplier.cs
src/LLMGameCreator.Runtime/EncounterRuntimeService.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Runtime/SelectedRuntimeVariantInteractiveSessionService.cs
src/LLMGameCreator.Runtime/RuntimeStateHelpers.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs

tests/LLMGameCreator.Tests/Application/CapabilityDrivenRuntimePlaythrough/**
tests/LLMGameCreator.Tests/Application/FeatureModuleComposition/**
tests/LLMGameCreator.Tests/Application/FeatureModuleAuthoring/**
tests/LLMGameCreator.Tests/Application/FeatureModuleCertification/**
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/**
tests/LLMGameCreator.Tests/Runtime/CharacterAttributesProgressionRuntimeTests.cs
tests/LLMGameCreator.Tests/Runtime/CapabilityDrivenEquipmentRuntimeTests.cs
tests/LLMGameCreator.Tests/WinForms/UnifiedGameProjectWorkspaceTests.cs
tests/LLMGameCreator.Tests/Devflow/RunCharacterAttributesLevelProgressionSliceScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/ProjectsPageProductSmokeTests.cs
```

## Forbidden paths

Do not modify/stage:

```text
.llmgc/manual/**
.llmgc/workspace/**
samples/minimal-map-game/**
all historical Goal142–149 procedural/export roots
catalogs/feature-modules/core/**
catalogs/feature-modules/optional/alchemy-focus.featuremodule.json
catalogs/feature-modules/optional/combat-focus.featuremodule.json
catalogs/feature-modules/optional/exploration-resource-focus.featuremodule.json
catalogs/feature-modules/optional/equipment-weapon-loadout.featuremodule.json
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

No public schema, sample, Unity or dependency changes.

## Validation

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore

dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~Goal150|FullyQualifiedName~CharacterAttributes|FullyQualifiedName~LevelProgression|FullyQualifiedName~CapabilityDrivenRuntimePlaythrough|FullyQualifiedName~FeatureModuleComposition|FullyQualifiedName~FeatureModuleCertification|FullyQualifiedName~UnifiedGameProjectWorkspace"

.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1 -DryRun
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1 -ApplyCleanup

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1 -DryRun
.\.devflow\scripts\run-goal148c-project-identity-hotfix.ps1 -DryRun
.\.devflow\scripts\run-goal148b-current-package-ui-thread-hotfix.ps1 -DryRun
.\.devflow\scripts\run-goal148a-new-project-support-files-hotfix.ps1 -DryRun
.\.devflow\scripts\run-featuremodule-authoring-persistence-and-certification.ps1 -DryRun

.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun

git diff --check
git diff --cached --check
git status --short --untracked-files=all
git diff --name-only
git diff --cached --name-only
git ls-files .llmgc/manual .llmgc/workspace
```

Required: zero warnings/errors, zero mojibake/escaped Cyrillic, forbidden diff empty.

Restore validation churn only by exact policy-derived paths. Do not use reset-hard, clean, broad restore, branch switching, merge, rebase or cherry-pick.

## Current-state update

After GREEN:

```text
goal148Accepted=true
goal149Accepted=false
goal149ManualReviewDeferred=false
goal150Accepted=false
goal150ManualReviewRequired=true
characterAttributesFeatureModule=true
levelProgressionFeatureModule=true
optionalModuleCount=6
defaultStrength=7
defaultStatDamageBonus=2
defaultProgressionAmount=10
defaultProgressionStage=level/2
equipmentAttributesAdditive=true
goal149HashesPreserved=true
additiveCatalogCompatibility=true
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
nextProductGoal=review_goals_149_150_equipment_attributes_progression_workflow
```

Do not mark Goal141 accepted.

## Publication

Commit:

```text
GREEN Goal 150 character attributes and level progression FeatureModules vertical slice
```

Push `origin main`.

Do not report GREEN if either mechanic uses a module/composition hardcoded branch, if equipment and stat bonuses do not add independently, or if existing projects become stale merely because the new default-off modules were added.
