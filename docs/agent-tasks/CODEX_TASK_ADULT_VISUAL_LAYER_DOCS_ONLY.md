# Codex Task — Adult Visual Layer Documentation Only

## Task ID

`adult-visual-layer-docs-only-v1`

## Goal

Add and cross-link documentation that records the adult-capable visual composition direction for LLMGameCreator.

This is a documentation-only task. It must not implement code, schemas, Unity changes, provider calls or ComfyUI integration.

## Why

LLMGameCreator is intended to support rich 2D / 2.5D / pseudo-3D generated games where graphics are composed from semantic visual packs, reusable part families, palettes, layers, states and deterministic recipes.

Some generated games may support adult/NSFW visuals for adult, sapient, humanoid-compatible fantasy species. That direction must be documented as a rating-gated extension of the existing composable visual pipeline, not as an ad-hoc prompt/image generator.

## Read first

- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/context/VISUAL_WORLD_GENERATION_CONTEXT_BRIEF.md`
- `docs/proposals/PROCEDURAL_VISUAL_PART_PACKS.md`
- `docs/agent-tasks/CODEX_TASK_VISUAL_DETAIL_GENERATOR_CORE.md`
- `docs/agent-tasks/CODEX_TASK_PROCEDURAL_VISUAL_PART_PACK_COMPILER.md`
- `docs/proposals/ADULT_VISUAL_LAYER_STRATEGY.md`
- `docs/proposals/CREATURE_VISUAL_GENOME_AND_PRESENTATION.md`
- `docs/proposals/VISUAL_PART_PACK_ADULT_EXTENSION.md`
- `docs/context/METAMODULE_CARRIER_VISUAL_NSFW_CONTEXT_BRIEF.md`

## Required documentation direction

Make sure the documentation states:

```text
Adult/NSFW visuals are not a separate generator.
They are rating-gated asset slots, overlays and reviewable provider candidates
inside the same composable visual recipe / asset pipeline system.
```

The docs must preserve these architectural rules:

- Runtime/Unity Player does not call LLM, ComfyUI, Fooocus, InvokeAI or media providers.
- GamePackage and manifests remain the source of truth.
- LLM may help with creative concepts and prompt hints only.
- Codex implements generators/validators/contracts, not thousands of finished art records.
- Provider output enters candidate quarantine before promotion.
- Adult slots require explicit rating/export policy.
- Safe/public builds must have deterministic safe fallbacks.

## Content boundary to preserve

Adult visual content is allowed only for adult, sapient, humanoid-compatible fantasy characters or adult humans when the project enables adult content.

Reject or quarantine any documentation or future implementation path that implies:

- minors, teen-like, childlike, young-looking or age-ambiguous subjects;
- feral or non-sapient sexualized creatures;
- non-consensual sexual framing;
- adult assets leaking into safe/public builds;
- provider output promoted without review;
- prompt text as source of truth.

## Allowed files

Documentation-only paths, such as:

```text
docs/proposals/ADULT_VISUAL_LAYER_STRATEGY.md
docs/proposals/CREATURE_VISUAL_GENOME_AND_PRESENTATION.md
docs/proposals/VISUAL_PART_PACK_ADULT_EXTENSION.md
docs/context/METAMODULE_CARRIER_VISUAL_NSFW_CONTEXT_BRIEF.md
docs/context/VISUAL_WORLD_GENERATION_CONTEXT_BRIEF.md
docs/proposals/PROCEDURAL_VISUAL_PART_PACKS.md
docs/CONTEXT_INDEX.md
```

If `docs/CONTEXT_INDEX.md` is touched, only add routing rows or a short context pointer. Do not rewrite active goal state.

## Forbidden scope

Do not:

- change C# source code;
- change Unity project files;
- change public GamePackage schema;
- add ComfyUI/Fooocus/InvokeAI provider integration;
- call external providers;
- add binary media assets;
- add real NSFW image fixtures;
- add dependencies;
- change `.sln` or `.csproj`;
- create or modify generated `.llmgc` evidence;
- add git/branch/push/rebase/cherry-pick instructions.

## Acceptance checklist

The final report should show:

- docs added/updated;
- no source code changed;
- no project files changed;
- no binary media added;
- no provider integration added;
- adult visual strategy documented;
- creature visual genome documented;
- visual part pack adult extension documented;
- "Носитель Метамодулей" context brief documented;
- existing visual generation context cross-linked.

## Stop conditions

Stop and report instead of implementing if:

- the task appears to require C# implementation;
- the task appears to require schema changes;
- the task appears to require real NSFW media assets;
- the task appears to require provider integration;
- the repository has conflicting active docs that make the intended documentation unsafe or contradictory.
