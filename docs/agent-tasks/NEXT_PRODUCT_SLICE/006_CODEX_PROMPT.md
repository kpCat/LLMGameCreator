Работай в репозитории `LLMGameCreator`.

Выполни задачу из файла:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/006_STRICT_CONTRACT_CATALOG_BATCH_GENERATION.md
```

Это большая, но ограниченная задача: расширить LLM Artifacts с 4 baseline contracts до первого набора расширенных strict contracts и batch presets.

Не делай documentation-only.
Не трогай Unity/Lua/generator-library/solution/csproj.
Не вызывай LLM/provider/LM Studio из тестов или smoke.
Не запускай git-команды.
Не переписывай runtime engine.
Не реализуй execution effects/combat/economy simulation.

Рекомендуемый reasoning level для Codex: High.

Сначала прочитай:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/PRODUCT_SLICE_006_STRICT_CONTRACT_CATALOG_BATCH_GENERATION.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/006_STRICT_CONTRACT_CATALOG_BATCH_GENERATION.md

Потом читай только нужные source/test/script files из task spec.

Обязательные проверки:
```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~StrictLlm"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~GamePackageAssembly"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~expanded"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-runtime-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario expanded-contract-batch-smoke
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Финальный отчёт дай на русском.
