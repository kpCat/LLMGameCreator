# Product Slice 026: Controlled Manual Provider Output Import v1 + Archive Review UX Snapshot Detail

Task ID: PRODUCT_SLICE_026_CONTROLLED_MANUAL_PROVIDER_OUTPUT_IMPORT_V1

Executor: Codex.

## Goal

Implement a controlled manual provider output import service for already materialized Unity archives, and polish the existing `Unity Archive Review` read-only UI so selecting a history snapshot displays the selected snapshot's JSON.

This slice is allowed to copy explicitly user-provided files into the archive's expected output paths, but it must not execute any provider or generation process.

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

src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveProviderJobPlanModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveProviderJobPlanService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveFulfillmentStateModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveFulfillmentStateService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonMarkdownRenderer.cs

src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPresenter.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewViewState.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/UnityArchiveFulfillmentStateTests.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveReviewHistoryTests.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveReviewComparisonTests.cs
tests/LLMGameCreator.Tests/WinForms/UnityArchiveReviewPresenterTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewReadonlySmokeTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewHistorySmokeTests.cs
```

Important existing file paths from materialization:

```text
production/fulfillment-plan.json
production/readiness-report.json
production/fulfillment-state.json
production/fulfilled-assets-index.json
production/fulfilled-audio-index.json
production/fulfilled-lua-index.json
production/invalid-outputs.json
assets/asset-slots.json
audio/audio-slots.json
lua/module-slots.json
```

## Allowed files

You may add or update only these areas:

```text
src/LLMGameCreator.Application/Composition/UnityArchiveManualProviderImportModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveManualProviderImportService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveManualProviderImportMarkdownRenderer.cs

src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPresenter.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewViewState.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/UnityArchiveManualProviderImportTests.cs
tests/LLMGameCreator.Tests/WinForms/UnityArchiveReviewPresenterTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveManualProviderImportSmokeTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewReadonlySmokeTests.cs

.devflow/scripts/run-product-smoke.ps1
.devflow/CURRENT_RUN.md

