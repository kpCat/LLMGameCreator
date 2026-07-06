# LLMGameCreator

LLMGameCreator is a WinForms editor, generator and validation combiner for
data-driven `GamePackage` games.

It is **not** a chat flow that writes and runs a whole game from one prompt.
It is a game product-line combiner: the user selects and configures mechanics,
world models, semantic packs, visual packs and generation options, then the
system deterministically materializes, validates, reviews and exports a
`GamePackage`.

The playable source of truth is the `GamePackage`: JSON definitions, maps,
entities, systems, dialogue, quests, items, asset references, script metadata,
validation reports and generation history.

## Product Intent

The long-term product goal is a configurable game combiner:

1. the user defines or imports lore/world bible material;
2. the user selects feature modules through UI controls, presets and rule packs;
3. the combiner generates a validated `GamePackage`;
4. the package is executed by canonical runtime services without live LLM calls;
5. Unity or another player consumes the package through stable player adapters;
6. automation verifies generated packages before rare manual milestone review.

The project should optimize for reusable game patterns, not one-off generated
code. A narrow alpha must prove an expansion-safe kernel, not hardcode a single
demo game.

## Runtime Boundary

Runtime/player code consumes compiled, validated package data. It must not call
live LLMs, RAG, media providers, asset generators, WinForms UI, or arbitrary Lua
execution.

LLM/RAG assistance belongs only in the editor, generation and authoring
pipeline. Future variable dialogue, quest text, descriptions, events and large
world variation should route through compiled semantic catalogs, seeds,
rule packs, phrase/dialogue/event grammars, deterministic runtime-safe variation,
validation and save-compatible runtime deltas.

## LLM Role

LLM assistance is optional and local-first. It may help with:

- lore discussion and world-bible drafting;
- semantic fact/relation extraction;
- naming, descriptions and prose drafts;
- strict draft artifacts that pass quarantine, validation and human review;
- validation-error explanations and repair proposals.

LLM must not be a runtime dependency, must not bypass validators and must not
create executable game behavior that is not represented by approved package
data, runtime primitives or typed/sandboxed script manifests.

## Expansion Model

Broad future scope must be reached through reusable module families:

- `FeatureModule`: selectable gameplay capability with dependencies, conflicts,
  validation rules, runtime primitives, save/load policy and presentation needs;
- `RuntimePrimitive`: deterministic command/effect/state primitive consumed by
  canonical runtime services;
- `SemanticPack`: compiled facts, relations, traits, dialogue/event grammar and
  naming/prose catalogs;
- `VisualPartPack`: deterministic visual part definitions, placement rules,
  fallback assets and approval metadata;
- `WorldSourceAdapter`: source-specific world inputs normalized into package-safe
  regions, chunks, POIs and traversal data;
- `PlayerAdapter`: Unity or other presentation layer over canonical package and
  runtime state, not a separate game implementation.

A feature is not considered product-ready merely because it has a projection,
report or candidate artifact. It must have validation, canonical runtime
behavior, save/load compatibility and a player-facing consumption path.

## Current Strategic Pressure

Recent work made the candidate pipeline visible from WinForms and proved
projection-compatible candidate selection. The next product risk is that
`projectionOnly=true` can continue indefinitely.

Near-term goals should reduce that gap:

```text
candidate package
→ package validation
→ canonical runtime playthrough
→ save/load/replay proof
→ Unity/player adapter consumes canonical state/transcript
→ one-click report
```

New proof-only goals are discouraged unless they directly unblock this path.

## Current State

README is a stable project overview, not the active handoff document. Mutable
project status, gate evidence and next-work routing are in:

- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Before any generator/Codex task, read those files and then:

- `docs/PRODUCT_LINE_CORE_STRATEGY.md`
- `docs/NARROW_ALPHA_EXPANSION_POLICY.md`
- `docs/AUTOMATED_VALIDATION_TIERS.md`
- the task-specific goal document.

## Automation Direction

The project should avoid manual checking after every goal. Each goal should
declare which automated tiers it covers:

1. build/source/forbidden-scope checks;
2. package schema and cross-reference validation;
3. canonical runtime playthrough;
4. generated candidate matrix;
5. Unity/player batchmode smoke;
6. rare manual milestone acceptance.

Manual Unity inspection is optional for normal goal verification unless the
active task explicitly declares a manual milestone gate.

## Repository Map

```text
src/
  LLMGameCreator.Domain/
    Domain contracts and game definitions.

  LLMGameCreator.GamePackage/
    Root GamePackage model and package path conventions.

  LLMGameCreator.Runtime.Abstractions/
    Runtime commands, events and state contracts.

  LLMGameCreator.Runtime/
    Headless command/event runtime and canonical runtime services.

  LLMGameCreator.Scripting/
    Typed Lua/script manifest abstractions and sandbox-oriented script lanes.

  LLMGameCreator.Generation/
    Generation jobs, context packs and LLM-facing editor models.

  LLMGameCreator.AssetPipeline/
    Asset request/provider abstractions.

  LLMGameCreator.Application/
    Application services, validators, generation workflows and use-cases.

  LLMGameCreator.Infrastructure/
    Storage, settings and infrastructure adapters.

  LLMGameCreator.WinForms/
    Editor UI shell and pages.

tests/
  LLMGameCreator.Tests/
    Smoke, contract, validator and runtime tests.
```

## Agent Rules

For current generator/product-slice work, do not use README as planning
authority. Use:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.*`
4. `docs/PRODUCT_LINE_CORE_STRATEGY.md`
5. `docs/NARROW_ALPHA_EXPANSION_POLICY.md`
6. `docs/AUTOMATED_VALIDATION_TIERS.md`
7. the current task/goal document
8. only then relevant architecture or contract docs.

Do not start the next goal while the task's required gate is still open. Do not
change public `GamePackage` schema, runtime contracts, Unity/player code,
provider execution, Lua execution or UI unless the task explicitly allows it.

Common validation scripts are task-specific:

```powershell
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\run-product-smoke.ps1 -Scenario <scenario-id>
```

A task is not complete merely because code compiles. It must satisfy the active
acceptance criteria, final gate status, artifact evidence, automated tier
coverage and forbidden-scope checks named by the active task.
