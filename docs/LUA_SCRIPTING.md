# Lua Scripting

Lua планируется как контролируемый scripting layer, а не как обход архитектуры.

## Типы Lua-файлов

- `prototype.lua` — декларации через `data:extend(...)`.
- `generator.lua` — генерация чанков, loot, NPC, encounters.
- `behavior.lua` — поведение NPC/врагов.
- `interaction.lua` — реакция на Enter/use/talk/inspect.
- `formula.lua` — сложные формулы.
- `event.lua` — сценарные события мира.
- `migration.lua` — миграции версии GamePackage.

## Правила

- Lua не имеет прямого доступа к GameState.
- Lua возвращает draft/effects/commands, которые проверяет runtime.
- Random доступен только через runtime API.
- Файловая система, сеть, OS API, debug/loadfile/dofile запрещены.
- LLM-generated Lua не применяется без sandbox/validation/test-run/preview.

В v0.1 есть только `ScriptDefinition` и `NullScriptEngine`.
