# Unity Player plan

Unity Player рассматривается как будущий универсальный frontend/runtime-player.

## Что Unity Player должен делать

- загружать `GamePackage`;
- читать `AssetCatalog`;
- отображать карту, чанки, тайлы и сущности;
- проигрывать sprite animations;
- показывать dialogue UI с portrait expression sets;
- проигрывать SFX/music/ambient;
- отправлять `RuntimeCommand` в runtime;
- отображать `RuntimeEvent`;
- сохранять/загружать runtime state.

## Что Unity Player не должен делать

- не должен содержать конкретные NPC/quests/items hardcoded;
- не должен вызывать LLM;
- не должен генерировать ассеты;
- не должен быть редактором игры;
- не должен хранить source of truth;
- не должен напрямую менять GameState мимо runtime.

## Схема

```text
Unity BootstrapScene
  -> GamePackageLoader
  -> RuntimeBridge
  -> AssetResolver
  -> MapRenderer
  -> EntityRenderer
  -> DialoguePresenter
  -> AudioController
  -> InputController
```

## Совместимость

Общие DTO/контракты GamePackage должны быть совместимы с Unity. Поэтому для shared contracts нужно избегать привязки к `net8.0-windows`.

Рекомендуемый подход позже:

```text
LLMGameCreator.GamePackage.Contracts
  target: netstandard2.1 или multi-target

LLMGameCreator.WinForms
  target: net8.0-windows
```

## Когда добавлять Unity

Не в v0.1.

Порядок:

1. GamePackage contract.
2. Headless runtime.
3. WinForms map preview.
4. Lua typed layer.
5. Asset catalog/manual import.
6. ComfyUI provider.
7. Unity Player prototype.

## Важный запрет

Unity scene не должна стать местом ручной сборки конкретной игры. Конкретная игра лежит в `GamePackage`.
