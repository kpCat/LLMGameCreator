Работай в репозитории `LLMGameCreator`.

Выполни задачу из файла:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/001_CAPABILITY_COMPOSER_V2_FOUNDATION.md
```

Ключевой смысл:
это большая продуктовая задача, но ограниченная. Нужно улучшить Capability Picker и заложить non-breaking основу Capability Composer v2.

Не делай документацию-only.
Не трогай runtime/package/Lua/solution/csproj/devflow scripts.
Не начинай M5/M6/M6-lite implementation.
Не запускай git-команды.

Сначала прочитай:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CAPABILITY_COMPOSER_V2_SPEC.md
- docs/CAPABILITY_COMPOSER_V2_RU_GLOSSARY.md
- docs/PRODUCT_SLICE_001_CAPABILITY_COMPOSER_V2_FOUNDATION.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/001_CAPABILITY_COMPOSER_V2_FOUNDATION.md

Потом читай только нужные source/test files.

Обязательные проверки:
```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Capability"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Финальный отчёт дай на русском.
