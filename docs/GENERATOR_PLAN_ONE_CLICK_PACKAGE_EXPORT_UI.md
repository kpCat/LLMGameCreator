# Generator Plan One-click Package Export UI

Status: v1 WinForms UX entrypoint

## Location

The page is available in the WinForms navigation as:

```text
Package Export
```

It is implemented by:

```text
src/LLMGameCreator.WinForms/Pages/PackageExport/PackageExportPageControl.cs
```

The page is a thin UI over the existing `GeneratorPlanPackageExportRunService` and latest-run artifact reader. It does not introduce a new backend pipeline.

## How To Use

1. Open the app.
2. Select the `Package Export` page.
3. Choose a built-in template or browse for a manual `.example.json` generator plan example.
4. For a built-in template, click `Use template` to create the `.example.json` file and fill the source path.
5. Choose an export folder, or use `Export selected` to materialize the template and run the export with a default export folder.
6. Click `Generate package` for the currently selected source example path.
7. Review status, diagnostics, and paths.
8. Open the export folder or `package.json`.
9. Use `Load latest run` to inspect the latest saved package export run.

## Built-in Templates

The page can materialize built-in example templates from the application catalog:

```text
Sky Lantern Outpost
Clockwork Orchard
Storm Glass Lighthouse
Moss Courier Trail
Underroot Signal
```

Template files are written to:

```text
<CurrentGameFolder>/.llmgc/example-templates
```

When no current game folder is loaded, the fallback location is:

```text
%LOCALAPPDATA%/LLMGameCreator/example-templates
```

`Export selected` also fills a default export folder under `.llmgc/package-exports` for the current game folder, or `%LOCALAPPDATA%/LLMGameCreator/package-exports` when no current game folder is loaded.

The manual `.example.json` browse field remains available and uses the same `Generate package` button.

## What Is Saved

The existing application pipeline saves:

```text
approval artifacts
assembly artifacts
final package export run artifact
optional markdown report artifact
validation results for warning/error diagnostics
```

The exported file is:

```text
<export folder>/package.json
```

Generated artifacts continue to use the existing generated artifact storage in `SqliteDesignDatabase`. No DB schema changes are required.

## Non-goals

```text
No LLM calls.
No provider/model calls.
No Lua execution.
No Unity export.
No runtime preview.
No GamePackage schema changes.
No DB schema changes.
```

## Troubleshooting

Missing `.example.json`:

The page shows a failed status and a diagnostic with code:

```text
generator_plan_package_export_run.source_example_not_found
```

Missing export folder path:

The page shows a failed status and a diagnostic with code:

```text
generator_plan_package_export_run.missing_export_folder_path
```

Succeeded with warnings:

The package can still be exported when the assembly pipeline reports warnings, for example for an unmapped semantic artifact. The status is:

```text
succeeded_with_warnings
```

Review the diagnostics grid and markdown report before using the exported package.
