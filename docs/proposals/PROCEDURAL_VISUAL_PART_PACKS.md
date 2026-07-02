# Procedural Visual Part Packs

## Статус

Proposal.

Документ описывает идею “semantic pack для визуальных деталей”: библиотеку форм, масок, декалей, материалов, слоёв и правил, из которых LLMGameCreator может программно собирать тайлы, поверхности, фасады, одежду, окружение, постройки и части существ.

## Главная идея

Не генерировать каждый tile/texture/object вручную.

Создать:

```text
VisualPartPack
+ palettes
+ layer rules
+ surface recipes
+ object recipes
+ deterministic renderer
```

Из этого программа может собирать:

- floor tiles;
- wall textures;
- ceiling textures;
- roads;
- cliffs;
- building facades;
- roof textures;
- trim sheets;
- decals;
- tree/rock billboards;
- clothing details;
- armor trims;
- creature horns/wings/tails silhouettes;
- UI frames;
- VFX masks;
- AI control images.

## Почему это сильнее готовых тайлов

Готовый тайл — одноразовая картинка.

Visual part — переиспользуемая смысловая деталь:

```text
трещина
мох
кирпичный шов
руна
технопанель
кабель
заклёпка
кость
ветка
царапина
грязь
светящаяся линия
```

Одна деталь может быть:

- перекрашена;
- повернута;
- масштабирована;
- наложена на разные материалы;
- использована в разных сеттингах;
- использована как mask/control image для AI-refinement;
- использована в surface/facade/clothing/creature systems.

## Слои композиции

Рекомендуемый layer stack:

1. base surface;
2. large structure;
3. edges / borders / transitions;
4. medium details;
5. micro details;
6. decals;
7. damage / dirt / moss / snow;
8. highlights / shadows;
9. optional glow / magic / tech emission;
10. final normalization.

## Размер деталей

Гипотеза:

- чем меньше детали, тем выше потенциальная детализация, но выше риск шума и дороже сборка;
- чем крупнее детали, тем быстрее и стабильнее, но ниже разнообразие.

Практическое решение — иерархия:

```text
macro parts  — крупная форма
midi parts   — узнаваемые детали
micro parts  — фактура/шум/царапины
accent parts — редкие выразительные элементы
```

Не нужно вручную делать 10 000 деталей. Лучше:

```text
100–300 базовых part definitions
× palettes
× transforms
× procedural jitter
× density rules
× layer combinations
= тысячи вариантов
```

## VisualPartDefinition

```json
{
  "partId": "tilepart/crack/thin_branching_01",
  "role": "decal_crack",
  "shapeKind": "vector_polyline",
  "sizeClass": "micro",
  "compatibleSurfaces": ["stone", "dry_ground", "ice", "bone"],
  "compatibleSurfaceRoles": ["floor", "wall", "ceiling"],
  "projectionModes": ["top_down", "front_wall", "side_wall"],
  "colorSlots": {
    "main": "shadow",
    "highlight": "edge_highlight"
  },
  "layerPolicy": {
    "defaultLayer": "decal",
    "blendMode": "multiply",
    "randomRotation": true,
    "randomScaleMin": 0.7,
    "randomScaleMax": 1.4
  }
}
```

## Surface roles

Каждая деталь должна знать, где её можно использовать:

- floor;
- wall;
- ceiling;
- roof;
- cliff;
- road;
- water;
- facade;
- prop;
- billboard;
- clothing;
- armor;
- creature_part;
- ui;
- vfx.

Это важно, чтобы генератор не лепил окно на пол, траву на потолок или дверь посреди болота.

## SurfaceRecipe

```json
{
  "surfaceRecipeId": "surface/fantasy_ruins/floor_stone_v1",
  "tileSize": 64,
  "surfaceRole": "floor",
  "materialClass": "ancient_stone",
  "styleProfileId": "style/dark_fantasy_readable_v1",
  "layerStack": [
    {
      "layer": "base_surface",
      "parts": ["part/stone/base_noise_soft"],
      "density": 1.0
    },
    {
      "layer": "large_structure",
      "parts": ["part/stone/slab_grid_irregular"],
      "density": 0.8
    },
    {
      "layer": "decal",
      "parts": ["part/crack/thin_branching_01", "part/moss/patch_03"],
      "density": 0.35
    },
    {
      "layer": "emissive",
      "parts": ["part/rune/green_small_01"],
      "density": 0.05
    }
  ],
  "paletteId": "palette/fantasy_ruins_swamp_gray_green",
  "variantPolicy": {
    "variantCount": 16,
    "seedMode": "stable_per_variant"
  }
}
```

## Outputs

Renderer может выдавать:

