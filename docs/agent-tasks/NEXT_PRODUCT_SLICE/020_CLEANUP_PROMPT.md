Работай в ветке `kilo-free` репозитория `LLMGameCreator`.

Выполни cleanup-задачу:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/020_CLEANUP_REQUEST_PIPELINE.md
```

Это не новый product slice, а quality pass перед решением о merge.

Главные пункты:
- убрать двойной BuildRequests в UnityArchiveMaterializationService;
- заменить misleading readiness BlockedByFutureProviders на BlockedByErrors;
- убрать unused UnityArchiveLuaModuleRequestEntry или реально использовать;
- агрегировать future provider warnings;
- добавить focused tests на duplicate/blank/unknown ids и aggregated warnings;
- сохранить текущий smoke и deterministic outputs.

Не реализуй Unity.
Не трогай Runtime.
Не меняй GamePackageDefinition/package schema.
Не трогай WinForms.
Не трогай generator-library.
Не вызывай LLM/provider.
Не исполняй generators.
Не исполняй Lua.
Не запускай git-команды.
Не делай broad repository discovery.

Рекомендуемый reasoning level: High.

Финальный отчёт дай на русском.
