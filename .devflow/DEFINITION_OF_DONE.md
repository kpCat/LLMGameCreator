# DEFINITION_OF_DONE.md — когда задача считается завершённой

Task считается done только если выполнены все применимые пункты.

## Universal Done

```text
[ ] Task id понятен и найден в TASK_GRAPH, phase plan, or docs/agent-tasks spec.
[ ] Scope не расширен.
[ ] Non-goals соблюдены.
[ ] Stop conditions проверены.
[ ] Changed files <= task limit.
[ ] Нет hidden build/test failures.
[ ] .devflow/scripts/check-all.ps1 passed.
[ ] CURRENT_RUN.md обновлён.
[ ] NEXT_TASK.md обновлён или stop reason записан.
[ ] Финальный report написан по RUN_REPORT_TEMPLATE.md.
```

## Agent Task Spec Done

If `Task source = agent_task_spec`, task is done only if:

```text
[ ] Exactly one task spec was read.
[ ] Shared agent-task quality docs were followed.
[ ] Task spec gate status allowed execution.
[ ] Required approval was present if needed.
[ ] All Allowed files / Forbidden files boundaries were respected.
[ ] Existing patterns listed by the spec were inspected.
[ ] Every “Tests to add” item was implemented or explicitly blocked with reason.
[ ] Every applicable “System gate” passed or is explicitly blocked with reason.
[ ] Diagnostic codes match the task spec or the report explains why existing codes were reused.
[ ] Next task pointer follows the task spec.
```

Task is not done if the task spec lacks proof tests. In that case the agent must stop before implementation.

## Code Done

```text
[ ] Код следует существующим локальным паттернам.
[ ] Для code task найдены 2-3 local analogs.
[ ] Ответственность слоя не нарушена.
[ ] Нет broad refactor.
[ ] Нет TODO вместо реализации.
[ ] Diagnostic/error path deterministic.
[ ] Public contracts/schema/dependencies не изменены без approval.
[ ] Existing style preserved unless task explicitly asked for style migration.
```

## Test Done

```text
[ ] New behavior has pass test.
[ ] New behavior has fail/reject test.
[ ] Bug fix has regression test when feasible.
[ ] LLM-facing behavior has fake/corpus coverage.
[ ] Runtime-facing behavior has smoke/scenario coverage when feasible.
[ ] Tests do not call real LLM/provider/network.
[ ] Agent task spec proof tests are represented in the diff.
[ ] Diagnostic behavior asserts exact diagnostic code unless explicitly allowed otherwise.
[ ] Count/order/state behavior is asserted exactly when it is part of the contract.
[ ] Tests were not weakened/deleted to make build pass.
```

## Fixture/Golden Done

```text
[ ] Fixtures are small, named by scenario, and deterministic.
[ ] Golden/snapshot files are human-readable and minimal.
[ ] Generated logs/run outputs are not used as source fixtures unless explicitly minimized/redacted by the task.
[ ] Fixture loader follows existing pattern or task explains why a new local helper is needed.
[ ] Raw JSON/Markdown/Lua strings remain readable; raw string literals are preferred when they improve readability.
```

## Diff Hygiene Done

```text
[ ] Final changed files match task allowed files.
[ ] No generated run artifacts/logs/TRX/build outputs are part of the final intended diff.
[ ] No unrelated formatting churn.
[ ] No .sln/.csproj/dependency changes unless explicitly allowed.
[ ] No future-phase files were edited unless the task explicitly targeted docs/planning for that phase.
```

If the agent cannot use git commands, it must still report the files it intentionally changed and any uncertainty about generated/untracked outputs.

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
- NEXT_TASK points to impossible/blocked task without explanation;
- agent_task_spec required proof tests were skipped;
- implementation touched files outside the task spec allowed files;
- diagnostic tests check only “failed” instead of exact code when exact code is part of the contract;
- generated run artifacts or logs are part of the final intended diff.
```
