# Pseudo-3D Asset Presentation Contracts

## Статус

Proposal.

Документ описывает, какие данные нужны, чтобы 2D/visual recipe outputs можно было использовать в pseudo-3D мире.

## Цель

LLMGameCreator должен поддерживать игры, где 2D ассеты разворачиваются в 3D-like presentation:

- first-person grid with 2D wall/floor/ceiling textures;
- pseudo-3D billboard worlds;
- facade/card-based buildings;
- 2D actors and objects in 3D space;
- hybrid map/panel RPG presentation.

## Не “3D из ничего”

2D → 3D означает не магическое создание 3D-модели из картинки, а:

```text
2D surface/facade/billboard assets
+ scale/pivot/collision/depth metadata
+ placement rules
+ presentation contract
= pseudo-3D world representation
```

## SurfaceTextureContract

Для floor/wall/ceiling:

```json
{
  "contractId": "surface_texture/fantasy_ruins_wall_v1",
  "surfaceRole": "wall",
  "assetId": "asset/surface/fantasy_ruins/wall_stone_01",
  "tileable": true,
  "worldScaleMeters": {
    "width": 2.0,
    "height": 2.0
  },
  "projectionMode": "front_wall",
  "seamPolicy": "repeat_x_y",
  "fallbackAssetId": "asset/surface/generic/stone_wall"
}
```

## FacadeContract

Для домов и построек:

```json
{
  "contractId": "facade/building/swamp_hut_042",
  "buildingRecipeId": "building/poor_dwelling/swamp_necropolis",
  "layers": [
    { "role": "facade", "assetId": "asset/building/swamp_hut_042/facade" },
    { "role": "roof", "assetId": "asset/building/swamp_hut_042/roof" },
    { "role": "door", "assetId": "asset/building/swamp_hut_042/door" },
    { "role": "window", "assetId": "asset/building/swamp_hut_042/window_set" },
    { "role": "decor", "assetId": "asset/building/swamp_hut_042/decor" },
    { "role": "shadow", "assetId": "asset/shadow/building_small" }
  ],
  "pivot": "bottom_center",
  "heightMeters": 3.2,
  "footprint": {
    "widthCells": 2,
    "depthCells": 2
  },
  "collisionPolicy": "box_footprint",
  "sortPolicy": "pivot_based",
  "fallbackFacadeId": "facade/building/generic_poor_hut"
}
```

## BillboardContract

Для деревьев, камней, NPC, монстров, props:

```json
{
  "contractId": "billboard/tree/dead_willow_07",
  "assetId": "asset/tree/dead_willow_07/front",
  "billboardKind": "vegetation",
  "pivot": "trunk_base",
  "heightMeters": 5.5,
  "widthMeters": 3.2,
  "collisionPolicy": "trunk_circle",
  "sortPolicy": "pivot_based",
  "occlusionPolicy": "world_depth_test",
  "variants": [
    "asset/tree/dead_willow_07/front",
    "asset/tree/dead_willow_07/front_mirrored"
  ],
  "fallbackBillboardId": "billboard/tree/generic_dead_tree"
}
```

Для монстров:

```json
{
  "contractId": "billboard_actor/slime_swamp_a",
  "actorVisualFamilyId": "family/monster_billboard/slime_swamp_a",
  "states": {
    "idle": "asset/monster/slime_swamp_a/idle_front",
    "attack": "asset/monster/slime_swamp_a/attack_front",
    "hurt": "asset/monster/slime_swamp_a/hurt_front",
    "death": "asset/monster/slime_swamp_a/death_front"
  },
  "nominalWorldHeight": 1.35,
  "footPivotNormalized": { "x": 0.5, "y": 0.03 },
  "shadowAssetId": "asset/shadow/blob_small",
  "sortPolicy": "pivot_based",
  "fallbackStatePolicy": {
    "missingAttack": "idle",
    "missingHurt": "idle",
    "missingDeath": "generic_death_marker"
  }
}
```

## GridRaycastPresentationContract

Для first-person grid dungeon:

```json
{
  "presentationContractId": "presentation/grid_dungeon/swamp_crypt_01",
  "mode": "first_person_grid_2d_textures",
  "cellSizeMeters": 2.0,
  "surfaceFamilies": {
    "wall": "surface_family/swamp_crypt/walls",
    "floor": "surface_family/swamp_crypt/floors",
    "ceiling": "surface_family/swamp_crypt/ceilings",
    "door": "surface_family/swamp_crypt/doors"
  },
  "billboardFamilies": [
    "billboard_family/swamp_crypt/props",
    "billboard_family/swamp_crypt/monsters"
  ],
  "fallbacks": {
    "wall": "asset/surface/generic/stone_wall",
    "floor": "asset/surface/generic/stone_floor",
    "ceiling": "asset/surface/generic/dark_ceiling",
    "billboard": "asset/placeholder/missing_billboard"
  }
}
```

## Почему это нужно

Простой PNG недостаточен.

Для pseudo-3D каждый объект должен знать:

- размер в мире;
- точку опоры;
- pivot;
- collision footprint;
- sort policy;
- occlusion policy;
- fallback;
- state set;
- surface role;
- projection mode;
- atlas rect;
- compatible presentation modes.

Без этого Unity/player будет содержать ad-hoc logic, а не универсальный renderer.

## Что можно делать процедурно

Для buildings:

- facade front;
- side facade;
- roof card;
- shadow;
- door/window layers;
- decor overlays;
- damage overlays.

Для vegetation:

- tree billboard variants;
- bush billboards;
- grass patches;
- trunk collision metadata.

Для rocks:

- billboard or low-poly proxy metadata;
- shadow;
- collision;
- scale classes.

Для surfaces:

- wall/floor/ceiling textures;
- trim sheets;
- decals;
- normal/height/emissive hints later.

## Quality constraints

Для pseudo-3D важны:

- читаемость силуэта;
- consistent scale;
- correct pivot;
- limited overdraw;
- stable sorting;
- stable fallback;
- не сливать NPC/objects с фоном;
- не делать двери/проходы нечитаемыми;
- не генерировать wall/floor textures с несовместимой перспективой;
- не использовать object art как surface texture.

## Early implementation

MVP должен:

- создать contracts and metadata;
- использовать fixture/manual images;
- создавать placeholder previews;
- валидировать pivot/size/fallback/state completeness;
- экспортировать sidecar manifests;
- не менять публичную GamePackage schema без доказанного consumer;
- не подключать external providers.
