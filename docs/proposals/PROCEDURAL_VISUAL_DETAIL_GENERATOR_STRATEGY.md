# Procedural Visual Detail Generator Strategy

## Статус

Proposal / strategic decision.

Этот документ фиксирует важное решение для LLMGameCreator:

> Codex не должен генерировать тысячи мелких визуальных деталей как большой JSON/PNG dump.  
> Codex должен внедрить compiler/generator/validator, который локально и детерминированно производит множество вариантов деталей из компактных part families, palettes, recipes и seeds.

## Короткая формула

Плохо:

```text
Codex → 10 000 ручных VisualPartDefinition
```

Хорошо:

```text
Codex → schema + generator + validator + small curated packs
LLMGameCreator → thousands of generated variants locally by seed
```

## Почему так

Массовая генерация деталей через Codex вредна по четырём причинам:

1. **Лимит**  
   Большой JSON dump потребляет много output tokens и быстро расходует Codex usage.

2. **Качество**  
   Тысячи сгенерированных вручную записей будут шумными, дублирующимися и плохо проверяемыми.

3. **Поддержка**  
   Репозиторий начнёт пухнуть от данных, которые лучше получать из рецептов.

4. **Архитектура**  
   LLMGameCreator должен быть комбайном/генератором, а не складом заранее выплюнутых деталей.

## Основной принцип

Хранить в репозитории:

- compact schemas;
- curated visual part families;
- palette definitions;
- procedural generation rules;
- recipe examples;
- small fixture packs;
- validators;
- tests;
- tiny golden outputs.

Не хранить в репозитории:

- 10 000 generated detail records;
- 10 000 PNG variants;
- огромные generated atlases;
- provider outputs без review/promotion;
- one-off data dumps от Codex/LLM.

## Что такое Visual Detail Generator

`VisualDetailGenerator` — это deterministic generator, который создаёт варианты деталей из небольшого числа базовых семейств.

Пример:

```text
PartFamily: crack/thin_branching
Base parameters:
- branch count range
- length range
- curvature range
- split probability
- width range
- compatible materials
- layer role
- color slots

Generated variants:
- crack/thin_branching/seed_0001
- crack/thin_branching/seed_0002
- ...
```

Это лучше, чем хранить каждую трещину руками.

## Пирамида данных

Рекомендуемая структура:

```text
VisualDetailVocabulary
  ↓
VisualPartFamily
  ↓
VisualPartGeneratorRule
  ↓
VisualPartVariant
  ↓
SurfaceRecipe / ObjectRecipe
  ↓
GeneratedSurfaceAtlas / Facade / Billboard / Metadata
```

## VisualPartFamily

Семейство деталей описывает не один конкретный рисунок, а класс похожих деталей.

Пример:

```json
{
  "partFamilyId": "partfamily/crack/thin_branching",
  "kind": "decal",
  "semanticRole": "damage_crack",
  "shapeGenerator": "branching_polyline",
  "compatibleSurfaceRoles": ["floor", "wall", "ceiling"],
  "compatibleMaterials": ["stone", "ice", "bone", "dry_ground"],
  "projectionModes": ["top_down", "front_wall", "side_wall"],
  "sizeClass": "small",
  "layer": "decal",
  "colorSlots": {
    "main": "dark_shadow",
    "edge": "weak_highlight"
  },
  "parameters": {
    "branchCountMin": 1,
    "branchCountMax": 5,
    "lengthMin": 0.25,
    "lengthMax": 0.9,
    "curvatureMin": 0.0,
    "curvatureMax": 0.45,
    "widthMin": 0.01,
    "widthMax": 0.04
  },
  "randomization": {
    "allowRotate": true,
    "allowMirror": true,
    "scaleMin": 0.7,
    "scaleMax": 1.4
  }
}
```

## VisualPartVariant

Вариант — это результат применения seed/parameters к семейству.

Вариант может быть:

- materialized as vector preview;
- materialized as raster stamp;
- stored only as recipe+seed;
- cached if used frequently.

Пример:

```json
{
  "partVariantId": "partvariant/crack/thin_branching/000184",
  "partFamilyId": "partfamily/crack/thin_branching",
  "seed": 184,
  "resolvedParameters": {
    "branchCount": 3,
    "length": 0.62,
    "curvature": 0.21,
    "width": 0.018
  },
  "semanticTags": ["damage", "crack", "stone_compatible"],
  "cachePolicy": "materialize_on_demand"
}
```

## Генераторные primitive types

Первый MVP может поддерживать простые procedural primitive generators:

