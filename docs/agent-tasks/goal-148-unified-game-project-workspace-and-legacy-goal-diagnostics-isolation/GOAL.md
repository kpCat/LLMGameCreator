# Goal 148 — Unified Game Project Workspace + Legacy Goal Diagnostics Isolation

## Identity

- Task ID: `goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation`
- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: commit `e210986d0d57483c00da2356c46b61a95e588700` or a direct descendant on `main`

## New-dialog rule

This task is executed in a fresh Codex dialog. This file is the complete instruction source. Do not rely on any other conversation.

## Goal type

Major product-UX consolidation goal.

Do not create another user-facing Goal-number tab.
Do not start a new gameplay mechanic.
Do not modify Runtime gameplay or the public GamePackage schema.

## User problem

The current WinForms application exposes a growing internal verification laboratory:

```text
Goal143 PlayerAdapter
Goal144 Live Session
Goal145 Variant Sessions
Goal146 Module Composer
...
```

The user reports that the application is becoming impossible to understand and that Goal-number tabs must not become the final UI.

The existing page titled `Игры` already owns game-project creation/open/save, while the accepted FeatureModule composer and authoring workflow lives inside the developer-oriented `Visual World Stream Preview` page.

Goal148 must create one coherent user-facing project workflow and isolate historical Goal surfaces as advanced diagnostics.

## First deliverable — record Goals146/147 human acceptance

Record exactly:

```text
Я принимаю Goals146/147 featuremodule_composer_and_authoring_workflow_verification GREEN. goal146Accepted=true, goal147Accepted=true, persistentFeatureModuleLibrary=true, moduleLibrarySourceOfTruth=true, requiredCoreModuleCount=10, optionalModuleCount=3, parameterDefinitionCount=8, catalogDrivenComposer=true, hardcodedCombinationTableAbsent=true, typedParameterAuthoring=true, savedCompositionPersistence=true, savedCompositionRoundtripPassed=true, incrementalModuleCertification=true, dependentModuleCertificationPassed=true, transitiveDependencyInvalidationPassed=true, hundredModuleCatalogAccepted=true, hundredModuleInteractionRowCount=9, programmaticItemCheckAppliedCount=0, operatorItemCheckAppliedCount=1, heavyWorkRunsOffUiThread=true, customPackageSha256=2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991, customFinalStateHash=80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e, checkpointReloadPassed=true, fullReplayEquivalent=true, actionBindingPassed=true, unitySmoke=GREEN, projectionOnly=false, runtimeAuthority=true, unityGameplayTruth=false.
```

Update:

```text
docs/manual-acceptance/featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix.md
docs/manual-acceptance/persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification.md
```

Write `goals146-147-human-acceptance-record.json` under both Goal148 artifact roots.

Required markers:

```text
goal146Accepted=true
goal147Accepted=true
acceptedByHuman=true
acceptedByCodex=false
rawManualInputNotCommitted=true
```

Goal148 itself remains:

```text
accepted=false
acceptedByHuman=false
acceptedByCodex=false
```

Do not mark Goal141 accepted.

## Product result

The normal user journey must become:

```text
Игры
→ create/open project
→ choose mechanics
→ configure parameters
→ save project
→ Собрать и проверить игру
→ see understandable result
```

The normal journey must not require:

```text
Visual World Stream Preview
Goal-number tabs
artifact groups
proof IDs
package SHA knowledge
manual JSON editing
Unity Editor
```

Technical hashes and proof details may remain available only in an explicitly advanced technical-details area.

## Primary workspace location

Evolve the existing `ProjectsPageControl` with `Title = Игры`.
Do not add another top-level user-facing page.

The page must remain responsible for games root, project list, new/open project, open arbitrary folder, and save current project.

Add the FeatureModule authoring/build workflow to the opened-project state.

## User-facing information architecture

When no project is open, show a simple project-start surface:

```text
Мои игры
Новая игра
Открыть выбранную
Открыть папку
```

When a project is open, show one workspace with sections:

```text
Обзор
Механики
Настройки
Сборка и проверка
Технические детали
```

Rules:

- no visible normal-workspace text may contain regex `\bGoal\d+\b`;
- no internal candidate/composition vocabulary as primary labels;
- friendly module titles are primary;
- module IDs only in tooltips or `Технические детали`;
- raw hashes hidden from main overview;
- no new top-level page.

### Обзор

Show project title/folder, package status, authoring status, selected mechanic count, last successful build, last Runtime qualification.

Use plain Russian states:

```text
Готово
Есть несохранённые изменения
Требуется пересборка
Не хватает модулей
Есть ошибки
Проверка ещё не запускалась
```

