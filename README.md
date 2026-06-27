# LLMGameCreator

`LLMGameCreator` — WinForms-редактор и генератор `GamePackage` для data-driven игр.

Проект предназначен не для генерации готовой игры одним prompt-ом, а для поэтапного создания, проверки, сборки и исполнения структурированного игрового пакета.

## Главная идея

- LLM используется только в editor/generation pipeline.
- Готовая игра описывается через `GamePackage`.
- Runtime/player исполняет `GamePackage` без LLM.
- Игровая логика должна быть data-driven и проверяемой.
- Генерация больших игр должна дробиться на отдельные generation jobs/context packs, а не зависеть от размера LLM context.
- Большой мир должен строиться через seed/rules/chunks/semantic packs/rule packs, а не через огромный prompt или огромный JSON-dump.

## Что является source of truth

`GamePackage` — runtime source of truth для готовой игры.

Он описывает игровые данные и контракты, которые должен загрузить runtime/player:

- JSON definitions;
- maps/chunks;
- entities/components;
- systems;
- dialogues;
- quests;
- abilities;
- interactions;
- items/resources;
- asset catalog;
- Lua script metadata;
- validation reports;
- generation history.

Документация, workflow profiles, context indexes и generation notes являются authoring references. Они помогают редактору и агентам, но не должны становиться runtime source of truth.

## Актуальное состояние проекта

README является обзором проекта, а не handoff-документом активного goal-а.

Актуальные active gate, recommended next work и source-of-truth routing находятся в:

- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Перед любой generator/Codex задачей нужно читать:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md` или `docs/CURRENT_GENERATOR_STATE.json`
4. task-specific документы, названные в текущем state/context routing

README не должен дублировать active goal, manual gate или next practical step, потому что эти данные меняются после каждого принятого goal.

## Архитектурные границы

Runtime не должен:

- вызывать LLM;
- вызывать RAG;
- вызывать ComfyUI/Fooocus или другие media providers;
- генерировать ассеты;
- зависеть от WinForms UI;
- быть editor pipeline;
- исполнять произвольный LLM-generated код без validation/apply workflow.

Runtime должен оставаться headless и command/event driven.

`Runtime Preview` и `Runtime Simulator` являются debug/editor frontend-ами. Они могут использовать runtime abstractions для проверки поведения, но не являются финальным player-ом готовой игры.

## Слои решения

```text
Domain
  Чистые модели и value objects.

GamePackage
  DTO/контракты пакета игры, совместимые с будущим player/export layer.

Runtime.Abstractions
  Команды, события, состояния и контракты исполнения.

Runtime
  Headless runtime без UI и без LLM.

Scripting
  Контракты typed Lua, script manifests, script diagnostics.

Generation
  LLM sessions, jobs, context packs, draft workflow.

AssetPipeline
  Asset catalog, asset contracts, generation requests, providers.

Application
  Use-cases: открыть проект, сохранить, валидировать, запускать preview, применять draft.

Infrastructure
  JSON storage, settings storage, logging, future SQLite/cache providers.

WinForms
  Editor shell, pages, presenters, composition root.
```

## Lua

Lua в проекте разделяется по назначению:

- `prototype`;
- `generator`;
- `behavior`;
- `interaction`;
- `formula`;
- `event`;
- `migration`.

LLM-generated Lua не должен напрямую мутировать C# `GameState`.

Ожидаемый workflow:

1. LLM создаёт draft/proposal/script.
2. Validator проверяет тип, manifest, capabilities, path, imports и contracts.
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
    Runtime commands, events and state contracts.

  LLMGameCreator.Runtime/
    Headless command/event runtime.

  LLMGameCreator.Scripting/
    Typed Lua/script manifest abstractions and prototype sandbox.

  LLMGameCreator.Generation/
    Generation jobs, context packs and LLM-facing editor models.

  LLMGameCreator.AssetPipeline/
    Asset request/provider abstractions.

  LLMGameCreator.Application/
    Application services, validators, generation workflows and use-cases.

  LLMGameCreator.Infrastructure/
    Storage, settings and infrastructure adapters.

  LLMGameCreator.WinForms/
    Editor UI shell and pages.

tests/
  LLMGameCreator.Tests/
    Smoke, contract, validator and runtime tests.
```

## Development rules for agents

For current generator/product-slice work, agents must not use README as current planning authority.

Use:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.*`
4. current task/goal document
5. only then relevant architecture/contract docs

Do not start the next goal while the current manual gate is still required.

Do not use git commands unless the user explicitly asks.

Do not change `GamePackage` schema, public runtime contracts, Unity/player code, provider execution, Lua execution or UI unless the task explicitly allows it.

## Validation

Typical validation commands are task-specific.

Common scripts:

```powershell
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\run-product-smoke.ps1 -Scenario <scenario-id>
```

A task is not complete merely because code compiles. It must satisfy the active acceptance criteria, final gate status, artifact evidence and forbidden-scope checks named by the active goal.