- `branching_polyline` — трещины, корни, молнии, вены, кабели;
- `blob_mask` — мох, грязь, пятна, лужи, коррозия;
- `panel_grid` — sci-fi панели, металлические плиты;
- `rivet_line` — заклёпки, болты, декоративные точки;
- `stripe_pattern` — warning stripes, тканевые полосы, ритуальные линии;
- `rune_glyph` — простые руны/знаки;
- `scratch_cluster` — царапины;
- `stone_slab_pattern` — каменные плиты;
- `wood_plank_pattern` — доски;
- `cable_curve` — кабели/трубы;
- `edge_trim` — бордюр/окантовка;
- `leaf_cluster` — листья/трава/органика.

Эти primitive types должны быть data-driven, seed-based и пригодными для разных сеттингов.

## Почему это подходит для разных объектов

Один и тот же generator можно использовать для разных областей.

### Окружение

- cracks;
- moss;
- dirt;
- rust;
- runes;
- roots;
- cables;
- panels;
- stones.

### Постройки

- facade seams;
- windows/doors as generated layout slots;
- roof pattern;
- trim;
- damage decals;
- material overlays.

### Одежда

- seams;
- belts;
- trims;
- buckles;
- cloth patches;
- armor plates;
- faction emblems;
- stains/damage.

### Существа

- horns silhouettes;
- wing membrane patterns;
- shell plates;
- scales;
- glowing markings;
- spikes;
- aura masks;
- body stripes;
- claws.

### UI/VFX

- frames;
- runes;
- glow masks;
- cracks;
- slash shapes;
- impact rings;
- particles layout.

## Где нужна LLM

LLM может помогать создавать:

- compact visual vocabularies;
- semantic tags;
- part family descriptions;
- compatibility rules;
- forbidden combinations;
- setting/domain-specific packs;
- palette naming;
- motif mapping.

Но LLM не должна генерировать каждый variant.

## Где нужен Codex

Codex должен писать:

- models;
- generator algorithms;
- validators;
- fixture packs;
- deterministic renderer;
- metadata exporter;
- tests.

Codex не должен писать тысячи generated detail records.

## Где работает LLMGameCreator

LLMGameCreator должен:

- принимать `VisualPartFamily`;
- генерировать `VisualPartVariant` по seed;
- использовать variants в `SurfaceRecipe`, `BuildingRecipe`, `VegetationRecipe`, `CreatureVisualRecipe`;
- materialize outputs on demand;
- кешировать результат;
- хранить provenance/generator version;
- валидировать compatibility;
- выдавать preview/atlas/metadata.

## Versioning

Важно фиксировать версию генератора.

Пример:

```json
{
  "generatorId": "visual-detail-generator/core",
  "generatorVersion": "1.0.0",
  "partFamilyId": "partfamily/crack/thin_branching",
  "seed": 184
}
```

Если алгоритм изменился, старые outputs должны быть воспроизводимы или мигрируемы.

## Cache policy

Не всё нужно хранить.

Рекомендуемые режимы:

- `recipe_only` — хранить только recipe+seed;
- `materialize_on_demand` — генерировать при необходимости;
- `cache_generated` — кешировать локально;
- `promote_to_asset` — утвердить как asset после review;
- `export_only` — генерировать только при сборке package/export.

## Validators

Нужны проверки:

- unknown part family;
- incompatible surface role;
- incompatible material;
- missing color slot;
- invalid parameter range;
- non-deterministic generation;
- missing fallback;
- excessive density/noise;
- forbidden combination;
- unsupported projection mode;
- unstable ids;
- invalid cache policy.

## MVP

Первый MVP должен быть маленьким:

- 10–20 part families;
- 3 setting packs:
  - `fantasy_ruins`;
  - `tech_hull`;
  - `natural_forest`;
- 3–5 primitive generators;
- deterministic previews;
- metadata JSON;
- tests.

Первый MVP не должен пытаться делать коммерчески красивую графику. Его цель — доказать архитектуру.

## Связь с pseudo-3D

Generated details должны иметь surface roles and projection modes:

- floor;
- wall;
- ceiling;
- facade;
- roof;
- billboard;
- prop;
- clothing;
- creature_part.

Это позволит использовать их не только для 2D tiles, но и для pseudo-3D surfaces/facades/billboards.

## Связь с AI refinement

Procedural output может стать:

- fallback art;
- placeholder art;
- preview art;
- control image;
- mask;
- layout guide for ComfyUI/InvokeAI;
- input for image-to-image refinement.

Поздняя схема:

```text
VisualPartFamily + SurfaceRecipe + Seed
→ deterministic rough texture/control image
→ optional AI refinement
→ candidate quarantine
→ human review
→ promoted asset
```

## Итоговое решение

Стратегически фиксируется:

```text
Codex writes the generator.
LLMGameCreator generates the detail variants.
LLM helps author compact semantic packs.
External AI refines selected outputs later.
```

Это направление следует считать предпочтительным для будущего visual/pseudo-3D roadmap.
