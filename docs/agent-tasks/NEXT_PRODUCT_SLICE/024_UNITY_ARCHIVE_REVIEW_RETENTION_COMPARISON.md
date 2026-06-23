# Product Slice 024: Unity Archive Review Retention & Comparison v1

Task ID: PRODUCT_SLICE_024_UNITY_ARCHIVE_REVIEW_RETENTION_COMPARISON_V1

Executor: Kilo Code first.

## Goal

Implement a bounded, Application-layer, headless retention and comparison layer for Product Slice 023 Unity archive review snapshots.

S023 already writes:

```text
.llmgc/unity-archive/production/archive-review.json
.llmgc/unity-archive/production/archive-review.md
```

S024 must:

1. store deterministic content-hash snapshots of `archive-review.json`;
2. maintain a deterministic history index;
3. compare the current snapshot to the previous distinct snapshot;
4. write deterministic comparison JSON and Markdown.

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

Do not use container paths in code, docs, tests, scripts or final report.

## Read first

Read these files before editing anything:

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/PRODUCT_SMOKE_SCENARIOS.md
.devflow/scripts/run-product-smoke.ps1
src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotMarkdownRenderer.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveReviewSnapshotTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewSnapshotSmokeTests.cs
src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveFulfillmentStateModels.cs
```

Also inspect nearby composition service/test style before adding new files.

## Allowed files

You may add or update only these areas:

```text
src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonMarkdownRenderer.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveReviewHistoryTests.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveReviewComparisonTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewHistorySmokeTests.cs
docs/PRODUCT_SLICE_024_UNITY_ARCHIVE_REVIEW_RETENTION_COMPARISON.md
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/agent-tasks/NEXT_PRODUCT_SLICE/024_UNITY_ARCHIVE_REVIEW_RETENTION_COMPARISON.md
docs/agent-tasks/NEXT_PRODUCT_SLICE/024_KILO_PROMPT.md
.devflow/scripts/run-product-smoke.ps1
.devflow/CURRENT_RUN.md
```

If a required change appears outside this list, stop and report it instead of editing.

## Forbidden files and areas

Do not edit:

```text
src/LLMGameCreator.Runtime/
src/LLMGameCreator.Runtime.Abstractions/
src/LLMGameCreator.WinForms/
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

Do not add WinForms UI.

## Required output contract

### Snapshot storage

Input:

```text
.llmgc/unity-archive/production/archive-review.json
```

Output:

```text
.llmgc/unity-archive/review-history/<snapshot-id>/archive-review.json
.llmgc/unity-archive/production/archive-review-history-index.json
```

Rules:

- `<snapshot-id>` must be content-hash based.
- Use SHA-256 over normalized review JSON.
- Prefer full lowercase hex.
- Do not use timestamps.
- Do not store absolute paths.
- Use stable ordering.
- If the same snapshot is stored twice, do not duplicate it in the index.
- Repeated unchanged output must be byte-identical.

### Comparison output

Output:

```text
.llmgc/unity-archive/production/archive-review-comparison.json
.llmgc/unity-archive/production/archive-review-comparison.md
```

If only one snapshot exists, write a deterministic report with readiness `NoPreviousSnapshot`.

If no review JSON exists, return/report readiness `MissingReview` and do not throw.

If review JSON is invalid, return/report readiness `Invalid` and do not throw.

### Comparison dimensions

Compare current vs previous:

- review readiness;
- validation materialization readiness;
- provider plan readiness;
- source file count;
- diagnostic total/error/warning/info counts;
- fulfillment total/missing/available/invalid/invalid-output counts;
- provider asset/audio/Lua slot counts and provider job count;
- request asset/audio/Lua request counts;
- invalid output reasons;
- diagnostics added/resolved/unchanged;
- source files added/removed/unchanged.

Use stable fingerprints:

Diagnostic fingerprint:

```text
severity|code|sourceFile|targetId|message
```

Source-file fingerprint:

```text
relativePath|kind
```

Invalid-reason fingerprint:

```text
reason
```

## Recommended model names

Use these names unless there is a strong reason not to:

```text
UnityArchiveReviewHistoryRequest
UnityArchiveReviewHistoryResult
UnityArchiveReviewHistoryReport
UnityArchiveReviewHistorySnapshotEntry
UnityArchiveReviewHistoryDiagnostic

UnityArchiveReviewComparisonRequest
UnityArchiveReviewComparisonResult
UnityArchiveReviewComparisonReport
UnityArchiveReviewComparisonSummary
UnityArchiveReviewComparisonDelta
UnityArchiveReviewComparisonDiagnosticChange
UnityArchiveReviewComparisonSourceFileChange
UnityArchiveReviewComparisonInvalidReasonChange

UnityArchiveReviewHistoryReadiness
UnityArchiveReviewComparisonReadiness
```

