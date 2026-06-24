# Product Slice 027: Controlled Manual Import Workspace UI v1

## Delivered workflow

The existing `Unity Archive Review` page now provides a complete controlled manual-import workspace over the S026 archive contract:

1. inspect and filter expected asset, audio, Lua, or unknown fulfillment slots;
2. create a deterministic missing/invalid-only manifest template;
3. create/open the archive-local `manual-import/` folder;
4. place user-supplied files and prepare `import-manifest.json`;
5. run the existing `UnityArchiveManualProviderImportService` with overwrite disabled by default;
6. refresh slot state, manual import reports, current review, history, comparison, and selected snapshot detail.

This slice does not execute providers, generators, LLMs, Lua, Unity, or Runtime gameplay.

## Slot dashboard

`UnityArchiveManualImportTemplateService` reads and safely merges:

```text
production/fulfillment-plan.json
production/fulfillment-state.json
assets/asset-slots.json
audio/audio-slots.json
lua/module-slots.json
```

The read-only grid exposes slot id, kind, provider kind, expected output path, fulfillment status, file existence, byte size, SHA-256 when readable, and request id. Selection detail also shows source id and the suggested manifest source path. Filters cover all, missing, available, invalid, manual-import provider, and future providers.

Missing archives, missing metadata, malformed JSON, absent fulfillment state, and missing/invalid reports become stable presenter status instead of exceptions. Typed slot indexes may supplement the plan, and the plan remains usable when typed indexes are absent.

## Manifest template

The template action writes only:

```text
.llmgc/unity-archive/manual-import/import-manifest.template.json
```

It never overwrites:

```text
.llmgc/unity-archive/manual-import/import-manifest.json
```

Entries are sorted deterministically and include only missing/invalid slots. Suggested source paths use `put-files-here/<safe-slot-name>.<extension>`, are relative to `manual-import/`, and contain no backslashes, drive prefixes, traversal, or empty segments.

## Import and overwrite gate

`Run manual import` calls `UnityArchiveManualProviderImportService.ImportAsync` with the current archive root, the default manual import folder/manifest, fulfillment and review refresh enabled, and `OverwriteExisting = false` unless the user explicitly checks the risk-labelled overwrite option.

The S026 service remains the authority for manifest validation, path containment, extension matching, conflict handling, copying, hashing, report generation, and refresh. A missing manifest writes/loads the existing report and gives the user a direct instruction to copy/edit the template as `import-manifest.json`.

## Verification

- ManualImport/UnityArchiveReview filtered tests: 51/51 passed.
- WinForms filtered tests: 52/52 passed.
- `unity-archive-manual-import-workflow-ui`: 1/1 passed.
- ProductSmoke filtered tests: 26/26 passed.
- Repository state/check-all results are recorded in `docs/CURRENT_GENERATOR_STATE.md` and `.devflow/CURRENT_RUN.md`.

M5 and M6 remain Locked. Runtime, GamePackage schema, generator-library, solution, and project files are unchanged.
