# Goal 150A — Parameterized Runtime Contract Synchronization Hotfix

## Identity

- Task ID: `goal-150a-parameterized-runtime-contract-synchronization-hotfix`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base commit: `a5b17a481ab4aa5807aa7b7d07776d4f7278ab9a`
- Base commit message: `GREEN Goal 150 character attributes and level progression FeatureModules vertical slice`

This is a fresh Codex dialog. This file is the complete instruction source. Do not rely on memory from any other dialog.

## Goal type

Narrow P1 correctness and architecture hotfix for Goals149/150.

Goal150 is **not accepted**. Do not record human acceptance. Do not perform the bundled manual gate. The gate remains deferred until this hotfix is independently audited.

## Why this hotfix is required

Goal150 correctly materializes parameter values into `mutationOperations`, but the effective selected module catalog still keeps the original static:

- `RuntimeEffectContracts.ExpectedValue`;
- `RuntimePlaythroughContracts.Args`.

The current parameter binding pipeline only produces `EffectiveMutationOperations`, and `FeatureModuleParameterizedCompositionService` only replaces `MutationOperations` in selected modules before qualification.

Consequences in the normal `Игры` workflow:

1. `weaponDamageBonus` accepts values `0..10`, but equipment Runtime qualification still expects damage delta `2`.
2. `startingStrength` accepts values `0..20`, but Runtime qualification still expects player strength `7`.
3. `damagePerStrengthPoint` accepts values `0..5`, but Runtime qualification still expects stat damage delta `2`.
4. `level2RequiredExperience` accepts values `1..1000`, but the playthrough still grants fixed `10` experience and expects `amount=10`, `stage=level/2`.
5. Valid non-default settings therefore either fail build/qualification or are not honestly exercised by the Runtime playthrough.

There is a second forward-compatibility defect in the Goal150 build summary: it uses `Single(...)` over all abilities whose stat-scaling metadata references the inspected stat. A future compatible module adding another strength-scaled ability can make the normal build fail even though the executed Runtime action is unambiguous.

## Product objective

The normal user workflow must support real non-default values:

```text
Игры
→ Механики
  → Экипировка и оружие
  → Характеристики персонажа
  → Уровни и опыт
→ Настройки
  weaponDamageBonus=3
  startingStrength=8
  damagePerStrengthPoint=2
  level2RequiredExperience=12
→ Собрать и проверить игру
```

Required resulting semantics:

```text
weapon damage bonus = 3
strength = 8
stat damage bonus = (8 - 5) * 2 = 6
total additional damage = 3 + 6 = 9
progression amount = 12
progression stage = level/2
```

The custom build must pass:

- package validation;
- exact Runtime-effect qualification;
- checkpoint reload;
- full replay equivalence;
- action descriptor/execution binding;
- project identity overlay;
- transactional activation;
- save, close/reopen and deterministic rebuild.

## Architectural rule

Parameter values are part of the effective FeatureModule definition used for qualification.

A parameter must be able to declaratively influence all relevant effective surfaces:

1. package mutation fields;
2. Runtime playthrough action arguments;
3. Runtime-effect expected values;
4. derived expected values computed from effective parameter/mutation values.

Do not solve this by weakening assertions.

Forbidden shortcuts:

- do not change `equal` checks to weak `changed_from_baseline`, `at_least` or unconditional pass merely to make tests green;
- do not skip Runtime-effect observations for custom values;
- do not clamp valid user values back to defaults;
- do not hardcode the test values `3`, `8`, `2`, `12` in orchestration;
- do not branch on `moduleId`, `parameterId`, `stat/strength`, `progression/character_level`, action ID or composition ID inside generic services;
- do not add a bespoke code service for equipment, attributes or progression;
- do not enumerate module combinations;
- do not add a new Goal-number tab/page;
- do not move gameplay truth into WinForms;
- do not add provider/network/LLM/RAG behavior.

## Required generic solution

Implement one deterministic, data-driven effective-value binding layer.

The exact record names may differ, but the catalog contract must support the equivalent of:

```text
target kind:
  mutation_operation_field
  runtime_effect_expected_value
  runtime_playthrough_arg

target identity:
  operation ID / effect ID / action ID

target field:
  newValue / expectedValue / argument key

value source:
  canonical parameter value
  effective mutation operation value
  deterministic derived numeric expression
```

