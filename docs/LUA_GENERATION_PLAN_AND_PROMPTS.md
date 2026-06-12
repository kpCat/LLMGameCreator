# LLMGameCreator — Lua Module Library Generation Plan

Version: 0.1  
Purpose: подготовить отдельную, управляемую генерацию Lua-модулей, документации и manifests для AI Game Builder / LLMGameCreator, не засоряя основной диалог разработки C#-приложения.

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

LLM должна быть:

```text
planner + designer + configurator + orchestrator + reviewer
```

а не:

```text
bulk content printer
```

Lua-модули нужны как библиотека повторно используемых генераторов и runtime/codegen building blocks.

---

## 1. Почему генерировать batch’ами, а не одной простынёй

Запрещено просить:

```text
"Сгенерируй 100 Lua файлов"
```

Проблемы такого подхода:

- модель начнёт снижать качество;
- появятся несовместимые контракты;
- документация станет поверхностной;
- tests/validation будут пропущены;
- manifests будут разъезжаться с кодом;
- сложно review’ить и пушить в репозиторий;
- повышается риск архитектурного мусора.

Правильный подход:

```text
1 batch = 2–4 Lua файла + 1–2 docs + manifest/index + мини-примеры
```

Каждый batch должен быть самодостаточным, проверяемым и пригодным для commit.

---

## 2. Требование к новому диалогу: генерировать файлами

В новом диалоге нужно просить не просто текст, а **готовый ZIP artifact**.

Каждый batch должен возвращать:

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
BATCH_REPORT.md
```

Если текущая среда не умеет создавать файлы/ZIP, fallback:

1. вывести список файлов;
2. дать каждый файл отдельным fenced block;
3. не продолжать следующий batch, пока пользователь не подтвердит, что файлы сохранены.

Но основной ожидаемый режим — ZIP artifact.

---

## 3. Master prompt для нового диалога

Скопируй этот текст в новый диалог перед генерацией Batch 001.

```text
Ты помогаешь мне подготовить Lua module library для проекта AI Game Builder / LLMGameCreator.

Главная идея:
- LLM не должна руками генерировать огромные карты, тысячи диалогов и весь контент.
- LLM должна обсуждать игру с пользователем, фиксировать лор/правила/ограничения, выбирать готовые generator/capability modules, заполнять их configs и запускать generators в правильной последовательности.
- Lua-модули должны быть заранее подготовленными, документированными, валидируемыми и расширяемыми.
- Позже эти Lua-модули будут использоваться C#-программой как generator/capability library.
- В будущем часть Lua-модулей может генерировать intermediate representation для Unity runtime/UI/codegen.
- Прямо сейчас нужны Lua-модули, manifests, docs, examples и tests маленькими batch’ами.
- Не интегрируй это в C#-приложение. Только файлы Lua library.

Формат ответа:
- Не выводи просто код в чат, если доступно создание файлов.
- Создай ZIP artifact для каждого batch.
- ZIP должен называться: lua_batch_XXX_<short_name>.zip
- Внутри ZIP должны быть готовые файлы, документация, manifests, examples/tests и BATCH_REPORT.md.
- Если создание ZIP недоступно, выведи файлы отдельными code blocks и остановись.

Строгие Lua-ограничения:
1. Lua 5.4-compatible.
2. Без внешних зависимостей.
3. Не использовать io, os, debug, package, load, loadfile, dofile, require внешних файлов, network, file system.
4. Код должен быть deterministic.
5. Не использовать math.random напрямую.
6. Если нужен random, использовать ctx.rng или core/rng.lua.
7. Модуль не должен писать в глобальное окружение.
8. Каждый модуль возвращает table.
9. Каждый модуль имеет manifest.
10. Каждый модуль имеет validate_config(config).
11. Каждый generator module имеет generate(input, ctx).
12. Ошибки возвращать через diagnostics, а не падать через error, кроме programmer errors.
13. Output data должны быть JSON-serializable: strings, numbers, booleans, arrays, dictionaries. No functions in output.
14. Не генерировать огромные словари/реплики/таблицы тайлов. Только infrastructure + compact examples.
15. Не использовать TODO вместо реализации.
16. Каждый модуль обязан иметь документацию.

Единый module contract:

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
  -- returns:
  -- {
  --   ok = true/false,
  --   data = {},
  --   diagnostics = {},
  --   artifacts = {}
  -- }
end

return M

Diagnostics format:
{
  severity = "error" | "warning" | "info",
  code = "module.problem_code",
  message = "Human-readable message",
  target = "optional/path/or/id"
}

ID rules:
- lowercase slash ids:
  - world/chunk/cursed_forest
  - entity/npc/elder
  - quest/investigate_road
- Coordinates are 0-based.
- Chunk coordinates are integer chunk grid coordinates.
- Local coordinates inside chunk are 0-based.

Документация для каждого модуля:
1. Purpose.
2. When to use.
3. When not to use.
4. Manifest summary.
5. Input schema explained.
6. Config schema explained.
7. Output schema explained.
8. Example config.
9. Example input.
10. Example output.
11. LLM prompting hints.
12. Validation rules.
13. Extension points.
14. Runtime target notes.
15. Unity/codegen notes if relevant.

