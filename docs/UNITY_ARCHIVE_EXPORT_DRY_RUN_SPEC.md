# Unity Archive Export Dry Run Spec

## Suggested service

```text
UnityArchiveExportDryRunService
```

## Suggested models

```text
UnityArchiveExportDryRunRequest
UnityArchiveExportDryRunResult
UnityArchiveExportPlan
UnityArchivePlannedFile
UnityArchiveExportDiagnostic
UnityArchiveExportReadiness
UnityArchiveExportPlanMarkdownRenderer
```

## Inputs

```text
project root path
GameDesignBrief
UnityTargetProfile
UnityGameArchiveManifest
available runtime module contracts
optional composition diagnostics report
```

## Output readiness

```text
ExportableNow
ExportableWithWarnings
BlockedByFutureModules
MissingRequirements
Invalid
```

## Planned files

The plan should include stable logical files, for example:

```text
manifest/unity-game-archive.json
composition/game-design-brief.json
composition/composition-report.md
data/game-package.json
ui/layouts/<layout-id>.json
assets/asset-requests.json
audio/audio-requests.json
localization/<language>.json
lua/modules-index.json
```

Files may be listed as planned even if not physically materialized yet, but diagnostics must explain missing/future parts.

## Validation

The service should call or consume Unity target validator, detect future runtime modules, detect missing required module contracts, validate planned output paths stay under output directory, write deterministic UTF-8 files, and not execute Unity, providers, generators, Runtime or Lua.