A recommended compatible shape is a module-level list such as:

```json
"effectiveValueBindings": [
  {
    "bindingId": "...",
    "targetKind": "runtime_effect_expected_value",
    "targetId": "runtime_effect.example",
    "targetField": "expectedValue",
    "valueExpression": "${parameter:moduleId.parameterId}"
  }
]
```

This is a recommendation, not a requirement to use these exact property names.

### Expression safety

If an expression mechanism is used:

- numeric expressions only;
- invariant-culture decimal values;
- references only to selected-module canonical parameters and effective mutation-operation fields;
- deterministic operators only: parentheses, `+`, `-`, `*`, `/`;
- no reflection execution;
- no C# compilation;
- no scripting engine;
- no arbitrary method calls;
- no environment/file/network access;
- reject unknown references, duplicate targets, invalid arithmetic and division by zero before package mutation/activation.

A typed expression tree or another equally generic deterministic representation is acceptable.

### Required effective bindings

Catalog metadata must make these relationships declarative.

#### Equipment

```text
weaponDamageBonus
→ equipment.rusty_knife_combat_damage_bonus.newValue
→ runtime_effect.equipment_combat_damage_delta.expectedValue
```

The equipment slot and inventory-transfer effects must retain their own nonnumeric expectations. Do not overwrite unrelated effects merely because their IDs appear in an old broad `runtimeEffectIds` list.

#### Character attributes

```text
startingStrength
→ attributes.strength_default_value.newValue
→ attributes.player_strength_amount.newValue
→ runtime_effect.player_strength_equals.expectedValue

damagePerStrengthPoint
→ attributes.basic_attack_source_stat_per_point.newValue
```

Derived exact expectation:

```text
runtime_effect.combat_stat_damage_delta.expectedValue
=
(
  attributes.strength_default_value.newValue
  -
  attributes.basic_attack_source_stat_baseline.newValue
)
*
attributes.basic_attack_source_stat_per_point.newValue
```

The derivation must remain generic and must not contain a C# branch for strength or this module.

#### Level progression

```text
level2RequiredExperience
→ progression.character_level_level2_required_amount.newValue
→ gain_character_experience.args.amount
→ runtime_effect.character_progression_amount.expectedValue
```

`runtime_effect.character_progression_stage.expectedValue` remains declaratively `level/2`.

The playthrough must grant the effective threshold amount, not fixed `10`, so any valid value from `1..1000` can reach level 2 and be honestly verified.

## Effective catalog requirements

Before `ComposeAndQualify`:

- resolve canonical parameter values;
- resolve effective mutation operations;
- resolve effective Runtime-effect contracts;
- resolve effective Runtime playthrough contracts;
- validate all targets and expressions;
- create an immutable effective catalog snapshot;
- use the same effective catalog for materialization, capability planning, Runtime qualification, semantic-effect evaluation, checkpoint identity and replay.

Do not mutate the shared loaded catalog instance in place.

Default values must produce an effective action plan and Runtime expectations semantically identical to Goal150.

## Build-summary robustness

Remove the assumption that exactly one ability references an inspected stat.

Current unsafe behavior is equivalent to:

```csharp
qualifiedPackage.Game.Abilities.Single(
    ability => ability.Metadata["source_stat_damage_stat_id"] == inspectedStat.StatId)
```

Required behavior:

- derive the displayed `StatDamageBonus` from the actual executed Runtime result/structured `DamageApplied` event or another unambiguous action-bound Runtime observation;
- do not search globally with `Single(...)`;
- if combat is absent, do not invent a combat bonus line;
- if multiple abilities use the same stat, the normal build must not fail;
- equipment/stat/total values in the summary and technical details must match the executed action.

No Runtime change is expected for this requirement because Goal150 already emits structured `statDamageBonus`, `equipmentDamageBonus` and `totalAdditionalDamage` event args. A Runtime edit is allowed only if a narrowly demonstrated missing datum makes it unavoidable; otherwise keep Runtime untouched.

## Required regression reproduction

Add a focused test that demonstrates the base-commit defect before the fix conceptually:

```text
weaponDamageBonus=3
startingStrength=8
damagePerStrengthPoint=2
level2RequiredExperience=12
```

