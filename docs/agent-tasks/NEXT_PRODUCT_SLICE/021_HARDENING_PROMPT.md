Работай в текущем рабочем дереве репозитория LLMGameCreator.

Выполни hardening-задачу:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/021_PROVIDER_JOB_PLAN_HARDENING.md
```

Не создавай ветки.
Не переключай ветки.
Не делай merge/rebase/cherry-pick.
Не запускай git-команды.
Branch management выполняет пользователь вручную.

Это не новый product slice. Нужно довести текущий S021 Provider Job Plan до merge-ready состояния.

Главные исправления:
- provider job plan errors должны влиять на materialization readiness;
- request diagnostics не должны превращаться в `request.request...`;
- добавить focused regression tests;
- не добавлять S022 в эту задачу.

Не реализуй Unity.
Не трогай Runtime.
Не меняй GamePackageDefinition/package schema.
Не трогай WinForms.
Не трогай generator-library.
Не вызывай LLM/provider.
Не исполняй generators.
Не исполняй Lua.
Не реализуй ComfyUI/Suno integration.

Рекомендуемый executor: Kilo Code first.
Рекомендуемый reasoning level: High.

Финальный отчёт дай на русском.
