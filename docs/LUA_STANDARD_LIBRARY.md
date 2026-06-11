# Lua Standard Library baseline

`LLMGameCreator` должен иметь заранее подготовленную базовую Lua-библиотеку. Это уменьшает объём prompt-а, снижает риск ошибок и не заставляет LLM каждый раз генерировать одно и то же.

## Расположение

```text
templates/lua-stdlib/
  core.lua
  random.lua
  noise.lua
  chunks.lua
  tiles.lua
  entities.lua
  effects.lua
  interactions.lua
  loot.lua
  dialogue.lua
  quests.lua
  combat.lua
  validation_helpers.lua
```

В sample-проекте копия лежит в:

```text
samples/minimal-map-game/lualib/
```

## Назначение файлов

| Файл | Назначение |
|---|---|
| `core.lua` | namespace `llmgc`, assert/helpers, table helpers |
| `random.lua` | deterministic random wrapper через `ctx` |
| `noise.lua` | wrapper для noise API runtime |
| `chunks.lua` | helpers для chunk draft |
| `tiles.lua` | helpers для tile draft |
| `entities.lua` | helpers для entity draft |
| `effects.lua` | constructors для стандартных effects |
| `interactions.lua` | helpers для interaction result |
| `loot.lua` | weighted loot helpers |
| `dialogue.lua` | dialogue/open node helpers |
| `quests.lua` | quest effects helpers |
| `combat.lua` | combat action/effects helpers |
| `validation_helpers.lua` | runtime-safe assertions для generated Lua |

## Принцип

Library не должна обходить sandbox. Она только упрощает запись разрешённых объектов.

Плохо:

```lua
GameState.player.gold = GameState.player.gold + 10
```

Хорошо:

```lua
return llmgc.effects.change_resource("player", "resource/gold", 10)
```

## Будущий путь

В следующих патчах эти Lua-файлы можно подключить к реальному Lua engine, а пока они выступают как стандарт контракта и примеры для генерации.
