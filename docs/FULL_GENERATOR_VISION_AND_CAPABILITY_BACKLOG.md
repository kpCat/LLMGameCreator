# Full Generator Vision And Capability Backlog

## Purpose

This document captures the broad target vision for LLMGameCreator so future tasks do not accidentally forget major desired capabilities.

This is not the active goal queue. It does not override:

- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- current Goal/task files

Use this document as a capability backlog, design pressure source and anti-forgetting reference when shaping future composite packs.

## North-star product

LLMGameCreator should become a deterministic procedural game generator and authoring pipeline capable of generating rich games from contracts, semantic catalogs, rule packs, seeds and modular capability bundles.

Runtime must not depend on live LLM/RAG/provider/media generation. LLM and RAG may assist editor-time/generation-time authoring, drafting and repair, but the shipped runtime consumes compiled, validated, deterministic data.

## Heavy reference fantasy-scale target

The generator should eventually be capable of producing games with properties similar to the following, without hardcoding one game:

- huge or effectively unbounded world structure;
- regions, chunks, biomes, settlements, cities, castles, interiors, houses and ownership;
- 100+ creature/NPC/monster archetypes;
- player travel, exploration, consequences and broad agency;
- procedural but coherent dialogue, quests, puzzles and events;
- factions, reputation, diplomacy, wars, treaties, alliances and kingdom/faction creation;
- NPC schedules, work, rest, feasts, travel, social life and non-uniform personalities;
- combat, sieges, bosses, minibosses, armies, abilities, magic, effects and terrain/status effects;
- base and advanced stats, progression, skills, perks, active/passive abilities;
- weapons, armor, equipment, inventory, loot and trade;
- crafting, cooking, alchemy/potions, recipes and resource chains;
- economy, vendors, caravans, property, land ownership, homes and settlements;
- stealth, perception, vision, theft, lockpicking, law/consequences;
- weather, day/night, sleep, seasons or world-event timing where needed;
- construction, destruction, territory effects and settlement changes;
- semantic variation so new playthroughs do not follow the same known path;
- portrait/appearance/dialogue presentation, facial reactions and relationship-sensitive responses;
- asset/audio/music planning through validated asset request catalogs and deterministic fallbacks.

This backlog intentionally preserves ambitious wants. It does not imply all are implemented in one vertical slice.

## Required architectural stance

The target is not "LLM writes the whole game." The target is:

```text
editor-time LLM/RAG assistance
+ deterministic semantic catalog
+ procedural generator modules
+ capability/module registry
+ contract validation
+ package/runtime assembly
+ deterministic runtime-safe variation
```

LLM may suggest themes, semantic anchors, local text, quest/dialogue intent drafts, faction/world bible drafts or repair candidates. Programmatic systems should generate bulk structure: topology, biome distributions, NPC population, schedules, economy, encounters, loot, weather, event graphs, content variation and balance envelopes.

## Capability families

### World and geography

- world topology;
- region graph;
- chunk/zone generation;
- biomes and climate;
- terrain descriptors;
- roads, rivers, borders, settlements;
- infinite/large-world paging strategy;
- interiors, houses, dungeons, caves, castles;
- travel costs, visibility and discovery;
- persistent world deltas.

### Actors, archetypes and population

- player archetypes;
- NPC archetypes;
- monster/creature archetypes;
- factional roles;
- professions;
- schedules;
- family/social links;
- moods and dispositions;
- traits and appearance;
- non-uniform dialogue style;
- synthetic population balancing.

### Factions, social systems and law

- faction membership;
- faction reputation;
- social standing;
- diplomacy;
- contracts/treaties;
- wars and raids;
- law/crime/theft;
- settlement governance;
- player-created faction/kingdom;
- property and land rights.

### Dialogue, quests and semantic variation

- dialogue graph generation;
- dialogue conditions and consequences;
- quest graph generation;
- puzzle objective generation;
- rumor boards and knowledge spread;
- local story arcs;
- global event hooks;
- compiled phrase/dialogue/event grammars;
- semantic catalog links to world facts;
- deterministic runtime-safe variation.

### Items, economy and crafting

- item definitions;
- resource definitions;
- recipes;
- crafting stations;
- cooking;
- alchemy/potions;
- loot tables;
- vendors;
- trade routes/caravans;
- transactions;
- inventory/equipment slots;
- supply/demand;
- property/land/home purchase.

### Combat, abilities and progression

- stats;
- abilities;
- status effects;
- damage/resistance;
- encounter definitions;
- combat actions;
- creature abilities;
- bosses/minibosses;
- armies/sieges;
- progression tracks;
- skill/perk trees;
- balance envelopes.

### Simulation and runtime state

- deterministic simulation ticks;
- save-compatible deltas;
- runtime-only generated deltas;
- absence handling for missing optional modules;
- module dependency effects;
- event scheduling;
- day/night/weather/time;
- NPC activities and tasks.

### Presentation, Unity and assets

- generated package projection into Unity;
- runtime player abstraction;
- 2D/3D presentation policy;
- tile/sprite/mesh requests;
- portraits and expressions;
- sound/music request catalogs;
- deterministic fallbacks;
- external asset policy;
- no runtime provider dependency.

## Backlog handling rules

- Do not delete wants because they are not implemented yet.
- Move wants into capability families, future gaps or campaign packs.
- Do not turn every want into an immediate implementation goal.
- Prefer module contracts and compatibility manifests before implementation.
- Product vertical gates are rare; module proof is enough for many intermediate goals.
- If a capability seems too large, change the implementation strategy; do not silently shrink the target.
