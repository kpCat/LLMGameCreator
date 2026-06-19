Работай в репозитории `LLMGameCreator`.

Выполни задачу из файла:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/003_ARTIFACT_REVIEW_APPLY_PACKAGE_ASSEMBLY.md
```

Это большая продуктовая задача: впервые превращаем approved strict artifacts в draft GamePackage/package assembly.

Не делай documentation-only.
Не трогай Lua/runtime/generator-library/solution/csproj/devflow scripts.
Не расширяй все future contracts.
Не запускай git-команды.
Не вызывай LLM/provider/LM Studio из apply/assembly path.

Сначала прочитай:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/M4_1_REAL_EVALUATION_GATE_REPORT.md
- docs/PRODUCT_SLICE_003_ARTIFACT_REVIEW_APPLY_PACKAGE_ASSEMBLY.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/003_ARTIFACT_REVIEW_APPLY_PACKAGE_ASSEMBLY.md

Потом читай только нужные source/test files из task spec.

Обязательные проверки:
```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Artifact"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Package"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Финальный отчёт дай на русском.
