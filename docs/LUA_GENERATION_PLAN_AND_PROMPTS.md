# LLMGameCreator — Lua Module Library Generation Plan

Version: 0.2
Purpose: подготовить отдельную, управляемую генерацию Lua-модулей, документации и manifests для AI Game Builder / LLMGameCreator без смешивания с C# runtime-интеграцией.

---

## 0. Главный принцип

LLMGameCreator не должен заставлять LLM вручную генерировать огромные карты, тысячи диалогов, сотни квестов и весь UI как сырой JSON.

Правильная модель:

```text
User discussion / lore / rules / mechanics
        ↓
Design Knowledge Base
        ↓
Capability selection
        ↓
Generator plan
        ↓
Lua modules / DSL modules / config generators
        ↓
Validated generated data / IR / Unity-facing artifacts
        ↓
Runtime Preview / Unity adapter
```

LLM должна быть `planner + designer + configurator + orchestrator + reviewer`, а не `bulk content printer`.

Lua-модули нужны как библиотека повторно используемых генераторов и runtime/codegen building blocks.

---

## 1. Batch generation rules

Правильный подход:

```text
1 batch = 2–4 Lua файла + 1–2 docs + manifest/index + мини-примеры
```

Каждый batch должен быть самодостаточным, проверяемым и пригодным для commit.

Каждый batch должен возвращать ZIP artifact:

```text
lua_batch_XXX_<name>.zip
```

Внутри ZIP:

```text
lua/
docs/
manifests/
examples/
tests/
BATCH_XXX_REPORT.md
```

`BATCH_REPORT.md` больше не использовать для новых batches. Отчёт должен иметь номер batch, например `BATCH_001_REPORT.md`, `BATCH_002_REPORT.md`, `BATCH_013_REPORT.md`.

Do not proceed to another batch until explicitly asked.

---

## 2. Strict Lua restrictions

1. Lua 5.4-compatible.
2. No external dependencies.
3. Do not use `io`, `os`, `debug`, `package`, `load`, `loadfile`, `dofile`, external `require`, network, file system.
4. Code must be deterministic.
5. Do not use `math.random` directly.
6. If random is needed, use `ctx.rng` or `core/rng.lua`.
7. Modules must not write to global environment.
8. Every module returns a table.
9. Every module has `manifest`.
10. Every module has `validate_config(config)`.
11. Every generator module has `generate(input, ctx)`.
12. Normal validation failures return diagnostics, not thrown errors.
13. Output data must be JSON-serializable: strings, numbers, booleans, arrays, dictionaries. No functions in output.
14. Do not generate huge dictionaries, dialogue dumps, or tile arrays. Generate infrastructure and compact examples.
15. Do not use TODO instead of implementation.
16. Every module must have documentation.

---

## 3. Lua module contract

```lua
local M = {}

M.manifest = {
  id = "category/module_name/v1",
  version = "0.1.0",
  category = "world",
  title = "Human readable title",
  purpose = "What this module does",
  capabilities = { "world.chunk.generate" },
  input_schema = {},
  output_schema = {},
  config_schema = {},
  deterministic = true,
  runtime_targets = { "debug", "unity2d" },
  unsafe_features = {}
}

function M.validate_config(config)
  -- returns ok:boolean, diagnostics:table
end

function M.generate(input, ctx)
  -- returns { ok = true/false, data = {}, diagnostics = {}, artifacts = {} }
end

return M
```

Diagnostics format:

```lua
{
  severity = "error" | "warning" | "info",
  code = "module.problem_code",
  message = "Human-readable message",
  target = "optional/path/or/id"
}
```

ID rules:

- module and content IDs use lowercase slash ids, for example `world/chunk/cursed_forest`, `entity/npc/elder`, `quest/investigate_road`;
- capabilities use lowercase dot ids, for example `world.chunk.generate`;
- coordinates are 0-based;
- chunk coordinates are integer chunk grid coordinates;
- local coordinates inside chunk are 0-based.

---

## 4. Canonical generator-library manifest contract

