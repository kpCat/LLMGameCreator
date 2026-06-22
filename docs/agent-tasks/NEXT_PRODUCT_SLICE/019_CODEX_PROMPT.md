Работай в репозитории `LLMGameCreator`.

Выполни задачу:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/019_UNITY_ARCHIVE_GAME_DATA_PAYLOAD_V1.md
```

Это bounded editor-side archive data-payload slice:
- добавить UnityArchiveGameDataPayloadService;
- писать `.llmgc/unity-archive/data/`;
- писать game-package.json и category indexes;
- интегрировать payload в UnityArchiveMaterializationService, если package data supplied;
- добавить smoke `unity-archive-game-data-payload`.

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