BATCH_REPORT.md должен содержать:
- files generated;
- contracts introduced;
- dependencies between files;
- how to validate manually;
- known limitations;
- next recommended batch;
- no broad claims that were not implemented.

Do not proceed to another batch until I explicitly ask.
```

---

## 4. Обязательные архитектурные режимы игры

Lua library должна заранее учитывать, что игра может быть:

### 4.1 Turn mode

```text
turn_mode = realtime | turn_based | mixed
```

Use cases:

```text
- fully turn-based roguelike/RPG;
- realtime exploration with turn-based combat;
- realtime map with turn-based dialogue choices;
- dialogue-combat hybrid;
- tactical combat separate from exploration;
- city-builder realtime simulation with paused planning mode;
- Factorio-like realtime automation;
- quest/adventure with no combat.
```

Core concepts:

```text
TimeModel
  - realtime
  - turn_based
  - mixed
  - paused_planning

TurnSystem
  - actor initiative
  - side turns
  - global turns
  - action points
  - cooldown ticks
  - status duration ticks

ModeTransition
  - exploration -> dialogue
  - exploration -> combat
  - dialogue -> combat
  - combat -> exploration
  - realtime -> pause
```

Every module that depends on time/combat/dialogue must declare supported turn modes.

### 4.2 Combat mode

```text
combat_mode = none | realtime | turn_based | tactical | dialogue_combat | hybrid
```

Dialogue-combat means:
- conversation options can act as attacks/defense/morale moves;
- facts/traits/statuses affect available choices;
- combat can end through persuasion/intimidation/trickery;
- “damage” can target HP, morale, trust, suspicion, focus, etc.

### 4.3 UI mode

```text
ui_mode = minimal_hud | rpg_hud | automation_hud | city_builder_ui | dialogue_focus | tactical_ui
```

Modules must output UI IR/config where relevant, not direct Unity objects.

### 4.4 World scale

```text
world_scale = single_map | multi_map | region | continent | planet | infinite_chunks
```

World modules must not assume finite small maps only.

---

## 5. Required top-level capability categories

The library must eventually cover these categories:

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

## 6. Batch roadmap

### Batch 001 — Core foundation

Files:

```text
lua/core/diagnostics.lua
lua/core/rng.lua
lua/core/schema.lua
docs/lua/core_foundation.md
manifests/core_foundation.manifest.json
tests/core_foundation_examples.lua
BATCH_REPORT.md
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
BATCH_REPORT.md
```

Must cover:
- lowercase slash id validation;
- position2d;
- chunk coord vs local coord;
- grid bounds;
- get/set cells;
- sparse overrides;
- neighborhood;
- facing direction;
- target cell in front of actor;
- adjacency modes: same_cell, cardinal_adjacent, diagonal_adjacent, radius;
- multiple target disambiguation.

### Batch 003 — Time, turn, mode model

Files:

```text
lua/core/time_model.lua
lua/core/turn_system.lua
lua/core/mode_transition.lua
docs/lua/time_turn_modes.md
manifests/time_turn.manifest.json
tests/time_turn_examples.lua
BATCH_REPORT.md
```

Must support:
- realtime;
- turn_based;
- mixed;
- exploration/combat/dialogue modes;
- action points;
- cooldown ticks;
- status duration ticks;
- dialogue-combat mode;
- mode transition rules.

### Batch 004 — Capability and generator module manifest helpers

Files:

```text
lua/generation/capability_manifest.lua
lua/generation/module_manifest.lua
lua/generation/generator_plan.lua
docs/lua/capability_and_module_manifests.md
manifests/generation_manifest.manifest.json
tests/generation_manifest_examples.lua
BATCH_REPORT.md
```

Must define:
- capability id;
- module id;
- inputs;
- outputs;
- config schema;
- supported runtime targets;
- supported time modes;
- supported combat modes;
- dependencies;
- incompatibilities;
- generator plan steps.

### Batch 005 — World blueprint

Files:

```text
lua/world/world_blueprint.lua
lua/world/region_graph.lua
lua/world/biome_catalog.lua
docs/lua/world_blueprint.md
manifests/world_blueprint.manifest.json
tests/world_blueprint_examples.lua
BATCH_REPORT.md
```

Must support:
- finite map;
- multi-map;
- region;
- chunked world;
- infinite seeded world;
- biomes;
- temperature/humidity/danger/resource tags;
- global map/minimap metadata;
- region connections.

### Batch 006 — Chunk/grid map generation

Files:

```text
lua/world/chunk_generator.lua
lua/world/tile_painter.lua
lua/world/landmark_placer.lua
docs/lua/chunk_generation.md
manifests/chunk_generation.manifest.json
tests/chunk_generation_examples.lua
BATCH_REPORT.md
```

Must support:
- chunk size config;
- seed;
- default tile;
- sparse overrides;
- landmarks;
- roads;
- blocked road case;
- walkability;
- minimap layer data;
- avoiding huge tile arrays when not needed.

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
BATCH_REPORT.md
```

