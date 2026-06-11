# Правила разработки LLMGameCreator

Эти правила обязательны для Codex, ChatGPT и ручной разработки.

## UI

1. `MainForm` — только shell.
2. Каждая страница WinForms — отдельный `UserControl`.
3. Каждая страница имеет отдельный presenter/controller/service boundary.
4. UI не читает и не пишет JSON напрямую.
5. UI не вызывает LLM напрямую.
6. UI не исполняет игровые эффекты.
7. Runtime Preview — только frontend, не источник игровой логики.

## Domain/Application/Runtime

8. `Domain` не зависит от UI, Infrastructure, Runtime, Generation, Scripting и DI.
9. Runtime не зависит от UI, LLM, ComfyUI, Fooocus и файловой системы.
10. Runtime должен запускаться headless.
11. Runtime работает только через команды и события.
12. Rendering/audio не меняют `GameState`.
13. GamePackage — единственный источник правды для готовой игры.

## LLM/Generation

14. Runtime никогда не вызывает LLM.
15. LLM используется только в editor/generation pipeline.
16. LLM создаёт draft, proposal, plan, Lua, asset request или data patch.
17. LLM output не применяется без validation.
18. Большая генерация всегда разбивается на jobs.
19. LLM получает `ContextPack`, а не весь проект.
20. Generation jobs сохраняются и должны быть возобновляемыми.
21. Поддерживаются несколько LLM endpoints, включая LAN.

## Lua

22. Lua-файлы строго типизированы: prototype/generator/behavior/interaction/formula/event/migration.
23. Каждый тип Lua имеет отдельный sandbox API.
24. Lua не имеет прямого доступа к C# `GameState`.
25. Lua возвращает drafts/effects/actions/chunks, которые валидируются runtime.
26. Lua не имеет доступа к filesystem/network/process/OS/debug API.
27. Random в Lua доступен только через `ctx`/`llmgc.random`.
28. Prototype Lua создаёт данные, Runtime Lua реагирует на события.
29. Базовая Lua-библиотека хранится в проекте и не должна каждый раз генерироваться LLM.

## Assets

30. Ассеты являются data-driven сущностями.
31. Игровые сущности ссылаются на `assetId`, а не на прямой путь.
32. Для spritesheet/tileset/portrait-set/sound/music есть asset contracts.
33. Asset generation выполняется только в editor pipeline.
34. ComfyUI/Fooocus не являются частью runtime.
35. Runtime должен работать с fallback-ассетами.
36. Missing asset — validation warning/error, а не crash.

## Unity Player

37. Unity Player является универсальным frontend/player, а не редактором конкретной игры.
38. Unity Player не содержит игровой логики конкретной игры.
39. Unity Player загружает GamePackage и отображает RuntimeEvents.
40. Общие контракты GamePackage должны быть совместимы с Unity.

## Data model

41. Опыт, уровни, деньги, мана, здоровье и ресурсы не являются обязательными полями игрока.
42. Stats/resources/progressions объявляются в данных игры.
43. Способности — data-driven сущности с conditions, stages, costs, effects, formulas.
44. Прокачка реализуется через ProgressionDefinition и может иметь разные режимы.
45. Формулы хранятся как DSL или typed Lua, не как C# eval.
46. Взаимодействия моделируются через InteractionDefinition.
47. Бой — частный случай encounter/interaction, но может иметь отдельный RuntimeSystem.
48. Отсутствие системы в конкретной игре — нормальный сценарий.

## Storage

49. Исходники игры: JSON/Lua/assets/docs.
50. SQLite допустим для cache/index/jobs/saves/chunks, но не обязан быть source of truth.
51. Большие игры не хранятся одним огромным JSON.
52. Используются индексы и summaries для context packs.

## DI/logging/tests

53. DryIoc используется только в composition root.
54. Запрещены service locator и `Container.Resolve()` внутри бизнес-кода.
55. Запрещены God Services и God Forms.
56. Ошибки не глотаются.
57. Логи пишутся структурированно.
58. Тесты минимальные, но обязательные: composition, load, validate, runtime start, first command.
59. Новая фича не добавляется, если ломает smoke-набор.

## Ограничение scope

60. Сначала vertical slice, потом расширение.
61. Не добавлять Unity, ComfyUI, Lua engine, SQLite и combat одним патчем.
62. Каждый патч должен иметь понятную цель и ограниченный список файлов.
