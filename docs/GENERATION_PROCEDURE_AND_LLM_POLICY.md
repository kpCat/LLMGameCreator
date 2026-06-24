# Generation Procedure and LLM Minimization Policy

## Product rule

LLMGameCreator is a deterministic combiner with bounded creative-model assistance. It must not become a pipeline where an LLM generates everything and C# merely accepts the result.

The decision rule is:

```text
If a step can be done deterministically without quality loss, variability loss,
balance loss, or authoring-power loss, the combiner does it without an LLM.
```

Semantic dictionaries are not a closed universal ontology and are not a restriction on author creativity. The project-local semantic catalog is reusable meaning memory: known terms, candidates, relations, provenance, diagnostics, and hints that reduce repeated prompt context while allowing new terms to enter review as candidates.

## Procedure from the user's point of view

1. Choose or create a game idea, example, or preset.
2. The combiner builds a deterministic capability and artifact plan.
3. An LLM is scheduled only for missing creative artifacts that cannot be produced without quality or authoring-power loss.
4. Generated artifacts enter review; nothing is silently promoted.
5. The user approves, rejects, or requests repair.
6. The combiner validates and assembles GamePackage deterministically.
7. The combiner exports deterministic package/archive/request plans.
8. Media, code, or provider outputs are fulfilled manually or by future explicitly enabled providers.
9. Review, history, and comparison show what changed.
10. The user tests the result and approves the next controlled vertical slice.

The intended flow is:

```text
User intent / preset / example
-> minimal LLM calls for creative semantic and game-design decisions only
-> deterministic validation and assembly
-> deterministic GamePackage/export/archive/request planning
-> manual/provider fulfillment
-> review/history/comparison
-> user approval/testing
```

## When an LLM is used

An LLM may be useful for:

- original game concept text and worldbuilding expansion;
- quest and dialogue concepts or prose;
- natural-language variants where authored variation is the product value;
- art/audio prompt phrasing when deterministic templates are insufficient;
- resolving ambiguous user intent when deterministic presets conflict.

An LLM is not used for:

- id or path creation;
- file copying;
- schema or compatibility validation;
- slot, request, or fulfillment planning;
- archive review, history, or comparison;
- manifest templates;
- report rendering;
- known-term lookup and semantic catalog merge;
- deterministic/configurable balance formulas;
- fallback placeholder generation.

If a requested artifact can be produced by deterministic templates, rules, presets, libraries, or semantic lookup without quality loss, deterministic generation wins. If it requires original creative prose, worldbuilding, dialogue, quest concepts, or prompt phrasing, the combiner schedules an LLM artifact request. If the distinction is uncertain, the combiner produces a reviewable generation plan instead of calling an LLM immediately.

## Expected LLM load tiers

These are rough planning ranges, not guarantees:

- Small prototype: 5-20 calls with compact prompts, mainly concepts, NPCs, quests, and dialogue seeds.
- Medium game: 30-150 calls batched by artifact kind, with semantic context reused.
- Large game: hundreds of calls chunked by region, faction, and questline; never one huge monolithic prompt.
- Huge content library: batching, caching, semantic reuse, deterministic expansion, and user-approved generation queues are mandatory.

Prompt count should scale with genuinely creative decisions, not with paths, ids, copies, reports, validation, or bulk deterministic expansion.

## Project-local semantic memory

The semantic catalog stores a compact vocabulary and relations for one project. Built-in seed terms provide a small useful baseline. Generated or author-supplied unfamiliar terms remain candidates rather than hard failures. Invalid or unsafe ids become diagnostics and are not promoted.

The semantic generation context preview demonstrates what compact context a future explicit LLM step would receive: key themes, tones, candidates, dialogue intents, quest motifs, asset/audio hints, relations, and unresolved conflicts. The preview itself is deterministic and performs no model call.

## Extensibility tiers for mechanics, formulas, and modes

The design target is:

```text
data-first mechanics
formula registry
effect/action DSL
validation contracts
small UI adapters
runtime support only when a truly new runtime primitive is needed
```

### Tier 1: data/config only

Examples: new item stats, dialogue intents, quest motifs, semantic tags, and balancing constants.

These should not require a large vertical slice.

### Tier 2: formula/effect extension

Examples: a new damage or reputation formula, requirement type, or reward type.

These should be small or medium slices through registries, effect/action contracts, and validators.

### Tier 3: new systemic mechanic

Examples: crafting networks, faction diplomacy, settlement economy, stealth, and weather survival.

These are medium or large slices, but contracts should isolate them from unrelated systems.

### Tier 4: new runtime interaction mode

Examples: turn-based tactical combat, real-time action combat, vehicle simulation, and colony-simulation UI.

These require a large controlled vertical slice because they introduce genuinely new runtime and presentation primitives.

The architecture should make most future author requests Tier 1 or Tier 2. Giant repeated slices are reserved for truly new systemic or runtime interaction primitives.

## Current milestone boundary

This policy is a decision record and project-local data foundation. It does not unlock M5 Lua module execution or M6 rich GamePackage assembly, does not change GamePackage schema, and does not authorize provider, generator, LLM, Lua, Unity, or Runtime gameplay execution.
