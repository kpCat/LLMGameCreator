# Generator Plan Preview Pipeline

Status: v1 application layer  
Scope: read-only `.example.json` generator plan preview, validation, Markdown rendering, Design DB artifact persistence, and saved preview reading.

## Purpose

The Generator Plan Preview Pipeline sits after the Atlas Registry Pipeline. It reads one generator plan example file, projects it into a typed preview model, validates that the draft plan shape is usable, renders a Markdown report, and can persist the preview into existing Design DB generated artifact tables.

It does not execute a generator plan. It is a preview/draft understanding layer for future artifact production.

## Services

`GeneratorPlanPreviewLoader` reads one `.example.json` file through `System.Text.Json`. It preserves the source path, tolerates missing optional fields, sorts steps by order and id, and returns an invalid-json diagnostic instead of throwing for malformed JSON.

`GeneratorPlanPreviewValidator` checks required plan fields, target artifact and feature bundle lists, step identity, duplicate step ids/orders, expected artifact contracts, and validation gates. It recalculates deterministic summary counts.

`GeneratorPlanPreviewMarkdownRenderer` renders a human-readable report with status, identity fields, summary, a steps table, and diagnostics. Table cells escape pipes and newlines.

`GeneratorPlanPreviewService` coordinates loader, validator, and renderer. It returns typed preview data, Markdown, diagnostics, and validation status. It does not persist artifacts by itself.

`GeneratorPlanPreviewArtifactService` saves preview results through `IGeneratedArtifactRepository`. It writes the result artifact and, when Markdown rendering is enabled, a Markdown artifact. Validation rows are derived only from warning/error diagnostics.

`GeneratorPlanPreviewArtifactReader` loads the standard latest result artifact, optional Markdown artifact, and validation rows.

## Data Flow

```text
Atlas Registry Pipeline
  -> generator-library/atlas/examples/*.example.json
  -> GeneratorPlanPreviewLoader
  -> GeneratorPlanPreviewValidator
  -> GeneratorPlanPreviewMarkdownRenderer
  -> optional GeneratorPlanPreviewArtifactService
  -> generated_artifacts / validation_results
```

## Artifact IDs And Kinds

```text
artifact/generator_plan_preview/latest
  kind: generator_plan.preview

artifact/generator_plan_preview_markdown/latest
  kind: generator_plan.markdown_report
```

Custom artifact ids are supported by the artifact request. Re-saving the same result artifact id is idempotent because generated artifacts are upserted and validation results are replaced for that artifact id.

## Validation States

```text
valid     => 0 errors, 0 warnings
warnings  => 0 errors, >0 warnings
invalid   => >0 errors
```

Info diagnostics remain in the preview and Markdown report but are not saved into `validation_results`.

## Diagnostics

```text
generator_plan_preview.invalid_json
generator_plan_preview.missing_example_id
generator_plan_preview.missing_title
generator_plan_preview.missing_source_profile
generator_plan_preview.no_steps
generator_plan_preview.step_missing_id
generator_plan_preview.step_duplicate_id
generator_plan_preview.step_missing_expected_artifact_contract
generator_plan_preview.step_missing_validation_gates
generator_plan_preview.step_order_duplicate
generator_plan_preview.target_artifacts_empty
generator_plan_preview.selected_feature_bundles_empty
generator_plan_preview.loaded
```

## Relation To Atlas Registry Pipeline

The Atlas Registry Pipeline discovers and summarizes atlas files and examples. The Generator Plan Preview Pipeline consumes a specific example plan after that discovery layer and turns it into typed preview data suitable for review and artifact persistence.

## Non-Goals

This layer deliberately does not:

```text
execute Lua
generate Lua
call an LLM
mutate GamePackage
change the GamePackage format
export Unity data
create runtime.db or save.db
generate C# from atlas data
add UI
```

The next intended step is draft-only plan execution and artifact production, still without actual Lua or LLM execution in this layer.
