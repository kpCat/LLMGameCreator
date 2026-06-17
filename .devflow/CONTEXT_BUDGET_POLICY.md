# CONTEXT_BUDGET_POLICY.md — политика экономии контекста для локального агента

Цель: локальный агент должен работать по точным рельсам и не раздувать контекст чтением всего проекта.

## Основной принцип

Не читай всё подряд. Читай только то, что нужно для текущей задачи.

```text
context = role + runbook + stop conditions + next task + one task source + task source docs + target files + 2-3 local analogs + failing logs
```

## Уровни чтения

### Tier 0 — всегда читать

```text
.devflow/LOCAL_AGENT_ROLE.md
.devflow/AUTONOMOUS_RUNBOOK.md
.devflow/STOP_CONDITIONS.md
.devflow/CONTEXT_BUDGET_POLICY.md
.devflow/DEFINITION_OF_DONE.md
.devflow/NEXT_TASK.md
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
```

### Tier 1 — читать по типу источника задачи

Use `.devflow/RECURSIVE_TASK_SELECTION_PROTOCOL.md`.

```text
Task source = task_graph       -> read .devflow/TASK_GRAPH.json entry only
Task source = phase_plan       -> read .devflow/PHASE_PLAN_INDEX.md + exactly one phase plan
Task source = agent_task_spec  -> read docs/agent-tasks/000_INDEX.md + exactly one task spec
```

### Tier 2 — читать по типу задачи

Используй `docs/CONTEXT_INDEX.md`, `.devflow/PHASE_PLAN_INDEX.md`, and/or the selected `docs/agent-tasks/**` spec.

Examples:

```text
LLM/evaluation task -> docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
strict generation task -> docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
validation task -> docs/VALIDATION_STRATEGY.md
runtime task -> runtime read set from docs/CONTEXT_INDEX.md
WinForms task -> docs/WINFORMS_DESIGNER_RULES.md + target page + 1-2 analog pages
agent_task_spec -> only Source-of-truth docs and Existing patterns listed in the spec
```

### Tier 3 — target files

Перед patch-ем найди и прочитай:

```text
- конкретные target files;
- 2-3 локальных аналога существующего паттерна;
- существующие tests рядом с аналогичным behavior;
- только нужные fixtures/samples.
```

### Tier 4 — запрещённое чтение без причины

```text
- весь docs/;
- все docs/agent-tasks/ specs;
- весь src/;
- все Designer-файлы;
- все tests/;
- generator-library целиком;
- все samples;
- любые большие generated artifacts без необходимости.
```

## Лимиты на чтение в одной задаче

Если task явно не требует больше:

```text
max source docs: 6
max agent task specs: 1
max phase plan files: 1
max source code files before planning: 12
max local analog files: 3
max target files to patch: 8
max log files: 3 latest/relevant
```

Если нужно больше — остановись и предложи разбиение.

## Как читать фазовые планы

Не читай все файлы `.devflow/phase-plans/`.

Algorithm:

```text
1. Read .devflow/PHASE_PLAN_INDEX.md.
2. Determine current phase from NEXT_TASK.md or CURRENT_GENERATOR_STATE.
3. Read exactly one phase plan file.
4. Take exactly one task card.
5. If task card points to docs/agent-tasks, switch to agent_task_spec mode and read exactly one spec.
```

## Как читать agent task specs

Не читай все файлы `docs/agent-tasks/`.

Algorithm:

```text
1. Read docs/agent-tasks/000_INDEX.md.
2. Read exactly one task spec named by NEXT_TASK.md.
3. Verify readiness: proof tests, allowed files, gate status, approval, stop conditions.
4. Read only the task spec's Source-of-truth docs and Existing patterns to inspect.
5. If implementation needs files outside Allowed files, stop.
```

## Как работать с ошибками

При build/test failure не перечитывай проект. Читай:

```text
1. failing command log;
2. files mentioned in error;
3. local analog for the failing pattern;
4. tests mentioned in failure.
```

Если причина не локализована за 2 repair attempts — stop condition.

## Правило против контекстного мусора

Перед каждым ответом себе сформулируй:

```text
What exact file/content do I need next and why?
```

Если ответа нет — не читай файл.
