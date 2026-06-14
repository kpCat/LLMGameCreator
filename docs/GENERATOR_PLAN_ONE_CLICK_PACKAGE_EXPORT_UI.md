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
3. Browse for a `.example.json` generator plan example.
4. Choose an export folder.
5. Click `Generate package`.
6. Review status, diagnostics, and paths.
7. Open the export folder or `package.json`.
8. Use `Load latest run` to inspect the latest saved package export run.

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
