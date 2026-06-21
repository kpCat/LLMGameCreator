Работай в репозитории `LLMGameCreator`.

Выполни задачу:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/017_UNITY_ARCHIVE_EXPORT_DRY_RUN.md
```

Это bounded editor-side dry-run slice:
- добавить UnityArchiveExportDryRunService;
- создать deterministic export plan;
- писать `.llmgc/unity-export-dry-run/` файлы;
- добавить readiness/future-module diagnostics;
- добавить smoke `unity-archive-export-dry-run`.

Не реализуй Unity.
Не создавай Unity project.
Не трогай Runtime.
Не меняй GamePackageDefinition/package schema.
Не трогай WinForms.
Не трогай generator-library.
Не вызывай LLM/provider.
Не исполняй генераторы.
Не реализуй ComfyUI/Suno integration.
Не запускай git-команды.

Рекомендуемый reasoning level: High.

Обязательные проверки указаны в task file.

Финальный отчёт дай на русском.
