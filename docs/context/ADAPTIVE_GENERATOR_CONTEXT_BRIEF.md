# Adaptive Generator Context Brief

## Цель

Добавить в LLMGameCreator безопасный механизм самоулучшения генераторов без LLM.

Не “ИИ, который сам себя улучшает”, а:

```text
Adaptive Generator Feedback Loop
```

Он должен:

- генерировать кандидатов;
- оценивать результаты валидаторами, метриками, симуляциями и review;
- сохранять feedback;
- находить плохие/хорошие комбинации;
- предлагать изменения весов/правил/blacklist/whitelist;
- оформлять эти изменения как candidate patches;
- применять их только через validation/promotion gate.

## Зачем

LLM хороша для редкой фантазии и создания исходных semantic packs:

- домены;
- культуры;
- религии;
- визуальные мотивы;
- необычные существа;
- редкие локации;
- словари материалов;
- archetype packs.

Но массовая генерация должна быть без LLM:

- дома;
- деревья;
- камни;
- поверхности;
- тайлы;
- фасады;
- NPC variants;
- визуальные детали;
- баланс;
- экономика;
- квестовые структуры;
- fallback outputs.

## Архитектурная формула

```text
Generator
→ Candidate
→ Evaluator / Validators / Simulation
→ FeedbackLedger
→ QualityScorer
→ TuningProposal
→ CandidatePatch
→ Validation
→ Review / Promotion
→ New Pack Version
```

## Что можно улучшать автоматически или полуавтоматически

- веса выбора visual parts;
- вероятности motifs;
- palette preferences;
- density limits;
- forbidden combinations;
- fallback priorities;
- recipe tuning;
- generator presets;
- seed policies;
- balance coefficients;
- loot/economy weights;
- visual diversity thresholds.

## Что нельзя менять автоматически

- C# runtime logic;
- public GamePackage schema;
- Unity/player logic;
- security/sandbox rules;
- license policy;
- approved lore foundations;
- approved assets;
- important story decisions;
- validators themselves.

## Важный принцип

Любое самоулучшение должно быть audit-friendly:

- stable IDs;
- old value / new value;
- reason;
- evidence;
- score impact;
- affected pack;
- generated diagnostics;
- rollback possible.