The final test must prove:

```text
package item combat_damage_bonus = 3
package stat default/player amount = 8
package ability multiplier = 2
package level/2 requiredAmount = 12

capability action gain_character_experience amount = 12

Runtime:
  equipmentDamageBonus = 3
  statValue = 8
  statDamageBonus = 6
  totalAdditionalDamage = 9
  progression amount = 12
  progression stage = level/2
```

## Required tests

### 1. End-to-end custom workspace build

Through the same project-local authoring/controller/build path used by `Игры`:

1. open/create a temporary project;
2. select equipment, attributes and progression;
3. set `3`, `8`, `2`, `12`;
4. save;
5. build and qualify;
6. assert exact package/runtime/summary values;
7. close/reopen;
8. assert selections and values persisted;
9. rebuild;
10. assert deterministic composition package hash, activated package hash, final-state hash and playthrough signature.

### 2. Parameter matrix

At minimum test these valid non-default cases independently:

```text
weaponDamageBonus: 0, 3, 10
startingStrength: 0, 8, 20
damagePerStrengthPoint: 0, 0.5, 2, 5
level2RequiredExperience: 1, 12, 1000
```

The tests may use focused service-level cases for the full matrix, but at least one combined custom case must use the complete normal workspace build path.

For each case, exact Runtime effects must be derived from the effective values. Zero is valid where the parameter contract permits zero.

### 3. Multiple ability compatibility

Create a synthetic in-memory package/catalog case with two abilities that reference the same source stat.

Required:

- capability planning remains deterministic;
- executing the intended basic attack remains unambiguous;
- the normal build summary does not throw;
- displayed stat bonus comes from the executed Runtime event;
- no module-ID/stat-ID branch is added.

Do not change `samples/minimal-map-game/package.json`.

### 4. Negative contract tests

Reject before activation:

- unknown parameter reference;
- unknown mutation operation/effect/action target;
- duplicate binding target;
- incompatible target field;
- nonnumeric value in numeric expression;
- division by zero;
- expression cycle, if the chosen representation permits cycles;
- attempt to bind an unselected module;
- attempt to modify a presentation/action field outside the allowlisted binding target kinds.

Failed build must preserve:

- current activated package bytes;
- project identity;
- saved valid composition;
- previous qualification hashes;
- support files;
- clean rollback state.

### 5. Default-value hash regressions

These hashes must remain exact.

Goal149/150 modules disabled:

```text
composition=e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221
activated=c46826d8231951ab941f6ee1608d30273b1e186f920ea8cad58c58c25317eeeb
final=95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8
```

Equipment enabled, Goal150 modules disabled, default weapon bonus `2`:

```text
composition=94a47ab896b425a76c2e523acef3ab87d538bb8f0c754b2402b0127e5ad82bf5
activated=147f88ac026f006ab5fbe93dc6c7cb039e85189fcb3421a71a1fd99284d3a5c1
final=51bba1ffada4ce9ffccfa9132e7e7c007afcbcec8632d7de13d26ce961b3ea0d
```

All six optional modules enabled with Goal150 defaults:

```text
composition=ba9dbf32c8e79d4e2bf37116dd611cc7eccd7bee73f880aefeb041cce4b2ee40
activated=19e837b8d4925b0b567c52adfb93905bc44ac6e9a13d3008726ff1be89ea49cf
final=ebb05a61036ddfde40b605267685ba8ab90baa01ed3b5efbb815615ae26eca5c
planned/checkpoint/final actions=20/16/20
```

Also preserve all Goal147–150 historical artifact hashes. Do not rewrite old procedural/export evidence to make it agree with the fix.

### 6. Existing test suites

Run at minimum:

```powershell
dotnet build
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug
.\.devflow\scripts\run-capability-driven-runtime-playthrough-equipment-featuremodule-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
```

Use the exact existing script names in the repository if spelling differs.

Known historical visual smoke failures 084–088 may remain only if the existing wrapper still classifies them as known debt and exits PASSED. Do not create new failures.

### 7. Static architecture checks

Fail the Goal if changed generic C# services contain branches or comparisons against:

```text
feature.equipment.weapon_loadout
feature.character.attributes
feature.character.level_progression
weaponDamageBonus
startingStrength
damagePerStrengthPoint
level2RequiredExperience
stat/strength
progression/character_level
gain_character_experience
```

These identifiers are allowed in:

- module JSON;
- focused tests;
- Goal150A artifacts/docs;
- manual acceptance instructions.

They are not allowed as dispatch logic in generic production services.

## Certification and fingerprints

The new declarative binding contract must participate in module fingerprints and incremental certification invalidation.

Required:

- first run after contract change executes affected modules;
- second unchanged run reuses them;
- changing only equipment binding invalidates equipment and dependents only;
- changing only attributes binding invalidates attributes and dependents only;
- changing only progression binding invalidates progression and dependents only;
- unrelated profile modules remain reusable;
- dependency closure/cycle behavior from Goal147A remains intact.

## Additive compatibility and project staleness

The accepted project:

```text
C:\Users\endim\AppData\Local\LLMGameCreator\Games\goal148-manual
packageId=game/goal148-manual
```

does not select the Goal149/150 mechanics. Adding/fixing binding metadata for unselected optional modules must remain additive-compatible:

```text
stale=false
no auto-selection
no manual JSON edit
```

Any selected-module fingerprint change must be reported honestly. Do not silently suppress real staleness.

## Normal UI

Do not add a page or Goal-number control.

The existing metadata-driven controls remain the only normal UI.

The successful custom summary must contain the equivalent of:

```text
Экипировано: Ржавый нож
Бонус урона: +3
Сила: 8
Бонус урона от силы: +6
Уровень: 2
Опыт: 12
```

Technical details must expose:

```text
equipment bonus=3
stat bonus=6
total additional damage=9
progression/character_level=12:level/2
```

Disabled modules add no summary lines.

## Manual acceptance status

After automated GREEN:

```text
goal149Accepted=false
goal150Accepted=false
goal150aAccepted=false
acceptedByCodex=false
manualReviewRequired=true
```

Update the bundled Goal149/150 manual document so the future single human gate uses one custom-value build, not only defaults:

```text
weapon=3
strength=8
per-point=2
level2 XP=12
expected stat/equipment/total=6/3/9
expected level/XP=2/12
```

Do not claim that the human performed it.

## Required artifacts

Write new artifacts under both:

```text
.llmgc/procedural/goal-150a-parameterized-runtime-contract-synchronization-hotfix/
.llmgc/exports/goal-150a-parameterized-runtime-contract-synchronization-hotfix/
```

At minimum:

```text
goal150a-dashboard.json
base-defect-analysis.json
effective-value-binding-contract-proof.json
custom-parameter-workspace-build-proof.json
custom-parameter-runtime-effects-proof.json
custom-parameter-save-reopen-proof.json
multiple-stat-scaled-abilities-proof.json
negative-binding-proof.json
incremental-certification-proof.json
default-hash-regression-proof.json
historical-artifact-integrity-proof.json
artifact-scope-proof.json
goal150a-file-index.json
goal150a-report.md
```

Requirements:

- procedural/export copies byte-identical;
- file index contains SHA-256;
- dashboard status is GREEN only when every required proof passes;
- acceptance flags remain false;
- artifacts must be generated by executable tests/services, not handwritten claims.

## Current-state documents

Update only the necessary current-state documents.

They must state honestly:

- Goal150 commit existed but was blocked from acceptance by parameterized Runtime-contract desynchronization;
- Goal150A fixed it;
- Goals149/150/150A still await one bundled human gate;
- exact default hashes are preserved;
- custom non-default build `3/8/2/12` is automated GREEN;
- no GitHub Actions check exists for the commit if that remains true;
- next step is independent audit, then the bundled human gate.

Do not rewrite historical acceptance records.

## Allowed production paths

The artifact-scope policy must list exact paths/prefixes. Keep the diff narrow.

Allowed production files, only when required:

```text
catalogs/feature-modules/optional/equipment-weapon-loadout.featuremodule.json
catalogs/feature-modules/optional/character-attributes.featuremodule.json
catalogs/feature-modules/optional/character-level-progression.featuremodule.json

src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterBindingService.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterizedCompositionPlanner.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterizedCompositionService.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryFingerprintService.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterValidator.cs

src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectEvaluator.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionService.cs

src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughModels.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughValidator.cs

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
```