This section is authoritative for every `generator-library/manifests/*.manifest.json` file.

Batch manifest required fields:

```json
{
  "id": "batch/category_name/v1",
  "version": "0.1.0",
  "batch": "001",
  "title": "Human readable title",
  "purpose": "What this batch contributes",
  "files": [],
  "modules": [],
  "runtime_targets": [],
  "supported_time_modes": [],
  "supported_combat_modes": [],
  "unsafe_features": []
}
```

Module entries required fields:

```json
{
  "id": "category/module_name/v1",
  "path": "lua/category/module_name.lua",
  "category": "category",
  "capabilities": [],
  "depends_on": [],
  "runtime_targets": [],
  "supported_turn_modes": [],
  "supported_combat_modes": [],
  "deterministic": true,
  "unsafe_features": []
}
```

Do not use aliases in new manifests:

- `module_id`
- `file`
- `depends_on_contracts`
- `dependencies`
- `description` as a replacement for `purpose`
- `supported_runtime_targets` as a replacement for `runtime_targets`
- nested `supports` as a replacement for top-level canonical fields

Migration rules:

- replace module `module_id` with `id`;
- replace module `file` with `path`;
- replace `depends_on_contracts` or `dependencies` with `depends_on`;
- if a batch has `description` but no `purpose`, copy the same text to `purpose` and remove `description`;
- preserve all existing capabilities;
- preserve all file paths;
- preserve all module ids;
- do not invent missing capabilities;
- do not change Lua source modules for manifest-only stabilization.

The detailed contract is documented in `generator-library/docs/lua/MANIFEST_CONTRACT.md` and example schema in `generator-library/manifests/MANIFEST_CONTRACT.schema.example.json`.

---

## 5. Required architecture modes

### Turn/time mode

```text
turn_mode = realtime | turn_based | mixed | paused_planning
```

Use cases:

- fully turn-based roguelike/RPG;
- realtime exploration with turn-based combat;
- realtime map with turn-based dialogue choices;
- dialogue-combat hybrid;
- tactical combat separate from exploration;
- city-builder realtime simulation with paused planning mode;
- Factorio-like realtime automation;
- quest/adventure with no combat.

Every module that depends on time/combat/dialogue must declare supported turn modes.

### Combat mode

```text
combat_mode = none | realtime | turn_based | tactical | dialogue_combat | hybrid
```

Dialogue-combat means conversation options can act as attacks/defense/morale moves; facts/traits/statuses affect available choices; combat can end through persuasion/intimidation/trickery; and damage can target HP, morale, trust, suspicion, focus, etc.

### UI mode

```text
ui_mode = minimal_hud | rpg_hud | automation_hud | city_builder_ui | dialogue_focus | tactical_ui
```

Modules must output UI IR/config where relevant, not direct Unity objects.

### World scale

```text
world_scale = single_map | multi_map | region | continent | planet | infinite_chunks
```

World modules must not assume finite small maps only.

---

## 6. Required top-level capability categories

```text
core
world
biome
chunk
region
map
entity
npc
interaction
dialogue
quest
inventory
item
progression
stats
formula
ability
combat
status_effect
automation
recipe
machine
city_builder
simulation
pathfinding
schedule
faction
economy
ui
unity_ir
codegen_ir
validation
orchestration
```

---

## 7. Batch roadmap

### Batch 001 — Core foundation

Files:

```text
lua/core/diagnostics.lua
lua/core/rng.lua
lua/core/schema.lua
docs/lua/core_foundation.md
manifests/core_foundation.manifest.json
tests/core_foundation_examples.lua
BATCH_001_REPORT.md
```

Purpose:

- diagnostics helpers;
- deterministic RNG;
- lightweight schema validation;
- manual examples.

### Batch 002 — IDs, grid, coordinates

Files:

```text
lua/core/id.lua
lua/core/grid.lua
lua/core/coordinates.lua
docs/lua/core_grid_and_ids.md
manifests/core_grid.manifest.json
tests/core_grid_examples.lua
BATCH_002_REPORT.md
```

