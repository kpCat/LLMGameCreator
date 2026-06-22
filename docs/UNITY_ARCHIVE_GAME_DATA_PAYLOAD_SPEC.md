# Unity Archive Game Data Payload v1 Spec

## Data folder

```text
.llmgc/unity-archive/data/
```

## Files

```text
game-package.json
generated-content-index.json
scenes-index.json
npcs-index.json
quests-index.json
dialogues-index.json
items-index.json
encounters-index.json
```

## Index shape

Each index should be small and stable:

```text
schemaVersion
category
sourcePackageId
entries[]
```

Entry fields when available:

```text
id
title/name
kind/type
sourceArtifactId
sourceContract
tags
linkedIds
```

Do not invent gameplay semantics that are not present in existing package/generated-content data. Use empty arrays when data is missing.

## Determinism

Stable file names, stable ordering by id/path, sorted tags/linkedIds, UTF-8 without BOM where possible, no timestamps, repeated unchanged materialization byte-identical.

## Safety

No path traversal, no absolute planned data paths, all files stay under `.llmgc/unity-archive/data/`.
