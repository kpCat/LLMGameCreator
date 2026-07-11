# Goal 148C — Project Identity Preservation + Project-Scoped Composition Hotfix

## Identity

- Task ID: `goal-148c-project-identity-preservation-and-project-scoped-composition-hotfix`
- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `4d0d21275a21b2ac9ce2c9cf0a7580e62cf49a03` or a direct descendant

This is a fresh Codex dialog. This file is the complete instruction source.

## Goal type

Focused P1 hotfix before Goal148 acceptance.

Do not start Goal149.
Do not mark Goal148 accepted.
Do not add Goal-number UI.
Do not rewrite historical Goal146–148B artifact roots.

## Current state

```text
Goal146 accepted=true by human
Goal147 accepted=true by human
Goal148 implementation=GREEN, accepted=false
Goal148A implementation=GREEN
Goal148B implementation=GREEN
Goal148 manual retry still required
Goal141 accepted=false
```

## Real manual result to record

The human repeated the real Goal148 workflow after Goal148B.

Project folder:

```text
C:\Users\endim\AppData\Local\LLMGameCreator\Games\goal148-manual
```

The build completed with no cross-thread error:

```text
Игра успешно собрана и проверена.
Механик включено: 13
Параметров настроено: 6
Сохранение/загрузка: пройдено
Повтор действий: пройден
Файлы проекта подготовлены: 1
Пакет проекта обновлён
```

Current values:

```text
healingPotionOutput=3
startingRedHerbQuantity=4 (default)
startingWaterFlaskQuantity=2 (default)
basicAttackDamage=5
goblinStartingHealth=18
appleYield=4
logYield=4
transactionPotionOutput=3
```

Observed composition/template package SHA:

```text
e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221
```

Observed final Runtime state hash:

```text
95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8
```

However the project title changed from:

```text
Проверка конструктора
```

to:

```text
Minimal Map Game
```

The MainForm status still displayed `Проверка конструктора` only because Goal148B cached the previous title for the same folder.

Record:

```text
goal148Accepted=false
manualRetryRequired=true
manualBuildExecutionPassed=true
manualCrossThreadFailureResolved=true
manualFailureClass=project_identity_overwritten_by_template_manifest
rawScreenshotsNotCommitted=true
```

## Defect A — template identity overwrites the user's game

The unified project build materializes from the immutable Goal142 baseline. The resulting package retains baseline `manifest.packageId` and `manifest.title`, and Goal148 replaces the opened project's `package.json` with it.

A successful build must preserve the user's project identity:

```text
PackageId
Title
Version
FormatVersion
Description
```

The generated package may own:

```text
StartMapId
Game
AssetCatalog
ScriptCatalog
GeneratedContent
composition/profile/source-context metadata
```

No successful build may rename a project to `Minimal Map Game`.

## Defect B — fixed Goal147 composition ID

The unified project workspace currently uses:

```text
goal147-custom-alchemy-combat-exploration
```

for every project's composition ID and filename.

Replace this normal-path identity with a deterministic project-scoped ID derived from package ID, for example:

```text
project-game-goal148-manual-<stable-short-hash>
```

Requirements:

```text
valid for existing document validator
filename-safe
deterministic
collision-resistant
contains no Goal number
different package IDs produce different IDs
title changes do not silently change it
```

## Defect C — stale MainForm title cache

On every marshalled `CurrentChanged` event, MainForm must refresh the displayed title from the current package. It must not preserve an old same-folder title.

After build all must agree:

```text
workspace title
overview title
MainForm status
activated package Manifest.Title
```

## Project identity sidecar

Create an editor-side document:

```text
<project>/.llmgc/project-identity.json
```

Suggested schema:

```text
schemaVersion=game_project_identity_v1
packageId
title
version
formatVersion
description
createdAtUtc
updatedAtUtc
source=created_project_package | migrated_legacy_workspace | recovered_after_template_overwrite
recoveryDiagnostics[]
```

This is not a public GamePackage schema change.

### Capture rules

On project open:

