# LLMGameCreator Product Slice 009 Pack

## Purpose

Product Slice 009 places generated NPCs and encounters onto the Runtime Preview map as preview markers.

Current state:
- generated NPCs/encounters exist in the Browser;
- quest/dialogue preview stubs work;
- Runtime Preview map still mostly shows only the player/runtime map.

Goal:

```text
generatedContent.npcs / generatedContent.encounters
-> deterministic preview placement
-> map markers
-> inspect/interact nearby marker
-> log/details integration
-> headless smoke
```

This is still not Unity and not a full runtime simulation.

## Apply

Unzip this archive at repository root.

Then give Codex:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/009_CODEX_PROMPT.md
```

## Recommended Codex reasoning level

Use **High**.

Important: do not solve language/localization or procedural quest template generation in this slice. Those are important, but should be a separate controlled slice after map placement.
