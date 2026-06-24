# Product Slice 025: Read-only Archive Review/History UI

Status: implemented and verified

## Delivered behavior

The WinForms editor now includes a read-only `Unity Archive Review` page (`unity_archive_review`, sort order `41`). It reads existing S023/S024/S024.1 outputs from the current project and never generates, exports, materializes, or mutates archive content.

The page displays current review Markdown/JSON, comparison Markdown/JSON, history index JSON, readiness values, project/archive paths, a missing/invalid-file summary, and the available snapshot list. It refreshes on load, activation, and current-project changes. The archive-folder button is enabled only when the archive directory exists.

## Read-only input files

```text
<project>/.llmgc/unity-archive/production/archive-review.json
<project>/.llmgc/unity-archive/production/archive-review.md
<project>/.llmgc/unity-archive/production/archive-review-history-index.json
<project>/.llmgc/unity-archive/production/archive-review-comparison.json
<project>/.llmgc/unity-archive/production/archive-review-comparison.md
<project>/.llmgc/unity-archive/review-history/*/archive-review.json
```

Missing project/archive/report files and invalid JSON produce view-state diagnostics rather than exceptions. Markdown remains visible when its adjacent JSON is invalid.

## Architecture

- `UnityArchiveReviewPresenter` owns filesystem reads and maps existing Application models into a WinForms view state.
- `UnityArchiveReviewPageControl` is an `IEditorPage` with a designer-safe parameterless constructor.
- Layout remains entirely in `UnityArchiveReviewPageControl.Designer.cs`.
- Runtime construction and registry inclusion are configured only in `CompositionRoot`.
- Existing write-capable S023/S024/S024.1 Application services are not called.

Sort order `41` is the nearest stable unique integer after Composition Workbench (`39`). Validation already occupies `40`, so placing S025 numerically between those existing values is not possible without changing an out-of-scope page.

## Verification

```text
ArchiveReview/UnityArchiveReview filter: 37/37 passed
WinForms filter: 42/42 passed
ProductSmoke filter: 24/24 passed
unity-archive-review-ui-readonly scenario: 1/1 passed
check-devflow-state.ps1: passed (STOP_REVIEW)
check-all.ps1: passed, 619/619 tests, build 0 warnings / 0 errors
```

The product smoke captures every archive file before and after presenter/page construction and refresh, proving that file names and bytes are unchanged.

No Unity implementation, provider/LLM/generator/Lua execution, Runtime behavior, GamePackage schema, Application archive service/model, solution, or project file was changed.
