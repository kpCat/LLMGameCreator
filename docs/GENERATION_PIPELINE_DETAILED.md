# Generation pipeline

## Главный принцип

LLM не получает весь проект. LLM получает маленький `ContextPack` для конкретной задачи.

## Уровни генерации

1. Design Bible.
2. World Rules.
3. Prototypes.
4. Lua generators.
5. Dialogue/quest/event templates.
6. Asset requirements.
7. Asset jobs.
8. Validation/apply.

## GenerationSession

Сессия — это не просто чат. Это рабочая область:

```text
GenerationSession
  topic
  messages
  decisions
  linkedEntities
  generationPlan
  jobs
  drafts
  acceptedPatches
  rejectedPatches
```

Примеры сессий:

- Lore;
- Creatures;
- Biomes;
- Items;
- Abilities;
- Maps;
- NPC portraits;
- Sound effects;
- Lua generators.

## GenerationJob

```text
Pending
Running
WaitingForValidation
Failed
Completed
Cancelled
```

Job должен сохраняться на диск и продолжаться после перезапуска.

## Массовая генерация существ

```text
Discuss creatures
 -> create 3 meta-archetypes
 -> create 12 archetypes
 -> generate 100 creature prototypes by small jobs
 -> validate ids/stats/assets/abilities
 -> create asset requests
 -> generate/import assets later
```

## Генерация мира

Не генерировать 10000 сцен.

Лучше:

```text
World rules
 -> biome rules
 -> chunk generators
 -> encounter tables
 -> NPC archetypes
 -> quest templates
 -> handcrafted key locations
```

## ContextPack должен содержать

- цель job;
- relevant summaries;
- allowed schema/API;
- existing ids;
- style/design constraints;
- validation rules;
- examples;
- expected output format.

## ContextPack не должен содержать

- весь проект;
- все сцены;
- всю историю чата;
- все ассеты;
- все логи генерации;
- нерелевантные сущности.
