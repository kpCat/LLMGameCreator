# Codex Patch Rules

Документ обязателен для всех задач Codex по `LLMGameCreator`.

## Общие правила

1. Работать только в `main`.
2. Не создавать ветки.
3. Не делать широкие рефакторинги без отдельной задачи.
4. Не добавлять новую архитектурную подсистему без документа.
5. Не добавлять фичу без validation/diagnostic story.
6. Не добавлять сотни тестов.
7. Не смешивать UI, runtime, generation, storage и validation.
8. Не добавлять runtime LLM generation.
9. Не добавлять Unity Player раньше milestone.
10. Не добавлять ComfyUI provider раньше asset pipeline/manual import.
11. Не править Designer.cs хаотично.

## Лимит файлов на задачу

Обычная задача:

```text
до 8 файлов
до 400 строк нового кода
1-3 теста максимум
```

Если нужно больше — остановиться и разбить задачу.

## Формат задачи

Каждая задача должна иметь:

```text
Goal
Scope
Files allowed
Files forbidden
Acceptance criteria
Validation/tests
Manual check
Non-goals
```

## Запрещённые формулировки

Плохо:

```text
улучши архитектуру
доделай генерацию
сделай runtime нормально
добавь всё для Unity
почини все баги
```

Хорошо:

```text
Добавить ScriptManifest domain model и validator для проверки script type/path/capabilities.
Менять только:
- src/LLMGameCreator.GamePackage/...
- src/LLMGameCreator.Application/Validation/...
- tests/...
Не подключать реальный Lua engine.
```

## Definition of Done

Задача завершена, если:

1. код компилируется;
2. тесты проходят;
3. UI Designer не сломан, если менялся WinForms;
4. обновлена документация, если менялся контракт;
5. нет новой зависимости без причины;
6. нет God Service/God Form;
7. есть краткий отчёт.

## Тестовая политика

Не писать сотни тестов.

Нужны:

```text
smoke tests
contract tests
validator tests
runtime critical path tests
```

Не нужны на раннем этапе:

```text
тесты каждого getter/setter
тесты дизайнерской разметки
тесты очевидных DTO
```

## Когда остановиться

Если задача требует 15+ файлов, новых понятий и нового UI одновременно — остановиться и вернуть план разделения.
