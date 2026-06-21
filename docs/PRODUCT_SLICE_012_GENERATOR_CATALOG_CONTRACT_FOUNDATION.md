# Product Slice 012: Generator Catalog Contract Foundation

## Goal

Add the first machine-readable Generator Catalog contract layer.

Slice 011 added `GameBlueprint` and capability compatibility validation. Slice 012 adds the next layer: a registry of generator modules that can produce content/contracts and declare their requirements, outputs, capabilities, maturity and LLM/determinism profile.

This slice does not execute generator plugins. It only models, validates and plans generator modules.

## Core concepts

```text
GeneratorModuleManifest
GeneratorCatalog
GeneratorCatalogValidator
GeneratorPlanningResult
GeneratorPlanResolver
```

The program should be able to answer:

```text
Which generator modules exist?
Which input contracts do they require?
Which output contracts do they produce?
Which capabilities do they require/provide?
Which modules use LLM?
Which modules are deterministic?
Which modules are current/planned/unsupported?
Which generator modules are useful for a GameBlueprint?
```

## Initial current generator modules

Minimum current generator manifests:

```text
generator.strict_llm.game_profile_v1
generator.strict_llm.region_pack_v1
generator.strict_llm.scene_pack_v1
generator.strict_llm.npc_pack_v1
generator.strict_llm.quest_pack_v1
generator.strict_llm.dialogue_pack_v1
generator.strict_llm.mechanics_pack_v1
generator.strict_llm.encounter_pack_v1
generator.strict_llm.item_pack_v1
generator.package.assembly_v1
generator.package.activation_v1
generator.runtime_preview.generated_map_markers_v1
```

## Initial planned generator modules

Minimum planned/future generator manifests:

```text
generator.semantic.world_model_seed_v1
generator.procedural.quest_templates_v1
generator.procedural.dialogue_realizer_v1
generator.world.lazy_region_cache_v1
generator.events.offscreen_scheduler_v1
generator.imported_map.osm_like_classifier_v1
generator.population.households_v1
generator.schedule.daily_life_v1
```

## Contract id style

Use stable ASCII ids.

Examples:

```text
game_profile_v1
region_pack_v1
scene_pack_v1
npc_pack_v1
quest_pack_v1
dialogue_pack_v1
mechanics_pack_v1
encounter_pack_v1
item_pack_v1
package.assembled_game_package
runtime_preview.generated_map_markers
semantic.world_model_seed
procedural.quest_templates
```

## Non-goals

Do not implement:
- dynamic plugin loading;
- runtime execution;
- new LLM provider calls;
- semantic world model;
- imported map pipeline;
- procedural quest engine;
- lazy world generation;
- UI wizard;
- package schema changes.

This is a contract/catalog foundation only.
