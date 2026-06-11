# Entity Component Model

Документ описывает модель сущностей для map-based и narrative игр.

## Главный принцип

Сущность — это не класс `Npc`, `Chest`, `Enemy`, `Portal`.

Сущность — это:

```text
EntityPrototype + components + runtime state
```

Сундук:

```text
renderable
collidable
interactable
container
lootable
```

NPC:

```text
renderable
collidable
interactable
dialogue
stats
behavior
faction_member
```

## Prototype / Placement / State

`EntityPrototype` — статическое описание типа сущности.

`EntityPlacement` — размещение конкретной сущности на карте.

`EntityState` — runtime-состояние конкретной сущности.

```json
{
  "id": "entity/village/old_guard_01",
  "prototypeId": "entity-prototype/npc/old_guard",
  "mapId": "map/village",
  "position": { "x": 12, "y": 7 },
  "overrides": {}
}
```

## Базовые компоненты

```text
position
renderable
collidable
interactable
dialogue
inventory
equipment
container
lootable
stats
resources
abilities
progression
combatant
behavior
faction_member
quest_giver
vendor
trigger
portal
light_source
sound_source
scripted
```

## Component rules

1. Component не содержит C# type names.
2. Component не ссылается на файл ассета напрямую; только `assetId`.
3. Component может ссылаться на `interactionId`, `dialogueId`, `scriptId`, `lootTableId`.
4. Unknown component — validation error, если игра не объявила custom component registry.
5. Runtime не должен молча игнорировать gameplay-components.

## Map model

Для ограниченной карты:

```json
{
  "id": "map/village",
  "mode": "finite",
  "width": 64,
  "height": 64
}
```

Для процедурной карты:

```json
{
  "id": "map/wilderness",
  "mode": "chunked",
  "chunkSize": 32,
  "generatorScriptId": "script/generator/infinite_perlin_world"
}
```

## Chunk model

Chunk — единица процедурной генерации и сохранения.

```text
chunk = mapId + chunkX + chunkY
GeneratedChunkDraft = generator(worldSeed, mapId, chunkX, chunkY, rules)
```

Если игрок изменил chunk, изменения пишутся в persistent state.

## Runtime command flow

```text
Unity/WinForms Input
  -> RuntimeCommand
  -> RuntimeCommandDispatcher
  -> MovementSystem / InteractionSystem / ScriptSystem
  -> GameState update
  -> RuntimeEvents
  -> Frontend rendering/audio/UI
```

Rendering не меняет состояние.

## Unity compatibility

Unity Player должен читать `GamePackage`, `MapDefinition`, `EntityPlacement`, `AssetCatalog` и отображать их.

Unity не должен содержать классы под конкретные игры:

```text
OldGuardBehaviour
FireTempleQuestController
```

Только универсальные:

```text
EntityView
ComponentViewFactory
RuntimeEventApplier
AssetCatalogLoader
```

## Validation checklist

Проверять:

- уникальность id;
- существование prototypeId;
- существование assetId;
- существование interactionId;
- существование dialogueId;
- существование scriptId;
- корректность mapId;
- позиция внутри finite map;
- components compatible with enabled systems;
- portal target существует;
- trigger interaction существует.
