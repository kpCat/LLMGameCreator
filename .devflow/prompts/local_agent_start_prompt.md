Ты работаешь в проекте LLMGameCreator.

Рабочий каталог:

```text
c:\Users\endim\LLMGameCreator
```

Твоя задача — работать как автономный исполнитель по `.devflow`, а не как свободный архитектор.

Сначала прочитай строго в этом порядке:

```text
.devflow/LOCAL_AGENT_ROLE.md
.devflow/AUTONOMOUS_RUNBOOK.md
.devflow/STOP_CONDITIONS.md
.devflow/CONTEXT_BUDGET_POLICY.md
.devflow/DEFINITION_OF_DONE.md
.devflow/CODE_QUALITY_AND_STYLE.md
.devflow/LOCAL_AGENT_REVIEW_CHECKLIST.md
.devflow/RECURSIVE_TASK_SELECTION_PROTOCOL.md
.devflow/TASK_GRAPH.json
.devflow/NEXT_TASK.md
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
```

Затем:

1. Возьми task id/source из `.devflow/NEXT_TASK.md`.
2. Если `Task source = task_graph`, найди задачу в `.devflow/TASK_GRAPH.json`.
3. Если `Task source = phase_plan`, прочитай `.devflow/PHASE_PLAN_INDEX.md`, затем ровно один релевантный phase plan file. Не читай все phase-plans.
4. Если `Task source = agent_task_spec`, прочитай `docs/agent-tasks/000_INDEX.md`, затем ровно один task spec file из `.devflow/NEXT_TASK.md`. Не читай все specs.
5. Проверь gate/status/blocked_by/approval/allowed files/proof tests/system gates.
6. Если задача заблокирована, locked, lacks proof tests, or requires missing approval — остановись и обнови `.devflow/BLOCKERS.md`.
7. Выполни только эту задачу.
8. Не выполняй git-команды.
9. Не меняй production-код, если задача baseline/devflow/docs-only.
10. Для code task найди 2-3 local analogs before patch.
11. После изменений или baseline-запуска выполни:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

12. Если build/test/checks упали — сделай максимум 2 repair attempts.
13. Если сработал stop condition — остановись и обнови `.devflow/BLOCKERS.md`.
14. Перед финалом пройди `.devflow/LOCAL_AGENT_REVIEW_CHECKLIST.md` и `.devflow/DEFINITION_OF_DONE.md`.
15. В конце обнови `.devflow/CURRENT_RUN.md`, `.devflow/NEXT_TASK.md` and give report by `.devflow/RUN_REPORT_TEMPLATE.md`.

Запрещено:

- менять scope;
- делать широкий рефакторинг;
- менять GamePackage schema;
- добавлять зависимости;
- запускать реальные LLM/provider calls в тестах;
- разрешать runtime вызывать LLM;
- читать все phase plans;
- читать все docs/agent-tasks specs;
- продолжать работу после stop condition.

Начни с задачи из `.devflow/NEXT_TASK.md`.
