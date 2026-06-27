# LLMGameCreator

LLMGameCreator is a WinForms editor and generator for data-driven `GamePackage`
games. It is not a chat flow that writes and runs a whole game from one prompt.

The project builds game packages through bounded authoring, validation,
assembly, review and runtime/export evidence. The playable source of truth is
the `GamePackage`: JSON definitions, maps, entities, systems, dialogue, quests,
items, asset references, script metadata, validation reports and generation
history.

## Runtime Boundary

Runtime/player code consumes compiled, validated package data. It must not call
live LLMs, RAG, media providers, asset generators, WinForms UI, or arbitrary Lua
execution.

LLM/RAG assistance belongs only in the editor, generation and authoring
pipeline. Future variable dialogue, quest text, descriptions, events and large
world variation should route through compiled semantic catalogs, seeds, rule
packs, phrase/dialogue/event grammars, deterministic runtime-safe variation,
validation and save-compatible runtime deltas.

## Current State

README is a stable project overview, not the active handoff document. Mutable
project status, gate evidence and next-work routing are in:

- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Before any generator/Codex task, read those files and then the task-specific
docs named by current state routing.

## Process Direction

Recent generator work proved these reusable steps:

- Formula/Effect/Action Registry Foundation
- Tiny Generated Runtime Loop
- Generated Package MVP
- Visible Generated Playable Preview

The current planning model uses modular contracts, bounded composite goals and
rare product vertical gates. Contract, module, integration and proof phases are
internal phases of one bounded composite goal by default, not separate manual
goals. See `docs/MODULAR_CONTRACT_GOAL_POLICY.md`.

Wanted future capabilities are tracked without making them active work in
`docs/LLMGameCreator_FEATURE_BACKLOG_AUDIT.md`. Package assembly sequencing is
planned in `docs/PACKAGE_ASSEMBLY_EXPANSION_CAMPAIGN_PACK.md`.

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
    Headless command/event runtime.

  LLMGameCreator.Scripting/
    Typed Lua/script manifest abstractions and prototype sandbox.

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
4. the current task/goal document
5. only then relevant architecture or contract docs

Do not start the next goal while the task's required gate is still open. Do
not use git commands unless the user explicitly asks. Do not change public
`GamePackage` schema, runtime contracts, Unity/player code, provider execution,
Lua execution or UI unless the task explicitly allows it.

Common validation scripts are task-specific:

```powershell
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\run-product-smoke.ps1 -Scenario <scenario-id>
```

A task is not complete merely because code compiles. It must satisfy the active
acceptance criteria, final gate status, artifact evidence and forbidden-scope
checks named by the active task.
