# Pack Evolution Candidate Patches

## Статус

Proposal.

## Цель

Описать безопасный механизм изменения semantic/visual/generation packs на основе feedback.

Главный принцип:

```text
Feedback never mutates active packs directly.
Feedback creates candidate patches.
Candidate patches pass validation and promotion.
```

## Что такое PackEvolutionCandidatePatch

Это data patch, который предлагает изменить pack:

- увеличить/уменьшить weight;
- добавить forbidden combination;
- добавить preferred combination;
- изменить density limit;
- изменить fallback priority;
- изменить palette preference;
- изменить generator preset;
- пометить part family as noisy;
- пометить motif как official-only/common-only;
- добавить district/object-specific override.

## Пример

```json
{
  "patchId": "patch/visualpack/necropolis_swamp_v1/0001",
  "targetPackId": "visualpack/necropolis_swamp_v1",
  "patchKind": "weight_tuning",
  "basedOn": {
    "feedbackIds": [
      "feedback/building/000184",
      "feedback/building/000187",
      "feedback/building/000203"
    ],
    "generationRunIds": [
      "run/building_batch_2026_07_02_001"
    ]
  },
  "changes": [
    {
      "changeKind": "increase_weight",
      "targetPath": "weights.shape.crooked_stilt_hut",
      "oldValue": 0.31,
      "newValue": 0.39,
      "evidence": "high score and human approve for poor swamp dwellings"
    },
    {
      "changeKind": "decrease_weight",
      "targetPath": "weights.motif.massive_bone_spire",
      "oldValue": 0.22,
      "newValue": 0.08,
      "evidence": "rejected for poor living-human dwellings"
    },
    {
      "changeKind": "add_forbidden_combination",
      "targetPath": "forbiddenCombinations",
      "newValue": ["building_role/dwelling", "wealth/poor", "motif/massive_bone_spire"],
      "evidence": "makes dwellings look like official necropolis temples"
    }
  ],
  "status": "candidate"
}
```

## Patch categories

### Weight tuning

Меняет вероятности выбора.

### Blacklist / forbidden combinations

Запрещает плохие комбинации.

### Whitelist / preferred combinations

Усиляет хорошие сочетания.

### Context override

Например:

```text
motif/green_soul_lantern allowed for necropolis common dwelling,
but motif/massive_bone_spire official-only.
```

### Density tuning

Меняет количество деталей.

### Palette tuning

Изменяет предпочтения цветов.

### Fallback tuning

Меняет fallback priority.

### Generator preset tuning

Меняет параметры procedural generator.

## Validation

Patch должен проверяться.

Diagnostics:

- target pack missing;
- target path missing;
- new weight out of range;
- weight sum invalid;
- forbidden combination duplicates existing;
- change conflicts with approved lore;
- change affects too broad scope;
- no evidence;
- no rollback path.

## Promotion

Patch states:

```text
candidate
validated
rejected
approved
applied
rolled_back
```

Нельзя сразу candidate → applied без validation.

## Rollback

Каждый applied patch должен иметь rollback data.

```json
{
  "rollback": {
    "targetPackId": "visualpack/necropolis_swamp_v1",
    "restoreValues": [
      {
        "targetPath": "weights.shape.crooked_stilt_hut",
        "value": 0.31
      }
    ]
  }
}
```

## Pack versioning

Применение patch создаёт новую версию pack.

```text
visualpack/necropolis_swamp_v1
→ visualpack/necropolis_swamp_v1.1
```

или:

```text
version: 1.0.0 → 1.0.1
```

## Human gate

Для MVP все patches должны требовать explicit promotion.

Позже можно позволить auto-apply только для low-risk local tuning, например:

- lowering density after validator failure;
- adding duplicate candidate seed to local blacklist;
- caching fallback priority.

Но не для lore/visual identity.

## Diff report

Patch должен уметь выдавать человекочитаемый report:

```text
Suggested change:
Poor necropolis dwellings were repeatedly over-decorated with massive bone spires.
Decrease motif/massive_bone_spire weight from 0.22 to 0.08 for context:
domain/necropolis + building_role/dwelling + wealth/poor + population/living_humans.

Evidence:
- 14 rejected candidates
- 2 positive candidates without this motif
- forbidden official/palace motif leakage detected
```

## MVP

Для первого этапа поддержать только:

- increase/decrease weights;
- add forbidden combination;
- add preferred combination;
- add density limit;
- validation report;
- no auto-apply.
