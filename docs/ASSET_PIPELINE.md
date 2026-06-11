# Asset Pipeline

Ассеты — отдельный слой GamePackage.

## Типы ассетов

- `tile`
- `tileset`
- `character_spritesheet`
- `npc_spritesheet`
- `item_icon`
- `ability_icon`
- `portrait_expression_set`
- `dialogue_background`
- `scene_image`
- `sound_effect`
- `music_loop`
- `ambient_loop`
- `vfx_spritesheet`

## Providers

Планируемые providers:

- manual import;
- ComfyUI;
- Fooocus;
- external sound generator.

В v0.1 есть только контракты и `NullAssetGenerationProvider`.

## Workflow profiles

В будущем workflow profile должен описывать:

- provider;
- workflow file;
- asset type;
- target contract;
- куда подставлять prompt/negative/seed/output prefix.