Must cover lowercase slash id validation, position2d, chunk/local coordinates, grid bounds, get/set cells, sparse overrides, neighborhood, facing direction, target cell in front of actor, adjacency modes and multiple target disambiguation.

### Batch 003 — Time, turn, mode model

Files:

```text
lua/core/time_model.lua
lua/core/turn_system.lua
lua/core/mode_transition.lua
docs/lua/time_turn_modes.md
manifests/time_turn.manifest.json
tests/time_turn_examples.lua
BATCH_003_REPORT.md
```

Must support realtime, turn_based, mixed, exploration/combat/dialogue modes, action points, cooldown ticks, status duration ticks, dialogue-combat mode and mode transition rules.

### Batch 004 — Capability and generator module manifest helpers

Files:

```text
lua/generation/capability_manifest.lua
lua/generation/module_manifest.lua
lua/generation/generator_plan.lua
docs/lua/capability_and_module_manifests.md
manifests/generation_manifest.manifest.json
tests/generation_manifest_examples.lua
BATCH_004_REPORT.md
```

Must define capability id, module id, inputs, outputs, config schema, supported runtime targets, supported time modes, supported combat modes, dependencies, incompatibilities and generator plan steps.

### Batch 005 — World blueprint

Files:

```text
lua/world/world_blueprint.lua
lua/world/region_graph.lua
lua/world/biome_catalog.lua
docs/lua/world_blueprint.md
manifests/world_blueprint.manifest.json
tests/world_blueprint_examples.lua
BATCH_005_REPORT.md
```

Must support finite map, multi-map, region, chunked world, infinite seeded world, biomes, temperature/humidity/danger/resource tags, global map/minimap metadata and region connections.

### Batch 006 — Chunk/grid map generation

Files:

```text
lua/world/chunk_generator.lua
lua/world/tile_painter.lua
lua/world/landmark_placer.lua
docs/lua/chunk_generation.md
manifests/chunk_generation.manifest.json
tests/chunk_generation_examples.lua
BATCH_006_REPORT.md
```

Must support chunk size config, seed, default tile, sparse overrides, landmarks, roads, blocked road case, walkability, minimap layer data and avoiding huge tile arrays when not needed.

### Batch 007 — Roads, paths, barriers, reachability

Files:

```text
lua/world/path_carver.lua
lua/world/road_generator.lua
lua/world/barrier_generator.lua
lua/world/reachability.lua
docs/lua/world_paths_barriers_reachability.md
manifests/world_paths.manifest.json
tests/world_paths_examples.lua
BATCH_007_REPORT.md
```

Must support simple paths, roads, barriers, gates, blocked road, bridge, ensure path from start to objective and reachable/unreachable diagnostics.

### Batch 008 — Entity and interaction foundation

Files:

```text
lua/entity/entity_factory.lua
lua/interaction/targeting.lua
lua/interaction/inspect_object.lua
lua/interaction/talk_to_npc.lua
docs/lua/entities_interactions.md
manifests/entities_interactions.manifest.json
tests/entities_interactions_examples.lua
BATCH_008_REPORT.md
```

Must support entity prototypes, entity instances, components `interactable`, `collidable`, `dialogue_source`, `inspectable`, `quest_target`, targeting through facing/same/adjacent and multiple target disambiguation.

### Batch 009 — Dialogue generation foundation

Files:

```text
lua/dialogue/dialogue_schema.lua
lua/dialogue/procedural_npc_dialogue.lua
lua/dialogue/fact_based_dialogue.lua
lua/dialogue/dialogue_combat.lua
docs/lua/dialogue_generation.md
manifests/dialogue_generation.manifest.json
tests/dialogue_generation_examples.lua
BATCH_009_REPORT.md
```

Must support static dialogue nodes, procedural dialogue from facts, quest-state dialogue, dialogue choices and dialogue-combat morale/trust/suspicion/focus, choice effects and conditions.

### Batch 010 — Quest/progress foundation

Files:

