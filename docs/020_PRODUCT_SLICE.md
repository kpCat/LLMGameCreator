# Product Slice 020: Unity Archive Asset/Audio/Lua Request Pipeline v1

## Goal

Add a production-facing request pipeline for future Unity archive assets, audio and Lua/data modules.

The current archive already has contract/meta files and game-data payload. Slice 020 should add deterministic request queues that describe what the future asset pipeline must generate or import.

This slice must not generate assets, call ComfyUI/Suno, execute Lua, implement Unity, or change Runtime/GamePackage schema.

## Main output

Under `.llmgc/unity-archive/` write deterministic metadata files:

```text
assets/asset-requests.json
assets/asset-request-index.json
audio/audio-requests.json
audio/audio-request-index.json
lua/module-requests.json
lua/modules-index.json
```

## Request coverage

Asset request candidates:
- NPC portraits;
- item icons;
- ability/mechanic icons;
- scene illustrations/backgrounds;
- tile/terrain textures;
- UI panels/widgets/theme pieces.

Audio request candidates:
- UI click/hover/confirm/cancel;
- footsteps by surface;
- combat/action sounds;
- magic/effect sounds;
- scene ambience;
- music theme slots.

Lua/data module candidates:
- inventory;
- dialogue;
- quest journal;
- combat;
- crafting;
- stats;
- world map;
- factions;
- future transport;
- future police/crime;
- future army battle.

The service should only create request metadata. It must not execute providers or generators.
