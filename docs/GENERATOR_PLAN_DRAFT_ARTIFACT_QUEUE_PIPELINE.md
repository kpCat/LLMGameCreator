# Generator Plan Draft Artifact Queue Pipeline

Status: v1 application layer  
Scope: draft-only artifact production queue after Generator Plan Draft Execution, validation, Markdown rendering, Design DB artifact persistence, and saved queue reading.

## Purpose

The Draft Artifact Production Queue turns a `GeneratorPlanDraftExecutionPlan` into a reviewable worklist for future artifact production. It creates per-step production tickets, validation gate tickets, repair request drafts for blocked items, diagnostics, summary counts, and an optional Markdown report.

This layer does not produce artifact content. It only stages the tickets needed by a future provider-gated production layer.

## Relation To Draft Execution Pipeline

```text
GeneratorPlanPreview
  -> GeneratorPlanDraftExecutionPlan
  -> GeneratorPlanDraftArtifactQueueBuilder
  -> GeneratorPlanDraftArtifactQueueValidator
  -> GeneratorPlanDraftArtifactQueueMarkdownRenderer
  -> optional GeneratorPlanDraftArtifactQueueArtifactService
  -> generated_artifacts / validation_results
```

Draft execution owns planned step ids, planned artifact ids, expected contracts, validation gate names, and repair request ids. The queue layer mirrors those fields into production tickets and validation gate tickets without executing any gate or producer.

## Services

`GeneratorPlanDraftArtifactQueueBuilder` creates deterministic queue ids, item ids, validation gate ticket ids, item states, optional repair request drafts, mapped execution diagnostics, and summary counts.

`GeneratorPlanDraftArtifactQueueValidator` checks queue contracts such as queue id, item presence, duplicate item ids, duplicate artifact ids, required artifact ids, artifact kind warnings, expected contract warnings, validation gate warnings, and repair request reason/message warnings.

`GeneratorPlanDraftArtifactQueuePolicy` maps summary counts to validation states and converts only warning/error diagnostics to `GeneratedArtifactValidationResultRecord` rows.

`GeneratorPlanDraftArtifactQueueMarkdownRenderer` renders a report with queue identity, summary fields, queue items, validation gates, repair requests, and diagnostics. Table cells escape pipes and newlines.

`GeneratorPlanDraftArtifactQueueService` is the in-memory application facade. It can create a queue from an existing `GeneratorPlanDraftExecutionResult` or from an example path through the existing draft execution service.

`GeneratorPlanDraftArtifactQueueArtifactService` saves queue results through `IGeneratedArtifactRepository`. It writes the result artifact and, when Markdown rendering is enabled, a Markdown report artifact. Validation rows are derived only from warning/error diagnostics.

`GeneratorPlanDraftArtifactQueueArtifactReader` loads the standard latest result artifact, optional Markdown artifact, and validation rows.

## Flow

```text
example path or draft execution result
  -> draft execution result
  -> queue builder
  -> queue validator
  -> optional Markdown report
  -> optional generated artifact persistence
  -> optional latest artifact reader
```

## Queue Statuses

```text
draft    => queue exists but has no production items yet
ready    => all items are ready and validation has no errors
blocked  => at least one item is blocked before production
invalid  => queue validation errors or invalid draft execution input exist
```

## Item States

```text
pending  => reserved for future queued-but-not-ready states
ready    => source step is ready and has artifact id, kind, expected contract, and gates
blocked  => source step is blocked or required production ticket data is missing
```

## Gate States

```text
pending  => validation gate ticket exists but has not run
blocked  => parent production item is blocked
```

Gate tickets are planning records only. The queue does not execute validation gates.

## Repair Request States

```text
draft    => repair request was created for a blocked item
resolved => reserved for future repair workflows
```

Repair requests are draft records only and do not mutate plans or artifacts.

## Artifact IDs And Kinds

```text
artifact/generator_plan_draft_artifact_queue/latest
  kind: generator_plan.draft_artifact_queue

artifact/generator_plan_draft_artifact_queue_markdown/latest
  kind: generator_plan.draft_artifact_queue_markdown_report
```

Queue ids are deterministic by default:

```text
draft_artifact_queue/{draftExecutionPlanId}
```

Queue item ids are deterministic:

```text
{queueId}/item/{stepOrder}
```

Validation gate ticket ids are deterministic:

```text
{itemId}/gate/{normalizedGateId}
```

Custom artifact ids are supported by the artifact request. Re-saving the same result artifact id is idempotent because generated artifacts are upserted and validation results are replaced for that artifact id.

## Validation States

```text
valid     => 0 errors, 0 warnings
warnings  => 0 errors, >0 warnings
invalid   => >0 errors
```

Info diagnostics remain in the queue and Markdown report but are not saved into `validation_results`.

## Diagnostics

```text
generator_plan_draft_artifact_queue.missing_queue_id
generator_plan_draft_artifact_queue.no_items
generator_plan_draft_artifact_queue.duplicate_item_id
generator_plan_draft_artifact_queue.duplicate_artifact_id
generator_plan_draft_artifact_queue.item_missing_source_execution_step_id
generator_plan_draft_artifact_queue.item_missing_artifact_id
generator_plan_draft_artifact_queue.item_missing_artifact_kind
generator_plan_draft_artifact_queue.item_missing_expected_artifact_contract
generator_plan_draft_artifact_queue.item_missing_validation_gates
generator_plan_draft_artifact_queue.gate_missing_id
generator_plan_draft_artifact_queue.repair_request_missing_reason
generator_plan_draft_artifact_queue.execution_diagnostic
```

Draft execution diagnostics are mirrored into queue diagnostics so invalid or warning state remains visible after projection.

## Non-Goals

```text
No Lua execution.
No LLM calls.
No provider calls.
No GamePackage mutation.
No Unity export.
No runtime.db/save.db.
No generated code execution.
No complex UI.
```

This layer also does not generate Lua, call local or remote models, apply patches, produce real artifact content, change the Design DB schema, or change the GamePackage format.

## Future Next Step

The next layer can add provider-gated draft artifact generation adapters that consume queue items and produce candidate artifacts. That future stage should still be reviewable and should not enable Lua execution, LLM calls, provider calls, or GamePackage mutation by default.
