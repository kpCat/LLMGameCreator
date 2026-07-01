# Visual World Grammar and Pseudo-3D Generation

## Статус

Proposal / strategic architecture note.

## Цель

Главная цель — не генерация отдельных картинок, тайлов или 3D-моделей. Главная цель — создать универсальную систему, которая позволяет по компактным семантическим признакам мира и объектов генерировать визуальные рецепты и pseudo-3D presentation packages.

Целевая формула:

```text
Semantic features
→ VisualRuleStack
→ VisualRecipe
→ Procedural composition
→ Surface/facade/billboard/atlas outputs
→ Pseudo-3D presentation package
→ Unity/player binding
```

## Почему это нужно

Если делать графику “в лоб”, проект быстро упрётся в проблемы:

- невозможно сгенерировать вручную каждый дом, дерево, камень, NPC, существо, тайл, стену, интерьер и сцену;
- набор из 50 готовых домиков будет быстро повторяться;
- LLM-вызовы для каждого объекта сделают генерацию медленной и дорогой;
- разрозненные prompt outputs дадут style drift;
- runtime может начать зависеть от live providers;
- Unity/player может получить hardcoded, non-data-driven visual logic;
- бесконечный или большой мир невозможно хранить как набор уникальных готовых ассетов.

## Базовая позиция

LLM должна быть не мотором генерации мира, а автором правил.

```text
LLM = authoring assistant for world laws.
LLMGameCreator = deterministic generator that applies those laws.
Unity/player = consumer of generated package data.
```

LLM используется редко:

- при создании domain/culture/religion profiles;
- при создании visual vocabularies;
- при создании blend rules;
- при создании редких landmark profiles;
- при repair/audit rule packs.

Массовая генерация должна быть обычным кодом.

## Pseudo-3D как главный target

Для LLMGameCreator целевой graphics mode — pseudo-3D / 2.5D:

- first-person grid 2D textures;
- pseudo-3D billboards;
- first-person free billboard;
- map-and-panel RPG;
- top-down/tactical вспомогательно.

Визуальная система должна уметь производить:

- floor textures;
- wall textures;
- ceiling textures;
- facade cards;
- roof cards;
- trim/decal sheets;
- object billboards;
- actor billboards;
- scene backgrounds;
- UI/card assets;
- atlas metadata;
- pseudo-3D placement hints.

## Object is not an asset

Важный принцип:

```text
Дом — это не ассет.
Дом — это экземпляр рецепта.
```

Готовый дом может быть кеширован как PNG/facade/billboard/mesh proxy, но source-of-truth должен быть:

```text
recipeId + semantic features + seed + context references
```

Пример:

```json
{
  "objectId": "building/swamp_village/house_042",
  "objectKind": "building",
  "role": "dwelling",
  "recipeId": "building/poor_dwelling",
  "domainId": "domain/necropolis",
  "regionId": "region/black_reed_swamp",
  "localTags": ["biome/swamp", "condition/damaged", "wealth/poor"],
  "seed": 420184
}
```

## VisualRecipe

`VisualRecipe` — результат применения visual rules к объекту.

```json
{
  "recipeKind": "building_visual_recipe",
  "archetype": "small_human_dwelling",
  "shapeGrammar": "crooked_stilt_hut",
  "scale": {
    "widthClass": "small",
    "heightClass": "low",
    "footprint": "2x2"
  },
  "materials": {
    "walls": "dark_rotten_wood",
    "foundation": "mossy_dark_stone",
    "roof": "wet_reed_thatch",
    "trim": "reused_bone_and_iron"
  },
  "motifs": [
    { "id": "small_ancestor_shrine", "strength": 0.7 },
    { "id": "green_soul_lantern", "strength": 0.35 },
    { "id": "skull_marker", "strength": 0.25 }
  ],
  "condition": {
    "decay": 0.65,
    "moss": 0.8,
    "damage": 0.35,
    "symmetry": 0.2
  },
  "pseudo3d": {
    "mode": "facade_billboard",
    "layers": ["facade", "roof", "door", "window", "decor", "shadow"],
    "pivot": "bottom_center",
    "fallback": "building/generic_swamp_hut"
  }
}
```

