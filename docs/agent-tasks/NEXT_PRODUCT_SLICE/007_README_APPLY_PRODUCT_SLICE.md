# LLMGameCreator Product Slice 007 Pack

## Purpose

Product Slice 007 adds the first interactive layer over generated content in Runtime Preview.

Current state:
- generatedContent is visible in Runtime Preview;
- expanded contracts generate regions/NPCs/items/dialogues/encounters;
- headless smoke validates package assembly and preview projection.

Goal:

```text
Runtime Preview
-> generated content browser
-> select scene/region/NPC/item/dialogue/quest/mechanic/encounter
-> inspect details/references
-> produce preview log messages/actions
-> headless smoke validates interactions
```

This is still not Unity and not full gameplay simulation. It is a preview-player layer for generated package content.

## Apply

Unzip this archive at repository root.

Then give Codex the contents of:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/007_CODEX_PROMPT.md
```

## Recommended Codex reasoning level

Use Codex reasoning: **High**.

Reason:
This touches Runtime Preview UI, preview projection, generatedContent interaction model, product smoke, and tests. It should not become a runtime rewrite, so High is the right balance.

## Archive note

This pack intentionally places README/manifest under `docs/agent-tasks/NEXT_PRODUCT_SLICE/` to avoid adding more pack README files to repository root.
