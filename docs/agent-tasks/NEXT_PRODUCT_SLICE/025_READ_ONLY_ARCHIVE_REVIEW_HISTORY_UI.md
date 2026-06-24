# Product Slice 025: Read-only Archive Review/History UI

Task ID: PRODUCT_SLICE_025_READ_ONLY_ARCHIVE_REVIEW_HISTORY_UI

Executor: Codex.

## Goal

Add a bounded read-only WinForms editor page for inspecting existing Unity archive review/history/comparison reports produced by Product Slices 023, 024 and 024.1.

This is a UI-only slice over existing headless report files.

It must not generate, materialize, mutate, execute or export anything.

## Current working tree policy

Work in the current working tree only.

Do not run git commands.

Do not create branches.

Do not switch branches.

Do not merge.

Do not rebase.

Do not cherry-pick.

Do not push.

The user handles all branch and push operations manually.

## Windows path policy

Use repo-relative paths and normal Windows/PowerShell paths only.

Do not use `/mnt`.

Do not use `/home/oai`.

Do not use `sandbox:/...`.

Do not use `C:\mnt`.

Do not use container paths in code, docs, tests or final report.

## Read first

Read these files before editing anything:

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/PRODUCT_SMOKE_SCENARIOS.md

src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchPageControl.cs
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchPresenter.cs
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchViewState.cs

tests/LLMGameCreator.Tests/WinForms/CompositionWorkbenchPresenterTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/CompositionWorkbenchReadonlySmokeTests.cs

src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonMarkdownRenderer.cs

tests/LLMGameCreator.Tests/Application/UnityArchiveReviewHistoryTests.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveReviewComparisonTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewHistorySmokeTests.cs
```

Important existing patterns:

- `CompositionWorkbenchPageControl` is a `UserControl` implementing `IEditorPage`.
- It has a default constructor for designer/test construction.
- It has a runtime constructor with presenter/current project service.
- It is registered in `CompositionRoot`.
- It is included in `EditorPageRegistry`.
- It uses code-only Designer layout.

Follow that pattern.

## Allowed files

You may add or update only these areas:

```text
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPresenter.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewViewState.cs

src/LLMGameCreator.WinForms/CompositionRoot.cs

tests/LLMGameCreator.Tests/WinForms/UnityArchiveReviewPresenterTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewReadonlySmokeTests.cs

