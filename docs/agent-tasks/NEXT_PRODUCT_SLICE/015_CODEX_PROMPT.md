Работай в репозитории `LLMGameCreator`.

Выполни задачу:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/015_READ_ONLY_COMPOSITION_WORKBENCH_UI.md
```

Это bounded read-only UI consumer slice:
- добавить Composition Workbench page;
- показывать blueprint presets;
- строить diagnostics report;
- показывать markdown;
- экспортировать report через существующий export service;
- обновлять список saved reports из `.llmgc/composition-diagnostics/index.json`;
- добавить smoke `composition-workbench-readonly`.

Не трогай Runtime.
Не меняй GamePackageDefinition/package schema.
Не трогай generator-library.
Не исполняй генераторы.
Не делай dynamic plugins.
Не делай semantic world model/imported map/lazy world/procedural quest engine.
Не вызывай LLM/provider.
Не запускай git-команды.
Сохрани Designer-safe split.

Рекомендуемый reasoning level: High.

Обязательные проверки указаны в task file.

Финальный отчёт дай на русском.
