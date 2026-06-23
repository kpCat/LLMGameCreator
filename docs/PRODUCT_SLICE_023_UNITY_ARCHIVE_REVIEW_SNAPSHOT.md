# Product Slice 023: Unity Archive Read-only Review Snapshot v1

## Goal

Add a read-only archive review layer that inspects an already-materialized `.llmgc/unity-archive` directory and writes deterministic review outputs:

- `production/archive-review.json`
- `production/archive-review.md`

This slice does not create Unity files, does not execute providers, does not execute Lua, does not call an LLM, and does not change Runtime, WinForms, GamePackage schema, generator-library, solution files or project files.

## Why this slice exists

Product Slices 017-022 established the deterministic archive pipeline:

1. dry-run validation;
2. archive materialization;
3. optional GamePackage payload;
4. asset/audio/Lua request metadata;
5. provider job planning without execution;
6. fulfillment state scanning.

The archive is now useful but still hard to inspect as a product artifact. S023 adds a bounded read-only review gate that summarizes whether the archive is coherent, what it contains, what is missing, what diagnostics exist, and what provider/fulfillment state looks like.

## Inputs

The service reads only files under an existing `.llmgc/unity-archive` directory.

Primary inputs:

- `export-validation.json`
- `production/readiness-report.json`
- `production/fulfillment-state.json`
- `production/invalid-outputs.json`
- `assets/asset-requests.json`
- `audio/audio-requests.json`
- `lua/module-requests.json`
- provider job batch files under `providers/*/jobs.json`

## Outputs

- `production/archive-review.json`
- `production/archive-review.md`

Output JSON must be deterministic:

- no timestamps;
- no absolute paths;
- stable ordering;
- source archive paths are archive-relative;
- repeated review of unchanged archive is byte-identical.

## Product smoke

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-review-snapshot
```