docs/PRODUCT_SLICE_025_READ_ONLY_ARCHIVE_REVIEW_HISTORY_UI.md
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/agent-tasks/NEXT_PRODUCT_SLICE/025_READ_ONLY_ARCHIVE_REVIEW_HISTORY_UI.md
docs/agent-tasks/NEXT_PRODUCT_SLICE/025_CODEX_PROMPT.md
.devflow/CURRENT_RUN.md
```

If a required change appears outside this list, stop and report it instead of editing.

Do not update `.sln` or `*.csproj`; the existing project should include new `.cs` files by globbing.

## Forbidden files and areas

Do not edit:

```text
src/LLMGameCreator.Runtime/
src/LLMGameCreator.Runtime.Abstractions/
src/LLMGameCreator.GamePackage/
src/LLMGameCreator.Scripting/
src/LLMGameCreator.Infrastructure/
src/LLMGameCreator.Application/Composition/UnityArchiveMaterialization*
src/LLMGameCreator.Application/Composition/UnityArchiveProvider*
src/LLMGameCreator.Application/Composition/UnityArchiveFulfillment*
src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshot*
src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistory*
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparison*
generator-library/
LLMGameCreator.sln
*.csproj
```

The S025 UI can read existing Application models but must not modify Application services/models.

Do not change GamePackage schema.

Do not change Runtime behavior.

Do not implement Unity.

Do not add provider execution.

Do not call any LLM.

Do not call ComfyUI.

Do not call Suno.

Do not execute Lua.

Do not execute generators.

## Required page contract

Add a new editor page:

```text
Id: unity_archive_review
Title: Unity Archive Review
```

Sort order:

- choose a unique sort order after `CompositionWorkbenchPageControl` and before `ValidationPageControl` if possible;
- if not obvious, choose a stable unique order and document it in the final report.

The page must be registered in `src/LLMGameCreator.WinForms/CompositionRoot.cs` and included in `EditorPageRegistry`.

## Required presenter behavior

Implement `UnityArchiveReviewPresenter` in WinForms layer.

It should read existing files from the current project folder:

```text
<project>/.llmgc/unity-archive/production/archive-review.json
<project>/.llmgc/unity-archive/production/archive-review.md
<project>/.llmgc/unity-archive/production/archive-review-history-index.json
<project>/.llmgc/unity-archive/production/archive-review-comparison.json
<project>/.llmgc/unity-archive/production/archive-review-comparison.md
```

It may also list:

```text
<project>/.llmgc/unity-archive/review-history/*/archive-review.json
```

Presenter output should be a view state with at least:

```text
ProjectFolder
ArchiveRoot
Status
CurrentReviewReadiness
ComparisonReadiness
HistorySnapshotCount
SelectedSnapshotId
HistorySnapshots
CurrentReviewMarkdown
ComparisonMarkdown
CurrentReviewJson
ComparisonJson
HistoryIndexJson
CanRefresh
CanOpenArchiveFolder
```

Recommended view state classes:

```text
UnityArchiveReviewViewState
UnityArchiveReviewSnapshotOption
```

Presenter must:

- handle `null` or empty project folder;
- handle missing archive root;
- handle missing files individually;
- handle invalid JSON without throwing;
- read Markdown even if JSON is invalid;
- never write files;
- never call write-capable S023/S024 services;
- return stable user-facing messages.

## Required UI behavior

`UnityArchiveReviewPageControl` should:

- implement `IEditorPage`;
- have a default constructor that initializes design/unavailable mode;
- have runtime constructor with:
  - `UnityArchiveReviewPresenter`;
  - `ICurrentGamePackageService`;
- refresh on load/activation;
- refresh when current project changes;
- use `RunBusyAsync` or equivalent to avoid UI lockups;
- display read-only text only;
- not mutate archive files;
- not invoke providers/generators/Unity/Runtime.

Suggested controls:

Toolbar:

```text
Refresh
Open archive folder
```

Left panel:

```text
Current review readiness
Comparison readiness
History snapshot count
History snapshot list
Missing files / status summary
```

Right panel:

```text
TabControl:
- Current Review
- Comparison
- Current Review JSON
- Comparison JSON
- History Index JSON
```

All text boxes must be read-only, multiline, scrollable, monospace for JSON/Markdown.

If you choose a simpler layout, it must still show current review, comparison and history list.

## Open folder behavior

Optional but allowed:

- only enable if archive root exists;
- use `ProcessStartInfo` with `UseShellExecute = true`;
- catch `IOException`, `UnauthorizedAccessException`, `InvalidOperationException`, `Win32Exception`.

If this adds too much risk, omit it.

## Required tests

Add focused WinForms/presenter tests.

Minimum:

1. `PresenterInitializesWithoutProject`
2. `PresenterReportsMissingArchive`
3. `PresenterReadsExistingReviewHistoryAndComparisonReports`
4. `PresenterHandlesInvalidJsonWithoutThrowing`
5. `UserControlCanBeConstructedWithoutRuntimeServices`
6. `CompositionRootRegistersArchiveReviewPage`
7. Product smoke: `UnityArchiveReviewReadonlySmokeTests`.

Product smoke should construct presenter/page, create temp project report files, assert view state displays reports, and assert no archive writes.

## Product smoke

Preferred scenario id:

```text
unity-archive-review-ui-readonly
```

If adding a scenario requires updating `.devflow/scripts/run-product-smoke.ps1`, this is allowed for this purpose only. If not added, still add ProductSmoke tests and document them.

## State docs update

After successful implementation, update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
```

Set last completed product slice to:

```text
product_slice_025_read_only_archive_review_history_ui
```

Summary:

```text
Read-only WinForms UI for existing Unity archive review/history/comparison outputs.
```

Keep M5/M6 locked.

Recommended next work after S025:

```text
controlled_manual_provider_output_import
```

or

```text
one_explicitly_approved_product_vertical_slice
```

Do not unlock M5/M6.

## Validation commands

Run:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ArchiveReview|FullyQualifiedName~UnityArchiveReview"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~WinForms"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

If a product smoke scenario was added:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-review-ui-readonly
```

## System gates

The task is not complete unless:

- focused S025 tests pass;
- WinForms tests pass;
- ProductSmoke tests pass;
- `check-devflow-state.ps1` passes;
- `check-all.ps1` passes;
- build has no unexpected warnings;
- M5/M6 remain locked in current state docs;
- no forbidden files were touched;
- no archive files are written by the UI/presenter.

## Stop conditions

Stop and report instead of continuing if:

- you need to modify Runtime;
- you need to modify GamePackage schema;
- you need to modify Application archive services/models;
- you need to modify generator-library;
- you need to modify `.sln` or `*.csproj`;
- you need a provider/LLM/generator/Lua/Unity call;
- you cannot keep the page read-only;
- you cannot register the page without touching forbidden project files;
- the current repository structure differs enough that the allowed file list is insufficient.

## Expected final report

Report in Russian:

- files read;
- files changed;
- UI page id/title/sort order;
- exact files the presenter reads;
- missing/invalid file behavior;
- tests run with pass/fail counts;
- product smoke result;
- `check-devflow-state` result;
- `check-all` result;
- confirmation that forbidden areas were not touched;
- confirmation that UI is read-only and does not execute providers/generators/Unity/Runtime;
- recommendation: ready for user review or needs repair.

## Completion record

Status: completed on 2026-06-24.

- `ArchiveReview` / `UnityArchiveReview`: 37/37 passed.
- `WinForms`: 42/42 passed.
- `ProductSmoke`: 24/24 passed.
- `unity-archive-review-ui-readonly`: 1/1 passed.
- `check-devflow-state.ps1`: passed in `STOP_REVIEW` mode.
- `check-all.ps1`: 619/619 passed; build 0 warnings / 0 errors.
- M5/M6 remain Locked.
