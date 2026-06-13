# Model Workflow Roles and Prompt Families

Status: seed architecture document  
Version: 0.1  
Related files:

```text
generator-library/atlas/model_workflow_roles_and_prompts.json
generator-library/atlas/capability_atlas.json
generator-library/atlas/artifact_contracts.json
generator-library/atlas/validation_pipeline.json
generator-library/atlas/library_growth_pipeline.json
```

## Purpose

This document defines how LLMGameCreator should use different model roles without turning model output into trusted runtime state.

The goal is to support a practical two-model workflow:

```text
larger flexible model -> design, lore, profile discussion, semantic planning
smaller fast model    -> contract-bound batch JSON generation and repair
C# validators/tools   -> authoritative parsing, validation, promotion and export gates
```

The model is never the final authority for runtime state, Unity export, package mutation, canonical IDs, enabled content overlays or promotion decisions.

## Core rule

```text
Model role -> prompt family -> selected context pack -> artifact contract -> validation -> promotion
```

A prompt should not be a vague request like "generate game data". It should name:

```text
- the model role;
- the artifact contract;
- the allowed input context;
- strict output shape;
- exact ids/enums that must be copied;
- enabled/disabled content overlays;
- validation gates;
- repair policy.
```

## Model tiers

### Designer large model

Example target: `Gemma 4 26B A4B Instruct Ultra Uncensored Heretic`.

Use it for:

```text
- lore and world concept discussion;
- factions, NPCs, tone, politics and themes;
- game profile selection;
- complex semantic design;
- adult/NSFW overlay planning when explicitly enabled;
- library growth proposals and high-level specs.
```

Do not use it as an unrestricted runtime writer. Its outputs are still drafts.

### Batch small/fast model

Example target: `Gemma 4 E4B Instruct Ultra Uncensored Heretic`.

Use it for:

```text
- many small JSON artifacts;
- semantic records;
- material/item/location/NPC variants;
- text pack records;
- dialogue variants;
- asset/audio/future animation request records;
- targeted repair attempts.
```

Use strict JSON prompts, selected context and enum/id preservation rules.

### Validator/scorer model

Optional. Use only for non-authoritative quality notes, sample review, style drift and semantic plausibility hints.

Formal pass/fail decisions remain deterministic C# or tooling decisions.

## Workflow roles

### Designer LLM

Produces game profiles, feature bundle drafts, semantic design notes and library growth proposals.

Requires human approval when it affects:

```text
- canon;
- runtime target;
- major mechanics;
- content overlays;
- artifact contracts;
- new library capabilities.
```

### Batch Generator LLM

Produces one contract-bound JSON artifact or a batch of independent records.

It must not:

```text
- rename ids;
- translate enum values;
- output markdown fences;
- invent approved canon;
- mix unrelated contracts;
- silently enable NSFW/adult content.
```

### Repair LLM

Repairs a failed artifact using a validation report.

The repair prompt must specify which fields may change and which fields are immutable.

### Context Selector

Context selection should be deterministic first:

```text
task -> artifact contract -> capability ids -> profile -> enabled overlays -> relevant snippets
```

Optional LLM ranking may be used only after deterministic shortlist selection.

### Validator Service

This is not an LLM role. It is deterministic C# or tooling.

It owns:

```text
- JSON parsing;
- schema checks;
- id/enum/reference validation;
- artifact contract validation;
- safety checks;
- export/dry-run gates;
- promotion decisions.
```

### Quality Scorer LLM

May score style, repetition and plausibility, but cannot override deterministic validation.

## Prompt families

### Design Discussion Prompt

Flexible enough for creative discussion, but bounded by profile, capability atlas and enabled overlays.

Good for:

```text
- game concept;
- lore;
- factions;
- reference profile selection;
- optional adult/NSFW layer planning;
- high-level mechanics.
```

### Strict Single JSON Artifact Prompt

Used for batch generation.

Mandatory rules:

```text
Return exactly one JSON object.
Do not wrap the response in Markdown/code fences.
Do not add explanations before or after JSON.
Do not translate machine-readable ids/enums.
Copy exact fields when requested.
Use proposed_new_facts only for optional suggestions.
Respect enabled/disabled content overlays.
```

### Lua Module Proposal Prompt

Used only after a capability gap or proposal exists.

A Lua module prompt must include:

```text
- module id;
- manifest fields;
- input_schema;
- output_schema;
- config_schema;
- artifact contract produced;
- diagnostics format;
- examples;
- unsafe feature ban.
```

The model must not use:

```text
io
os
debug
package
load
loadfile
dofile
network
filesystem
external dependencies
global writes
```

### Targeted Repair Prompt

Used when validation returns `repairable_fail`.

It must include:

```text
- failed artifact;
- validation errors;
- original immutable input;
- fields allowed to change;
- fields forbidden to change;
- target contract.
```

### Quality Sample Review Prompt

Used for sample review only. It outputs a report, not patched artifacts.

## Adult/NSFW content overlay policy

Adult/NSFW is not a genre. It is an optional content overlay that may apply to many profiles.

When disabled:

```text
- prompts must explicitly say it is disabled;
- generation must not produce adult/NSFW text, image, audio or animation requests.
```

When enabled:

```text
- generated records must be tagged;
- records must remain filterable;
- export/platform filters must be respected;
- core mechanics must not silently depend on adult content;
- asset/audio/animation requests must keep overlay metadata.
```

## Repair loop policy

Default repair attempts: `2`.

Allowed only for `repairable_fail`.

Stop when:

```text
- validation passes;
- the same error repeats;
- a forbidden field changes;
- max attempts are reached;
- blocked_fail or review_required appears.
```

## Why this matters

This lets LLMGameCreator use local models aggressively without making them dangerous:

```text
26B model thinks and designs.
E4B model generates many strict small artifacts.
Repair role fixes local failures.
Validator service decides what is valid.
Human approves only important changes.
Runtime consumes compiled data, not model output.
```

That keeps the system expandable without burning Codex limits or requiring a new C# subsystem for every small idea.

## Next recommended seed

```text
generator-library/atlas/prompt_context_pack_map.json
docs/PROMPT_CONTEXT_PACK_MAP.md
```

This should define how selected context packs are assembled, cached, versioned and attached to model roles without dumping the whole project into prompts.