### Механики

Show required core mechanics as locked friendly rows and optional modules as selectable friendly rows with title, description, category, dependencies/conflicts.

For the current library, present friendly equivalents of:

```text
Углублённая алхимия
Усиленный бой
Расширенный сбор ресурсов
```

Do not rename IDs or catalog files. Avoid `if moduleId` UI branches; use metadata/localized presentation data.

### Настройки

Generate controls from `FeatureModuleParameterDefinition`:

```text
integer/number → NumericUpDown
boolean → CheckBox
enum → ComboBox
```

Show title, description, unit, allowed range and validation error.
Programmatic binding remains silent; user edit marks dirty once; no edit auto-materializes.

### Сборка и проверка

One primary action:

```text
Собрать и проверить игру
```

It performs in-process:

```text
save authoring document
validate modules and parameters
incremental module certification
materialize GamePackage
GamePackage validation
canonical Runtime qualification
checkpoint reload
full replay
action binding
activate package in opened project
save package.json
```

Run heavy work off UI thread, keep message pump responsive, reject concurrent builds.

Show human summary, not proof jargon:

```text
Игра успешно собрана и проверена.
Механик включено: N
Параметров настроено: N
Сохранение/загрузка: пройдено
Повтор действий: пройден
Пакет проекта обновлён
```

Failure shows actionable diagnostics without replacing current package.

### Технические детали

Collapsed/not selected by default. May show IDs, revision, fingerprints, package SHA, final hash, certification executed/reused, coverage rows and artifact paths.

## Project-local authoring document

Create bounded application facade under:

```text
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/
```

Suggested components:

```text
UnifiedGameProjectWorkspaceController
GameProjectFeatureModuleAuthoringService
GameProjectBuildAndQualificationService
GameProjectBuildTransaction
GameProjectWorkspaceModels
GameProjectWorkspaceStatusPresenter
```

Project-local files:

```text
<game-project>/.llmgc/authoring/
<game-project>/.llmgc/certification-cache/
<game-project>/.llmgc/build-staging/
<game-project>/.llmgc/build-history/
```

Reuse existing `FeatureModuleCompositionPersistenceService`, `FeatureModuleParameterizedCompositionService`, `FeatureModuleCertificationService`, and file-based module library.
Do not create a second persistence format or mutation engine.
The active source remains `FeatureModuleCompositionDocument`.

If a project has no authoring document, create a default document from the current library but do not replace package.json until the primary build action.

## Successful package activation

Use the currently opened project folder.
On GREEN qualification:

1. deserialize qualified package using existing conventions;
2. validate again for real project folder;
3. atomically replace project package;
4. update `ICurrentGamePackageService`;
5. save package.json;
6. persist last successful package/final hashes;
7. refresh project summary.

Use existing `ICurrentGamePackageService.ReplaceCurrent` and `SaveAsync` or equivalent repository path.

## Transaction and rollback

Snapshot existing package.json bytes/hash, current in-memory package, authoring revision and last-successful metadata.

On failure after materialization begins:

```text
package.json byte-identical
current in-memory package unchanged
last successful hashes unchanged
user edits retained
temporary staging removed
diagnostics explain failure
```

## Legacy Goal diagnostics isolation

`VisualWorldStreamPreviewWorkspacePageControl` becomes a developer diagnostics surface.

Required:

```text
Title = Диагностика генератора
SortOrder near end of navigation
```

Default view clearly says advanced/developer page. Legacy Goal tabs are not visible by default. Add explicit control `Показать внутренние проверки`; only after it may existing Goal-number tabs become visible.

Do not delete legacy panels in this goal. Preserve them for regression/debugging.
Do not create any new Goal-number tab.

Required behavior:

```text
startup → no Goal-number tab visible
Игры → no Goal-number control visible
Диагностика генератора → legacy tabs hidden
explicit advanced toggle → legacy tabs available
```

## Normal-versus-diagnostics boundary

Normal `Игры` consumes project folder, module library, authoring document, validation, certification, Runtime qualification and current package. It must not read or present proof artifact groups as product state.

Diagnostics may continue reading historical evidence.

## UI responsiveness

Heavy actions capture UI on UI thread, execute Application services on worker thread, disable project controls, keep message pump responsive, reject concurrent duplicate build, restore controls on success/failure and start no compiler/test/PowerShell/Unity process.

## Required behavioral tests

### Real Projects page STA workflow

Instantiate real `ProjectsPageControl` with production services over temporary games root/project.

Prove:

