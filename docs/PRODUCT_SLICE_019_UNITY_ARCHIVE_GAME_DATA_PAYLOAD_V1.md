# Product Slice 019: Unity Archive Game Data Payload v1

## Goal

Add the first real game-data payload section to the materialized Unity archive.

Slice 018 created the archive contract/meta folder:

```text
.llmgc/unity-archive/
```

but that folder is still mostly manifest/contract metadata. Slice 019 should add a deterministic `data/` payload that a future Unity Player can later load.

This still does not implement Unity and does not change the GamePackage schema.

## Required output

Under the existing materialized archive folder:

```text
.llmgc/unity-archive/data/
```

write deterministic UTF-8 files:

```text
data/game-package.json
data/generated-content-index.json
data/scenes-index.json
data/npcs-index.json
data/quests-index.json
data/dialogues-index.json
data/items-index.json
data/encounters-index.json
```

If some categories are empty, write valid empty indexes instead of skipping files.

## Purpose

The future Unity Player should be able to open the archive and see:

```text
manifest
target profile
runtime modules
ui/layout metadata
asset/audio request metadata
game data payload
category indexes
```

## Non-goals

No Unity runtime/player, Unity project/export/build, asset generation, ComfyUI/Suno, Lua execution, generator execution, Runtime changes, GamePackageDefinition changes or WinForms UI.
