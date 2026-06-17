# LOCAL_AGENT_REVIEW_CHECKLIST.md — самопроверка локального агента

Используй этот checklist перед финальным отчётом и перед переходом к следующей задаче.

## 1. Scope check

```text
[ ] Я выполнил ровно текущий task.
[ ] Я не добавил новую цель.
[ ] Я не трогал unrelated files.
[ ] Changed files не превышают лимит task-а.
[ ] Нет массового formatting/rename.
[ ] Я не выполнял следующую задачу без явного разрешения.
```

## 2. Architecture check

```text
[ ] Runtime не вызывает LLM/provider/UI.
[ ] UI не владеет бизнес-логикой.
[ ] UI не пишет GamePackage JSON напрямую.
[ ] LLM output не применяется без validation/review/apply boundary.
[ ] Schema/public contracts не изменены без explicit approval.
[ ] Новые зависимости/проекты не добавлены без approval.
[ ] Layer ownership из CODE_QUALITY_AND_STYLE.md соблюдён.
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
[ ] Existing readable style preserved.
[ ] Нет unrelated formatting churn.
```

## 4. Test quality check

```text
[ ] Новые tests являются proof tests, а не weak tests.
[ ] Tests assert exact diagnostic code when diagnostics are part of behavior.
[ ] Tests assert exact count/order/state when those are part of behavior.
[ ] Pass and fail/reject paths covered when applicable.
[ ] Fixture/golden names describe scenarios.
[ ] Fixtures/goldens are minimal and deterministic.
[ ] Tests do not call real LLM/provider/network.
[ ] Tests were not weakened/deleted to pass.
```

Перед финальным отчётом сверяйся с:

```text
docs/agent-tasks/_TEST_QUALITY_RULES.md
docs/agent-tasks/_FIXTURE_AND_GOLDEN_RULES.md
```

## 5. Verification check

```text
[ ] Запущен .devflow/scripts/check-all.ps1.
[ ] Unexpected warnings отсутствуют.
[ ] Tests passed.
[ ] Для behavior добавлены tests/fixtures по VERIFICATION_MATRIX.
[ ] Для LLM-facing code использованы fake/corpus tests.
[ ] Реальный LLM/provider не использован в tests.
```

## 6. Diff hygiene check

```text
[ ] Final intended changed files match allowed files.
[ ] Generated run artifacts/logs/TRX/build outputs are not intended source changes.
[ ] No .sln/.csproj changes unless explicitly allowed.
[ ] No generated caches/local settings were edited as source.
[ ] If git commands are forbidden, I reported changed files from my own edits and noted any uncertainty.
```

Перед финальным отчётом сверяйся с:

```text
docs/agent-tasks/_DIFF_HYGIENE_RULES.md
```

## 7. Documentation/state check

```text
[ ] CURRENT_RUN.md обновлён.
[ ] NEXT_TASK.md обновлён или явно оставлен с причиной.
[ ] BLOCKERS.md обновлён, если есть blocker.
[ ] RUN_REPORT_TEMPLATE.md использован для отчёта.
[ ] Если milestone/current gate изменился, предложено обновить CURRENT_GENERATOR_STATE.md + .json, но не выдумано самовольно.
[ ] Common quality rules added to shared docs, not copy-pasted into one-off task specs.
```

## 8. Stop condition check

```text
[ ] Я проверил STOP_CONDITIONS.md.
[ ] Ни один stop condition не проигнорирован.
[ ] Если был stop condition, работа остановлена.
```

## 9. Final answer format

Финальный отчёт:

```text
Task id:
Summary:
Changed files:
Tests/checks:
Proof-test quality:
Fixture/golden quality:
Diff hygiene:
Warnings:
Risks:
Blocked/needs user decision:
Next task suggestion:
```
