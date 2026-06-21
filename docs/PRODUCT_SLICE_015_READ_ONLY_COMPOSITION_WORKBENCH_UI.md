# Product Slice 015: Read-only Composition Workbench UI

## Goal

Add a read-only WinForms Composition Workbench page that consumes the existing composition stack:

```text
GameBlueprintPresetProvider
GameCompositionDiagnosticsService
GameCompositionDiagnosticsMarkdownRenderer
GameCompositionDiagnosticsExportService
.llmgc/composition-diagnostics/index.json
```

The page should show composition readiness and saved reports without executing generators, plugins, Runtime, Lua, or providers.

## UI intent

Simple read-only page:

```text
left/top: blueprint presets and saved reports
summary: readiness and recommended actions
main: read-only markdown report preview
buttons: Refresh reports, Build preview report, Export report
```

## Non-goals

No blueprint editor, no custom blueprint builder, no generator execution, no plugins, no semantic model, no imported maps, no lazy world, no procedural quests, no Runtime/package schema changes.
