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

Если задача generator/full-generator — дополнительно прочитай:

```text
docs/ROADMAP_TO_FULL_GENERATOR.md
```

Если задача касается validation — прочитай:

```text
docs/VALIDATION_STRATEGY.md
```

Если задача касается текущего M4.1 strict LLM evaluation — прочитай:

```text
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
docs/GENERATOR_PLAN_ARTIFACT_REVIEW_UI.md
```

Не читай весь `docs/` без необходимости. Используй `docs/CONTEXT_INDEX.md` как роутинг.

Если `.devflow/NEXT_TASK.md` указывает на phase plan или task card, прочитай `.devflow/PHASE_PLAN_INDEX.md`, затем ровно один релевантный файл из `.devflow/phase-plans/`. Не читай все фазовые планы за один запуск.

## 2. Выбор задачи

1. Открой `.devflow/NEXT_TASK.md`.
2. Найди id задачи.
3. Найди этот id в `.devflow/TASK_GRAPH.json`.
4. Если id не найден в `TASK_GRAPH.json`, но `NEXT_TASK.md` указывает phase plan/task card, работай по `.devflow/RECURSIVE_TASK_SELECTION_PROTOCOL.md` и прочитай ровно один phase plan file.
5. Проверь:
   - `status`;
   - `blocked_by`;
   - `requires_approval`;
   - `allowed_before_m4_1_gate_review`;
   - `max_changed_files`;
   - `required_checks`.

Если задача заблокирована, остановись и обнови `.devflow/BLOCKERS.md`.

Если задача требует approval, остановись и явно напиши, какое решение нужно от пользователя.

## 3. Мини-план перед изменениями

Перед изменением файлов создай краткий план в `.devflow/CURRENT_RUN.md`:

```text
Task id:
Goal:
Source docs read:
Target files:
Local analogs found:
Non-goals:
Expected checks:
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

- UI и storage;
- runtime и LLM generation;
- schema и validator;
- Lua executor и package assembly;
- большой refactor и feature behavior.

## 4.1. Контекстный бюджет

Перед чтением дополнительных файлов проверь `.devflow/CONTEXT_BUDGET_POLICY.md`.

Лимиты по умолчанию:

```text
max source docs: 6
max source code files before planning: 12
max local analog files: 3
max target files to patch: 8
```

Если нужно больше — остановись и предложи разбиение.

## 5. Изменение кода

Делай минимальный patch под задачу.

Правила:

- сохраняй существующий стиль;
- добавляй стабильные diagnostic codes;
- не глотай исключения;
- не добавляй silent fallback, если ошибка должна быть видна;
- для новых behavior добавляй validation/test/sample, если это требуется матрицей;
- не меняй public contracts без явного указания task-а;
- не добавляй TODO вместо реализации, если task требует завершённый behavior;
- соблюдай `.devflow/CODE_QUALITY_AND_STYLE.md`;
- перед финальным отчётом пройди `.devflow/LOCAL_AGENT_REVIEW_CHECKLIST.md`.

## 6. Проверки после изменения

Всегда запускай:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Если задача требует только быстрый build, можно сначала запустить:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\build.ps1
```

Но финальная проверка задачи всё равно должна идти через `check-all.ps1`, если она не заблокирована внешней причиной.

## 7. Repair loop

Если build/test/checks упали:

1. Прочитай конкретную ошибку.
2. Определи, это ошибка твоего patch-а или существующая baseline-проблема.
3. Сделай максимум 2 repair attempts.
4. После каждой repair attempt снова запускай релевантную проверку.
5. Если после 2 попыток не исправлено — остановись и запиши блокер.

Запрещено:

- удалять тесты ради прохождения;
- ослаблять validator без причины;
- менять production behavior только ради теста;
- менять schema ради быстрого фикса;
- делать широкий refactor ради одной ошибки.

## 8. Simulation/modeling gate

Если задача касается LLM pipeline, parser, repair, validator, package assembly или runtime smoke, проверь требования из `.devflow/MODELING_STRATEGY.md`.

Приоритет моделирования:

```text
fake client / corpus / fixtures / deterministic simulation
```

Реальные LLM-вызовы разрешены только вручную или в explicit user-triggered режимах. В автотестах реальные LLM/provider вызовы запрещены.

## 9. Завершение задачи

После успешных проверок обнови:

```text
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Если task graph требует смены статуса — предложи изменение, но не выдумывай новый roadmap.

Финальный отчёт должен идти по `.devflow/RUN_REPORT_TEMPLATE.md`.

Задача считается done только если выполнен `.devflow/DEFINITION_OF_DONE.md`.

## 10. Переход к следующей задаче

По умолчанию остановись после одной задачи.

Продолжать к следующей задаче можно только если:

- текущая задача прошла build/test/check-all;
- нет блокеров;
- следующая задача не требует approval;
- следующая задача не заблокирована M4.1 gate;
- итоговое число изменённых файлов за весь запуск остаётся разумным;
- не было признаков “пошло не туда”.

Если сомневаешься — остановись.
