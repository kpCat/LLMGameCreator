# Visual World Generation — краткий контекст

## Текущая цель графики

Главная графическая цель LLMGameCreator — не text RPG и не “картинки к тексту”, а генерация богатых 2D / 2.5D / pseudo-3D игр, где:

- source of truth остаётся GamePackage;
- runtime/player не вызывает LLM;
- runtime/player не вызывает ComfyUI/Fooocus/media providers;
- Unity/player является frontend/player, а не редактором конкретной игры;
- визуальные данные подключаются через assetId, manifests, recipes, contracts, fallbacks и bindings;
- отсутствующий ассет не должен ломать runtime.

## Ключевая идея

LLMGameCreator должен генерировать визуальный мир не через готовые описания каждого объекта, а через компактные семантические признаки и визуальные правила.

LLM выдаёт не длинное описание дома, а нормализованные признаки:

```json
{
  "objectKind": "building",
  "role": "dwelling",
  "domain": "necropolis",
  "population": "living_humans",
  "biome": "swamp",
  "wealth": "poor",
  "condition": "damaged",
  "religion": "ancestor_cult",
  "materials": ["rotten_wood", "dark_stone"],
  "motifs": ["small_shrine", "green_lantern", "moss"]
}
```

Код превращает это в visual recipe:

```json
{
  "shapeGrammar": "crooked_stilt_hut",
  "materials": {
    "walls": "dark_rotten_wood",
    "foundation": "mossy_dark_stone",
    "roof": "wet_reed_thatch"
  },
  "motifs": ["small_ancestor_shrine", "green_soul_lantern"],
  "pseudo3d": {
    "mode": "facade_billboard",
    "pivot": "bottom_center",
    "fallback": "building/generic_swamp_hut"
  }
}
```

## LLM должна делать

- создавать domain/culture/religion/architecture/material/motif profiles;
- создавать compact semantic tags and weights;
- создавать blend profiles между доменами/культурами;
- создавать forbidden combinations;
- создавать редкие unique landmark profiles;
- помогать чинить противоречивые rule packs;
- помогать формировать visual grammar, но не генерировать каждый объект.

## LLM не должна делать

- вызываться для каждого дома, дерева, камня, чанка, NPC, тайла или монстра;
- генерировать live runtime prompts;
- генерировать game-specific Unity logic;
- напрямую управлять runtime;
- создавать финальные ассеты без validation/review;
- подменять deterministic generation.

## Массовую генерацию должен делать код

- выбор district layouts;
- расчёт influence weights;
- выбор object archetypes;
- генерация VisualRuleStack;
- разрешение VisualRecipe;
- выбор parts/materials/palettes;
- procedural surface/facade/billboard generation;
- deterministic seed-based variation;
- validation;
- caching;
- fallback resolution.

## VisualRuleStack

`VisualRuleStack` — стек влияний для конкретного объекта.

```json
{
  "visualRuleStack": [
    { "source": "world/dark_fantasy", "weight": 1.0 },
    { "source": "domain/necropolis", "weight": 0.65 },
    { "source": "biome/swamp", "weight": 0.8 },
    { "source": "settlement/village", "weight": 0.7 },
    { "source": "population/living_humans", "weight": 0.85 },
    { "source": "religion/ancestor_cult", "weight": 0.45 },
    { "source": "building_role/dwelling", "weight": 1.0 },
    { "source": "wealth/poor", "weight": 0.9 },
    { "source": "condition/damaged", "weight": 0.6 }
  ]
}
```

## Самая важная практическая мысль

Не делать генератор “некропольных домов”. Делать универсальный resolver:

```text
любой объект + стек влияний + seed
→ visual recipe
→ procedural output
```

Тогда система работает для любого лора, а не только для “Носителя метамодулей”.

## Важное дополнение: Codex не генерирует детали, Codex внедряет генератор

Зафиксировано стратегическое решение:

```text
Codex не должен выплёвывать тысячи visual detail JSON/PNG.
Codex должен реализовать Visual Detail Generator Core:
schema + compact part families + deterministic generators + validators + fixtures + tests.
```

Массовые варианты деталей должны генерироваться локально LLMGameCreator по seed.

Это экономит Codex-лимит, не раздувает репозиторий и лучше соответствует идее LLMGameCreator как комбайна, который сам строит визуальный мир по правилам.

## Важное дополнение: adult/NSFW слой

Некоторые генерируемые игры могут иметь взрослый визуальный слой: suggestive/adult portraits, adult-only reference assets, relationship scene cards and private adult variants.

Этот слой не должен быть отдельным NSFW-генератором. Он должен быть rating-gated extension of the same visual recipe / asset slot / manifest / review pipeline.

Правильная схема:

```text
CreatureVisualGenome
+ VisualRuleStack
+ SpeciesBodyPlan
+ SexPresentationProfile
+ ClothingState
+ EquipmentState
+ DamageState
+ AdultPresentationPolicy
+ seed
→ VisualRecipe
→ safe slots
→ optional adult-only slots
→ deterministic fallback/control output
→ optional offline AI refinement
→ candidate quarantine
→ human review
→ promoted asset binding
```

Adult visual slots require explicit policy:

- adult project flag;
- adult character flag;
- adult/sapient species;
- humanoid or humanoid-compatible body plan;
- safe fallback;
- review and promotion;
- export filtering.

Reject or quarantine:

- minor, teen-like, childlike, young-looking or age-ambiguous subjects;
- feral or non-sapient sexualized creatures;
- non-consensual framing;
- adult assets leaking into safe/public builds;
- provider output promoted without review;
- provider prompt tags as source of truth.

The intended fantasy-species direction is adult relationships and erotic presentation involving adult humans and adult sapient humanoid-compatible fantasy peoples/species, not feral/non-sapient sexualization.

Read together with:

- `docs/proposals/ADULT_VISUAL_LAYER_STRATEGY.md`
- `docs/proposals/CREATURE_VISUAL_GENOME_AND_PRESENTATION.md`
- `docs/proposals/VISUAL_PART_PACK_ADULT_EXTENSION.md`
- `docs/context/METAMODULE_CARRIER_VISUAL_NSFW_CONTEXT_BRIEF.md`
