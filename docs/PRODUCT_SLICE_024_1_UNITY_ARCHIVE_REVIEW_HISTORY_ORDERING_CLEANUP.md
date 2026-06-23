# Product Slice 024.1: Unity Archive Review History Ordering Cleanup

## Goal

Repair and harden the Product Slice 024 archive review history/comparison semantics before building any UI on top of it.

S024 added:

```text
.llmgc/unity-archive/review-history/<sha256>/archive-review.json
.llmgc/unity-archive/production/archive-review-history-index.json
.llmgc/unity-archive/production/archive-review-comparison.json
.llmgc/unity-archive/production/archive-review-comparison.md
```

The current history storage is deterministic, but the comparison layer risks choosing the "previous" snapshot by hash ordering instead of actual distinct snapshot insertion order.

S024.1 must keep the existing output contract compatible while adding explicit deterministic sequence/ordinal metadata to the history index.

## Problem

A content hash is stable, but hash sorting does not encode history order.

If snapshots are created as:

```text
A -> B -> C
```

then comparing current `C` should compare against `B`.

It must not compare against whichever of `A` or `B` has a lexicographically larger SHA-256 hash.

## Required fix

Add an insertion-order field to `UnityArchiveReviewHistorySnapshotEntry`, for example:

```text
Sequence
```

Rules:

- First distinct stored snapshot gets sequence `1`.
- Next distinct stored snapshot gets sequence `2`.
- Re-storing the same snapshot id does not create a duplicate and does not change its sequence.
- History index entries should be stored in stable `Sequence`, then `SnapshotId` order.
- Existing index entries without sequence should be migrated deterministically:
  - assign sequence by existing index order if available;
  - otherwise by stable fallback order;
  - preserve snapshot ids and relative paths.
- Current-vs-previous comparison must use sequence:
  - current snapshot id = content hash of current `production/archive-review.json`;
  - current entry = matching history index entry;
  - previous entry = highest sequence lower than current entry sequence;
  - if no previous lower-sequence entry exists, readiness `NoPreviousSnapshot`.
- If current review is not in history index:
  - do not guess previous by hash;
  - return deterministic diagnostic/readiness, or store current first if the existing service contract clearly supports that path.
  - Prefer not mutating history from comparison service unless S024 already does so intentionally.

## Diagnostics completeness

Harden missing/invalid cases:

- Missing archive root or missing `production/archive-review.json` should return `MissingReview` with diagnostic code.
- Invalid `archive-review.json` should return `Invalid` with diagnostic code.
- Invalid/corrupt `archive-review-history-index.json` should return a deterministic diagnostic, not silently pretend no previous snapshot unless the report clearly records the problem.
- Missing previous snapshot file referenced by index should return/report a deterministic diagnostic.

Recommended diagnostic codes:

```text
unity.archive_review_history.missing_review
unity.archive_review_history.invalid_review_json
unity.archive_review_history.invalid_history_index
unity.archive_review_history.missing_snapshot_file
unity.archive_review_history.current_snapshot_not_indexed
```

Keep codes stable and machine-readable.

## Compatibility

Do not break the existing S024 output paths.

Adding fields to JSON is allowed if tests confirm existing behavior remains green.

Do not rename existing output files.

Do not change S023 archive-review schema except for moving the S024 fingerprint helper if needed.

## Non-goals

This slice must not:

- create a Unity project;
- execute Unity;
- call any LLM/provider;
- call ComfyUI/Suno/local provider code;
- execute Lua;
- execute generators;
- change Runtime;
- change WinForms UI;
- change GamePackage schema;
- change generator-library;
- change `.sln` or `*.csproj`.

## Expected result

After S024.1:

- history index records deterministic sequence/ordinal order;
- comparison uses actual previous distinct snapshot;
- missing/invalid cases include diagnostics;
- existing `unity-archive-review-history` product smoke remains green;
- focused S024 tests remain green;
- full `check-all` remains green.
