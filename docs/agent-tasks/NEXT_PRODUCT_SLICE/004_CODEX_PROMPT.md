Работай в репозитории `LLMGameCreator`.

Выполни задачу из файла:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/004_HEADLESS_PRODUCT_SMOKE_RUNNER.md
```

Это большая, но ограниченная задача: автоматизировать ручной baseline flow через headless product smoke.

Не делай documentation-only.
Не трогай runtime/Lua/WinForms/generator-library/solution/csproj.
Не вызывай LLM/provider/LM Studio.
Не запускай git-команды.

Рекомендуемый reasoning level для Codex: High.

Сначала прочитай:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/M4_1_REAL_EVALUATION_GATE_REPORT.md
- docs/PRODUCT_SLICE_004_HEADLESS_PRODUCT_SMOKE_RUNNER.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/004_HEADLESS_PRODUCT_SMOKE_RUNNER.md

Потом читай только нужные source/test/script files из task spec.

Обязательные проверки:
```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Package"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Финальный отчёт дай на русском.
