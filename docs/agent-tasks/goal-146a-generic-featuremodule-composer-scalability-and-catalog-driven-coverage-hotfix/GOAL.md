# Goal 146A — Generic FeatureModule Composer Scalability + Catalog-Driven Coverage Hotfix

## Identity

- Task ID: `goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix`
- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: commit `e18a2016af5e0bfda6b99f373d37a1007e8e4ba5` or a direct descendant

## Why this hotfix exists

Goal146 is mechanically GREEN, but its current matrix/orchestration layer does not yet satisfy the intended scalable product-line architecture.

The current implementation contains several fixed three-module assumptions:

```text
FeatureModuleCompositionVocabulary.OptionalModuleIds contains exactly three IDs
FeatureModuleCatalog.OptionalProfileModules switches on known Goal142 recipe IDs
FeatureModuleCompositionService.MatrixSpecs manually lists eight combinations
ValidateMatrix requires exactly 8/3/4 counts
CompositionId special-cases moduleIds.Count == 3
ShortName/DisplayName special-case the three current module IDs
BuildSemanticProof contains fixed alchemy/combat/exploration booleans
Tests only prove the fixed eight-row matrix
```

That means adding a fourth independent module requires changing the Composer/orchestrator itself. This is the architecture defect to correct.

The desired model is:

```text
implement one independent FeatureModule
+ declare its dependencies/conflicts/effect contracts/parameters
→ register it in a catalog
→ generic Composer accepts it without code changes
```

The eight current combinations are only an automatically generated exhaustive fixture for a tiny three-module catalog. They are not eight products and must not become the long-term testing policy.

## Status policy

Goal146 remains:

```text
accepted=false
acceptedByHuman=false
acceptedByCodex=false
manualReviewDeferred=true
```

Do not request or record a human Goal146 acceptance in this hotfix.

After GREEN Goal146A, continue to Goal147 without a manual gate unless a new real P0/P1 defect is found.

## Required read-first

Read in order:

```text
AGENTS.md
README.md
docs/CONTEXT_INDEX.md
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md

docs/agent-tasks/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix/GOAL.md

src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCatalog.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionValidator.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionPlanner.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionService.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionWorkbenchController.cs
src/LLMGameCreator.Application/Design/ProductLineRuntimeQualification/ProductLineRuntimeQualifier.cs
src/LLMGameCreator.Application/Design/ProductLineInteractiveSessionMatrix/ProductLineInteractiveSessionMatrixService.cs
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantMaterializer.cs

src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal146.cs

tests/LLMGameCreator.Tests/Application/FeatureModuleComposition/FeatureModuleCompositionTests.cs
tests/LLMGameCreator.Tests/WinForms/Goal146FeatureModuleComposerBindingTests.cs

.llmgc/procedural/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix/featuremodule-composition-dashboard.json
.llmgc/procedural/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix/featuremodule-composition-matrix-result.json
```

## Critical architecture rules

### 1. Generic Composer

The Composer accepts an arbitrary `FeatureModuleCatalogDocument` and arbitrary compatible `selectedModuleIds`.

It must not know specific composition IDs or fixed combinations.

Forbidden in generic Composer/planner/coverage/effect-evaluator paths:

```text
switch/if on minimal-map-game-composed-* composition IDs
manual eight-row MatrixSpecs table
indexing OptionalModuleIds[0..2]
moduleIds.Count == 3 special cases
switch/if on feature.profile.alchemy_focus
switch/if on feature.profile.combat_focus
switch/if on feature.profile.exploration_resource_focus
```

A legacy Goal142 import adapter may translate Goal142 records into FeatureModule definitions, but the Composer must consume only the resulting typed definitions.

### 2. Catalog-derived optional modules

Remove `FeatureModuleCompositionVocabulary.OptionalModuleIds` as the source of active Composer truth.

The active optional set is always:

```csharp
catalog.Modules
    .Where(module => module.Selectable && !module.Required)
    .OrderBy(module => module.ModuleId)
```

Default selected modules are derived from that set.

Current compatibility constants may remain only where a historical artifact/test explicitly needs them, but generic service logic must not depend on them.

### 3. Generic composition IDs and labels

Generate composition IDs from sorted selected module IDs using a deterministic slug function.

Examples for current modules may remain byte-compatible:

