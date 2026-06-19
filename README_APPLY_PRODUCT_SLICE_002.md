# LLMGameCreator Product Slice 002 Pack

## Purpose

Product Slice 002 makes the Capability Picker actually use the non-breaking composable capability fields introduced in Product Slice 001.

Main goal:

```text
Composable module selection UI
+ selected_module_ids / selected_modifier_ids / selected_constraint_ids / runtime_requirement_ids wired end-to-end
+ better user-facing list rendering
+ prompt/save/load compatibility
+ tests/check-all
```

This is a large implementation task for Codex, not documentation-only.

## Apply

Unzip this archive at repository root.

Then give Codex the contents of:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/002_CODEX_PROMPT.md
```

## Recommended reasoning level

Use Codex reasoning: **High**.

Do not use Max/Ultra for the first attempt unless High fails, because this task is broad enough to need reasoning, but still bounded to a known subsystem.

Do not use Low/Medium: it is likely to miss UI-state persistence or saved-selection compatibility and cause more repair runs.

## Not included

This slice does not implement:

- full Design Assistant chat;
- package assembly/apply path;
- runtime preview;
- balance simulator;
- economy simulator;
- Lua executor.
