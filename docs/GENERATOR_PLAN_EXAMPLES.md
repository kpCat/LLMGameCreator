# Generator Plan Examples

Status: seed examples  
Version: 0.1  
Related files:

```text
generator-library/atlas/generator_plan_map.json
generator-library/atlas/feature_bundle_map.json
generator-library/atlas/game_profile_negotiation_map.json
generator-library/atlas/examples/might_and_magic_profile_plan.example.json
generator-library/atlas/examples/anno_city_builder_profile_plan.example.json
generator-library/atlas/examples/narrative_political_rpg_profile_plan.example.json
```

## Purpose

These examples show how an approved or candidate `game_profile_v1` can become a small, ordered, validated generator plan.

They are intentionally data-only. They do not execute Lua, mutate `GamePackage`, generate C# or Unity code, build runtime databases, or create media assets.

## Core pattern

```text
game_profile_v1
  -> selected feature bundles
  -> target artifact contracts
  -> ordered generator steps
  -> validation gates
  -> staged artifacts
  -> approval / dry-run / export pipeline
```

A generator plan is not the game. It is a controlled proposal for producing named artifacts.

## Included examples

### Might-and-Magic-like party RPG

File:

```text
generator-library/atlas/examples/might_and_magic_profile_plan.example.json
```

This example focuses on pseudo-3D exploration, party roster, paper-doll inventory, factions, hybrid combat, runtime DB planning and Unity IR export planning.

Important lesson: combat formulas, semantic world data and runtime export planning are separate steps with separate contracts and validation gates.

### Anno-like city builder with conquest

File:

```text
generator-library/atlas/examples/anno_city_builder_profile_plan.example.json
```

This example focuses on production chains, population needs, trade, factions, conquest pressure, headless balance checks and runtime DB planning.

Important lesson: the model should not simulate the live economy at runtime. It proposes rules and formulas; deterministic runtime systems execute and validate them.

### Narrative political RPG / visual novel

File:

```text
generator-library/atlas/examples/narrative_political_rpg_profile_plan.example.json
```

This example focuses on faction routes, relationship flags, dialogue graphs, scene metadata, media request packs and optional adult/NSFW content overlays.

Important lesson: adult/NSFW support is an explicit optional content overlay, not hidden behavior inside ordinary dialogue or media records.

## What these examples prevent

Without examples, the system can drift into vague instructions such as:

```text
Make the whole game.
Generate all data.
Create the Unity export.
Add the adult route.
Balance everything.
```

The intended architecture is different:

```text
Generate this one artifact contract.
Use this exact context pack.
Preserve these enums.
Validate through these gates.
Repair only this failure.
Stage the result.
Promote only after the required approval/dry-run.
```

## How to extend

New examples should be added when a reference profile or genre blend introduces a new planning pattern.

Good future examples:

```text
survival_sandbox_profile_plan.example.json
factorio_automation_profile_plan.example.json
tactical_rpg_profile_plan.example.json
alien_non_human_perception_profile_plan.example.json
colony_sim_profile_plan.example.json
```

Do not create examples that require immediate C# or Unity implementation. Examples should first prove that the profile can be decomposed into feature bundles, artifact contracts and validation gates.
