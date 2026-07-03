# LFZ Archive Analysis Manifest

## Purpose

This document records that `LFZ(3).zip` was inspected manually as a reference architecture for real-world / geospatial game-map ingestion.

The archive is not to be given to Codex and its code is not to be copied into LLMGameCreator. Only architectural patterns are extracted.

## Scope inspected

Important areas found in the archive:

- `LFZ/7/MapOperator/Runtime/Geography/`
- `LFZ/7/MapOperator/Runtime/Configs/Tiles/`
- `LFZ/7/MapOperator/Runtime/Providers/`
- `LFZ/7/MapOperator/Runtime/Data/`
- `LFZ/7/MapOperator/Runtime/Data/Converters/`
- `LFZ/7/MapOperator/Runtime/Generators/`
- `LFZ/10/Ifz/MapSelector/`
- `LFZ/10/Ifz/MapTiler/`
- `LFZ/10/Ifz/MapEssentials/`
- `LFZ/10/Ifz/SugestedMapContainer.cs`
- `LFZ/10/Ifz/DownloadedCitiesUtils.cs`

## Core finding

The LFZ-style system is not primarily OCR-based. It is a geodata/tile pipeline:

1. Choose a place or coordinate.
2. Convert to WebMercator / Google tile coordinates.
3. Build a tile grid and optional border.
4. Load data tiles from cache, server, file, or Overpass-like source.
5. Convert raw geodata into typed features.
6. Generate Unity/game-map content from typed data.
7. Save downloaded/imported city state for later use.

## Rule for LLMGameCreator

Do not copy LFZ implementation. Use the pattern:

`Geo source -> tile/cache/provenance -> normalized features -> WorldSourceGraph -> visual/runtime projection`.

## Future track

This belongs to the Dream Full Final / Realism Geoworld Simulator track, not to the current immediate visual-world feature track.
