# Procedural World Generation

## Цель

Большие игры не должны генерироваться как тысячи заранее написанных сцен. LLM должна создавать:

- правила мира;
- биомы;
- прототипы сущностей;
- таблицы вероятностей;
- Lua generator scripts;
- ключевые сюжетные чанки;
- asset requests.

Runtime затем создаёт конкретные chunks детерминированно по seed/state без вызова LLM.

## Два режима карты

### 1. Finite Map

Ограниченная карта заранее известного размера:

```text
worldWidth = 128
worldHeight = 128
chunkSize = 16
```

Подходит для:

- сюжетных RPG;
- небольших изометрических игр;
- handcrafted + procedural mix;
- городов, подземелий, островов.

### 2. Infinite / Expandable Map

Карта создаётся по мере движения игрока:

```text
chunk = generate_chunk(worldSeed, chunkX, chunkY, generationRules, persistentOverrides)
```

Подходит для:

- survival/exploration;
- roguelite;
- sandbox;
- процедурных миров.

## Chunk lifecycle

```text
NeedChunk(x,y)
 → if chunk exists in save/cache: load
 → else execute generator.lua
 → validate GeneratedChunkDraft
 → persist generated base chunk
 → apply state overrides
 → return runtime chunk
```

## Важные правила

1. Генератор чанка не меняет GameState напрямую.
2. Генератор возвращает `GeneratedChunkDraft`.
3. Runtime валидирует tileId/prototypeId/triggerId/entity components.
4. Random/noise должны быть deterministic и seed-based.
5. Сгенерированный чанк после первого посещения сохраняется или воспроизводится по seed + overrides.
6. Runtime LLM не используется.

## Что должен получать Lua generator ctx

```text
ctx.world_seed
ctx.chunk_x
ctx.chunk_y
ctx.chunk_size
ctx.generation_mode
ctx:noise2d(seed, x, y, scale)
ctx:random_int(min, max)
ctx:random_float()
ctx:weighted_pick(tableId)
ctx:is_chunk_visited(cx, cy)
ctx:get_world_flag(flagId)
```

## Что возвращает generator.lua

```lua
return {
  width = 16,
  height = 16,
  tiles = {},
  entities = {},
  triggers = {},
  events = {},
  metadata = {}
}
```

## Что делает LLM

LLM не генерирует бесконечную карту. LLM генерирует:

- правила биомов;
- Lua generator template;
- таблицы spawn/loot/events;
- ключевые локации;
- условия появления сюжетных объектов;
- asset requirements.
