# LLMGameCreator — итоговая концепция

`LLMGameCreator` — это не чат, который пишет готовую игру одним prompt-ом.

Цель проекта — создать редактор и генератор `GamePackage` для data-driven игр, где:

- LLM помогает проектировать и генерировать данные, Lua, задания и ассеты;
- игра хранится как набор JSON/Lua/assets/metadata;
- runtime исполняет готовый пакет без LLM;
- крупный мир создаётся не размером контекста модели, а процедурными правилами, Lua-генераторами, индексами и маленькими generation jobs;
- визуальный player в будущем может быть реализован на Unity, но Unity не должен содержать конкретную игровую логику.

## Основная схема

```text
LLMGameCreator WinForms Editor
  -> GamePackage
     -> JSON definitions
     -> typed Lua scripts
     -> asset catalog
     -> asset generation requests
     -> maps/chunks/generators
     -> dialogues/quests/entities/items/abilities
     -> validation reports
     -> generation history

Runtime Player
  -> loads GamePackage
  -> executes RuntimeCommands
  -> runs typed Lua in sandbox
  -> renders map/entities/dialogues/assets
  -> saves deterministic state
```

## Что принципиально запрещено

- Runtime не вызывает LLM.
- Unity Player не знает конкретную игру заранее.
- LLM-generated данные не применяются напрямую без draft/validation/apply pipeline.
- Lua не должен напрямую мутировать C# `GameState`.
- Ассеты не должны быть зашиты в код.
- UI не должен читать/писать JSON напрямую.
- `MainForm` не должен превращаться в God Form.

## Что является источником правды

Источник правды — `GamePackage`.

Редактор, Unity Player, WinForms preview, валидатор, ассетный пайплайн и генератор должны работать вокруг одного формата.

## Почему не runtime LLM generation

Runtime LLM generation исключается намеренно:

- нестабильно;
- медленно;
- зависит от модели и контекста;
- плохо воспроизводится;
- сложно валидируется;
- ломает сохранения и баланс.

LLM используется только в authoring/generation phase.

## Главная идея масштабирования

Большая игра не должна требовать большого prompt-а.

Вместо этого:

```text
Design Bible
  -> World Rules
  -> Prototypes
  -> Lua Generators
  -> Asset Requirements
  -> Generation Jobs
  -> Validation
  -> GamePackage
```

Runtime получает уже готовые правила и сам создаёт мир через deterministic procedural generation.
