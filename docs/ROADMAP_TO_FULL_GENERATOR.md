# Roadmap To Full Generator

Status: proposed strategic roadmap after Goal 003.

## Source Of Truth

Before choosing the next milestone, read `docs/CURRENT_GENERATOR_STATE.md` and `docs/CURRENT_GENERATOR_STATE.json`.

The current state handoff, active manual gate and strategy reset override any older roadmap sequence that suggests starting a feature slice early.

Current locked baseline: `M4.1` remains the last completed milestone until `docs/CURRENT_GENERATOR_STATE.json` explicitly records a later milestone.

## Definition Of Full Generator

The full generator is not a universal magic engine that can generate every possible game without new primitives.

The target is:

- user defines genre, tone, rules, semantic packs, asset direction, and gameplay families;
- combiner generates a coherent game package;
- most game-specific behavior comes from data/rule packs;
- C# core changes only when a new primitive family is required;
- runtime/export can play the generated game;
- Unity or another final runtime can consume the generated package;
- LLM is optional authoring assistance, not runtime dependency.

## Stage A: Proving Core Generation

Status: mostly done by Goals 001-003.

Purpose:

- prove deterministic procedural generation;
- prove formula/effect/action rules;
- prove generated package MVP;
- prove runtime-backed microgame loop;
- prove configurable seed/preset variation;
- prove extension through rule packs.

Exit criteria:

- multiple generated variants;
- runtime-owned progress/reward/completion;
- extension proof without bespoke gameplay C#;
- automated scenario harness;
- one final manual verification per goal.

## Stage B: Playable Generated Microgame

Purpose:

- make generated games understandable and playable for 5-10 minutes;
- add region travel;
- add goal HUD;
- add inventory/reward visibility;
- add interaction hints;
- add simple quest journal;
- keep Runtime Preview as proving ground only.

Likely goals:

- regional navigation and variable maps;
- player-facing HUD and journal;
- inventory/equipment visibility;
- simple dialogue interaction;
- multi-step quest acceptance.

Exit criteria:

- user can play a generated microgame without reading debug logs;
- at least 3 seed/preset variants are distinct;
- automated harness covers core loop;
- one manual check confirms playability.

## Stage C: Rule-Pack Driven Gameplay Families

Purpose:

- make mechanics extensible through data/rule packs;
- avoid C# per feature.

Gameplay families:

- inventory/equipment;
- jobs/work actions;
- crafting;
- trading/economy;
- reputation/factions;
- status effects;
- combat;
- stealth/theft;
- relationship/social interactions;
- adult content tags and gated scenes;
- weather/environment effects;
- simple building/ownership.

Exit criteria:

- each family has primitives in C#;
- concrete variants are data/rule-pack authored;
- invalid packs are rejected;
- headless scenario tests prove behavior;
- generated package can combine several families.

## Stage D: Procedural World Structure

Purpose:

- move from small generated maps to larger worlds.

Needed systems:

- regions;
- chunks;
- biome distribution;
- travel graph;
- points of interest;
- spawn tables;
- faction territories;
- simulation levels: near detailed, far abstract;
- save/load snapshots.

Exit criteria:

- world can be larger than loaded play area;
- generated regions connect;
- far-world state advances abstractly;
- no full-world per-frame simulation;
- deterministic save/load remains stable.

## Stage E: Content Generation At Scale

Purpose:

- produce varied quests, dialogue patterns, NPC archetypes, loot, regions, and events without LLM per instance.

Needed systems:

- quest grammar;
- dialogue intent grammar;
- event grammar;
- relationship/reputation-driven reactions;
- semantic-guided item/biome/location generation;
- conflict and dependency validation;
- repetition control.

Exit criteria:

- hundreds of generated instances from compact packs;
- repetition is controlled;
- semantic conflicts are caught;
- LLM authoring remains optional and offline.

## Stage F: Asset Pipeline

Purpose:

- connect generated package to visual/audio assets.

Needed systems:

- asset request queue;
- tile set requests;
- portrait requests;
- UI icon requests;
- sound effect requests;
- music import/loop metadata;
- ComfyUI/Fooocus integration;
- Suno/manual music import path;
- review/import workflow;
- deterministic asset mapping.

Exit criteria:

- generated game can request missing assets;
- user can approve/import assets;
- assets are mapped to semantic roles;
- runtime has fallback assets if generation is missing.

## Stage G: Unity Runtime Export

Purpose:

- move from proving ground to real playable runtime.

Needed systems:

- package-to-Unity mapping;
- tile/chunk renderer;
- 2D assets unfolded into 2.5D/3D presentation;
- input/controller;
- interaction UI;
- dialogue UI;
- inventory UI;
- save/load;
- streaming/chunk loading;
- performance budget.

Exit criteria:

- one generated game exports and runs in Unity;
- runtime supports at least one complete gameplay family set;
- performance remains acceptable on target hardware;
- no Runtime Preview-only dependency.

## Stage H: Advanced Runtime Primitives

Purpose:

- support richer game ambitions.

Primitive families:

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
- procedural settlement/city systems.

Exit criteria:

- new primitive families are C# core;
- game-specific behavior is still rule-pack/data-driven;
- simulation has near/far levels;
- performance budgets are enforced.

## Stage I: Authoring UX

Purpose:

- let user control generation without editing raw files.

Needed systems:

- semantic pack editor;
- rule pack editor;
- preset editor;
- generator profile editor;
- asset request/review UI;
- validation dashboard;
- package comparison;
- authoring assistant/RAG integration.

Exit criteria:

- user can create/modify a project through UI;
- validation explains issues;
- generation remains reproducible;
- raw files still remain source-controlled.

## Stage J: Alpha Definition

Alpha is reached when:

- generated game can be configured by seed/preset/semantic/rule packs;
- generated game has a playable loop of 15-30 minutes;
- at least 3 game styles can be created from different packs;
- most content is generated deterministically;
- runtime/export is playable;
- extension proof exists for several mechanics without C# changes;
- manual checks are reduced to final acceptance per milestone;
- LLM is optional and offline.

## Stage K: Beyond Alpha

Beyond alpha:

- richer AI;
- larger worlds;
- more content families;
- better asset generation;
- Unity polish;
- modding tools;
- more advanced semantic authoring;
- balancing tools;
- scenario simulation;
- performance tuning;
- multiplayer only if explicitly chosen much later.

## Main Risk

The project fails if it becomes:

- C# slice per mechanic;
- Runtime Preview as the final engine;
- semantic dump instead of curated meaning;
- LLM runtime dependency;
- manual verification after every small step;
- endless documentation without playable gain.

The project succeeds if each stage increases either:

- playable generated output;
- extensibility without C#;
- validation automation;
- export/runtime reality;
- user control over generated games.