Must support:
- simple paths;
- roads;
- barriers;
- gates;
- blocked road;
- bridge;
- ensure path from start to objective;
- reachable/unreachable diagnostics.

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
BATCH_REPORT.md
```

Must support:
- entity prototypes;
- entity instances;
- components: interactable, collidable, dialogue_source, inspectable, quest_target;
- targeting: facing cell, same cell, adjacent, multiple target disambiguation;
- output compatible with future runtime interaction.

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
BATCH_REPORT.md
```

Must support:
- static dialogue nodes;
- procedural dialogue from facts;
- quest-state dialogue;
- dialogue choices;
- dialogue-combat: morale/trust/suspicion/focus, choice effects, conditions.

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
BATCH_REPORT.md
```

Must support:
- quest stages;
- objective types;
- completion conditions;
- effects;
- stage transitions;
- abstract progress not only XP;
- quest from dialogue/interaction.

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
BATCH_REPORT.md
```

Must support:
- stackable items;
- quest items;
- equipment;
- durability;
- rarity;
- tags;
- inventory constraints;
- item description generation configs.

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
BATCH_REPORT.md
```

Must support:
- XP curves;
- skill trees;
- attribute formulas;
- abstract progress tracks: reputation, research, faction favor, suspicion, morale;
- formula IR, not raw unsafe code.

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
BATCH_REPORT.md
```

Must support:
- no combat;
- turn-based combat;
- dialogue-combat bridge;
- status effects;
- cooldowns;
- action points;
- ability definitions;
- damage/healing formula references.

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
BATCH_REPORT.md
```

Must support:
- static NPC;
- walking NPC;
- scheduled NPC;
- faction role;
- pathfinding config;
- dynamic obstacles;
- turn/realtime compatibility.

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
BATCH_REPORT.md
```

Must support:
- recipes;
- inputs/outputs;
- machines;
- conveyors;
- production graph;
- power;
- resource nodes;
- not full simulation yet, but configs and IR.

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
BATCH_REPORT.md
```

Must support:
- citizens;
- needs;
- jobs;
- buildings;
- services;
- zones;
- economy hooks;
- simulation tick mode.

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
BATCH_REPORT.md
```

Must support:
- minimap;
- global map;
- inventory;
- dialogue window;
- quest book;
- notes;
- item descriptions;
- status bars;
- stat bars;
- build menu;
- UI layout IR for Unity adapter.

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
BATCH_REPORT.md
```

Must support:
- abstract Unity scene plan;
- prefab slots;
- script/component glue plan;
- generated C# IR;
- compile/smoke validation metadata;
- not direct raw C# generation yet;
- codegen IR must be schema-validated.

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
BATCH_REPORT.md
```

Must support:
- map reachability;
- missing references;
- invalid quest conditions;
- interaction without target;
- module contract mismatch;
- capability dependencies.

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
BATCH_REPORT.md
```

Must support:
- generator plan ordering;
- dependencies;
- artifacts;
- validation results;
- context pack metadata for LLM;
- no actual C# integration.

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
BATCH_REPORT.md
```

Must demonstrate:
- small RPG slice;
- Factorio-like automation game;
- open world RPG;
- city-builder;
- space quest;
- what capabilities/modules are selected;
- what configs are produced.

---

## 7. Quality checklist for every generated batch

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
[ ] Batch report generated.
[ ] No huge hardcoded content dumps.
[ ] Contracts align with previous batches.
[ ] Batch does not proceed to next batch automatically.
```

---

## 8. Repository placement recommendation

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

This is a library asset until the C# app gets:
- SQLite design DB;
- capability registry;
- generator module registry;
- module importer;
- manifest validator;
- later Lua sandbox/executor.

---

## 9. When to continue C# Goal development

Do not wait for all 21 Lua batches before continuing C# work.

Recommended order:

```text
1. Generate Batch 001–004 first:
   - core foundation;
   - grid/ids;
   - time/turn modes;
   - capability/module manifests.

2. Then continue C# Goal 008:
   - SQLite Design DB;
   - Capability Registry;
   - Generator Module Registry;
   - importer for manifests;
   - UI page or service to inspect modules.

3. Continue Lua batches in parallel.

4. After enough modules exist:
   - import manifests;
   - let LLM select capabilities;
   - build generator plans;
   - later run trusted Lua modules.
```

Reason:
C# app can be built against manifests/contracts before all Lua implementation exists.

---

## 10. First task to send in the new dialogue

After the master prompt, send:

```text
Generate Batch 001 — Core foundation.

Create ZIP artifact:
lua_batch_001_core_foundation.zip

Files:
- lua/core/diagnostics.lua
- lua/core/rng.lua
- lua/core/schema.lua
- docs/lua/core_foundation.md
- manifests/core_foundation.manifest.json
- tests/core_foundation_examples.lua
- BATCH_REPORT.md

Follow all rules from the master prompt.
Do not proceed to Batch 002.
```
