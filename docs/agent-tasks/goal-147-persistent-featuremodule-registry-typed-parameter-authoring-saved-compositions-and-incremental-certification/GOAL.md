# Goal 147 — Persistent FeatureModule Registry + Typed Parameter Authoring + Saved Compositions + Incremental Certification

## Identity

- Task ID: `goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification`
- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: commit `4c2113cf4abe752a0d33fefbe825b72e6b1c9e2e` or a direct descendant on `main`

## Critical context for this fresh Codex conversation

This task is intentionally self-contained. Do not rely on memory from any earlier Codex conversation.

The product is a configurable game product-line composer. The intended development model is:

```text
implement an independent FeatureModule once
→ declare dependencies/conflicts/parameters/runtime effects
→ register it in the module library
→ select it in any compatible composition
→ materialize a GamePackage
→ qualify through the shared Runtime/save/replay kernel
```

The product must not be developed as a manually maintained catalog of game combinations.

Goal146A removed the fixed eight-combination table and introduced catalog-driven exhaustive coverage for tiny catalogs plus bounded coverage for larger catalogs. Goal147 must turn that proof seam into an actual authoring product:

```text
persistent module library
+ typed parameters
+ saved composition documents
+ incremental module certification
+ bounded interaction coverage
+ materialize/qualify selected composition
```

## Goal status policy

Goal146 remains:

```text
accepted=false
acceptedByHuman=false
acceptedByCodex=false
manualReviewDeferred=true
```

Do not invent Goal146 human acceptance.

Goal147 remains:

```text
accepted=false
acceptedByHuman=false
acceptedByCodex=false
```

A single bundled human review may be requested only after Goal147 implementation and audit. Do not mark acceptance automatically.

## Product gaps to close

Goal146A is materially better, but four gaps remain:

1. Optional modules are still primarily imported from Goal142 C# / committed artifacts rather than loaded from a durable module library that can grow by adding files.
2. Module numeric values are fixed mutation payloads; there is no typed parameter authoring.
3. Composition requests are transient; there is no user project document that can be saved, reopened, cloned and deterministically rebuilt.
4. Bounded composition coverage still includes every singleton row inside `maxTotalRows=24`; a normal catalog above roughly twenty optional modules can fail even though the problem is linear certification rather than interaction coverage.

Goal147 must close all four without changing the public GamePackage schema.

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
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md

# Relevant completed task specifications
docs/agent-tasks/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix/GOAL.md
docs/agent-tasks/goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix/GOAL.md

# Current FeatureModule composition implementation
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCatalog.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionValidator.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionPlanner.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionCoverageModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionCoveragePlanner.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionService.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionWorkbenchController.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectEvaluator.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleComposerScalabilityProofService.cs

# Shared materialization and Runtime qualification
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantMaterializer.cs
src/LLMGameCreator.Application/Design/ProductLineRuntimeQualification/ProductLineRuntimeQualifier.cs
src/LLMGameCreator.Application/Design/ProductLineInteractiveSessionMatrix/ProductLineInteractiveSessionMatrixService.cs

# Existing UI and tests
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal146.cs
tests/LLMGameCreator.Tests/Application/FeatureModuleComposition/FeatureModuleCompositionTests.cs
tests/LLMGameCreator.Tests/WinForms/Goal146FeatureModuleComposerBindingTests.cs