1. Validate and use the sidecar when present.
2. Otherwise capture identity from the current project package before build.
3. Write it atomically.
4. Opening a project must not replace `package.json`.

### Recovery for the already affected project

When the sidecar is absent, a legacy authoring document exists, and the current package is clearly a Goal146/147 composed template package, recover generically:

```text
title = authoring DisplayName when meaningful
packageId = "game/" + normalized project-folder name
version = 0.1.0 when current version is a generated Goal146 composition version
formatVersion = current supported format
description = meaningful authoring description or normal project description
source = recovered_after_template_overwrite
```

For the manual folder this must recover:

```text
title=Проверка конструктора
packageId=game/goal148-manual
version=0.1.0
```

Do not hardcode the folder/title.

If recovery is ambiguous, fail safely and show an actionable diagnostic.

## Legacy authoring migration

Existing projects may contain:

```text
.llmgc/authoring/goal147-custom-alchemy-combat-exploration.featurecomposition.json
```

When the new project-scoped document is absent:

1. Load the legacy document.
2. Preserve selected modules and every parameter value.
3. Replace `CompositionId` with the project-scoped ID.
4. Set `DisplayName` to project identity title.
5. Clear/mark stale the old package SHA because it used template identity.
6. Preserve final-state evidence only as previous evidence.
7. Atomically save the new document.
8. Preserve or archive the legacy file without data loss.
9. Use only the project-scoped document afterward.

The human must not re-enter current values.

## Honest hash semantics

Separate:

```text
CompositionPackageSha256
  generic modules/parameters package before project identity overlay

ActivatedProjectPackageSha256
  final bytes saved as the user's package.json

FinalStateHash
  canonical Runtime final state for the activated package
```

Rules:

- Existing Goal146/147 historical hashes remain unchanged.
- `GameProjectBuildResult.PackageSha256` should mean the activated project package SHA, or be replaced by an unambiguous equivalent.
- Add `CompositionPackageSha256`.
- Technical details show both.
- Two projects with identical mechanics but different identity:
  - same composition package SHA;
  - same final state hash;
  - different activated package SHA.

Current manual values must preserve:

```text
CompositionPackageSha256=e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221
FinalStateHash=95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8
```

Historical control values with `logYield=3` must preserve:

```text
CompositionPackageSha256=2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991
FinalStateHash=80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e
```

## Build path

Prefer project-specific services under:

```text
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/
```

Suggested components:

```text
GameProjectIdentityDocument
GameProjectIdentityStore
GameProjectIdentityRecoveryService
GameProjectCompositionIdentityService
GameProjectPackageIdentityOverlayService
```

Required build sequence:

```text
capture/validate identity
materialize generic composition
record CompositionPackageSha256
overlay project manifest identity into staged package
compute ActivatedProjectPackageSha256
validate identity-overlaid package
run the existing canonical ProductLineRuntimeQualifier against the identity-overlaid package
assert identity overlay does not change gameplay final-state semantics
prepare support files
staged validation
transactional activation
real-project validation
current-package replacement
```

Do not create another Runtime or action plan.

Prefer project-specific overlay/requalification rather than changing generic Goal146/147 materialization. A bounded optional identity input is allowed only when omitted behavior remains byte-identical.

## Transaction and rollback

Include sidecar and migrated authoring files in rollback.

On failure restore:

```text
package.json
current in-memory package
project identity sidecar
new project-scoped authoring document
legacy authoring document
last successful hashes
support files
```

## UI

Technical details must show:

```text
Project package ID
Project title
Project version
Project-scoped composition ID
Composition package SHA-256
Activated project package SHA-256
Final Runtime state hash
Identity source/recovery status
```

After build no normal UI area may show `Minimal Map Game` as the manual project's title.

## Required executable tests

### Legacy manual-project migration

Create affected state:

```text
folder=goal148-manual
current package title=Minimal Map Game
legacy Goal147 authoring document
DisplayName=Проверка конструктора
logYield=4 and other current values
no identity sidecar
```

Prove:

```text
identity title=Проверка конструктора
identity packageId=game/goal148-manual
identity version=0.1.0
project-scoped composition created
all selections/values preserved
legacy document preserved/archived
open/migration alone does not replace package.json
```

### Real build under MainForm

Through real production services:

```text
MainForm
ProjectsPageControl
GameProjectService/CreateAsync or migrated project
UnifiedGameProjectWorkspaceController
```

Prove:

```text
build GREEN
cross-thread exception absent
workspace title remains Проверка конструктора
status remains Открыт проект: Проверка конструктора
activated manifest title/packageId/version preserved
CompositionPackageSha256=e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221
FinalStateHash=95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8
ActivatedProjectPackageSha256 non-empty
ActivatedProjectPackageSha256 != CompositionPackageSha256
support file prepared/reused
```

### Historical control

With `logYield=3` prove the historical composition/final hashes above and preserved identity.

### Two-project isolation

Two identities with identical mechanics:

```text
same composition SHA
same final state hash
different activated package SHA
own title/packageId/version preserved
```

### Repeat build

```text
same composition SHA
same activated SHA
same final hash
same sidecar
same project-scoped composition ID
support reused
```

### Rollback

Inject failure after overlay/support activation and prove full identity/package/authoring/support rollback.

### No fixed Goal147 normal identity

Prove the unified normal path no longer uses the fixed Goal147 composition ID.

## Regressions

Preserve:

```text
Goal148B UI-thread dispatch GREEN
unsafe CurrentChanged subscriber count=0
Goal148A support files GREEN
normal Goal-number controls=0
legacy diagnostics hidden by default
Goal146/147 accepted=true
Goal148 accepted=false
Goal141 accepted=false
```

## Required artifacts

Write under both:

```text
.llmgc/procedural/goal-148c-project-identity-preservation-and-project-scoped-composition-hotfix/
.llmgc/exports/goal-148c-project-identity-preservation-and-project-scoped-composition-hotfix/
```

At minimum:

```text
goal148-manual-project-identity-failure-record.json
project-identity-capture-proof.json
legacy-authoring-migration-proof.json
project-scoped-composition-identity-proof.json
manual-values-project-build-proof.json
historical-control-values-proof.json
two-project-identity-isolation-proof.json
identity-repeat-build-proof.json
identity-rollback-proof.json
goal148c-regression-compatibility-proof.json
goal148c-negative-proof.json
goal148c-dashboard.json
goal148c-file-index.json
goal148c-report.md
```

Dashboard markers include:

```text
status=GREEN
manualIdentityFailureRecorded=true
projectIdentitySidecar=true
projectIdentityPreserved=true
legacyAuthoringMigrated=true
fixedGoal147CompositionIdAbsent=true
projectScopedCompositionId=true
manualProjectTitle=Проверка конструктора
manualProjectPackageId=game/goal148-manual
manualProjectVersion=0.1.0
manualValuesCompositionPackageSha256=e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221
manualValuesFinalStateHash=95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8
manualValuesActivatedPackageSha256NonEmpty=true
historicalControlCompositionHashPreserved=true
historicalControlFinalHashPreserved=true
twoProjectsSameCompositionHash=true
twoProjectsDifferentActivatedPackageHash=true
mainFormAndWorkspaceTitleConsistent=true
goal148BRegressionGreen=true
goal148ARegressionGreen=true
goal148Accepted=false
accepted=false
```

## Current-state update

After GREEN:

```text
current_phase_title=Goal 148C project identity preservation and project-scoped composition hotfix
goal148Accepted=false
goal148ManualRetryRequired=true
goal148ProjectIdentityPreserved=true
goal148ProjectScopedCompositionId=true
goal148HonestHashSemantics=true
goal148ManualProjectMigrationReady=true
nextProductGoal=retry_goal_148_unified_game_project_workspace_manual_verification
```

Do not mark Goal141 accepted.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/AUTOMATED_VALIDATION_TIERS.md

