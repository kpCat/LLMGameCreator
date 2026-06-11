# Asset pipeline

Ассеты — отдельный слой GamePackage.

## Типы ассетов

- `tile`;
- `tileset`;
- `character_spritesheet`;
- `npc_spritesheet`;
- `item_icon`;
- `ability_icon`;
- `portrait`;
- `portrait_expression_set`;
- `dialogue_background`;
- `scene_image`;
- `sound_effect`;
- `music_loop`;
- `ambient_loop`;
- `vfx_spritesheet`;
- `ui_icon`.

## AssetDefinition

Сущности игры должны ссылаться на `assetId`, а не на прямой путь.

```json
{
  "id": "asset/npc/old_guard/portrait_set",
  "type": "portrait_expression_set",
  "contractId": "contract/portrait_set/basic_expressions",
  "variants": {
    "neutral": "assets/portraits/old_guard/neutral.png",
    "angry": "assets/portraits/old_guard/angry.png",
    "wounded": "assets/portraits/old_guard/wounded.png"
  },
  "fallbackVariant": "neutral"
}
```

## Asset contracts

Контракт описывает ожидаемый формат ассета.

### Character spritesheet

```json
{
  "id": "contract/spritesheet/4dir_4frames_32x48",
  "assetType": "character_spritesheet",
  "frameWidth": 32,
  "frameHeight": 48,
  "directions": ["down", "left", "right", "up"],
  "framesPerDirection": 4,
  "layout": "rows_by_direction"
}
```

### Portrait expression set

```json
{
  "id": "contract/portrait_set/basic_expressions",
  "assetType": "portrait_expression_set",
  "requiredVariants": ["neutral"],
  "optionalVariants": ["smile", "happy", "sad", "angry", "crying", "wounded", "dead"],
  "fallbackVariant": "neutral"
}
```

### Sound effect

```json
{
  "id": "contract/sfx/short_0_5_3s",
  "assetType": "sound_effect",
  "allowedFormats": ["wav", "ogg"],
  "minDurationMs": 500,
  "maxDurationMs": 3000,
  "loop": false
}
```

## Asset generation modes

1. Manual import.
2. Assisted generation: программа готовит prompt/workflow request, пользователь генерирует отдельно.
3. Direct provider integration: программа вызывает ComfyUI/Fooocus API.

## ComfyUI/Fooocus

ComfyUI/Fooocus не являются частью runtime. Они используются только в editor/generation pipeline.

## AssetGenerationJob

```text
AssetGenerationJob
  id
  provider
  workflowProfileId
  targetContractId
  linkedEntityId
  prompt
  negativePrompt
  status
  outputFiles
  validationResult
```

## Порядок генерации

```text
Design/World/Prototypes
 -> Asset requirements
 -> Asset generation jobs
 -> Generate/import outputs
 -> Validate contract
 -> Register AssetDefinition
 -> Link to entity/prototype/dialogue
```
