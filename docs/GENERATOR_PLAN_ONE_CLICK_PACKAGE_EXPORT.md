# Generator Plan One-click Package Export

Status: v1 application orchestration layer
Scope: run the existing draft artifact approval and GamePackage assembly pipelines from one `.example.json` path, export `package.json`, save final run artifacts, and return a compact report.

## Purpose

The one-click package export service is a finalizer over the existing generator plan pipeline. It is meant for the first "called one service and got a package folder" workflow.

It does not replace preview, production, approval, or assembly. It coordinates them and records the final run result.

## Relation To Previous Pipelines

```text
.example.json
  -> GeneratorPlanDraftArtifactApprovalArtifactService
  -> approved artifact set
  -> GeneratorPlanGamePackageAssemblyService
  -> package.json export
  -> GeneratorPlanGamePackageAssemblyArtifactService
  -> GeneratorPlanPackageExportRunArtifactService
```

Approval still owns staging decisions and approved artifact set persistence. Assembly still owns deterministic `GamePackageDefinition` creation, validation, serialization, and export through `IGamePackageRepository`.

## One-click Flow

`GeneratorPlanPackageExportRunService.RunAsync` validates `SourceExamplePath` and `ExportFolderPath`, captures approval artifacts with auto-approval enabled by default, assembles from the latest approved artifact set, exports `package.json`, saves assembly artifacts, verifies the exported file exists, aggregates diagnostics, renders Markdown when requested, and optionally saves final run artifacts.

## Request And Result

The external request is intentionally small:

```text
SourceExamplePath
ExportFolderPath
AutoApproveValidArtifacts
RenderMarkdown
SaveArtifacts
```

The result reports status, source/export paths, `package.json` path, approval artifact result, assembly result, optional assembly artifact result, diagnostics, and optional Markdown.

Statuses are:

```text
succeeded
succeeded_with_warnings
failed
```

## Generated Artifacts

Final run artifacts use existing generated artifact storage:

```text
artifact/generator_plan_package_export_run/latest
  kind: generator_plan.package_export_run

artifact/generator_plan_package_export_run_markdown/latest
  kind: generator_plan.package_export_run_markdown_report
```

The final artifact metadata includes source example path, export folder path, package JSON path, run status, approval status, assembly status, package id, title, error count, and warning count.

Validation results contain warning/error run diagnostics only and are replaced on repeated saves for the same latest artifact id.

## Exported Files

The only filesystem export in this slice is:

```text
<ExportFolderPath>/package.json
```

The export is performed by the existing assembly service through `IGamePackageRepository`.

## Diagnostics

Run diagnostics wrap validation failures and upstream approval/assembly diagnostics:

```text
generator_plan_package_export_run.missing_source_example_path
generator_plan_package_export_run.missing_export_folder_path
generator_plan_package_export_run.source_example_not_found
generator_plan_package_export_run.approval_failed
generator_plan_package_export_run.assembly_failed
generator_plan_package_export_run.package_json_missing_after_export
generator_plan_package_export_run.approval_diagnostic
generator_plan_package_export_run.assembly_diagnostic
```

## Non-goals

```text
No Lua execution.
No LLM calls.
No provider calls.
No Unity export.
No runtime.db/save.db.
No generated code execution.
No UI in this slice.
```

This layer also does not change the GamePackage format, add a DB schema, or rewrite existing pipeline semantics.

## Next Step

A thin UI button or runtime preview loader can call this application service and open the exported package folder.
