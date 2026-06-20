# Product Slice 007: Generated Content Interaction Preview

## Goal

Turn Runtime Preview from “map + generated content text” into a small interactive preview-player for generated package content.

Current product chain:

```text
Capability Picker
-> LLM Artifacts / batch presets
-> strict artifacts
-> Artifact Review
-> package assembly
-> Runtime Preview generated content tab
-> product smoke
```

Slice 007 adds:

```text
generated content catalog
-> selectable generated entries
-> details/references panel
-> preview actions
-> quest/dialogue/encounter/item/NPC inspect
-> headless interaction smoke
```

## What “interaction preview” means

This is not full gameplay simulation.

It means the user can browse and inspect generated content in context:

```text
select generated scene
select generated region
select generated NPC
select generated item
select generated dialogue
select generated quest
select generated mechanic
select generated encounter
```

And the preview can produce simple non-destructive actions:

```text
Inspect selected
Focus scene
Preview dialogue
Preview encounter
Show quest journal
Show item list
Show NPC roster
```

## Non-goal distinction

Do not implement:
- real dialogue choice execution;
- real combat resolution;
- real inventory pickup/drop;
- real quest objective state machine;
- region travel engine;
- Unity runtime;
- Lua/effect execution;
- LLM generation.

This slice is a preview/inspection layer, not a full player engine.

## Desired user flow

```text
1. Generate/apply full_small_rpg_seed.
2. Open Runtime Preview.
3. Press Start.
4. Open Generated Content tab.
5. Choose category: NPCs.
6. Select an NPC.
7. See NPC details, scene/region references, dialogue refs if available.
8. Choose Dialogues.
9. Select dialogue.
10. See dialogue lines and linked NPC/scene.
11. Choose Encounters.
12. Select encounter.
13. See setup/participants/scene/region refs.
14. Move player; generated content browser remains populated.
```

## Runtime Preview UI shape

Preferred minimal UI:

```text
Generated Content tab:
  left: categories/list
  right: details panel
  bottom/top: action buttons
```

Categories:

```text
Current scene
Regions
NPCs
Items
Dialogues
Quests
Mechanics
Encounters
Applied artifacts
Warnings
```

Actions:

```text
Inspect selected
Focus linked scene
Append to log
```

Action names may differ, but the user should be able to inspect details without opening raw JSON.

## Headless smoke

Add smoke scenario:

```text
generated-content-interaction-preview
```

The smoke should assemble expanded fixture content and validate:

```text
interaction catalog has entries for scenes/regions/npcs/items/dialogues/quests/mechanics/encounters
selecting each entry returns readable details
dialogue details include lines
quest details include steps/objectives
encounter details include references
NPC details include region/scene references
no LLM/provider required
existing runtime movement smoke still works
```

## Done

Done when generated content can be interactively browsed in Runtime Preview and validated headlessly.
