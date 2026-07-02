# Adult Visual Layer Documentation Manifest

## Status

Docs routing note.

This manifest exists so future agents can quickly find the adult-capable visual composition documents without relying on chat memory.

## Read order

1. `docs/context/VISUAL_ADULT_LAYER_CONTEXT_INDEX.md`
2. `docs/context/VISUAL_WORLD_GENERATION_CONTEXT_BRIEF.md`
3. `docs/proposals/PROCEDURAL_VISUAL_PART_PACKS.md`
4. `docs/proposals/ADULT_VISUAL_LAYER_STRATEGY.md`
5. `docs/proposals/CREATURE_VISUAL_GENOME_AND_PRESENTATION.md`
6. `docs/proposals/VISUAL_PART_PACK_ADULT_EXTENSION.md`
7. `docs/context/METAMODULE_CARRIER_VISUAL_NSFW_CONTEXT_BRIEF.md`
8. `docs/proposals/VISUAL_MEDIA_PIPELINE_IMPLEMENTATION_ROADMAP.md`
9. `docs/agent-tasks/CODEX_TASK_ADULT_VISUAL_LAYER_DOCS_ONLY.md`

## Core decision

```text
Adult/NSFW visuals are not a separate generator.
They are rating-gated slots, overlays, recipes, provider candidates and reviewed assets
inside the existing composable visual asset pipeline.
```

## Current scope

Documentation only.

No code, schema, Unity, Runtime, provider, ComfyUI or Civitai integration is implied by these docs.

## Goal 083 routing

Goal 083 integrates these documents into the official context spine with:

- `docs/context/VISUAL_ADULT_LAYER_CONTEXT_INDEX.md`
- `docs/proposals/VISUAL_MEDIA_PIPELINE_IMPLEMENTATION_ROADMAP.md`
- `.llmgc/procedural/goal-083-visual-adult-layer-context-integration/`
