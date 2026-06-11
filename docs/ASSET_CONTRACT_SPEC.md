# Asset Contract Specification

Asset contract описывает, какой формат ассета ожидает игра, редактор и runtime.

Ассет — не просто путь к файлу. Ассет — data-driven сущность с `assetId`, типом, контрактом, metadata и связями.

## AssetDefinition

```json
{
  "id": "asset/npc/old_guard/spritesheet",
  "type": "character_spritesheet",
  "path": "assets/characters/old_guard.png",
  "contractId": "contract/spritesheet/4dir_4frames_32x48",
  "linkedEntityIds": ["entity-prototype/npc/old_guard"]
}
```

Сущности ссылаются на `assetId`, не на path.

## Asset types

```text
tile
tileset
character_spritesheet
npc_spritesheet
item_icon
ability_icon
portrait
portrait_expression_set
dialogue_background
scene_image
sound_effect
music_loop
ambient_loop
vfx_spritesheet
ui_icon
```

## Tileset contract

```json
{
  "id": "contract/tileset/topdown_32_grid_8x8",
  "assetType": "tileset",
  "image": { "format": ["png"], "width": 256, "height": 256 },
  "tile": { "width": 32, "height": 32 },
  "layout": { "columns": 8, "rows": 8 }
}
```

## Character spritesheet contract

```json
{
  "id": "contract/spritesheet/4dir_4frames_32x48",
  "assetType": "character_spritesheet",
  "image": { "format": ["png"], "width": 128, "height": 192 },
  "frame": { "width": 32, "height": 48 },
  "directions": ["down", "left", "right", "up"],
  "framesPerDirection": 4,
  "layout": "rows_by_direction"
}
```

## Portrait expression set

```json
{
  "id": "asset/npc/old_guard/portrait_set",
  "type": "portrait_expression_set",
  "contractId": "contract/portrait_set/basic_expressions_512",
  "variants": {
    "neutral": "assets/portraits/old_guard/neutral.png",
    "angry": "assets/portraits/old_guard/angry.png",
    "wounded": "assets/portraits/old_guard/wounded.png"
  },
  "fallbackVariant": "neutral"
}
```

Стандартные expression ids:

```text
neutral
smile
happy
sad
angry
fear
surprised
wounded
crying
dead
thinking
embarrassed
hostile
friendly
```

`neutral` обязателен. Остальные optional.

## Sound effect contract

```json
{
  "id": "contract/sfx/short_0_5_3s_wav",
  "assetType": "sound_effect",
  "audio": {
    "format": ["wav", "ogg"],
    "minDurationMs": 500,
    "maxDurationMs": 3000,
    "channels": ["mono", "stereo"]
  }
}
```

## AssetGenerationRequest

```json
{
  "id": "asset-request/npc/old_guard/portrait_set",
  "assetType": "portrait_expression_set",
  "targetEntityId": "entity-prototype/npc/old_guard",
  "styleProfileId": "style/dark_fantasy_portraits",
  "workflowProfileId": "workflow/comfy/portrait_expression_set_v1",
  "prompt": "Old tired village guard, weathered face, dark fantasy",
  "negativePrompt": "bad anatomy, blurry, extra fingers",
  "requiredVariants": ["neutral", "angry", "wounded"],
  "status": "draft"
}
```

## Workflow providers

```text
manual_import
assisted_generation
direct_comfyui
direct_fooocus
```

## Validation checklist

Проверять:

- asset id unique;
- type known;
- file exists or fallback exists;
- contract known;
- file matches contract;
- required variants exist;
- linked entities exist;
- format supported by runtime/frontend;
- for Unity export assetId is resolvable.

## Runtime rule

Отсутствующий ассет не ломает игру. Runtime/frontend использует fallback и выдаёт warning.
