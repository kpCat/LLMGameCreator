# PROJECT_FACTS.md — компактная карта проекта

Локальный путь пользователя:

```text
c:\Users\endim\LLMGameCreator
```

Файл решения:

```text
LLMGameCreator.sln
```

Основные проекты решения:

```text
src/LLMGameCreator.Domain/LLMGameCreator.Domain.csproj
src/LLMGameCreator.GamePackage/LLMGameCreator.GamePackage.csproj
src/LLMGameCreator.Runtime.Abstractions/LLMGameCreator.Runtime.Abstractions.csproj
src/LLMGameCreator.Runtime/LLMGameCreator.Runtime.csproj
src/LLMGameCreator.Scripting/LLMGameCreator.Scripting.csproj
src/LLMGameCreator.Generation/LLMGameCreator.Generation.csproj
src/LLMGameCreator.AssetPipeline/LLMGameCreator.AssetPipeline.csproj
src/LLMGameCreator.Application/LLMGameCreator.Application.csproj
src/LLMGameCreator.Infrastructure/LLMGameCreator.Infrastructure.csproj
src/LLMGameCreator.WinForms/LLMGameCreator.WinForms.csproj
tests/LLMGameCreator.Tests/LLMGameCreator.Tests.csproj
```

Тестовый проект:

```text
tests/LLMGameCreator.Tests/LLMGameCreator.Tests.csproj
```

Target framework тестов:

```text
net8.0-windows
```

Обязательные начальные документы для любых generator/development задач:

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/ROADMAP_TO_FULL_GENERATOR.md
```

Текущая активная фаза на момент создания пакета:

```text
M4.1 real-model evaluation gate
```

Практическое следствие:

- нельзя переходить к широкому расширению контрактов;
- нельзя переходить к M5 Lua executor;
- нельзя переходить к M6 rich package assembly;
- можно усиливать prompt/repair/parser/validator/evaluation/reporting/simulation вокруг M4.1;
- можно добавлять безопасные devflow/diagnostics/test harness задачи, если они не мутируют GamePackage и не расширяют schema.
