# Generator Plan Draft Artifact Approval Pipeline

Status: v1 application layer  
Scope: review produced draft artifacts, record approve/reject/repair decisions, build a staging snapshot, persist the approved artifact set, and expose a reader worklist for the next package assembly layer.

## Purpose

The Draft Artifact Approval/Staging pipeline turns produced draft JSON artifacts into a reviewable staging result. It does not assemble or mutate a GamePackage. Its output is an approved artifact set that a later GamePackage Assembly v1 layer can consume.

## Relation To Draft Artifact Production Pipeline

```text
GeneratorPlanPreview
  -> GeneratorPlanDraftExecutionPlan
  -> GeneratorPlanDraftArtifactQueue
  -> GeneratorPlanDraftArtifactProductionBatch
  -> GeneratorPlanDraftArtifactApprovalService
  -> optional GeneratorPlanDraftArtifactApprovalArtifactService
  -> generated_artifacts / validation_results
```

Production owns candidate JSON content. Approval owns decisions, staging state, approval validation, Markdown reporting, and approved artifact set persistence.

## Services

`GeneratorPlanDraftArtifactApprovalService` is the in-memory facade. It creates a staging snapshot from an existing production result or from an example path through the existing production service.

`GeneratorPlanDraftArtifactApprovalValidator` validates snapshot identity, item identity, duplicate artifacts, JSON shape, approved item contracts, rejected/repair reasons, and blocked repair references.

`GeneratorPlanDraftArtifactApprovalPolicy` builds summary counts, maps staging status, maps validation state, and converts warning/error diagnostics to deterministic `GeneratedArtifactValidationResultRecord` rows.

`GeneratorPlanDraftArtifactApprovalMarkdownRenderer` renders status, worklist, approved set, rejected/repair items, diagnostics, and compact approved JSON previews.

`GeneratorPlanDraftArtifactApprovalArtifactService` saves the staging snapshot, optional Markdown report, approved artifact set, and validation results through `IGeneratedArtifactRepository`.

`GeneratorPlanDraftArtifactApprovalArtifactReader` reads the latest staging snapshot, optional Markdown report, approved artifact set, validation results, and compact approval worklist.

## Flow

```text
production result or example path
  -> approval decisions
  -> staging snapshot
  -> validator / policy
  -> optional Markdown report
  -> optional generated artifact persistence
  -> latest reader / worklist
```

## Decisions

Decision kinds:

```text
pending
approved
rejected
repair_requested
```

Explicit decisions override auto approval. `AutoApproveValidArtifacts` approves ready-for-approval artifacts with valid JSON. Blocked artifacts remain blocked unless an explicit repair request decision is supplied.

## Staging Statuses

```text
draft
ready_for_package
needs_review
needs_repair
invalid
```

Status rules:

```text
invalid           => any validation error
needs_repair      => any repair_requested or blocked item
needs_review      => any pending item
ready_for_package => all non-rejected items are approved and at least one item is approved
draft             => fallback state
```

## Item States

```text
pending
approved
rejected
repair_requested
blocked
```

## Approved Artifact Set

The approved artifact set is saved as JSON with schema version `0.1`, the staging snapshot id, the source production batch id, and approved artifacts. `content_json` is embedded as a JSON object or array when the approved artifact content can be parsed.

## Artifact IDs And Kinds

```text
artifact/generator_plan_draft_artifact_staging/latest
  kind: generator_plan.draft_artifact_staging

artifact/generator_plan_draft_artifact_staging_markdown/latest
  kind: generator_plan.draft_artifact_staging_markdown_report

artifact/generator_plan_approved_artifact_set/latest
  kind: generator_plan.approved_artifact_set
```

## Validation States

```text
valid     => 0 errors, 0 warnings
warnings  => 0 errors, >0 warnings
invalid   => >0 errors
```

Info diagnostics remain in the snapshot and Markdown report but are not saved into `validation_results`.

## Diagnostics

```text
generator_plan_draft_artifact_approval.missing_snapshot_id
generator_plan_draft_artifact_approval.no_items
generator_plan_draft_artifact_approval.duplicate_artifact_id
generator_plan_draft_artifact_approval.item_missing_artifact_id
generator_plan_draft_artifact_approval.item_missing_artifact_kind
generator_plan_draft_artifact_approval.item_invalid_json
generator_plan_draft_artifact_approval.approved_item_invalid_json
generator_plan_draft_artifact_approval.approved_item_missing_contract
generator_plan_draft_artifact_approval.rejected_item_missing_reason
generator_plan_draft_artifact_approval.repair_requested_missing_reason
generator_plan_draft_artifact_approval.blocked_item_without_repair_request
generator_plan_draft_artifact_approval.production_diagnostic
```

## Persistence Model

The pipeline uses existing `generated_artifacts` and `validation_results` storage. It does not require a new table or schema version. Saving is idempotent for stable artifact ids: generated artifacts are upserted, and staging validation rows are replaced for the staging artifact id.

## Reader And Worklist

The reader loads the latest staging snapshot, optional Markdown artifact, approved artifact set artifact, staging validation results, and a compact worklist containing artifact id, kind, state, approval requirement, repair request id, and reason code.

## Non-Goals

```text
No Lua execution.
No LLM calls.
No provider calls.
No GamePackage mutation in this layer.
No Unity export.
No runtime.db/save.db.
No generated code execution.
No complex UI.
```

This layer also does not apply patches, change the GamePackage format, or touch WinForms UI.

## Future Next Step

The next intended layer is GamePackage Assembly v1, which can consume the approved artifact set and build a package draft in a separate explicit pass.
