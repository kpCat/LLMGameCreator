# DEFINITION_OF_DONE.md — когда задача считается завершённой

Task считается done только если выполнены все применимые пункты.

## Universal Done

```text
[ ] Task id понятен и найден в TASK_GRAPH или phase plan.
[ ] Scope не расширен.
[ ] Non-goals соблюдены.
[ ] Stop conditions проверены.
[ ] Changed files <= task limit.
[ ] Нет hidden build/test failures.
[ ] .devflow/scripts/check-all.ps1 passed.
[ ] CURRENT_RUN.md обновлён.
[ ] Финальный report написан по RUN_REPORT_TEMPLATE.md.
```

## Code Done

```text
[ ] Код следует существующим локальным паттернам.
[ ] Для code task найдены 2-3 local analogs.
[ ] Ответственность слоя не нарушена.
[ ] Нет broad refactor.
[ ] Нет TODO вместо реализации.
[ ] Diagnostic/error path deterministic.
[ ] Public contracts/schema/dependencies не изменены без approval.
```

## Test Done

```text
[ ] New behavior has pass test.
[ ] New behavior has fail/reject test.
[ ] Bug fix has regression test when feasible.
[ ] LLM-facing behavior has fake/corpus coverage.
[ ] Runtime-facing behavior has smoke/scenario coverage when feasible.
[ ] Tests do not call real LLM/provider/network.
```

## Docs Done

```text
[ ] Docs changed only when behavior/plan/state changed.
[ ] Current state docs updated together if milestone/gate changed.
[ ] No outdated instruction contradicts new docs.
[ ] New phase/task docs are linked from PHASE_PLAN_INDEX.md if they are intended for agent routing.
```

## Not Done

Task is not done if:

```text
- build/test passes only after deleting/weakening tests;
- warning was hidden instead of fixed/registered;
- task needs approval but agent continued;
- error was swallowed;
- runtime gained dependency on LLM/provider/UI;
- UI gained direct package JSON ownership;
- GamePackage schema changed silently;
- NEXT_TASK points to impossible/blocked task without explanation.
```
