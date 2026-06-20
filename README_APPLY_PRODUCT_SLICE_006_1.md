# LLMGameCreator Product Slice 006.1 Pack

## Purpose

Slice 006.1 is a narrow UI/productivity slice.

Goal:

```text
LLM Artifacts
-> batch preset dropdown
-> selecting a preset checks the matching contracts
-> existing Generate/Preview/Load flow remains unchanged
```

This uses the existing batch preset API added in Slice 006.

## Apply

Unzip at repository root.

Then give Codex the contents of:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/006_1_CODEX_PROMPT.md
```

## Recommended Codex reasoning level

Use Codex reasoning: **Medium**.

Reason:
This is a narrow UI wiring task. High is acceptable if Codex starts drifting, but Medium should be enough because the API/preset definitions already exist and are tested.

## Not included

This slice does not add new contracts, validators, package assembly mapping, runtime logic, Unity, Lua, or LLM generation behavior.
