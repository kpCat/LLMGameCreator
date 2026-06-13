# Generator Plan Draft Artifact Production Pipeline

Status: v1 application layer  
Scope: deterministic draft artifact JSON production after the Draft Artifact Queue, validation, Markdown reporting, Design DB artifact persistence, and saved produced artifact reading.

## Purpose

The Draft Artifact Production Pipeline is the first layer that turns queued generator-plan work into real draft JSON payloads. It consumes a `GeneratorPlanDraftArtifactQueueResult`, creates one produced draft artifact per queue item, validates the produced JSON, renders a Markdown production report, and can persist the batch plus individual artifacts through existing generated artifact storage.

This layer is deterministic and template-based. It is intentionally provider-gated through an interface, but the v1 implementation does not call any provider.

## Relation To Draft Artifact Queue Pipeline

```text
GeneratorPlanPreview
  -> GeneratorPlanDraftExecutionPlan
  -> GeneratorPlanDraftArtifactQueue
  -> GeneratorPlanDraftArtifactProductionService
  -> optional GeneratorPlanDraftArtifactProductionArtifactService
  -> generated_artifacts / validation_results
```

The queue layer owns production tickets, artifact ids, expected contracts, gate tickets, and repair request drafts. The production layer consumes those tickets and writes actual draft artifact JSON content without mutating any GamePackage.

## Services

`IGeneratorPlanDraftArtifactProducer` is the provider-gated interface for producing one draft artifact from one queue item. v1 ships only a deterministic implementation.

`DeterministicGeneratorPlanDraftArtifactProducer` creates valid JSON payloads with common envelope fields and contract-shaped draft fields for known artifact kinds.

`GeneratorPlanDraftArtifactProductionValidator` checks batch identity, artifact identity, duplicate ids, valid JSON, envelope fields, artifact id mismatches, and blocked artifact repair references.

`GeneratorPlanDraftArtifactProductionPolicy` maps summary counts to validation states and converts only warning/error diagnostics into `GeneratedArtifactValidationResultRecord` rows.

`GeneratorPlanDraftArtifactProductionMarkdownRenderer` renders status, summary counts, produced artifact rows, diagnostics, and compact JSON previews.

`GeneratorPlanDraftArtifactProductionService` is the in-memory facade. It can produce from an existing queue result or create the queue from an example path through the existing queue service.

`GeneratorPlanDraftArtifactProductionArtifactService` saves the batch snapshot, optional Markdown report, and each produced artifact as generated artifacts. It also saves validation results for the batch and individual produced artifacts.

`GeneratorPlanDraftArtifactProductionArtifactReader` reads the latest batch snapshot, optional Markdown report, produced artifacts referenced by the batch, validation results, and a compact approval/repair worklist.

## Flow

```text
example path or queue result
  -> queue result
  -> deterministic producer per queue item
  -> production validator
  -> optional Markdown report
  -> optional generated artifact persistence
  -> optional latest artifact reader / worklist
```

## Producer Interface

```csharp
Task<GeneratorPlanProducedDraftArtifact> ProduceAsync(
    GeneratorPlanDraftArtifactQueueItem queueItem,
    GeneratorPlanDraftArtifactProductionRequest request,
    CancellationToken cancellationToken = default);
```

The interface allows a future provider-backed implementation, but v1 does not include one. Blocked queue items remain blocked in the production service. When blocked production is disabled, the service writes a minimal blocked envelope with a repair request id instead of asking the producer for meaningful content.

## Deterministic Producer

Every produced JSON payload includes:

```json
{
  "schema_version": "0.1",
  "artifact_id": "...",
  "artifact_kind": "...",
  "expected_artifact_contract": "...",
  "title": "...",
  "purpose": "...",
  "source": {
    "queue_item_id": "...",
    "execution_step_id": "..."
  },
  "draft": true
}
```

Supported artifact kinds:

```text
game_profile_v1     -> game { title, genre, camera, core_loop }, pillars[]
semantic_pack_v1   -> semantic_groups[]
mechanics_pack_v1  -> mechanics[]
scene_pack_v1      -> scenes[]
entity_pack_v1     -> entities[]
quest_pack_v1      -> quests[]
```

Unknown artifact kinds receive a generic draft envelope with `draft_sections[]`.

## Produced Artifact States

```text
draft
ready_for_approval
blocked
```

Ready queue items become `ready_for_approval`. Blocked queue items become `blocked` even when deterministic content is produced.

## Artifact IDs And Kinds

```text
artifact/generator_plan_draft_artifact_production/latest
  kind: generator_plan.draft_artifact_production

artifact/generator_plan_draft_artifact_production_markdown/latest
  kind: generator_plan.draft_artifact_production_markdown_report
```

Each produced artifact is also saved under its own `ArtifactId`, with its `ArtifactKind`, generated by `generator_plan_draft_artifact_producer`, and a path shaped as:

```text
.llmgc/generated-artifacts/{normalized artifact id}.json
```

Produced artifact metadata includes batch id, queue item id, source execution step id, expected artifact contract, approval requirement, repair request id, and produced state.

## Validation States

```text
valid     => 0 errors, 0 warnings
warnings  => 0 errors, >0 warnings
invalid   => >0 errors
```

Info diagnostics remain in the batch and Markdown report but are not saved into `validation_results`.

## Diagnostics

```text
generator_plan_draft_artifact_production.missing_batch_id
generator_plan_draft_artifact_production.no_artifacts
generator_plan_draft_artifact_production.duplicate_artifact_id
generator_plan_draft_artifact_production.artifact_missing_queue_item_id
generator_plan_draft_artifact_production.artifact_missing_artifact_id
generator_plan_draft_artifact_production.artifact_missing_artifact_kind
generator_plan_draft_artifact_production.artifact_missing_expected_artifact_contract
generator_plan_draft_artifact_production.artifact_invalid_json
generator_plan_draft_artifact_production.artifact_missing_schema_version
generator_plan_draft_artifact_production.artifact_content_missing_artifact_id
generator_plan_draft_artifact_production.artifact_content_artifact_id_mismatch
generator_plan_draft_artifact_production.blocked_artifact_missing_repair_request
generator_plan_draft_artifact_production.queue_invalid
generator_plan_draft_artifact_production.queue_diagnostic
```

## Persistence Model

The pipeline uses existing `generated_artifacts` and `validation_results` records through `IGeneratedArtifactRepository`. It does not require a new table or schema version.

Saving is idempotent for stable artifact ids: generated artifacts are upserted, and validation results are replaced for each artifact id.

## Reader And Worklist

The reader loads:

```text
latest batch snapshot
optional Markdown report
batch validation results
produced artifacts referenced by the batch
produced artifact validation results
approval / repair worklist
```

The worklist exposes artifact id, kind, state, human approval requirement, and repair request id so a future Approval/Staging layer can decide what to approve or repair.

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

This layer also does not generate Lua, call local or remote models, apply patches, change the Design DB schema, or change the GamePackage format.

## Future Next Step

The next intended layer is Approval/Staging v1: review produced artifacts, approve or request repair, and stage approved draft artifacts for a later explicit apply pipeline.