```text
project list loads
project opens
authoring document created without replacing package
required mechanics visible and locked
optional mechanics visible from catalog
parameter editors generated from metadata
normal control tree contains no regex \bGoal\d+\b
technical details not selected by default
```

### Save/reopen

Change modules/parameters, save authoring, recreate controller/page, open same project, restore same values without manual JSON.

### Build success

Use accepted Goal147 custom values and prove:

```text
primary action off UI thread
message pump responsive
controls disabled
qualification GREEN
activated package SHA = 2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991
final-state hash = 80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e
project package.json updated
current in-memory package matches saved package
authoring document stores successful hashes/status
```

### Build failure rollback

Inject bounded invalid parameter/binding/package-save failure and prove package.json byte-identical, current package semantic hash unchanged, last successful hashes unchanged, user edits retained, staging removed, controls restored, diagnostics visible.

### Diagnostics isolation

Instantiate real diagnostics page and prove Title, hidden legacy container by default, no visible Goal-number text before toggle, toggle reveals preserved legacy tabs.

### No new Goal UI

Scan changed user-facing strings. Projects page contains no Goal-number labels; no new TabPage text contains Goal<number>; primary button exactly `Собрать и проверить игру`.

## Required artifacts

Write under both:

```text
.llmgc/procedural/goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation/
.llmgc/exports/goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation/
```

At minimum:

```text
goals146-147-human-acceptance-record.json
unified-game-project-workspace-dashboard.json
project-local-authoring-roundtrip-proof.json
project-build-activation-proof.json
project-build-rollback-proof.json
user-facing-control-inventory.json
legacy-diagnostics-isolation-proof.json
goal148-regression-compatibility-proof.json
goal148-negative-proof.json
goal148-file-index.json
goal148-report.md
```

## Dashboard markers

```text
status=GREEN
goal146Accepted=true
goal147Accepted=true
unifiedGameProjectWorkspace=true
projectsPageIsPrimaryWorkflow=true
newTopLevelPageAdded=false
normalWorkspaceGoalNumberControlCount=0
legacyDiagnosticsHiddenByDefault=true
legacyDiagnosticsAvailableByExplicitToggle=true
projectLocalAuthoringPersistence=true
projectAuthoringRoundtripPassed=true
friendlyMechanicPresentation=true
dynamicParameterEditor=true
primaryActionText=Собрать и проверить игру
heavyWorkRunsOffUiThread=true
uiPumpResponsive=true
packageActivationPassed=true
packageActivationTransactional=true
failureRollbackPassed=true
currentPackageMatchesSavedPackage=true
customPackageHashPreserved=true
customFinalHashPreserved=true
goal146RegressionGreen=true
goal147RegressionGreen=true
goal147ARegressionGreen=true
runtimeAuthority=true
unityGameplayTruth=false
goal148Accepted=false
accepted=false
```

## Negative proof

Executable where practical:

```text
buildWithoutOpenProjectRejected
unknownModuleRejected
invalidParameterRejectedBeforePackageActivation
staleCompositionRejectedOrExplained
concurrentBuildRejected
packageSaveFailureRollsBack
failedBuildDoesNotReplaceCurrentPackage
failedBuildDoesNotOverwriteLastSuccessfulHashes
projectPathEscapeRejected
projectAuthoringPathConfined
normalWorkspaceDoesNotReadProofArtifactGroups
normalWorkspaceContainsNoGoalNumberLabels
legacyDiagnosticsNotDefaultVisible
legacyDiagnosticsNotDeleted
noChildToolProcessStarted
noUnityExecutionStarted
```

## Backward compatibility

Preserve Goals146/147 acceptance values, Goal147A UI/dependency proofs, Goal145 matrix, Goal146 default hashes, Goal147 custom hashes, existing project create/open/save, module library and Runtime/save/replay behavior.

Do not rewrite historical Goal142–147A artifact roots.

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

docs/agent-tasks/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix/GOAL.md
docs/agent-tasks/goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix/GOAL.md
docs/agent-tasks/goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification/GOAL.md
docs/agent-tasks/goal-147a-authoring-ui-event-lifecycle-and-dependent-module-certification-hotfix/GOAL.md

