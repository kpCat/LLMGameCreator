# Codex Task - S034 Hotfix: Generate Preview UI Thread Boundary

## Objective

Fix the WinForms cross-thread exception observed when the user clicks `Generate Preview` in Runtime Preview after S034.

Observed error:

```text
InvalidOperationException:
Недопустимая операция в нескольких потоках:
попытка доступа к элементу управления '_navigation'
не из того потока, в котором он был создан.
```

This is a focused hotfix, not a new product slice.

## Likely root cause

`OneClickGeneratedPreviewWorkflowService.ExecuteAsync()` calls `_currentGamePackageService.ReplaceCurrent(...)` after awaits using `ConfigureAwait(false)`. That can raise `CurrentChanged` from a worker thread, and `MainForm.UpdateStatus()` then touches WinForms controls from the wrong thread.

## Required fix

In the WinForms one-click flow:

- call workflow with `ReplaceCurrentPackage = false`;
- capture project root/current folder on UI thread before starting the workflow;
- after `await` returns to UI thread and result is OK, call `_currentGamePackageService?.ReplaceCurrent(result.GeneratedPackage)` from `RuntimePreviewPageControl`;
- then update preview/log/tabs from UI thread.

The workflow service may still support service-side replacement for headless seams, but WinForms must not use that mode if it can fire UI events from a worker thread.

Do not suppress cross-thread checks.

Do not globally swallow the exception.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~OneClickGeneratedPreviewWorkflow"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario one-click-generated-preview-workflow
.\.devflow\scripts\check-all.ps1
```

If docs/state are touched, run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Codex should not launch Visual Studio. User will manually retry:

```text
Runtime Preview -> Generate Preview -> wait ready log -> Старт
```

## Completion report

Report:

- root cause;
- files changed;
- verification results;
- whether full `check-all.ps1` passed;
- explicit note that the user should retry manual one-click preview verification.

