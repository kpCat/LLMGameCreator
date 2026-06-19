Работай в репозитории `LLMGameCreator`.

Выполни задачу из файла:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/005_GENERATED_PACKAGE_RUNTIME_PREVIEW.md
```

Это большая, но ограниченная задача: сделать generated package content видимым в Runtime Preview и добавить headless runtime-preview smoke.

Не делай documentation-only.
Не трогай Unity/Lua/generator-library/solution/csproj.
Не вызывай LLM/provider/LM Studio.
Не запускай git-команды.
Не переписывай DefaultGameRuntime целиком.

Рекомендуемый reasoning level для Codex: High.

Сначала прочитай:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/PRODUCT_SLICE_005_GENERATED_PACKAGE_RUNTIME_PREVIEW.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/005_GENERATED_PACKAGE_RUNTIME_PREVIEW.md

Потом читай только нужные source/test/script files из task spec.

Обязательные проверки:
```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~RuntimePreview"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~GeneratedPackageRuntimePreviewSmoke"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Если добавишь scenario `generated-package-runtime-preview`, также запусти:
```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-runtime-preview
```

Финальный отчёт дай на русском.
