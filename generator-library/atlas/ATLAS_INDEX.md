# Generator Library Atlas Index

Status: seed navigation document  
Version: 0.1

This index is the local navigation entry point for the `generator-library/atlas` seed architecture.
It does not execute Lua, run generators, mutate `GamePackage`, compile runtime databases or generate Unity code.
It explains what each atlas file is for and how the files connect into one bounded planning pipeline.

## Core flow

```text
User discussion
  -> game profile negotiation
  -> reference profile / feature bundle selection
  -> artifact contract selection
  -> generator plan proposal
  -> prompt context pack selection
  -> model workflow execution
  -> validation pipeline
  -> staged artifacts
  -> approved runtime DB / Unity export plan
  -> future deterministic C# build/export services
```

The atlas is intentionally data-first. It should reduce ambiguity before C#, Lua execution, Unity export or Codex-sized implementation work starts.

## Current atlas files

### `capability_atlas.schema.json`

JSON schema seed for the static Capability Atlas. It defines the approximate expected structure for atlas data.

Use it when:

```text
- adding new capability domains;
- adding new capabilities;
- validating basic atlas shape;
- preparing future C# import/validation.
```

### `capability_atlas.json`

Top-level capability registry seed.

It describes:

```text
- runtime targets;
- capability domains;
- reusable capabilities;
- feature/capability dependencies;
- artifact outputs;
- validators;
- content tag principles;
- optional adult/NSFW as an explicit content layer, not a hidden default.
```

Read this first when deciding whether a new idea is already covered by existing architecture.

### `reference_profiles.json`

Reference game profiles used to test atlas flexibility.

Examples include:

```text
- Might-and-Magic-like party RPG;
- Anno-like city builder with conquest;
- narrative political RPG / visual novel;
- automation / factory-like game;
- survival sandbox;
- tactical RPG;
- dungeon crawler / roguelike;
- colony sim;
- alien / non-human perception game.
```

Optional adult/NSFW and horror/gore overlays live here as profile-selectable layers.

### `game_form_factor_taxonomy.json`

Machine-readable taxonomy for presentation modes, view models and asset modes.

Important ids include:

```text
presentation_mode/first_person_grid_2d_textures
presentation_mode/pseudo3d_billboard
asset_mode/2d_billboards
asset_mode/2d_wall_textures
```

Use it when future tasks must choose how a game is presented before generating artifacts.

### `game_system_variant_taxonomy.json`

Machine-readable taxonomy for world topologies, chunk streaming, actor models, inventory/equipment, interactions, combat, progression, pathfinding and NPC behavior.

Use it when replacing vague genre labels with concrete variant ids such as:

```text
world_topology/seamless_chunks
actor_model/party_blob
inventory_model/grid_inventory
combat_model/blobber_party_turn_based
pathfinding/first_person_grid_movement
```

### `character_actor_contracts.json`

Future data-only character card, party roster and actor model profile contracts. These contracts do not generate C# classes.

### `world_topology_contracts.json`

Future data-only contracts for finite maps, region graphs, first-person grid dungeons, seamless/infinite chunks and runtime chunk deltas.

### `interaction_combat_progression_contracts.json`

Future data-only contracts for interactions, requirements, effects, combat, encounters, abilities, statuses, progression, inventory and equipment.

### `artifact_contracts.json`

Registry of named artifact contracts.

The rule is:

```text
A generator does not produce arbitrary data.
A generator produces a named artifact contract.
```

Examples:

```text
game_profile_v1
feature_bundle_v1
generator_plan_v1
semantic_pack_v1
text_pack_v1
morphology_pack_v1
dialogue_pack_v1
formula_pack_v1
content_overlay_pack_v1
runtime_db_build_plan_v1
unity_ir_v1
asset_request_pack_v1
audio_request_pack_v1
animation_request_pack_v1
```

### `validation_pipeline.json`

Validation-level and promotion-gate registry.

It defines:

```text
- validation levels 0-6;
- artifact states;
- outcomes: pass, warn, repairable_fail, blocked_fail, review_required;
- repair loop policies;
- sampling review policies;
- model role boundaries;
- content overlay validation rules.
```

Use this whenever deciding whether an artifact can be promoted automatically, repaired, blocked, staged or sent for human review.

### `library_growth_pipeline.json`

Rules for safely expanding the generator library.

It defines how missing capabilities become proposals before they become files:

```text
capability gap
  -> proposal
  -> staged spec
  -> strict generation prompt
  -> generated files
  -> static checks
  -> examples checked
  -> registry preview
  -> approval
  -> activation
```

Use this instead of letting a model invent untracked Lua/C#/Unity glue.

### `runtime_db_and_unity_export_map.json`

Map from approved artifacts to runtime deliverables.

It separates:

```text
- authoring source;
- immutable compiled runtime content;
- mutable save state;
- asset index;
- Unity IR;
- build manifest.
```

Core principle:

