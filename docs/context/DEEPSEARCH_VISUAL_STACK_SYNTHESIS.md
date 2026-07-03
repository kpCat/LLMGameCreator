# Deepsearch Visual Stack Synthesis

Status: Goal 085 implementation-oriented synthesis.

## Purpose

This document condenses the eight `docs/deepsearch/*.md` research files into the bounded implementation direction used by the Goal 085 Application-side visual part-pack rule stack. It is a routing and synthesis document only. It does not add dependencies, generate images, call media providers, mutate Runtime, mutate Unity or change the public GamePackage schema.

## Consumed Deepsearch Inputs

- `docs/deepsearch/01_PROCEDURAL_VISUAL_SYNTHESIS_CORE_AND_PART_PACKS.md`
- `docs/deepsearch/02_TILE_BIOME_WATER_WORLD_MAP_GENERATION.md`
- `docs/deepsearch/03_PSEUDO3D_FIRST_PERSON_FROM_2D_ASSETS.md`
- `docs/deepsearch/04_CREATURE_NPC_APPEARANCE_BODYPLAN_PAPERDOLL.md`
- `docs/deepsearch/05_SETTLEMENTS_CITIES_CARAVANS_LIVING_WORLD_VISUALS.md`
- `docs/deepsearch/06_UI_THEMES_EFFECTS_WEATHER_DAYNIGHT_VFX.md`
- `docs/deepsearch/07_MEDIA_PIPELINE_PROVIDER_QUARANTINE_PROVENANCE_RATING_ADULT.md`
- `docs/deepsearch/08_EXISTING_LIBRARIES_AND_TOOLS_SCOUTING.md`

## Immediate Future Optional Adapters

These are optional adapter candidates for later goals, not Goal 085 dependencies:

- DeBroglie for local constraint repair or WFC-like tile adjacency stages.
- SkiaSharp for editor-side preview/materialization adapters behind a boundary.
- RectpackSharp for deterministic atlas packing adapters.
- FastNoise Lite for seed-driven noise fields and masks.
- Clipper2 for coast, road, parcel and footprint polygon operations.
- Tiled and LDtk as external importers into neutral editor IR.
- Unity BillboardAsset, LODGroup and SpriteAtlas as presentation-side consumers of already-approved refs and staged payloads.

## Prototyping Candidates

These require isolated prototype gates before adoption:

- MarkovJunior for settlement/block/facade grammar experiments.
- mxgmn/WaveFunctionCollapse as a reference baseline, not a core dependency.
- ConvChain for constrained local variation experiments.
- Unity Sprite Shape for presentation-side splines such as roads and shorelines.
- SuperTiled2Unity and LDtkToUnity as Unity preview/import prototypes only.
- OR-Tools or smaller graph/constraint options for offline zoning, roads, caravan schedules or placement constraints.

## Rejected Or Deferred Candidates

- ImageSharp is not a default dependency because its licensing model adds avoidable product risk for core/default paths.
- Triangle.NET is rejected for now because its licensing/provenance story is ambiguous enough to avoid in core.
- Unclear-license city generators are research-only; algorithms can be studied and reimplemented under local contracts.
- GPL or complex-provenance paperdoll tools are research-only and must not be directly integrated as code or base assets.
- Heavy 3D authoring stacks remain deferred until a separate goal proves a narrow need.

## Design Requirements

- Water, coast, river, lake and marsh are first-class visual rule-stack concepts, not incidental tags.
- Logical map and visual map remain separate: logical GamePackage/world state is not replaced by visual compiler data.
- The visual compiler is editor-time/offline. It may produce metadata, review candidates, control masks or future materialized payloads, but Runtime and Unity Player do not call generators or providers.
- Runtime and Unity Player consume approved refs, deterministic safe fallbacks or staged payloads only.
- Adult/rating support remains metadata, review and export policy. It is not real adult media, not prompt authority and not a separate generator.
- Provider outputs remain quarantined candidates until deterministic validation and human review promote approved refs.
- Prompts and provider text are never source of truth.

## Goal 085 Implementation Boundary

Goal 085 implements only the BCL-only Application-side metadata contract, fixture packs, validator, negative proof and compact evidence. It intentionally stops before rasterizers, atlas packing, Unity presentation, provider quarantine storage, Runtime consumption, public GamePackage schema changes or UI.
