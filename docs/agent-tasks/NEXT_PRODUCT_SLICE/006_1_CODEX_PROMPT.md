Работай в репозитории `LLMGameCreator`.

Выполни задачу из файла:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/006_1_LLM_ARTIFACTS_BATCH_PRESET_DROPDOWN.md
```

Это узкая UI-задача: добавить batch preset dropdown на страницу LLM Artifacts, используя уже существующие batch presets из `GeneratorPlanStrictLlmArtifactContractCatalog`.

Не делай documentation-only.
Не добавляй новые contracts.
Не трогай package assembly/runtime/Lua/generator-library/solution/csproj.
Не вызывай LLM/provider в тестах.
Не запускай git-команды.
Не переписывай страницу широко, если можно сделать узко.

Рекомендуемый reasoning level для Codex: Medium.

Сначала прочитай:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactContractCatalog.cs
- src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/StrictLlmArtifactsPageControl.cs
- src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/StrictLlmArtifactsPresenter.cs
- src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/StrictLlmArtifactsViewModels.cs
- tests/LLMGameCreator.Tests/WinForms/StrictLlmArtifactsPresenterTests.cs

Обязательные проверки:
```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~StrictLlmArtifacts"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~StrictLlm"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario expanded-contract-batch-smoke
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Финальный отчёт дай на русском.
