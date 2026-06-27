# LLMGameCreator Feature Backlog Audit

Status: draft audit based on current repository documentation and known user goals  
Scope: documented wishes, planned capabilities, likely missing wishes, and semantic/runtime generation concerns  
Non-scope: implementation task, schema change, production code

## 1. Important warning

This file is not an authoritative source of truth yet.

It is a human-readable audit of the feature direction currently visible from project docs and user-stated goals. If accepted, it should be reconciled with:

- `docs/CURRENT_GENERATOR_STATE.*`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/GAME_GENERATION_CAPABILITY_MATRIX.md`
- `docs/ROADMAP_TO_FULL_GENERATOR.md`
- semantic/rule-pack/world/runtime docs

## 2. Core product goal

LLMGameCreator should become a controlled game-generation machine:

```text
user intent / preset / example
-> game profile
-> capability selection
-> contract-bound artifacts
-> validation and repair
-> human approval where needed
-> deterministic GamePackage assembly
-> runtime preview / export / playable loop
```

It is not a chat that writes a whole game in one prompt.

## 3. Documented feature families

### 3.1 Game profile and capability selection

Documented wishes:

- genre/tone/profile selection;
- runtime target selection;
- presentation mode selection;
- world topology selection;
- actor model selection;
- inventory/combat/progression/pathfinding/NPC behavior ids;
- feature bundle selection;
- future gaps and blocked combinations visible before generation.

Risk:
A vague request like "make RPG" must not advance without concrete ids.

### 3.2 GamePackage-centered architecture

Documented wishes:

- GamePackage as runtime source of truth;
- package validation;
- deterministic package assembly;
- no direct mutation from unapproved model/Lua output;
- reproducible generated game data;
- data/rule packs instead of bespoke C# per feature.

### 3.3 Runtime/player

Documented wishes:

- headless runtime;
- command/event-driven execution;
- runtime preview/simulator for diagnostics;
- future Unity/player frontend;
- runtime without LLM/provider dependency;
- save/load and runtime-owned mutable state;
- runtime smoke for generated packages.

### 3.4 Presentation modes

Documented wishes:

- top-down 2D;
- side-view 2D;
- isometric 2D/2.5D;
- tactical grid 2D;
- first-person grid with 2D textures;
- pseudo-3D billboard presentation;
- free first-person billboard presentation;
- UI-only/text RPG;
- map-and-panel RPG.

Important:
Pseudo-3D / first-person from 2D assets is documented as first-class future direction and should not require generated 3D models.

### 3.5 World topology and scale

Documented wishes:

- single finite map;
- multiple finite maps;
- region graph;
- overworld plus instances;
- room graph;
- node map;
- grid dungeon;
- first-person grid dungeon;
- sector/portal world;
- seamless chunks;
- infinite chunks;
- deterministic chunk generation by seed/rules/config;
- persistent runtime/save deltas;
- no huge precomputed tile dumps.

### 3.6 Pathfinding and reachability

Documented wishes:

- grid 4-way/8-way;
- waypoint graph;
- region graph pathing;
- chunk-aware pathfinding;
- tactical grid pathfinding;
- first-person grid movement;
- city-agent pathing;
- reachability reports;
- blocked path diagnostics.

Potential missing explicit wish:
Pathfinding should have anti-overfit consumer fixtures, for example caravan routing plus NPC city walking/patrol compatibility.

### 3.7 Actors, characters and NPCs

Documented wishes:

- player character card;
- party member card;
- companion card;
- NPC card;
- enemy/boss card;
- vendor card;
- faction leader card;
- party roster;
- actor model profile;
- schedule behavior;
- faction hooks;
- dialogue style;
- semantic memory hooks;
- quest hooks;
- approval gates for canon/player/companion/boss/faction-leader.

### 3.8 NPC behavior

Documented wishes:

- static NPCs;
- patrol;
- schedule-based behavior;
- faction-driven behavior;
- quest-state-driven behavior;
- dialogue-state-driven behavior;
- economy worker;
- colony citizen;
- hostile AI;
- companion AI;
- vendor AI.

Potential missing explicit wish:
NPC city walk / ambient city movement should be represented as a capability or future consumer fixture, not only implied by schedules/pathfinding.

### 3.9 Dialogue, quests and text generation

Documented wishes:

- dialogue graphs;
- choices;
- conditions/effects;
- quest graphs;
- stages/objectives/rewards;
- dialogue-combat;
- dialogue intent grammar;
- quest motif grammar;
- semantic-guided quest/dialogue/item/biome generation;
- repetition control;
- runtime choices execute through validated state/effects.

User-critical wish to document more explicitly:
Semantic packs should eventually support generation of dialogue text, quest text, descriptions, event text and other textual content both before runtime and, if explicitly selected, during runtime variability.

Recommended wording:
Runtime text variation should default to deterministic runtime-safe generation from compiled semantic catalogs, phrase plans, templates, morphology packs, seeds and approved rule packs. Live runtime LLM/RAG should be a later optional quarantined mode, not core runtime authority.

### 3.10 Items, inventory, equipment and economy

Documented wishes:

- list/slot/grid/weight/volume inventory;
- party shared inventory;
- per-character inventory;
- equipment paper-doll;
- quickbar;
- containers/stash;
- item packs;
- equipment packs;
- loot;
- vendors;
- economy;
- crafting;
- transactions;
- durability/charges;
- requirements/costs/outputs.

### 3.11 Combat and progression

Documented wishes:

- no-combat mode;
- realtime combat;
- turn-based combat;
- tactical grid combat;
- active pause;
- blobber party turn-based combat;
- JRPG rows;
- action RPG light;
- dialogue combat;
- auto battler;
- encounter card-based combat;
- combat spaces: same map, separate arena, tactical instance, abstract encounter, first-person party frontline;
- abilities/status effects;
- XP/levels;
- skill-use progression;
- perk/class trees;
- trainer-based progression;
- reputation/faction favor;
- research tree;
- equipment-based progression;
- card unlocks;
- relationship progression;
- colony tech progression.

### 3.12 Rule-pack and Lua direction

Documented wishes:

- C# owns authority, validation, promotion, runtime;
- LLM owns drafts only;
- Lua owns deterministic IR/config/data only;
- Lua sandbox;
- manifest-declared modules;
- formula/effect/action DSL;
- deterministic rule packs;
- game-specific behavior through data/rules instead of C# per feature.

### 3.13 Semantic pack / RAG direction

Documented wishes:

- core semantic pack;
- genre semantic pack;
- project semantic pack;
- imported candidate pack;
- LLM candidate pack;
- candidate quarantine;
- compiled semantic catalog;
- semantic relation validator;
- semantic candidate review workflow;
- semantic import adapters;
- semantic diff/comparison;
- semantic-guided quest/dialogue/item/biome generation;
- local RAG authoring helper;
- curated meaning, not massive semantic dumps.

Potential missing explicit wish:
A dedicated `semantic_runtime_variation_v1` / `runtime_text_variation_v1` planning contract may be needed to separate:
- offline authoring generation;
- deterministic runtime variation;
- optional quarantined live LLM/RAG proposals.

### 3.14 Assets and audio

Documented wishes:

- asset request queue;
- tileset requests;
- portrait requests;
- UI icon requests;
- sound effect requests;
- music import/loop metadata;
- ComfyUI/Fooocus integration;
- Suno/manual music import path;
- review/import workflow;
- deterministic asset mapping;
- fallback assets when generation/import is missing.

Important:
Provider calls are editor pipeline, not runtime.

### 3.15 Unity/export

Documented wishes:

- package-to-Unity mapping;
- tile/chunk renderer;
- 2D assets unfolded into 2.5D/3D-like presentation;
- input/controller;
- interaction UI;
- dialogue UI;
- inventory UI;
- save/load;
- streaming/chunk loading;
- performance budget;
- no Unity-specific game logic hardcoded per generated game.

### 3.16 Advanced runtime primitives

Documented wishes:

- NPC perception;
- line of sight;
- projectile/ranged combat;
- cover;
- group AI;
- siege/destruction;
- building;
- economy simulation;
- political/faction simulation;
- weather/environment interaction;
- relationship/NSFW scene gating;
- procedural settlement/city systems;
- near/far simulation levels.

### 3.17 Authoring UX

Documented wishes:

- semantic pack editor;
- rule pack editor;
- preset editor;
- generator profile editor;
- asset request/review UI;
- validation dashboard;
- package comparison;
- authoring assistant/RAG integration;
- user can control generation without editing raw files.

## 4. Process wishes

Documented or user-stated wishes:

- fewer manual checks;
- Codex should self-review before final report;
- hotfix count should fall;
- goals should not grow without bound;
- manual verification only when automation/simulation cannot prove the result;
- no endless docs without playable gain;
- no Runtime Preview as final engine;
- no LLM runtime dependency as core path;
- no C# slice per mechanic.

## 5. Likely gaps to add to documentation

These may need explicit docs/policy entries:

1. `semantic_runtime_variation_v1`
   - deterministic runtime text/dialogue/event variation from compiled semantic catalog.
   - no live LLM by default.
   - output must be runtime-safe and save-compatible.

2. Optional live LLM/RAG runtime proposal mode
   - only if user explicitly wants it later.
   - off by default.
   - quarantined, validated, non-authoritative.
   - cannot directly mutate live gameplay state.

3. Pathfinding reusable foundation
   - conformance matrix with at least two consumer shapes:
     - caravan routing;
     - NPC city walk/patrol synthetic fixture.

4. NPC ambient behavior capability
   - city walks;
   - schedules;
   - patrol;
   - work routes;
   - far-world abstract simulation.

5. Goal batching / milestone-pack policy
   - plan 3-5 goals together;
   - execute bounded goals separately;
   - rare product vertical gates;
   - mandatory self-review evidence table.

6. Runtime exploitability/operability concern
   - final system must be usable, not only green on artifacts.
   - add usability/operability smoke before alpha:
     - generate package;
     - run preview/player;
     - recover from failed generation;
     - inspect diagnostics;
     - accept/reject/repair artifacts;
     - save/load.
