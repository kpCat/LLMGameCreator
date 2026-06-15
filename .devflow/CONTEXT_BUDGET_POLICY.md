# CONTEXT_BUDGET_POLICY.md — политика экономии контекста для локального агента

Цель: локальный агент должен работать по точным рельсам и не раздувать контекст чтением всего проекта.

## Основной принцип

Не читай всё подряд. Читай только то, что нужно для текущей задачи.

```text
context = role + runbook + stop conditions + next task + task source docs + target files + 2-3 local analogs + failing logs
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

### Tier 1 — читать по типу задачи

Используй `docs/CONTEXT_INDEX.md` и `.devflow/PHASE_PLAN_INDEX.md`.

Примеры:

```text
LLM/evaluation task -> docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
strict generation task -> docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
validation task -> docs/VALIDATION_STRATEGY.md
runtime task -> runtime read set from docs/CONTEXT_INDEX.md
WinForms task -> docs/WINFORMS_DESIGNER_RULES.md + target page + 1-2 analog pages
```

### Tier 2 — target files

Перед patch-ем найди и прочитай:

```text
- конкретные target files;
- 2-3 локальных аналога существующего паттерна;
- существующие tests рядом с аналогичным behavior;
- только нужные fixtures/samples.
```

### Tier 3 — запрещённое чтение без причины

```text
- весь docs/;
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
max source code files before planning: 12
max local analog files: 3
max target files to patch: 8
max log files: 3 latest/relevant
```

Если нужно больше — остановись и предложи разбиение.

## Как читать фазовые планы

Не читай все файлы `.devflow/phase-plans/`.

Алгоритм:

```text
1. Прочитай .devflow/PHASE_PLAN_INDEX.md.
2. Определи текущую phase по NEXT_TASK.md или CURRENT_GENERATOR_STATE.
3. Прочитай ровно один phase plan file.
4. Возьми из него ровно один task card.
5. Если task card ссылается на docs/code, читай только этот read set.
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
