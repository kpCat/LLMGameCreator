# Game Profile Negotiation Map

Status: seed architecture document  
Version: 0.1  
Related files:

```text
generator-library/atlas/game_profile_negotiation_map.json
generator-library/atlas/capability_atlas.json
generator-library/atlas/reference_profiles.json
generator-library/atlas/artifact_contracts.json
generator-library/atlas/prompt_context_pack_map.json
```

## Purpose

The Game Profile Negotiation Map defines how LLMGameCreator should turn user discussion into a concrete `game_profile_v1` planning artifact.

The goal is to avoid starting heavy generation from vague genre labels like `RPG`, `city builder`, `visual novel`, or `Might and Magic like`. Those labels are useful inspiration, but they are not enough for capability selection, validation, runtime DB planning or Unity export.

A negotiated profile answers the decisions that materially affect architecture:

```text
runtime target
view mode
world scale
player structure
combat mode
dialogue model
inventory/equipment model
progression model
content overlays
media generation scope
persistence model
validation focus
```

## Core rule

```text
User discussion -> negotiated game profile -> capability selection -> generator plan -> artifact contracts -> validation/export
```

The profile does not execute Lua, mutate `GamePackage`, generate Unity code or trust model output. It is a compact planning contract.

## Why this matters

Without profile negotiation, every new idea becomes an open-ended architecture task:

```text
"I want a Might and Magic style game" -> unclear runtime, unclear inventory, unclear combat, unclear DB/export plan
```

With profile negotiation:

```text
Might-and-Magic-like profile
  -> pseudo3d + first_person_grid + unity3d + headless
  -> party player structure
  -> grid inventory + paper doll + party inventory
  -> hybrid realtime/turn-based combat
  -> region/infinite chunk world option
  -> runtime.db + save.db + Unity IR requirements
```

Then later generation can stay modular instead of becoming an endless custom-code pile.

## Content overlays

Adult/NSFW support is not a separate genre and not a hidden default.

It is an optional project/profile overlay that may apply across many genres when explicitly enabled:

```text
party RPG
visual novel
political RPG
sandbox
survival
city builder
relationship-heavy RPG
```

When enabled, it must remain:

```text
tagged
filterable
export-aware
separate from core mechanics
traceable through artifact contracts
```

This lets the same core mechanics support different export variants, such as private creator builds, safer public builds, platform-specific builds or adult-enabled builds.

## Decision groups

The seed map starts with these groups:

```text
game_identity
runtime_view_and_platform
world_and_generation
characters_party_and_progression
combat_interaction_and_dialogue
content_overlays_and_media
```

Each decision records:

```text
id
question
answer type
allowed values or examples
mapping targets
```

This keeps future UI and model workflows focused on decisions instead of free-form interrogation.

## Reference negotiation profiles

The seed map includes initial reference negotiation profiles:

```text
Might-and-Magic-like Party RPG
Anno-like City Builder with Conquest
Narrative Political RPG / Visual Novel
```

These are not final templates. They are starting presets that can be mixed, overridden or split into multiple variants during planning.

## Profile variants

If the user has not decided between incompatible directions, the system should create profile variants instead of guessing.

Example:

```text
Variant A: pseudo-3D party RPG with hybrid combat
Variant B: isometric tactical party RPG
Variant C: visual novel political RPG with RPG stats
```

A variant may be validated, compared and approved before generator plans are created.

## Validation focus

The negotiated profile should produce validation focus items. Examples:

```text
party/equipment consistency
combat formula bounds
quest reachability
production graph validity
content overlay export filters
runtime DB lookup performance
Unity adapter dry-run
```

This tells the validation pipeline what matters most for the selected profile.

## Non-goals

This seed does not implement profile UI, C# import, model prompts, Lua modules or Unity adapters.

It only defines the data map that those future components should follow.

## Next likely implementation step

When this architecture seed is ready for code, the smallest useful C# step would be an importer/viewer for atlas negotiation data, not a full game creator.

A safe first implementation would only:

```text
read generator-library/atlas/game_profile_negotiation_map.json
show decision groups and reference negotiation profiles
validate basic shape and ids
produce a draft game_profile_v1 JSON artifact
not mutate GamePackage
not execute Lua
not run Unity export
```
