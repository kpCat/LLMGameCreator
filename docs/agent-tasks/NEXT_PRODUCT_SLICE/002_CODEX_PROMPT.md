Работай в репозитории `LLMGameCreator`.

Выполни задачу из файла:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/002_COMPOSABLE_MODULE_SELECTION_UI.md
```

Это большая продуктовая задача, но ограниченная одним subsystem: Capability Picker / Capability Composer v2 selection.

Не делай documentation-only.
Не трогай runtime/package/Lua/solution/csproj/devflow scripts.
Не начинай Package Assembly.
Не запускай git-команды.

Рекомендуемый reasoning level для Codex: High.

Сначала прочитай:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CAPABILITY_COMPOSER_V2_SPEC.md
- docs/CAPABILITY_COMPOSER_V2_RU_GLOSSARY.md
- docs/PRODUCT_SLICE_002_COMPOSABLE_MODULE_SELECTION_UI.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/002_COMPOSABLE_MODULE_SELECTION_UI.md

Потом читай только нужные source/test files из task spec.

Обязательные проверки:
```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Capability"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~CapabilityPicker"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Финальный отчёт дай на русском.
