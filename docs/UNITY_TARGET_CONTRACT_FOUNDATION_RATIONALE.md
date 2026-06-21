# Unity Target Contract Foundation Rationale

## Why not generate one-off Unity C# games?

Because this would create fragile code. The safer architecture is:

```text
generic Unity runtime
+ independent runtime modules
+ strict archive contracts
+ generated data
+ generated/approved assets
+ Lua/data modules
```

## What should LLM do?

LLM should mostly generate lore seeds, style, factions, unique NPCs, special quest chains, rare scenes, asset prompts and semantic hints.

## What should the program generate?

The program should generate bulk NPC population, routine quests, schedules, loot placement, shops, encounters, chunks, UI layout variants, asset queues and validation reports.

## What should Lua define?

Lua should define typed declarative extensions: item families, recipes, effects, quest templates, NPC archetypes, building archetypes, encounter archetypes, UI style presets and audio event bindings.