```text
lua/quest/quest_schema.lua
lua/quest/simple_investigation.lua
lua/quest/fetch_quest.lua
lua/quest/location_discovery.lua
docs/lua/quest_generation.md
manifests/quest_generation.manifest.json
tests/quest_generation_examples.lua
BATCH_010_REPORT.md
```

Must support quest stages, objective types, completion conditions, effects, stage transitions, abstract progress and quest from dialogue/interaction.

### Batch 011 — Inventory/items/loot

Files:

```text
lua/item/item_schema.lua
lua/item/item_catalog_generator.lua
lua/item/loot_table_generator.lua
lua/item/inventory_rules.lua
docs/lua/items_inventory_loot.md
manifests/items_inventory.manifest.json
tests/items_inventory_examples.lua
BATCH_011_REPORT.md
```

Must support stackable items, quest items, equipment, durability, rarity, tags, inventory constraints and item description generation configs.

### Batch 012 — Stats, formulas, progression

Files:

```text
lua/formula/formula_schema.lua
lua/progression/xp_curve.lua
lua/progression/skill_tree_generator.lua
lua/progression/progress_track.lua
docs/lua/progression_formulas.md
manifests/progression_formulas.manifest.json
tests/progression_examples.lua
BATCH_012_REPORT.md
```

Must support XP curves, skill trees, attribute formulas, abstract progress tracks such as reputation/research/faction favor/suspicion/morale and formula IR, not raw unsafe code.

### Batch 013 — Combat/status/abilities

Files:

```text
lua/combat/combat_schema.lua
lua/combat/turn_based_combat.lua
lua/combat/status_effects.lua
lua/ability/ability_catalog_generator.lua
docs/lua/combat_status_abilities.md
manifests/combat_abilities.manifest.json
tests/combat_examples.lua
BATCH_013_REPORT.md
```

Must support no combat, turn-based combat, dialogue-combat bridge, status effects, cooldowns, action points, ability definitions and damage/healing formula references.

### Batch 014 — NPC, schedule, pathfinding

Files:

```text
lua/npc/npc_archetype_generator.lua
lua/npc/schedule_generator.lua
lua/pathfinding/pathfinding_config.lua
lua/faction/faction_model.lua
docs/lua/npc_schedule_pathfinding.md
manifests/npc_pathfinding.manifest.json
tests/npc_pathfinding_examples.lua
BATCH_014_REPORT.md
```

Must support static NPC, walking NPC, scheduled NPC, faction role, pathfinding config, dynamic obstacles and turn/realtime compatibility.

### Batch 015 — Automation / Factorio-like systems

Files:

```text
lua/automation/recipe_graph.lua
lua/automation/machine_catalog.lua
lua/automation/conveyor_grid.lua
lua/automation/power_network.lua
docs/lua/automation_factorio_like.md
manifests/automation.manifest.json
tests/automation_examples.lua
BATCH_015_REPORT.md
```

Must support recipes, inputs/outputs, machines, conveyors, production graph, power, resource nodes, and configs/IR instead of full simulation.

### Batch 016 — City-builder / simulation basics

Files:

```text
lua/simulation/citizen_needs.lua
lua/simulation/job_system_config.lua
lua/simulation/building_catalog.lua
lua/simulation/service_coverage.lua
docs/lua/city_builder_simulation.md
manifests/city_builder.manifest.json
tests/city_builder_examples.lua
BATCH_016_REPORT.md
```

Must support citizens, needs, jobs, buildings, services, zones, economy hooks and simulation tick mode.

### Batch 017 — UI IR

Files:

```text
lua/ui/ui_schema.lua
lua/ui/hud_layout.lua
lua/ui/minimap_config.lua
lua/ui/inventory_ui.lua
lua/ui/quest_journal_ui.lua
docs/lua/ui_ir.md
manifests/ui_ir.manifest.json
tests/ui_ir_examples.lua
BATCH_017_REPORT.md
```

Must support minimap, global map, inventory, dialogue window, quest book, notes, item descriptions, status bars, stat bars, build menu and UI layout IR for Unity adapter.

