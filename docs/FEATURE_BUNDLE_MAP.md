# Feature Bundle Map

Status: seed architecture document  
Version: 0.1  
Related files:

```text
generator-library/atlas/feature_bundle_map.json
generator-library/atlas/capability_atlas.json
generator-library/atlas/reference_profiles.json
generator-library/atlas/artifact_contracts.json
generator-library/atlas/game_profile_negotiation_map.json
generator-library/atlas/runtime_db_and_unity_export_map.json
```

## Purpose

Feature bundles are the bridge between a negotiated game profile and real generation/runtime/export work.

The point is to avoid this trap:

```text
new idea -> new JSON -> new Lua -> new C# -> new Unity glue -> another one-off system
```

Instead, the planning path should look like this:

```text
game_profile_v1
  -> selected feature bundles
  -> required capabilities
  -> artifact contracts
  -> validators
  -> generator plan seed
  -> runtime DB / Unity export plan
```

A feature bundle is not the final code. It is a compact planning unit that says:

```text
- what gameplay/runtime feature is being requested;
- what capabilities it depends on;
- what artifact contracts it produces or consumes;
- what validators must run;
- what runtime targets are expected;
- what prompt context templates are useful;
- which parts may need future Lua/C#/Unity implementation.
```

## Core rule

```text
Game Profile selects Feature Bundles.
Feature Bundles select Capabilities and Artifact Contracts.
Artifact Contracts select Validators and Export Paths.
```

This gives the project a scalable middle layer. A new idea does not immediately force a custom subsystem. It first becomes a bundle, then either maps to existing contracts or creates a controlled library-growth proposal.

## Why this matters

The project needs flexibility across very different game types:

```text
- Might-and-Magic-like party RPG;
- Anno-like city builder with conquest;
- The-Last-Sovereign-like narrative/political RPG;
- Factorio-like automation;
- survival sandbox;
- visual novel / route-heavy game;
- tactical RPG;
- pseudo-3D / first-person-grid dungeon crawler;
- optional adult/NSFW overlays;
- optional horror/gore overlays;
- future image/audio/animation generation.
```

Without feature bundles, each of those becomes a separate architecture. With bundles, they become combinations of reusable planning units.

## Feature bundle shape

Each bundle should declare:

```text
id
title
domain
purpose
requires
provides
artifact_contracts
validators
runtime_targets
prompt_context_templates
validation_focus
```

Optional fields:

```text
incompatible_with
recommended_with
content_overlays
unity_ir_groups
runtime_db_groups
future_module_gaps
notes
```

## Bundle categories

The seed map uses these categories:

```text
core_planning
gameplay_foundation
runtime_export
genre_systems
content_overlays
media_generation
```

This is intentionally broad. The map is not trying to implement all mechanics now. It creates a vocabulary for selecting and validating future work.

## Content overlays

Adult/NSFW support is modeled as a feature bundle and content overlay, not as a hidden property of a genre.

That means it can apply to many profiles when explicitly enabled:

```text
party RPG
visual novel
political RPG
sandbox
survival
city builder
dialogue-heavy game
future image/audio/animation generation
```

But it must remain:

```text
explicitly enabled
tagged
filterable
export-aware
separate from core mechanics
```

A game should not break if the adult overlay is disabled or filtered for a target export.

## Example: Might-and-Magic-like profile

A party RPG profile may select:

```text
feature_bundle/might_and_magic_like_party_rpg/v1
feature_bundle/world_region_chunk_generation/v1
feature_bundle/party_roster_and_progression/v1
feature_bundle/inventory_paper_doll_grid/v1
feature_bundle/combat_realtime_turn_hybrid/v1
feature_bundle/dialogue_choice_graph/v1
feature_bundle/faction_reputation/v1
feature_bundle/quest_multi_stage/v1
feature_bundle/runtime_db_build_plan/v1
feature_bundle/unity_ir_runtime_shell/v1
```

Optional additions:

```text
feature_bundle/dialogue_free_text_nlu/v1
feature_bundle/media_request_generation/v1
feature_bundle/content_overlay_optional_adult_nsfw/v1
feature_bundle/content_overlay_horror_gore/v1
```

## Example: Anno-like profile

A city builder profile may select:

```text
feature_bundle/anno1404_like_city_builder_conquest/v1
feature_bundle/city_builder_production_conquest/v1
feature_bundle/faction_reputation/v1
feature_bundle/runtime_db_build_plan/v1
feature_bundle/unity_ir_runtime_shell/v1
```

Validation focus:

```text
production graph validity
resource loops
population needs
trade routes
region control
conquest refs
headless economy smoke tests
```

## Example: The-Last-Sovereign-like profile

A narrative political RPG profile may select:

```text
feature_bundle/the_last_sovereign_like_narrative_political_rpg/v1
feature_bundle/dialogue_choice_graph/v1
feature_bundle/narrative_relationship_routes/v1
feature_bundle/faction_reputation/v1
feature_bundle/quest_multi_stage/v1
feature_bundle/runtime_db_build_plan/v1
```

Optional additions:

```text
feature_bundle/content_overlay_optional_adult_nsfw/v1
feature_bundle/media_request_generation/v1
feature_bundle/unity_ir_runtime_shell/v1
```

The important boundary: mature/adult content is a project/profile overlay. It may be central to a particular game, but it still remains tagged and filterable at the artifact/export level.

## Selection flow

The feature selection flow is:

```text
1. Read approved or candidate game profile.
2. Match required feature bundles.
3. Match optional feature bundles and overlays.
4. Resolve dependencies and incompatibilities.
5. Produce capability gap report if something is missing.
6. Build generator plan seed and validation focus plan.
7. Review high-impact selections.
```

This keeps the work bounded. The system does not need to implement all future modules immediately. It only needs to know which bundle is selected, what it requires, and what gaps are still unresolved.

## Validation

Feature bundle validation should check:

```text
- required fields exist;
- ids are canonical and unique;
- dependencies resolve or create a gap proposal;
- artifact contracts are known;
- validators are known;
- runtime targets are compatible;
- content overlays are explicit;
- runtime-impacting or overlay-policy changes require human approval.
```

## Current non-goals

This seed does not:

```text
- import bundles into C# Design DB;
- implement generator plan selection;
- execute Lua;
- generate Unity code;
- compile runtime.db;
- validate real GamePackage content;
- add runtime mechanics.
```

Those can be added later as small steps after the atlas data stabilizes.

## Practical next step

After this file is committed, the next useful seed is a generator-plan map:

```text
generator-library/atlas/generator_plan_map.json
docs/GENERATOR_PLAN_MAP.md
```

That map should define how selected feature bundles become ordered generator steps, with declared inputs, output artifact contracts, validation gates, repair loops and staging rules.
