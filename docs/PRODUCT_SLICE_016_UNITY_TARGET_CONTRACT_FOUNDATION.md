# Product Slice 016: Unity Target Contract + Game Design Brief Foundation

## Goal

Redirect the project from internal Runtime Preview toward the real end target:

```text
LLMGameCreator builds game archives
→ flexible Unity runtime/player loads those archives
→ later a standalone Unity build can be produced for a generated game
```

This slice does not implement Unity. It adds the first machine-readable contracts for:

```text
game design brief / lore / rules
Unity target profile
Unity game archive manifest
Unity runtime module contracts
dynamic UI layout contract
asset/audio generation request contracts
world streaming/persistence policy
```

## Important principle

For large worlds, do not store every NPC/quest/object in advance. Store:

```text
world seed
semantic rules
generation rules
templates
important authored entities
persistent generated deltas
save-game state
```

## Non-goals

No Unity project/runtime/build, no ComfyUI/Suno integration, no real map import, no semantic world model implementation, no procedural quest engine, no NPC schedules, no package schema changes.
