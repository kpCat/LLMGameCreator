# Goal 004 - Rule-Pack Driven Quest, Dialogue, And Interaction Families

## Goal

Prove that richer generated gameplay can be expanded through semantic/rule packs instead of new C# gameplay slices for every quest, dialogue, and interaction variant.

This goal must not turn Runtime Preview into a full game. Runtime Preview remains a proving ground.

## Required Starting Condition

Before starting this goal, the user must confirm:

```text
manual_extension_spine_verification passed
```

Do not infer this automatically from tests. If the manual gate is still active and the user has not confirmed it, stop.

## Context Budget Rule

Read first:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- this goal file

Read `docs/CURRENT_GENERATOR_STATE.md` only because this goal changes state/gate handoff.

Read broad strategy docs only if needed:

- `docs/ARCHITECTURE_STRATEGY_AND_BOUNDARIES.md`
- `docs/SEMANTIC_PACK_AND_RAG_STRATEGY.md`
- `docs/EXTENSION_RULE_PACK_CONTRACT_V1.md`
- `docs/AGENT_CONTEXT_BUDGET_POLICY.md`

Do not read old slice docs unless the code or tests require them.

## Non-Goals

- No Unity work.
- No media generation.
- No provider/LLM execution.
- No arbitrary Lua execution.
- No broad GamePackage schema redesign.
- No Runtime Preview polish beyond minimal evidence display required by acceptance.
- No S054 or later slices.

## Product Slices

### S049 - Record Manual Extension Verification And Context Budget Policy

Purpose:

- record `manual_extension_spine_verification passed`;
- add/link `AGENT_CONTEXT_BUDGET_POLICY.md`;
- update state/docs so future tasks read compact context first.

Acceptance:

- state records Goal 003 manual verification as passed;
- context budget policy is linked in `CONTEXT_INDEX.md`;
- `CurrentGeneratorStateDocsTests` pass.

### S050 - Quest Pattern Rule Pack V1

Purpose:

Add quest-pattern declarations without adding bespoke quest C# for each pattern.

Support at least:

- fetch item;
- deliver item;
- recover item from encounter/region;
- interact with NPC/object;
- sequence of 2-3 objectives.

Acceptance:

- validator accepts valid quest pattern packs;
- validator rejects unsafe/unknown objective/action ids;
- generated scenario can produce at least two different quest structures from rule-pack data;
- sidecar report lists selected quest pattern and objectives.

### S051 - Dialogue Intent Pattern Rule Pack V1

Purpose:

Add dialogue intent patterns as data declarations, not free LLM text generation.

Support at least:

- greeting;
- ask about quest;
- warn/threaten;
- bargain/reward;
- completion response.

Acceptance:

- generated dialogue lines or dialogue nodes are derived from intent templates and semantic slots;
- no LLM call is required;
- invalid dialogue intent pack is rejected;
- generated content includes non-empty dialogue evidence for at least one scenario.

### S052 - Interaction Pattern Rule Pack V1

Purpose:

Add interaction patterns as rule-pack declarations.

Support at least:

- inspect;
- talk;
- take/collect;
- resolve challenge;
- use item/action on target.

Acceptance:

- interaction patterns bind to generated NPC/item/encounter/location targets;
- runtime scenario can invoke at least two different interaction families;
- interaction result changes runtime state or generated report evidence;
- invalid interaction target/action references are rejected.

### S053 - Quest/Dialog/Interaction Family Acceptance

Purpose:

Prove the combined family works across several generated variants.

Required scenarios:

- baseline generated microgame;
- quest pattern variant;
- dialogue intent variant;
- interaction pattern variant;
- invalid pack rejection scenario.

Acceptance:

- headless scenario harness passes;
- product smoke route exists;
- deterministic reports under `.llmgc/procedural/quest-dialog-interaction-families/`;
- runtime-backed goal/reward/completion evidence remains valid;
- final `check-all.ps1` passes;
- state stops at `manual_quest_dialog_interaction_family_verification`;
- no S054 created.

## Required Reports

Write deterministic reports under:

```text
.llmgc/procedural/quest-dialog-interaction-families/
```

Expected files:

- `quest-dialog-interaction-family-report.json`
- `quest-dialog-interaction-family-report.md`
- `manual-quest-dialog-interaction-family-verification.md`

## Final Manual Gate

At the end of S053, stop.

Do not continue to another feature goal.

Next manual check should verify:

- generated quest has meaningful objectives;
- generated dialogue is non-empty and tied to quest/interaction context;
- interactions are understandable;
- reward/completion still comes from runtime-backed state;
- different variants differ in quest/dialogue/interaction behavior.

## Final Report

The final Codex report must include:

- slices completed;
- changed files;
- tests run;
- whether any C# gameplay primitive was added and why;
- which behavior is now data/rule-pack extensible;
- what still requires C# primitives.