```text
PNG atlas
metadata JSON
tile role map
atlas rects
surface tags
transition hints
fallback ids
preview image
validation report
optional control masks
```

## Использование для одежды и существ

Visual parts применимы не только к окружению.

### Clothing/armor

Детали:

- fabric patch;
- seam;
- belt;
- buckle;
- trim;
- emblem;
- armor plate;
- shoulder pad;
- cloak edge;
- stain/damage;
- magic rune;
- tech strip.

### Creature parts

Детали:

- horns;
- wings;
- tail;
- claws;
- spines;
- shell plates;
- glowing eyes;
- aura masks;
- silhouette extensions;
- body markings.

На раннем этапе это может быть не полноценная анатомия, а 2D billboard/silhouette composition.

## Adult/NSFW extension

Adult-capable visuals should extend this same part-pack system instead of bypassing it.

Adult layer examples:

- mature humanoid body silhouettes;
- sex-presentation silhouette variants;
- species-specific body surface/marking variants;
- clothing coverage/state overlays;
- torn/wet/damaged clothing overlays;
- wounds/dirt/exhaustion overlays;
- adult-only nude/reference slots;
- adult-only scene masks/control images;
- safe fallback parts for every adult-only part.

The adult layer must be rating-gated.

Suggested ratings:

- `safe`;
- `suggestive`;
- `adult_nude_reference`;
- `adult_erotic_scene`;
- `adult_private_explicit`.

Adult-only parts require explicit constraints:

```json
{
  "partId": "part/character/adult_reference/body_silhouette_mature_humanoid_01",
  "role": "adult_body_reference",
  "shapeKind": "silhouette_mask",
  "sizeClass": "macro",
  "compatibleSurfaceRoles": ["character_body"],
  "compatibleBodyPlans": ["human", "humanoid_variant", "alien_humanoid", "monster_humanoid"],
  "compatibleSexPresentationProfiles": ["female", "male", "androgynous", "mixed"],
  "contentRatings": ["adult_nude_reference"],
  "exportPolicies": ["adult_build_only", "private_local_only"],
  "requiresFlags": ["adult_project", "adult_character", "sapient", "humanoid_compatible"],
  "forbiddenTags": ["minor", "young_looking", "feral", "non_sapient", "non_consensual"],
  "safeFallbackPartId": "part/character/body/safe_clothed_silhouette_01"
}
```

Adult extension rules:

- adult visuals are optional slots, not required presentation;
- adult-only slots must never leak into safe/public builds;
- adult slots require safe fallbacks;
- adult parts are allowed only for adult, sapient, humanoid-compatible species/characters;
- nonhumanoid or feral creatures are safe-only unless a future reviewed policy explicitly says otherwise;
- ComfyUI/Civitai/provider output remains candidate media until reviewed and promoted;
- no real NSFW fixtures should be checked into the repository for early MVPs.

Read the dedicated adult extension docs:

- `docs/proposals/ADULT_VISUAL_LAYER_STRATEGY.md`
- `docs/proposals/CREATURE_VISUAL_GENOME_AND_PRESENTATION.md`
- `docs/proposals/VISUAL_PART_PACK_ADULT_EXTENSION.md`
- `docs/context/METAMODULE_CARRIER_VISUAL_NSFW_CONTEXT_BRIEF.md`

## Как это связано с AI

Procedural output может быть:

1. финальным stylized placeholder;
2. fallback asset;
3. preview proof;
4. control image для ComfyUI/InvokeAI;
5. mask/layout для image-to-image refinement;
6. seed-stable чертёж, который можно улучшать AI-адаптером.

Самая сильная схема:

```text
VisualRecipe
→ deterministic rough atlas/facade
→ AI refinement by workflow profile
→ candidate quarantine
→ human review
→ promoted asset
```

## Codex может сделать

Codex не должен “рисовать красиво”. Он должен сделать compiler:

- data models;
- JSON fixtures;
- deterministic renderer;
- palette system;
- layer composition;
- atlas writer;
- metadata writer;
- validators;
- golden/snapshot tests.

## Что считать успехом MVP

- три темы: fantasy_ruins, tech_hull, natural_forest;
- deterministic generation by seed;
- floor/wall/ceiling surface variants;
- simple facade variants;
- metadata with assetId/role/tags/atlas rect;
- validation reports;
- preview output;
- no external providers;
- no Unity dependency;
- no GamePackage public schema mutation.

## Что не делать

- не генерировать 10 000 деталей вручную;
- не делать production painterly art на первом этапе;
- не подключать ComfyUI до появления recipes/contracts;
- не делать editor UI раньше application-layer proof;
- не мешать surface parts, character identity и creature rigging в один goal;
- не делать adult/NSFW слой отдельным генератором в обход ratings, manifests, review and export policy.
