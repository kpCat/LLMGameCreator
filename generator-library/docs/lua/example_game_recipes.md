# Example game recipes

Batch 021 adds compact recipe examples for the generator library. These files are not generated games, not runtime scripts and not pipeline execution. They are deterministic recipe/config metadata that shows how earlier modules can be selected together for future planning, validation and UI work.

## What a recipe is

A recipe is a small Lua file returning a table with:

- project/game idea metadata;
- selected capabilities;
- selected generator module ids;
- compact generator plan steps;
- example configs;
- expected artifact descriptors;
- validation checklist and expected diagnostics shape;
- context pack hints for selecting compact LLM context.

Recipe files do not load modules, do not execute a pipeline, do not generate full content databases, do not integrate Unity and do not generate C# source code.

## Recipe files

### Dark fantasy village RPG slice

`examples/dark_fantasy_village_recipe.lua` demonstrates a small RPG slice with a village world outline, NPC cast, dialogue/quest hooks, optional combat/status references, HUD/quest journal UI references and validation modules.

### Factorio-like alchemy game

`examples/factorio_like_alchemy_recipe.lua` demonstrates automation planning with recipe graph, machine catalog, conveyor/logistics metadata, power network metadata, build menu UI and artifact manifest planning.

### Open world RPG

`examples/open_world_rpg_recipe.lua` demonstrates world blueprint, region graph, biome catalog, roads/reachability, NPC schedules/factions, quests/dialogue/combat/progression, minimap/global map UI and validation references.

### Frontier city-builder

`examples/city_builder_frontier_recipe.lua` demonstrates citizen needs, job system, building catalog, service coverage, economy hooks, simulation tick metadata, build menu UI and validation references.

### Space quest adventure

`examples/space_quest_recipe.lua` demonstrates a compact multi-location space adventure with NPC/dialogue, quest stages, inventory/items, progression/reputation/faction favor, UI references and Unity target IR references as metadata only.

## Generator plan representation

Each recipe uses `generator_plan.steps[]` as plain metadata. A step has an id, module id, optional config reference and optional dependencies. The plan is suitable for future orchestration planning, but this batch does not run it.

## Expected artifacts

`expected_artifacts[]` describes planned artifact ids, kinds and producing steps. These are logical artifact descriptors, not files written by the recipe.

## Validation plan

`validation_plan` lists validation modules and checks that a future host could run after generation. Diagnostics are expected as JSON-serializable tables containing severity, code, message and target fields.

## Context pack hints

`context_pack_hints` gives metadata for selecting concise context for an LLM: purpose, token budget, module ids to include and exclusions. The recipe does not read files, summarize files, fetch external data or call an LLM.

## Boundaries

This batch does not execute modules, execute generated plans, mutate game packages, integrate Unity, generate C# source files, compile anything, call shell commands, access filesystem/network/process APIs or extend the current roadmap beyond Batch 021.
