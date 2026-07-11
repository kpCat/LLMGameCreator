# Goal 148A — New-Project Required Support Files + Transactional Activation Hotfix

## Identity

- Task ID: `goal-148a-new-project-required-support-files-and-transactional-activation-hotfix`
- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: commit `5f2dd3b54e4c799f854525d3e2a54dad9c569b4a` or a direct descendant on `main`

## New-dialog rule

This task is executed in a fresh Codex dialog. Treat this file as the complete instruction source.

## Goal type

Focused P1 product-workflow hotfix before Goal148 human acceptance.

Do not start Goal149.
Do not mark Goal148 accepted.
Do not add any Goal-number UI.

## Current accepted state

```text
Goal146 accepted=true by human
Goal147 accepted=true by human
Goal148 implementation=GREEN, accepted=false
Goal141 accepted=false
```

Preserve the exact Goals146/147 acceptance record committed by Goal148.

## P1 defect

Goal148 claims the normal user path:

```text
Игры
→ Новая игра
→ configure mechanics/parameters
→ Собрать и проверить игру
```

However the production `GameProjectService.CreateAsync` creates only empty:

```text
assets/
scripts/
saves/
package.json
```

The composed Goal148 package inherits the Goal142 balanced baseline script catalog, including relative script paths such as:

```text
scripts/generators/basic_village.lua
```

`ScriptCatalogValidator` treats a missing referenced script file as an Error.

The Goal148 automated build-success test hides this production defect by manually copying:

```text
samples/minimal-map-game/scripts/**
```

into its test project before building.

The actual `Новая игра` route does not perform that copy. A newly created user project can therefore open normally but fail its first primary build because required project support files are absent.

Fix this generically. Do not add a one-off copy for `basic_village.lua`.

## Product result

A project created through the real production `GameProjectService.CreateAsync` must build successfully without any test/user manual file copying.

Required path:

```text
new empty project
→ qualified package identifies required relative support files
→ deterministic support-file plan
→ validate source and path confinement
→ stage package + required files
→ validate staged project
→ transactionally activate support files + package
→ validate real project
→ update current in-memory package
```

For the current narrow alpha, required support files include every non-empty relative `ScriptCatalog.Scripts[].Path`.

Design the seam so additional support-file kinds can be added later without rewriting transaction logic.

## Source ownership

Introduce a bounded source abstraction, for example:

```text
IGameProjectSupportFileSource
NarrowAlphaTemplateSupportFileSource
GameProjectSupportFilePlan
GameProjectSupportFilePlanEntry
GameProjectSupportFileMaterializer
```

Default source root may resolve to:

```text
samples/minimal-map-game/
```

for the current narrow alpha, but:

- the algorithm must not branch on a specific script ID/path;
- source root must be injectable/configurable;
- every package-declared required relative script path is processed generically;
- the sample source is read-only;
- record this current narrow-alpha source limitation as technical debt.

Copy only package-declared required support paths.

## Support-file plan

Each required file records:

```text
scriptId
relativePath
sourcePath
sourceSha256
targetPath
targetState = missing | matching_existing | conflicting_existing
activationAction = copy | reuse | reject
```

Validate:

```text
relative path non-empty
not rooted
no traversal
source path confined under source root
target path confined under project root
source file exists
script IDs unique
relative target paths unique
source SHA computed
```

Rules:

1. Missing target → copy transactionally.
2. Existing target with identical SHA → reuse.
3. Existing target with different SHA → reject with actionable conflict diagnostics.
4. Never overwrite a differing user file.
5. Missing source → reject before package activation.
6. Duplicate target path from different scripts → reject unless exactly equivalent.
7. Invalid plan must not mutate package or current in-memory package.

## Staged validation

Before activating the real project:

1. create a confined validation project under project-local build staging;
2. place qualified `package.json` there;
3. place planned support files at package-relative paths;
4. include matching existing files for reuse entries;
5. run existing `IGamePackageValidator` against the staged project folder;
6. fail before real activation when staged validation is not GREEN.

After activation, validate once more against the real project folder.

## Transaction extension

Extend `GameProjectBuildTransaction` or add a bounded companion transaction.