## Классы объектов

Система должна постепенно покрывать:

- buildings;
- settlement districts;
- walls/fences/gates;
- roads/paths/bridges;
- vegetation;
- rocks/cliffs/caves;
- props/containers;
- creatures;
- NPC visual proxies;
- machines;
- vehicles;
- spaceships/modules;
- interior surfaces;
- scene cards/backgrounds;
- VFX and UI elements.

## Важный приоритет

Функция объекта сильнее домена.

`building_role/dwelling` должен оставаться домом, даже если находится в землях Некрополя. Некропольное влияние накладывает материалы, мотивы, религиозные знаки, атмосферу и ограничения, но не превращает каждый дом в замок нежити.

Приоритеты для здания:

1. `ObjectFunction` — базовая форма и читаемость.
2. `SettlementTier` — масштаб и сложность.
3. `OccupantProfile` — бытовая логика.
4. `BiomeAndResources` — материалы.
5. `PoliticalControl / DomainInfluence` — официальные символы, власть.
6. `Religion / Ideology` — ритуальные мотивы.
7. `WealthClass` — качество и сложность.
8. `HistoryLayer / Condition` — повреждения, старость, следы событий.
9. `SpecialInfluence / MetaModule` — аномалии и уникальные изменения.
10. `GameplayConstraints` — читаемость, навигация, интерактивность.

## Seed-based generation

Для больших и бесконечных миров нужно хранить не каждый результат, а seed и recipe.

```json
{
  "chunkId": "chunk/region_mortion_swamp/120_-34",
  "worldSeed": 100500,
  "regionProfileId": "region/mortion_swamp_border",
  "objects": [
    {
      "kind": "building",
      "recipeId": "building/poor_dwelling",
      "semanticTags": ["condition/damaged", "history/battle_scarred"],
      "seed": 848201
    }
  ]
}
```

Если кеш удалён, output должен восстанавливаться детерминированно.

## Что делать первым

Первый технический слой должен быть не renderer и не ComfyUI adapter, а `VisualGrammarResolver`.

Вход:

```json
{
  "domainId": "domain/necropolis",
  "objectKind": "building",
  "role": "dwelling",
  "localTags": ["biome/swamp", "condition/damaged", "wealth/poor"],
  "seed": 123
}
```

Выход:

```json
{
  "recipeKind": "building_visual_recipe",
  "shapeGrammar": "crooked_low_hut",
  "materials": {
    "wall": "rotten_wood",
    "foundation": "dark_stone",
    "roof": "mossy_thatch",
    "trim": "bone"
  },
  "motifs": ["skull", "green_rune", "rusted_chain"],
  "palette": ["black_gray", "old_bone", "sickly_green"],
  "pseudo3d": {
    "presentation": "facade_billboard",
    "heightClass": "small",
    "footprintClass": "narrow",
    "fallback": "building/generic_poor_hut"
  }
}
```

## Non-goals for early implementation

На ранних этапах запрещено:

- подключать ComfyUI/Fooocus;
- генерировать production-quality art;
- менять публичную GamePackage schema без доказанного consumer;
- писать Unity-specific game logic;
- делать runtime provider calls;
- делать генератор всех объектов мира сразу;
- строить huge UI;
- генерировать 10 000 деталей вручную.

## Roadmap

### Stage 1 — Semantic visual recipe resolver

- domain profiles;
- rule stacks;
- object archetypes;
- visual recipes;
- validators;
- fixtures.

### Stage 2 — Procedural preview materializer

- simple deterministic SVG/PNG previews;
- building facade previews;
- surface previews;
- metadata;
- snapshot/golden tests.

### Stage 3 — Visual part pack compiler

- reusable visual parts;
- palettes;
- layer stacks;
- tile/surface recipes;
- deterministic atlas output.

### Stage 4 — Pseudo-3D presentation proof

- facade billboards;
- wall/floor/ceiling texture sets;
- object placement metadata;
- fallback proof.

### Stage 5 — Optional AI refinement

- ComfyUI/InvokeAI as editor-time optional adapters;
- rough procedural output as control/mask;
- candidate quarantine;
- human review;
- promotion ledger.
