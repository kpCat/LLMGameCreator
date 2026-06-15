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
4. Выполни только эту задачу.
5. Не выполняй git-команды.
6. Не меняй production-код, если задача baseline/devflow-only.
7. После изменений или baseline-запуска выполни:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

8. Если build/test/checks упали — сделай максимум 2 repair attempts.
9. Если сработал stop condition — остановись и обнови `.devflow/BLOCKERS.md`.
10. В конце обнови `.devflow/CURRENT_RUN.md` и дай отчёт по `.devflow/RUN_REPORT_TEMPLATE.md`.

Запрещено:

- менять scope;
- делать широкий рефакторинг;
- менять GamePackage schema;
- добавлять зависимости;
- запускать реальные LLM/provider calls в тестах;
- разрешать runtime вызывать LLM;
- продолжать работу после stop condition.

Начни с задачи из `.devflow/NEXT_TASK.md`.