```text
Unity runtime consumes compiled runtime data, Unity IR, asset indexes and save overlays.
It should not consume an unbounded pile of loose generated JSON files.
```

### `model_workflow_roles_and_prompts.json`

Role map for local/remote models and prompt families.

It separates:

```text
- large designer model;
- small fast batch model;
- repair model;
- context selector;
- validator/scorer assistant;
- deterministic validator service.
```

Use this to decide which model should do a task and how strict the prompt must be.

### `prompt_context_pack_map.json`

Context-selection map.

Core rule:

```text
Context is a selected artifact, not a full project dump.
```

It defines:

```text
- context budget classes;
- source types;
- context templates;
- selection pipeline;
- priority rules;
- anti-patterns;
- trace shape for prompt context packs.
```

### `game_profile_negotiation_map.json`

Question/decision map for turning user discussion into a compact `game_profile_v1`.

It helps decide:

```text
- player fantasy;
- reference inspirations;
- tone/rating/overlays;
- runtime targets;
- view mode;
- world scale;
- combat mode;
- party/single/city/faction role;
- inventory/equipment style;
- media generation scope;
- persistence/export strategy.
```

### `feature_bundle_map.json`

Bridge between game profiles and implementation planning.

It maps profile requirements into bundles, and bundles into:

```text
- capabilities;
- artifact contracts;
- validators;
- prompt context templates;
- runtime targets;
- Unity IR groups;
- runtime DB groups;
- future module gaps.
```

### `generator_plan_map.json`

Rules for turning selected feature bundles into ordered generator steps.

It defines:

```text
- plan states;
- step states;
- plan shape;
- step shape;
- build flow;
- step kinds;
- repair policies;
- anti-patterns.
```

A generator plan is an execution proposal, not trusted execution.

### `examples/*.example.json`

Concrete example plans.

Current examples:

```text
examples/might_and_magic_profile_plan.example.json
examples/anno_city_builder_profile_plan.example.json
examples/narrative_political_rpg_profile_plan.example.json
```

Use these to understand how profiles become selected bundles, target artifacts and ordered steps.

## Related documentation

```text
docs/ARCHITECTURE_CAPABILITY_ATLAS_AND_RUNTIME_EXPORT.md
docs/CAPABILITY_ATLAS.md
docs/ARTIFACT_CONTRACTS.md
docs/VALIDATION_PIPELINE_ATLAS.md
docs/LIBRARY_GROWTH_PIPELINE.md
docs/RUNTIME_DB_AND_UNITY_EXPORT_MAP.md
docs/MODEL_WORKFLOW_ROLES_AND_PROMPTS.md
docs/PROMPT_CONTEXT_PACK_MAP.md
docs/GAME_PROFILE_NEGOTIATION_MAP.md
docs/FEATURE_BUNDLE_MAP.md
docs/GENERATOR_PLAN_MAP.md
docs/GENERATOR_PLAN_EXAMPLES.md
docs/GAME_FORM_FACTORS_AND_PRESENTATION_MODES.md
docs/GAME_SYSTEM_VARIANT_TAXONOMY.md
docs/CHARACTER_CARD_AND_ACTOR_MODEL_CONTRACTS.md
docs/WORLD_TOPOLOGY_AND_CHUNKING_CONTRACTS.md
docs/INTERACTION_COMBAT_PROGRESSION_VARIANTS.md
```

## Reading order

For architecture review:

```text
1. docs/ARCHITECTURE_CAPABILITY_ATLAS_AND_RUNTIME_EXPORT.md
2. generator-library/atlas/ATLAS_INDEX.md
3. docs/GENERATOR_LIBRARY_ATLAS_OVERVIEW.md
4. docs/CAPABILITY_ATLAS.md
5. docs/ARTIFACT_CONTRACTS.md
6. docs/VALIDATION_PIPELINE_ATLAS.md
7. docs/GENERATOR_PLAN_EXAMPLES.md
```

For implementing future C# import/validation:

```text
1. artifact_contracts.json
2. validation_pipeline.json
3. capability_atlas.json
4. feature_bundle_map.json
5. generator_plan_map.json
6. runtime_db_and_unity_export_map.json
```

For adding a new game profile:

```text
1. game_profile_negotiation_map.json
2. reference_profiles.json
3. feature_bundle_map.json
4. generator_plan_map.json
5. prompt_context_pack_map.json
```

For adding a new Lua generator module later:

```text
1. library_growth_pipeline.json
2. artifact_contracts.json
3. validation_pipeline.json
4. feature_bundle_map.json
5. generator_plan_map.json
```

## Hard boundaries

The atlas must not become a dumping ground for generated content.

Do not put here:

```text
- thousands of generated items;
- full dialogue dumps;
- full generated maps;
- generated images/audio;
- per-game save state;
- runtime logs;
- arbitrary C# or Unity code;
- unsafe Lua experiments.
```

The atlas should describe reusable contracts, capabilities, profiles, bundles, plans, validation gates and export maps.
