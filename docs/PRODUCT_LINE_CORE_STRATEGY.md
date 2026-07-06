# Product Line Core Strategy

Status: proposed source-of-truth strategy addition.

## Purpose

LLMGameCreator should be developed as a configurable game product-line combiner,
not as a one-off generated game and not as an unconstrained prompt-to-game
system.

The long-term dream scope is broad, but the near-term alpha must prove a narrow,
expansion-safe kernel. Narrowing the alpha is acceptable only when it preserves
future expansion through stable module seams.

## Core Principle

Do not narrow by hardcoding a single game.

Narrow by selecting one thin vertical slice through the same architecture that
will later support more game families:

```text
FeatureModule selection
→ GamePackage materialization
→ validation
→ canonical runtime playthrough
→ save/load/replay
→ player adapter presentation
→ one-click automated report
```

A goal that improves projection, evidence, dashboards or candidate selection is
only product-progress if it reduces the distance to this path.

## Product-Line Building Blocks

### FeatureModule

A selectable gameplay/world/visual capability.

Every product-ready `FeatureModule` should declare:

```text
id
title
category
dependencies
conflicts
requiredSchemaSections
requiredRuntimePrimitives
requiredValidationRules
requiredSaveLoadPolicy
requiredPlayerAdapterSurface
generatorInputs
authoringControls
goldenPackages
smokePlaythroughs
knownLimitations
futureExpansionNotes
```

Examples:

```text
feature.world.grid_chunks
feature.quest.objective_chain
feature.dialogue.intent_templates
feature.inventory.basic
feature.equipment.basic
feature.crafting.recipes
feature.combat.turn_based_encounter
feature.faction.reputation
feature.weather.daynight_modifiers
feature.settlement.slot_building
feature.visual.tile_partpack
```

### RuntimePrimitive

A deterministic command/effect/state primitive executed by canonical runtime
services. New gameplay must not live only in UI projection or Unity-specific
scripts.

Examples:

```text
runtime.command.start_quest
runtime.command.advance_objective
runtime.command.add_item
runtime.command.craft_recipe
runtime.command.start_encounter
runtime.command.use_ability
runtime.command.change_reputation
runtime.command.build_structure
runtime.command.tick_daynight
```

### SemanticPack

A compiled semantic catalog, not a runtime LLM conversation.

It may include:

```text
facts
relations
traits
factions
locations
species
roles
dialogue intents
phrase grammars
event grammars
naming catalogs
content safety/rating metadata
```

### VisualPartPack

A deterministic visual part system for non-provider generation.

It may include:

```text
body plans
tile parts
palette/material rules
composition rules
rating metadata
fallback assets
distinctness metrics
Unity/player binding metadata
```

### WorldSourceAdapter

A source-specific import/generation adapter normalized into package-safe world
facts.

Examples:

```text
procedural_seed_world
finite_hand_authored_map
huge_sparse_world
infinite_chunk_window
offline_geoworld_source
space_sector_source
```

### PlayerAdapter

A presentation layer over canonical package and runtime state.

Examples:

```text
player_adapter.topdown_2d
player_adapter.isometric_2d
player_adapter.grid_2_5d
player_adapter.first_person_grid
player_adapter.free_3d_later
player_adapter.third_person_later
```

Player adapters must not become separate game implementations.

## Near-Term Strategic Pivot

Current candidate work has reached WinForms-visible selected candidate pipelines.
The urgent product risk is continued `projectionOnly=true`.

After the current candidate operator work, the next strategic milestone should be:

```text
Canonical Runtime Candidate Playthrough Matrix
```

Required proof:

1. read selected/generated candidate packages from disk;
2. validate package schema and cross references;
3. create canonical runtime state;
4. execute a deterministic scripted playthrough;
5. cover at least movement/interaction/dialogue/quest/inventory/economy/combat
   where the candidate supports those features;
6. save, load and replay state;
7. write a state-hash chain and command transcript;
8. expose the result in WinForms;
9. let Unity/player consume the canonical transcript/state summary rather than
   projection-local truth;
10. keep manual Unity inspection optional for normal verification.

## Anti-Goals

The following are not near-term alpha blockers unless explicitly selected as a
milestone:

- free third-person action controller;
- free first-person action controller;
- real provider-generated art;
- live geodata/network ingestion;
- arbitrary Lua execution;
- runtime LLM dialogue;
- production-grade NPC intelligence;
- planet/space/station scale simulation;
- fully general construction/destruction.

They remain valid future lanes only when implemented through the product-line
building blocks above.

## Goal Acceptance Policy

A product goal should state which gap it closes:

```text
projectionOnly reduction
canonicalRuntime coverage
saveLoadReplay coverage
featureModule contract coverage
playerAdapter consumption
automation tier coverage
user-facing workflow
```

A goal that only adds reports, dashboards or routing must explain which future
product gate it unblocks and must not claim alpha progress by itself.
