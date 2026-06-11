# Script Manifest Specification

`script-manifest.json` описывает typed Lua scripts в GamePackage.

## Зачем нужен manifest

Lua сложно валидировать статически. Поэтому Lua ограничивается через:

1. тип скрипта;
2. sandbox API;
3. manifest;
4. entry point contract;
5. validation/dry-run;
6. runtime diagnostics.

Скрипт без manifest runtime не исполняет.

## Script types

```text
prototype     — декларации data:extend(...), без runtime ctx;
generator     — генерация map/chunk/entities/events;
behavior      — поведение NPC/enemy/entity;
interaction   — реакция на Enter/use/talk/inspect;
formula       — расчёт числового/логического значения;
event         — world/runtime event handler;
migration     — миграция GamePackage между версиями.
```

## Минимальный manifest

```json
{
  "scripts": [
    {
      "id": "script/generator/infinite_perlin_world",
      "type": "generator",
      "path": "scripts/generators/infinite_perlin_world.lua",
      "entryPoints": [
        {
          "name": "generate_chunk",
          "inputContract": "contract/script-input/generate_chunk_v1",
          "outputContract": "contract/script-output/chunk_draft_v1"
        }
      ],
      "capabilities": [
        "read_world_seed",
        "read_generation_rules",
        "noise",
        "random",
        "return_chunk_draft"
      ],
      "usedBy": [
        "map/wilderness"
      ],
      "validation": {
        "dryRun": true
      }
    }
  ]
}
```

## Capabilities

Разрешения должны быть explicit allow-list.

Примеры:

```text
read_world_seed
read_map_info
read_generation_rules
read_entity_state
read_player_state
read_flags
read_stats
read_resources
read_inventory
read_relationships
noise
random
weighted_pick
return_effects
return_chunk_draft
return_action_draft
return_formula_value
open_dialogue
spawn_entity
```

Запрещено:

```text
filesystem
network
process
reflection
debug
raw_os
raw_io
raw_package
loadfile
dofile
require_unapproved
global_game_state_mutation
```

## Prototype scripts

Prototype Lua:

- разрешает только декларативный `data:extend(...)`;
- не получает runtime `ctx`;
- не имеет доступа к состоянию прохождения;
- используется при сборке GamePackage.

## Generator scripts

Generator Lua возвращает draft, а не меняет мир напрямую.

```lua
function generate_chunk(ctx)
    local chunk = llmgc.chunks.new(ctx.chunk_size, ctx.chunk_size)
    return chunk
end
```

## Behavior scripts

Behavior Lua возвращает action draft.

```lua
function decide_action(ctx)
    return { action = "move_towards", target = "player" }
end
```

## Interaction scripts

Interaction Lua возвращает effects.

```lua
function on_interact(ctx)
    return {
        effects = {
            llmgc.effects.open_dialogue("dialogue/old_guard")
        }
    }
end
```

## Formula scripts

Formula Lua возвращает число, bool или string согласно output contract. Formula script не возвращает effects.

## Validation stages

```text
1. Manifest JSON schema validation.
2. Script path exists.
3. Script type known.
4. Entry points declared.
5. Capabilities allowed for script type.
6. usedBy references exist.
7. Static forbidden token scan.
8. Sandbox load.
9. Dry-run with sample ctx.
10. Output contract validation.
```

## Forbidden token scan

Минимально искать:

```text
io.
os.
debug.
package.
require(
loadfile
dofile
load(
coroutine.
collectgarbage
```

Это не полноценная безопасность, но ранняя диагностика.

## LLM rule

LLM получает только:

- script type;
- allowed API;
- manifest fragment;
- expected entry point;
- input/output contract;
- похожий blueprint.

LLM не меняет sandbox model.