docs/agent-tasks/goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation/GOAL.md
docs/agent-tasks/goal-148a-new-project-required-support-files-and-transactional-activation-hotfix/GOAL.md
docs/agent-tasks/goal-148b-current-package-ui-thread-dispatch-and-real-workspace-build-retry-hotfix/GOAL.md

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/**
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterizedCompositionService.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleCompositionDocumentModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleCompositionPersistenceService.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionService.cs
src/LLMGameCreator.Application/Design/ProductLineRuntimeQualification/**
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/ProductLineRuntimeVariantMaterializer.cs

src/LLMGameCreator.WinForms/MainForm.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
```

## Allowed paths

```text
docs/agent-tasks/goal-148c-project-identity-preservation-and-project-scoped-composition-hotfix/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal148c-project-identity-hotfix.ps1
.devflow/scripts/run-goal148c-project-identity-hotfix.cmd

.llmgc/procedural/goal-148c-project-identity-preservation-and-project-scoped-composition-hotfix/**
.llmgc/exports/goal-148c-project-identity-preservation-and-project-scoped-composition-hotfix/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/unified-game-project-workspace-and-legacy-goal-diagnostics-isolation.md

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/**
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleCompositionDocumentModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleCompositionPersistenceService.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterizedCompositionService.cs

src/LLMGameCreator.WinForms/MainForm.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs

tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/**
tests/LLMGameCreator.Tests/WinForms/UnifiedGameProjectWorkspaceTests.cs
tests/LLMGameCreator.Tests/WinForms/CurrentGamePackageUiThreadDispatchTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal148CProjectIdentityHotfixScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/ProjectsPageProductSmokeTests.cs
```

Prefer implementation under UnifiedGameProjectWorkspace.

## Forbidden paths

Do not modify/stage:

```text
.llmgc/manual/**
.llmgc/workspace/**
catalogs/feature-modules/**
samples/minimal-map-game/**
all historical Goal142–148B procedural/export roots

src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Generation/**
src/LLMGameCreator.AssetPipeline/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.Application/Projects/**
src/LLMGameCreator.Application/Validation/**
src/LLMGameCreator.Application/Design/FeatureModuleComposition/**
src/LLMGameCreator.Application/Design/ProductLineRuntimeQualification/**
src/LLMGameCreator.Application/Design/ProductLineRuntimeVariantMatrix/**
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

Do not modify generic composition/materializer/qualifier unless a bounded optional identity input is necessary and omitted behavior is byte-identical.

No public schema, Runtime gameplay, module catalog, Unity or dependency changes.

## Validation

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore

dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~Goal148C|FullyQualifiedName~UnifiedGameProjectWorkspace|FullyQualifiedName~CurrentGamePackageUiThreadDispatch|FullyQualifiedName~ProjectsPage"

.\.devflow\scripts\run-goal148c-project-identity-hotfix.ps1 -DryRun
.\.devflow\scripts\run-goal148c-project-identity-hotfix.ps1 -ApplyCleanup

.\.devflow\scripts\run-goal148b-current-package-ui-thread-hotfix.ps1 -DryRun
.\.devflow\scripts\run-goal148a-new-project-support-files-hotfix.ps1 -DryRun
.\.devflow\scripts\run-unified-game-project-workspace.ps1 -DryRun
.\.devflow\scripts\run-featuremodule-authoring-persistence-and-certification.ps1 -DryRun

.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-148c-project-identity-preservation-and-project-scoped-composition-hotfix
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun

git diff --check
git diff --cached --check
git status --short --untracked-files=all
git diff --name-only
git diff --cached --name-only
git ls-files .llmgc/manual .llmgc/workspace
```

Required: 0 warnings/errors, zero mojibake/escaped Cyrillic, forbidden diff empty.

Restore validation churn only through exact policy-derived paths. Do not use reset-hard, clean, broad restore, branch switching, merge, rebase or cherry-pick.

## Publication

Commit:

```text
GREEN Goal 148C project identity preservation and project-scoped composition hotfix
```

Push `origin main`.

Do not report GREEN if a successful project build can still replace the user's title/package ID/version with template identity, or if unified projects still use the fixed Goal147 composition ID.
