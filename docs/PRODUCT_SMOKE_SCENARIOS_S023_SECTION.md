## unity-archive-review-snapshot

Validates deterministic read-only review over an already materialized Unity archive:

```text
.llmgc/unity-archive materialization
-> read-only archive review snapshot
-> validation/provider/fulfillment/request/source-file summaries
-> deterministic archive-review.json and archive-review.md
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-review-snapshot
```

Expected assertions:

- `.llmgc/unity-archive/production/archive-review.json` is written;
- `.llmgc/unity-archive/production/archive-review.md` is written;
- review JSON has `schemaVersion` and readiness;
- review JSON summarizes validation, provider, fulfillment, request and source-file sections;
- review JSON contains no timestamps and no absolute archive root path;
- repeated review of unchanged archive is byte-identical;
- no Unity, Runtime, WinForms, GamePackage schema, provider, generator, LLM or Lua execution.

No manual UI verification is required.
