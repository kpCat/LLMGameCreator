# Product Slice 024: Unity Archive Review Retention & Comparison v1

## Goal

Add a bounded Application-layer, headless retention and comparison layer for the existing Unity archive review snapshot produced by Product Slice 023.

Product Slice 023 writes the current archive review:

```text
.llmgc/unity-archive/production/archive-review.json
.llmgc/unity-archive/production/archive-review.md
```

Product Slice 024 should persist deterministic review snapshots and compare the current snapshot against the previous one.

## Why this slice exists

S023 answers:

> What does the current materialized archive look like?

S024 should answer:

> What changed since the previous review snapshot, and did the archive get better or worse?

This is useful before adding UI, provider import, Lua execution, Runtime expansion, or Unity output because it creates a stable, deterministic product-quality gate over the archive itself.

## Required behavior

### Snapshot retention

Read an existing:

```text
.llmgc/unity-archive/production/archive-review.json
```

Store a deterministic copy under:

```text
.llmgc/unity-archive/review-history/<snapshot-id>/archive-review.json
```

Write/update a deterministic index:

```text
.llmgc/unity-archive/production/archive-review-history-index.json
```

Rules:

- `<snapshot-id>` must be derived from content hash, not from current time.
- Use SHA-256 over normalized `archive-review.json` content.
- Lowercase hex is preferred.
- Do not store timestamps.
- Do not store absolute paths.
- Re-storing the same review must not duplicate the index.
- The same unchanged archive must produce byte-identical history outputs.

### Comparison

When at least two distinct snapshots exist, compare the newest/current snapshot against the previous snapshot and write:

```text
.llmgc/unity-archive/production/archive-review-comparison.json
.llmgc/unity-archive/production/archive-review-comparison.md
```

When only one snapshot exists, write a deterministic comparison report that clearly says no previous snapshot exists.

Comparison should cover at least:

- review readiness before/after;
- validation/materialization readiness before/after;
- provider summary deltas:
  - asset slots;
  - audio slots;
  - Lua module slots;
  - provider job count;
- fulfillment summary deltas:
  - total slots;
  - missing;
  - available;
  - invalid;
  - invalid output count;
- request summary deltas:
  - asset requests;
  - audio requests;
  - Lua module requests;
- diagnostic count deltas:
  - total;
  - errors;
  - warnings;
  - info;
- diagnostics added/resolved/unchanged by stable fingerprint;
- source files added/removed/unchanged by archive-relative path and kind;
- invalid output reason deltas.

## Determinism requirements

All new JSON/Markdown outputs must be deterministic:

- UTF-8 without BOM.
- No timestamps.
- No absolute paths.
- Stable ordering.
- Stable casing for machine-readable status values.
- Output files must not include local machine-specific temporary paths.
- Repeated unchanged snapshot/compare runs must be byte-identical.

## Readiness model

Add a small S024-specific readiness model rather than reusing unrelated readiness enums directly.

Recommended values:

```text
Ready
ReadyWithWarnings
NoPreviousSnapshot
MissingReview
Invalid
Blocked
```

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

## Expected product smoke

Add a new product smoke scenario:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-review-history
```

The smoke should prove:

1. materialization creates a Unity archive;
2. S023 review writes `archive-review.json`;
3. S024 stores the first deterministic content-hash snapshot;
4. S024 can store a second distinct snapshot after a controlled deterministic archive-state change;
5. comparison JSON/Markdown are written;
6. comparison reports at least one meaningful delta;
7. outputs contain no timestamps or absolute paths;
8. repeated unchanged history/compare outputs are byte-identical;
9. no Unity/provider/LLM/generator/Lua/Runtime/WinForms/GamePackage schema path is executed.

## Recommended next step after S024

If S024 is green, the next safe options are:

1. a read-only WinForms viewer over existing S023/S024 review reports;
2. a controlled manual provider output import slice;
3. one explicitly selected product vertical slice.

M5/M6 remain locked unless the user explicitly unlocks them.
