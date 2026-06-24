# Product Slice 029 Codex Task: Seeded Procedural Game Kernel v1

This is the agent-task copy of:

```text
docs/NEXT_PRODUCT_SLICE_029_SEEDED_PROCEDURAL_GAME_KERNEL_TASK.md
```

Use this task only after the strategy-reset source-of-truth files are in place:

- `README.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`

## Task

Implement Product Slice 029: Seeded Procedural Game Kernel v1.

Read and follow:

```text
docs/NEXT_PRODUCT_SLICE_029_SEEDED_PROCEDURAL_GAME_KERNEL_TASK.md
```

## Hard Bounds

- No LLM calls.
- No provider calls.
- No Unity work.
- No media generation.
- No Lua execution.
- No semantic catalog approval UI.
- No archive review/manual import polish.
- No broad template family work.
- No C# code generation.
- No GamePackage schema change unless absolutely required and explicitly approved.

## Required Outcome

Produce deterministic runtime-facing generated game structure from a seed:

- generated-game-plan JSON;
- generated-game-plan Markdown;
- small world/region graph or map plan;
- at least two factions/groups;
- actor archetype seeds;
- item/resource seeds;
- encounter seeds;
- quest/event seeds;
- deterministic diagnostics and summary;
- tests proving same-seed repeatability and different-seed variation.

## Required Product Smoke

Add and pass:

```text
procedural-game-kernel
```

## Final State Update

After completing this slice, update:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`

The next recommended work after Slice 029 must be:

```text
formula_effect_action_registry_foundation
```
