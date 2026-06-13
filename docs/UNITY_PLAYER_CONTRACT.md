## Runtime v1 Command/Event Bridge

Gameplay runtime v1 exposes `GameRuntimeCommand`, `GameRuntimeEvent` and `GameRuntimeResult` for economy/gameplay simulation. These contracts are frontend-friendly and can be adapted by a future Unity bridge without adding game-specific logic to Unity.

Unity is still not implemented in this repository. Runtime v1 remains headless C# and does not execute Lua, generator modules, LLM calls, Unity codegen or asset providers.

## Unified Runtime Bridge Target

Future Unity work should target `IUnifiedGameRuntimeService` as the command/event bridge. The bridge exposes a single `UnifiedRuntimeSession` containing legacy map state plus gameplay runtime state, while keeping `GamePackageDefinition` separate from runtime snapshots.

Unity remains out of scope here: no Unity project, codegen, MonoBehaviour game logic or Unity-specific runtime execution is added by the bridge.

Encounter runtime commands/events are part of the future bridge surface. A Unity adapter may send `StartEncounter`, `UseAbility`, `BasicAttack`, `EndTurn`, `ResolveEncounter`, `FleeEncounter` or `RunCurrentTurnAi` as gameplay commands and render encounter events such as `EncounterStarted`, `TurnStarted`, `AbilityUsed`, `DamageApplied`, `HealingApplied`, `ParticipantDefeated`, `EncounterWon`, `EncounterLost` and `RewardGranted`.

Unity still must not contain game-specific combat rules in C#; it should display data and route commands/events through the runtime bridge.

## Narrative Command/Event Notes

Future Unity narrative UI should route quest, dialogue and faction actions through the existing runtime bridge instead of implementing game-specific rules in Unity C#.

Relevant commands include `StartQuest`, `AdvanceQuestObjective`, `CompleteQuest`, `FailQuest`, `OpenDialogue`, `ChooseDialogueOption`, `CloseDialogue`, `ChangeReputation`, `SetReputation` and `RefreshQuestObjectives`.

Relevant events include `QuestStarted`, `QuestObjectiveUpdated`, `QuestStageChanged`, `QuestCompleted`, `JournalUpdated`, `DialogueOpened`, `DialogueNodeChanged`, `DialogueChoiceSelected`, `DialogueClosed`, `FactionReputationChanged` and `FactionRelationChanged`.

Unity remains out of scope: no Unity project, codegen, MonoBehaviour game logic or runtime Lua execution is added by narrative runtime v1.

# Unity Player Contract

## Назначение

Unity Player — это будущий универсальный frontend/runtime-player для игр, созданных в LLMGameCreator.

Unity Player **не является редактором конкретной игры** и **не должен содержать игровую логику конкретного проекта**. Он загружает `GamePackage`, отображает карту/сущности/диалоги/ассеты, отправляет команды в runtime и применяет runtime events к визуальному представлению.

## Жёсткие правила

1. Unity Player не вызывает LLM.
2. Unity Player не вызывает ComfyUI/Fooocus.
3. Unity Player не содержит конкретных NPC, квестов, предметов, сцен и биомов в C#-коде.
4. Единственный источник правды для игры — `GamePackage`.
5. Unity Player получает пользовательский ввод и преобразует его в `RuntimeCommand`.
6. Runtime возвращает `RuntimeEvent`, а Unity только отображает результат.
7. Rendering/audio/UI не меняют `GameState` напрямую.
8. Unity может иметь собственные adapters/loaders, но не должен переписывать game rules.
9. Ассеты загружаются через `AssetCatalog` по `assetId`, а не через hardcoded paths.
10. Отсутствующий ассет заменяется fallback-ассетом, а не ломает runtime.

## Минимальная будущая схема Unity Player

```text
UnityPlayer
 ├─ BootstrapScene
 ├─ GamePackageLoader
 ├─ AssetCatalogLoader
 ├─ RuntimeBridge
 ├─ InputController
 ├─ MapRenderer
 ├─ EntityRenderer
 ├─ DialoguePresenter
 ├─ InventoryPresenter
 ├─ AudioController
 ├─ CameraController
 └─ SaveGameAdapter
```

## Команды от frontend к runtime

Минимальный набор команд, который нужно сохранять совместимым:

```text
MoveNorth
MoveSouth
MoveWest
MoveEast
MoveBy(dx, dy)
Interact
UseItem(itemId, targetId?)
UseAbility(abilityId, targetId?)
ChooseDialogueOption(optionId)
OpenInventory
CloseInventory
Wait
```

## События от runtime к frontend

Unity Player должен уметь отображать такие события:

```text
GameStarted
MapLoaded
ChunkLoaded
PlayerMoved
EntityMoved
EntitySpawned
EntityDespawned
DialogueOpened
DialogueClosed
InventoryChanged
QuestUpdated
StatChanged
ResourceChanged
StatusAdded
StatusRemoved
SoundRequested
MusicRequested
VisualEffectRequested
LogMessageAdded
```

## GamePackage, который должен быть удобен Unity

GamePackage должен содержать:

```text
manifest.json
asset-catalog.json
script-manifest.json
runtime-settings.json
maps/
chunks/
prototypes/
scripts/
assets/
dialogues/
quests/
```

Unity не должен строить игру из редакторских данных напрямую. Если редакторские данные сложнее, чем нужно player-у, должен быть отдельный export/build step:

```text
Editor Project → Validate → Build GamePackage → Unity Player loads GamePackage
```

## Почему Unity не добавляется в v0.1

Unity-проект не добавляется в первый скелет намеренно:

- он резко увеличит технологический стек;
- Codex начнёт тратить лимит на Unity-specific детали;
- легко смешать game logic с MonoBehaviour;
- сначала нужно стабилизировать GamePackage contract.

Правильный порядок:

```text
v0.1: GamePackage + WinForms preview + contracts
v0.2: map/runtime validation
v0.3: typed Lua execution
v0.4: asset pipeline/manual import
v0.5: ComfyUI provider
v0.6: Unity Player prototype
```

## Definition of Done для первого Unity Player prototype

1. Загружает `minimal-map-game`.
2. Показывает tilemap из GamePackage.
3. Показывает игрока и NPC по `assetId`.
4. Принимает WASD.
5. Отправляет команды в runtime bridge.
6. Enter открывает dialogue/interaction.
7. Воспроизводит sound event через `assetId`.
8. Не содержит hardcoded логики конкретной игры.
