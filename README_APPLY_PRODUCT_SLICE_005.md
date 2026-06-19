# LLMGameCreator Product Slice 005 Pack

## Purpose

Product Slice 005 makes the assembled generated package visible/playable in Runtime Preview.

Main goal:

```text
assembled package.json / current package
-> Runtime Preview starts
-> generated profile/scene/quest/mechanic summaries are visible
-> start scene description is shown
-> basic runtime start/move still works
-> headless runtime-preview smoke validates it
```

This is still WinForms/runtime-preview, not Unity.

## Apply

Unzip at repository root.

Then give Codex the contents of:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/005_CODEX_PROMPT.md
```

## Recommended Codex reasoning level

Use Codex reasoning: **High**.

Reason:
This touches RuntimePreview UI, Runtime/Application preview model, tests, and smoke flow. Medium risks missing the split between actual runtime mechanics and generated-content preview projection. Max/Ultra is not needed unless High fails.

## Not included

This slice does not implement:

- Unity runtime;
- real dialogue/combat simulator;
- economy simulation;
- Lua execution;
- new LLM generation;
- full quest progression engine.
