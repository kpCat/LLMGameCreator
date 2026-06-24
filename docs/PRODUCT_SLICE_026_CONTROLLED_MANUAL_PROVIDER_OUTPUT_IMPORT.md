# Product Slice 026: Controlled Manual Provider Output Import v1

## Delivered behavior

`UnityArchiveManualProviderImportService` imports explicitly listed user files from an already materialized archive's `manual-import/` directory into existing asset, audio, or Lua fulfillment slots. It never invents a target path and does not execute providers, generators, LLMs, Lua, Unity, or Runtime gameplay.

Default manifest path:

```text
.llmgc/unity-archive/manual-import/import-manifest.json
```

Manifest contract:

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

`expectedOutputRelativePath` is optional in the manifest. When supplied, it must exactly match the materialized slot metadata.

## Validation and copy rules

- manifest and slot metadata must parse;
- `slotId` must be present, unique in the manifest, and exist in the materialized slot indexes;
- source paths must be relative, use archive-style separators, and remain under `manual-import/`;
- source files must exist and be non-empty;
- target paths come only from `production/fulfillment-plan.json` and the typed asset/audio/Lua slot indexes;
- target paths must pass the existing Unity archive path-safety contract and remain under the archive root;
- source and target extensions must match;
- identical existing bytes are reported as `AlreadyImported`;
- different existing bytes are a `Conflict` unless `OverwriteExisting` is explicitly enabled;
- copied content records stable byte length and lowercase SHA-256.

## Outputs and refresh

The service writes deterministic UTF-8-without-BOM reports:

```text
production/manual-provider-import-report.json
production/manual-provider-import-report.md
```

Reports contain no timestamps or absolute paths and use stable slot/path ordering. After import, the existing fulfillment scanner rewrites its five production indexes. The existing review, history, and comparison services then refresh their reports without provider or generator execution.

## Archive Review UX

The existing `Unity Archive Review` page remains read-only. It now shows:

- selected history snapshot status, sequence, relative path, and JSON;
- manual import report Markdown;
- manual import report JSON.

Missing or invalid selected snapshots remain a stable presenter state and do not throw. The parameterless page constructor remains Designer-safe.

## Verification

- focused ManualProviderImport/UnityArchiveReview tests: 48/48 passed;
- `unity-archive-manual-provider-import` product smoke: 1/1 passed;
- all ProductSmoke tests: 25/25 passed.
- `check-devflow-state.ps1`: passed in `STOP_REVIEW` mode;
- `check-all.ps1`: 630/630 tests passed, build 0 warnings / 0 errors.
