Работай в репозитории `LLMGameCreator`.

Выполни задачу из файла:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/007_GENERATED_CONTENT_INTERACTION_PREVIEW.md
```

Это bounded product slice: добавить interactive generated content browser в Runtime Preview.

Не делай documentation-only.
Не трогай Unity/Lua/generator-library/solution/csproj.
Не вызывай LLM/provider/LM Studio.
Не запускай git-команды.
Не переписывай DefaultGameRuntime.
Не реализуй настоящую симуляцию combat/dialogue/inventory/quest engine.

Рекомендуемый reasoning level для Codex: High.

Сначала прочитай:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/PRODUCT_SLICE_007_GENERATED_CONTENT_INTERACTION_PREVIEW.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/007_GENERATED_CONTENT_INTERACTION_PREVIEW.md

Потом читай только нужные source/test/script files из task spec.

Обязательные проверки:
```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~InteractionPreview"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-runtime-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario expanded-contract-batch-smoke
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-content-interaction-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Финальный отчёт дай на русском.
