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

1. Возьми task id из `.devflow/NEXT_TASK.md`.
2. Найди задачу в `.devflow/TASK_GRAPH.json`.
3. Проверь, не заблокирована ли она.
4. Если `NEXT_TASK.md` указывает phase plan/task card, прочитай `.devflow/PHASE_PLAN_INDEX.md`, затем ровно один релевантный phase plan file. Не читай все phase-plans.
5. Выполни только эту задачу.
6. Не выполняй git-команды.
7. Не меняй production-код, если задача baseline/devflow-only.
8. После изменений или baseline-запуска выполни:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

9. Если build/test/checks упали — сделай максимум 2 repair attempts.
10. Если сработал stop condition — остановись и обнови `.devflow/BLOCKERS.md`.
11. Перед финалом пройди `.devflow/LOCAL_AGENT_REVIEW_CHECKLIST.md` и `.devflow/DEFINITION_OF_DONE.md`.
12. В конце обнови `.devflow/CURRENT_RUN.md` и дай отчёт по `.devflow/RUN_REPORT_TEMPLATE.md`.

Запрещено:

- менять scope;
- делать широкий рефакторинг;
- менять GamePackage schema;
- добавлять зависимости;
- запускать реальные LLM/provider calls в тестах;
- разрешать runtime вызывать LLM;
- продолжать работу после stop condition.

Начни с задачи из `.devflow/NEXT_TASK.md`.
