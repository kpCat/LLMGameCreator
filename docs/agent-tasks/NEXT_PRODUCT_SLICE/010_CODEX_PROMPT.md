Работай в репозитории `LLMGameCreator`.

Выполни задачу:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/010_OFFICIAL_PLAN_CONTENT_LANGUAGE_POLICY.md
```

Это bounded foundation slice:
- зафиксировать официальный план продукта/архитектуры;
- добавить Content Language Policy foundation;
- сделать так, чтобы будущие LLM artifact generation requests получали инструкцию генерировать player-facing content на выбранном языке, по умолчанию `ru`;
- сохранить technical ids ASCII/kebab_case;
- добавить non-blocking language diagnostics;
- добавить smoke `content-language-policy`.

Не делай documentation-only.
Не делай переводчик.
Не переписывай runtime.
Не меняй `GamePackageDefinition.cs`.
Не трогай `.sln`/`*.csproj`.
Не трогай `generator-library`.
Не вызывай реальный LLM/provider в тестах.
Не запускай git-команды.

Рекомендуемый reasoning level: High.

Обязательные проверки указаны в task file. Финальный отчёт дай на русском.
