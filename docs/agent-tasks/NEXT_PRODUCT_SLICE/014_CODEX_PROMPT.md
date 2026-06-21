Работай в репозитории `LLMGameCreator`.

Выполни задачу:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/014_HEADLESS_COMPOSITION_REPORT_EXPORT.md
```

Это bounded headless persistence/export slice:
- добавить export service для `GameCompositionDiagnosticsReport`;
- писать markdown report в `.llmgc/composition-diagnostics/`;
- писать deterministic index;
- обеспечить safe path / no traversal;
- добавить smoke `composition-report-export`.

Не делай UI.
Не трогай Runtime.
Не меняй GamePackageDefinition/package schema.
Не трогай WinForms.
Не трогай generator-library.
Не исполняй генераторы.
Не делай dynamic plugins.
Не делай semantic world model.
Не делай imported map/lazy world/procedural quest engine.
Не вызывай LLM/provider.
Не запускай git-команды.

Рекомендуемый reasoning level: High.

Обязательные проверки указаны в task file.

Финальный отчёт дай на русском.
