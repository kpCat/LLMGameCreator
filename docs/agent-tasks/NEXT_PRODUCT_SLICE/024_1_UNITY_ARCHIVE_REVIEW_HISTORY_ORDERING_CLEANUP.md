# Product Slice 024.1: Unity Archive Review History Ordering Cleanup

Task ID: PRODUCT_SLICE_024_1_UNITY_ARCHIVE_REVIEW_HISTORY_ORDERING_CLEANUP

Executor: Kilo Code first.

## Goal

Fix S024 archive review history ordering semantics and diagnostics completeness before building read-only UI over review/history/comparison reports.

S024 is accepted and green, but comparison currently risks selecting the "previous" snapshot by SHA-256 hash ordering. S024.1 must make "previous" mean the immediately previous distinct stored snapshot.

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
docs/PRODUCT_SLICE_024_UNITY_ARCHIVE_REVIEW_RETENTION_COMPARISON.md
.devflow/scripts/run-product-smoke.ps1
src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonMarkdownRenderer.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotModels.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveReviewHistoryTests.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveReviewComparisonTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewHistorySmokeTests.cs
```

Also inspect existing test style before editing.

## Allowed files

You may update only these areas:

```text
src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonMarkdownRenderer.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotModels.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveReviewHistoryTests.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveReviewComparisonTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewHistorySmokeTests.cs
docs/PRODUCT_SLICE_024_1_UNITY_ARCHIVE_REVIEW_HISTORY_ORDERING_CLEANUP.md
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/agent-tasks/NEXT_PRODUCT_SLICE/024_1_UNITY_ARCHIVE_REVIEW_HISTORY_ORDERING_CLEANUP.md
docs/agent-tasks/NEXT_PRODUCT_SLICE/024_1_KILO_PROMPT.md
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

## Required behavior

### 1. Add deterministic insertion order to history index

Add a sequence/ordinal field to `UnityArchiveReviewHistorySnapshotEntry`, for example:

```csharp
public int Sequence { get; init; }
```

Rules:

- first distinct snapshot gets `Sequence = 1`;
- each next distinct snapshot gets `max(existing.Sequence) + 1`;
- storing identical content again does not duplicate index and does not alter the existing sequence;
- index JSON should be ordered by `Sequence`, then `SnapshotId`;
- no timestamps;
- no absolute paths.

### 2. Deterministic migration for old index entries

Existing S024 indexes may not have `Sequence`.

Handle this without crashing:

- if all entries have valid positive sequence, preserve them;
- if any entry lacks sequence or has `0`, assign deterministic sequence values;
- preferred migration order:
  1. current file order as deserialized if it is stable;
  2. fallback to `SnapshotId` ordinal order if needed;
- write the migrated index only when the store operation writes/updates the index;
- do not invent timestamps.

### 3. Use sequence for previous snapshot selection

Update `UnityArchiveReviewComparisonService`.

Comparison must select previous snapshot like this:

```text
currentSnapshotId = hash(current archive-review.json)
currentEntry = history index entry with currentSnapshotId
previousEntry = entry with highest Sequence lower than currentEntry.Sequence
```

If no previous entry exists, readiness is `NoPreviousSnapshot`.

Do not select previous by lexicographic hash ordering.

If current snapshot id is missing from the index:

- do not compare against arbitrary hash;
- return/report deterministic readiness/diagnostic;
- acceptable readiness: `NoPreviousSnapshot` or `Blocked`, but diagnostic must be explicit.

### 4. Diagnostics completeness

Add diagnostics to history/comparison reports where currently missing.

At minimum:

- missing archive root or missing `production/archive-review.json`;
- invalid `archive-review.json`;
- invalid/corrupt history index;
- missing snapshot file referenced by index;
- current snapshot not present in history index.

Use stable diagnostic codes.

Recommended codes:

```text
unity.archive_review_history.missing_review
unity.archive_review_history.invalid_review_json
unity.archive_review_history.invalid_history_index
unity.archive_review_history.missing_snapshot_file
unity.archive_review_history.current_snapshot_not_indexed
```

If existing model lacks diagnostics, add:

```csharp
public IReadOnlyList<UnityArchiveReviewSnapshotDiagnostic> Diagnostics { get; init; }
```

or a S024-specific diagnostic record if cleaner.

Prefer reuse of `UnityArchiveReviewSnapshotDiagnostic` only if it does not pollute S023 semantics.

### 5. Keep existing output contract

Do not rename existing S024 output paths:

```text
.llmgc/unity-archive/review-history/<sha256>/archive-review.json
.llmgc/unity-archive/production/archive-review-history-index.json
.llmgc/unity-archive/production/archive-review-comparison.json
.llmgc/unity-archive/production/archive-review-comparison.md
```

Adding JSON fields is allowed.

### 6. Determinism

All outputs must remain:

- UTF-8 without BOM;
- timestamp-free;
- absolute-path-free;
- stable ordered;
- byte-identical on repeated unchanged runs.

## Required tests

Add or update focused tests.

Minimum new test cases:

1. `HistoryAssignsMonotonicSequenceForDistinctSnapshots`
   - store A, store B, store C;
   - assert sequences 1, 2, 3.

2. `HistoryDoesNotChangeSequenceWhenSameSnapshotStoredAgain`
   - store A twice;
   - assert one index entry and sequence unchanged.

3. `ComparisonUsesPreviousSequenceNotHashOrder`
   - create three deterministic snapshots A, B, C with hash order intentionally not matching insertion order if practical;
   - current C must compare against B;
   - assert `PreviousSnapshotId == snapshotIdB`.

4. `ComparisonReportsCurrentSnapshotNotIndexed`
   - current archive-review.json hash is absent from index;
   - assert deterministic readiness and diagnostic code.

5. `HistoryMissingReviewIncludesDiagnostic`
   - missing archive/review returns `MissingReview` plus diagnostic.

6. `HistoryInvalidReviewIncludesDiagnostic`
   - invalid archive-review.json returns `Invalid` plus diagnostic.

7. `ComparisonMissingSnapshotFileIncludesDiagnostic`
   - index references missing snapshot file;
   - assert diagnostic and no throw.

8. Regression: repeated unchanged store/compare remains byte-identical.

Update product smoke if needed, but keep the existing scenario id:

```text
unity-archive-review-history
```

## Validation commands

Run:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveReviewHistory|FullyQualifiedName~UnityArchiveReviewComparison"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-review-history
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## System gates

The task is not complete unless:

- focused history/comparison tests pass;
- `unity-archive-review-history` product smoke passes;
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
- you need a timestamp;
- you need provider/LLM/generator/Lua execution;
- you cannot keep output deterministic;
- you cannot keep paths archive-relative;
- the current repository structure differs enough that the allowed file list is insufficient.

## Expected final report

Report in Russian:

- files read;
- files changed;
- exact ordering/sequence behavior implemented;
- diagnostics added;
- compatibility notes;
- tests run with pass/fail counts;
- product smoke result;
- `check-devflow-state` result;
- `check-all` result;
- confirmation that forbidden areas were not touched;
- recommendation: ready for user review or needs repair.

## Next task pointer

After S024.1 is green, recommend:

```text
Product Slice 025: Read-only Archive Review/History UI
```

Do not unlock M5/M6 automatically.