docs/PRODUCT_SLICE_026_CONTROLLED_MANUAL_PROVIDER_OUTPUT_IMPORT.md
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/agent-tasks/NEXT_PRODUCT_SLICE/026_CONTROLLED_MANUAL_PROVIDER_OUTPUT_IMPORT.md
docs/agent-tasks/NEXT_PRODUCT_SLICE/026_CODEX_PROMPT.md
```

If a required change appears outside this list, stop and report it instead of editing.

Do not update `.sln` or `*.csproj`; the existing projects should include new `.cs` files by globbing.

## Forbidden files and areas

Do not edit:

```text
src/LLMGameCreator.Runtime/
src/LLMGameCreator.Runtime.Abstractions/
src/LLMGameCreator.GamePackage/
src/LLMGameCreator.Scripting/
src/LLMGameCreator.Infrastructure/
generator-library/
LLMGameCreator.sln
*.csproj
```

Do not change GamePackage schema.

Do not change Runtime behavior.

Do not implement Unity.

Do not add provider execution.

Do not call any LLM.

Do not call ComfyUI.

Do not call Suno.

Do not execute Lua.

Do not execute generators.

## Manual import contract

Add a new Application service:

```text
UnityArchiveManualProviderImportService
```

Recommended request model:

```text
UnityArchiveManualProviderImportRequest
- ArchiveDirectoryPath
- ImportDirectoryRelativePath = "manual-import"
- ManifestRelativePath = "manual-import/import-manifest.json"
- OverwriteExisting = false
- RefreshFulfillmentState = true
- RefreshReviewHistoryComparison = true
```

Recommended manifest model:

```json
{
  "schemaVersion": "1",
  "entries": [
    {
      "slotId": "asset-slot.hero_portrait",
      "sourceRelativePath": "files/hero_portrait.png",
      "expectedOutputRelativePath": "assets/generated/portrait/hero_portrait.png"
    }
  ]
}
```

`expectedOutputRelativePath` may be optional. If present, it must match the slot's expected path exactly.

## Slot source of truth

Read existing slot metadata from the already materialized archive:

```text
production/fulfillment-plan.json
assets/asset-slots.json
audio/audio-slots.json
lua/module-slots.json
```

Use these to map `slotId` to:

```text
ProviderKind
ExpectedOutputRelativePath
AssetId/AudioId/ModuleId when available
Kind
```

Do not invent target paths.

## Import validation rules

For every manifest entry:

- manifest exists and parses;
- `slotId` is non-empty;
- slot exists;
- source path is relative, safe, under `manual-import/`;
- source file exists;
- source file is non-empty;
- target path is the slot's `ExpectedOutputRelativePath`;
- target path is safe and stays under archive root;
- source extension equals target extension;
- duplicate slot entries are rejected or diagnosed;
- target conflict with different bytes is diagnosed unless `OverwriteExisting = true`;
- target with identical bytes is `AlreadyImported`/skipped.

Recommended diagnostic codes:

```text
manual_import.missing_manifest
manual_import.invalid_manifest_json
manual_import.duplicate_slot
manual_import.unknown_slot
manual_import.unsafe_source_path
manual_import.missing_source_file
manual_import.empty_source_file
manual_import.expected_output_mismatch
manual_import.unsafe_target_path
manual_import.extension_mismatch
manual_import.target_conflict
manual_import.copy_failed
manual_import.refresh_failed
```

## Import result/report

Write deterministic import reports:

```text
production/manual-provider-import-report.json
production/manual-provider-import-report.md
```

Recommended result fields:

```text
SchemaVersion
Readiness: Ready | ReadyWithWarnings | BlockedByErrors | MissingManifest | InvalidManifest
ImportedCount
SkippedCount
ConflictCount
InvalidCount
Entries[]
Diagnostics[]
WrittenRelativePaths[]
```

Entry fields:

```text
SlotId
ProviderKind
SourceRelativePath
ExpectedOutputRelativePath
Status: Imported | AlreadyImported | Conflict | Invalid | Failed
FileSizeBytes
ContentSha256
DiagnosticCodes[]
```

No timestamps.

No absolute paths in report JSON/Markdown.

Stable ordering by `SlotId`, then `ExpectedOutputRelativePath`.

UTF-8 without BOM.

## Refresh after import

After import, refresh fulfillment state using existing fulfillment state service if possible:

```text
production/fulfillment-state.json
production/fulfilled-assets-index.json
production/fulfilled-audio-index.json
production/fulfilled-lua-index.json
production/invalid-outputs.json
```

Then refresh review/history/comparison using existing S023/S024 services if possible:

```text
production/archive-review.json
production/archive-review.md
production/archive-review-history-index.json
production/archive-review-comparison.json
production/archive-review-comparison.md
```

If full refresh becomes too large, stop and report rather than touching forbidden services.

## UX polish required in same slice

Update the existing `Unity Archive Review` page so selecting a history snapshot displays that selected snapshot's JSON.

Presenter/view state should include:

```text
SelectedSnapshotJson
SelectedSnapshotStatus
SelectedSnapshotRelativePath
SelectedSnapshotSequence
```

UI should display these in a read-only tab/textbox.

Also display manual import report if present:

```text
production/manual-provider-import-report.json
production/manual-provider-import-report.md
```

Add tabs or read-only text boxes for:

- selected snapshot JSON;
- manual import report Markdown;
- manual import report JSON.

Do not add write/import buttons unless they are very small, fully tested, and only call the controlled manual import service. Prefer no import button in S026.

## Required tests

Add focused Application tests for:

1. missing import manifest returns `MissingManifest` and no throw;
2. invalid import manifest JSON returns `InvalidManifest` and no throw;
3. unknown slot is diagnosed and no file copied;
4. unsafe source path is rejected;
5. extension mismatch is rejected;
6. valid asset/audio/lua manual import copies to expected output path;
7. same bytes import is idempotent and skipped/already imported;
8. different existing target bytes conflict when overwrite is false;
9. import report JSON/Markdown are deterministic, timestamp-free and archive-relative;
10. fulfillment refresh sees imported file as available;
11. review/history/comparison refresh reflects import delta if implemented.

Update WinForms tests for:

1. selected history snapshot loads selected snapshot JSON;
2. missing selected snapshot shows status, no throw;
3. manual import report JSON/Markdown are displayed when present;
4. existing S025 missing/invalid behavior still passes;
5. default constructor remains safe.

Add ProductSmoke:

```text
UnityArchiveManualProviderImportSmokeTests
```

Smoke flow:

1. materialize a deterministic Unity archive with provider slots;
2. create `.llmgc/unity-archive/manual-import/import-manifest.json`;
3. create one valid source file under manual-import/files;
4. run manual import service;
5. assert expected target file exists;
6. assert fulfillment/review/history/comparison reports exist/updated if implemented;
7. assert Archive Review presenter displays import report and selected snapshot JSON;
8. assert no provider/LLM/generator/Lua/Unity/Runtime execution.

## Product smoke runner

Add scenario:

```text
unity-archive-manual-provider-import
```

in `.devflow/scripts/run-product-smoke.ps1`.

## State docs update

After successful implementation, update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
```

Set last completed product slice:

```text
product_slice_026_controlled_manual_provider_output_import_v1
```

Title:

```text
Controlled Manual Provider Output Import v1
```

Keep M5/M6 Locked.

Recommended next work after S026:

```text
controlled_manual_import_ui_action
```

or one explicitly approved product vertical slice.

## Validation commands

Run:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ManualProviderImport|FullyQualifiedName~UnityArchiveReview"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-manual-provider-import
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## System gates

The task is not complete unless:

- focused manual import tests pass;
- updated Unity Archive Review UI tests pass;
- new product smoke passes;
- all ProductSmoke tests pass;
- `check-devflow-state.ps1` passes;
- `check-all.ps1` passes;
- build has no unexpected warnings;
- M5/M6 remain locked in current state docs;
- no forbidden files were touched;
- no provider/generator/LLM/Lua/Unity/Runtime execution was added.

## Stop conditions

Stop and report instead of continuing if:

- you need to modify Runtime;
- you need to modify GamePackage schema;
- you need to modify generator-library;
- you need to modify `.sln` or `*.csproj`;
- you need provider/LLM/generator/Lua/Unity execution;
- you cannot validate paths safely;
- you cannot keep import reports timestamp-free and archive-relative;
- you cannot refresh fulfillment/review without touching forbidden services.

## Expected final report

Report in Russian:

- files read;
- files changed;
- manual import manifest contract;
- exact import validation rules implemented;
- exact files written by import;
- UX snapshot-detail polish implemented;
- tests run with pass/fail counts;
- product smoke result;
- `check-devflow-state` result;
- `check-all` result;
- confirmation that forbidden areas were not touched;
- confirmation that providers/generators/LLM/Lua/Unity/Runtime were not executed;
- recommendation: ready for user review or needs repair.