Snapshot every support-file target:

```text
target path
existed before
original bytes/hash
```

Activation order:

```text
validate plan
staged validation
copy/reuse support files
replace package.json
replace current in-memory package
save qualified authoring metadata
write build history
commit
```

Before commit, any failure must restore:

```text
package.json byte-identically
current in-memory package
authoring document per Goal148 policy
new support files deleted
matching existing support files unchanged
conflicting files unchanged
last successful hashes unchanged
build staging removed
```

Use atomic temporary-file replacement where applicable.

## Build result and summary

Extend `GameProjectBuildResult` with:

```text
RequiredSupportFileCount
CopiedSupportFileCount
ReusedSupportFileCount
SupportFilesPrepared
SupportFileDiagnostics[]
```

Successful human summary includes:

```text
Файлы проекта подготовлены: <count>
```

Normal overview must not show absolute source paths.

## Required executable production-path tests

### Real new-project build without manual copying

Use real:

```text
GameProjectService.CreateAsync
NewGamePackageFactory
CurrentGamePackageService
UnifiedGameProjectWorkspaceController
```

Do not call any test `CopyDirectory`.

Prove:

```text
new scripts directory starts empty
open unified workspace
apply accepted custom values
build GREEN
package SHA =
2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991
final hash =
80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e
required script now exists under project
target SHA matches source
real-project validation GREEN
current in-memory package matches saved package
```

Remove the manual script-copy step from the old Goal148 build-success test.

### Repeat build

```text
copied=0
reused>=1
support bytes unchanged
package/final hashes deterministic
```

### Conflicting user file

Create a differing target script before build:

```text
build FAILED
diagnostic names relative path
user file bytes unchanged
package.json unchanged
current package unchanged
last successful hashes unchanged
```

### Missing source

Use an injected source root without the required script:

```text
build FAILED before activation
package unchanged
target absent
staging removed
```

### Failure after support copy

Inject failure after copying a missing support file but before package commit:

```text
new support file removed by rollback
package unchanged
current package unchanged
authoring successful hashes unchanged
```

### Path safety

Executable tests:

```text
rooted support path rejected
path traversal rejected
source escape rejected
target escape rejected
duplicate target conflict rejected
```

## Preserve Goal148 behavior

Keep GREEN:

```text
Goals146/147 accepted
normal sections: Обзор, Механики, Настройки, Сборка и проверка, Технические детали
normal Goal-number controls=0
legacy diagnostics hidden by default
project-local authoring roundtrip
heavy work off UI thread
transactional rollback
custom package/final hashes
Goal148 accepted=false
```

Do not rewrite Goal148 artifact roots.

## Required Goal148A artifacts

Write under both:

```text
.llmgc/procedural/goal-148a-new-project-required-support-files-and-transactional-activation-hotfix/
.llmgc/exports/goal-148a-new-project-required-support-files-and-transactional-activation-hotfix/
```

At minimum:

```text
new-project-support-files-dashboard.json
new-project-production-build-proof.json
support-file-plan-proof.json
support-file-repeat-build-proof.json
support-file-conflict-proof.json
support-file-missing-source-proof.json
support-file-rollback-proof.json
goal148-regression-compatibility-proof.json
goal148a-negative-proof.json
goal148a-file-index.json
goal148a-report.md
```

Dashboard:

```text
status=GREEN
realNewProjectBuildPassed=true
manualTestScriptCopyRemoved=true
requiredSupportFileCount>=1
copiedSupportFileCount>=1
repeatBuildCopiedSupportFileCount=0
repeatBuildReusedSupportFileCount>=1
stagedProjectValidationPassed=true
realProjectValidationPassed=true
supportFileSourceHashMatched=true
conflictingExistingFileRejected=true
conflictingExistingFilePreserved=true
missingSourceRejectedBeforeActivation=true
newSupportFileRemovedOnRollback=true
packageRollbackPassed=true
currentPackageRollbackPassed=true
customPackageHashPreserved=true
customFinalHashPreserved=true
goal148RegressionGreen=true
normalWorkspaceGoalNumberControlCount=0
legacyDiagnosticsHiddenByDefault=true
goal148Accepted=false
accepted=false
```

