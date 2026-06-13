# Generator Plan Draft Execution Pipeline

Status: v1 application layer  
Scope: draft-only execution planning after Generator Plan Preview, validation, Markdown rendering, Design DB artifact persistence, and saved draft reading.

## Purpose

The Generator Plan Draft Execution Pipeline turns a validated `GeneratorPlanPreview` into a staged execution plan. It describes the future queue of steps, step states, expected artifact contracts, validation gates, planned artifact ids, and repair request ids.

This is not generator execution. It is a dry-run planning layer that makes the next execution stage reviewable before any provider, Lua runtime, GamePackage mutation, or patch application exists.

## Relation To Generator Plan Preview Pipeline

```text
GeneratorPlanPreview
  -> GeneratorPlanDraftExecutionPlanner
  -> GeneratorPlanDraftExecutionValidator
  -> GeneratorPlanDraftExecutionMarkdownRenderer
  -> optional GeneratorPlanDraftExecutionArtifactService
  -> generated_artifacts / validation_results
```

The preview pipeline still owns `.example.json` loading and preview validation. Draft execution consumes preview output and projects each preview step into a future execution step.

## Services

`GeneratorPlanDraftExecutionPlanner` creates deterministic plan ids, step ids, planned artifact ids, repair request ids, step states, and summary counts.

`GeneratorPlanDraftExecutionValidator` checks draft execution plan contracts such as plan id, step presence, duplicate step ids, duplicate planned artifact ids, missing source preview step ids, missing expected artifact contracts, missing validation gates, and missing producer roles.

`GeneratorPlanDraftExecutionMarkdownRenderer` renders a reviewable report with plan identity, summary fields, a step queue table, and diagnostics. Table cells escape pipes and newlines.

`GeneratorPlanDraftExecutionService` is the in-memory application facade. It can create a draft from an existing `GeneratorPlanPreviewResult` or from an example path through the existing preview service.

`GeneratorPlanDraftExecutionArtifactService` saves the draft result through `IGeneratedArtifactRepository`. It writes the result artifact and, when Markdown rendering is enabled, a Markdown report artifact. Validation rows are derived only from warning/error diagnostics.

`GeneratorPlanDraftExecutionArtifactReader` loads the standard latest result artifact, optional Markdown artifact, and validation rows.

## Draft Statuses

```text
draft    => plan exists but is not ready or blocked
ready    => no preview errors and no blocked steps
blocked  => at least one step is missing required execution planning data
invalid  => preview errors or draft validation errors exist
```

## Step States

```text
pending  => reserved for future queued-but-not-ready states
ready    => expected artifact contract and validation gates are present
blocked  => expected artifact contract or validation gates are missing
skipped  => reserved for future policy-controlled skip decisions
```

## Artifact IDs And Kinds

```text
artifact/generator_plan_draft_execution/latest
  kind: generator_plan.draft_execution

artifact/generator_plan_draft_execution_markdown/latest
  kind: generator_plan.draft_execution_markdown_report
```

Planned future step artifacts use deterministic ids:

```text
artifact/draft_execution/{planId}/step/{stepOrder}/{expectedArtifactContract}
```

Repair request drafts use deterministic ids:

```text
repair/draft_execution/{planId}/step/{stepOrder}
```

Custom artifact ids are supported by the artifact request. Re-saving the same result artifact id is idempotent because generated artifacts are upserted and validation results are replaced for that artifact id.

## Validation States

```text
valid     => 0 errors, 0 warnings
warnings  => 0 errors, >0 warnings
invalid   => >0 errors
```

Info diagnostics remain in the draft result and Markdown report but are not saved into `validation_results`.

## Diagnostics

```text
generator_plan_draft_execution.missing_plan_id
generator_plan_draft_execution.no_steps
generator_plan_draft_execution.duplicate_step_id
generator_plan_draft_execution.duplicate_planned_artifact_id
generator_plan_draft_execution.step_missing_source_preview_step_id
generator_plan_draft_execution.step_missing_expected_artifact_contract
generator_plan_draft_execution.step_missing_validation_gates
generator_plan_draft_execution.step_missing_producer_role
generator_plan_draft_execution.preview_diagnostic
```

Preview diagnostics are mapped into draft diagnostics so invalid preview state remains visible after projection.

## Non-Goals

```text
No Lua execution.
No LLM calls.
No GamePackage mutation.
No Unity export.
No runtime.db/save.db.
No generated code execution.
No complex UI.
```

This layer also does not apply patches, call local or remote models, generate Lua, generate C# from atlas data, or change the GamePackage format.

## Future Next Step

The next layer can add real draft execution adapters that consume the staged plan and produce draft artifacts. That future stage should still remain provider-gated and reviewable before any Lua execution, LLM calls, or GamePackage mutation are introduced.
