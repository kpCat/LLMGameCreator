# Composition Report Export Spec

## Suggested types

```text
GameCompositionDiagnosticsExportService
GameCompositionDiagnosticsExportRequest
GameCompositionDiagnosticsExportResult
GameCompositionDiagnosticsExportIndex
GameCompositionDiagnosticsExportIndexEntry
```

## Required behavior

- Accept project root path and `GameCompositionDiagnosticsReport`.
- Render markdown using `GameCompositionDiagnosticsMarkdownRenderer`.
- Create `.llmgc/composition-diagnostics`.
- Write UTF-8 markdown.
- Write/update deterministic `index.json`.
- Sanitize blueprint id for file name.
- Prevent path traversal.
- Keep output under project root.
- Do not include timestamps in markdown.
- Avoid timestamps in index unless a deterministic clock is injected.

## Product smoke

Scenario:

```text
composition-report-export
```