## Current-state update

After GREEN:

```text
current_phase_title=Goal 148A new-project required support files and transactional activation hotfix
goal148Accepted=false
goal148NewProjectBuildPassed=true
goal148RequiredSupportFilesMaterialized=true
goal148SupportFileConflictProtection=true
goal148SupportFileRollbackPassed=true
nextProductGoal=review_goal_148_unified_game_project_workspace
```

Do not mark Goal141 accepted.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md

docs/agent-tasks/goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation/GOAL.md

src/LLMGameCreator.Application/Projects/GameProjectService.cs
src/LLMGameCreator.Application/Projects/NewGamePackageFactory.cs
src/LLMGameCreator.Application/Validation/ScriptCatalogValidator.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildTransaction.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectFeatureModuleAuthoringService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceControllerTests.cs
tests/LLMGameCreator.Tests/WinForms/UnifiedGameProjectWorkspaceTests.cs
```

## Allowed paths

Only:

```text
docs/agent-tasks/goal-148a-new-project-required-support-files-and-transactional-activation-hotfix/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal148a-new-project-support-files-hotfix.ps1
.devflow/scripts/run-goal148a-new-project-support-files-hotfix.cmd

.llmgc/procedural/goal-148a-new-project-required-support-files-and-transactional-activation-hotfix/**
.llmgc/exports/goal-148a-new-project-required-support-files-and-transactional-activation-hotfix/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/unified-game-project-workspace-and-legacy-goal-diagnostics-isolation.md

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/**
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs

tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/**
tests/LLMGameCreator.Tests/WinForms/UnifiedGameProjectWorkspaceTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal148ANewProjectSupportFilesHotfixScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/ProjectsPageProductSmokeTests.cs
```

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
.llmgc/procedural/goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation/**
.llmgc/exports/goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation/**

src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Generation/**
src/LLMGameCreator.AssetPipeline/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.Application/Projects/**
src/LLMGameCreator.Application/Validation/**
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

Read but do not modify project creation, validator or sample files.

No public schema, Runtime, module catalog, Unity or dependency changes.

## Validation

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore

dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~Goal148A|FullyQualifiedName~UnifiedGameProjectWorkspace|FullyQualifiedName~ProjectsPage"

.\.devflow\scripts\run-goal148a-new-project-support-files-hotfix.ps1 -DryRun
.\.devflow\scripts\run-goal148a-new-project-support-files-hotfix.ps1 -ApplyCleanup

.\.devflow\scripts\run-unified-game-project-workspace.ps1 -DryRun
.\.devflow\scripts\run-goal147a-authoring-and-certification-hotfix.ps1 -DryRun
.\.devflow\scripts\run-featuremodule-authoring-persistence-and-certification.ps1 -DryRun

.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-148a-new-project-required-support-files-and-transactional-activation-hotfix
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun

git diff --check
git diff --cached --check
git status --short --untracked-files=all
git diff --name-only
git diff --cached --name-only
git ls-files .llmgc/manual .llmgc/workspace
```

Required:

```text
0 warnings
0 errors
0 mojibake matches
0 escaped Cyrillic matches
forbidden diff empty
```

Restore validation churn only through exact policy-derived paths.

Do not use reset-hard, clean, broad restore, branch switching, merge, rebase or cherry-pick.

## Publication

Before staging:

```text
real production New Game build GREEN without manual copy
support source/target hash proof GREEN
repeat reuse GREEN
conflict protection GREEN
missing-source rejection GREEN
support rollback GREEN
Goal148 regressions GREEN
Goal148 accepted=false
scope clean
forbidden diff empty
```

Commit:

```text
GREEN Goal 148A new-project required support files and transactional activation hotfix
```

Push `origin main`.

Final report must include commit SHA, real New Game proof, support counts/paths/hashes, staged and real validation, repeat reuse, conflict preservation, missing source rejection, rollback cleanup, preserved package/final hashes, Goal148 regression, tests, scope, forbidden diff and clean `HEAD == origin/main`.

Do not report GREEN if a production-created project still needs a manual/test file copy or if rollback can leave a copied support file behind.
