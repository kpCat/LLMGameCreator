# Product Slice 034 Hotfix - Generate Preview UI Thread Boundary

## Purpose

Fix the S034 manual verification failure in WinForms:

```text
Generate Preview failed:
InvalidOperationException:
Недопустимая операция в нескольких потоках:
попытка доступа к элементу управления '_navigation'
не из того потока, в котором он был создан.
```

This is a focused hotfix, not a new feature slice.

## Root cause to verify

The likely cause is:

- `RuntimePreviewPageControl.GeneratePreviewAsync()` calls `OneClickGeneratedPreviewWorkflowService.ExecuteAsync()`;
- `OneClickGeneratedPreviewWorkflowService.ExecuteAsync()` uses awaits with `ConfigureAwait(false)`;
- later it calls `_currentGamePackageService.ReplaceCurrent(...)`;
- `CurrentGamePackageService.CurrentChanged` is observed by WinForms `MainForm`;
- `MainForm.UpdateStatus()` touches UI controls such as `_navigation` or status controls from the wrong thread.

Fix the thread boundary. Do not suppress the exception globally.

## Required fix direction

The workflow service may generate/write artifacts in the background.

The WinForms UI must own UI-affecting state changes.

Required behavior:

- `OneClickGeneratedPreviewWorkflowService` must not replace the current package from a background continuation during the WinForms one-click flow.
- In `RuntimePreviewPageControl.GeneratePreviewAsync`, call the workflow with `ReplaceCurrentPackage = false`.
- Capture needed UI state such as current folder/project root before background work.
- After the awaited workflow returns successfully, on the UI thread call:

```csharp
_currentGamePackageService?.ReplaceCurrent(result.GeneratedPackage);
```

- Then update preview controls/log/tabs on the UI thread.
- The success log should still say the generated package was loaded as current package.

If the service still supports service-side `ReplaceCurrentPackage = true` for headless tests, ensure it is not used by the WinForms page in a way that triggers UI events from a worker thread.

## Optional robustness

If needed, add a small helper in `RuntimePreviewPageControl`:

```csharp
private void RunOnUiThread(Action action)
```

or use existing WinForms `InvokeRequired`/`BeginInvoke` patterns.

Do not make broad threading framework changes.

Do not add global `CheckForIllegalCrossThreadCalls = false`.

Do not swallow `InvalidOperationException`.

## UI behavior after fix

Manual flow should be:

1. Open WinForms.
2. Open `Runtime Preview`.
3. Click `Generate Preview`.
4. Wait for ready log.
5. Click `Старт`.
6. Generated map appears and movement/interaction works.

Expected log:

```text
Generate Preview: running deterministic S029-S033 workflow...
Generate Preview ready: ...
Generated package loaded as current package. Press Start to run Runtime Preview.
```

No cross-thread exception should appear.

## Tests

Add focused tests if practical:

- service `ReplaceCurrentPackage = false` does not call/replace current package;
- service still returns generated package and paths;
- existing `OneClickGeneratedPreviewWorkflow` tests remain passing.

If there is an established WinForms test pattern for `RuntimePreviewPageControl`, add a small test seam to ensure the page requests `ReplaceCurrentPackage = false` and performs current-package replacement itself. Do not add brittle pixel/UI tests.

## Product smoke

Keep the existing scenario:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario one-click-generated-preview-workflow
```

It should continue to pass.

This smoke is headless and cannot fully prove WinForms UI-thread behavior; the final proof is the user's manual WinForms click.

## Docs/state updates

Update only if useful:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/MANUAL_VISIBLE_GENERATED_PLAYABLE_PREVIEW_CHECK.md`

State should indicate:

- S034 hotfix was needed because manual one-click preview caught a UI-thread bug;
- next action remains `manual_one_click_preview_verification`, not a new feature slice.

## Verification commands

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~OneClickGeneratedPreviewWorkflow"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario one-click-generated-preview-workflow
.\.devflow\scripts\check-all.ps1
```

If docs/state are changed, also run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Codex should not launch Visual Studio or perform manual UI verification.

## Constraints

Do not add:

- LLM/provider execution;
- Lua execution;
- Unity execution;
- media generation;
- broad runtime changes;
- broad GamePackage schema changes;
- broad UI redesign.

Do not convert this hotfix into S035.

## Completion report

Report:

- exact cause fixed;
- files changed;
- how WinForms now avoids background `ReplaceCurrent`;
- verification commands and results;
- whether `check-all.ps1` passed;
- confirmation that manual user verification should be retried by clicking `Generate Preview` then `Старт`.

