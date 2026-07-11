# Goal 148B — Current-Package UI-Thread Dispatch + Real Workspace Build Retry Hotfix

## Identity

- Task ID: `goal-148b-current-package-ui-thread-dispatch-and-real-workspace-build-retry-hotfix`
- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: commit `41688352841bc23a6b75709f69ad5772709c4bec` or a direct descendant on `main`

## New-dialog rule

This task is executed in a fresh Codex dialog. Treat this file as the complete instruction source.

## Goal type

Focused P1 manual-workflow hotfix.

Do not start Goal149.
Do not mark Goal148 accepted.
Do not add Goal-number UI.
Do not disable WinForms cross-thread checking.

## Manual failure to record

The real Goal148 manual workflow failed after clicking:

```text
Собрать и проверить игру
```

Visible error:

```text
Недопустимая операция в нескольких потоках:
попытка доступа к элементу управления '_navigation'
не из того потока, в котором он был создан.
```

Project title used by the human:

```text
Проверка конструктора
```

Record this as a real manual failure/retry-required entry in:

```text
docs/manual-acceptance/unified-game-project-workspace-and-legacy-goal-diagnostics-isolation.md
```

Required state:

```text
goal148Accepted=false
manualRetryRequired=true
manualFailureClass=current_package_changed_cross_thread_ui_dispatch
rawScreenshotNotCommitted=true
```

Do not commit the screenshot or raw manual files.

## Root cause

Goal148 correctly runs the heavy project build using `Task.Run`.

Inside background execution:

```text
ProjectsPageControl.BuildAndQualifyAsync
→ Task.Run(_workspaceController.BuildAndQualify)
→ GameProjectBuildAndQualificationService
→ GameProjectBuildTransaction.ReplaceCurrentPackage
→ CurrentGamePackageService.ReplaceCurrent
→ synchronous CurrentChanged event on worker thread
```

Unsafe WinForms subscribers currently include:

### MainForm

```csharp
_currentGamePackageService.CurrentChanged += (_, _) => UpdateStatus();
```

`UpdateStatus()` changes a ToolStrip item. WinForms layout then reaches the form/navigation controls from the worker thread and raises the `_navigation` cross-thread exception.

### CompositionWorkbenchPageControl

Uses an anonymous async `CurrentChanged` handler and immediately calls UI-binding methods without marshaling.

### UnityArchiveReviewPageControl

Uses an anonymous async `CurrentChanged` handler and immediately calls UI-binding methods without marshaling.

Dashboard and Generation already use `InvokeRequired`/`BeginInvoke`; audit and preserve them.

This exact error class existed previously in the repository's S034 hotfix documentation. Do not reintroduce the old anti-pattern.

## Required architecture

`ICurrentGamePackageService` remains UI-framework-agnostic. Its event has no guaranteed delivery thread.

Every WinForms subscriber to `CurrentChanged` must explicitly marshal UI work to its owning UI thread.

Implement a small bounded WinForms helper or an equivalent consistent pattern, for example:

```text
WinFormsUiThreadDispatcher
  Post(Control owner, Action operation)
  PostAsync(Control owner, Func<Task> operation, Action<Exception> onError)
```

Required behavior:

1. Same-thread calls execute directly.
2. Worker-thread calls use `BeginInvoke`/captured WinForms synchronization context.
3. No operation executes after owner disposal.
4. Handle-destruction races do not escape as unhandled exceptions.
5. Async operations are observed; exceptions are routed to the page's diagnostics/status surface.
6. No global suppression:
   - no `Control.CheckForIllegalCrossThreadCalls = false`;
   - no swallowed generic `InvalidOperationException`;
   - no `Application.DoEvents` in production code.
7. Event handlers are named so they can be unsubscribed during disposal.

## Required subscribers audit

Search the entire WinForms project for `CurrentChanged`.

Create an inventory with every subscriber and classify it:

