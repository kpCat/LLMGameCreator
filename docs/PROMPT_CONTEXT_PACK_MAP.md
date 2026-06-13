# Prompt Context Pack Map

Status: seed architecture document  
Version: 0.1  
Related files:

```text
generator-library/atlas/prompt_context_pack_map.json
generator-library/atlas/model_workflow_roles_and_prompts.json
generator-library/atlas/artifact_contracts.json
generator-library/atlas/validation_pipeline.json
generator-library/atlas/capability_atlas.json
```

## Purpose

Prompt Context Pack Map defines how LLMGameCreator should assemble compact, relevant prompt context for local or remote models.

The system must not solve context limits by sending the whole project, whole repository, whole lore, all generated artifacts or full old chat history to every request.

Instead, every model call should be built from a traceable prompt context pack:

```text
role + task + artifact contract + schema + enums + selected project context + output rules
```

## Core rule

```text
Select context by contract, capability, profile, tags and validation target.
Do not dump the project.
```

## Why this matters

LLMGameCreator is intended to use different model roles:

```text
large creative model -> design/lore/profile discussion
small fast model     -> strict batch JSON artifacts
repair model         -> targeted correction of validation failures
scorer/reviewer      -> sampled quality review
```

These roles need different context. A lore discussion may need broader summaries; a strict artifact generator needs exact schema, enum preservation rules, one task input and very little extra prose.

## Budget classes

The seed map defines four budget classes:

```text
tiny   -> 1k-4k, targeted repair or one small JSON artifact
small  -> 4k-12k, one contract-bound generation task
medium -> 12k-32k, batch/semantic expansion or profile-scoped consistency
large  -> 32k-64k, high-level design discussion or canon consolidation
```

These are planning budgets, not hard runtime guarantees.

## Context source types

Typical sources:

```text
game_profile
artifact_contract
output_schema
canonical_enums
design_knowledge_summary
tagged_snippets
examples
validation_report
previous_outputs_summary
content_overlay_rules
```

Required sources depend on the prompt template. For strict JSON generation, artifact contract, output schema and canonical enums are more important than lore.

## Pack templates

The seed map defines initial pack templates:

```text
design_discussion
strict_single_json_artifact
batch_generation_round_robin
targeted_repair
lua_module_proposal
quality_sample_review
```

Each template defines intended model roles, budget, required sources, optional sources, must-include sections, must-not-include sections and expected output contracts.

## Selection pipeline

The standard context selection pipeline is:

```text
identify task
  -> collect candidates
  -> filter by permissions
  -> rank by relevance
  -> budget
  -> render prompt pack
  -> archive trace
```

Permission filtering is important. Disabled content overlays, unapproved canon, stale drafts and unrelated runtime targets must not leak into prompts.

## Adult/NSFW overlay handling

Adult/NSFW context is not a global default. It is included only when the active game profile explicitly enables the corresponding content overlay.

When enabled, it remains:

```text
tagged
filterable
export-aware
separate from core mechanics
```

This makes it possible to build games where adult content is central, optional, disabled or export-target dependent without corrupting the core gameplay model.

## Repair prompts

Repair prompts must be narrow. They should include:

```text
original input
invalid output
validation report
exact contract/schema
enum/id rules
```

They should not invite the model to redesign unrelated systems.

## Anti-patterns

Avoid:

```text
- dumping the entire repository into every prompt;
- treating previous model output as trusted fact;
- mixing several artifact contracts in one strict JSON request;
- giving broad creative freedom while expecting enum-perfect JSON;
- including disabled overlay context;
- letting context grow linearly with every generated item.
```

## Future implementation notes

A future C# implementation can store prompt context traces in the Design DB. A trace does not need to store full prompt text forever; it should at least store role, pack template, source ids, included/excluded source list, budget class, estimated token count, model tier and validation result.
