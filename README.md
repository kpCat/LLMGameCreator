# LLMGameCreator

`LLMGameCreator` — WinForms-редактор и генератор `GamePackage` для data-driven игр.

Проект предназначен не для генерации готовой игры одним prompt-ом, а для поэтапного создания, проверки и исполнения структурированного игрового пакета.

Главная идея:

* LLM используется только в editor/generation pipeline;
* готовая игра описывается через `GamePackage`;
* runtime/player исполняет `GamePackage` без LLM;
* игровая логика должна быть data-driven и проверяемой;
* генерация больших игр должна дробиться на отдельные jobs/context packs, а не зависеть от размера LLM context.

## Текущий статус

Проект находится на ранней стадии, но уже содержит рабочий вертикальный срез редактора, пакета, runtime, validation и тестов.

Включено:

* WinForms editor shell;
* отдельные `UserControl`-страницы;
* DryIoc composition root;
* GamePackage contract;
* Domain-модели для карты, сущностей, ассетов, Lua-скриптов, диалогов, способностей, квестов, систем и runtime-состояний;
* JSON storage для `package.json` и настроек;
* application services для текущего проекта, пакета и validation;
* editor-side OpenAI-compatible / LM Studio chat client;
* asset pipeline abstractions;
* generation pipeline models;
* headless runtime services;
* runtime commands для movement, interaction, inventory, resources, crafting, loot, transactions, equipment, containers, harvesting, encounters, quests, dialogues and factions;
* WinForms Runtime Preview для быстрой отладки;
* WinForms Runtime Simulator для расширенной проверки runtime-команд;
* Runtime snapshot serialization/store для debug/simulator сценариев;
* Lua scripting abstractions;
* prototype Lua executor/sandbox;
* generator library registry metadata/import workflow;
* sample `samples/minimal-map-game`;
* smoke/contract/validator/runtime tests.

## Что принципиально не делает runtime

Runtime не должен:

* вызывать LLM;
* вызывать ComfyUI/Fooocus;
* генерировать ассеты;
* зависеть от WinForms UI;
* быть editor pipeline;
* исполнять произвольный LLM-generated код без validation/apply workflow.

Runtime должен оставаться headless и command/event driven.

`Runtime Preview` и `Runtime Simulator` являются debug/editor frontend-ами. Они могут использовать runtime abstractions для проверки поведения, но не являются финальным player-ом готовой игры.

## GamePackage

## Current generator workflow

Current source-of-truth handoff:
- docs/CURRENT_GENERATOR_STATE.md
- docs/CONTEXT_INDEX.md
- docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md

Current phase:
Goal 001 reached Product Slice 037: Runtime Preview now exposes a headless-proven generated microgame loop through the existing one-click generated preview path. The deterministic path runs Slice 029 generated plan, Slice 030 Formula/Effect/Action Registry Foundation, Slice 031 Tiny Generated Runtime Loop, Slice 032 Generated Package MVP, Slice 033 Visible Generated Playable Preview, Slice 035 active goal/progress projection and Slice 036 challenge/reward/completion projection, then writes the S037 acceptance snapshot under `.llmgc/procedural/generated-microgame-loop/`.

Next practical step:
Manual Microgame Loop Verification: launch the WinForms app, open Runtime Preview, press `Generate Preview`, press `Start`, then manually confirm the active goal, objective, interaction/challenge, reward and completion state are readable before another Codex feature slice is selected.

`GamePackage` — runtime source of truth для готовой игры.

Он описывает игровые данные и контракты, которые должен уметь загрузить отдельный runtime/player.

Типичные части пакета:

* JSON definitions;
* maps/chunks;
* entities/components;
* systems;
* dialogues;
* quests;
* abilities;
* interactions;
* items/resources;
* asset catalog;
* Lua script metadata;
* validation reports;
* generation history.

Документация, workflow profiles, context indexes и generation notes являются authoring references. Они помогают редактору и агентам, но не должны становиться runtime source of truth.

## Lua

Lua в проекте разделяется по назначению:

* `prototype`;
* `generator`;
* `behavior`;
* `interaction`;
* `formula`;
* `event`;
* `migration`.

LLM-generated Lua не должен напрямую мутировать C# `GameState`.

Ожидаемый workflow:

1. LLM создаёт draft/proposal/script.
2. Validator проверяет тип, manifest, capabilities, path, imports and contracts.
3. Application pipeline принимает или отклоняет результат.
4. Runtime получает только проверенные effects/actions/data.

На текущем этапе Lua runtime/generator/behavior/interaction execution не является финальной runtime-подсистемой. Prototype Lua sandbox используется как ограниченный экспериментальный слой.

## Asset pipeline

Ассеты являются data-driven сущностями.

Игровые сущности должны ссылаться на ассеты через `assetId`, а не через hardcoded filesystem paths.

Asset generation providers, такие как ComfyUI/Fooocus, относятся к editor pipeline и не являются частью runtime.

Runtime должен иметь fallback-поведение для отсутствующих ассетов.

## Основные проекты