Recommended readiness values:

```text
Ready
ReadyWithWarnings
NoPreviousSnapshot
MissingReview
Invalid
Blocked
```

## Exact behavior details

1. `UnityArchiveReviewHistoryService` should:
   - take archive directory path;
   - read `production/archive-review.json`;
   - compute deterministic snapshot id;
   - copy normalized review JSON to `review-history/<snapshot-id>/archive-review.json`;
   - update/write `production/archive-review-history-index.json`;
   - sort index entries by snapshot id or deterministic content hash order;
   - return report/result with archive-relative written paths.

2. `UnityArchiveReviewComparisonService` should:
   - read history index and snapshot files;
   - identify current snapshot and previous distinct snapshot deterministically;
   - compare current vs previous;
   - write comparison JSON and Markdown;
   - support one-snapshot case as `NoPreviousSnapshot`;
   - never throw for missing review/history; return diagnostics.

3. The services must not depend on system time.

4. The services must not embed `DateTime.UtcNow`, `DateTime.Now`, `Stopwatch`, file last-write timestamps, creation timestamps, machine name, user name, process id or absolute paths into output JSON/Markdown.

5. Output JSON should use camelCase and string enums, consistent with S023.

6. Output text should be UTF-8 without BOM, consistent with S023.

7. Path handling must be containment-checked under archive root.

8. The comparison output must exclude itself from source-file deltas.

## Product smoke scenario

Add scenario to `.devflow/scripts/run-product-smoke.ps1`:

```powershell
elseif ($Scenario -eq "unity-archive-review-history") {
    $TestFilter = "FullyQualifiedName~UnityArchiveReviewHistoryProductSmoke"
}
```

Add docs to `docs/PRODUCT_SMOKE_SCENARIOS.md`.

Product smoke should:

1. materialize archive using existing S017-S023 services/presets;
2. run S023 review;
3. store first S024 snapshot;
4. make a controlled deterministic change that affects the review:
   - preferred: create one expected output file from an existing fulfillment slot, then rerun fulfillment scan/materialization/review if existing services support it safely;
   - acceptable: use two deterministic synthetic `archive-review.json` fixtures under a temp archive if that is safer and still proves S024 product flow;
5. store second snapshot;
6. run comparison;
7. assert comparison files exist and parse;
8. assert at least one delta is reported;
9. assert no timestamps and no absolute paths;
10. assert repeated unchanged compare is byte-identical.

## Tests

Add focused Application tests for:

- missing archive review returns `MissingReview` diagnostic and does not throw;
- invalid archive review JSON returns `Invalid` diagnostic and does not throw;
- storing one review creates `review-history/<hash>/archive-review.json`;
- storing the same review twice does not duplicate the index;
- history index is deterministic and timestamp-free;
- comparison with one snapshot returns `NoPreviousSnapshot`;
- comparison with two snapshots reports count deltas;
- comparison reports added/resolved/unchanged diagnostics;
- comparison reports added/removed/unchanged source files;
- comparison reports invalid reason deltas;
- repeated unchanged history/compare outputs are byte-identical;
- outputs do not contain timestamps or absolute archive root paths.

Add ProductSmoke test:

```text
UnityArchiveReviewHistoryProductSmoke
```

## Validation commands

Run these commands:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveReviewHistory|FullyQualifiedName~UnityArchiveReviewComparison"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-review-history
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## System gates

The task is not complete unless:

- all focused S024 tests pass;
- the new product smoke scenario passes;
- all ProductSmoke tests pass;
- `check-devflow-state.ps1` passes;
- `check-all.ps1` passes;
- build has no unexpected warnings;
- M5/M6 remain locked in current state docs;
- no forbidden files were touched.

## Stop conditions

Stop and report instead of continuing if:

- you need to modify Runtime;
- you need to modify WinForms;
- you need to modify GamePackage schema;
- you need to modify generator-library;
- you need to modify `.sln` or `*.csproj`;
- you need a timestamp to satisfy the feature;
- you need a provider/LLM/generator/Lua call;
- you cannot keep output deterministic;
- you cannot keep paths archive-relative;
- the current repository structure differs enough that the allowed file list is insufficient.

## Expected final report

Report in Russian:

- files read;
- files changed;
- summary of implementation;
- output contract;
- determinism guarantees;
- tests run with pass/fail counts;
- product smoke result;
- `check-devflow-state` result;
- `check-all` result;
- confirmation that forbidden areas were not touched;
- recommendation: ready for user review or needs repair.

## Next task pointer

After S024 is green, recommend one of:

1. read-only archive review/history UI;
2. controlled manual provider output import;
3. one explicitly approved controlled product vertical slice.

Do not unlock M5/M6 automatically.