```text
minimal-map-game-composed-baseline
minimal-map-game-composed-alchemy
minimal-map-game-composed-combat-exploration
minimal-map-game-composed-alchemy-combat-exploration
```

But the algorithm must also produce a valid ID for an unknown future module without modification.

Display names come from module titles, not a switch over module IDs.

### 4. Catalog-driven coverage planner

Create a reusable bounded coverage layer, preferably:

```text
FeatureModuleCompositionCoveragePolicy
FeatureModuleCompositionCoveragePlan
FeatureModuleCompositionCoveragePlanner
```

Required policy behavior:

#### Small catalog

When selectable optional module count is at or below a configurable threshold, default `3`:

```text
coverageMode=exhaustive_small_catalog
```

Generate the complete powerset algorithmically.

For the current three modules this must still produce exactly eight rows and preserve the existing eight package/final-state hashes.

#### Larger catalog

When optional module count exceeds the threshold:

```text
coverageMode=bounded_interaction_coverage
fullPowersetEnumerated=false
```

Generate a bounded deterministic set from:

```text
baseline composition
operator-selected composition
all-enabled composition
module singleton certification rows
compatible pair/interaction rows within a configurable cap
declared shared-target or interaction groups
deterministic sampled compositions within a configurable cap
```

Do not enumerate `2^N`.

Required policy fields include at least:

```text
exhaustiveOptionalModuleLimit
maxPairwiseRows
maxSampledRows
maxTotalRows
deterministicSeed
```

Required plan fields include:

```text
coverageMode
optionalModuleCount
theoreticalPowersetSize or overflow-safe representation
generatedCompositionCount
fullPowersetEnumerated
baselineIncluded
selectedCompositionIncluded
allEnabledIncluded
singletonCoverageCount
pairwiseCoverageCount
sampledCoverageCount
bounded
compositionSpecs[]
```

The selected operator composition must always be included even when the matrix uses bounded coverage.

### 5. No manual combination tables

Delete/replace the current fixed `MatrixSpecs()` table.

The current eight rows must be generated from the three catalog modules by the coverage planner.

`ValidateMatrix` must compare against the generated coverage plan and semantic requirements, not hardcoded numeric constants `8`, `3`, and `4`.

For the current three-module Goal146 fixture, compatibility assertions may still confirm 8/8 after generic validation.

### 6. Generic module runtime-effect contracts

Replace fixed `AlchemyEffectObserved`, `CombatEffectObserved`, and `ExplorationResourceEffectObserved` as the only internal truth with a generic collection.

Add a typed contract such as:

```text
FeatureModuleRuntimeEffectContract
FeatureModuleRuntimeEffectObservation
```

A contract should include enough information to evaluate a fresh Runtime result, for example:

```text
effectId
moduleId
metricKind
targetId
resourceOrItemId
comparisonKind
expectedValue or baseline comparison policy
sourceOperationIds[]
runtimeDimension
```

The evaluator loops over contracts declared by the selected modules. It must not switch on module ID or composition ID.

Current summary convenience fields for alchemy/combat/exploration may remain as derived compatibility projections, but matrix pass/fail and `combinedEffectCount` must come from the generic observation list.

The Goal142 import adapter should derive dependencies/effect contracts from mutation operation target kinds/runtime dimensions or attach them when building the module definition. New modules declare their own effect contracts; the Composer does not change.

### 7. Goal142 adapter is not Composer truth

`FeatureModuleCatalog.LoadFromGoal142` is a bounded legacy/import adapter.

Remove the current recipe-ID dependency switch where practical. Prefer deriving dependencies from mutation target kinds:

```text
inventory_stack_amount → inventory
recipe_output_amount → crafting + inventory
encounter_participant_resource_amount → combat
ability_power / ability_effect_arg_amount → combat
loot_entry_* / resource_node_production_amount → harvest + inventory/world
transaction_output_amount → economy + inventory
```

Unknown future definitions must be accepted when they already provide explicit dependencies and effect contracts.

Do not throw merely because a recipe/module ID is not one of the current three.

### 8. Shared Runtime qualifier remains single

Preserve:

```text
one ProductLineRuntimeQualifier
one canonical action plan
one Runtime interactive-session service
```

Goal145 and Goal146 continue using the same qualifier.

Do not create module-specific Runtime services.

## Required synthetic fourth-module proof