A new narrowly named Application-layer file for deterministic effective-value binding/expression evaluation is allowed under:

```text
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/
```

Do not use that prefix as permission for unrelated refactors.

Allowed tests:

```text
tests/LLMGameCreator.Tests/Application/FeatureModuleAuthoring/
tests/LLMGameCreator.Tests/Application/CapabilityDrivenRuntimePlaythrough/
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/
tests/LLMGameCreator.Tests/Application/FeatureModuleCertification/
tests/LLMGameCreator.Tests/WinForms/
tests/LLMGameCreator.Tests/Devflow/
```

Allowed task, docs, scripts and new evidence:

```text
docs/agent-tasks/goal-150a-parameterized-runtime-contract-synchronization-hotfix/
docs/manual-acceptance/capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice.md
docs/manual-acceptance/character-attributes-and-level-progression-featuremodules-vertical-slice.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal150a-parameterized-runtime-contract-synchronization-hotfix.ps1
.devflow/scripts/run-goal150a-parameterized-runtime-contract-synchronization-hotfix.cmd
.llmgc/procedural/goal-150a-parameterized-runtime-contract-synchronization-hotfix/
.llmgc/exports/goal-150a-parameterized-runtime-contract-synchronization-hotfix/
```

If an additional path is genuinely required:

1. stop;
2. explain why it is necessary in the final status;
3. add that exact path to the Goal150A artifact-scope scenario before editing;
4. do not broaden to an entire sensitive subtree.

## Forbidden paths

Do not modify:

```text
src/LLMGameCreator.GamePackage/
src/LLMGameCreator.Domain/
src/LLMGameCreator.Generation/
src/LLMGameCreator.AssetPipeline/
src/LLMGameCreator.Scripting/
src/LLMGameCreator.Runtime/
src/LLMGameCreator.Runtime.Abstractions/
src/LLMGameCreator.WinForms/
unity/
samples/
generator-library/
provider/
LLM/
RAG/
ProjectSettings/
Packages/
```

Also forbidden:

- existing `.llmgc/procedural/goal-149-*`;
- existing `.llmgc/exports/goal-149-*`;
- existing `.llmgc/procedural/goal-150-*`;
- existing `.llmgc/exports/goal-150-*`;
- accepted manual project files;
- generated user workspace files;
- unrelated cleanup/formatting.

If a Runtime edit is proven unavoidable, return `BLOCKED` with the exact missing Runtime datum and proposed exact path. Do not silently expand scope.

## Git discipline

Mandatory:

- work directly on `main`;
- no branch creation;
- no merge;
- no rebase;
- no cherry-pick;
- no broad `git reset`;
- no broad `git clean`;
- no broad `git restore`;
- no checkout of unrelated paths;
- stage only allowlisted exact paths;
- preserve unrelated user changes;
- worktree clean after commit;
- `HEAD == origin/main` after push.

Before editing:

```powershell
git status --short
git rev-parse HEAD
git rev-parse origin/main
```

Required start condition:

```text
HEAD == origin/main == a5b17a481ab4aa5807aa7b7d07776d4f7278ab9a
```

If not true, return `BLOCKED` and do not improvise.

## Commit and push

On GREEN:

```text
commit message:
GREEN Goal 150A parameterized Runtime contract synchronization hotfix
```

Then:

```powershell
git push origin main
git rev-parse HEAD
git rev-parse origin/main
git status --short
```

## Final report format

Return exactly one status:

```text
GREEN
BLOCKED
FAILED
```

For GREEN include:

- commit SHA and message;
- confirmation `HEAD == origin/main`;
- concise root cause;
- chosen generic binding contract;
- changed production paths;
- custom `3/8/2/12` package/runtime results;
- custom summary values `6/3/9` and `2/12`;
- default hash regression values;
- checkpoint/replay/action-binding results;
- certification invalidation/reuse counts;
- artifact-scope count and violations;
- tests/scripts with pass counts;
- confirmation old Goal149/150 artifacts unchanged;
- confirmation all acceptance flags remain false;
- confirmation no manual review was claimed.

For BLOCKED/FAILED include the exact failing command, diagnostic and whether any commit/push occurred.