src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.Application/Projects/CurrentGamePackageService.cs
src/LLMGameCreator.Application/Projects/GameProjectService.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/**
src/LLMGameCreator.Application/Design/FeatureModuleCertification/**
src/LLMGameCreator.Application/Design/FeatureModuleComposition/**
```

## Allowed paths

Only:

```text
docs/agent-tasks/goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-unified-game-project-workspace.ps1
.devflow/scripts/run-unified-game-project-workspace.cmd
.llmgc/procedural/goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation/**
.llmgc/exports/goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation/**
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix.md
docs/manual-acceptance/persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification.md
docs/manual-acceptance/unified-game-project-workspace-and-legacy-goal-diagnostics-isolation.md
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/**
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleAuthoringWorkbenchController.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleCompositionPersistenceService.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Designer.cs
src/LLMGameCreator.WinForms/CompositionRoot.cs
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/**
tests/LLMGameCreator.Tests/WinForms/UnifiedGameProjectWorkspaceTests.cs
tests/LLMGameCreator.Tests/WinForms/LegacyGoalDiagnosticsIsolationTests.cs
tests/LLMGameCreator.Tests/Devflow/RunUnifiedGameProjectWorkspaceScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/ProjectsPageProductSmokeTests.cs
```

Use equivalent existing test paths if needed. No project-file modification.

## Forbidden paths

Do not modify/stage:

```text
.llmgc/manual/**
.llmgc/workspace/**
catalogs/feature-modules/**
samples/minimal-map-game/**
.llmgc/procedural/goal-142*/**
.llmgc/exports/goal-142*/**
.llmgc/procedural/goal-143*/**
.llmgc/exports/goal-143*/**
.llmgc/procedural/goal-144*/**
.llmgc/exports/goal-144*/**
.llmgc/procedural/goal-145*/**
.llmgc/exports/goal-145*/**
.llmgc/procedural/goal-146*/**
.llmgc/exports/goal-146*/**
.llmgc/procedural/goal-147*/**
.llmgc/exports/goal-147*/**
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
unity/**
*.sln
*.csproj
Directory.Build.*
dependency/package files
```

No public schema, module catalog, Runtime gameplay or Unity changes. No dependency.

## Validation

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
```

Required: 0 warnings/errors.

Focused tests:

```powershell
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~UnifiedGameProjectWorkspace|FullyQualifiedName~LegacyGoalDiagnosticsIsolation|FullyQualifiedName~ProjectsPage|FullyQualifiedName~FeatureModuleAuthoring|FullyQualifiedName~FeatureModuleCertification"
```

Normal command:

```powershell
.\.devflow\scripts\run-unified-game-project-workspace.ps1 -DryRun
.\.devflow\scripts\run-unified-game-project-workspace.ps1 -ApplyCleanup
```

Regressions:

```powershell
.\.devflow\scripts\run-goal147a-authoring-and-certification-hotfix.ps1 -DryRun
.\.devflow\scripts\run-featuremodule-authoring-persistence-and-certification.ps1 -DryRun
.\.devflow\scripts\run-featuremodule-composer-scalability-hotfix.ps1 -DryRun
.\.devflow\scripts\run-featuremodule-composition-runtime-matrix.ps1 -DryRun
```

Guards:

```powershell
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git diff --name-only
git diff --cached --name-only
git ls-files .llmgc/manual .llmgc/workspace
```

Check changed text for mojibake/escaped Cyrillic: zero. Forbidden diff empty. Restore validation churn only by exact policy-derived paths. No reset --hard, clean, broad restore, branch switch, merge, rebase or cherry-pick.

## Current-state update

```text
goal146Accepted=true
goal146AcceptedByHuman=true
goal146AcceptedByCodex=false
goal147Accepted=true
goal147AcceptedByHuman=true
goal147AcceptedByCodex=false
goal148Accepted=false
unifiedGameProjectWorkspace=true
projectsPageIsPrimaryWorkflow=true
normalWorkspaceGoalNumberControlCount=0
legacyGoalDiagnosticsHiddenByDefault=true
projectLocalAuthoringPersistence=true
projectBuildActivationPassed=true
projectBuildRollbackPassed=true
nextProductGoal=review_goal_148_unified_game_project_workspace
```

## Publication

Before staging: acceptance recorded, normal workflow has zero Goal-number controls, project-local authoring works, package activation/rollback pass, diagnostics hidden/preserved, regressions GREEN, Goal148 accepted=false, scope clean, forbidden diff empty.

Commit:

```text
GREEN Goal 148 unified game project workspace and legacy Goal diagnostics isolation
```

Push `origin main`.

Final report: status/SHA, acceptance status, normal sections, Goal-number visible count, diagnostics toggle, project-local roundtrip, activation hash, rollback, responsiveness, tests/scope/forbidden diff, clean `HEAD == origin/main`.

Do not report GREEN if normal workflow still requires a Goal-number tab, failed build can replace package/current package, or legacy diagnostics are visible by default.