Add an executable test that creates a synthetic fourth optional module without editing Composer code.

Recommended test module:

```text
moduleId=feature.profile.synthetic_fuel_reserve
selectable=true
required=false
mutation target kind=inventory_stack_amount
targetId=inventory/player_start|item/fuel_can
expected old value=1
new value=2
runtime effect contract=final inventory item/fuel_can quantity increased
```

The test must:

1. Start from the real current catalog.
2. Append the synthetic module definition at runtime.
3. Build an arbitrary selected composition containing that module and at least one existing module.
4. Materialize a valid package.
5. Run the shared Runtime qualifier.
6. Pass checkpoint reload, full replay and action binding.
7. Observe the synthetic module effect.
8. Generate a deterministic composition ID without Composer changes.

Required markers:

```text
syntheticFourthModuleRegistered=true
composerSourceUnchangedForSyntheticModule=true
syntheticCompositionMaterialized=true
syntheticCompositionRuntimeQualified=true
syntheticEffectObserved=true
syntheticCheckpointReloadPassed=true
syntheticFullReplayEquivalent=true
```

## Required non-exponential coverage proof

Add tests with synthetic catalogs larger than three modules.

At minimum:

### Four optional modules

```text
coverageMode=bounded_interaction_coverage
fullPowersetEnumerated=false
generatedCompositionCount < 16
selectedCompositionIncluded=true
```

### Twelve optional modules

```text
fullPowersetEnumerated=false
generatedCompositionCount <= maxTotalRows
generatedCompositionCount << 4096
same input + seed → byte-identical coverage plan
```

No package materialization is required for every synthetic twelve-module row; this is a coverage-planner scalability test.

## Current Goal146 compatibility

After refactoring, the real three-module Goal146 run must remain GREEN:

```text
compositionCount=8
passedCompositionCount=8
distinctPackageSha256Count=8
distinctFinalStateHashCount=8
selectedCompositionId=minimal-map-game-composed-alchemy-combat-exploration
selected package SHA=9a83d47e8e2ae541e7789b804c32f489acb8e7525c0a9dc32a7cc8be8822d65a
selected final hash=d5ad29ee7c350918681c2859b80f5d2944834a6414918a16d8b4e1c0746753b9
checkpoint/full replay/action binding GREEN
Unity smoke GREEN
```

The eight current composition package hashes and final hashes must remain unchanged.

Goal145 matrix and selector regressions must remain GREEN.

## WinForms compatibility

The Goal146 workbench must load optional modules dynamically from the catalog.

It must not assume three checkbox/list entries.

Add a behavioral binder/controller test proving a synthetic fourth optional module:

```text
appears in the workbench list
can be checked programmatically without triggering materialization
is returned by explicit Validate/Materialize button state collection
requires no new WinForms branch
```

Preserve no-reentrancy behavior and no child compiler/test/PowerShell processes.

## Updated artifacts

Goal146 artifacts may be regenerated only when required by the generic schema/proof updates, while preserving current package/final hashes.

Also write compact Goal146A artifacts under both:

```text
.llmgc/procedural/goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix/
.llmgc/exports/goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix/
```

At minimum:

```text
generic-composer-scalability-dashboard.json
catalog-driven-coverage-proof.json
synthetic-fourth-module-proof.json
current-goal146-compatibility-proof.json
generic-composer-scalability-report.md
generic-composer-scalability-file-index.json
```

Required dashboard markers:

```text
status=GREEN
catalogDrivenComposer=true
hardcodedCombinationTableAbsent=true
activeOptionalModuleSetDerivedFromCatalog=true
genericCompositionIdGenerator=true
genericRuntimeEffectContracts=true
currentCoverageMode=exhaustive_small_catalog
currentOptionalModuleCount=3
currentGeneratedCompositionCount=8
currentEightPackageHashesPreserved=true
currentEightFinalHashesPreserved=true
syntheticFourthModulePassed=true
syntheticFourthCoverageMode=bounded_interaction_coverage
syntheticFourthFullPowersetEnumerated=false
syntheticFourthGeneratedCompositionCount<16
largeCatalogFullPowersetEnumerated=false
largeCatalogCoverageBounded=true
largeCatalogCoverageDeterministic=true
sharedRuntimeQualifierStillUsed=true
goal145RegressionGreen=true
goal146RuntimeMatrixGreen=true
goal146UnitySmokeGreen=true
goal146Accepted=false
manualReviewDeferred=true
accepted=false
```

