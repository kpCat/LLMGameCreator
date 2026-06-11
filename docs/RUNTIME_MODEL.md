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
