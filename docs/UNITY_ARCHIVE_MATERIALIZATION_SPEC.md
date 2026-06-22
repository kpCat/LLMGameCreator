# Unity Archive Materialization v1 Spec

## Suggested service

```text
UnityArchiveMaterializationService
```

## Suggested models

```text
UnityArchiveMaterializationRequest
UnityArchiveMaterializationResult
UnityArchiveMaterializedFile
UnityArchiveMaterializationReadiness
UnityArchiveMaterializationDiagnostic
```

## Inputs

```text
project root path
GameDesignBrief
UnityTargetProfile
UnityGameArchiveManifest
runtime module contracts
dry-run plan/result
optional composition report markdown
createZip flag
```

## Required behavior

- Run/consume Slice 017 dry-run validation.
- Refuse materialization when dry-run readiness is Invalid or MissingRequirements.
- Allow materialization with warnings when readiness is ExportableWithWarnings.
- Allow future-blocked materialization only as metadata-preview, not as playable archive.
- Write deterministic UTF-8 files under `.llmgc/unity-archive`.
- Optionally write deterministic zip with stable entry order.
- Never write outside project root.
- No Unity, Runtime, provider, generator or Lua execution.

## Important distinction

The materialized archive is still not a playable Unity game. It is the first concrete archive contract instance that the future Unity player will load.
