# Limit Budget and Goal Plan

Это честная оценка разработки через Codex/LLM. Это не обещание точных цифр, а ориентир, чтобы не сжечь лимиты снова.

## Версии

### v0.1 — Skeleton Stabilization

Текущий скелет компилируется, дизайнер открывается, есть docs/contracts, game manager, minimal runtime preview.

Оценка:

```text
2-5 goal tasks
```

### v0.2 — GamePackage Validation Core

GamePackage validator, script manifest validator, asset contract validator, map/entity reference validation, readable validation report.

```text
5-10 goal tasks
```

### v0.3 — Typed Lua Foundation

Lua engine, sandbox, Prototype Lua load, Generator Lua dry-run, forbidden API scan, sample scripts execute.

```text
6-12 goal tasks
```

### v0.4 — Procedural Map Runtime

Finite map, chunked map, generated chunks, persistent chunk state, movement/interact, runtime events.

```text
8-15 goal tasks
```

### v0.5 — Asset Pipeline / Manual Import

Asset catalog UI, asset contracts, manual import, fallback assets, validation, link assets to entities.

```text
6-12 goal tasks
```

### v0.6 — Generation Sessions / Jobs

Chat sessions, decisions, generation jobs, context packs, draft storage, apply workflow.

```text
8-16 goal tasks
```

### v0.7 — LLM Client + Draft/Patch

OpenAI-compatible/LM Studio client, local/LAN profiles, prompt builders, draft parser, validation feedback loop.

```text
8-16 goal tasks
```

### v0.8 — ComfyUI Asset Provider

Workflow profiles, ComfyUI API client, asset jobs, import outputs, validate generated files.

```text
8-15 goal tasks
```

### v0.9 — Gameplay Systems

Inventory, resources/stats, abilities, progression, dialogue, basic quests, basic interactions.

```text
15-30 goal tasks
```

### v1.0 — Unity Player Prototype

Unity BootstrapScene, GamePackage loader, tilemap rendering, entity rendering, WASD, dialogue cards, sound event playback, no hardcoded game logic.

```text
15-35 goal tasks
```

## Практический бюджет

Минимально рабочий редактор + headless/map preview:

```text
20-35 goal tasks
```

Рабочий прототип с Lua procedural chunks и validation:

```text
35-60 goal tasks
```

Версия, где реально делать простые игры с ассетами и LLM-assisted authoring:

```text
60-100 goal tasks
```

Версия с Unity Player, ComfyUI, способностями, инвентарём, квестами, процедурным миром:

```text
100-180+ goal tasks
```

## Как не повторить прошлый провал

1. Каждая задача имеет один measurable outcome.
2. После каждых 5 goal tasks — stabilization pass.
3. После каждых 10 goal tasks — architecture audit.
4. Любая новая система обязана иметь validator.
5. Нельзя добавлять UI раньше domain/contract.
6. Нельзя добавлять generation раньше validation.
7. Нельзя добавлять Unity раньше GamePackage contract.
8. Нельзя писать сотни тестов.
9. Нельзя принимать "вроде работает" без smoke scenario.
10. Если после 3 задач подряд нет проверяемого прогресса — остановить направление.

## Stop criteria

Проект нужно остановить/перепроектировать, если:

- runtime начинает зависеть от UI;
- GamePackage меняется под конкретную страницу;
- LLM output применяется без validation;
- Unity получает hardcoded game logic;
- Lua получает прямой доступ к GameState;
- Codex просит читать весь проект для каждой мелкой задачи;
- любой новый feature требует менять 20+ файлов.
