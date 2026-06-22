Работай в репозитории `LLMGameCreator`.

Выполни задачу:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/018_UNITY_ARCHIVE_MATERIALIZATION_V1.md
```

Это bounded editor-side archive materialization slice:
- добавить UnityArchiveMaterializationService;
- использовать Slice 017 dry-run;
- писать `.llmgc/unity-archive/` contract/meta files;
- optionally писать `.llmgc/unity-archive.zip`, если это безопасно и мало;
- добавить smoke `unity-archive-materialization`.

Не реализуй Unity.
Не создавай Unity project.
Не трогай Runtime.
Не меняй GamePackageDefinition/package schema.
Не трогай WinForms.
Не трогай generator-library.
Не вызывай LLM/provider.
Не исполняй генераторы.
Не исполняй Lua.
Не реализуй ComfyUI/Suno integration.
Не запускай git-команды.

Рекомендуемый reasoning level: High.

Обязательные проверки указаны в task file.

Финальный отчёт дай на русском.
