# Runtime DB and Unity Export Map

Status: seed architecture document  
Version: 0.1  
Related files:

```text
generator-library/atlas/runtime_db_and_unity_export_map.json
generator-library/atlas/artifact_contracts.json
generator-library/atlas/capability_atlas.json
generator-library/atlas/reference_profiles.json
generator-library/atlas/validation_pipeline.json
generator-library/atlas/library_growth_pipeline.json
```

## Purpose

This document defines how approved LLMGameCreator artifacts should eventually become fast runtime data and Unity runtime inputs.

The goal is to avoid a final Unity player that reads thousands or hundreds of thousands of loose generated JSON files. Authoring data may remain modular and human-readable, but the player runtime should consume compiled, indexed and validated data.

## Core rule

```text
Authoring artifacts -> validation -> runtime DB build plan -> runtime.db + Unity IR + asset index -> Unity runtime shell
```

The generator library may help produce build plans and IR, but C# build/export services own compilation, indexing, validation, migration and export.

## Storage split

The runtime architecture should separate immutable compiled content from mutable player state:

```text
runtime.db   read-only compiled content shipped with the game/build
save.db      mutable player/world state created and updated during play
asset_index  generated/imported asset references and Unity bindings
unity_ir     scene/UI/input/camera/prefab/audio/VFX binding descriptors
```

This prevents game content updates from being tangled with player saves.

## runtime.db

`runtime.db` is a compiled read-optimized product. It is not the only source of truth for design history.

Typical content:

```text
- items, equipment, inventories, loot tables;
- entities, components, maps, regions, chunks;
- quests, factions, reputation tracks;
- dialogue graphs, text templates, morphology lexemes;
- rules, formulas, abilities, statuses, combat profiles;
- semantic traits, material reactions, content overlays;
- asset refs, prefab bindings, UI screens, audio/VFX events;
- build metadata and source artifact hashes.
```

## save.db

`save.db` or an equivalent save overlay stores mutable runtime state:

```text
- world variables;
- entity state;
- NPC memory;
- relationships and reputation state;
- inventory/equipment state;
- quest state;
- chunk deltas;
- visited locations;
- runtime event log.
```

## Unity IR

Unity IR is not Unity C# source code. It is a data contract that tells a Unity runtime shell how to bind approved data to existing runtime features.

Typical Unity IR groups:

```text
runtime shell       runtime target, scene mode, camera mode, input mode
prefab binding      content ids -> prefabs/addressables
UI                  screens, panels, widgets, paper-doll slots, dialogue layouts
audio/VFX           audio events, music states, VFX events, animation requests
world view          tilemap, isometric, pseudo-3D, first-person grid, region map
```

Generated C# should be a late and narrow export option, not the default mechanism for every feature.

## Asset index

The asset index maps approved content ids to generated/imported assets and Unity bindings.

It should support:

```text
- imported asset packs;
- generated images;
- generated music/audio/voice;
- future animation requests;
- adult/NSFW overlay tags when explicitly enabled;
- platform/export filters;
- variants and fallback assets;
- Unity addressables or prefab references.
```

## Export pipeline

The standard export pipeline is:

```text
collect approved sources
  -> normalize contracts
  -> build runtime DB plan
  -> compile runtime.db
  -> compile Unity IR and asset index
  -> package Unity runtime input bundle
  -> Unity runtime smoke test
```

The pipeline must reject raw LLM output, unapproved overlays, unresolved references, invalid schema versions and Unity bindings that target unavailable runtime shell features.

## Profile examples

### Might-and-Magic-like party RPG

Important runtime pieces:

```text
pseudo-3D / first-person grid target
party roster
paper-doll inventory
equipment slots
spells/skills/combat formulas
dialogues/quests/factions
runtime.db + save.db split
Unity UI + prefab + world view IR
```

### Anno-like city builder with conquest

Important runtime pieces:

```text
city-builder/isometric target
production chains
population needs
trade, diplomacy, region control
large UI data
simulation smoke tests
runtime DB indexes for resources/buildings/routes
```

### Narrative political RPG / The-Last-Sovereign-like profile

Important runtime pieces:

```text
visual novel / RPG target
dialogue graph
relationship/faction/reputation state
route and narrative flags
optional adult/NSFW overlay
image/audio/scene request packs
content filters per export target
```

## Non-goals

This seed does not implement SQLite, Unity export, Unity runtime code, C# generators, asset generation or save migration.

It only defines the map that future implementation should follow.
