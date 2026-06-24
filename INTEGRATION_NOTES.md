# Strategy Reset Integration Notes

These notes describe the minimal repository integration needed for:

`docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`

The GitHub connector in this session had read access only, so the repository was not updated directly.

## 1. Add the new source-of-truth document

Add:

```text
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
```

Use the file from this artifact.

## 2. Update docs/CONTEXT_INDEX.md

In `Generator task routing`, change the order to:

```text
1. AGENTS.md
2. docs/CONTEXT_INDEX.md
3. docs/CURRENT_GENERATOR_STATE.md
4. docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
5. docs/ROADMAP_TO_FULL_GENERATOR.md
6. only then task-specific docs
```

In `Full generator source-of-truth docs`, add this row near `CURRENT_GENERATOR_STATE.md`:

```markdown
| `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md` | Enforcing the post-S028 pivot from infrastructure growth to a generated playable/simulatable procedural kernel; checking freeze rules, next-three-slices limit and kill criterion before selecting next work. |
```

## 3. Update docs/CURRENT_GENERATOR_STATE.md

Change the header metadata:

```text
Updated by: Strategy Reset after Product Slice 028
```

In `Current phase`, replace the old phase summary with:

```text
M4.1 gate passed for sampled baseline contracts; Product Slice 028 completed manual import repair and semantic catalog foundation. The active product direction is now reset toward a playable/simulatable procedural generator kernel.
```

Add a section after `Last completed product slice`:

```markdown
## Active strategy reset

- Source of truth: `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`.
- Infrastructure-only progress is frozen unless explicitly requested by the user or required to unblock the playable/simulatable loop.
- The next three large product slices must produce a generated playable or simulatable loop.
- If that loop is not reached within three large slices, stop and reassess the architecture before spending more limit.
- Recommended next work is `Seeded Procedural Game Kernel v1`, followed by Formula/Effect/Action Registry Foundation and Tiny Generated Runtime Loop.
```

Replace the old allowed-next-work paragraph under `Current M5/M6 lock semantics`:

```text
Allowed next work remains bounded to manual import workflow polish after user testing, one controlled product vertical-slice selection, semantic catalog UI review/approval, or formula registry foundation.
```

with:

```text
Allowed next work is now bounded by the strategy reset: Seeded Procedural Game Kernel v1, Formula/Effect/Action Registry Foundation, or Tiny Generated Runtime Loop. Manual import polish, semantic catalog review UI, archive review polish and other infrastructure-only tasks require explicit user approval or a direct blocker relationship to the playable/simulatable loop.
```

## 4. Update docs/CURRENT_GENERATOR_STATE.json

Recommended field changes:

```json
{
  "current_phase": "strategy_reset_playable_procedural_generator",
  "current_phase_title": "Post-S028 strategy reset toward a playable/simulatable procedural generator kernel",
  "active_manual_gate": "Infrastructure-only next work is frozen unless explicitly approved by the user or required to unblock the generated playable/simulatable loop.",
  "current_user_action": "Review the strategy reset and choose Seeded Procedural Game Kernel v1 as the next product slice unless explicitly overriding the pivot.",
  "recommended_next_decision": "Start Seeded Procedural Game Kernel v1; do not spend further slices on semantic UI, manual import polish, archive review polish or other infrastructure-only work without explicit user approval.",
  "recommended_next_work_item": "seeded_procedural_game_kernel_v1",
  "recommended_next_work_item_title": "Seeded Procedural Game Kernel v1: generate deterministic runtime-facing game structure without LLM, providers, Unity, Lua execution or UI polish."
}
```

Also add:

```json
"active_strategy_reset": {
  "document": "docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md",
  "freeze_infrastructure_only_work": true,
  "required_outcome_within_large_slices": 3,
  "required_outcome": "generated_playable_or_simulatable_loop",
  "next_sequence": [
    "seeded_procedural_game_kernel_v1",
    "formula_effect_action_registry_foundation",
    "tiny_generated_runtime_loop"
  ],
  "kill_criterion": "If no generated playable or simulatable loop exists after the next three large product slices, stop and reassess architecture before spending more limit."
}
```

## 5. Suggested next Codex task title

```text
Product Slice 029: Seeded Procedural Game Kernel v1
```

This slice should not touch UI, providers, Unity, Lua execution, media generation or archive polish unless required by the generated runtime-facing output.
