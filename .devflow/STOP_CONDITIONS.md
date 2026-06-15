# STOP_CONDITIONS.md — когда агент обязан остановиться

Если срабатывает любой пункт ниже, агент должен прекратить изменение файлов и записать блокер в `.devflow/BLOCKERS.md`.

## Архитектурные stop conditions

Остановись, если задача или найденное решение требует:

1. Изменить GamePackage schema.
2. Добавить новую NuGet-зависимость.
3. Добавить новый проект в solution.
4. Перенести классы между слоями проекта.
5. Изменить C# / LLM / Lua ownership boundaries.
6. Разрешить runtime вызывать LLM/provider/ComfyUI/Fooocus/WinForms.
7. Разрешить UI напрямую читать/писать JSON GamePackage.
8. Разрешить LLM output применяться без validation/review/apply boundary.
9. Разрешить Lua прямой доступ к C# `GameState`, filesystem, network, process, OS или debug API.
10. Включить широкий Lua executor без отдельной sandbox-задачи.
11. Включить M5/M6/M8 до прохождения M4.1 gate.

## Scope stop conditions

Остановись, если:

1. Нужно изменить больше 8 файлов в одной задаче.
2. Нужно изменить больше 10 файлов даже после очевидного разбиения.
3. Задача требует одновременно UI + storage + domain + runtime.
4. Требуется массовый rename.
5. Требуется массовый форматинг.
6. Требуется удалить compatibility path.
7. Требуется переписать существующий сервис целиком.
8. Текущий task не имеет acceptance criteria.
9. Ты не можешь назвать 2-3 локальных аналога паттерна.
10. Ты не понимаешь, какой слой владеет поведением.

## Build/test stop conditions

Остановись, если:

1. `dotnet build` не исправлен за 2 repair attempts.
2. `dotnet test` не исправлен за 2 repair attempts.
3. Ошибка выглядит как существующий baseline, а не результат patch-а.
4. Тест требует реальный LLM/provider вызов.
5. Для прохождения тестов хочется удалить или ослабить проверку без явного основания.
6. Ошибка не воспроизводится или не локализуется.

## Safety stop conditions

Остановись, если:

1. Нужно выполнить git-команду.
2. Нужно удалить файл.
3. Нужно изменить `.sln` без явного task-а.
4. Нужно изменить `.csproj` без явного task-а.
5. Нужно менять секреты, API keys, локальные токены или user-specific settings.
6. Нужно обращаться к интернету.
7. Нужно запускать неизвестный exe/script.
8. Нужно менять файлы вне корня решения.

## M4.1 gate stop conditions

Текущая фаза — M4.1 real-model evaluation gate.

До явного обновления current state запрещено:

1. Переходить к M5 Lua Module Registry / Executor Integration.
2. Переходить к M6 Rich GamePackage Assembly.
3. Делать broad artifact contract expansion.
4. Делать Runtime Preview repair loop.
5. Включать новые генерационные capability families как production-ready.

Разрешены только bounded задачи вокруг:

- baseline checks;
- M4.1 evaluation report analysis;
- prompt/repair/parser/validator hardening based on evaluation evidence;
- diagnostics/logging/reporting around strict generation/evaluation;
- fake/corpus/simulation tests that не мутируют package.
