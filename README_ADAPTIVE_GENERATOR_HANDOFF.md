# LLMGameCreator — Adaptive Generator / Self-Improvement Without LLM

Дата: 2026-07-02

Этот пакет фиксирует отдельное направление:

> LLMGameCreator не должен “сам себя переписывать” и не должен постоянно вызывать LLM.  
> Ему нужен безопасный adaptive generation loop: генератор создаёт варианты, валидаторы и scoring оценивают, feedback ledger сохраняет результаты, tuner предлагает candidate patches для semantic/visual packs, а promotion gate решает, применять ли изменения.

## Главная идея

Не делать:

```text
self-learning AI that rewrites code
runtime LLM calls
automatic lore rewrite
automatic schema mutation
automatic asset approval
```

Делать:

```text
generation → evaluation → feedback ledger → candidate tuning patch → validation → review/promotion → new pack version
```

## Куда положить

Распаковать архив в корень репозитория:

```text
C:\Users\endim\LLMGameCreator\
```

Файлы попадут в:

```text
docs/proposals/
docs/agent-tasks/
docs/context/
```

## Как подать в другом диалоге

```text
Я добавил docs по Adaptive Generator / Self-Improvement Without LLM:
- docs/proposals/ADAPTIVE_GENERATOR_FEEDBACK_LOOP.md
- docs/proposals/GENERATION_QUALITY_SCORING.md
- docs/proposals/PACK_EVOLUTION_CANDIDATE_PATCHES.md
- docs/proposals/SEMANTIC_PACK_VERBALIZER.md
- docs/context/ADAPTIVE_GENERATOR_CONTEXT_BRIEF.md

Прими это как стратегический контекст. Не переключай текущую разработку автоматически. Когда будешь формировать будущие goal-задачи, учитывай это как безопасный путь к самоулучшению генераторов без runtime LLM.
```

## Рекомендуемый порядок реализации

1. `GenerationFeedbackLedger`
2. `GenerationQualityScorer`
3. `PackEvolutionCandidatePatchBuilder`
4. `PromotionGate`
5. `SemanticPackVerbalizer`

Не начинать с tuner, который автоматически меняет активные packs. Сначала только ledger + reports.
