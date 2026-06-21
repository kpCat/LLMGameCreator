# Composition Workbench UI Spec

## Suggested page files

```text
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchPageControl.cs
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchPresenter.cs
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchViewModels.cs
```

Use existing page and presenter patterns. Preserve Designer-safe split.

## Minimum behavior

- List blueprint presets.
- Build diagnostics report for selected preset.
- Render markdown to a read-only text area.
- Export report using existing export service.
- Refresh/load saved report entries from `.llmgc/composition-diagnostics/index.json`.
- No LLM/provider/generator/runtime execution.