```text
subscriber
owner control
handler name
marshals to UI thread
async exceptions observed
unsubscribes on dispose
```

At minimum cover:

```text
MainForm
DashboardPageControl
GenerationPageControl
CompositionWorkbenchPageControl
UnityArchiveReviewPageControl
```

After the hotfix:

```text
unsafeSubscriberCount=0
anonymousCurrentChangedUiHandlerCount=0
```

If additional subscribers exist, fix and include them.

## MainForm requirements

Replace the anonymous subscription with a named handler.

A worker-thread `CurrentChanged` must:

```text
not throw
not directly touch _navigation
not directly touch _workspace
schedule UpdateStatus on MainForm UI thread
update status to the new project title
```

Unsubscribe when the form is disposed/closed.

Repeated events and form-close races must not throw.

## Async page requirements

For `CompositionWorkbenchPageControl` and `UnityArchiveReviewPageControl`:

- worker-thread event only schedules work;
- actual `ApplyViewState`, control enabling, list/data binding and status text happen on UI thread;
- no anonymous async event handler;
- exceptions from async refresh are observed and displayed;
- unsubscribe on disposal;
- repeated current-package events are bounded and do not run concurrent destructive refreshes;
- no child process.

Dashboard and Generation must remain safe.

## Build/transaction behavior

Do not move heavy certification/materialization/Runtime qualification back onto the UI thread.

Preserve:

```text
heavyWorkRunsOffUiThread=true
uiPumpResponsive=true
support-file activation
staged validation
real-project validation
transactional package/support rollback
```

The current-package event dispatch fix must not alter package/final hashes.

## Required behavioral tests

### MainForm worker event

On a real STA thread:

1. create a real `MainForm`;
2. use a minimal real/fake `IEditorPageRegistry`;
3. create the form handle;
4. trigger `CurrentGamePackageService.ReplaceCurrent` from `Task.Run`;
5. pump WinForms messages;
6. prove:
   - worker call completes without exception;
   - status text updates to the project title;
   - `_navigation` remains accessible;
   - selected page remains unchanged;
   - event callback executed on the UI thread.

### MainForm disposal race

Trigger worker `ReplaceCurrent` while closing/disposing the form.

Required:

```text
no unhandled InvalidOperationException
no ObjectDisposedException
no queued UI operation after disposal
```

### Real Goal148 workspace build under MainForm

Reproduce the user's route with production services:

```text
real GameProjectService.CreateAsync
real ProjectsPageControl
real MainForm
real UnifiedGameProjectWorkspaceController
real Goal148/148A build
```

Requirements:

- do not manually copy scripts;
- open a production-created new project;
- apply the accepted custom parameter values;
- invoke the real primary build button/method;
- keep message pump responsive;
- package activation raises `CurrentChanged` from worker;
- no cross-thread exception appears;
- build result is GREEN;
- status strip shows `Открыт проект: Проверка конструктора` or the test project title;
- page remains on `Игры`;
- package SHA:
  `2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991`;
- final hash:
  `80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e`;
- required support file prepared;
- controls restored.

This test must exercise the `CurrentChanged` event path. Calling the controller directly without a real `MainForm` is insufficient.

### Subscriber audit test

Executable/source-backed test:

```text
all CurrentChanged WinForms subscriptions use named handlers
all named handlers marshal through the approved helper/pattern
no unsafe anonymous CurrentChanged handler remains
```

Source scans supplement real STA tests; they do not replace them.

### Async page dispatch tests

Use real controls or a testable production dispatcher seam to prove worker-thread package changes do not directly execute UI binding for:

```text
CompositionWorkbenchPageControl
UnityArchiveReviewPageControl
```

At minimum prove callback thread IDs and observed exception handling.

## Regression requirements

Preserve:

```text
Goal148A real New Game build GREEN
required support copied/reused correctly
Goal148 normal workspace Goal-number count=0
legacy diagnostics hidden by default
Goals146/147 accepted=true
Goal148 accepted=false
Goal141 accepted=false
```

