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