```text
src/
  LLMGameCreator.Domain/
    Domain contracts and game definitions.

  LLMGameCreator.GamePackage/
    Root GamePackage model and package path conventions.

  LLMGameCreator.Runtime.Abstractions/
    Runtime command/state/event interfaces.

  LLMGameCreator.Runtime/
    Headless runtime implementation.

  LLMGameCreator.Scripting/
    Script engine abstractions and prototype Lua executor.

  LLMGameCreator.Generation/
    LLM authoring/generation models.

  LLMGameCreator.AssetPipeline/
    Asset generation provider abstractions and jobs.

  LLMGameCreator.Application/
    Application services, validation and use-cases.

  LLMGameCreator.Infrastructure/
    JSON storage, settings persistence, logging and external editor-side integrations.

  LLMGameCreator.WinForms/
    WinForms editor shell and pages.
```

## Важные папки

```text
docs/
  Architecture, development rules, package format, validation strategy and agent guidance.

samples/minimal-map-game/
  Minimal GamePackage sample.

tests/LLMGameCreator.Tests/
  Smoke, contract, validator and runtime tests.

generator-library/
  Lua generator/capability library assets and manifests.

templates/
  Lua stdlib and blueprint templates.
```

## Первый запуск

Требования:

* .NET SDK, совместимый с проектом;
* Windows для WinForms editor;
* Visual Studio / VS Code / Rider по желанию.

Восстановить зависимости и собрать решение:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
```

Запустить тесты:

```powershell
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore
```

После сборки можно открыть:

```text
LLMGameCreator.sln
```

и запустить проект:

```text
LLMGameCreator.WinForms
```

## Минимальный sample

В редакторе открыть папку:

```text
samples\minimal-map-game
```

После открытия sample можно использовать:

* validation;
* Runtime Preview;
* Runtime Simulator;
* package/editor workflows.

## Runtime Preview

`Runtime Preview` — отладочная страница для быстрой проверки базового runtime-поведения.

Она не является финальным player-ом.

For the generated preview workflow, use `Generate Preview` on Runtime Preview. It runs the deterministic generated-preview pipeline, writes artifacts under `.llmgc/procedural/`, loads the generated MVP package as the current package, and then the existing `Старт` action can start the preview.

## Runtime Simulator

`Runtime Simulator` — расширенная отладочная страница для проверки runtime-команд, unified runtime session, сериализации состояния и snapshot-сценариев.

Он предназначен для разработки и диагностики runtime-систем.

## LLM / LM Studio / OpenAI-compatible API

Проект содержит editor-side OpenAI-compatible chat client.

Он предназначен для использования в generation pipeline и не должен вызываться runtime-ом.

Runtime должен оставаться полностью работоспособным без LLM endpoint.

## Правила разработки

Перед крупными изменениями читать:

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/PROJECT_VISION.md
docs/ARCHITECTURE.md
docs/DEVELOPMENT_RULES.md
docs/CODEX_PATCH_RULES.md
```

Для конкретных задач читать только релевантные документы из `docs/`, чтобы не раздувать контекст.

Основные правила:

* не делать широкий рефакторинг без отдельной задачи;
* не смешивать UI, runtime, generation, storage and validation в одном большом патче;
* не добавлять Unity, ComfyUI, Lua engine, SQLite или combat одним патчем;
* не добавлять новую зависимость без причины;
* не менять public contracts/package format без обновления документации;
* не править `*.Designer.cs` хаотично;
* тесты должны быть минимальными и полезными.

## Definition of Done для code changes

Изменение считается завершённым, если:

1. код компилируется;
2. релевантные тесты проходят;
3. если менялся контракт — обновлена документация;
4. если менялся WinForms UI — Designer не сломан;
5. нет новой ненужной зависимости;
6. нет God Service/God Form;
7. есть краткий отчёт по изменённым файлам и проверкам.

Рекомендуемые команды проверки:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore
```

## Агентная разработка

Репозиторий рассчитан на работу с AI coding agents, но агент должен работать ограниченными патчами.

Базовый workflow для агента:

1. Прочитать `AGENTS.md`.
2. Прочитать `docs/CONTEXT_INDEX.md`.
3. Прочитать только релевантные docs.
4. Найти 2-3 локальных аналога.
5. Составить краткий план.
6. Внести минимальный patch.
7. Запустить build/tests.
8. Если build/tests упали — исправить и повторить.
9. В финальном отчёте перечислить изменённые файлы и команды проверки.

Агент не должен читать весь репозиторий без необходимости и не должен выполнять git-команды без прямого запроса пользователя.

## Текущие ограничения

Пока не является завершённым production game engine:

* Unity Player не реализован как полноценный frontend/player;
* ComfyUI/Fooocus не являются runtime-подсистемами;
* Lua execution pipeline не является финальным runtime-механизмом;
* editor UI покрывает только часть будущих сущностей;
* runtime systems развиваются поэтапно;
* GamePackage format может расширяться, но должен оставаться документированным и валидируемым.

## Лицензия

Copyright © 2026 Рауль Ендимион. All rights reserved.

No license is granted to use, copy, modify, distribute, sublicense, or sell this software without explicit written permission.