# Current evidence
.llmgc/procedural/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix/featuremodule-composition-dashboard.json
.llmgc/procedural/goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix/generic-composer-scalability-dashboard.json
.llmgc/procedural/goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix/catalog-driven-coverage-proof.json
```

Inspect actual file/type names before modifying code.

# Part A — Persistent FeatureModule library

## Permanent source location

Create a durable repository source library:

```text
catalogs/feature-modules/
```

Recommended structure:

```text
catalogs/feature-modules/catalog.json
catalogs/feature-modules/core/*.featuremodule.json
catalogs/feature-modules/optional/*.featuremodule.json
```

This is source data, not generated evidence and not a user workspace.

Do not use `generator-library/**` in this goal.

## Library requirements

The library must contain the current:

```text
10 required core modules
3 optional profile modules
```

The three optional modules must preserve Goal142 lineage, mutation operations and current default values so the unparameterized Goal146 packages remain byte-identical.

A module file must be independently loadable and must contain at least:

```text
schemaVersion
moduleId
title
category
moduleKind
required
selectable
moduleVersion
dependencies[]
conflicts[]
requiredSchemaSections[]
requiredRuntimePrimitives[]
requiredValidationRules[]
requiredSaveLoadPolicy[]
requiredPlayerAdapterSurface[]
generatorInputs[]
authoringControls[]
goldenPackages[]
smokePlaythroughs[]
knownLimitations[]
futureExpansionNotes[]
mutationOperations[]
runtimeEffectContracts[]
parameterDefinitions[]
sourceLineage
```

## Generic file-based loader

Implement a loader, preferably under:

```text
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/
```

Suggested services:

```text
FeatureModuleLibraryLoader
FeatureModuleLibraryValidator
FeatureModuleLibraryFingerprintService
```

Requirements:

- discovers module files from the library manifest/directories;
- deterministic ordering by module ID;
- no module-ID-specific C# switch;
- validates duplicate IDs, duplicate file references, path escape, malformed JSON, unsupported schema version and mismatched counts;
- validates dependencies/conflicts against the full loaded library;
- validates operation/effect/parameter references;
- computes a stable SHA-256 fingerprint for each module definition;
- computes a catalog fingerprint from ordered module fingerprints;
- supports injecting an alternate library root for tests;
- adding a valid fourth module JSON file must not require modifying Composer, loader, Runtime, WinForms or coverage planner source.

`FeatureModuleCatalog.LoadFromGoal142` may remain as a migration/regression helper, but the normal Goal147 authoring path must load `catalogs/feature-modules/**` as the source of truth.

## Backward-compatible seeded library

Seeded module defaults must reproduce the current Goal146 composition package hashes and final-state hashes for all eight current compositions.

Required compatibility proof:

```text
seededLibraryCurrentEightPackageHashesPreserved=true
seededLibraryCurrentEightFinalHashesPreserved=true
```

# Part B — Typed parameter authoring

## Parameter definition contract

Introduce an application-level typed parameter schema. Do not add it to public GamePackage.

Every parameter definition includes at least:

```text
parameterId
moduleId
title
description
valueType
required
defaultValue
minimum
maximum
step
allowedValues[]
unit
authoringControl
bindings[]
validationRules[]
runtimeEffectIds[]
atomicGroupId
```

Supported initial value types:

```text
integer
number
boolean
enum
```

Supported controls:

```text
numeric_up_down
check_box
combo_box
```

A binding must reference mutation operations declaratively:

```text
operationId
operationField=newValue
transformKind
atomicGroupId
```

The parameter engine must not branch on module ID or parameter ID.

## Seeded authoring parameters

Define useful bounded parameters for the current optional modules.

At minimum:

### Alchemy

```text
healingPotionOutput
startingRedHerbQuantity
startingWaterFlaskQuantity
```

### Combat

```text
goblinStartingHealth
basicAttackDamage
```

`basicAttackDamage` must atomically update both the ability power and the matching damage-effect amount so they cannot diverge.

### Exploration/resource

```text
appleYield
logYield
transactionPotionOutput
```

Where one parameter controls multiple mutation operations, all bindings must apply atomically.

Default parameter values must equal the existing Goal146 mutation values.

## Parameter value document

Create a typed composition parameter value representation.

Requirements:

- unknown parameter rejected;
- parameter belonging to an unselected module rejected;
- wrong type rejected;
- range violation rejected;
- invalid enum rejected;
- step violation rejected when a step is declared;
- duplicate parameter rejected;
- missing required value resolves to declared default;
- canonical ordering by `moduleId`, then `parameterId`;
- deterministic serialization;
- applying the same values in any input order produces byte-identical GamePackage JSON.

## Generic parameter binding engine

Implement, preferably:

```text
FeatureModuleParameterValidator
FeatureModuleParameterBindingService
FeatureModuleParameterizedCompositionPlanner
```

The binding engine must:

1. start from module mutation operations;
2. resolve parameter defaults and overrides;
3. apply bindings to copied operations;
4. validate all referenced operation IDs exist exactly once;
5. reject duplicate/conflicting bindings;
6. apply atomic groups transactionally;
7. return an effective mutation plan and parameter audit;
8. never mutate source module definitions.

No module-specific C# branches are allowed.

# Part C — Saved composition documents

## User workspace

Use a repository-local but untracked default workspace:

```text
.llmgc/workspace/featuremodule-compositions/
```

Add a precise `.gitignore` rule for:

```text
.llmgc/workspace/
```

Do not ignore `.llmgc/procedural/**` or `.llmgc/exports/**`.

Normal source/catalog files remain tracked. User composition documents remain untracked.

The persistence service must also accept an alternate workspace root for tests and external callers.

## Saved composition contract

Create a versioned document:

```text
schemaVersion
compositionId
displayName
description
baseCandidateId
selectedModuleIds[]
parameterValues[]
catalogFingerprint
moduleFingerprints{}
createdAtUtc
updatedAtUtc
lastMaterializedPackageSha256
lastQualifiedFinalStateHash
lastQualificationStatus
revision
```

Timestamps may be supplied by an injected clock for deterministic tests.

## Persistence service

Implement, preferably:

```text
FeatureModuleCompositionDocumentValidator
FeatureModuleCompositionPersistenceService
FeatureModuleCompositionWorkspaceIndex
FeatureModuleCompositionStalenessService
```

Required operations:

```text
CreateNew
Load
Save
SaveAs
List
Clone
Delete
```

Delete must be explicit and confined to the workspace root.

Requirements:

- atomic write through temporary file + replace/move;
- path traversal rejected;
- invalid composition ID rejected;
- duplicate ID rejected on SaveAs/Clone;
- corrupted JSON rejected without overwriting anything;
- deterministic canonical serialization;
- revision increments on successful save;
- load does not automatically materialize or execute Runtime;
- source document remains unchanged when validation/materialization fails;
- stale status when catalog fingerprint or any selected module fingerprint changed;
- missing selected module produces a clear unresolved state, not silent fallback;
- no fallback to Goal142 selected candidate, baseline, Goal131 or sample package.

## Composition lifecycle

Required workflow:

```text
load module library
→ create/open saved composition
→ choose modules
→ edit typed parameters
→ validate
→ save
→ materialize selected composition
→ qualify through shared Runtime
→ update last materialization/qualification metadata only after success
```

Saving and materializing are separate operations.

# Part D — Incremental module certification

## Problem to solve

The current bounded coverage planner puts every singleton composition inside `maxTotalRows=24` and throws when all singleton rows no longer fit. This must not remain the normal scaling model.

Per-module certification is linear and belongs in a separate incremental ledger. Interaction-composition coverage remains bounded independently.

## Certification model

Introduce, preferably under:

```text
src/LLMGameCreator.Application/Design/FeatureModuleCertification/
```

Suggested services:

```text
FeatureModuleCertificationPlanner
FeatureModuleCertificationService
FeatureModuleCertificationCache
FeatureModuleCertificationFingerprintService
```

A certification entry includes:

```text
moduleId
moduleFingerprint
dependencyFingerprint
basePackageSha256
runtimeQualifierContractVersion
actionPlanSignature
parameterDefaultsFingerprint
status
structuralValidationPassed
defaultParameterValidationPassed
materializationPassed
packageValidationPassed
runtimeQualificationPassed
runtimeEffectsPassed
checkpointReloadPassed
fullReplayEquivalent
actionBindingPassed
certifiedAtUtc
diagnostics[]
```

## Cache behavior

Default cache root:

```text
.llmgc/workspace/featuremodule-certification-cache/
```

Requirements:

- cache key includes module fingerprint, dependency fingerprints, base package SHA and Runtime qualifier/action-plan contract version;
- first certification of a module executes the required checks;
- an unchanged module reuses a valid cache entry after hash validation;
- changing one module invalidates that module and modules whose declared dependency fingerprint changes;
- unrelated modules remain reusable;
- corrupt cache rejected and regenerated;
- no cached entry may claim GREEN when package/Runtime contract version changed;
- cache is an optimization, never the source of gameplay truth.

## Separate certification from interaction coverage

Refactor coverage semantics:

```text
module certification ledger:
  one entry per optional module, incremental/cacheable

composition interaction matrix:
  baseline
  operator-selected
  all-enabled when compatible
  bounded pairwise rows
  declared shared-target/runtime-dimension interaction rows
  deterministic sampled rows
```

The composition matrix must not require every singleton row.

Required behavior:

```text
100 optional modules
maxTotalRows=24
→ no exception
→ composition interaction rows <=24
→ selected composition included
→ baseline included
→ all optional modules represented in certification plan/ledger
→ no 2^100 enumeration
```

For the current three-module catalog preserve exhaustive eight-row behavior and all existing hashes.

## Small-catalog conflicts

For exhaustive small catalogs:

- enumerate compatible combinations as executable GREEN rows;
- record incompatible combinations as rejected coverage evidence with diagnostics;
- do not require a declared-invalid combination to materialize or pass Runtime;
- current three-module catalog remains eight compatible rows.

## Multi-effect module accounting

A module may declare multiple runtime effect contracts.

Separate these values:

```text
effectObservationCount
passedEffectObservationCount
selectedModuleCount
satisfiedSelectedModuleCount
```

A module is satisfied only when all its required effect contracts pass.

Do not use:

```text
passed observation count == selected module count
```

as a generic correctness rule.

Add a synthetic module with at least two effect contracts and prove that one module may produce two observations while `satisfiedSelectedModuleCount=1`.

# Part E — Materialize and qualify a saved parameterized composition

## Required default regression

Materializing the seeded all-three composition with only default parameter values must preserve the Goal146 selected package/final hashes:

```text
packageSha256=9a83d47e8e2ae541e7789b804c32f489acb8e7525c0a9dc32a7cc8be8822d65a
finalStateHash=d5ad29ee7c350918681c2859b80f5d2944834a6414918a16d8b4e1c0746753b9
```

## Required customized composition

Create a saved all-three composition in a temporary test workspace with at least four non-default parameters spanning all three optional modules.

It must:

```text
save/load roundtrip identically
validate typed values
materialize a package distinct from default Goal146
pass GamePackage validation
pass shared Runtime qualification
pass checkpoint reload
pass full replay
pass action binding
satisfy all selected modules' runtime effect contracts
produce deterministic package/final hashes across repeated runs
```

Record concrete parameter values and resulting semantic state in artifacts.

Do not hardcode expected final hash before executing; compute it and verify repeatability.

# Part F — WinForms authoring workflow

## UX direction

Do not add another isolated top-level Goal-number tab when the existing Goal146 Module Composer can be evolved.

Preserve the existing Goal146 tab and add a bounded Goal147 authoring surface within it, using a sub-tab/group/panel if necessary.

Required UI:

- persistent module library status/fingerprint;
- required modules shown locked;
- optional module checked list loaded from library files;
- dynamically generated typed parameter editor for selected modules;
- saved-composition list;
- composition ID, title and description;
- dirty/stale/valid status;
- validation diagnostics;
- last materialized package SHA;
- last Runtime qualification/final-state hash;
- certification ledger summary;
- bounded interaction coverage summary.

Required controls:

```text
New Composition
Open
Save
Save As / Clone
Delete
Validate
Materialize & Qualify
Refresh Library
```

One primary milestone action:

```text
Save, Materialize & Qualify
```

## Dynamic UI requirements

- no module-ID-specific control creation;
- controls derive from parameter definition metadata;
- numeric parameter → NumericUpDown;
- boolean → CheckBox;
- enum → ComboBox;
- programmatic load/binding does not mark the document dirty or trigger materialization;
- user edits mark dirty exactly once;
- module selection changes refresh parameters but do not materialize;
- missing/stale module state shown clearly;
- all normal actions use in-process Application services;
- no compiler, test or PowerShell child process;
- disable relevant controls while materializing/qualifying;
- preserve Goal145A selection-event fix and Goal142A self-lock fix.

Add behavioral STA tests with real controls or a production testable binder/coordinator. Source-string tests alone are insufficient.

# Part G — Unity read-only consumption

Add or update a read-only Unity window:

```text
LLMGameCreator/Accepted Alpha/Saved FeatureModule Composition
```

Unity reads Goal147 generated qualification artifacts only.

It may display:

- saved composition ID/revision;
- module IDs and parameter values;
- catalog/module fingerprints;
- stale status;
- package SHA;
- Runtime final hash;
- semantic effects;
- checkpoint/full replay/action binding status.

Unity must not:

- edit or persist composition documents;
- apply parameters;
- materialize GamePackage;
- execute gameplay;
- become gameplay truth.

Batch smoke requirements:

```text
savedCompositionLoaded=true
catalogFingerprintMatches=true
selectedModuleFingerprintsMatch=true
parameterValuesLoaded=true
packageShaMatches=true
runtimeQualificationPassed=true
checkpointReloadPassed=true
fullReplayEquivalent=true
actionBindingPassed=true
runtimeAuthority=true
unityGameplayTruth=false
passMarkerPresent=true
failMarkerPresent=false
unityExitCode=0
```

# Required source artifacts

Create tracked source library files under:

```text
catalogs/feature-modules/**
```

No generated artifacts belong there.

# Required Goal147 evidence artifacts

Write under both:

```text
.llmgc/procedural/goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification/
.llmgc/exports/goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification/
```

At minimum:

```text
featuremodule-library-index.json
featuremodule-library-validation.json
featuremodule-parameter-schema.json
featuremodule-default-hash-compatibility-proof.json
saved-composition-roundtrip-proof.json
parameterized-composition-materialization-proof.json
module-certification-ledger.json
module-certification-cache-proof.json
bounded-interaction-coverage-proof.json
hundred-module-scalability-proof.json
multi-effect-module-proof.json
featuremodule-authoring-dashboard.json
featuremodule-authoring-negative-proof.json
featuremodule-authoring-file-index.json
one-click-featuremodule-authoring-report.json
one-click-featuremodule-authoring-report.md
unity-saved-featuremodule-composition-smoke.json

selected-composition/composition.json
selected-composition/effective-parameter-values.json
selected-composition/effective-mutation-plan.json
selected-composition/parameter-audit.json
selected-composition/package.json
selected-composition/package-validation.json
selected-composition/session-state.json
selected-composition/action-catalog.json
selected-composition/journal.json
selected-composition/checkpoint.json
selected-composition/checkpoint-replay-result.json
selected-composition/final-replay-result.json
selected-composition/runtime-effect-observations.json
```

Evidence composition files are bounded copies for review. User workspace documents remain untracked.

File index includes SHA-256 for every required artifact.

# Dashboard markers

Required:

```text
status=GREEN
persistentFeatureModuleLibrary=true
moduleLibraryFileBased=true
moduleLibrarySourceOfTruth=true
publicGamePackageSchemaChanged=false
requiredCoreModuleCount=10
optionalModuleCount=3
moduleFingerprintingPassed=true
catalogFingerprintingPassed=true
addingModuleFileRequiresNoComposerCodeChange=true
typedParameterAuthoring=true
parameterDefinitionCount>=8
genericParameterBinding=true
atomicParameterGroupsPassed=true
defaultParametersPreserveGoal146Hashes=true
savedCompositionPersistence=true
savedCompositionRoundtripPassed=true
savedCompositionAtomicWritePassed=true
savedCompositionStalenessDetectionPassed=true
incrementalModuleCertification=true
allOptionalModulesCertified=true
unchangedCertificationCacheReusePassed=true
changedModuleSelectiveInvalidationPassed=true
interactionCoverageIndependentFromSingletonCertification=true
hundredModuleCatalogAccepted=true
hundredModuleInteractionRowCount<=24
hundredModulePowersetEnumerated=false
selectedCompositionAlwaysIncluded=true
smallCatalogCompatibleExhaustiveCoveragePassed=true
smallCatalogInvalidCombinationsClassified=true
multiEffectModuleAccountingPassed=true
customParameterizedCompositionPassed=true
customPackageDistinctFromDefault=true
customRuntimeQualificationPassed=true
operatorUsesInProcessService=true
unitySmokePassed=true
goal146Accepted=false
goal147Accepted=false
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
accepted=false
```

# Negative proof

Executable proofs should cover:

```text
moduleLibraryPathEscapeRejected
moduleFilePathEscapeRejected
malformedModuleJsonRejected
unsupportedModuleSchemaRejected
duplicateModuleIdRejected
duplicateModuleFileRejected
unknownDependencyRejected
conflictReferenceMismatchRejected
operationReferenceMismatchRejected
effectOperationReferenceMismatchRejected
unknownParameterRejected
unselectedModuleParameterRejected
wrongParameterTypeRejected
parameterRangeViolationRejected
parameterStepViolationRejected
invalidEnumRejected
duplicateParameterRejected
conflictingParameterBindingRejected
atomicGroupPartialApplyRejected
compositionWorkspacePathEscapeRejected
invalidCompositionIdRejected
corruptCompositionRejected
saveAsDuplicateRejected
missingSelectedModuleNoFallback
staleCompositionDetected
corruptCertificationCacheRejected
runtimeContractVersionInvalidatesCache
catalogAboveTwentyModulesDoesNotThrow
fullPowersetAboveSmallLimitAbsent
incompatibleSmallCatalogCombinationNotExecuted
multiEffectCountNotComparedToModuleCount
moduleIdSpecificParameterBranchAbsent
compositionIdSpecificRuntimeBranchAbsent
unityDoesNotAuthorMaterializeOrExecute
winFormsStartsNoCompilerOrTestProcess
previousArtifactsPreservedOnFailure
```

Do not satisfy executable requirements only with boolean constants or source scans.

# Normal command

Add:

```text
.devflow\scripts\run-featuremodule-authoring-persistence-and-certification.cmd
.devflow/scripts/run-featuremodule-authoring-persistence-and-certification.ps1
```

Parameters:

```text
-CatalogRoot
-WorkspaceRoot
-CertificationCacheRoot
-CompositionId
-UnityPath
-DryRun
-ApplyCleanup
```

The normal automated command must use a temporary workspace/cache by default unless explicit paths are supplied, so validation does not write user workspace state.

Script requirements:

1. resolve/guard repository paths;
2. refuse `.llmgc/manual/**`;
3. load and validate tracked module library;
4. run default-hash compatibility proof;
5. run saved-composition persistence/parameter proof in a temporary workspace;
6. run certification cache proof;
7. run 100-module bounded coverage proof;
8. run selected composition materialization and shared Runtime qualification;
9. run Unity read-only batch smoke;
10. rerun core proof requiring Unity smoke;
11. write only Goal147 procedural/export evidence;
12. transactional backup/rollback outside repository;
13. non-zero exit on any validation/materialization/Runtime/replay/Unity failure.

The external script may execute tests and Unity. WinForms must not.

# Backward compatibility

Required regressions:

```text
Goal146 current three-module coverage remains exhaustive_small_catalog
Goal146 current composition count remains 8
all eight Goal146 package hashes unchanged
all eight Goal146 final hashes unchanged
Goal146 selected package/final hash unchanged
Goal146A synthetic fourth module proof remains GREEN
Goal146A 12-module bounded proof remains GREEN
Goal145 Runtime matrix remains GREEN
Goal145 selector proof remains GREEN
Goal144 action binding remains GREEN
```

Do not rewrite Goal142–146A historical artifacts as Goal147 outputs.

# Allowed paths

Only create/modify:

```text
.gitignore
catalogs/feature-modules/**

docs/agent-tasks/goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-featuremodule-authoring-persistence-and-certification.ps1
.devflow/scripts/run-featuremodule-authoring-persistence-and-certification.cmd

.llmgc/procedural/goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification/**
.llmgc/exports/goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix.md
docs/manual-acceptance/persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification.md

src/LLMGameCreator.Application/Design/FeatureModuleComposition/**
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/**
src/LLMGameCreator.Application/Design/FeatureModuleCertification/**
src/LLMGameCreator.Application/Design/ProductLineRuntimeQualification/**
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantMaterializer.cs
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineRuntimeVariantMatrixModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineModels.cs
src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/ProductLineStrategyRebaselineService.cs
src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**

src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal146.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal147.cs

unity/LLMGameCreatorAlpha/Assets/Scripts/CanonicalRuntimeUnitySavedFeatureModuleCompositionHarness.cs
unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnitySavedFeatureModuleCompositionWindow.cs

tests/LLMGameCreator.Tests/Application/FeatureModuleComposition/**
tests/LLMGameCreator.Tests/Application/FeatureModuleAuthoring/**
tests/LLMGameCreator.Tests/Application/FeatureModuleCertification/**
tests/LLMGameCreator.Tests/Application/ProductLineRuntimeQualification/**
tests/LLMGameCreator.Tests/Application/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantMaterializerTests.cs
tests/LLMGameCreator.Tests/Application/ProductLineInteractiveSessionMatrix/ProductLineInteractiveSessionMatrixTests.cs
tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/FeatureModuleAuthoringScriptProof.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal146Tests.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal147Tests.cs
tests/LLMGameCreator.Tests/Devflow/RunFeatureModuleAuthoringPersistenceAndCertificationScriptTests.cs
tests/LLMGameCreator.Tests/WinForms/Goal146FeatureModuleComposerBindingTests.cs
tests/LLMGameCreator.Tests/WinForms/Goal147FeatureModuleAuthoringBindingTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

Use the actual existing ProductLine Runtime model filename if it differs. Do not create a duplicate type file.

# Forbidden paths

Do not modify, stage or commit:

```text
.llmgc/manual/**
.llmgc/workspace/**

samples/minimal-map-game/**

.llmgc/procedural/goal-142*/**
.llmgc/exports/goal-142*/**
.llmgc/procedural/goal-143*/**
.llmgc/exports/goal-143*/**
.llmgc/procedural/goal-144*/**
.llmgc/exports/goal-144*/**
.llmgc/procedural/goal-145*/**
.llmgc/exports/goal-145*/**
.llmgc/procedural/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix/**
.llmgc/exports/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix/**
.llmgc/procedural/goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix/**
.llmgc/exports/goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix/**

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

The `.gitignore` change is allowed, but no actual `.llmgc/workspace/**` file may be staged.

No public GamePackage schema change.
No Runtime gameplay implementation change.
No sample mutation.
No provider/network/LLM/Lua work.
No new dependency.

# Validation

Run sequentially.

## Restore/build

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
```

Required:

```text
0 warnings
0 errors
```

## Focused tests

```powershell
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~FeatureModuleAuthoring|FullyQualifiedName~FeatureModuleCertification|FullyQualifiedName~FeatureModuleComposition|FullyQualifiedName~Goal147|FullyQualifiedName~Goal146|FullyQualifiedName~ProductLineRuntimeQualification|FullyQualifiedName~ProductLineRuntimeVariantMaterializer|FullyQualifiedName~ProductLineInteractiveSessionMatrix|FullyQualifiedName~VisualWorldStreamPreviewWorkspace|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection"
```

## Normal command

```powershell
.\.devflow\scripts\run-featuremodule-authoring-persistence-and-certification.ps1 -DryRun
.\.devflow\scripts\run-featuremodule-authoring-persistence-and-certification.ps1 -ApplyCleanup
```

Run a second time to prove cache reuse/determinism:

```powershell
.\.devflow\scripts\run-featuremodule-authoring-persistence-and-certification.ps1 -ApplyCleanup
```

## Required regressions

```powershell
.\.devflow\scripts\run-featuremodule-composer-scalability-hotfix.ps1 -DryRun
.\.devflow\scripts\run-featuremodule-composition-runtime-matrix.ps1 -DryRun
.\.devflow\scripts\run-product-line-interactive-session-matrix.ps1 -DryRun
```

Do not commit regenerated historical artifacts.

## Guards

```powershell
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun

git diff --check
git diff --cached --check
git status --short --untracked-files=all
git diff --name-only
git diff --cached --name-only
git ls-files .llmgc/manual
git ls-files .llmgc/workspace
```

Required:

```text
no tracked .llmgc/manual files
no tracked .llmgc/workspace files
forbidden diff empty
```

Check changed text files for mojibake markers and escaped Cyrillic. Required: zero matches.

Validation-generated churn outside Goal147 allowlist must be restored only by exact paths computed from the Goal147 artifact-scope scenario.

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

# Current-state updates

After GREEN update truthfully:

```text
goal146Accepted=false
goal146ManualReviewDeferred=true
goal147Accepted=false
persistentFeatureModuleLibrary=true
moduleLibrarySourceOfTruth=true
typedFeatureModuleParameters=true
savedFeatureModuleCompositions=true
incrementalFeatureModuleCertification=true
interactionCoverageDecoupledFromModuleCertification=true
hundredModuleCatalogAccepted=true
hundredModuleInteractionRowCount<=24
allCurrentOptionalModulesCertified=true
defaultParameterGoal146HashesPreserved=true
customParameterizedCompositionQualified=true
featureModuleWorkspaceIgnored=true
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
nextProductGoal=review_goals_146_147_featuremodule_composer_authoring_workflow
```

Do not mark Goal141 accepted.

# Publication

Before staging:

```text
tracked module library GREEN
parameter authoring GREEN
saved composition persistence GREEN
incremental certification/cache GREEN
100-module bounded interaction coverage GREEN
current Goal146 hashes preserved
custom parameterized composition Runtime-qualified
Unity smoke GREEN
Goal146 accepted=false
Goal147 accepted=false
artifact scope clean
forbidden diff empty
.llmgc/workspace untracked
```

Stage only explicit Goal147 allowlisted paths.

Required commit message:

```text
GREEN Goal 147 persistent FeatureModule registry typed parameter authoring saved compositions and incremental certification
```

Push:

```text
git push origin main
```

Final report must include:

- status and commit SHA;
- module library path/module counts/catalog fingerprint;
- parameter definition count and parameterized composition values;
- default Goal146 hash compatibility;
- saved composition create/save/load/clone/delete/stale results;
- certification executed/reused/invalidated counts;
- 100-module certification-plan count and bounded interaction-row count;
- multi-effect module result;
- custom package SHA/final-state hash/semantic effects;
- WinForms in-process status;
- Unity read-only smoke;
- test counts;
- artifact scope;
- forbidden diff;
- `.llmgc/workspace` untracked;
- clean worktree and `HEAD == origin/main`.

Do not claim GREEN when any library, parameter, persistence, certification, coverage, materialization, Runtime, replay, Unity or scope gate failed.
