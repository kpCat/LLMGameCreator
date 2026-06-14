# Generator Plan Strict LLM Artifact Generation

Status: M4 implementation guide  
Scope: editor-side strict LLM draft artifact generation, validation, bounded repair, audit persistence and review staging  
Non-scope: runtime LLM calls, Lua execution, GamePackage mutation, schema changes, package export, media generation

## Purpose

M4 connects the LLM to generation as a draft-only producer of contract-bound JSON artifacts.

The workflow is:

```text
Capability Picker
  -> latest capability selection artifact
  -> LLM Artifacts page
  -> strict prompt for one artifact contract
  -> exactly one JSON object
  -> strict parser
  -> C# contract validator
  -> optional one-attempt repair
  -> pending Artifact Review staging snapshot
  -> human approve/reject/repair
```

The LLM is not a runtime authority and does not mutate a `GamePackage`. C# owns parsing, validation, storage, staging and later promotion.

## Supported First Contracts

The first strict contract set is intentionally small:

```text
game_profile_v1
scene_pack_v1
quest_pack_v1
mechanics_pack_v1
```

These contracts prove the profile/concept, scene seed, quest seed and mechanic/ability seed path. They are not full game generation.

## Exact JSON Rule

Strict responses must be exactly one JSON object:

- no Markdown fences;
- no text before or after JSON;
- no JSON array root;
- no invalid JSON;
- no code, scripts, commands, provider instructions or package mutation instructions;
- no multi-contract mixing in one artifact.

The strict parser rejects wrapped or fenced JSON instead of extracting it.

## Prompt Context

Prompts are built from the latest capability selection summary only:

- selection id, title and purpose;
- selected variant ids;
- selected feature bundle ids;
- resolved artifact contracts;
- resolved validators;
- resolved runtime targets;
- gaps and warnings;
- exact output schema for the selected contract;
- optional extra user brief.

The prompt does not include the repository, full docs, whole project state or previous generated artifacts.

## Validation

The C# validator checks:

- JSON object root;
- `schema_version`;
- `artifact_kind` equals the contract id;
- required top-level and payload fields;
- lowercase slash ids for `id` fields;
- required arrays;
- contract-specific minimum semantics;
- forbidden top-level fields such as `code`, `script`, `lua`, `csharp`, `sql`, `powershell`, `command`, `commands`, `execute` and `eval`.

Validation diagnostics are saved in the generation audit artifact.

## Repair Loop

When parsing or validation fails and repair is enabled, the service builds one targeted repair prompt by default.

The repair prompt includes:

- original contract id;
- exact output schema;
- validation diagnostics;
- invalid response content;
- instruction to return only corrected JSON.

Repair must not redesign selected variants, feature bundles, contract id, artifact kind or source context. If repair still fails, the artifact is not staged as ready for review.

## Audit Artifacts

The latest strict LLM generation audit is saved through existing generated artifact storage:

```text
id: artifact/generator_plan_strict_llm_artifact_generation/latest
kind: generator_plan.strict_llm_artifact_generation
path: .llmgc/generator-plans/generator_plan_strict_llm_artifact_generation.json
```

The audit includes generation time, status, source capability selection id, requested contracts, generated artifact metadata, attempt metadata, hashes, diagnostics and validation rows.

When `Generation.SaveEveryRequest` is true, prompt and response text are included in attempt metadata. API keys and secrets are not stored in prompts or artifacts.

M4.1 evaluation reads this latest audit artifact to compute pass rates, repair recovery rates, repeated diagnostic codes and deterministic quality warnings. The audit stores generated artifact `content_json` so evaluation can inspect title, description, tags and `source_context` without reading prompts or secrets.

## Review Staging

Valid artifacts can be staged immediately for the existing Artifact Review UI:

```text
SourceExecutionStepId: strict_llm/{contract_id}
QueueItemId: strict_llm/{contract_id}
SourceProductionBatchId: strict_llm/{batch_id}
State: pending
RequiresHumanApproval: true
```

The approved artifact set remains empty until a human approves items in Artifact Review.

## UI

The WinForms page is `LLM Artifacts`.

Typical flow:

1. Use Capability Picker and save latest selection.
2. Open LLM Artifacts.
3. Load latest capability selection.
4. Select an existing LLM profile and contracts.
5. Preview prompt for one selected contract.
6. Generate selected artifacts explicitly.
7. Open Artifact Review to approve, reject or request repair.

The page does not call the LLM on startup or activation. Prompt preview does not call the LLM.

The companion `LLM Evaluation` page can evaluate the latest audit without any LLM call or run a small explicit batch through this same strict generation service. Batch evaluation is disabled by default until the user selects batch mode, a profile and contracts.

## Boundaries

Preserved boundaries:

- LLM equals draft only;
- C# owns parser, validator, audit and staging;
- Lua is not executed;
- runtime does not call LLM;
- `GamePackage` is not mutated;
- package export is not performed;
- Design DB schema is not changed.
