# Goal 034 Spec — Strict LLM Draft Artifact Loop

## Goal id

```text
goal_034_strict_llm_draft_artifact_loop
```

## Gate

```text
strict_llm_draft_artifact_loop_verification
```

## Purpose

Create a strict Application-layer loop for future LLM/manual/imported draft artifacts:

- request contracts;
- quarantined candidate envelopes;
- validation diagnostics;
- deterministic repair request records;
- promotion decisions;
- compact evidence.

The goal is not to make LLM write the game. The goal is to make sure that when LLM is used later, it can only produce bounded JSON draft candidates that the program validates and may reject or route to repair.

## Why this goal is next

Goal 033 produced a semantic authoring workspace and feature-driven content intent resolver. The next risk is that an LLM draft pipeline could bypass the programmatic semantic system and start writing final content directly.

Goal 034 closes that risk by introducing a strict draft artifact loop:

```text
Goal 033 intents + authoring context
-> draft request records
-> quarantined candidate envelopes
-> deterministic validation
-> repair request records
-> promotion decision records
-> no final prose / no GamePackage materialization
```

## Required product direction

The program owns:

- semantic features;
- applicability;
- inheritance;
- influence rules;
- content intents;
- validation;
- promotion decisions.

LLM may only help later by proposing draft candidates under a contract. In Goal 034 even that is simulated by deterministic fixtures. No provider calls.

## Minimum draft artifact families

The loop must cover at least these draft families at planning/candidate level:

1. lore rule draft;
2. species/archetype feature draft;
3. faction relation draft;
4. NPC role/personality draft;
5. quest motive/objective draft;
6. dialogue act/template-slot draft, without final prose;
7. economy/item/resource hint draft;
8. combat/ability/progression hint draft;
9. settlement/region/event hint draft.

## Required concepts

### Draft request

A draft request describes what a future LLM/manual/import source is allowed to propose.

Required meanings:

- request id;
- scenario/profile id;
- target draft family;
- source intent ids from Goal 033 style data;
- allowed artifact contract ids / semantic scopes;
- required fields;
- forbidden fields;
- maximum candidate count;
- no-final-prose flag;
- no-runtime-authority flag;
- expected provenance class;
- repair policy id;
- deterministic ordering key.

### Candidate envelope

A candidate envelope represents a returned draft before acceptance.

Required meanings:

- candidate id;
- request id;
- source kind: manual / llm / imported / programmatic_fixture;
- provenance details;
- payload kind;
- payload fields as typed BCL data, not arbitrary unvalidated object graphs;
- declared links to intents/features/contracts;
- declared constraints;
- candidate status: quarantined / rejected / repair_required / promotable / promoted;
- diagnostics.

### Validator

Must causally validate:

- duplicate ids;
- unknown request;
- wrong family;
- missing required fields;
- forbidden final prose fields;
- provider/runtime/UI/Unity/Lua/GamePackage/code generation leakage;
- candidate marked accepted without promotion decision;
- provenance mismatch;
- missing intent/feature trace;
- fake target contract;
- fake semantic scope;
- incompatible scenario/profile;
- over-budget candidate count;
- invalid repair loop target;
- nondeterministic candidate ordering.

### Repair request

A repair request is a data record, not a provider call.

Required meanings:

- repair request id;
- source candidate id;
- blocking diagnostic codes;
- allowed fields to fix;
- fields that must not be changed;
- semantic context digest/hints;
- max retry count;
- next candidate request id or same request id;
- deterministic status.

### Promotion decision

Promotion is not materialization into GamePackage.

Required meanings:

- candidate id;
- request id;
- promotable boolean;
- promoted draft artifact id if accepted;
- reasons;
- diagnostics;
- preserved provenance;
- target draft artifact family;
- promotion status.

### Evidence

Write compact deterministic evidence under:

```text
.llmgc/procedural/goal-034-strict-llm-draft-artifact-loop/
```

Required files:

```text
draft-loop-contract-summary.json
draft-request-matrix.json
candidate-quarantine-matrix.json
repair-request-matrix.json
promotion-decision-matrix.json
strict-draft-plan-frontier.json
strict-draft-plan-gothic.json
strict-draft-plan-caravan.json
strict-draft-plan-metamodule-kingdoms.json
invalid-draft-diagnostics-matrix.json
strict-llm-draft-artifact-loop-report.md
```

All JSON must be stable and compact. Avoid timestamps, absolute paths and heavy logs.

## Required scenarios

At minimum:

- `frontier_survival`;
- `gothic_intrigue`;
- `caravan_trade`;
- `metamodule_kingdoms`.

The metamodule scenario must prove that the loop can handle many species/archetype slots without asking LLM to produce final content.

## Out of scope

- provider/LLM/RAG calls;
- final prose/dialogue/quest text generation;
- prompt engineering as accepted output;
- runtime use of LLM;
- Lua execution;
- GamePackage schema changes;
- WinForms/UI changes;
- Unity changes;
- external NuGet dependencies;
- media/image/audio generation.

## Acceptance expectation

The final report must leave:

```text
strict_llm_draft_artifact_loop_verification required
```

Do not mark the gate passed inside the same goal.
