# Typed Lua scripting

Lua нужен для гибкости, процедурной генерации и поведения мира. Но Lua не должен стать дырой, через которую LLM обходит архитектуру.

## Два слоя Lua

### Prototype Lua

Создаёт декларативные данные.

```lua
data:extend({
  {
    type = "tile",
    id = "tile/grass",
    name = "Трава",
    walkable = true,
    assetId = "asset/tile/grass"
  }
})
```

Prototype Lua не имеет доступа к runtime state.

### Runtime Lua

Реагирует на события runtime.

```lua
function on_interact(ctx)
  if ctx:has_flag("flag/guard_bribed") then
    return llmgc.effects.open_dialogue("dialogue/guard_friendly")
  end

  return llmgc.effects.open_dialogue("dialogue/guard_default")
end
```

Runtime Lua не меняет `GameState` напрямую. Он возвращает effects/actions/chunk drafts.

## Типы Lua-файлов

| Тип | Назначение | Entry points |
|---|---|---|
| `prototype` | Описание tiles/items/npc/abilities/resources | `data:extend(...)` |
| `generator` | Генерация chunks/biomes/events/loot | `generate_chunk(ctx)`, `generate_loot(ctx)` |
| `behavior` | Поведение NPC/врагов | `decide_action(ctx)` |
| `interaction` | Реакция на Enter/use/talk | `on_interact(ctx)`, `on_use(ctx)` |
| `formula` | Сложные вычисления | `calculate(ctx)` |
| `event` | Сценарные/global события | `on_event(ctx)` |
| `migration` | Миграции GamePackage | `migrate(ctx)` |

## Sandbox API

Запрещено:

- `io`;
- `os`;
- `debug`;
- `dofile`;
- `loadfile`;
- network/process API;
- прямой доступ к C# objects;
- недетерминированный random.

Разрешённый API должен приходить через `ctx` и `llmgc`.

## Standard library

Базовые helpers лежат в:

```text
templates/lua-stdlib/
samples/minimal-map-game/lualib/
```

Эти файлы нужны, чтобы LLM не изобретала базовые функции каждый раз.

## Validation pipeline

```text
Lua file
 -> classify by script type
 -> sandbox parse/run
 -> collect returned definitions/drafts
 -> validate schema
 -> validate references
 -> validate capabilities
 -> produce diagnostics
 -> allow apply only after approval
```

## Prototype Lua execution status

Prototype Lua execution is implemented only for `data:extend(...)`.

Supported declaration types:

- `tile`;
- `map`;
- `entity_prototype`;
- `manifest_update`;
- `item`;
- `resource`;
- `status`;
- `recipe`;
- `loot_table`;
- `transaction`;
- `resource_network`;
- `resource_node`;
- `inventory`;
- `equipment_slot`;
- `stat`;
- `progression`;
- `encounter`;
- `ability`;
- `quest`;
- `dialogue`;
- `faction`.

The editor-side flow is:

```text
Prototype Lua source
 -> static forbidden API analyzer
 -> fresh MoonSharp sandbox
 -> capture data:extend declarations
 -> map declarations to package_operations
 -> validate through game_package_patch_v1 rules
 -> save Design DB patch artifact
 -> optional dry-run through the existing patch service
```

Lua does not receive a `GamePackage`, `GameState`, filesystem, process, network or direct host object API. The sandbox registers only `data:extend(...)` and `llmgc.version`. The static analyzer conservatively rejects obvious unsafe tokens such as `io`, `os`, `debug`, `dofile`, `loadfile`, `load`, `require`, `package`, `collectgarbage`, environment APIs, `coroutine`, `math.random`, `math.randomseed`, and loop keywords. Declaration count is capped; MoonSharp 2.0.0 does not expose a simple per-script timeout/instruction-limit option in this target, so loops are rejected for this first declarative layer. The analyzer is not a full Lua parser; false positives are acceptable for safety.

Prototype Lua output is not applied automatically. It becomes a `game_package_patch_v1` artifact and apply remains explicit through the existing dry-run/apply/rollback pipeline.

Economy declarations use the same data-only patch path:

```lua
data:extend({
  {
    type = "recipe",
    id = "recipe/healing_potion",
    name = "Healing Potion",
    category = "alchemy",
    inputs = {
      { kind = "item", id = "item/red_herb", amount = 2 }
    },
    costs = {
      { kind = "resource", id = "resource/mana", amount = 5 }
    },
    outputs = {
      { kind = "item", id = "item/healing_potion", amount = 1 }
    }
  }
})
```

The mapper creates allowlisted patch operations such as `upsert_recipe`; Lua still cannot mutate `GamePackage` directly.

Equipment slot declarations map to `upsert_equipment_slot`:

```lua
data:extend({
  {
    type = "equipment_slot",
    id = "slot/tool",
    name = "Tool",
    allowed_tags = { "tool" },
    allowed_kinds = { "tool" }
  }
})
```

Encounter declarations map to data-only patch operations:

```lua
data:extend({
  { type = "stat", id = "stat/strength", name = "Strength", kind = "attribute", default_value = 5 },
  { type = "progression", id = "progression/character_level", name = "Character Level", kind = "xp_level", stages = { { id = "level/1", name = "Level 1", required_amount = 0 } } },
  { type = "ability", id = "ability/basic_attack", name = "Basic Attack", kind = "attack", power = 4, resource_id = "resource/health" },
  {
    type = "encounter",
    id = "encounter/goblin_duel",
    name = "Goblin Duel",
    kind = "combat",
    participants = {
      { id = "player", name = "Player", kind = "player", team = "player", resources = { { kind = "resource", id = "resource/health", amount = 30 } }, abilities = { "ability/basic_attack" } },
      { id = "goblin", name = "Goblin", kind = "enemy", team = "enemy", resources = { { kind = "resource", id = "resource/health", amount = 12 } }, abilities = { "ability/basic_attack" } }
    }
  }
})
```

Runtime Lua, generator Lua, behavior Lua, interaction Lua, formula Lua, event Lua and migration Lua execution are still not implemented. Generator modules are not executed by this layer.

Narrative declarations map to data-only patch operations:

```lua
data:extend({
  { type = "faction", id = "faction/village", name = "Village", kind = "settlement", default_reputation = 0, min_reputation = -100, max_reputation = 100 },
  {
    type = "quest",
    id = "quest/help_healer",
    title = "Help the Healer",
    description = "Gather herbs.",
    objectives = {
      { id = "objective/herbs", kind = "collect_item", target_id = "item/red_herb", required_amount = 3 }
    },
    rewards = {
      { kind = "reputation", id = "faction/village", amount = 5 }
    }
  },
  {
    type = "dialogue",
    id = "dialogue/healer",
    title = "Village Healer",
    start_node_id = "start",
    nodes = {
      { id = "start", text = "Can you help?", choices = { { id = "accept", text = "Yes", start_quest_id = "quest/help_healer", close_dialogue = true } } }
    }
  }
})
```

These declarations still produce `game_package_patch_v1` artifacts. They are not applied automatically and do not execute runtime Lua.

## LLM generation rules

LLM должна получать:

- тип Lua-файла;
- разрешённый API;
- examples;
- список существующих ids;
- constraints;
- expected output format.

LLM не должна получать весь проект.
