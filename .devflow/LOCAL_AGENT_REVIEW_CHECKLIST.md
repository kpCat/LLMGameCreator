# LOCAL_AGENT_REVIEW_CHECKLIST.md — самопроверка локального агента

Используй этот checklist перед финальным отчётом и перед переходом к следующей задаче.

## 1. Scope check

```text
[ ] Я выполнил ровно текущий task.
[ ] Я не добавил новую цель.
[ ] Я не трогал unrelated files.
[ ] Changed files не превышают лимит task-а.
[ ] Нет массового formatting/rename.
```

## 2. Architecture check

```text
[ ] Runtime не вызывает LLM/provider/UI.
[ ] UI не владеет бизнес-логикой.
[ ] UI не пишет GamePackage JSON напрямую.
[ ] LLM output не применяется без validation/review/apply boundary.
[ ] Schema/public contracts не изменены без explicit approval.
[ ] Новые зависимости/проекты не добавлены без approval.
```

## 3. Code quality check

```text
[ ] Использован локальный паттерн из 2-3 аналогов.
[ ] Нет God Service.
[ ] Нет catch с потерей ошибки.
[ ] Нет silent fallback вместо diagnostic.
[ ] Нет TODO вместо реализации.
[ ] Diagnostic codes стабильные.
[ ] Null/empty/error cases обработаны явно.
```

## 4. Verification check

```text
[ ] Запущен .devflow/scripts/check-all.ps1.
[ ] Unexpected warnings отсутствуют.
[ ] Tests passed.
[ ] Для behavior добавлены tests/fixtures по VERIFICATION_MATRIX.
[ ] Для LLM-facing code использованы fake/corpus tests.
[ ] Реальный LLM/provider не использован в tests.
```

## 5. Documentation/state check

```text
[ ] CURRENT_RUN.md обновлён.
[ ] NEXT_TASK.md обновлён или явно оставлен с причиной.
[ ] BLOCKERS.md обновлён, если есть blocker.
[ ] RUN_REPORT_TEMPLATE.md использован для отчёта.
[ ] Если milestone/current gate изменился, предложено обновить CURRENT_GENERATOR_STATE.md + .json, но не выдумано самовольно.
```

## 6. Stop condition check

```text
[ ] Я проверил STOP_CONDITIONS.md.
[ ] Ни один stop condition не проигнорирован.
[ ] Если был stop condition, работа остановлена.
```

## 7. Final answer format

Финальный отчёт:

```text
Task id:
Summary:
Changed files:
Tests/checks:
Warnings:
Risks:
Blocked/needs user decision:
Next task suggestion:
```
