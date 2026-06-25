# LLMGameCreator Architecture Strategy And Boundaries

Status: proposed strategic documentation after Goal 003.

## Purpose

This document fixes the strategic direction of LLMGameCreator so future product goals do not drift into endless infrastructure, Runtime Preview polish, or C# feature slicing.

The product target is not "clone Heroes 3, MM7, RimWorld, Factorio, Mount and Blade, Workers & Resources, Kenshi, and Skyrim." The target is a generator architecture that can create the user's own games using similar classes of mechanics through reusable primitives, semantic packs, rule packs, procedural generators, and controlled runtime extension.

## Core Principle

LLMGameCreator must be a game-generation combiner, not a pile of bespoke games.

The durable architecture is:

- C# core owns safe primitives, runtime state, validation, serialization, deterministic execution, and stable APIs.
- Data/rule packs declare game-specific behavior.
- Lua-like scripting declares rules through a restricted API, not arbitrary engine mutation.
- Procedural generators expand seeds, semantics, archetypes, formulas, and rule packs into concrete game packages.
- LLM helps author and refine packs offline, but is not required at runtime.
- Runtime Preview proves generated packages run, but must not become the target engine.
- Unity/runtime export is the later playable target for richer visuals and larger worlds.

## What Belongs In C#

C# should contain primitives that are difficult or unsafe to express as untrusted data:

- runtime state containers;
- map/region/chunk data structures;
- inventory/equipment/resource containers;
- quest/objective state;
- faction/reputation state;
- combat state and damage application;
- perception/line-of-sight/projectile primitives;
- status effect application;
- scheduling/time simulation;
- pathfinding primitives;
- save/load;
- deterministic random services;
- validation and diagnostics;
- sandbox boundaries;
- asset import/export boundaries;
- Unity/package export contracts.

C# should not grow for every generated quest, inventory variant, reward rule, NPC reaction, or content pattern.

## What Belongs In Data Or Rule Packs

Data/rule packs should define:

- triggers;
- conditions;
- formulas;
- actions;
- rewards;
- objective templates;
- quest motifs;
- dialogue intent templates;
- loot tables;
- spawn tables;
- biome/region tables;
- faction relation tables;
- NPC archetypes;
- encounter templates;
- status definitions;
- interaction affordances;
- UI panel declarations;
- asset requests.

The ideal future state is that most new gameplay variants are data/rule-pack changes, not new C# slices.

## What Belongs In Lua-like Scripts

Lua-like scripts should be a declarative authoring language over a restricted API.

Allowed direction:

```lua
define_effect("infect_target", {
  trigger = "on_attack_hit",
  condition = "attacker.has_tag('carrier')",
  action = "defender.add_status('infected')",
  chance = "0.15 + attacker.virulence * 0.02"
})
```

Dangerous direction:

```lua
while true do
  mutate_runtime_directly()
end
```

Lua should declare game rules. It should not freely access the filesystem, network, UI controls, threads, arbitrary C# objects, or unrestricted runtime mutation.

## What Belongs In LLM

LLM is an offline authoring helper:

- propose semantic terms;
- propose quest/dialogue/event patterns;
- generate draft rule packs;
- generate faction/world archetypes;
- generate style/tone packs;
- explain validation errors;
- suggest balancing changes;
- expand lore from curated inputs.

LLM must not be required during runtime gameplay.

## Runtime Preview Boundary

Runtime Preview is a proving ground.

It should prove:

- generated package can load;
- generated runtime can start;
- movement/interactions work;
- runtime-owned goal progress changes;
- reward/completion state is visible;
- generated variations remain deterministic.

It should not become:

- a polished final game engine;
- a full map editor;
- a replacement for Unity/runtime export;
- a permanent place for every future gameplay feature.

## Development Process Rule

One product goal should have one manual verification gate at the end.

Intermediate manual stops are allowed only for:

- UI crash;
- threading bug;
- runtime/schema contract break;
- unsafe behavior;
- broad redesign need;
- blocker that cannot be proven headlessly.

Every goal should prefer:

- headless scenario harnesses;
- deterministic sidecars;
- focused tests;
- product smoke route;
- final check-all;
- final manual verification only.

## Current Strategic State After Goal 003

Goal 003 proves the first version of extension spine:

- data/rule-pack extensible triggers;
- conditions;
- formulas as validated declarations;
- actions;
- rewards;
- quest objectives;
- extension proof through inventory objective and additional reward.

Still requiring C# primitives:

- new runtime command families;
- new mutable runtime state containers;
- new formula evaluator semantics;
- new rendering/UI interaction modes;
- Lua execution boundary;
- providers;
- media generation;
- Unity export/runtime integration.

## Non-Negotiable Direction

Do not return to infrastructure-only development.

Every future goal must either:

- make generated games more playable;
- make generated games more extensible without C#;
- make generation more controllable;
- make runtime/export more real;
- make validation stronger enough to reduce manual work.

If a goal only adds reports, UI polish, or docs without reducing risk or moving toward playable generated games, it should be rejected.
