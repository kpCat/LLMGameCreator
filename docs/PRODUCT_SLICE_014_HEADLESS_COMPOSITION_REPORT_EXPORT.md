# Product Slice 014: Headless Composition Report Export

## Goal

Persist/export `GameCompositionDiagnosticsReport` as deterministic project artifacts.

Slice 013 produced a deterministic readiness report and markdown renderer. Slice 014 should store those reports under the project `.llmgc` folder so a future read-only Composition Workbench UI can consume stable files instead of inventing its own report format.

## Output

Preferred path:

```text
.llmgc/composition-diagnostics/
```

Suggested files:

```text
.llmgc/composition-diagnostics/<blueprint-id>.composition-report.md
.llmgc/composition-diagnostics/index.json
```

Optional JSON report export is allowed only if it stays small and deterministic.

## Non-goals

No UI, no Runtime changes, no package schema changes, no generator execution, no plugins, no semantic world model, no imported maps, no lazy worlds, no procedural quest engine.
