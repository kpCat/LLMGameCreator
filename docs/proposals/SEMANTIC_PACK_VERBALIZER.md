# Semantic Pack Verbalizer

## Статус

Proposal.

## Цель

Описать “говорилку” для semantic packs без LLM.

Важно: это не mini-LLM. Это deterministic / template-based verbalizer, который умеет объяснять данные, генерировать короткие описания и UI/debug text по semantic tags and profiles.

## Зачем

Полезные сценарии:

- объяснить, почему generator выбрал те или иные материалы;
- показать пользователю смысл visual recipe;
- создать короткие описания объектов;
- создать debug explanations;
- генерировать простые NPC chatter lines из phrase banks;
- генерировать названия;
- объяснять validation diagnostics;
- помогать human review.

## Что это не должно делать

Verbalizer не должен:

- писать сложные диалоги;
- заменять LLM;
- принимать сюжетные решения;
- менять semantic packs;
- генерировать runtime-болтовню без ограничений;
- писать большие художественные описания каждого объекта.

## Вход

```json
{
  "tags": [
    "domain/necropolis",
    "population/living_humans",
    "biome/swamp",
    "building_role/dwelling",
    "condition/damaged",
    "wealth/poor"
  ],
  "weights": {
    "domainInfluence": 0.35,
    "localHumanCulture": 0.85,
    "religiosity": 0.45,
    "decay": 0.65,
    "moss": 0.8
  },
  "recipe": {
    "shapeGrammar": "crooked_stilt_hut",
    "materials": ["rotten_wood", "dark_stone", "wet_reed_thatch"],
    "motifs": ["small_ancestor_shrine", "green_soul_lantern"]
  }
}
```

## Выход

### Debug explanation

```text
Это бедное жилище живых людей на болотной окраине Некрополя. 
Форма остаётся бытовой, потому что population/living_humans сильнее domain influence.
Некропольное влияние проявляется через малый алтарь предков и зелёный фонарь, но не через дворцовые костяные шпили.
```

### Short description

```text
Кривая болотная хижина из гнилого дерева и тёмного камня, с маленьким алтарём предков у входа.
```

### Review hint

```text
Проверь, не выглядит ли дом слишком официальным для бедного жилища. 
Запрещённые мотивы: massive_bone_spire, palace_arch, polished_bone_facade.
```

## Template packs

Verbalizer должен использовать template packs.

Пример:

```json
{
  "templateId": "explain/building/domain_population_balance",
  "conditions": [
    "objectKind/building",
    "population/living_humans",
    "domain/necropolis"
  ],
  "template": "Форма остаётся бытовой, потому что {populationLabel} сильнее влияния {domainLabel}. Влияние домена проявляется через {motifList}."
}
```

## Phrase banks

Для NPC chatter можно иметь phrase banks.

```json
{
  "phraseBankId": "chatter/necropolis_swamp_commoners",
  "conditions": [
    "domain/necropolis",
    "population/living_humans",
    "biome/swamp"
  ],
  "phrases": [
    "Предки смотрят, путник. Не шуми у воды.",
    "Фонари душ сегодня горят тускло.",
    "Жрецы снова забрали камень с кладбища.",
    "Не всякий мертвец здесь враг, но всякий закон здесь старше нас."
  ]
}
```

Важно: phrase banks должны быть curated/limited. Не генерировать бесконечную речь.

## Naming

Verbalizer может помогать с названиями:

Inputs:

- domain;
- biome;
- settlement role;
- history layer.

Output examples:

- Чёрный Камыш;
- Старые Сваи;
- Зелёный Погост;
- Тихая Заводь;
- Каменный Архив;
- Сухая Купель.

## Diagnostics text

Verbalizer должен уметь превращать diagnostics в понятные объяснения.

Input:

```json
{
  "diagnosticCode": "VISUAL_FORBIDDEN_MOTIF",
  "objectKind": "building",
  "role": "dwelling",
  "forbiddenMotif": "massive_bone_spire",
  "reason": "official/palace motif not allowed for poor dwelling"
}
```

Output:

```text
Мотив massive_bone_spire не подходит для бедного жилища: он относится к официальной или дворцовой архитектуре, а не к бытовым домам.
```

## Architecture

Основные компоненты:

- `SemanticVerbalizationRequest`
- `SemanticVerbalizationContext`
- `VerbalizationTemplate`
- `PhraseBank`
- `NamePattern`
- `VerbalizationResult`
- `SemanticPackVerbalizer`
- `VerbalizationDiagnostic`

## Determinism

Для одинакового входа и seed output должен быть стабильным.

## Safety

- no LLM calls;
- no free-form hallucination;
- no runtime provider dependency;
- no code execution;
- no changing packs;
- templates are data.

## MVP

Первый MVP:

- debug explanations for VisualRuleStack;
- short descriptions for BuildingVisualRecipe;
- diagnostic explanation text;
- 2–3 phrase banks;
- deterministic name patterns;
- tests.
