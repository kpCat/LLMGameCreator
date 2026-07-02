# Adaptive Generator Feedback Loop

## Статус

Proposal / practical architecture.

## Цель

Создать в LLMGameCreator безопасный механизм самоулучшения генераторов без участия LLM в массовой генерации.

Этот механизм должен улучшать не код, а данные и параметры:

- semantic packs;
- visual packs;
- recipe weights;
- probability tables;
- blacklist/whitelist combinations;
- balance presets;
- fallback policies;
- generation presets.

## Что это не такое

Это не:

- self-modifying code;
- нейросетевое обучение;
- runtime LLM;
- автоматическая перепись архитектуры;
- автопринятие ассетов;
- автоизменение GamePackage schema.

## Что это такое

Это feedback loop:

```text
1. Генератор создаёт кандидаты.
2. Валидаторы и scoring оценивают кандидаты.
3. FeedbackLedger сохраняет evidence.
4. Tuner/Analyzer строит предложения.
5. CandidatePatch описывает изменения.
6. Validation проверяет patch.
7. PromotionGate применяет patch или отклоняет.
```

## Минимальная реализация

### Stage 1 — Ledger only

Только сохранение feedback.

Модели:

- `GenerationRunRecord`
- `GenerationCandidateRecord`
- `GenerationFeedbackRecord`
- `GenerationMetricSet`
- `GenerationDecision`
- `FeedbackLedger`

Результат:

```text
Система помнит, какие генерации были хорошими/плохими, но ничего сама не меняет.
```

### Stage 2 — Scoring

Добавить deterministic scoring.

Модели:

- `GenerationQualityScore`
- `ScoreComponent`
- `ScoreProfile`
- `ScoringDiagnostic`
- `GenerationQualityReport`

Результат:

```text
Система может сравнивать candidates по понятным метрикам.
```

### Stage 3 — Candidate tuning patches

Добавить builder, который предлагает изменения, но не применяет их автоматически.

Модели:

- `PackEvolutionCandidatePatch`
- `WeightAdjustment`
- `ForbiddenCombinationPatch`
- `PreferredCombinationPatch`
- `PalettePreferencePatch`
- `DensityLimitPatch`
- `FallbackPriorityPatch`

Результат:

```text
Система говорит: “на основе feedback предлагаю повысить/понизить такие веса”.
```

### Stage 4 — PromotionGate

Добавить безопасное применение patch.

Модели:

- `PackPromotionRequest`
- `PackPromotionDecision`
- `PackVersionRecord`
- `RollbackToken`
- `PromotionValidationReport`

Результат:

```text
Изменения применяются только если прошли validation и review/promotion rules.
```

## Data flow

```text
VisualGrammarResolver
  ↓
BuildingVisualRecipe candidates
  ↓
Pseudo3D/Visual validators
  ↓
QualityScorer
  ↓
FeedbackLedger
  ↓
PackEvolutionCandidatePatchBuilder
  ↓
Candidate patch
  ↓
PromotionGate
  ↓
New pack version
```

## Пример feedback record

```json
{
  "feedbackId": "feedback/building/000184",
  "runId": "run/visual_building_batch_2026_07_02_001",
  "target": {
    "recipeId": "building/poor_dwelling",
    "domainId": "domain/necropolis",
    "biomeId": "biome/swamp"
  },
  "candidate": {
    "seed": 184,
    "shapeGrammar": "crooked_stilt_hut",
    "materials": ["rotten_wood", "dark_stone"],
    "motifs": ["small_ancestor_shrine", "green_lantern", "moss"]
  },
  "scores": {
    "loreFit": 0.91,
    "visualReadability": 0.82,
    "novelty": 0.74,
    "styleConsistency": 0.86,
    "pseudo3dFit": 0.88,
    "performanceCost": 0.2,
    "overDecorationPenalty": 0.05
  },
  "decision": "positive_signal",
  "reasons": [
    "good human dwelling under necropolis influence",
    "not overdecorated",
    "readable silhouette"
  ]
}
```

## Пример candidate patch

```json
{
  "patchId": "patch/visual_pack/necropolis_swamp_dwelling_tuning_001",
  "targetPackId": "visualpack/necropolis_swamp_v1",
  "basedOnFeedbackIds": [
    "feedback/building/000184",
    "feedback/building/000187",
    "feedback/building/000203"
  ],
  "changes": [
    {
      "kind": "increase_weight",
      "target": "shape/crooked_stilt_hut",
      "delta": 0.08,
      "reason": "high approval and readability for poor living-human dwellings"
    },
    {
      "kind": "decrease_weight",
      "target": "motif/massive_bone_spire",
      "delta": -0.12,
      "reason": "overdecorates poor dwellings and makes them look like official temples"
    },
    {
      "kind": "add_forbidden_combination",
      "tags": ["building_role/dwelling", "wealth/poor", "motif/massive_bone_spire"],
      "reason": "repeatedly rejected as lore-inappropriate"
    }
  ],
  "status": "candidate"
}
```

## Безопасность

Все изменения должны:

- быть candidate-first;
- иметь evidence;
- иметь old/new values;
- проходить validation;
- иметь rollback;
- не применяться к active packs без explicit promotion;
- не менять код;
- не менять schema;
- не вызывать LLM.

## Где применимо

### Visual world generation

- дома;
- поселения;
- surface packs;
- visual part packs;
- pseudo-3D facades;
- billboards;
- silhouettes.

### Gameplay balance

- economy;
- loot;
- combat;
- ability costs;
- crafting recipes.

### Quest/dialogue structure

- reachability;
- broken references;
- rewards;
- branching density.

### Asset pipeline

- candidate rejection reasons;
- preferred workflow profiles;
- fallback quality;
- license policy compliance.

## Anti-patterns

Запрещено:

- “если score высокий — автоматически заменить активный pack”;
- “самообучение меняет C#”;
- “самообучение переписывает лор”;
- “метрика novelty доминирует над readability”;
- “human reject игнорируется”;
- “нет rollback”.

## MVP recommendation

Первый MVP:

```text
GenerationFeedbackLedger + VisualRecipe scoring reports
```

Без tuner.

Второй MVP:

```text
PackEvolutionCandidatePatchBuilder
```

Третий:

```text
PromotionGate with explicit approve/apply.
```