Do not rewrite historical Goal148 or Goal148A artifact roots.

## Required Goal148B artifacts

Write under both:

```text
.llmgc/procedural/goal-148b-current-package-ui-thread-dispatch-and-real-workspace-build-retry-hotfix/
.llmgc/exports/goal-148b-current-package-ui-thread-dispatch-and-real-workspace-build-retry-hotfix/
```

At minimum:

```text
goal148-manual-cross-thread-failure-record.json
current-package-subscriber-inventory.json
mainform-worker-currentchanged-proof.json
mainform-disposal-race-proof.json
async-page-currentchanged-dispatch-proof.json
real-workspace-build-retry-proof.json
goal148b-regression-compatibility-proof.json
goal148b-negative-proof.json
goal148b-dashboard.json
goal148b-file-index.json
goal148b-report.md
```

File index includes SHA-256.

Dashboard markers:

```text
status=GREEN
manualFailureRecorded=true
manualRetryRequired=true
mainFormWorkerCurrentChangedPassed=true
mainFormStatusUpdatedOnUiThread=true
mainFormNavigationUntouchedFromWorker=true
mainFormDisposalRacePassed=true
compositionWorkbenchDispatchPassed=true
unityArchiveReviewDispatchPassed=true
unsafeCurrentChangedSubscriberCount=0
anonymousCurrentChangedUiHandlerCount=0
asyncExceptionsObserved=true
realWorkspaceBuildRetryAutomatedPassed=true
crossThreadExceptionAbsent=true
packageSha256=2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991
finalStateHash=80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e
supportFilesPrepared=true
heavyWorkRunsOffUiThread=true
uiPumpResponsive=true
goal148ARegressionGreen=true
normalWorkspaceGoalNumberControlCount=0
legacyDiagnosticsHiddenByDefault=true
goal148Accepted=false
accepted=false
```

## Negative proof

Prove:

```text
crossThreadChecksNotDisabled
genericInvalidOperationNotSwallowed
currentPackageServiceHasNoWinFormsDependency
workerThreadDoesNotCallUpdateStatusDirectly
workerThreadDoesNotCallApplyViewStateDirectly
disposedControlDoesNotReceiveQueuedCallback
asyncEventExceptionObserved
duplicateBuildStillRejected
failedBuildRollbackStillPassed
noChildToolProcessStarted
historicalArtifactsRewritten=false
```

## Current-state update

After GREEN:

```text
current_phase_title=Goal 148B current-package UI-thread dispatch and real workspace build retry hotfix
goal148Accepted=false
goal148ManualRetryRequired=true
goal148CrossThreadFailureRecorded=true
goal148CurrentChangedUiDispatchFixed=true
goal148UnsafeCurrentChangedSubscriberCount=0
goal148RealWorkspaceBuildRetryAutomatedPassed=true
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
docs/NEXT_PRODUCT_SLICE_034_HOTFIX_GENERATE_PREVIEW_UI_THREAD_TASK.md

src/LLMGameCreator.WinForms/MainForm.cs
src/LLMGameCreator.WinForms/MainForm.Designer.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Dashboard/DashboardPageControl.cs
src/LLMGameCreator.WinForms/Pages/Generation/GenerationPageControl.cs
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchPageControl.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.cs

src/LLMGameCreator.Application/Projects/CurrentGamePackageService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/**
```

## Allowed paths

Only:

```text
docs/agent-tasks/goal-148b-current-package-ui-thread-dispatch-and-real-workspace-build-retry-hotfix/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal148b-current-package-ui-thread-hotfix.ps1
.devflow/scripts/run-goal148b-current-package-ui-thread-hotfix.cmd

.llmgc/procedural/goal-148b-current-package-ui-thread-dispatch-and-real-workspace-build-retry-hotfix/**
.llmgc/exports/goal-148b-current-package-ui-thread-dispatch-and-real-workspace-build-retry-hotfix/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/unified-game-project-workspace-and-legacy-goal-diagnostics-isolation.md

src/LLMGameCreator.WinForms/MainForm.cs
src/LLMGameCreator.WinForms/MainForm.Designer.cs
src/LLMGameCreator.WinForms/WinFormsUiThreadDispatcher.cs
src/LLMGameCreator.WinForms/Pages/Dashboard/DashboardPageControl.cs
src/LLMGameCreator.WinForms/Pages/Generation/GenerationPageControl.cs
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchPageControl.cs
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs

tests/LLMGameCreator.Tests/WinForms/CurrentGamePackageUiThreadDispatchTests.cs
tests/LLMGameCreator.Tests/WinForms/UnifiedGameProjectWorkspaceTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal148BCurrentPackageUiThreadHotfixScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/ProjectsPageProductSmokeTests.cs
```

If the helper belongs in an existing WinForms infrastructure directory, use that equivalent path without changing project files.

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
.llmgc/procedural/goal-148a-new-project-required-support-files-and-transactional-activation-hotfix/**
.llmgc/exports/goal-148a-new-project-required-support-files-and-transactional-activation-hotfix/**

src/LLMGameCreator.Application/**
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

Do not modify `CurrentGamePackageService`; keep it UI-agnostic.

No public schema, Runtime, module catalog, support-file algorithm, Unity or dependency changes.

## Validation

Run sequentially:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
```

Required: 0 warnings, 0 errors.

Focused tests:

```powershell
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~Goal148B|FullyQualifiedName~CurrentGamePackageUiThreadDispatch|FullyQualifiedName~UnifiedGameProjectWorkspace|FullyQualifiedName~ProjectsPage"
```

New command:

```powershell
.\.devflow\scripts\run-goal148b-current-package-ui-thread-hotfix.ps1 -DryRun
.\.devflow\scripts\run-goal148b-current-package-ui-thread-hotfix.ps1 -ApplyCleanup
```

Regressions:

```powershell
.\.devflow\scripts\run-goal148a-new-project-support-files-hotfix.ps1 -DryRun
.\.devflow\scripts\run-unified-game-project-workspace.ps1 -DryRun
.\.devflow\scripts\run-goal147a-authoring-and-certification-hotfix.ps1 -DryRun
```

Guards:

```powershell
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-148b-current-package-ui-thread-dispatch-and-real-workspace-build-retry-hotfix
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun

git diff --check
git diff --cached --check
git status --short --untracked-files=all
git diff --name-only
git diff --cached --name-only
git ls-files .llmgc/manual .llmgc/workspace
```

Check changed text for mojibake and escaped Cyrillic: zero matches.
Forbidden diff: empty.

Restore validation churn only through exact policy-derived paths.

Do not use reset-hard, clean, broad restore, branch switching, merge, rebase or cherry-pick.

## Publication

Before staging:

```text
manual failure recorded
all CurrentChanged WinForms subscribers audited
unsafe subscriber count=0
real MainForm worker event GREEN
real workspace build under MainForm GREEN
no cross-thread exception
Goal148A regression GREEN
Goal148 accepted=false
scope clean
forbidden diff empty
```

Commit:

```text
GREEN Goal 148B current-package UI-thread dispatch and real workspace build retry hotfix
```

Push `origin main`.

Final report must include commit SHA, exact root cause, subscriber inventory, UI dispatcher strategy, MainForm worker/disposal proof, CompositionWorkbench/UnityArchive proof, real workspace retry package/final hashes, support-file status, Goal148A regression, tests, scope, forbidden diff and clean `HEAD == origin/main`.

Do not report GREEN if any WinForms `CurrentChanged` subscriber can still touch controls directly from a worker thread or if the real MainForm + Projects build path still produces the `_navigation` exception.
