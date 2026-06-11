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

## LLM generation rules

LLM должна получать:

- тип Lua-файла;
- разрешённый API;
- examples;
- список существующих ids;
- constraints;
- expected output format.

LLM не должна получать весь проект.
