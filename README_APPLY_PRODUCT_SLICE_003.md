# LLMGameCreator Product Slice 003 Pack

## Purpose

Product Slice 003 turns strict LLM artifacts from “valid JSON waiting for review” into useful editor/package state.

Main goal:

```text
Artifact Review
-> approve baseline strict artifacts
-> apply approved artifacts into a draft GamePackage assembly
-> validate
-> save/export inspectable package output
```

This is the first slice where generated content starts becoming an actual game package, not only evaluated artifacts.

## Apply

Unzip this archive at repository root.

Then give Codex the contents of:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/003_CODEX_PROMPT.md
```

## Not included

This slice does not implement:

- full runtime preview;
- combat simulator;
- economy simulator;
- Lua executor;
- Unity export;
- full schema expansion for all future contracts.

It should map only the baseline contracts that passed M4.1:

```text
game_profile_v1
scene_pack_v1
quest_pack_v1
mechanics_pack_v1
```