## Negative proof

Prove at least:

```text
manualMatrixSpecsTableAbsent
fixedOptionalModuleIndexingAbsentFromComposer
fixedThreeModuleCountSpecialCaseAbsentFromComposer
unknownFutureModuleDoesNotRequireComposerChange
moduleIdSpecificRuntimeBranchAbsent
compositionIdSpecificRuntimeBranchAbsent
largeCatalogPowersetEnumerationRejectedOrAvoided
coveragePlanMaxRowsEnforced
selectedCompositionNeverDropped
moduleOrderStillByteIndependent
conflictingTargetStillRejected
missingDependencyStillRejected
candidateSpecificRuntimeImplementationAbsent
winFormsSyntheticModuleRequiresNoBranch
```

Use executable behavioral tests wherever practical. Source scans are supplemental only.

## Suggested architecture

Preferred bounded additions:

```text
src/LLMGameCreator.Application/Design/FeatureModuleComposition/
  FeatureModuleCompositionCoverageModels.cs
  FeatureModuleCompositionCoveragePlanner.cs
  FeatureModuleRuntimeEffectModels.cs
  FeatureModuleRuntimeEffectEvaluator.cs
```

Do not create a second Composer or mutation engine.

## Allowed paths

Only create/modify:

```text
docs/agent-tasks/goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-featuremodule-composer-scalability-hotfix.ps1
.devflow/scripts/run-featuremodule-composer-scalability-hotfix.cmd

.llmgc/procedural/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix/**
.llmgc/exports/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix/**
.llmgc/procedural/goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix/**
.llmgc/exports/goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix.md

src/LLMGameCreator.Application/Design/FeatureModuleComposition/**
src/LLMGameCreator.Application/Design/ProductLineRuntimeQualification/**
src/LLMGameCreator.Application/Design/ProductLineInteractiveSessionMatrix/ProductLineInteractiveSessionMatrixService.cs
src/LLMGameCreator.Application/Design/ProductLineInteractiveSessionMatrix/ProductLineInteractiveSessionMatrixModels.cs
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantMaterializer.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineRuntimeVariantMatrixModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineService.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**

src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal146.cs

unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnityFeatureModuleCompositionMatrixHarness.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnityFeatureModuleCompositionMatrixWindow.cs

tests/LLMGameCreator.Tests/Application/FeatureModuleComposition/**
tests/LLMGameCreator.Tests/Application/ProductLineRuntimeQualification/**
tests/LLMGameCreator.Tests/Application/ProductLineInteractiveSessionMatrix/ProductLineInteractiveSessionMatrixTests.cs
tests/LLMGameCreator.Tests/Application/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantMaterializerTests.cs
tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/FeatureModuleCompositionScriptProof.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal146Tests.cs
tests/LLMGameCreator.Tests/Devflow/RunFeatureModuleCompositionRuntimeMatrixScriptTests.cs
tests/LLMGameCreator.Tests/Devflow/RunFeatureModuleComposerScalabilityHotfixScriptTests.cs
tests/LLMGameCreator.Tests/WinForms/Goal146FeatureModuleComposerBindingTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage or commit:

```text
.llmgc/manual/**
samples/minimal-map-game/**

.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/**
.llmgc/exports/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/**
.llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/**
.llmgc/exports/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/**
.llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/**
.llmgc/exports/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/**
.llmgc/procedural/goal-144a-live-session-action-target-binding-and-replay-evidence-hotfix/**
.llmgc/exports/goal-144a-live-session-action-target-binding-and-replay-evidence-hotfix/**
.llmgc/procedural/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/**
.llmgc/exports/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/**
.llmgc/procedural/goal-145a-winforms-candidate-selector-reentrancy-and-selection-stability-hotfix/**
.llmgc/exports/goal-145a-winforms-candidate-selector-reentrancy-and-selection-stability-hotfix/**

src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Generation/**
src/LLMGameCreator.AssetPipeline/**
src/LLMGameCreator.Scripting/**
generator-library/**
provider/**
LLM/**
RAG/**

unity/LLMGameCreatorAlpha/Assets/Scenes/**
unity/LLMGameCreatorAlpha/Assets/Prefabs/**
unity/LLMGameCreatorAlpha/Assets/StreamingAssets/**
unity/LLMGameCreatorAlpha/ProjectSettings/**
unity/LLMGameCreatorAlpha/Packages/**

*.sln
*.csproj
Directory.Build.*
dependency/package files
```

No public GamePackage schema change.
No sample mutation.
No new dependency.
No Runtime gameplay implementation changes.
No provider/network/LLM/Lua work.

## Normal command

Add:

```text
.devflow\scripts\run-featuremodule-composer-scalability-hotfix.cmd
.devflow/scripts/run-featuremodule-composer-scalability-hotfix.ps1
```

The command must:

1. validate paths and refuse `.llmgc/manual/**`;
2. run the generic coverage/composer/effect-contract proof;
3. run the real Goal146 matrix and preserve current package/final hashes;
4. run the synthetic fourth-module Runtime qualification;
5. run the large-catalog bounded-coverage proof;
6. run Goal145 regressions;
7. run Unity Goal146 read-only smoke when required;
8. write Goal146A procedural/export artifacts;
9. use transactional backup/rollback outside the repository;
10. return non-zero on any failure.

Support:

```text
-OutputRoot
-UnityPath
-DryRun
-ApplyCleanup
```

## Validation

Run sequentially:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
```

Required: 0 warnings, 0 errors.

Focused tests:

```powershell
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~FeatureModuleComposition|FullyQualifiedName~Goal146|FullyQualifiedName~ProductLineRuntimeQualification|FullyQualifiedName~ProductLineInteractiveSessionMatrix|FullyQualifiedName~Goal145|FullyQualifiedName~VisualWorldStreamPreviewWorkspace|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection"
```

Commands:

```powershell
.\.devflow\scripts\run-featuremodule-composer-scalability-hotfix.ps1 -DryRun
.\.devflow\scripts\run-featuremodule-composer-scalability-hotfix.ps1 -ApplyCleanup
.\.devflow\scripts\run-featuremodule-composition-runtime-matrix.ps1 -DryRun
```

Guards:

```powershell
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun

git diff --check
git diff --cached --check
git status --short --untracked-files=all
git diff --name-only
git diff --cached --name-only
git ls-files .llmgc/manual
```

Check changed text for mojibake and escaped Cyrillic: zero matches.

Forbidden diff: empty.

Restore validation-generated historical churn only by exact paths computed from the Goal146A scenario policy.

Do not use:

```text
git reset --hard
git clean
broad git restore
branch switching
merge
rebase
cherry-pick
```

## Current-state updates

After GREEN:

```text
goal146Accepted=false
goal146ManualReviewDeferred=true
goal146GenericCatalogDrivenComposer=true
goal146HardcodedCombinationTableAbsent=true
goal146ActiveOptionalSetDerivedFromCatalog=true
goal146GenericCompositionIdGenerator=true
goal146GenericRuntimeEffectContracts=true
goal146CurrentCoverageMode=exhaustive_small_catalog
goal146CurrentGeneratedCompositionCount=8
goal146SyntheticFourthModulePassed=true
goal146SyntheticFourthCoverageMode=bounded_interaction_coverage
goal146LargeCatalogCoverageBounded=true
goal146LargeCatalogCoverageDeterministic=true
goal146CurrentPackageHashesPreserved=true
goal146CurrentFinalHashesPreserved=true
nextProductGoal=goal_147_featuremodule_authoring_parameters_and_composition_persistence
```

Do not mark Goal141 accepted.

## Publication

Stage only explicit Goal146A allowlisted paths.

Required commit message:

```text
GREEN Goal 146A generic FeatureModule composer scalability and catalog-driven coverage hotfix
```

Push to `origin/main`.

Final report must include:

- commit SHA;
- removal of fixed MatrixSpecs and fixed optional indexing;
- current three-module coverage mode/count;
- preservation of all current Goal146 package/final hashes;
- synthetic fourth-module package/runtime/effect/replay result;
- four-module bounded coverage count (<16);
- twelve-module bounded coverage count and policy limit;
- generic effect-contract status;
- Goal145/Goal146 regressions;
- Unity smoke;
- test counts;
- artifact scope;
- forbidden diff;
- clean worktree and `HEAD == origin/main`.

Do not report GREEN if adding the synthetic fourth module still requires a Composer code branch or if a larger catalog enumerates the full powerset.
