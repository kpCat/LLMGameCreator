# Runtime Model

Runtime работает по модели:

```text
GameState + PlayerCommand -> CommandResult + RuntimeEvents
```

Минимальные команды v0.1:

- `Move`
- `Interact`

Будущие команды:

- `UseItem`
- `UseAbility`
- `OpenInventory`
- `ChooseDialogueOption`
- `Wait`
- `Attack`
- `Trade`

Runtime не знает про LLM, ComfyUI, WinForms editor и генерацию ассетов.

## Unified Runtime Bridge v1

Runtime now has two compatible layers:

- legacy map preview runtime: `PlayerCommand`, `GameState`, `RuntimeEvent`, `IGameRuntime`;
- gameplay runtime: `GameRuntimeCommand`, `GameRuntimeState`, `GameRuntimeEvent`, `IGameRuntimeService`.

`IUnifiedGameRuntimeService` creates a `UnifiedRuntimeSession` with both map and gameplay state. It routes map movement and map interaction through the legacy runtime and gameplay commands through `IGameRuntimeService`. It does not replace the old `IGameRuntime` contract.

`UnifiedRuntimeSession` stores only runtime state and event logs. It does not embed `GamePackageDefinition`; package definitions remain the immutable source of truth passed into runtime methods as read-only definitions.

`UseItem` and `ExecuteInteraction` are gameplay commands. `Wait` routes to gameplay ticks while keeping map state compatible.

`IRuntimeStateSerializer` serializes/deserializes `GameRuntimeState` and `UnifiedRuntimeSession` as camelCase indented JSON strings without file IO or database persistence.

## Exploration Inventory Runtime v1

Equipment, containers and harvesting are implemented in the gameplay runtime layer and keep `GamePackageDefinition` immutable.

Equipment state lives in `GameRuntimeState.Equipment`. `EquipItem` moves one item stack unit from an inventory into a slot, validates optional `EquipmentSlotDefinition` allowed tags/kinds/requirements, and atomically returns an old equipped item to inventory when replacing it. `UnequipItem` returns the equipped item to inventory. Packages do not need mandatory slots; if a command names a slot that has no definition, runtime accepts items whose `metadata.equip_slot`, kind or tags match the requested slot.

Container runtime uses normal `InventoryState` records. `OpenContainer` reports contents without mutation. `TakeFromContainer` and `DepositToContainer` transfer stack data between player and container inventories while preserving unique id, quest flag, durability, charge and metadata. Container inventories are discovered by `ownerKind = "container"`, `container` tags or metadata.

Harvesting is exposed as `HarvestResourceNode`. It evaluates node requirements, optional tool metadata (`required_tool_tag`, `required_tool_item_id`), consumes node costs plus optional `durability_cost` and `charge_cost`, applies `Production`, `ConversionOutputs` and optional `loot_table_id` / `harvest_loot_table_id` output rolls with deterministic seeds. Simple depletion may be represented only in runtime metadata, never by mutating node definitions.

Durability and charge costs can target an item id or an equipped slot id. If stack/item metadata contains `break_on_zero=true`, the runtime removes the stack or clears the equipment slot when the meter reaches zero; otherwise the meter remains at zero or below and diagnostics/events describe the consumption.

`IRuntimeSnapshotStore` saves and loads `UnifiedRuntimeSession` JSON files under `.llmgc/runtime-saves/<slot>.runtime.json`. Snapshot files contain runtime state only, not package definitions. Slot names are sanitized and path traversal is rejected. There is no autosave, database persistence or save-game menu system in this layer.

## Encounter and Combat Runtime v1

Gameplay runtime now supports these encounter commands: `StartEncounter`, `UseAbility`, `BasicAttack`, `EndTurn`, `ResolveEncounter`, `FleeEncounter` and `RunCurrentTurnAi`.

Encounter state lives in `GameRuntimeState.ActiveEncounter`. It stores only runtime values: encounter id/kind, active flag, round, turn index, participants, resources, stats, statuses, cooldowns, action history and metadata. It does not embed or mutate `GamePackageDefinition`.

Events include `EncounterStarted`, `TurnStarted`, `AbilityUsed`, `DamageApplied`, `HealingApplied`, `ParticipantDefeated`, `EncounterWon`, `EncounterLost`, `EncounterEnded`, `RewardGranted`, `ProgressionChanged`, `ProgressionStageChanged` and `AiActionChosen`.

Combat v1 is intentionally small: participant order is initiative, damage/healing uses resource deltas, status ticking only decrements remaining turns, AI chooses the first available action, and rewards/loot/progression are applied only when the encounter is won. Runtime still does not execute Lua, generator modules, LLM calls, Unity codegen or external providers.

## Narrative Runtime v1

Gameplay runtime now supports narrative commands:

- `StartQuest`, `AdvanceQuestObjective`, `SetQuestStage`, `CompleteQuest`, `FailQuest`, `RefreshQuestObjectives`;
- `OpenDialogue`, `ChooseDialogueOption`, `CloseDialogue`;
- `ChangeReputation`, `SetReputation`.

Narrative events include `QuestStarted`, `QuestObjectiveUpdated`, `QuestStageChanged`, `QuestCompleted`, `QuestFailed`, `QuestRewardGranted`, `JournalUpdated`, `DialogueOpened`, `DialogueNodeChanged`, `DialogueChoiceSelected`, `DialogueClosed`, `DialogueEffectApplied`, `FactionReputationChanged` and `FactionRelationChanged`.

`QuestRuntimeService`, `DialogueRuntimeService`, `FactionRuntimeService` and `QuestObjectiveTracker` operate on `GameRuntimeState` only. Definitions are read from `GamePackageDefinition` and are not embedded in runtime snapshots. Dialogue choices use existing requirements, costs and output application; quest and dialogue rewards can grant items, resources, progression and faction reputation.

Limitations: no full dialogue UI, no quest editor, no localization system, no runtime Lua/generator Lua, no generator module execution, no LLM call and no Unity/codegen.
