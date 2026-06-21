Работай в репозитории `LLMGameCreator`.

Выполни задачу из файла:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/009_GENERATED_NPC_ENCOUNTER_MAP_PLACEMENT.md
```

Это bounded product slice: generated NPCs и encounters должны появиться на Runtime Preview map как preview markers.

Не делай documentation-only.
Не трогай Unity/Lua/generator-library/solution/csproj.
Не вызывай LLM/provider/LM Studio.
Не запускай git-команды.
Не переписывай DefaultGameRuntime.
Не меняй GamePackage schema.
Не реализуй настоящую симуляцию combat/dialogue/inventory/quest engine.
Не исполняй generated effects.

Рекомендуемый reasoning level для Codex: High.

Сначала прочитай:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/PRODUCT_SLICE_009_GENERATED_NPC_ENCOUNTER_MAP_PLACEMENT.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/009_GENERATED_NPC_ENCOUNTER_MAP_PLACEMENT.md

Потом читай только нужные source/test/script files из task spec.

Обязательные проверки:
```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~MapPlacement"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-runtime-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario expanded-contract-batch-smoke
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-content-interaction-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario active-package-quest-dialogue-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-map-placement-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Финальный отчёт дай на русском.
