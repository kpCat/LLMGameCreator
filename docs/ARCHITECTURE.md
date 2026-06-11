# Архитектура LLMGameCreator

## Цель архитектуры

Проект должен поддерживать несколько классов игр:

- простая текстовая/narrative игра;
- игра с картой и WASD-передвижением;
- 2D/изометрическая пошаговая RPG;
- процедурная игра с чанками, NPC, квестами, предметами, способностями, Lua-логикой и ассетами;
- гибридная игра: карта + диалоги + сцены + боёвка + события.

При этом первый скелет не должен реализовывать всё сразу. Он должен заложить правильные границы.

## Слои решения

```text
Domain
  Чистые модели и value objects.

GamePackage
  DTO/контракты пакета игры, совместимые с будущим Unity Player.

Runtime.Abstractions
  Команды, события, состояния и контракты исполнения.

Runtime
  Headless runtime без UI и без LLM.

Scripting
  Контракты typed Lua, script manifests, script diagnostics.

Generation
  LLM sessions, jobs, context packs, draft workflow.

AssetPipeline
  Asset catalog, asset contracts, generation requests, providers.

Application
  Use-cases: открыть проект, сохранить, валидировать, запускать preview, применять draft.

Infrastructure
  JSON storage, settings storage, logging, future SQLite/cache providers.

WinForms
  Editor shell, pages, presenters, composition root.
```

## Зависимости

```text
Domain <- GamePackage
Domain <- Runtime.Abstractions
Domain <- Runtime
Domain <- Scripting
Domain <- AssetPipeline
Domain <- Generation
Domain <- Application
Application <- Infrastructure
Application <- WinForms
```

Правило: `Domain` не знает ни про WinForms, ни про DryIoc, ни про JSON storage, ни про LLM, ни про Unity.

## Runtime

Runtime должен быть command-based:

```text
GameState + RuntimeCommand -> RuntimeResult + RuntimeEvents + NewGameState
```

Минимальные команды:

- `StartGame`;
- `MoveNorth/MoveSouth/MoveWest/MoveEast`;
- `Interact`;
- `UseItem`;
- `UseAbility`;
- `ChooseDialogueOption`;
- `Wait`.

Runtime не отображает графику. Он только меняет состояние и возвращает события.

## Unity Player

Unity Player должен быть отдельным frontend/player:

```text
UnityPlayer
  loads GamePackage
  renders maps/entities/assets
  plays audio
  displays dialogues
  sends RuntimeCommands
  consumes RuntimeEvents
```

Unity Player не должен содержать конкретную игровую логику.

## Lua

Lua разделяется на строгие типы:

- `prototype.lua` — объявления data/prototypes;
- `generator.lua` — генерация чанков/биомов/loot/events;
- `behavior.lua` — поведение NPC/врагов;
- `interaction.lua` — реакции на interact/use/talk;
- `formula.lua` — сложные вычисления;
- `event.lua` — сценарные/global события;
- `migration.lua` — миграции пакета в будущем.

Каждый тип имеет свой sandbox API.

## Assets

Ассеты — отдельные сущности:

- `tile`;
- `tileset`;
- `character_spritesheet`;
- `npc_spritesheet`;
- `item_icon`;
- `ability_icon`;
- `portrait`;
- `portrait_expression_set`;
- `dialogue_background`;
- `sound_effect`;
- `music_loop`;
- `ambient_loop`;
- `vfx_spritesheet`.

Игровые сущности ссылаются на `assetId`, а не на прямой путь.

## Settings-first подход

В настройки выносятся:

- корневая папка игр;
- папка ассетов;
- профили LLM endpoints;
- профили ComfyUI endpoints;
- workflow profiles;
- лимиты контекста;
- параллельность generation jobs;
- логирование;
- подтверждения применения draft.

## Минимальный вертикальный срез

Первый рабочий vertical slice:

```text
Open project
 -> Load GamePackage
 -> Validate
 -> Start Runtime Preview
 -> Move player on map
 -> Interact with object/NPC
 -> Runtime event appears in log
```

Все будущие системы должны расширять этот срез, а не обходить его.