### Batch 018 — Unity target IR and C# codegen IR

Files:

```text
lua/unity/unity_runtime_plan.lua
lua/unity/unity_scene_ir.lua
lua/unity/unity_ui_ir.lua
lua/unity/unity_csharp_codegen_ir.lua
docs/lua/unity_target_ir.md
manifests/unity_ir.manifest.json
tests/unity_ir_examples.lua
BATCH_018_REPORT.md
```

Must support abstract Unity scene plan, prefab slots, script/component glue plan, generated C# IR, compile/smoke validation metadata and schema-validated codegen IR.

### Batch 019 — Validation modules

Files:

```text
lua/validation/world_validation.lua
lua/validation/quest_validation.lua
lua/validation/interaction_validation.lua
lua/validation/module_contract_validation.lua
docs/lua/validation_modules.md
manifests/validation.manifest.json
tests/validation_examples.lua
BATCH_019_REPORT.md
```

Must support map reachability, missing references, invalid quest conditions, interaction without target, module contract mismatch and capability dependencies.

### Batch 020 — Orchestration and artifact manifest

Files:

```text
lua/generation/dependency_sort.lua
lua/generation/artifact_manifest.lua
lua/generation/pipeline_runner_plan.lua
lua/generation/context_pack_plan.lua
docs/lua/generator_orchestration.md
manifests/orchestration.manifest.json
tests/orchestration_examples.lua
BATCH_020_REPORT.md
```

Must support generator plan ordering, dependencies, artifacts, validation results and context pack metadata for LLM, with no actual C# integration.

### Batch 021 — Example game recipes

Files:

```text
examples/dark_fantasy_village_recipe.lua
examples/factorio_like_alchemy_recipe.lua
examples/open_world_rpg_recipe.lua
examples/city_builder_frontier_recipe.lua
examples/space_quest_recipe.lua
docs/lua/example_game_recipes.md
manifests/example_recipes.manifest.json
BATCH_021_REPORT.md
```

Must demonstrate a small RPG slice, Factorio-like automation game, open world RPG, city-builder, space quest, selected capabilities/modules and produced configs.

---

## 8. Quality checklist for every generated batch

```text
[ ] ZIP artifact provided.
[ ] File paths correct.
[ ] No external dependencies.
[ ] No forbidden APIs: io/os/debug/package/load/loadfile/dofile.
[ ] No global writes.
[ ] Every module returns table.
[ ] Every module has manifest.
[ ] Every module has validate_config.
[ ] Generator modules have generate.
[ ] Diagnostics returned, not thrown for normal validation failures.
[ ] Outputs are JSON-serializable.
[ ] Docs generated.
[ ] Examples/tests generated.
[ ] Numbered batch report generated: BATCH_XXX_REPORT.md.
[ ] Manifest uses canonical fields from section 4.
[ ] Manifest does not use module_id/file/depends_on_contracts/dependencies aliases.
[ ] No huge hardcoded content dumps.
[ ] Contracts align with previous batches.
[ ] Batch does not proceed to next batch automatically.
```

---

## 9. Repository placement recommendation

Suggested repo layout:

```text
generator-library/
  lua/
    core/
    world/
    entity/
    interaction/
    dialogue/
    quest/
    item/
    progression/
    combat/
    npc/
    automation/
    simulation/
    ui/
    unity/
    validation/
    generation/
  docs/
    lua/
  manifests/
  examples/
  tests/
  README.md
```

Do not immediately mix generated Lua into current runtime execution folder.

This is a library asset until the C# app gets SQLite Design DB, capability registry, generator module registry, importer for manifests, UI/service to inspect modules, and later Lua sandbox/executor.

---

## 10. Continuation state after manifest stabilization

Do not generate a new batch during manifest stabilization.

After this patch is applied and reviewed, the exact safe batch to continue from is:

```text
Batch 013 — Combat/status/abilities
```

Reason: Batch 012 was the last completed ZIP artifact before the manifest stabilization stop; Batch 013 should be regenerated/issued only after canonical manifest rules are in force.
