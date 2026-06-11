# Architecture

## Главные правила

1. Runtime никогда не вызывает LLM.
2. LLM используется только в editor/generation pipeline.
3. GamePackage является единственным источником правды для готовой игры.
4. Unity/MonoGame/Godot/WinForms Preview — это только frontends/player-ы.
5. WinForms editor не содержит runtime-логики конкретной игры.
6. Каждая страница WinForms — отдельный `UserControl`.
7. `MainForm` — только shell.
8. Domain не зависит от WinForms, DryIoc, JSON-хранилища, LLM и ComfyUI.
9. Lua-файлы имеют строгий тип: prototype/generator/behavior/interaction/formula/event/migration.
10. Ассеты являются data-driven сущностями и подключаются через `assetId`.
11. ComfyUI/Fooocus — внешние providers editor pipeline, не часть runtime.
12. Готовая игра должна исполняться без модели и без контекста LLM.

## Контуры

```text
WinForms Editor
  -> Application Use Cases
  -> GamePackage / Runtime / Validation / Generation / AssetPipeline

Runtime Player
  -> GamePackage
  -> Runtime commands
  -> Runtime events
  -> Rendering/audio/input frontend
```

## Почему GamePackage отдельный

В будущем Unity Player должен уметь читать тот же GamePackage. Поэтому DTO/контракты должны оставаться простыми, сериализуемыми и не завязанными на WinForms.
