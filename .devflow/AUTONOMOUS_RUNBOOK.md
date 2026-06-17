# AUTONOMOUS_RUNBOOK.md — операционный протокол автономной разработки

Этот файл обязателен для локального агента. Не импровизируй вокруг него.

## 0. Перед стартом

Рабочая директория должна быть корнем решения:

```text
c:\Users\endim\LLMGameCreator
```

Проверь наличие:

```text
LLMGameCreator.sln
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
.devflow/TASK_GRAPH.json
.devflow/STOP_CONDITIONS.md
.devflow/VERIFICATION_MATRIX.md
.devflow/CONTEXT_BUDGET_POLICY.md
.devflow/DEFINITION_OF_DONE.md
.devflow/CODE_QUALITY_AND_STYLE.md
.devflow/PHASE_PLAN_INDEX.md
```

Если чего-то нет — остановись и запиши блокер в `.devflow/BLOCKERS.md`.

## 1. Обязательная ориентация

Перед любой нетривиальной задачей прочитай:

```text
.devflow/LOCAL_AGENT_ROLE.md
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

Если задача использует `Task source = agent_task_spec`, прочитай:

```text
docs/agent-tasks/000_INDEX.md
```

Затем читай ровно один task spec, указанный в `NEXT_TASK.md`. Do not read all task specs.

Если задача касается текущего M4.1 strict LLM evaluation — прочитай:

```text
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
docs/GENERATOR_PLAN_ARTIFACT_REVIEW_UI.md
```

Не читай весь `docs/` без необходимости. Используй `docs/CONTEXT_INDEX.md` как роутинг.

## 2. Выбор задачи

1. Открой `.devflow/NEXT_TASK.md`.
2. Найди task source/id.
3. Если `Task source = task_graph`, найди задачу в `.devflow/TASK_GRAPH.json`.
4. Если `Task source = phase_plan`, работай по `.devflow/RECURSIVE_TASK_SELECTION_PROTOCOL.md` и прочитай ровно один phase plan file.
5. Если `Task source = agent_task_spec`, прочитай `docs/agent-tasks/000_INDEX.md`, shared quality docs listed there, and exactly one task spec file.
6. Проверь status/approval/allowed files/proof tests/system gates.

Если задача заблокирована или требует approval — остановись и обнови `.devflow/BLOCKERS.md`.

## 3. Мини-план перед изменениями

Перед изменением файлов создай краткий план в `.devflow/CURRENT_RUN.md`:

```text
Task id:
Goal:
Source docs read:
Task spec:
Target files:
Local analogs found:
Expected proof assertions:
Non-goals:
Expected checks:
Diff hygiene risks:
Risk:
```

Для кодовой задачи обязательно найди 2-3 локальных аналога существующего паттерна. Не пиши код по воображаемой архитектуре.

## 4. Ограничение изменения файлов

По умолчанию:

```text
max changed files per task: 8
```

Если нужно больше — остановись и предложи разбиение.

Не меняй одновременно:

```text
UI и storage
runtime и LLM generation
schema и validator
Lua executor и package assembly
большой refactor и feature behavior
```

## 5. Изменение кода

Делай минимальный patch под задачу.

Правила:

```text
- сохраняй существующий стиль;
- не переписывай unrelated code/tests механически;
- тесты должны assert-ить exact diagnostic/state/count/order, если это часть контракта;
- не добавляй silent fallback, если ошибка должна быть видна;
- не меняй public contracts без явного указания task-а;
- не добавляй TODO вместо реализации;
- соблюдай .devflow/CODE_QUALITY_AND_STYLE.md;
- соблюдай shared quality docs from docs/agent-tasks/.
```

## 6. Проверки после изменения

Всегда запускай:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Финальная проверка задачи должна идти через `check-all.ps1`, если она не заблокирована внешней причиной.

## 7. Repair loop

Если build/test/checks упали:

```text
1. Прочитай конкретную ошибку.
2. Определи, это ошибка твоего patch-а или существующая baseline-проблема.
3. Сделай максимум 2 repair attempts.
4. После каждой repair attempt снова запускай релевантную проверку.
5. Если после 2 попыток не исправлено — остановись и запиши блокер.
```

Запрещено удалять/ослаблять тесты ради прохождения.

## 8. Proof-test gate

Перед финальным отчётом убедись:

```text
- tests are not weak;
- diagnostic behavior asserts exact codes when applicable;
- state/count/order assertions are exact when behavior requires them;
- fixtures/goldens are small and deterministic;
- existing readable style was preserved.
```

Use:

```text
docs/agent-tasks/_TEST_QUALITY_RULES.md
docs/agent-tasks/_FIXTURE_AND_GOLDEN_RULES.md
```

## 9. Diff hygiene gate

Перед финальным отчётом убедись:

```text
- final intended changed files match allowed files;
- no generated run/log/TRX/build outputs are intended source changes;
- no unrelated formatting churn;
- no .sln/.csproj/dependency changes unless explicitly allowed.
```

If git commands are forbidden, do not run git. Report the files you intentionally edited and any uncertainty.

Use:

```text
docs/agent-tasks/_DIFF_HYGIENE_RULES.md
```

## 10. Завершение задачи

После успешных проверок обнови:

```text
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Финальный отчёт должен идти по `.devflow/RUN_REPORT_TEMPLATE.md`.

Задача считается done только если выполнен `.devflow/DEFINITION_OF_DONE.md`.

По умолчанию остановись после одной задачи. Если сомневаешься — остановись.
