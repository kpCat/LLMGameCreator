# Generation Quality Scoring

## Статус

Proposal.

## Цель

Описать практичную scoring-систему для оценки кандидатов, generated recipes и outputs без LLM.

Scoring нужен для:

- отбора кандидатов;
- диагностики плохих комбинаций;
- поиска повторяемости;
- оценки lore fit;
- оценки visual/readability constraints;
- подготовки tuning patches.

## Почему нельзя иметь один score

Один общий score опасен. Генератор начнёт оптимизировать его и ломать качество.

Нужен многокомпонентный score:

```text
qualityScore =
  loreFit
+ styleConsistency
+ gameplayReadability
+ visualDiversity
+ pseudo3dFit
+ assetCompleteness
- repetitionPenalty
- overDecorationPenalty
- performanceCost
- licenseRisk
```

Но хранить это лучше как отдельные компоненты, а не только total.

## Базовые компоненты score

### `loreFit`

Соответствие лору/домену/культуре.

Примеры:

- бедный дом живых людей в Некрополе не должен выглядеть как дворец нежити;
- sci-fi habitation module не должен получать соломенную крышу;
- solar temple не должен использовать forbidden necropolis motifs.

### `styleConsistency`

Соответствие style profile, palette, shape language.

### `gameplayReadability`

Читаемость для игрока:

- дверь похожа на дверь;
- опасность читается как опасность;
- NPC не сливается с фоном;
- интерактивные объекты не выглядят как декор.

### `visualDiversity`

Насколько candidate отличается от уже принятых вариантов.

Важно: novelty не должен побеждать readability.

### `repetitionPenalty`

Штраф за чрезмерное сходство с уже существующими outputs.

### `pseudo3dFit`

Есть ли:

- pivot;
- height class;
- footprint;
- surface role;
- fallback;
- collision policy;
- sort policy.

### `assetCompleteness`

Есть ли все обязательные slots.

### `performanceCost`

Оценка дороговизны:

- слишком много layers;
- слишком много unique assets;
- слишком высокая density;
- слишком большой atlas;
- слишком много variants.

### `licenseRisk`

Для media outputs:

- unknown source;
- missing provenance;
- non-commercial license;
- unclear model license;
- missing attribution.

## Пример score model

```json
{
  "scoreId": "score/building/000184",
  "candidateId": "candidate/building/000184",
  "components": {
    "loreFit": {
      "value": 0.91,
      "confidence": 0.82,
      "reasons": ["dwelling form preserved", "necropolis influence moderate"]
    },
    "styleConsistency": {
      "value": 0.86,
      "confidence": 0.74,
      "reasons": ["palette matches domain", "materials fit swamp"]
    },
    "gameplayReadability": {
      "value": 0.78,
      "confidence": 0.7,
      "reasons": ["door visible", "silhouette readable"]
    },
    "visualDiversity": {
      "value": 0.64,
      "confidence": 0.65,
      "reasons": ["not duplicate, but shares roof profile with prior candidate"]
    },
    "pseudo3dFit": {
      "value": 0.88,
      "confidence": 0.9,
      "reasons": ["has pivot, footprint, fallback"]
    }
  },
  "penalties": {
    "repetitionPenalty": 0.12,
    "overDecorationPenalty": 0.05,
    "performanceCost": 0.2,
    "licenseRisk": 0.0
  },
  "decisionHint": "positive_signal"
}
```

## Deterministic scoring

Scoring должен быть deterministic.

Для одного и того же candidate и score profile должен получаться тот же report.

## ScoreProfile

Разные задачи требуют разных весов.

Пример для poor dwelling:

```json
{
  "scoreProfileId": "scoreprofile/building_poor_dwelling",
  "weights": {
    "loreFit": 0.25,
    "styleConsistency": 0.2,
    "gameplayReadability": 0.2,
    "visualDiversity": 0.1,
    "pseudo3dFit": 0.15,
    "assetCompleteness": 0.1
  },
  "penaltyWeights": {
    "repetitionPenalty": 0.15,
    "overDecorationPenalty": 0.25,
    "performanceCost": 0.1,
    "licenseRisk": 1.0
  }
}
```

Для boss creature weights другие:

```text
visual uniqueness ↑
silhouette readability ↑
performance cost менее строгий
asset completeness важнее
```

## Hard gates vs soft scores

Некоторые вещи не score, а gate.

Hard gates:

- missing fallback;
- license forbidden;
- missing required asset slot;
- forbidden motif;
- invalid id;
- incompatible surface role;
- public schema violation;
- runtime provider dependency.

Soft scores:

- diversity;
- style strength;
- density;
- decoration amount;
- novelty.

## Human feedback

Human review должен попадать в score system как сильный signal, но не как магическая истина.

Пример:

```json
{
  "reviewSignal": {
    "decision": "reject",
    "reasons": ["too much skull decoration", "looks like temple, not dwelling"],
    "weight": 1.0
  }
}
```

Tuner должен учиться на reasons.

## Repetition detection

Для раннего этапа можно без image AI:

- сравнивать recipes;
- сравнивать tags;
- сравнивать materials;
- сравнивать shape grammar;
- сравнивать motif sets;
- сравнивать seed-derived structural parameters;
- сравнивать atlas/layout metadata.

Позже можно добавить perceptual hash/image similarity, но не в MVP.

## Визуальный шум

Для VisualPartPack нужен noise score:

```text
part density
micro detail count
contrast spikes
overlap count
decal count
emissive area ratio
```

Если noise high, tuner снижает density.

## MVP scoring

Для первого этапа достаточно:

- hard gate diagnostics;
- loreFit by forbidden/required tags;
- completeness score;
- repetition score by recipe similarity;
- pseudo3dFit;
- human review signals.

Не нужно сразу строить сложную ML-систему.
